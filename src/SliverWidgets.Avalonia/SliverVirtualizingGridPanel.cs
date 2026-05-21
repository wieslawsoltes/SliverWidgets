using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverVirtualizingGridPanel : VirtualizingPanel, ILogicalScrollable
{
    private readonly Dictionary<int, Control> _containersByIndex = [];
    private readonly Dictionary<Control, int> _indexesByContainer = [];
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<SliverGridSizingMode> SizingModeProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, SliverGridSizingMode>(nameof(SizingMode), SliverGridSizingMode.FixedCrossAxisCount);

    public static readonly StyledProperty<int> CrossAxisCountProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, int>(nameof(CrossAxisCount), 2);

    public static readonly StyledProperty<double> MaxCrossAxisExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(MaxCrossAxisExtent), 240d);

    public static readonly StyledProperty<double> MainAxisSpacingProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(MainAxisSpacing));

    public static readonly StyledProperty<double> CrossAxisSpacingProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(CrossAxisSpacing));

    public static readonly StyledProperty<double> ChildAspectRatioProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(ChildAspectRatio), 1d);

    public static readonly StyledProperty<double> MainAxisExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(MainAxisExtent), double.NaN);

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingGridPanel, double>(nameof(CacheExtent), 250d);

    static SliverVirtualizingGridPanel()
    {
        AffectsMeasure<SliverVirtualizingGridPanel>(
            AxisProperty,
            SizingModeProperty,
            CrossAxisCountProperty,
            MaxCrossAxisExtentProperty,
            MainAxisSpacingProperty,
            CrossAxisSpacingProperty,
            ChildAspectRatioProperty,
            MainAxisExtentProperty,
            ScrollOffsetProperty,
            CacheExtentProperty);
        AffectsArrange<SliverVirtualizingGridPanel>(
            AxisProperty,
            SizingModeProperty,
            CrossAxisCountProperty,
            MaxCrossAxisExtentProperty,
            MainAxisSpacingProperty,
            CrossAxisSpacingProperty,
            ChildAspectRatioProperty,
            MainAxisExtentProperty,
            ScrollOffsetProperty,
            CacheExtentProperty);
    }

    public SliverVirtualizingGridPanel()
    {
        ClipToBounds = true;
    }

    public event EventHandler? ScrollInvalidated;

    public SliverAxis Axis
    {
        get => GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public SliverGridSizingMode SizingMode
    {
        get => GetValue(SizingModeProperty);
        set => SetValue(SizingModeProperty, value);
    }

    public int CrossAxisCount
    {
        get => GetValue(CrossAxisCountProperty);
        set => SetValue(CrossAxisCountProperty, value);
    }

    public double MaxCrossAxisExtent
    {
        get => GetValue(MaxCrossAxisExtentProperty);
        set => SetValue(MaxCrossAxisExtentProperty, value);
    }

    public double MainAxisSpacing
    {
        get => GetValue(MainAxisSpacingProperty);
        set => SetValue(MainAxisSpacingProperty, value);
    }

    public double CrossAxisSpacing
    {
        get => GetValue(CrossAxisSpacingProperty);
        set => SetValue(CrossAxisSpacingProperty, value);
    }

    public double ChildAspectRatio
    {
        get => GetValue(ChildAspectRatioProperty);
        set => SetValue(ChildAspectRatioProperty, value);
    }

    public double MainAxisExtent
    {
        get => GetValue(MainAxisExtentProperty);
        set => SetValue(MainAxisExtentProperty, value);
    }

    public double ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    public double CacheExtent
    {
        get => GetValue(CacheExtentProperty);
        set => SetValue(CacheExtentProperty, value);
    }

    public bool CanHorizontallyScroll
    {
        get => _canHorizontallyScroll;
        set
        {
            if (_canHorizontallyScroll == value)
            {
                return;
            }

            _canHorizontallyScroll = value;
            InvalidateMeasure();
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    public bool CanVerticallyScroll
    {
        get => _canVerticallyScroll;
        set
        {
            if (_canVerticallyScroll == value)
            {
                return;
            }

            _canVerticallyScroll = value;
            InvalidateMeasure();
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    public bool IsLogicalScrollEnabled => true;

    public Size ScrollSize => SliverAvaloniaPrimitives.LineScrollSize(Axis);

    public Size PageScrollSize => _viewport;

    public Size Extent => _extent;

    public Vector Offset
    {
        get => SliverAvaloniaPrimitives.ToVector(Axis, ScrollOffset);
        set => SetMainOffset(value.Main(Axis));
    }

    public Size Viewport => _viewport;

    protected override Control? ContainerFromIndex(int index)
    {
        return _containersByIndex.GetValueOrDefault(index);
    }

    protected override int IndexFromContainer(Control container)
    {
        return _indexesByContainer.GetValueOrDefault(container, -1);
    }

    protected override IEnumerable<Control> GetRealizedContainers()
    {
        return _containersByIndex.OrderBy(pair => pair.Key).Select(pair => pair.Value);
    }

    protected override Control? ScrollIntoView(int index)
    {
        if (index < 0 || index >= Items.Count)
        {
            return null;
        }

        SetMainOffset(GetRowOffset(index, _viewport.Cross(Axis)));
        return ContainerFromIndex(index);
    }

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var currentIndex = from is Control control ? IndexFromContainer(control) : -1;
        var metrics = ResolveMetrics(_viewport.Cross(Axis));
        var targetIndex = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Items.Count - 1,
            NavigationDirection.Next => currentIndex + 1,
            NavigationDirection.Previous => currentIndex - 1,
            NavigationDirection.Down => currentIndex + metrics.CrossAxisCount,
            NavigationDirection.Up => currentIndex - metrics.CrossAxisCount,
            NavigationDirection.Right => Axis == SliverAxis.Vertical ? currentIndex + 1 : currentIndex + metrics.CrossAxisCount,
            NavigationDirection.Left => Axis == SliverAxis.Vertical ? currentIndex - 1 : currentIndex - metrics.CrossAxisCount,
            _ => currentIndex
        };

        if (wrap && Items.Count > 0)
        {
            if (targetIndex < 0)
            {
                targetIndex = Items.Count - 1;
            }
            else if (targetIndex >= Items.Count)
            {
                targetIndex = 0;
            }
        }

        return targetIndex >= 0 && targetIndex < Items.Count
            ? ContainerFromIndex(targetIndex) ?? ScrollIntoView(targetIndex)
            : null;
    }

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(items, e);
        ClearRealizedContainers();
        InvalidateMeasure();
        RaiseScrollInvalidated(EventArgs.Empty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var viewportMainAxisExtent = SliverAvaloniaPrimitives.ResolveViewportMainAxisExtent(
            availableSize.Main(axis),
            _viewport.Main(axis));
        var crossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var constraints = CreateConstraints(axis, viewportMainAxisExtent, crossAxisExtent);
        var result = CreateLayout(Items.Count).Layout(constraints);
        var realizedIndexes = result.Slots.Select(slot => slot.Index).ToHashSet();

        UpdateScrollInfo(
            SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, crossAxisExtent),
            SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent));
        ClearUnrealizedContainers(realizedIndexes);

        foreach (var slot in result.Slots)
        {
            var container = Realize(slot.Index);
            container.Measure(SliverAvaloniaPrimitives.ToSize(axis, slot.MainAxisExtent, slot.CrossAxisExtent));
        }

        return IsScrollingEnabled(axis) ? _viewport : _extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var axis = Axis;
        var constraints = CreateConstraints(axis, finalSize.Main(axis), finalSize.Cross(axis));
        var result = CreateLayout(Items.Count).Layout(constraints);
        var arranged = new HashSet<int>();

        UpdateScrollInfo(
            SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, finalSize.Cross(axis)),
            SliverAvaloniaPrimitives.ToSize(axis, finalSize.Main(axis), finalSize.Cross(axis)));

        foreach (var slot in result.Slots)
        {
            if (ContainerFromIndex(slot.Index) is { } container)
            {
                SliverAvaloniaPrimitives.ArrangeSlot(container, axis, slot, finalSize.Main(axis));
                arranged.Add(slot.Index);
            }
        }

        foreach (var pair in _containersByIndex)
        {
            if (!arranged.Contains(pair.Key))
            {
                SliverAvaloniaPrimitives.HideArrangedChild(pair.Value);
            }
        }

        return finalSize;
    }

    public bool BringIntoView(Control target, Rect targetRect)
    {
        var index = IndexFromContainer(target);
        if (index < 0)
        {
            return false;
        }

        var rowStart = GetRowOffset(index, _viewport.Cross(Axis));
        var rowEnd = rowStart + ResolveMetrics(_viewport.Cross(Axis)).TileMainAxisExtent;
        return BringRangeIntoView(rowStart, rowEnd);
    }

    public Control? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        return GetControl(direction, from, false) as Control;
    }

    public void RaiseScrollInvalidated(EventArgs e)
    {
        ScrollInvalidated?.Invoke(this, e);
    }

    private SliverConstraints CreateConstraints(SliverAxis axis, double viewportMainAxisExtent, double crossAxisExtent)
    {
        var cacheExtent = Math.Max(0d, CacheExtent);
        return new SliverConstraints(
            axis,
            Math.Max(0d, ScrollOffset),
            0d,
            0d,
            viewportMainAxisExtent,
            crossAxisExtent,
            viewportMainAxisExtent,
            -cacheExtent,
            viewportMainAxisExtent + (cacheExtent * 2d));
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

    private GridMetrics ResolveMetrics(double crossAxisExtent)
    {
        var layout = CreateLayout(Items.Count);
        var crossAxisCount = layout.ResolveCrossAxisCount(Math.Max(0d, crossAxisExtent));
        var crossAxisSpacing = Math.Max(0d, CrossAxisSpacing);
        var tileCrossAxisExtent = Math.Max(0d, (Math.Max(0d, crossAxisExtent) - ((crossAxisCount - 1) * crossAxisSpacing)) / crossAxisCount);
        var tileMainAxisExtent = NormalizeOptionalExtent(MainAxisExtent) ?? tileCrossAxisExtent / Math.Max(SliverMath.Epsilon, ChildAspectRatio);
        return new GridMetrics(crossAxisCount, tileMainAxisExtent);
    }

    private double GetRowOffset(int index, double crossAxisExtent)
    {
        var metrics = ResolveMetrics(crossAxisExtent);
        var row = index / metrics.CrossAxisCount;
        return row * (metrics.TileMainAxisExtent + Math.Max(0d, MainAxisSpacing));
    }

    private Control Realize(int index)
    {
        var existing = ContainerFromIndex(index);
        if (existing is not null)
        {
            return existing;
        }

        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");
        var item = Items[index];
        var needsContainer = generator.NeedsContainer(item, index, out var recycleKey);
        var container = needsContainer
            ? generator.CreateContainer(item, index, recycleKey)
            : item as Control;

        if (container is null)
        {
            throw new InvalidOperationException($"Item at index {index} did not produce a control container.");
        }

        generator.PrepareItemContainer(container, item, index);
        AddInternalChild(container);
        generator.ItemContainerPrepared(container, item, index);
        _containersByIndex[index] = container;
        _indexesByContainer[container] = index;

        return container;
    }

    private void ClearUnrealizedContainers(ISet<int> realizedIndexes)
    {
        var generator = ItemContainerGenerator ?? throw new InvalidOperationException("The panel is not attached to an ItemsControl.");

        foreach (var container in GetRealizedContainers().ToArray())
        {
            var index = IndexFromContainer(container);
            if (index >= 0 && !realizedIndexes.Contains(index))
            {
                _containersByIndex.Remove(index);
                _indexesByContainer.Remove(container);
                generator.ClearItemContainer(container);
                RemoveInternalChild(container);
            }
        }
    }

    private void ClearRealizedContainers()
    {
        if (ItemContainerGenerator is not { } generator)
        {
            _containersByIndex.Clear();
            _indexesByContainer.Clear();
            return;
        }

        foreach (var container in GetRealizedContainers().ToArray())
        {
            generator.ClearItemContainer(container);
            RemoveInternalChild(container);
        }

        _containersByIndex.Clear();
        _indexesByContainer.Clear();
    }

    private bool BringRangeIntoView(double start, double end)
    {
        var viewportMain = _viewport.Main(Axis);
        var current = ScrollOffset;
        var target = current;

        if (start < current)
        {
            target = start;
        }
        else if (end > current + viewportMain)
        {
            target = end - viewportMain;
        }

        return SetMainOffset(target);
    }

    private bool SetMainOffset(double value)
    {
        var axis = Axis;
        var clamped = SliverAvaloniaPrimitives.ClampScrollOffset(value, _extent.Main(axis), _viewport.Main(axis));
        if (SliverAvaloniaPrimitives.AreClose(clamped, ScrollOffset))
        {
            return false;
        }

        SetCurrentValue(ScrollOffsetProperty, clamped);
        InvalidateMeasure();
        RaiseScrollInvalidated(EventArgs.Empty);
        return true;
    }

    private void UpdateScrollInfo(Size extent, Size viewport)
    {
        var changed = !SliverAvaloniaPrimitives.AreClose(_extent, extent) ||
                      !SliverAvaloniaPrimitives.AreClose(_viewport, viewport);
        _extent = extent;
        _viewport = viewport;

        var clamped = SliverAvaloniaPrimitives.ClampScrollOffset(ScrollOffset, extent.Main(Axis), viewport.Main(Axis));
        if (!SliverAvaloniaPrimitives.AreClose(clamped, ScrollOffset))
        {
            SetCurrentValue(ScrollOffsetProperty, clamped);
            changed = true;
        }

        if (changed)
        {
            RaiseScrollInvalidated(EventArgs.Empty);
        }
    }

    private bool IsScrollingEnabled(SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? CanVerticallyScroll : CanHorizontallyScroll;
    }

    private readonly record struct GridMetrics(int CrossAxisCount, double TileMainAxisExtent);
}
