using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverGridPanel : Panel, ILogicalScrollable
{
    private bool _canHorizontallyScroll;
    private bool _canVerticallyScroll;
    private Size _extent;
    private Size _viewport;

    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverGridPanel, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<SliverGridSizingMode> SizingModeProperty =
        AvaloniaProperty.Register<SliverGridPanel, SliverGridSizingMode>(nameof(SizingMode), SliverGridSizingMode.FixedCrossAxisCount);

    public static readonly StyledProperty<int> CrossAxisCountProperty =
        AvaloniaProperty.Register<SliverGridPanel, int>(nameof(CrossAxisCount), 2);

    public static readonly StyledProperty<double> MaxCrossAxisExtentProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(MaxCrossAxisExtent), 240d);

    public static readonly StyledProperty<double> MainAxisSpacingProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(MainAxisSpacing));

    public static readonly StyledProperty<double> CrossAxisSpacingProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(CrossAxisSpacing));

    public static readonly StyledProperty<double> ChildAspectRatioProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(ChildAspectRatio), 1d);

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(ScrollOffset));

    public static readonly StyledProperty<double> CacheExtentProperty =
        AvaloniaProperty.Register<SliverGridPanel, double>(nameof(CacheExtent), 250d);

    static SliverGridPanel()
    {
        AffectsMeasure<SliverGridPanel>(
            AxisProperty,
            SizingModeProperty,
            CrossAxisCountProperty,
            MaxCrossAxisExtentProperty,
            MainAxisSpacingProperty,
            CrossAxisSpacingProperty,
            ChildAspectRatioProperty);
        AffectsArrange<SliverGridPanel>(
            AxisProperty,
            SizingModeProperty,
            CrossAxisCountProperty,
            MaxCrossAxisExtentProperty,
            MainAxisSpacingProperty,
            CrossAxisSpacingProperty,
            ChildAspectRatioProperty,
            ScrollOffsetProperty,
            CacheExtentProperty);
    }

    public SliverGridPanel()
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

    protected override Size MeasureOverride(Size availableSize)
    {
        var axis = Axis;
        var viewportMainAxisExtent = SliverAvaloniaPrimitives.ResolveViewportMainAxisExtent(
            availableSize.Main(axis),
            _viewport.Main(axis));
        var crossAxisExtent = SliverAvaloniaPrimitives.ResolveViewportCrossAxisExtent(
            availableSize.Cross(axis),
            _viewport.Cross(axis));
        var layout = CreateLayout(Children.Count);
        var constraints = new SliverConstraints(
            axis,
            0d,
            0d,
            0d,
            double.MaxValue,
            crossAxisExtent,
            viewportMainAxisExtent,
            0d,
            double.MaxValue);
        var result = layout.Layout(constraints);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, crossAxisExtent);
        var measured = new bool[Children.Count];

        foreach (var slot in result.Slots)
        {
            if (slot.Index >= 0 && slot.Index < Children.Count)
            {
                Children[slot.Index].Measure(SliverAvaloniaPrimitives.ToSize(axis, slot.MainAxisExtent, slot.CrossAxisExtent));
                measured[slot.Index] = true;
            }
        }

        for (var index = 0; index < Children.Count; index++)
        {
            if (!measured[index])
            {
                Children[index].Measure(default);
            }
        }

        UpdateScrollInfo(extent, viewport);
        return IsScrollingEnabled(axis) ? viewport : extent;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var axis = Axis;
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
        var result = CreateLayout(Children.Count).Layout(constraints);
        var extent = SliverAvaloniaPrimitives.ToSize(axis, result.Geometry.ScrollExtent, crossAxisExtent);
        var viewport = SliverAvaloniaPrimitives.ToSize(axis, viewportMainAxisExtent, crossAxisExtent);
        var arranged = new bool[Children.Count];

        UpdateScrollInfo(extent, viewport);

        foreach (var slot in result.Slots)
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

        var metrics = ResolveMetrics(_viewport.Cross(Axis));
        var row = index / metrics.CrossAxisCount;
        var rowStart = row * (metrics.TileMainAxisExtent + Math.Max(0d, MainAxisSpacing));
        return BringRangeIntoView(rowStart, rowStart + metrics.TileMainAxisExtent);
    }

    public Control? GetControlInDirection(NavigationDirection direction, Control? from)
    {
        if (Children.Count == 0)
        {
            return null;
        }

        var metrics = ResolveMetrics(_viewport.Cross(Axis));
        var currentIndex = from is null ? -1 : Children.IndexOf(from);
        var targetIndex = direction switch
        {
            NavigationDirection.First => 0,
            NavigationDirection.Last => Children.Count - 1,
            NavigationDirection.Next => currentIndex + 1,
            NavigationDirection.Previous => currentIndex - 1,
            NavigationDirection.Down => currentIndex + metrics.CrossAxisCount,
            NavigationDirection.Up => currentIndex - metrics.CrossAxisCount,
            NavigationDirection.Right => Axis == SliverAxis.Vertical ? currentIndex + 1 : currentIndex + metrics.CrossAxisCount,
            NavigationDirection.Left => Axis == SliverAxis.Vertical ? currentIndex - 1 : currentIndex - metrics.CrossAxisCount,
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

    private SliverGridLayout CreateLayout(int itemCount)
    {
        var options = SizingMode == SliverGridSizingMode.FixedCrossAxisCount
            ? SliverGridLayoutOptions.FixedCrossAxisCount(
                itemCount,
                Math.Max(1, CrossAxisCount),
                Math.Max(0d, MainAxisSpacing),
                Math.Max(0d, CrossAxisSpacing),
                Math.Max(SliverMath.Epsilon, ChildAspectRatio))
            : SliverGridLayoutOptions.WithMaxCrossAxisExtent(
                itemCount,
                Math.Max(SliverMath.Epsilon, MaxCrossAxisExtent),
                Math.Max(0d, MainAxisSpacing),
                Math.Max(0d, CrossAxisSpacing),
                Math.Max(SliverMath.Epsilon, ChildAspectRatio));

        return new SliverGridLayout(options);
    }

    private GridMetrics ResolveMetrics(double crossAxisExtent)
    {
        var layout = CreateLayout(Children.Count);
        var crossAxisCount = layout.ResolveCrossAxisCount(Math.Max(0d, crossAxisExtent));
        var crossAxisSpacing = Math.Max(0d, CrossAxisSpacing);
        var tileCrossAxisExtent = Math.Max(0d, (Math.Max(0d, crossAxisExtent) - ((crossAxisCount - 1) * crossAxisSpacing)) / crossAxisCount);
        var tileMainAxisExtent = tileCrossAxisExtent / Math.Max(SliverMath.Epsilon, ChildAspectRatio);
        return new GridMetrics(crossAxisCount, tileMainAxisExtent);
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

    private readonly record struct GridMetrics(int CrossAxisCount, double TileMainAxisExtent);
}
