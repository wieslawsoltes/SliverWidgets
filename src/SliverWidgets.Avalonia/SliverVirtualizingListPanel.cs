using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverVirtualizingListPanel : VirtualizingPanel, ILogicalScrollable
{
    private readonly Dictionary<int, Control> _containersByIndex = [];
    private readonly Dictionary<Control, int> _indexesByContainer = [];
    private readonly Dictionary<int, double> _extentCache = [];
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverVirtualizingListPanel, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<double> EstimatedItemExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingListPanel, double>(nameof(EstimatedItemExtent), 48d);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SliverVirtualizingListPanel, double>(nameof(Spacing));

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverVirtualizingListPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverVirtualizingListPanel, double>(nameof(CacheExtent), 250d);

    static SliverVirtualizingListPanel()
    {
        AffectsMeasure<SliverVirtualizingListPanel>(AxisProperty, EstimatedItemExtentProperty, SpacingProperty, ScrollOffsetProperty, CacheExtentProperty);
        AffectsArrange<SliverVirtualizingListPanel>(AxisProperty, EstimatedItemExtentProperty, SpacingProperty, ScrollOffsetProperty, CacheExtentProperty);
    }

    public SliverVirtualizingListPanel()
    {
        ClipToBounds = true;
    }

    public event EventHandler? ScrollInvalidated;

    public SliverAxis Axis
    {
        get => GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public double EstimatedItemExtent
    {
        get => GetValue(EstimatedItemExtentProperty);
        set => SetValue(EstimatedItemExtentProperty, value);
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
            var step = Math.Max(1d, Math.Max(0d, EstimatedItemExtent) + Math.Max(0d, Spacing));
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

        SetMainOffset(GetOffset(index));
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

        return targetIndex >= 0 && targetIndex < Items.Count
            ? ContainerFromIndex(targetIndex) ?? ScrollIntoView(targetIndex)
            : null;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var viewportMainAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Main(axis));
        var crossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var realizedIndexes = GetRealizedIndexes(viewportMainAxisExtent).ToArray();
        var realizedSet = realizedIndexes.ToHashSet();

        ClearUnrealizedContainers(realizedSet);

        foreach (var index in realizedIndexes)
        {
            var container = Realize(index);
            container.Measure(SliverAvaloniaPrimitives.ToSize(axis, double.PositiveInfinity, crossAxisExtent));
            _extentCache[index] = Math.Max(0d, container.DesiredSize.Main(axis));
        }

        var extent = SliverAvaloniaPrimitives.ToSize(axis, GetTotalExtent(), crossAxisExtent);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent);
        UpdateScrollInfo(extent, viewport);
        return IsScrollingEnabled(axis) ? viewport : extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var axis = Axis;
        var viewportMainAxisExtent = finalSize.Main(axis);
        var realizedIndexes = GetRealizedIndexes(viewportMainAxisExtent).ToArray();
        var arranged = new HashSet<int>();
        UpdateScrollInfo(
            SliverAvaloniaPrimitives.ToSize(axis, GetTotalExtent(), finalSize.Cross(axis)),
            SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, finalSize.Cross(axis)));

        foreach (var index in realizedIndexes)
        {
            var container = ContainerFromIndex(index);
            if (container is null)
            {
                continue;
            }

            var slot = new SliverLayoutSlot(
                index,
                GetOffset(index) - Math.Max(0d, ScrollOffset),
                0d,
                GetExtent(index),
                finalSize.Cross(axis),
                IsCacheOnly: !SliverAvaloniaPrimitives.IntersectsViewport(
                    GetOffset(index) - Math.Max(0d, ScrollOffset),
                    GetExtent(index),
                    viewportMainAxisExtent));
            SliverAvaloniaPrimitives.ArrangeSlot(container, axis, slot, viewportMainAxisExtent);
            arranged.Add(index);
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

        var itemStart = GetOffset(index);
        var itemEnd = itemStart + GetExtent(index);
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

    private IEnumerable<int> GetRealizedIndexes(double viewportMainAxisExtent)
    {
        var itemCount = Items.Count;
        if (itemCount <= 0)
        {
            yield break;
        }

        var cacheExtent = Math.Max(0d, CacheExtent);
        var cacheStart = Math.Max(0d, Math.Max(0d, ScrollOffset) - cacheExtent);
        var cacheEnd = Math.Max(cacheStart, Math.Max(0d, ScrollOffset) + viewportMainAxisExtent + cacheExtent);
        var offset = 0d;
        var spacing = Math.Max(0d, Spacing);

        for (var index = 0; index < itemCount; index++)
        {
            var extent = GetExtent(index);
            var end = offset + extent;

            if (end >= cacheStart && offset <= cacheEnd)
            {
                yield return index;
            }

            if (offset - cacheEnd > SliverMath.Epsilon)
            {
                yield break;
            }

            offset = end + spacing;
        }
    }

    private double GetOffset(int index)
    {
        var offset = 0d;
        var spacing = Math.Max(0d, Spacing);

        for (var i = 0; i < index; i++)
        {
            offset += GetExtent(i) + spacing;
        }

        return offset;
    }

    private double GetTotalExtent()
    {
        var itemCount = Items.Count;
        if (itemCount == 0)
        {
            return 0d;
        }

        var total = 0d;
        var spacing = Math.Max(0d, Spacing);

        for (var i = 0; i < itemCount; i++)
        {
            total += GetExtent(i);
        }

        return total + ((itemCount - 1) * spacing);
    }

    private double GetExtent(int index)
    {
        return _extentCache.TryGetValue(index, out var extent)
            ? extent
            : Math.Max(1d, EstimatedItemExtent);
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
        var extent = _extent.Main(axis) > SliverMath.Epsilon ? _extent.Main(axis) : GetTotalExtent();
        var clamped = SliverAvaloniaPrimitives.ClampScrollOffset(value, extent, _viewport.Main(axis));
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
