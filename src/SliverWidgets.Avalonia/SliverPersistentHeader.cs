using Avalonia;
using Avalonia.Controls;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

public class SliverPersistentHeader : Decorator
{
    public static readonly StyledProperty<SliverAxis> AxisProperty =
        AvaloniaProperty.Register<SliverPersistentHeader, SliverAxis>(nameof(Axis), SliverAxis.Vertical);

    public static readonly StyledProperty<double> MinExtentProperty =
        AvaloniaProperty.Register<SliverPersistentHeader, double>(nameof(MinExtent), 48d);

    public static readonly StyledProperty<double> MaxExtentProperty =
        AvaloniaProperty.Register<SliverPersistentHeader, double>(nameof(MaxExtent), 160d);

    public static readonly StyledProperty<bool> PinnedProperty =
        AvaloniaProperty.Register<SliverPersistentHeader, bool>(nameof(Pinned), true);

    public static readonly StyledProperty<double> ScrollOffsetProperty =
        AvaloniaProperty.Register<SliverPersistentHeader, double>(nameof(ScrollOffset));

    static SliverPersistentHeader()
    {
        AffectsMeasure<SliverPersistentHeader>(AxisProperty, MinExtentProperty, MaxExtentProperty, PinnedProperty, ScrollOffsetProperty);
        AffectsArrange<SliverPersistentHeader>(AxisProperty, MinExtentProperty, MaxExtentProperty, PinnedProperty, ScrollOffsetProperty);
    }

    public SliverPersistentHeader()
    {
        ClipToBounds = true;
    }

    public SliverAxis Axis
    {
        get => GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    public double MinExtent
    {
        get => GetValue(MinExtentProperty);
        set => SetValue(MinExtentProperty, value);
    }

    public double MaxExtent
    {
        get => GetValue(MaxExtentProperty);
        set => SetValue(MaxExtentProperty, value);
    }

    public bool Pinned
    {
        get => GetValue(PinnedProperty);
        set => SetValue(PinnedProperty, value);
    }

    public double ScrollOffset
    {
        get => GetValue(ScrollOffsetProperty);
        set => SetValue(ScrollOffsetProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Child is null)
        {
            return default;
        }

        var axis = Axis;
        var measureCrossAxisExtent = SliverAvaloniaPrimitives.FiniteOrZero(availableSize.Cross(axis));
        var currentExtent = GetCurrentExtent();
        var visibleExtent = GetVisibleExtent(currentExtent, availableSize.Main(axis));
        Child.Measure(SliverAvaloniaPrimitives.ToSize(axis, currentExtent, measureCrossAxisExtent));
        var desiredCrossAxisExtent = SliverAvaloniaPrimitives.ResolveDesiredCrossAxisExtent(
            availableSize.Cross(axis),
            Child.DesiredSize.Cross(axis),
            0d);
        return SliverAvaloniaPrimitives.ToSize(axis, visibleExtent, desiredCrossAxisExtent);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is null)
        {
            return finalSize;
        }

        var axis = Axis;
        var viewportMainAxisExtent = finalSize.Main(axis);
        var constraints = new SliverConstraints(
            axis,
            Math.Max(0d, ScrollOffset),
            0d,
            0d,
            viewportMainAxisExtent,
            finalSize.Cross(axis),
            viewportMainAxisExtent,
            0d,
            viewportMainAxisExtent);
        var layout = new SliverPersistentHeaderLayout(
            new SliverPersistentHeaderOptions(Math.Max(0d, MinExtent), Math.Max(MinExtent, MaxExtent), Pinned));
        var result = layout.Layout(constraints);
        var slot = result.Slots.FirstOrDefault();
        if (result.Slots.Count == 0)
        {
            SliverAvaloniaPrimitives.HideArrangedChild(Child);
            return finalSize;
        }

        SliverAvaloniaPrimitives.ArrangeSlot(Child, axis, slot, viewportMainAxisExtent);
        return finalSize;
    }

    private double GetCurrentExtent()
    {
        var minExtent = Math.Max(0d, MinExtent);
        var maxExtent = Math.Max(minExtent, MaxExtent);
        var shrinkOffset = SliverMath.Clamp(Math.Max(0d, ScrollOffset), 0d, maxExtent - minExtent);
        return SliverMath.Clamp(maxExtent - shrinkOffset, minExtent, maxExtent);
    }

    private double GetVisibleExtent(double currentExtent, double availableMainAxisExtent)
    {
        var maxExtent = Math.Max(Math.Max(0d, MinExtent), MaxExtent);
        var visibleExtent = Pinned
            ? currentExtent
            : Math.Min(currentExtent, Math.Max(0d, maxExtent - Math.Max(0d, ScrollOffset)));

        if (double.IsFinite(availableMainAxisExtent))
        {
            return Math.Min(visibleExtent, Math.Max(0d, availableMainAxisExtent));
        }

        return visibleExtent;
    }
}
