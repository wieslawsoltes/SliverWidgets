using SliverWidgets.Core;

namespace SliverWidgets.Core.Tests;

public sealed class SliverLayoutTests
{
    private static SliverConstraints Constraints(
        double scrollOffset = 0d,
        double remainingPaintExtent = 300d,
        double crossAxisExtent = 100d,
        double remainingCacheExtent = 300d)
    {
        return new SliverConstraints(
            SliverAxis.Vertical,
            scrollOffset,
            0d,
            0d,
            remainingPaintExtent,
            crossAxisExtent,
            remainingPaintExtent,
            0d,
            remainingCacheExtent);
    }

    [Fact]
    public void FixedExtentListUsesArithmeticScrollExtentAndVisibleSlots()
    {
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(10, 20d, 2d));

        var result = layout.Layout(Constraints(scrollOffset: 44d, remainingPaintExtent: 50d));

        Assert.Equal(218d, result.Geometry.ScrollExtent);
        Assert.Equal(2, result.Slots[0].Index);
        Assert.Equal(0d, result.Slots[0].MainAxisOffset);
        Assert.Contains(result.Slots, slot => slot.Index == 4 && !slot.IsCacheOnly);
    }

    [Fact]
    public void FixedExtentListHonorsNegativeCacheOriginAndPaintBoundaries()
    {
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(100, 32d, 4d));
        var constraints = new SliverConstraints(
            SliverAxis.Vertical,
            ScrollOffset: 360d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 240d,
            CrossAxisExtent: 800d,
            ViewportMainAxisExtent: 240d,
            CacheOrigin: -120d,
            RemainingCacheExtent: 480d);

        var result = layout.Layout(constraints);

        Assert.Equal(6, result.Slots[0].Index);
        Assert.Equal(19, result.Slots[^1].Index);
        Assert.DoesNotContain(result.Slots, slot => slot.Index == 20);
        Assert.Contains(result.Slots, slot => slot.Index == 9 && slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.Index == 10 && !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.Index == 16 && !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.Index == 17 && slot.IsCacheOnly);
    }

    [Fact]
    public void FixedExtentListMarksItemsTouchingPaintEndAsCacheOnly()
    {
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(5, 20d));

        var result = layout.Layout(Constraints(remainingPaintExtent: 40d, remainingCacheExtent: 80d));

        Assert.Equal(new[] { 0, 1, 2, 3 }, result.Slots.Select(slot => slot.Index));
        Assert.False(result.Slots.Single(slot => slot.Index == 1).IsCacheOnly);
        Assert.True(result.Slots.Single(slot => slot.Index == 2).IsCacheOnly);
        Assert.DoesNotContain(result.Slots, slot => slot.Index == 4);
    }

    [Fact]
    public void VariableListDeadReckonsFromMeasuredExtents()
    {
        var layout = new SliverListLayout(new SliverListOptions(new[] { 10d, 30d, 20d }, 5d));

        var result = layout.Layout(Constraints(scrollOffset: 12d, remainingPaintExtent: 40d));

        Assert.Equal(70d, result.Geometry.ScrollExtent);
        Assert.Equal(1, result.Slots[0].Index);
        Assert.Equal(3d, result.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void VariableListReportsFullScrollExtentWhenRealizationStopsEarly()
    {
        var layout = new SliverListLayout(new SliverListOptions(
            Enumerable.Repeat(20d, 1_000).ToArray(),
            2d));

        var result = layout.Layout(Constraints(scrollOffset: 220d, remainingPaintExtent: 40d, remainingCacheExtent: 40d));

        Assert.Equal(21_998d, result.Geometry.ScrollExtent);
        Assert.True(result.Slots.Count < 10);
    }

    [Fact]
    public void GridComputesRowsColumnsAndTileOffsets()
    {
        var layout = new SliverGridLayout(
            SliverGridLayoutOptions.FixedCrossAxisCount(
                itemCount: 7,
                crossAxisCount: 3,
                mainAxisSpacing: 4d,
                crossAxisSpacing: 2d,
                childAspectRatio: 2d));

        var result = layout.Layout(Constraints(remainingPaintExtent: 200d, crossAxisExtent: 304d));

        Assert.Equal(3, layout.ResolveCrossAxisCount(304d));
        Assert.Equal(158d, result.Geometry.ScrollExtent);
        Assert.Equal(7, result.Slots.Count);
        Assert.Equal(102d, result.Slots[1].CrossAxisOffset);
        Assert.Equal(54d, result.Slots[3].MainAxisOffset);
    }

    [Fact]
    public void GridWithZeroCrossAxisExtentDoesNotCreateInvalidSlots()
    {
        var layout = new SliverGridLayout(
            SliverGridLayoutOptions.WithMaxCrossAxisExtent(itemCount: 10, maxCrossAxisExtent: 200d));

        var result = layout.Layout(Constraints(remainingPaintExtent: 200d, crossAxisExtent: 0d));

        Assert.Equal(0d, result.Geometry.ScrollExtent);
        Assert.Empty(result.Slots);
    }

    [Fact]
    public void GridHonorsNegativeCacheOrigin()
    {
        var layout = new SliverGridLayout(
            SliverGridLayoutOptions.FixedCrossAxisCount(
                itemCount: 100,
                crossAxisCount: 2,
                mainAxisSpacing: 4d,
                crossAxisSpacing: 0d,
                childAspectRatio: 1d,
                mainAxisExtent: 40d));
        var constraints = new SliverConstraints(
            SliverAxis.Vertical,
            ScrollOffset: 120d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 80d,
            CrossAxisExtent: 200d,
            ViewportMainAxisExtent: 80d,
            CacheOrigin: -40d,
            RemainingCacheExtent: 160d);

        var result = layout.Layout(constraints);

        Assert.Equal(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, result.Slots.Select(slot => slot.Index));
        Assert.All(result.Slots.Where(slot => slot.Index is 2 or 3), slot => Assert.True(slot.IsCacheOnly));
        Assert.All(result.Slots.Where(slot => slot.Index is 4 or 5 or 6 or 7 or 8 or 9), slot => Assert.False(slot.IsCacheOnly));
        Assert.All(result.Slots.Where(slot => slot.Index is 10 or 11), slot => Assert.True(slot.IsCacheOnly));
    }

    [Fact]
    public void PinnedHeaderShrinksAndReportsObstruction()
    {
        var layout = new SliverPersistentHeaderLayout(
            new SliverPersistentHeaderOptions(MinExtent: 40d, MaxExtent: 120d, Pinned: true));

        var result = layout.Layout(Constraints(scrollOffset: 90d, remainingPaintExtent: 100d));

        Assert.Equal(120d, result.Geometry.ScrollExtent);
        Assert.Equal(40d, result.Geometry.MaxScrollObstructionExtent);
        Assert.Equal(40d, result.Slots[0].MainAxisExtent);
        Assert.True(result.Slots[0].IsPinned);
        Assert.Equal(0d, result.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void ViewportComposesMultipleSliversIntoOneScrollSurface()
    {
        var engine = new SliverViewportLayoutEngine();
        var slivers = new ISliverLayout[]
        {
            new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(40d, 100d, Pinned: true)),
            new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(20, 25d))
        };

        var result = engine.Layout(slivers, new SliverViewport(200d, 300d), scrollOffset: 120d);

        Assert.Equal(600d, result.ScrollExtent);
        Assert.Equal(400d, result.MaxScrollOffset);
        Assert.Contains(result.Slots, slot => slot.SliverIndex == 0 && slot.IsPinned);
        Assert.Contains(result.Slots, slot => slot.SliverIndex == 1 && slot.ItemIndex == 0);
    }

    [Fact]
    public void ViewportOffsetsLaterSliversBelowPinnedObstructions()
    {
        var engine = new SliverViewportLayoutEngine();
        var recordingSliver = new RecordingSliver();
        var slivers = new ISliverLayout[]
        {
            new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(40d, 100d, Pinned: true)),
            new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(30d, 80d, Pinned: true)),
            recordingSliver
        };

        var result = engine.Layout(slivers, new SliverViewport(200d, 300d), scrollOffset: 190d);

        var firstHeader = result.Slots.Single(slot => slot.SliverIndex == 0);
        var secondHeader = result.Slots.Single(slot => slot.SliverIndex == 1);
        var followingSlot = result.Slots.Single(slot => slot.SliverIndex == 2);

        Assert.Equal(0d, firstHeader.MainAxisOffset);
        Assert.Equal(40d, secondHeader.MainAxisOffset);
        Assert.Equal(70d, recordingSliver.LastConstraints.Overlap);
        Assert.Equal(130d, recordingSliver.LastConstraints.RemainingPaintExtent);
        Assert.Equal(70d, followingSlot.MainAxisOffset);
    }

    private sealed class RecordingSliver : ISliverLayout
    {
        public SliverConstraints LastConstraints { get; private set; }

        public SliverLayoutResult Layout(in SliverConstraints constraints)
        {
            LastConstraints = constraints;
            var paintExtent = Math.Min(25d, constraints.RemainingPaintExtent);
            return new SliverLayoutResult(
                new SliverGeometry
                {
                    ScrollExtent = 100d,
                    PaintExtent = paintExtent,
                    LayoutExtent = paintExtent,
                    MaxPaintExtent = 100d,
                    HitTestExtent = paintExtent,
                    Visible = paintExtent > SliverMath.Epsilon,
                    CrossAxisExtent = constraints.CrossAxisExtent
                },
                paintExtent > SliverMath.Epsilon
                    ? new[] { new SliverLayoutSlot(0, 0d, 0d, 25d, constraints.CrossAxisExtent) }
                    : Array.Empty<SliverLayoutSlot>());
        }
    }
}
