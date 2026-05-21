using SliverWidgets.Core;

namespace SliverWidgets.Core.Tests;

public sealed class SliverLayoutTests
{
    private static SliverConstraints Constraints(
        double scrollOffset = 0d,
        double remainingPaintExtent = 300d,
        double crossAxisExtent = 100d,
        double remainingCacheExtent = 300d,
        double precedingScrollExtent = 0d)
    {
        return new SliverConstraints(
            SliverAxis.Vertical,
            scrollOffset,
            precedingScrollExtent,
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
    public void GridWithMaxCrossAxisExtentUsesCeilingColumnCount()
    {
        var layout = new SliverGridLayout(
            SliverGridLayoutOptions.WithMaxCrossAxisExtent(itemCount: 4, maxCrossAxisExtent: 200d));

        var result = layout.Layout(Constraints(
            remainingPaintExtent: 400d,
            crossAxisExtent: 300d,
            remainingCacheExtent: 400d));

        Assert.Equal(2, layout.ResolveCrossAxisCount(300d));
        Assert.Equal(300d, result.Geometry.ScrollExtent);
        Assert.Equal(150d, result.Slots[0].CrossAxisExtent);
        Assert.Equal(150d, result.Slots[1].CrossAxisOffset);
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
        Assert.Equal(30d, result.Geometry.LayoutExtent);
        Assert.Equal(40d, result.Geometry.MaxScrollObstructionExtent);
        Assert.Equal(40d, result.Slots[0].MainAxisExtent);
        Assert.True(result.Slots[0].IsPinned);
        Assert.Equal(0d, result.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void ScrollingHeaderShrinksBeforeScrollingOffWithoutGap()
    {
        var layout = new SliverPersistentHeaderLayout(
            new SliverPersistentHeaderOptions(MinExtent: 56d, MaxExtent: 160d));

        var shrinking = layout.Layout(Constraints(scrollOffset: 50d, remainingPaintExtent: 300d));
        var scrollingOff = layout.Layout(Constraints(scrollOffset: 130d, remainingPaintExtent: 300d));

        Assert.Equal(110d, shrinking.Geometry.PaintExtent);
        Assert.Equal(110d, shrinking.Geometry.LayoutExtent);
        Assert.Equal(110d, shrinking.Slots[0].MainAxisExtent);
        Assert.Equal(0d, shrinking.Slots[0].MainAxisOffset);

        Assert.Equal(30d, scrollingOff.Geometry.PaintExtent);
        Assert.Equal(30d, scrollingOff.Geometry.LayoutExtent);
        Assert.Equal(56d, scrollingOff.Slots[0].MainAxisExtent);
        Assert.Equal(-26d, scrollingOff.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void ViewportComposesScrollingHeaderAndRowsWithoutReservedGap()
    {
        var engine = new SliverViewportLayoutEngine();
        var slivers = new ISliverLayout[]
        {
            new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(56d, 160d)),
            new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(10, 54d, 6d))
        };

        var result = engine.Layout(slivers, new SliverViewport(300d, 500d), scrollOffset: 130d);

        var header = result.Slots.Single(slot => slot.SliverIndex == 0);
        var firstRow = result.Slots.Single(slot => slot.SliverIndex == 1 && slot.ItemIndex == 0);

        Assert.Equal(-26d, header.MainAxisOffset);
        Assert.Equal(56d, header.MainAxisExtent);
        Assert.Equal(30d, firstRow.MainAxisOffset);
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
    public void ViewportPassesPinnedOverlapWithoutPushingNormalSlivers()
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
        Assert.Equal(200d, recordingSliver.LastConstraints.RemainingPaintExtent);
        Assert.Equal(0d, followingSlot.MainAxisOffset);
    }

    [Fact]
    public void ViewportConsumesCacheExtentAcrossSlivers()
    {
        var engine = new SliverViewportLayoutEngine();
        var slivers = new ISliverLayout[]
        {
            new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(100, 20d)),
            new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(100, 20d))
        };

        var result = engine.Layout(slivers, new SliverViewport(100d, 300d), scrollOffset: 0d);

        Assert.Contains(result.Slots, slot => slot.SliverIndex == 0);
        Assert.DoesNotContain(result.Slots, slot => slot.SliverIndex == 1);
    }

    [Fact]
    public void FillRemainingWithScrollBodyReportsViewportScrollExtent()
    {
        var layout = new SliverFillRemainingLayout(new SliverFillRemainingOptions(800d));

        var result = layout.Layout(Constraints(
            remainingPaintExtent: 500d,
            remainingCacheExtent: 500d,
            precedingScrollExtent: 600d));

        Assert.Equal(500d, result.Geometry.ScrollExtent);
    }

    [Fact]
    public void FillRemainingWithoutScrollBodyUsesChildWhenPrecedingExceedsViewport()
    {
        var layout = new SliverFillRemainingLayout(new SliverFillRemainingOptions(220d, HasScrollBody: false));

        var result = layout.Layout(Constraints(
            remainingPaintExtent: 500d,
            remainingCacheExtent: 500d,
            precedingScrollExtent: 600d));

        Assert.Equal(220d, result.Geometry.ScrollExtent);
    }

    [Fact]
    public void ViewportAppliesScrollOffsetCorrectionsAndRelayouts()
    {
        var engine = new SliverViewportLayoutEngine();
        var correctingSliver = new CorrectingSliver(25d);

        var result = engine.Layout(
            new ISliverLayout[] { correctingSliver },
            new SliverViewport(200d, 300d),
            scrollOffset: 10d);

        Assert.Equal(new[] { 10d, 35d }, correctingSliver.ObservedScrollOffsets);
        Assert.Single(result.Slots);
        Assert.Equal(-35d, result.Slots[0].MainAxisOffset);
    }

    [Fact]
    public void ViewportRejectsNonConvergingScrollOffsetCorrections()
    {
        var engine = new SliverViewportLayoutEngine();

        Assert.Throws<InvalidOperationException>(() => engine.Layout(
            new ISliverLayout[] { new NonConvergingCorrectionSliver() },
            new SliverViewport(200d, 300d),
            scrollOffset: 10d));
    }

    [Fact]
    public void NonNegativeGuardRejectsInfiniteValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SliverMath.ThrowIfNegative(double.PositiveInfinity, "value"));
        Assert.Throws<ArgumentOutOfRangeException>(() => SliverMath.ThrowIfNegative(double.NegativeInfinity, "value"));
    }

