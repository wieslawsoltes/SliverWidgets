using Microsoft.Maui.Controls;
using SliverWidgets.Core;

namespace SliverWidgets.Maui;

public static class SliverItemsLayoutFactory
{
    public static LinearItemsLayout CreateFixedExtentList(SliverAxis axis, double spacing)
    {
        ValidateFiniteNonNegative(spacing, nameof(spacing));

        return new LinearItemsLayout(ToItemsLayoutOrientation(axis))
        {
            ItemSpacing = spacing
        };
    }

    public static GridItemsLayout CreateFixedExtentGrid(
        SliverAxis axis,
        int crossAxisCount,
        double mainAxisSpacing,
        double crossAxisSpacing)
    {
        if (crossAxisCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(crossAxisCount));
        }

        ValidateFiniteNonNegative(mainAxisSpacing, nameof(mainAxisSpacing));
        ValidateFiniteNonNegative(crossAxisSpacing, nameof(crossAxisSpacing));

        var isVertical = axis == SliverAxis.Vertical;
        return new GridItemsLayout(crossAxisCount, ToItemsLayoutOrientation(axis))
        {
            VerticalItemSpacing = isVertical ? mainAxisSpacing : crossAxisSpacing,
            HorizontalItemSpacing = isVertical ? crossAxisSpacing : mainAxisSpacing
        };
    }

    public static ItemsLayoutOrientation ToItemsLayoutOrientation(SliverAxis axis)
    {
        return axis switch
        {
            SliverAxis.Vertical => ItemsLayoutOrientation.Vertical,
            SliverAxis.Horizontal => ItemsLayoutOrientation.Horizontal,
            _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
        };
    }

    internal static void ValidateFiniteNonNegative(double value, string name)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
        {
            throw new ArgumentOutOfRangeException(name, value, "Value must be finite and non-negative.");
        }
    }
}
