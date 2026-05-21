using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SliverWidgets.Core;
using Windows.Foundation;

namespace SliverWidgets.Uno;

public class SliverWrapVirtualizingLayout : VirtualizingLayout
{
    private SliverWrapLayout? _layout;
    private WrapLayoutKey _layoutKey;

    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(SliverAxis), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(SliverAxis.Vertical, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinItemMainAxisExtentProperty =
        DependencyProperty.Register(nameof(MinItemMainAxisExtent), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(56d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxItemMainAxisExtentProperty =
        DependencyProperty.Register(nameof(MaxItemMainAxisExtent), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(132d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinItemCrossAxisExtentProperty =
        DependencyProperty.Register(nameof(MinItemCrossAxisExtent), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(120d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxItemCrossAxisExtentProperty =
        DependencyProperty.Register(nameof(MaxItemCrossAxisExtent), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(280d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MainAxisSpacingProperty =
        DependencyProperty.Register(nameof(MainAxisSpacing), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty CrossAxisSpacingProperty =
        DependencyProperty.Register(nameof(CrossAxisSpacing), typeof(double), typeof(SliverWrapVirtualizingLayout), new PropertyMetadata(0d, OnLayoutPropertyChanged));

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
        if (dependencyObject is SliverWrapVirtualizingLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }

    private SliverWrapLayout CreateLayout(int itemCount)
    {
        var minMain = Math.Max(0d, MinItemMainAxisExtent);
        var maxMain = Math.Max(minMain, MaxItemMainAxisExtent);
        var minCross = Math.Max(0d, MinItemCrossAxisExtent);
        var maxCross = Math.Max(minCross, MaxItemCrossAxisExtent);
        var mainSpacing = Math.Max(0d, MainAxisSpacing);
        var crossSpacing = Math.Max(0d, CrossAxisSpacing);
        var key = new WrapLayoutKey(itemCount, minMain, maxMain, minCross, maxCross, mainSpacing, crossSpacing);

        if (_layout is not null && key == _layoutKey)
        {
            return _layout;
        }

        _layoutKey = key;
        _layout = new SliverWrapLayout(new SliverWrapLayoutOptions(
            new SliverDeterministicWrapExtentList(itemCount, minMain, maxMain, minCross, maxCross),
            mainSpacing,
            crossSpacing));
        return _layout;
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

    private readonly record struct WrapLayoutKey(
        int ItemCount,
        double MinMain,
        double MaxMain,
        double MinCross,
        double MaxCross,
        double MainSpacing,
        double CrossSpacing);
}
