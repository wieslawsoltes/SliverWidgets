using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using SliverWidgets.Core;

namespace SliverWidgets.Maui;

public class SliverStackLayout : Layout
{
    public static readonly BindableProperty AxisProperty =
        BindableProperty.Create(nameof(Axis), typeof(SliverAxis), typeof(SliverStackLayout), SliverAxis.Vertical, propertyChanged: InvalidateLayout);

    public static readonly BindableProperty ItemExtentProperty =
        BindableProperty.Create(nameof(ItemExtent), typeof(double), typeof(SliverStackLayout), 48d, propertyChanged: InvalidateLayout);

    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(nameof(Spacing), typeof(double), typeof(SliverStackLayout), 0d, propertyChanged: InvalidateLayout);

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

    protected override ILayoutManager CreateLayoutManager()
    {
        return new SliverStackLayoutManager(this);
    }

    private static void InvalidateLayout(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SliverStackLayout layout)
        {
            layout.InvalidateMeasure();
        }
    }
}

internal sealed class SliverStackLayoutManager : ILayoutManager
{
    private readonly SliverStackLayout _layout;

    public SliverStackLayoutManager(SliverStackLayout layout)
    {
        _layout = layout;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        var axis = _layout.Axis;
        var crossAxisConstraint = axis == SliverAxis.Vertical
            ? ResolveMeasureConstraint(widthConstraint)
            : ResolveMeasureConstraint(heightConstraint);
        var itemExtent = Math.Max(0d, _layout.ItemExtent);
        var childWidth = axis == SliverAxis.Vertical ? crossAxisConstraint : itemExtent;
        var childHeight = axis == SliverAxis.Vertical ? itemExtent : crossAxisConstraint;
        var measuredCrossAxisExtent = 0d;

        foreach (var child in _layout)
        {
            var desired = child.Measure(childWidth, childHeight);
            measuredCrossAxisExtent = Math.Max(
                measuredCrossAxisExtent,
                axis == SliverAxis.Vertical ? desired.Width : desired.Height);
        }

        var crossAxisExtent = ResolveCrossAxisExtent(
            axis == SliverAxis.Vertical ? widthConstraint : heightConstraint,
            measuredCrossAxisExtent);
        var mainAxisExtent = SliverFixedExtentListLayout.GetScrollExtent(_layout.Count, itemExtent, Math.Max(0d, _layout.Spacing));
        return axis == SliverAxis.Vertical
            ? new Size(crossAxisExtent, mainAxisExtent)
            : new Size(mainAxisExtent, crossAxisExtent);
    }

    public Size ArrangeChildren(Rect bounds)
    {
        var axis = _layout.Axis;
        var itemExtent = Math.Max(0d, _layout.ItemExtent);
        var spacing = Math.Max(0d, _layout.Spacing);
        var crossAxisExtent = axis == SliverAxis.Vertical ? bounds.Width : bounds.Height;
        var viewportMainAxisExtent = axis == SliverAxis.Vertical ? bounds.Height : bounds.Width;
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(_layout.Count, itemExtent, spacing));
        var result = layout.Layout(new SliverConstraints(
            axis,
            0d,
            0d,
            0d,
            viewportMainAxisExtent,
            crossAxisExtent,
            viewportMainAxisExtent,
            0d,
            viewportMainAxisExtent));

        foreach (var slot in result.Slots)
        {
            var child = _layout[slot.Index];
            var rect = axis == SliverAxis.Vertical
                ? new Rect(bounds.X + slot.CrossAxisOffset, bounds.Y + slot.MainAxisOffset, slot.CrossAxisExtent, slot.MainAxisExtent)
                : new Rect(bounds.X + slot.MainAxisOffset, bounds.Y + slot.CrossAxisOffset, slot.MainAxisExtent, slot.CrossAxisExtent);
            child.Arrange(rect);
        }

        return bounds.Size;
    }

    private static double ResolveMeasureConstraint(double value)
    {
        return double.IsFinite(value) ? Math.Max(0d, value) : double.PositiveInfinity;
    }

    private static double ResolveCrossAxisExtent(double constraint, double measuredExtent)
    {
        return double.IsFinite(constraint)
            ? Math.Max(0d, constraint)
            : Math.Max(0d, measuredExtent);
    }
}
