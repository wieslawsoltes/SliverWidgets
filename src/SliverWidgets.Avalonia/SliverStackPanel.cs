using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverStackPanel : Panel, ILogicalScrollable
{
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverStackPanel, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<double> ItemExtentProperty =
        AvaloniaProperty.Register<SliverStackPanel, double>(nameof(ItemExtent), 48d);

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<SliverStackPanel, double>(nameof(Spacing));

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverStackPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverStackPanel, double>(nameof(CacheExtent), 250d);

    static SliverStackPanel()
    {
        AffectsMeasure<SliverStackPanel>(AxisProperty, ItemExtentProperty, SpacingProperty);
        AffectsArrange<SliverStackPanel>(AxisProperty, ItemExtentProperty, SpacingProperty, ScrollOffsetProperty, CacheExtentProperty);
    }

    public SliverStackPanel()
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

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var itemExtent = Math.Max(0d, ItemExtent);
        var crossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var childConstraint = SliverAvaloniaPrimitives.ToSize(axis, itemExtent, crossAxisExtent);

        foreach (var child in Children)
        {
            child.Measure(childConstraint);
        }

        var mainAxisExtent = SliverFixedExtentListLayout.GetScrollExtent(Children.Count, itemExtent, Math.Max(0d, Spacing));
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Main(axis)), crossAxisExtent);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, mainAxisExtent, crossAxisExtent);
        UpdateScrollInfo(extent, viewport);
        return IsScrollingEnabled(axis) ? viewport : extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var axis = Axis;
        var itemExtent = Math.Max(0d, ItemExtent);
        var spacing = Math.Max(0d, Spacing);
        var viewportMainAxisExtent = finalSize.Main(axis);
        var crossAxisExtent = finalSize.Cross(axis);
        var constraints = new SliverConstraints(
            axis,
            Math.Max(0d, ScrollOffset),
            0d,
            0d,
            viewportMainAxisExtent,
            crossAxisExtent,
            viewportMainAxisExtent,
            -Math.Max(0d, CacheExtent),
            viewportMainAxisExtent + (Math.Max(0d, CacheExtent) * 2d));
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(Children.Count, itemExtent, spacing));
        var result = layout.Layout(constraints);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, crossAxisExtent);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent);
        UpdateScrollInfo(extent, viewport);
        var slots = result.Slots;
        var arranged = new bool[Children.Count];

        foreach (var slot in slots)
        {
            if (slot.Index >= 0 && slot.Index < Children.Count)
            {
                SliverAvaloniaPrimitives.ArrangeSlot(Children[slot.Index], axis, slot, viewportMainAxisExtent);
                arranged[slot.Index] = true;
            }
        }

        for (var i = 0; i < Children.Count; i++)
        {
            if (!arranged[i])
            {
                SliverAvaloniaPrimitives.HideArrangedChild(Children[i]);
            }
        }

        return finalSize;
    }

    public bool BringIntoView(Control target, Rect targetRect)
    {
        var index = Children.IndexOf(target);
        if (index < 0)
        {
            return false;
        }

        var itemStart = index * (Math.Max(0d, ItemExtent) + Math.Max(0d, Spacing));
        var itemEnd = itemStart + Math.Max(0d, ItemExtent);
        return BringRangeIntoView(
            itemStart + targetRect.MainStart(Axis),
            Math.Min(itemEnd, itemStart + targetRect.MainEnd(Axis)));
    }

    public Control? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        if (Children.Count == 0)
        {
            return null;
        }

        var currentIndex = from is null ? -1 : Children.IndexOf(from);
        var targetIndex = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Children.Count - 1,
            NavigationDirection.Next or NavigationDirection.Down or NavigationDirection.Right => currentIndex + 1,
            NavigationDirection.Previous or NavigationDirection.Up or NavigationDirection.Left => currentIndex - 1,
            _ => currentIndex
        };

        if (targetIndex < 0 || targetIndex >= Children.Count)
        {
            return null;
        }

        var target = Children[targetIndex];
        BringIntoView(target, target.Bounds);
        return target;
    }

    public void RaiseScrollInvalidated(EventArgs e)
    {
        ScrollInvalidated?.Invoke(this, e);
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
        InvalidateArrange();
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
