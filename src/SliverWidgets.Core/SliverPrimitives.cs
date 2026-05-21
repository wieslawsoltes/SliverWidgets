namespace SliverWidgets.Core;

public enum SliverAxis
{
    Vertical,
    Horizontal
}

public enum SliverGrowthDirection
{
    Forward,
    Reverse
}

public enum SliverUserScrollDirection
{
    Idle,
    Forward,
    Reverse
}

public readonly record struct SliverViewport(
    double MainAxisExtent,
    double CrossAxisExtent,
    SliverAxis Axis = SliverAxis.Vertical);

public readonly record struct SliverConstraints(
    SliverAxis Axis,
    double ScrollOffset,
    double PrecedingScrollExtent,
    double Overlap,
    double RemainingPaintExtent,
    double CrossAxisExtent,
    double ViewportMainAxisExtent,
    double CacheOrigin,
    double RemainingCacheExtent,
    SliverGrowthDirection GrowthDirection = SliverGrowthDirection.Forward,
    SliverUserScrollDirection UserScrollDirection = SliverUserScrollDirection.Idle)
{
    public void Validate()
    {
        SliverMath.ThrowIfNegative(ScrollOffset, nameof(ScrollOffset));
        SliverMath.ThrowIfNegative(PrecedingScrollExtent, nameof(PrecedingScrollExtent));
        SliverMath.ThrowIfNotFinite(Overlap, nameof(Overlap));
        SliverMath.ThrowIfNegative(RemainingPaintExtent, nameof(RemainingPaintExtent));
        SliverMath.ThrowIfNegative(CrossAxisExtent, nameof(CrossAxisExtent));
        SliverMath.ThrowIfNegative(ViewportMainAxisExtent, nameof(ViewportMainAxisExtent));
        SliverMath.ThrowIfNotFinite(CacheOrigin, nameof(CacheOrigin));
        if (CacheOrigin > SliverMath.Epsilon)
        {
            throw new ArgumentOutOfRangeException(nameof(CacheOrigin), CacheOrigin, "CacheOrigin must be zero or negative.");
        }

        SliverMath.ThrowIfNegative(RemainingCacheExtent, nameof(RemainingCacheExtent));
        if (RemainingPaintExtent - RemainingCacheExtent > SliverMath.Epsilon)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RemainingCacheExtent),
                RemainingCacheExtent,
                "RemainingCacheExtent must be greater than or equal to RemainingPaintExtent.");
        }
    }
}

public sealed record SliverGeometry
{
    public static SliverGeometry Zero { get; } = new();

    public double ScrollExtent { get; init; }

    public double PaintExtent { get; init; }

    public double PaintOrigin { get; init; }

    public double LayoutExtent { get; init; }

    public double MaxPaintExtent { get; init; }

    public double MaxScrollObstructionExtent { get; init; }

    public double HitTestExtent { get; init; }

    public bool Visible { get; init; }

    public bool HasVisualOverflow { get; init; }

    public double? ScrollOffsetCorrection { get; init; }

    public double CacheExtent { get; init; }

    public double CrossAxisExtent { get; init; }

    public void Validate(in SliverConstraints constraints)
    {
        SliverMath.ThrowIfNegative(ScrollExtent, nameof(ScrollExtent));
        SliverMath.ThrowIfNegative(PaintExtent, nameof(PaintExtent));
        SliverMath.ThrowIfNotFinite(PaintOrigin, nameof(PaintOrigin));
        SliverMath.ThrowIfNegative(LayoutExtent, nameof(LayoutExtent));
        SliverMath.ThrowIfNegative(MaxPaintExtent, nameof(MaxPaintExtent));
        SliverMath.ThrowIfNegative(MaxScrollObstructionExtent, nameof(MaxScrollObstructionExtent));
        SliverMath.ThrowIfNegative(HitTestExtent, nameof(HitTestExtent));
        SliverMath.ThrowIfNegative(CacheExtent, nameof(CacheExtent));
        SliverMath.ThrowIfNegative(CrossAxisExtent, nameof(CrossAxisExtent));

        if (PaintExtent - constraints.RemainingPaintExtent > SliverMath.Epsilon)
        {
            throw new InvalidOperationException(
                $"PaintExtent ({PaintExtent}) cannot exceed RemainingPaintExtent ({constraints.RemainingPaintExtent}).");
        }

        if (LayoutExtent - PaintExtent > SliverMath.Epsilon)
        {
            throw new InvalidOperationException(
                $"LayoutExtent ({LayoutExtent}) cannot exceed PaintExtent ({PaintExtent}).");
        }

        if (PaintExtent - MaxPaintExtent > SliverMath.Epsilon)
        {
            throw new InvalidOperationException(
                $"PaintExtent ({PaintExtent}) cannot exceed MaxPaintExtent ({MaxPaintExtent}).");
        }

        if (PaintOrigin + PaintExtent - constraints.RemainingPaintExtent > SliverMath.Epsilon)
        {
            throw new InvalidOperationException(
                $"PaintOrigin + PaintExtent ({PaintOrigin + PaintExtent}) cannot exceed RemainingPaintExtent ({constraints.RemainingPaintExtent}).");
        }

        if (ScrollOffsetCorrection is { } correction &&
            (!double.IsFinite(correction) || Math.Abs(correction) <= SliverMath.Epsilon))
        {
            throw new InvalidOperationException("ScrollOffsetCorrection must be finite and non-zero.");
        }
    }
}

