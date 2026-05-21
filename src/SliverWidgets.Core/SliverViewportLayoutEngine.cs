namespace SliverWidgets.Core;

public readonly record struct SliverViewportSlot(
    int SliverIndex,
    int ItemIndex,
    double MainAxisOffset,
    double CrossAxisOffset,
    double MainAxisExtent,
    double CrossAxisExtent,
    bool IsPinned,
    bool IsCacheOnly);

public sealed record SliverViewportLayoutResult(
    double ScrollExtent,
    double MaxScrollOffset,
    IReadOnlyList<SliverGeometry> Geometries,
    IReadOnlyList<SliverViewportSlot> Slots);

public sealed class SliverViewportLayoutEngine
{
    private const int MaxScrollOffsetCorrectionIterations = 8;

    public SliverViewportLayoutResult Layout(
        IReadOnlyList<ISliverLayout> slivers,
        SliverViewport viewport,
        double scrollOffset,
        double cacheExtent = 0d,
        SliverUserScrollDirection userScrollDirection = SliverUserScrollDirection.Idle)
    {
        ArgumentNullException.ThrowIfNull(slivers);
        SliverMath.ThrowIfNegative(viewport.MainAxisExtent, nameof(viewport.MainAxisExtent));
        SliverMath.ThrowIfNegative(viewport.CrossAxisExtent, nameof(viewport.CrossAxisExtent));
        SliverMath.ThrowIfNegative(scrollOffset, nameof(scrollOffset));
        SliverMath.ThrowIfNegative(cacheExtent, nameof(cacheExtent));

        var effectiveScrollOffset = scrollOffset;

        for (var correctionIteration = 0; correctionIteration <= MaxScrollOffsetCorrectionIterations; correctionIteration++)
        {
            var slots = new List<SliverViewportSlot>();
            var geometries = new List<SliverGeometry>(slivers.Count);
            var precedingScrollExtent = 0d;
            var scrollOffsetRemaining = effectiveScrollOffset;
            var layoutOffset = 0d;
            var maxPaintOffset = 0d;
            var cacheOrigin = -cacheExtent;
            var remainingCacheExtent = viewport.MainAxisExtent + (cacheExtent * 2d);
            var corrected = false;

            for (var sliverIndex = 0; sliverIndex < slivers.Count; sliverIndex++)
            {
                var localScrollOffset = Math.Max(0d, scrollOffsetRemaining);
                var correctedCacheOrigin = Math.Max(cacheOrigin, -localScrollOffset);
                var cacheExtentCorrection = cacheOrigin - correctedCacheOrigin;
                var remainingPaintExtent = Math.Max(0d, viewport.MainAxisExtent - layoutOffset);
                var constraints = new SliverConstraints(
                    viewport.Axis,
                    localScrollOffset,
                    precedingScrollExtent,
                    maxPaintOffset - layoutOffset,
                    remainingPaintExtent,
                    viewport.CrossAxisExtent,
                    viewport.MainAxisExtent,
                    correctedCacheOrigin,
                    Math.Max(0d, remainingCacheExtent + cacheExtentCorrection),
                    UserScrollDirection: userScrollDirection);

                var result = slivers[sliverIndex].Layout(constraints);
                result.Geometry.Validate(constraints);

                if (result.Geometry.ScrollOffsetCorrection is { } correction &&
                    Math.Abs(correction) > SliverMath.Epsilon)
                {
                    if (!double.IsFinite(correction))
                    {
                        throw new InvalidOperationException("ScrollOffsetCorrection must be finite.");
                    }

                    if (correctionIteration == MaxScrollOffsetCorrectionIterations)
                    {
                        throw new InvalidOperationException("Sliver scroll offset corrections did not converge.");
                    }

                    effectiveScrollOffset = Math.Max(0d, effectiveScrollOffset + correction);
                    corrected = true;
                    break;
                }

                geometries.Add(result.Geometry);
                var effectiveLayoutOffset = layoutOffset + result.Geometry.PaintOrigin;

                foreach (var slot in result.Slots)
                {
                    slots.Add(new SliverViewportSlot(
                        sliverIndex,
                        slot.Index,
                        effectiveLayoutOffset + slot.MainAxisOffset,
                        slot.CrossAxisOffset,
                        slot.MainAxisExtent,
                        slot.CrossAxisExtent,
                        slot.IsPinned,
                        slot.IsCacheOnly));
                }

                maxPaintOffset = Math.Max(
                    maxPaintOffset,
                    effectiveLayoutOffset + result.Geometry.PaintExtent);
                scrollOffsetRemaining -= result.Geometry.ScrollExtent;
                precedingScrollExtent += result.Geometry.ScrollExtent;
                layoutOffset += result.Geometry.LayoutExtent;

                if (result.Geometry.CacheExtent > SliverMath.Epsilon)
                {
                    remainingCacheExtent = Math.Max(
                        0d,
                        remainingCacheExtent - (result.Geometry.CacheExtent - cacheExtentCorrection));
                    cacheOrigin = Math.Min(correctedCacheOrigin + result.Geometry.CacheExtent, 0d);
                }
            }

            if (corrected)
            {
                continue;
            }

            var maxScrollOffset = Math.Max(0d, precedingScrollExtent - viewport.MainAxisExtent);
            return new SliverViewportLayoutResult(precedingScrollExtent, maxScrollOffset, geometries, slots);
        }

        throw new InvalidOperationException("Sliver scroll offset corrections did not converge.");
    }
}
