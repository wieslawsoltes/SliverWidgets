using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.WinUI;

public class SliverStackVirtualizingLayout : VirtualizingLayout
{
    private SliverStackLayout? _layout;
    private StackLayoutKey _layoutKey;

    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(SliverAxis), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(SliverAxis.Vertical, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinItemMainAxisExtentProperty =
        DependencyProperty.Register(nameof(MinItemMainAxisExtent), typeof(double), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(52d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxItemMainAxisExtentProperty =
        DependencyProperty.Register(nameof(MaxItemMainAxisExtent), typeof(double), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(128d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinItemCrossAxisExtentProperty =
        DependencyProperty.Register(nameof(MinItemCrossAxisExtent), typeof(double), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(160d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxItemCrossAxisExtentProperty =
        DependencyProperty.Register(nameof(MaxItemCrossAxisExtent), typeof(double), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(640d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty CrossAxisAlignmentProperty =
        DependencyProperty.Register(nameof(CrossAxisAlignment), typeof(SliverCrossAxisAlignment), typeof(SliverStackVirtualizingLayout), new PropertyMetadata(SliverCrossAxisAlignment.Start, OnLayoutPropertyChanged));

    public SliverAxis Axis
    {
        get => (SliverAxis)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public double MinItemMainAxisExtent
    {
        get => (double)GetValue(MinItemMainAxisExtentProperty);
        set => SetValue(MinItemMainAxisExtentProperty, value);
    }

    public double MaxItemMainAxisExtent
    {
        get => (double)GetValue(MaxItemMainAxisExtentProperty);
        set => SetValue(MaxItemMainAxisExtentProperty, value);
    }

    public double MinItemCrossAxisExtent
    {
        get => (double)GetValue(MinItemCrossAxisExtentProperty);
        set => SetValue(MinItemCrossAxisExtentProperty, value);
    }

    public double MaxItemCrossAxisExtent
    {
        get => (double)GetValue(MaxItemCrossAxisExtentProperty);
        set => SetValue(MaxItemCrossAxisExtentProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public SliverCrossAxisAlignment CrossAxisAlignment
    {
        get => (SliverCrossAxisAlignment)GetValue(CrossAxisAlignmentProperty);
        set => SetValue(CrossAxisAlignmentProperty, value);
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
        if (dependencyObject is SliverStackVirtualizingLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }

    private SliverStackLayout CreateLayout(int itemCount)
    {
        var minMain = Math.Max(0d, MinItemMainAxisExtent);
        var maxMain = Math.Max(minMain, MaxItemMainAxisExtent);
        var minCross = Math.Max(0d, MinItemCrossAxisExtent);
        var maxCross = Math.Max(minCross, MaxItemCrossAxisExtent);
        var spacing = Math.Max(0d, Spacing);
        var alignment = CrossAxisAlignment;
        var key = new StackLayoutKey(itemCount, minMain, maxMain, minCross, maxCross, spacing, alignment);

        if (_layout is not null && key == _layoutKey)
        {
            return _layout;
        }

        _layoutKey = key;
        _layout = new SliverStackLayout(new SliverStackLayoutOptions(
            new SliverDeterministicStackExtentList(itemCount, minMain, maxMain, minCross, maxCross),
            spacing,
            alignment));
        return _layout;
    }

    private static SliverViewportInfo GetViewport(VirtualizingLayoutContext context, Size availableSize, SliverAxis axis)
    {
        var visible = context.VisibleRect;
        var realization = context.RealizationRect;
        var visibleStart = axis == SliverAxis.Vertical ? visible.Y : visible.X;
        var realizationStart = axis == SliverAxis.Vertical ? realization.Y : realization.X;
        var scrollOffset = visibleStart;
        var cacheOrigin = realizationStart - visibleStart;
        var remainingPaintExtent = axis == SliverAxis.Vertical ? visible.Height : visible.Width;
        var remainingCacheExtent = axis == SliverAxis.Vertical ? realization.Height : realization.Width;
        var crossAxisExtent = axis == SliverAxis.Vertical ? availableSize.Width : availableSize.Height;

        if (double.IsInfinity(crossAxisExtent) || double.IsNaN(crossAxisExtent))
        {
            crossAxisExtent = axis == SliverAxis.Vertical ? visible.Width : visible.Height;
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

    private readonly record struct StackLayoutKey(
        int ItemCount,
        double MinMain,
        double MaxMain,
        double MinCross,
        double MaxCross,
        double Spacing,
        SliverCrossAxisAlignment Alignment);
}
