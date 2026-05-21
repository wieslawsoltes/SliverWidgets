using System.Collections.Specialized;
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
    private readonly List<double> _offsetCache = [];
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private bool _offsetCacheDirty = true;
    private double _offsetCacheEstimatedItemExtent = double.NaN;
    private double _offsetCacheSpacing = double.NaN;
    private int _offsetCacheItemCount = -1;
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

    protected override void OnItemsChanged(IReadOnlyList<object?> items, NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(items, e);
        ClearRealizedContainers();
        _extentCache.Clear();
        InvalidateOffsetCache();
        InvalidateMeasure();
        RaiseScrollInvalidated(EventArgs.Empty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var viewportMainAxisExtent = SliverAvaloniaPrimitives.ResolveViewportMainAxisExtent(
            availableSize.Main(axis),
            _viewport.Main(axis));
        var measureCrossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var realizedIndexes = GetRealizedIndexes(viewportMainAxisExtent).ToArray();
        var realizedSet = realizedIndexes.ToHashSet();
        var measuredCrossAxisExtent = 0d;

        ClearUnrealizedContainers(realizedSet);

        foreach (var index in realizedIndexes)
        {
            var container = Realize(index);
            container.Measure(SliverAvaloniaPrimitives.ToSize(axis, double.PositiveInfinity, measureCrossAxisExtent));
            var measuredExtent = Math.Max(0d, container.DesiredSize.Main(axis));
            measuredCrossAxisExtent = Math.Max(measuredCrossAxisExtent, container.DesiredSize.Cross(axis));
            if (!_extentCache.TryGetValue(index, out var previousExtent) ||
                !SliverAvaloniaPrimitives.AreClose(previousExtent, measuredExtent))
            {
                _extentCache[index] = measuredExtent;
                InvalidateOffsetCache();
            }
        }

        var desiredCrossAxisExtent = SliverAvaloniaPrimitives.ResolveDesiredCrossAxisExtent(
            availableSize.Cross(axis),
            measuredCrossAxisExtent,
            _viewport.Cross(axis));
        var extent = SliverAvaloniaPrimitives.ToSize(axis, GetTotalExtent(), desiredCrossAxisExtent);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, desiredCrossAxisExtent);
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

            var itemStart = GetOffset(index);
            var itemExtent = GetExtent(index);
            var slotOffset = itemStart - Math.Max(0d, ScrollOffset);
            var slot = new SliverLayoutSlot(
                index,
                slotOffset,
                0d,
                itemExtent,
                finalSize.Cross(axis),
                IsCacheOnly: !SliverAvaloniaPrimitives.IntersectsViewport(
                    slotOffset,
                    itemExtent,
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
        EnsureOffsetCache();
        var startIndex = FindFirstRangeOverlapIndex(cacheStart);

        for (var index = startIndex; index < itemCount; index++)
        {
            var offset = _offsetCache[index];
            var extent = GetExtent(index);
            var end = offset + extent;

            if (offset - cacheEnd > SliverMath.Epsilon)
            {
                yield break;
            }

            if (end - cacheStart > SliverMath.Epsilon && cacheEnd - offset > SliverMath.Epsilon)
            {
                yield return index;
            }
        }
    }

    private double GetOffset(int index)
    {
        EnsureOffsetCache();
        return index <= 0
            ? 0d
            : _offsetCache[Math.Min(index, _offsetCache.Count - 1)];
    }

    private double GetTotalExtent()
    {
        EnsureOffsetCache();
        return _offsetCache.Count == 0 ? 0d : _offsetCache[^1];
    }

    private double GetExtent(int index)
    {
        return _extentCache.TryGetValue(index, out var extent)
            ? extent
            : Math.Max(1d, EstimatedItemExtent);
    }

    private int FindFirstRangeOverlapIndex(double cacheStart)
    {
        var itemCount = Items.Count;
        var low = 0;
        var high = itemCount - 1;
        var result = itemCount;

        while (low <= high)
        {
            var mid = low + ((high - low) / 2);
            var itemEnd = _offsetCache[mid] + GetExtent(mid);
            if (itemEnd + SliverMath.Epsilon >= cacheStart)
            {
                result = mid;
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return result;
    }

    private void EnsureOffsetCache()
    {
        var itemCount = Items.Count;
        var spacing = Math.Max(0d, Spacing);
        var estimatedItemExtent = Math.Max(1d, EstimatedItemExtent);

        if (!_offsetCacheDirty &&
            _offsetCacheItemCount == itemCount &&
            SliverAvaloniaPrimitives.AreClose(_offsetCacheSpacing, spacing) &&
            SliverAvaloniaPrimitives.AreClose(_offsetCacheEstimatedItemExtent, estimatedItemExtent) &&
            _offsetCache.Count == itemCount + 1)
        {
            return;
        }

        _offsetCache.Clear();
        var offset = 0d;

        for (var index = 0; index < itemCount; index++)
        {
            _offsetCache.Add(offset);
            offset += GetExtent(index);

            if (index < itemCount - 1)
            {
                offset += spacing;
            }
        }

        _offsetCache.Add(offset);
        _offsetCacheDirty = false;
        _offsetCacheItemCount = itemCount;
        _offsetCacheSpacing = spacing;
        _offsetCacheEstimatedItemExtent = estimatedItemExtent;
    }

    private void InvalidateOffsetCache()
    {
        _offsetCacheDirty = true;
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
            var index = IndexFromContainer(container);
            if (index >= 0)
            {
                _containersByIndex.Remove(index);
            }

            _indexesByContainer.Remove(container);
            generator.ClearItemContainer(container);
            RemoveInternalChild(container);
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
