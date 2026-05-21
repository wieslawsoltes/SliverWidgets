using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.WinUI;

public class SliverFixedExtentVirtualizingLayout : VirtualizingLayout
{
    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(SliverAxis), typeof(SliverFixedExtentVirtualizingLayout), new PropertyMetadata(SliverAxis.Vertical, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ItemExtentProperty =
        DependencyProperty.Register(nameof(ItemExtent), typeof(double), typeof(SliverFixedExtentVirtualizingLayout), new PropertyMetadata(48d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SliverFixedExtentVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public SliverAxis Axis
    {
        get => (SliverAxis)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public double ItemExtent
    {
        get => (double)GetValue(ItemExtentProperty);
        set => SetValue(ItemExtentProperty, value);
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var axis = Axis;
        var itemExtent = Math.Max(0d, ItemExtent);
        var spacing = Math.Max(0d, Spacing);
        var viewport = GetViewport(context, availableSize, axis);
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(context.ItemCount, itemExtent, spacing));
        var result = layout.Layout(CreateConstraints(axis, viewport));
        var childSize = ToSize(axis, itemExtent, viewport.CrossAxisExtent);

        foreach (var slot in result.Slots)
        {
            var element = context.GetOrCreateElementAt(slot.Index);
            element.Measure(childSize);
        }

        return ToSize(axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var axis = Axis;
        var itemExtent = Math.Max(0d, ItemExtent);
        var spacing = Math.Max(0d, Spacing);
        var viewport = GetViewport(context, finalSize, axis);
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(context.ItemCount, itemExtent, spacing));
        var result = layout.Layout(CreateConstraints(axis, viewport));

        context.LayoutOrigin = new Point(0d, 0d);

        foreach (var slot in result.Slots)
        {
            var element = context.GetOrCreateElementAt(slot.Index);
            element.Arrange(ToContentRect(axis, slot, viewport.ScrollOffset));
        }

        return ToSize(axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
    }

    private static void OnLayoutPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        if (dependencyObject is SliverFixedExtentVirtualizingLayout layout)
        {
            layout.InvalidateMeasure();
        }
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
}
