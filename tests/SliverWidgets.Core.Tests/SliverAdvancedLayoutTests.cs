using SliverWidgets.Core;

namespace SliverWidgets.Core.Tests;

public sealed class SliverAdvancedLayoutTests
{
    private static SliverConstraints Constraints(
        double scrollOffset = 0d,
        double remainingPaintExtent = 100d,
        double crossAxisExtent = 200d,
        double cacheOrigin = 0d,
        double remainingCacheExtent = 100d,
        SliverUserScrollDirection userScrollDirection = SliverUserScrollDirection.Idle)
    {
        return new SliverConstraints(
            SliverAxis.Vertical,
            scrollOffset,
            0d,
            0d,
            remainingPaintExtent,
            crossAxisExtent,
            remainingPaintExtent,
            cacheOrigin,
            Math.Max(remainingCacheExtent, remainingPaintExtent),
            UserScrollDirection: userScrollDirection);
    }

    [Fact]
    public void ChildExtentCacheDeadReckonsMissingExtentsFromObservedAverage()
    {
        var cache = new SliverChildExtentCache(itemCount: 5, defaultExtent: 20d, spacing: 2d);

        cache.Observe(0, 10d);
        cache.Observe(2, 30d);

        Assert.Equal(20d, cache.DeadReckonedExtent);
        Assert.Equal(34d, cache.GetLeadingOffset(2));
        Assert.Equal(108d, cache.EstimateScrollExtent());
        Assert.Equal(2, cache.GetIndexAtScrollOffset(35d));
    }

    [Fact]
    public void VariableExtentListRealizesPaintAndCacheWindowOnly()
    {
        var cache = new SliverChildExtentCache(itemCount: 1_000, defaultExtent: 20d);
        var layout = new SliverVariableExtentListLayout(cache);

        var result = layout.Layout(Constraints(
            scrollOffset: 100d,
            remainingPaintExtent: 40d,
            cacheOrigin: -20d,
            remainingCacheExtent: 100d));

        Assert.Equal(20_000d, result.Geometry.ScrollExtent);
        Assert.Equal(5, result.Slots.Count);
        Assert.Equal(4, result.Slots[0].Index);
        Assert.Equal(-20d, result.Slots[0].MainAxisOffset);
        Assert.Contains(result.Slots, slot => slot.Index == 4 && slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.Index == 5 && !slot.IsCacheOnly);
        Assert.DoesNotContain(result.Slots, slot => slot.Index > 8);
    }

    [Fact]
    public void VariableExtentListUsesObservedExtentsForOffsets()
    {
        var cache = new SliverChildExtentCache(itemCount: 4, defaultExtent: 25d, spacing: 1d);
        cache.Observe(0, 10d);
        var layout = new SliverVariableExtentListLayout(cache);

        var result = layout.Layout(Constraints(scrollOffset: 18d, remainingPaintExtent: 40d, remainingCacheExtent: 60d));

        Assert.Equal(43d, result.Geometry.ScrollExtent);
        Assert.Equal(1, result.Slots[0].Index);
        Assert.Equal(-7d, result.Slots[0].MainAxisOffset);
        Assert.Equal(10d, result.Slots[0].MainAxisExtent);
    }

    [Fact]
    public void SliverToBoxAdapterReportsSingleFixedBoxSlot()
    {
        var layout = new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(120d, 80d));

        var result = layout.Layout(Constraints(scrollOffset: 100d, remainingPaintExtent: 50d, remainingCacheExtent: 150d));

