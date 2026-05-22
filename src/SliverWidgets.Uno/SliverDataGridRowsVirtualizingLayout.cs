using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.Uno;

public class SliverDataGridRowsVirtualizingLayout : VirtualizingLayout
{
    private double[]? _rowOffsets;
    private int _cachedItemCount = -1;
    private int _cachedItemsVersion;
    private double _scrollExtent;
    private Func<object?, int, double>? _rowExtentSelector;

    public static readonly DependencyProperty DefaultRowExtentProperty =
        DependencyProperty.Register(nameof(DefaultRowExtent), typeof(double), typeof(SliverDataGridRowsVirtualizingLayout), new PropertyMetadata(64d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinRowExtentProperty =
        DependencyProperty.Register(nameof(MinRowExtent), typeof(double), typeof(SliverDataGridRowsVirtualizingLayout), new PropertyMetadata(36d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxRowExtentProperty =
        DependencyProperty.Register(nameof(MaxRowExtent), typeof(double), typeof(SliverDataGridRowsVirtualizingLayout), new PropertyMetadata(96d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(nameof(RowSpacing), typeof(double), typeof(SliverDataGridRowsVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty TableWidthProperty =
        DependencyProperty.Register(nameof(TableWidth), typeof(double), typeof(SliverDataGridRowsVirtualizingLayout), new PropertyMetadata(800d, OnLayoutPropertyChanged));

    public double DefaultRowExtent
    {
        get => (double)GetValue(DefaultRowExtentProperty);
        set => SetValue(DefaultRowExtentProperty, value);
    }

    public double MinRowExtent
    {
        get => (double)GetValue(MinRowExtentProperty);
        set => SetValue(MinRowExtentProperty, value);
    }

    public double MaxRowExtent
    {
        get => (double)GetValue(MaxRowExtentProperty);
        set => SetValue(MaxRowExtentProperty, value);
    }

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public double TableWidth
    {
        get => (double)GetValue(TableWidthProperty);
        set => SetValue(TableWidthProperty, value);
    }

    public Func<object?, int, double>? RowExtentSelector
    {
        get => _rowExtentSelector;
        set
        {
            if (!ReferenceEquals(_rowExtentSelector, value))
            {
                _rowExtentSelector = value;
                InvalidateItems();
            }
        }
    }

    public void InvalidateItems()
    {
        _rowOffsets = null;
        _cachedItemCount = -1;
        _cachedItemsVersion = 0;
        _scrollExtent = 0d;
        InvalidateMeasure();
    }

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var viewport = GetViewport(context, availableSize);
        EnsureMetrics(context);
        var tableWidth = ResolveExtent(TableWidth, availableSize.Width, viewport.CrossAxisExtent);

        foreach (var slot in EnumerateRealizedRows(viewport))
        {
            context.GetOrCreateElementAt(slot.Index).Measure(new Size(tableWidth, slot.Extent));
        }

        return new Size(tableWidth, _scrollExtent);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var viewport = GetViewport(context, finalSize);
        EnsureMetrics(context);
        var tableWidth = ResolveExtent(TableWidth, finalSize.Width, viewport.CrossAxisExtent);
        context.LayoutOrigin = new Point(0d, 0d);

        foreach (var slot in EnumerateRealizedRows(viewport))
        {
            context.GetOrCreateElementAt(slot.Index).Arrange(new Rect(
                0d,
                slot.Offset,
                tableWidth,
                slot.Extent));
        }

        return new Size(tableWidth, _scrollExtent);
    }

    private void EnsureMetrics(VirtualizingLayoutContext context)
    {
        var itemCount = context.ItemCount;
        var itemsVersion = ComputeItemsVersion(context);

        if (_rowOffsets is not null &&
            itemCount == _cachedItemCount &&
            itemsVersion == _cachedItemsVersion)
        {
            return;
        }

        var offsets = new double[itemCount + 1];
        var cursor = 0d;
        var rowSpacing = ResolveNonNegative(RowSpacing);

        for (var index = 0; index < itemCount; index++)
        {
            offsets[index] = cursor;
            cursor += GetRowExtent(context, index);
            if (index < itemCount - 1)
            {
                cursor += rowSpacing;
            }
        }

        offsets[itemCount] = cursor;
        _rowOffsets = offsets;
        _scrollExtent = cursor;
        _cachedItemCount = itemCount;
        _cachedItemsVersion = itemsVersion;
    }

    private IEnumerable<DataGridRowSlot> EnumerateRealizedRows(DataGridViewportInfo viewport)
    {
        if (_rowOffsets is null || _cachedItemCount == 0)
        {
            yield break;
        }

        var cacheStart = Math.Max(0d, viewport.ScrollOffset + viewport.CacheOrigin);
        var cacheEnd = Math.Max(cacheStart, viewport.ScrollOffset + viewport.RemainingCacheExtent);
        var startIndex = FindFirstIndex(cacheStart);

        for (var index = startIndex; index < _cachedItemCount; index++)
        {
            var rowStart = _rowOffsets[index];
            var rowExtent = Math.Max(0d, _rowOffsets[index + 1] - rowStart - (index < _cachedItemCount - 1 ? ResolveNonNegative(RowSpacing) : 0d));
            var rowEnd = rowStart + rowExtent;

            if (rowStart - cacheEnd >= -SliverMath.Epsilon)
            {
                yield break;
            }

            if (RangesOverlap(rowStart, rowEnd, cacheStart, cacheEnd))
            {
                yield return new DataGridRowSlot(index, rowStart, rowExtent);
            }
        }
    }

    private int FindFirstIndex(double cacheStart)
    {
        var offsets = _rowOffsets!;
        var low = 0;
        var high = _cachedItemCount;

        while (low < high)
        {
            var middle = low + ((high - low) / 2);
            var rowEnd = offsets[middle + 1];
            if (rowEnd - cacheStart > SliverMath.Epsilon)
            {
                high = middle;
            }
            else
            {
                low = middle + 1;
            }
        }

        return Math.Max(0, low);
    }

    private double GetRowExtent(VirtualizingLayoutContext context, int index)
    {
        var defaultExtent = ResolveNonNegative(DefaultRowExtent);
        var extent = _rowExtentSelector?.Invoke(context.GetItemAt(index), index) ?? defaultExtent;
        if (!double.IsFinite(extent))
        {
            extent = defaultExtent;
        }

        var min = ResolveNonNegative(MinRowExtent);
        var max = Math.Max(min, ResolveNonNegative(MaxRowExtent, min));
        return SliverMath.Clamp(extent, min, max);
    }

    private int ComputeItemsVersion(VirtualizingLayoutContext context)
    {
        var itemCount = context.ItemCount;
        var hash = new HashCode();
        hash.Add(itemCount);
        hash.Add(DefaultRowExtent);
        hash.Add(MinRowExtent);
        hash.Add(MaxRowExtent);
        hash.Add(RowSpacing);

        if (itemCount > 0)
        {
            AddItemVersion(context, ref hash, 0);
            AddItemVersion(context, ref hash, itemCount / 2);
            AddItemVersion(context, ref hash, itemCount - 1);
        }

        return hash.ToHashCode();
    }

    private static void AddItemVersion(VirtualizingLayoutContext context, ref HashCode hash, int index)
    {
        var item = context.GetItemAt(index);
        hash.Add(item?.GetHashCode() ?? index);
    }

    private static DataGridViewportInfo GetViewport(VirtualizingLayoutContext context, Size availableSize)
    {
        var realization = context.RealizationRect;
        var remainingCacheExtent = realization.Height;
        var remainingPaintExtent = ResolveVisibleMainAxisExtent(availableSize.Height, remainingCacheExtent);
        var leadingCacheExtent = realization.Y <= SliverMath.Epsilon
            ? 0d
            : Math.Max(0d, (remainingCacheExtent - remainingPaintExtent) / 2d);
        var scrollOffset = realization.Y + leadingCacheExtent;
        var cacheOrigin = realization.Y - scrollOffset;
        var crossAxisExtent = double.IsFinite(availableSize.Width) ? availableSize.Width : realization.Width;

        return new DataGridViewportInfo(
            Math.Max(0d, scrollOffset),
            Math.Min(0d, cacheOrigin),
            Math.Max(0d, remainingPaintExtent),
            Math.Max(0d, remainingCacheExtent),
            Math.Max(0d, crossAxisExtent));
    }

    private static double ResolveVisibleMainAxisExtent(double availableMainAxisExtent, double realizationMainAxisExtent)
    {
        if (double.IsFinite(availableMainAxisExtent) && availableMainAxisExtent > SliverMath.Epsilon)
        {
            return Math.Min(Math.Max(0d, availableMainAxisExtent), Math.Max(0d, realizationMainAxisExtent));
        }

        return Math.Max(0d, realizationMainAxisExtent);
    }

    private static double ResolveExtent(double preferred, double available, double fallback)
    {
        if (double.IsFinite(preferred) && preferred > SliverMath.Epsilon)
        {
            return preferred;
        }

        if (double.IsFinite(available) && available > SliverMath.Epsilon)
        {
            return available;
        }

        return Math.Max(0d, fallback);
    }

    private static double ResolveNonNegative(double value, double fallback = 0d)
    {
        return double.IsFinite(value) && value >= 0d ? value : Math.Max(0d, fallback);
    }

    private static void OnLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is SliverDataGridRowsVirtualizingLayout layout)
        {
            layout.InvalidateItems();
        }
    }

    private static bool RangesOverlap(double start, double end, double otherStart, double otherEnd)
    {
        return end - otherStart > SliverMath.Epsilon && otherEnd - start > SliverMath.Epsilon;
    }

    private readonly record struct DataGridViewportInfo(
        double ScrollOffset,
        double CacheOrigin,
        double RemainingPaintExtent,
        double RemainingCacheExtent,
        double CrossAxisExtent);

    private readonly record struct DataGridRowSlot(int Index, double Offset, double Extent);
}