public readonly record struct SliverLayoutSlot(
    int Index,
    double MainAxisOffset,
    double CrossAxisOffset,
    double MainAxisExtent,
    double CrossAxisExtent,
    bool IsPinned = false,
    bool IsCacheOnly = false);

public sealed record SliverLayoutResult(SliverGeometry Geometry, IReadOnlyList<SliverLayoutSlot> Slots)
{
    public static SliverLayoutResult Empty { get; } = new(SliverGeometry.Zero, Array.Empty<SliverLayoutSlot>());
}

public interface ISliverLayout
{
    SliverLayoutResult Layout(in SliverConstraints constraints);
}

public static class SliverMath
{
    public const double Epsilon = 0.0001d;

    public static double Clamp(double value, double min, double max)
    {
        if (double.IsNaN(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Layout values cannot be NaN.");
        }

        if (max < min)
        {
            max = min;
        }

        return Math.Min(Math.Max(value, min), max);
    }

    public static double ClampNonNegative(double value) => Clamp(value, 0d, double.MaxValue);

    public static double ClampPaintExtent(double scrollExtent, double scrollOffset, double remainingPaintExtent)
    {
        return Clamp(scrollExtent - scrollOffset, 0d, remainingPaintExtent);
    }

    internal static double CalculatePaintOffset(in SliverConstraints constraints, double from, double to)
    {
        if (to < from)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "The end offset must be greater than or equal to the start offset.");
        }

        var start = constraints.ScrollOffset;
        var end = constraints.ScrollOffset + constraints.RemainingPaintExtent;
        return Clamp(Clamp(to, start, end) - Clamp(from, start, end), 0d, constraints.RemainingPaintExtent);
    }

    internal static double CalculateCacheOffset(in SliverConstraints constraints, double from, double to)
    {
        if (to < from)
        {
            throw new ArgumentOutOfRangeException(nameof(to), "The end offset must be greater than or equal to the start offset.");
        }

        var start = constraints.ScrollOffset + constraints.CacheOrigin;
        var end = constraints.ScrollOffset + constraints.RemainingCacheExtent;
        return Clamp(Clamp(to, start, end) - Clamp(from, start, end), 0d, constraints.RemainingCacheExtent);
    }

    public static void ThrowIfNegative(double value, string name)
    {
        if (!double.IsFinite(value) || value < -Epsilon)
        {
            throw new ArgumentOutOfRangeException(name, value, "Sliver geometry values must be finite and non-negative.");
        }
    }

    public static void ThrowIfNotFinite(double value, string name)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(name, value, "Sliver layout values must be finite.");
        }
    }

    internal static (double Start, double End) ResolveCacheRange(double scrollOffset, double cacheOrigin, double remainingCacheExtent)
    {
        var start = Math.Max(0d, scrollOffset + cacheOrigin);
        var end = Math.Max(start, scrollOffset + cacheOrigin + remainingCacheExtent);
        return (start, end);
    }

    internal static bool RangesOverlap(double start, double end, double rangeStart, double rangeEnd)
    {
        return end - rangeStart > Epsilon && rangeEnd - start > Epsilon;
    }

    internal static bool IsCacheOnly(double start, double end, double visibleStart, double visibleEnd)
    {
        return !RangesOverlap(start, end, visibleStart, visibleEnd);
    }
}
