using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.Uno;

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
        var viewport = GetViewport(context, availableSize, Axis);
        var layout = new SliverFixedExtentListLayout(
            new SliverFixedExtentListOptions(context.ItemCount, Math.Max(0d, ItemExtent), Math.Max(0d, Spacing)));
        var result = layout.Layout(CreateConstraints(Axis, viewport));
        var childSize = ToSize(Axis, Math.Max(0d, ItemExtent), viewport.CrossAxisExtent);

        foreach (var slot in result.Slots)
        {
            context.GetOrCreateElementAt(slot.Index).Measure(childSize);
        }

        return ToSize(Axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var viewport = GetViewport(context, finalSize, Axis);
        var layout = new SliverFixedExtentListLayout(
            new SliverFixedExtentListOptions(context.ItemCount, Math.Max(0d, ItemExtent), Math.Max(0d, Spacing)));
        var result = layout.Layout(CreateConstraints(Axis, viewport));
        context.LayoutOrigin = new Point(0d, 0d);

        foreach (var slot in result.Slots)
        {
            context.GetOrCreateElementAt(slot.Index).Arrange(ToContentRect(Axis, slot, viewport.ScrollOffset));
        }

        return ToSize(Axis, result.Geometry.ScrollExtent, viewport.CrossAxisExtent);
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
