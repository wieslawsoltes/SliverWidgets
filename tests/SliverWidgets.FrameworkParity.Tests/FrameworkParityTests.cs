using SliverWidgets.Core;

namespace SliverWidgets.FrameworkParity.Tests;

public sealed class FrameworkParityTests
{
    [Theory]
    [InlineData(SliverAxis.Vertical)]
    [InlineData(SliverAxis.Horizontal)]
    public void FixedExtentVirtualizedAdaptersShareCoreRealizationWindow(SliverAxis axis)
    {
        var layout = new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(1_000, 32d, 4d));
        var constraints = new SliverConstraints(
            axis,
            ScrollOffset: 360d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 240d,
            CrossAxisExtent: 800d,
            ViewportMainAxisExtent: 240d,
            CacheOrigin: -120d,
            RemainingCacheExtent: 480d);

        var result = layout.Layout(constraints);

        Assert.True(result.Slots.Count < 30);
        Assert.Contains(result.Slots, slot => !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.IsCacheOnly);
        Assert.Equal(35_996d, result.Geometry.ScrollExtent);
    }

    [Fact]
    public void MixedSliverViewportProducesDeterministicScrollExtent()
    {
        var engine = new SliverViewportLayoutEngine();
        var slivers = new ISliverLayout[]
        {
            new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(48d, 144d, Pinned: true)),
            new SliverGridLayout(SliverGridLayoutOptions.FixedCrossAxisCount(42, 3, 8d, 8d, 1.25d)),
            new SliverFillRemainingLayout(new SliverFillRemainingOptions(360d, HasScrollBody: false))
        };

        var result = engine.Layout(slivers, new SliverViewport(720d, 960d), scrollOffset: 200d, cacheExtent: 240d);

        Assert.True(result.ScrollExtent > 720d);
        Assert.True(result.MaxScrollOffset > 0d);
        Assert.Contains(result.Slots, slot => slot.SliverIndex == 0 && slot.IsPinned);
        Assert.Contains(result.Slots, slot => slot.SliverIndex == 1);
    }

    [Theory]
    [InlineData(SliverAxis.Vertical)]
    [InlineData(SliverAxis.Horizontal)]
    public void GridVirtualizedAdaptersShareCoreRealizationWindow(SliverAxis axis)
    {
        var layout = new SliverGridLayout(
            SliverGridLayoutOptions.FixedCrossAxisCount(
                itemCount: 500,
                crossAxisCount: 4,
                mainAxisSpacing: 8d,
                crossAxisSpacing: 4d,
                childAspectRatio: 1.5d));
        var constraints = new SliverConstraints(
            axis,
            ScrollOffset: 480d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 320d,
            CrossAxisExtent: 808d,
            ViewportMainAxisExtent: 320d,
            CacheOrigin: -160d,
            RemainingCacheExtent: 640d);

        var result = layout.Layout(constraints);

        Assert.True(result.Slots.Count < 80);
        Assert.Contains(result.Slots, slot => !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.IsCacheOnly);
        Assert.All(result.Slots, slot => Assert.True(slot.CrossAxisExtent > 0d));
    }
}
