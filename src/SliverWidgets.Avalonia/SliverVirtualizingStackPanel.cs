using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverVirtualizingStackPanel : VirtualizingPanel, ILogicalScrollable
{
    private readonly Dictionary<int, Control> _containersByIndex = [];
    private readonly Dictionary<Control, int> _indexesByContainer = [];
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverVirtualizingStackPanel, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<double> ItemExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingStackPanel, double>(nameof(ItemExtent), 48d);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SliverVirtualizingStackPanel, double>(nameof(Spacing));

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverVirtualizingStackPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingStackPanel, double>(nameof(CacheExtent), 250d);

    static SliverVirtualizingStackPanel()
    {
        AffectsMeasure<SliverVirtualizingStackPanel>(AxisProperty, ItemExtentProperty, SpacingProperty, ScrollOffsetProperty, CacheExtentProperty);
        AffectsArrange<SliverVirtualizingStackPanel>(AxisProperty, ItemExtentProperty, SpacingProperty, ScrollOffsetProperty, CacheExtentProperty);
    }

    public SliverVirtualizingStackPanel()
    {
        ClipToBounds = true;
    }

    public event EventHandler? ScrollInvalidated;

    public SliverAxis Axis
    {
        get => GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public double ItemExtent
    {
        get => GetValue(ItemExtentProperty);
        set => SetValue(ItemExtentProperty, value);
    }

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
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

    public Size ScrollSize
    {
        get
        {
            var step = Math.Max(1d, Math.Max(0d, ItemExtent) + Math.Max(0d, Spacing));
            return SliverAvaloniaPrimitives.ToSize(Axis, step, 16d);
        }
    }

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

        SetMainOffset(index * (Math.Max(0d, ItemExtent) + Math.Max(0d, Spacing)));
        return ContainerFromIndex(index);
    }

    protected override IInputElement? GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
    {
        var currentIndex = from is Control control ? IndexFromContainer(control) : -1;
        var targetIndex = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Items.Count - 1,
            NavigationDirection.Next or NavigationDirection.Down or NavigationDirection.Right => currentIndex + 1,
            NavigationDirection.Previous or NavigationDirection.Up or NavigationDirection.Left => currentIndex - 1,
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

        if (targetIndex < 0 || targetIndex >= Items.Count)
        {
            return null;
        }

        return ContainerFromIndex(targetIndex) ?? ScrollIntoView(targetIndex);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var itemExtent = Math.Max(0d, ItemExtent);
        var spacing = Math.Max(0d, Spacing);
        var viewportMainAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Main(axis));
        var crossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var constraints = CreateConstraints(axis, viewportMainAxisExtent, crossAxisExtent);
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(Items.Count, itemExtent, spacing));
        var result = layout.Layout(constraints);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, crossAxisExtent);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent);
        var realizedIndexes = result.Slots.Select(slot => slot.Index).ToHashSet();

        UpdateScrollInfo(extent, viewport);
        ClearUnrealizedContainers(realizedIndexes);

        foreach (var slot in result.Slots)
        {
            var container = Realize(slot.Index);
            container.Measure(SliverAvaloniaPrimitives.ToSize(axis, slot.MainAxisExtent, slot.CrossAxisExtent));
        }

        return IsScrollingEnabled(axis) ? viewport : extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var axis = Axis;
        var constraints = CreateConstraints(axis, finalSize.Main(axis), finalSize.Cross(axis));
        var layout = new SliverFixedExtentListLayout(
            new SliverFixedExtentListOptions(Items.Count, Math.Max(0d, ItemExtent), Math.Max(0d, Spacing)));
        var result = layout.Layout(constraints);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, finalSize.Cross(axis));
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, finalSize.Main(axis), finalSize.Cross(axis));

        UpdateScrollInfo(extent, viewport);

        foreach (var slot in result.Slots)
        {
            if (ContainerFromIndex(slot.Index) is { } container)
            {
                SliverAvaloniaPrimitives.ArrangeSlot(container, axis, slot, finalSize.Main(axis));
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

        var interval = Math.Max(0d, ItemExtent) + Math.Max(0d, Spacing);
        var itemStart = index * interval;
        var itemEnd = itemStart + Math.Max(0d, ItemExtent);
        return BringRangeIntoView(
            itemStart + targetRect.MainStart(Axis),
            Math.Min(itemEnd, itemStart + targetRect.MainEnd(Axis)));
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
}
