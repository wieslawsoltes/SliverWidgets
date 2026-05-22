using Avalonia;
using Avalonia.Controls;
using SliverWidgets.Core;

namespace SliverWidgets.Avalonia;

internal static class SliverAvaloniaPrimitives
{
    private const double DefaultViewportMainAxisExtent = 600d;
    private const double DefaultLineScrollExtent = 16d;

    public static double Main(this Size size, SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? size.Height : size.Width;
    }

    public static double Cross(this Size size, SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? size.Width : size.Height;
    }

    public static Size ToSize(SliverAxis axis, double mainAxisExtent, double crossAxisExtent)
    {
        return axis == SliverAxis.Vertical
            ? new Size(crossAxisExtent, mainAxisExtent)
            : new Size(mainAxisExtent, crossAxisExtent);
    }

    public static Rect ToRect(SliverAxis axis, SliverLayoutSlot slot)
    {
        return axis == SliverAxis.Vertical
            ? new Rect(slot.CrossAxisOffset, slot.MainAxisOffset, slot.CrossAxisExtent, slot.MainAxisExtent)
            : new Rect(slot.MainAxisOffset, slot.CrossAxisOffset, slot.MainAxisExtent, slot.CrossAxisExtent);
    }

    public static double MainStart(this Rect rect, SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? rect.Top : rect.Left;
    }

    public static double MainEnd(this Rect rect, SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? rect.Bottom : rect.Right;
    }

    public static Vector ToVector(SliverAxis axis, double mainAxisOffset, double crossAxisOffset = 0d)
    {
        return axis == SliverAxis.Vertical
            ? new Vector(crossAxisOffset, mainAxisOffset)
            : new Vector(mainAxisOffset, crossAxisOffset);
    }

    public static double Main(this Vector vector, SliverAxis axis)
    {
        return axis == SliverAxis.Vertical ? vector.Y : vector.X;
    }

    public static Size LineScrollSize(SliverAxis axis)
    {
        return ToSize(axis, DefaultLineScrollExtent, DefaultLineScrollExtent);
    }

    public static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) <= SliverMath.Epsilon;
    }

    public static bool AreClose(Size left, Size right)
    {
        return AreClose(left.Width, right.Width) && AreClose(left.Height, right.Height);
    }

    public static double ClampScrollOffset(double offset, double extent, double viewport)
    {
        return SliverMath.Clamp(offset, 0d, Math.Max(0d, extent - viewport));
    }

    public static bool IntersectsViewport(double mainAxisOffset, double mainAxisExtent, double viewportMainAxisExtent)
    {
        return mainAxisOffset + mainAxisExtent > SliverMath.Epsilon &&
               viewportMainAxisExtent - mainAxisOffset > SliverMath.Epsilon;
    }

    public static void ArrangeSlot(Control child, SliverAxis axis, SliverLayoutSlot slot, double viewportMainAxisExtent)
    {
        if (slot.IsCacheOnly ||
            !IntersectsViewport(slot.MainAxisOffset, slot.MainAxisExtent, viewportMainAxisExtent))
        {
            HideArrangedChild(child);
            return;
        }

        child.Opacity = 1d;
        child.IsHitTestVisible = true;
        child.Arrange(ToRect(axis, slot));
    }

    public static void HideArrangedChild(Control child)
    {
        child.Opacity = 0d;
        child.IsHitTestVisible = false;
        child.Arrange(default);
    }

    public static double FiniteOrZero(double value)
    {
        return double.IsNaN(value) ? 0d : Math.Max(0d, value);
    }

    public static double ResolveViewportMainAxisExtent(double availableMainAxisExtent, double previousViewportMainAxisExtent)
    {
        if (!double.IsInfinity(availableMainAxisExtent) && !double.IsNaN(availableMainAxisExtent))
        {
            return Math.Max(0d, availableMainAxisExtent);
        }

        return previousViewportMainAxisExtent > SliverMath.Epsilon
            ? previousViewportMainAxisExtent
            : DefaultViewportMainAxisExtent;
    }

    public static double ResolveViewportCrossAxisExtent(double availableCrossAxisExtent, double previousCrossAxisExtent)
    {
        if (double.IsFinite(availableCrossAxisExtent))
        {
            return Math.Max(0d, availableCrossAxisExtent);
        }

        return previousCrossAxisExtent > SliverMath.Epsilon
            ? previousCrossAxisExtent
            : DefaultViewportMainAxisExtent;
    }

    public static double ResolveDesiredCrossAxisExtent(double availableCrossAxisExtent, double measuredCrossAxisExtent, double fallbackCrossAxisExtent)
    {
        if (double.IsFinite(availableCrossAxisExtent))
        {
            return Math.Max(0d, availableCrossAxisExtent);
        }

        if (double.IsFinite(measuredCrossAxisExtent) && measuredCrossAxisExtent > SliverMath.Epsilon)
        {
            return measuredCrossAxisExtent;
        }

        return Math.Max(0d, fallbackCrossAxisExtent);
    }
}