        Assert.Equal(120d, result.Geometry.ScrollExtent);
        Assert.Equal(20d, result.Geometry.PaintExtent);
        Assert.Single(result.Slots);
        Assert.Equal(-100d, result.Slots[0].MainAxisOffset);
        Assert.Equal(120d, result.Slots[0].MainAxisExtent);
        Assert.Equal(80d, result.Slots[0].CrossAxisExtent);
    }

    [Fact]
    public void SliverToBoxAdapterDoesNotRealizeWhenCacheOnlyTouchesBoundary()
    {
        var layout = new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(120d, 80d));

        var result = layout.Layout(Constraints(
            scrollOffset: 160d,
            remainingPaintExtent: 50d,
            cacheOrigin: -40d,
            remainingCacheExtent: 80d));

        Assert.Equal(120d, result.Geometry.ScrollExtent);
        Assert.Empty(result.Slots);
    }

    [Fact]
    public void PaddingOffsetsChildSlotsAndExtendsScrollGeometry()
    {
        var child = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(2, 20d));
        var layout = new SliverPaddingLayout(new SliverEdgeInsets(Before: 10d, After: 5d, CrossBefore: 3d), child);

        var result = layout.Layout(Constraints(remainingPaintExtent: 100d, crossAxisExtent: 80d, remainingCacheExtent: 100d));

        Assert.Equal(55d, result.Geometry.ScrollExtent);
        Assert.Equal(10d, result.Slots[0].MainAxisOffset);
        Assert.Equal(3d, result.Slots[0].CrossAxisOffset);
        Assert.Equal(77d, result.Slots[0].CrossAxisExtent);
    }

    [Fact]
    public void PaddingTranslatesCacheOriginIntoChildCoordinates()
    {
        var child = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(20, 10d));
        var layout = new SliverPaddingLayout(new SliverEdgeInsets(Before: 10d, After: 5d), child);

        var result = layout.Layout(Constraints(
            scrollOffset: 20d,
            remainingPaintExtent: 40d,
            cacheOrigin: -10d,
            remainingCacheExtent: 100d));

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, result.Slots.Select(slot => slot.Index));
        Assert.DoesNotContain(result.Slots, slot => slot.Index == 0);
        Assert.False(result.Slots.Single(slot => slot.Index == 1).IsCacheOnly);
        Assert.True(result.Slots.Single(slot => slot.Index == 10).IsCacheOnly);
    }

    [Fact]
    public void PaddingPreservesPinnedChildScrollObstruction()
    {
        var child = new SliverPersistentHeaderLayout(
            new SliverPersistentHeaderOptions(MinExtent: 40d, MaxExtent: 120d, Pinned: true));
        var layout = new SliverPaddingLayout(new SliverEdgeInsets(Before: 10d, After: 5d), child);

        var result = layout.Layout(Constraints(scrollOffset: 100d, remainingPaintExtent: 120d));

        Assert.Equal(135d, result.Geometry.ScrollExtent);
        Assert.Equal(40d, result.Geometry.MaxScrollObstructionExtent);
        Assert.Equal(0d, result.Slots[0].MainAxisOffset);
        Assert.True(result.Slots[0].IsPinned);
    }

    [Fact]
    public void PaddingPropagatesChildScrollOffsetCorrection()
    {
        var layout = new SliverPaddingLayout(
            new SliverEdgeInsets(Before: 10d, After: 5d),
            new CorrectingSliver(12d));

        var result = layout.Layout(Constraints(scrollOffset: 40d, remainingPaintExtent: 100d));

        Assert.Equal(12d, result.Geometry.ScrollOffsetCorrection);
        Assert.Empty(result.Slots);
    }

    [Fact]
    public void VisibilityCanRemoveMaintainOrUseReplacement()
    {
        var child = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(2, 20d));
        var replacement = new SliverToBoxAdapterLayout(new SliverToBoxAdapterOptions(12d));

        var removed = new SliverVisibilityLayout(false, child).Layout(Constraints());
        var maintained = new SliverVisibilityLayout(false, child, maintainSize: true).Layout(Constraints());
        var replaced = new SliverVisibilityLayout(false, child, replacement).Layout(Constraints());

        Assert.Equal(0d, removed.Geometry.ScrollExtent);
        Assert.Empty(removed.Slots);
        Assert.Equal(40d, maintained.Geometry.ScrollExtent);
        Assert.Empty(maintained.Slots);
        Assert.Equal(12d, replaced.Geometry.ScrollExtent);
        Assert.Single(replaced.Slots);
    }

    [Fact]
    public void FloatingHeaderRevealsFromScrollDeltaAndSnapsDeterministically()
    {
        var state = new SliverPersistentHeaderState();
        var layout = new SliverAdvancedPersistentHeaderLayout(
            new SliverAdvancedPersistentHeaderOptions(
                MinExtent: 40d,
                MaxExtent: 120d,
                Floating: true,
                Snap: true),
            state);

        var collapsed = layout.Layout(Constraints(
            scrollOffset: 100d,
            remainingPaintExtent: 120d,
            userScrollDirection: SliverUserScrollDirection.Reverse));

        Assert.Equal(40d, collapsed.Slots[0].MainAxisExtent);

        var floating = layout.Layout(Constraints(
            scrollOffset: 20d,
            remainingPaintExtent: 120d,
            userScrollDirection: SliverUserScrollDirection.Forward));

        Assert.Equal(120d, floating.Slots[0].MainAxisExtent);

        var snapped = layout.Layout(Constraints(
            scrollOffset: 20d,
            remainingPaintExtent: 120d,
            userScrollDirection: SliverUserScrollDirection.Idle));

        Assert.Equal(120d, state.CurrentExtent);
        Assert.Equal(SliverHeaderSnapStatus.SnappingToMax, state.SnapStatus);
        Assert.Equal(120d, snapped.Slots[0].MainAxisExtent);
        Assert.Equal(0d, snapped.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void AdvancedPinnedHeaderTransitionsUntilCollapsedBeforePinning()
    {
        var layout = new SliverAdvancedPersistentHeaderLayout(
            new SliverAdvancedPersistentHeaderOptions(40d, 120d, Pinned: true));

        var partiallyCollapsed = layout.Layout(Constraints(scrollOffset: 32d, remainingPaintExtent: 120d));
        var pinned = layout.Layout(Constraints(scrollOffset: 90d, remainingPaintExtent: 120d));

        Assert.Equal(88d, partiallyCollapsed.Slots[0].MainAxisExtent);
        Assert.Equal(0d, partiallyCollapsed.Slots[0].MainAxisOffset);
        Assert.Equal(40d, pinned.Slots[0].MainAxisExtent);
        Assert.Equal(0d, pinned.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void StepSnapServiceAdvancesTowardTargetWithoutFrameworkTime()
    {
        var state = new SliverPersistentHeaderState();
        var layout = new SliverAdvancedPersistentHeaderLayout(
            new SliverAdvancedPersistentHeaderOptions(40d, 120d, Floating: true, Snap: true),
            state,
            new SliverStepHeaderSnapAnimationService(10d));

        layout.Layout(Constraints(scrollOffset: 60d, userScrollDirection: SliverUserScrollDirection.Reverse));
        layout.Layout(Constraints(scrollOffset: 20d, userScrollDirection: SliverUserScrollDirection.Forward));

        var snapped = layout.Layout(Constraints(scrollOffset: 20d));

        Assert.Equal(110d, snapped.Slots[0].MainAxisExtent);
        Assert.Equal(120d, state.SnapTargetExtent);
        Assert.Equal(SliverHeaderSnapStatus.SnappingToMax, state.SnapStatus);
    }

    private sealed class CorrectingSliver : ISliverLayout
    {
        private readonly double _correction;

        public CorrectingSliver(double correction)
        {
            _correction = correction;
        }

        public SliverLayoutResult Layout(in SliverConstraints constraints)
        {
            return new SliverLayoutResult(
                new SliverGeometry
                {
                    ScrollOffsetCorrection = _correction,
                    CrossAxisExtent = constraints.CrossAxisExtent
                },
                Array.Empty<SliverLayoutSlot>());
        }
    }
}