    [Fact]
    public void ConstraintsRejectPositiveCacheOrigin()
    {
        var constraints = Constraints() with { CacheOrigin = 1d };

        Assert.Throws<ArgumentOutOfRangeException>(() => constraints.Validate());
    }

    [Fact]
    public void GeometryRejectsPaintExtentBeyondMaxPaintExtent()
    {
        var geometry = new SliverGeometry
        {
            ScrollExtent = 100d,
            PaintExtent = 40d,
            LayoutExtent = 40d,
            MaxPaintExtent = 20d,
            HitTestExtent = 40d,
            CacheExtent = 40d,
            CrossAxisExtent = 100d
        };

        Assert.Throws<InvalidOperationException>(() => geometry.Validate(Constraints()));
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

    private sealed class CorrectingSliver : ISliverLayout
    {
        private readonly double _correction;
        private bool _corrected;
        private readonly List<double> _observedScrollOffsets = new();

        public CorrectingSliver(double correction)
        {
            _correction = correction;
        }

        public IReadOnlyList<double> ObservedScrollOffsets => _observedScrollOffsets;

        public SliverLayoutResult Layout(in SliverConstraints constraints)
        {
            _observedScrollOffsets.Add(constraints.ScrollOffset);
            if (!_corrected)
            {
                _corrected = true;
                return new SliverLayoutResult(
                    new SliverGeometry
                    {
                        ScrollExtent = 100d,
                        PaintExtent = 0d,
                        LayoutExtent = 0d,
                        MaxPaintExtent = 100d,
                        HitTestExtent = 0d,
                        ScrollOffsetCorrection = _correction,
                        CrossAxisExtent = constraints.CrossAxisExtent
                    },
                    Array.Empty<SliverLayoutSlot>());
            }

            var paintExtent = Math.Min(25d, constraints.RemainingPaintExtent);
            return new SliverLayoutResult(
                new SliverGeometry
                {
                    ScrollExtent = 100d,
                    PaintExtent = paintExtent,
                    LayoutExtent = paintExtent,
                    MaxPaintExtent = 100d,
                    HitTestExtent = paintExtent,
                    Visible = true,
                    CrossAxisExtent = constraints.CrossAxisExtent
                },
                new[] { new SliverLayoutSlot(0, -constraints.ScrollOffset, 0d, 25d, constraints.CrossAxisExtent) });
        }
    }

    private sealed class NonConvergingCorrectionSliver : ISliverLayout
    {
        public SliverLayoutResult Layout(in SliverConstraints constraints)
        {
            return new SliverLayoutResult(
                new SliverGeometry
                {
                    ScrollExtent = 100d,
                    PaintExtent = 0d,
                    LayoutExtent = 0d,
                    MaxPaintExtent = 100d,
                    HitTestExtent = 0d,
                    ScrollOffsetCorrection = 1d,
                    CrossAxisExtent = constraints.CrossAxisExtent
                },
                Array.Empty<SliverLayoutSlot>());
        }
    }
}
