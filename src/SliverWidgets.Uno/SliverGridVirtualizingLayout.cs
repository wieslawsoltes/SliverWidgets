using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.Uno;

public class SliverGridVirtualizingLayout : VirtualizingLayout
{
    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(SliverAxis), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(SliverAxis.Vertical, OnLayoutPropertyChanged));

    public static readonly DependencyProperty SizingModeProperty =
        DependencyProperty.Register(nameof(SizingMode), typeof(SliverGridSizingMode), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(SliverGridSizingMode.FixedCrossAxisCount, OnLayoutPropertyChanged));

    public static readonly DependencyProperty CrossAxisCountProperty =
        DependencyProperty.Register(nameof(CrossAxisCount), typeof(int), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(2, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxCrossAxisExtentProperty =
        DependencyProperty.Register(nameof(MaxCrossAxisExtent), typeof(double), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(240d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MainAxisSpacingProperty =
        DependencyProperty.Register(nameof(MainAxisSpacing), typeof(double), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty CrossAxisSpacingProperty =
        DependencyProperty.Register(nameof(CrossAxisSpacing), typeof(double), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ChildAspectRatioProperty =
        DependencyProperty.Register(nameof(ChildAspectRatio), typeof(double), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(1d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MainAxisExtentProperty =
        DependencyProperty.Register(nameof(MainAxisExtent), typeof(double), typeof(SliverGridVirtualizingLayout), new PropertyMetadata(double.NaN, OnLayoutPropertyChanged));

    public SliverAxis Axis
    {
        get => (SliverAxis)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public SliverGridSizingMode SizingMode
    {
        get => (SliverGridSizingMode)GetValue(SizingModeProperty);
        set => SetValue(SizingModeProperty, value);
    }

    public int CrossAxisCount
    {
        get => (int)GetValue(CrossAxisCountProperty);
        set => SetValue(CrossAxisCountProperty, value);
    }

    public double MaxCrossAxisExtent
    {
        get => (double)GetValue(MaxCrossAxisExtentProperty);
        set => SetValue(MaxCrossAxisExtentProperty, value);
    }

    public double MainAxisSpacing
    {
        get => (double)GetValue(MainAxisSpacingProperty);
        set => SetValue(MainAxisSpacingProperty, value);
    }

    public double CrossAxisSpacing
    {
        get => (double)GetValue(CrossAxisSpacingProperty);
        set => SetValue(CrossAxisSpacingProperty, value);
    }

    public double ChildAspectRatio
    {
        get => (double)GetValue(ChildAspectRatioProperty);
        set => SetValue(ChildAspectRatioProperty, value);
    }

    public double MainAxisExtent
    {
        get => (double)GetValue(MainAxisExtentProperty);
        set => SetValue(MainAxisExtentProperty, value);
    }

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var axis = Axis;
        var viewport = GetViewport(context, availableSize, axis);
        var result = CreateLayout(context.ItemCount).Layout(CreateConstraints(axis, viewport));

        foreach (var slot in result.Slots)
        {
            context.GetOrCreateElementAt(slot.Index).Measure(ToSize(axis, slot.MainAxisExtent, slot.CrossAxisExtent));
        }

        return ToSize(axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var axis = Axis;
        var viewport = GetViewport(context, finalSize, axis);
        var result = CreateLayout(context.ItemCount).Layout(CreateConstraints(axis, viewport));

        context.LayoutOrigin = new Point(0d, 0d);

        foreach (var slot in result.Slots)
        {
            context.GetOrCreateElementAt(slot.Index).Arrange(ToContentRect(axis, slot, viewport.ScrollOffset));
        }

        return ToSize(axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
    }

    private static void OnLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is SliverGridVirtualizingLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }

    private SliverGridLayout CreateLayout(int itemCount)
    {
        var mainAxisExtent = NormalizeOptionalExtent(MainAxisExtent);
        var options = SizingMode == SliverGridSizingMode.FixedCrossAxisCount
            ? SliverGridLayoutOptions.FixedCrossAxisCount(
                itemCount,
                Math.Max(1, CrossAxisCount),
                Math.Max(0d, MainAxisSpacing),
                Math.Max(0d, CrossAxisSpacing),
                Math.Max(SliverMath.Epsilon, ChildAspectRatio),
                mainAxisExtent)
            : SliverGridLayoutOptions.WithMaxCrossAxisExtent(
                itemCount,
                Math.Max(SliverMath.Epsilon, MaxCrossAxisExtent),
                Math.Max(0d, MainAxisSpacing),
                Math.Max(0d, CrossAxisSpacing),
                Math.Max(SliverMath.Epsilon, ChildAspectRatio),
                mainAxisExtent);

        return new SliverGridLayout(options);
    }

    private static double? NormalizeOptionalExtent(double value)
    {
        return double.IsNaN(value) || double.IsInfinity(value) ? null : Math.Max(0d, value);
    }

    private static SliverViewportInfo GetViewport(VirtualizingLayoutContext context, Size availableSize, SliverAxis axis)
    {
        var realization = context.RealizationRect;
        var realizationStart = axis == SliverAxis.Vertical ? realization.Y : realization.X;
        var remainingCacheExtent = axis == SliverAxis.Vertical ? realization.Height : realization.Width;
        var availableMainAxisExtent = axis == SliverAxis.Vertical ? availableSize.Height : availableSize.Width;
        var remainingPaintExtent = ResolveVisibleMainAxisExtent(availableMainAxisExtent, remainingCacheExtent);
        var leadingCacheExtent = realizationStart <= SliverMath.Epsilon
            ? 0d
            : Math.Max(0d, (remainingCacheExtent - remainingPaintExtent) / 2d);
        var scrollOffset = realizationStart + leadingCacheExtent;
        var cacheOrigin = realizationStart - scrollOffset;
        var crossAxisExtent = axis == SliverAxis.Vertical ? availableSize.Width : availableSize.Height;

        if (double.IsInfinity(crossAxisExtent) || double.IsNaN(crossAxisExtent))
        {
            crossAxisExtent = axis == SliverAxis.Vertical ? realization.Width : realization.Height;
        }

        return new SliverViewportInfo(
            Math.Max(0d, scrollOffset),
            Math.Min(0d, cacheOrigin),
            Math.Max(0d, remainingPaintExtent),
            Math.Max(0d, remainingCacheExtent),
            Math.Max(0d, crossAxisExtent));
    }

    private static SliverConstraints CreateConstraints(SliverAxis axis, SliverViewportInfo viewport)
    {
        return new SliverConstraints(
            axis,
            viewport.ScrollOffset,
            0d,
            0d,
            viewport.RemainingPaintExtent,
            viewport.CrossAxisExtent,
            viewport.RemainingPaintExtent,
            viewport.CacheOrigin,
            viewport.RemainingCacheExtent);
    }

    private static double ResolveVisibleMainAxisExtent(double availableMainAxisExtent, double realizationMainAxisExtent)
    {
        if (double.IsFinite(availableMainAxisExtent) && availableMainAxisExtent > SliverMath.Epsilon)
        {
            return Math.Min(Math.Max(0d, availableMainAxisExtent), Math.Max(0d, realizationMainAxisExtent));
        }

        return Math.Max(0d, realizationMainAxisExtent);
    }

    private static Size ToSize(SliverAxis axis, double mainAxisExtent, double crossAxisExtent)
    {
        return axis == SliverAxis.Vertical
            ? new Size(crossAxisExtent, mainAxisExtent)
            : new Size(mainAxisExtent, crossAxisExtent);
    }

    private static Rect ToContentRect(SliverAxis axis, SliverLayoutSlot slot, double scrollOffset)
    {
        var contentMainOffset = slot.MainAxisOffset + scrollOffset;
        return axis == SliverAxis.Vertical
            ? new Rect(slot.CrossAxisOffset, contentMainOffset, slot.CrossAxisExtent, slot.MainAxisExtent)
            : new Rect(contentMainOffset, slot.CrossAxisOffset, slot.MainAxisExtent, slot.CrossAxisExtent);
    }

    private readonly record struct SliverViewportInfo(
        double ScrollOffset,
        double CacheOrigin,
        double RemainingPaintExtent,
        double RemainingCacheExtent,
        double CrossAxisExtent);
}
