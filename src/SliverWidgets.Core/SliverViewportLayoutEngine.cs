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
            var activeScrollObstructionExtent = 0d;
            var corrected = false;

            for (var sliverIndex = 0; sliverIndex < slivers.Count; sliverIndex++)
            {
                var localScrollOffset = Math.Max(0d, effectiveScrollOffset - precedingScrollExtent);
                var naturalLeadingViewportOffset = Math.Max(0d, precedingScrollExtent - effectiveScrollOffset);
                var sliverLeadingViewportOffset = Math.Max(activeScrollObstructionExtent, naturalLeadingViewportOffset);
                var remainingPaintExtent = Math.Max(
                    0d,
                    viewport.MainAxisExtent - Math.Min(viewport.MainAxisExtent, sliverLeadingViewportOffset));
                var constraints = new SliverConstraints(
                    viewport.Axis,
                    localScrollOffset,
                    precedingScrollExtent,
                    activeScrollObstructionExtent,
                    remainingPaintExtent,
                    viewport.CrossAxisExtent,
                    viewport.MainAxisExtent,
                    -cacheExtent,
                    viewport.MainAxisExtent + (cacheExtent * 2d),
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

                foreach (var slot in result.Slots)
                {
                    slots.Add(new SliverViewportSlot(
                        sliverIndex,
                        slot.Index,
                        sliverLeadingViewportOffset + slot.MainAxisOffset,
                        slot.CrossAxisOffset,
                        slot.MainAxisExtent,
                        slot.CrossAxisExtent,
                        slot.IsPinned,
                        slot.IsCacheOnly));
                }

                precedingScrollExtent += result.Geometry.ScrollExtent;
                activeScrollObstructionExtent += result.Geometry.MaxScrollObstructionExtent;
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
