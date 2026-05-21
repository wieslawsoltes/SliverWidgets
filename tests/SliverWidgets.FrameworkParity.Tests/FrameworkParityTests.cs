using SliverWidgets.Core;
using SliverWidgets.GalleryData;

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

    [Theory]
    [InlineData(SliverAxis.Vertical)]
    [InlineData(SliverAxis.Horizontal)]
    public void WrapVirtualizedAdaptersShareCoreRealizationWindow(SliverAxis axis)
    {
        var layout = new SliverWrapLayout(new SliverWrapLayoutOptions(
            new SliverDeterministicWrapExtentList(100_000),
            MainAxisSpacing: 8d,
            CrossAxisSpacing: 8d));
        var constraints = new SliverConstraints(
            axis,
            ScrollOffset: 2_400d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 360d,
            CrossAxisExtent: 840d,
            ViewportMainAxisExtent: 360d,
            CacheOrigin: -180d,
            RemainingCacheExtent: 720d);

        var result = layout.Layout(constraints);

        Assert.True(result.Slots.Count < 100);
        Assert.Contains(result.Slots, slot => !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.IsCacheOnly);
        Assert.All(result.Slots, slot =>
        {
            Assert.True(slot.MainAxisExtent > 0d);
            Assert.True(slot.CrossAxisExtent > 0d);
            Assert.True(slot.CrossAxisExtent <= constraints.CrossAxisExtent);
        });
    }

    [Theory]
    [InlineData(SliverAxis.Vertical)]
    [InlineData(SliverAxis.Horizontal)]
    public void StackVirtualizedAdaptersShareCoreRealizationWindow(SliverAxis axis)
    {
        var layout = new SliverStackLayout(new SliverStackLayoutOptions(
            new SliverDeterministicStackExtentList(100_000),
            Spacing: 4d,
            CrossAxisAlignment: SliverCrossAxisAlignment.Center));
        var constraints = new SliverConstraints(
            axis,
            ScrollOffset: 3_600d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 360d,
            CrossAxisExtent: 840d,
            ViewportMainAxisExtent: 360d,
            CacheOrigin: -180d,
            RemainingCacheExtent: 720d);

        var result = layout.Layout(constraints);

        Assert.True(result.Slots.Count < 40);
        Assert.Contains(result.Slots, slot => !slot.IsCacheOnly);
        Assert.Contains(result.Slots, slot => slot.IsCacheOnly);
        Assert.All(result.Slots, slot =>
        {
            Assert.True(slot.MainAxisExtent > 0d);
            Assert.True(slot.CrossAxisExtent > 0d);
            Assert.True(slot.CrossAxisExtent <= constraints.CrossAxisExtent);
        });
    }

    [Fact]
    public void DataGridAdaptersShareCoreRowAndColumnRealizationWindow()
    {
        var layout = new SliverDataGridLayout(new SliverDataGridLayoutOptions(
            new SliverDeterministicDataGridRowExtentList(100_000),
            Enumerable.Range(0, 32)
                .Select(index => new SliverDataGridColumnDefinition(
                    $"c{index}",
                    $"Column {index}",
                    index % 3 == 0 ? SliverDataGridColumnWidthMode.SizeToCells : SliverDataGridColumnWidthMode.Fixed,
                    Width: 112d,
                    CellWidth: 128d + (index % 4 * 18d)))
                .ToArray(),
            HeaderExtent: 44d,
            RowSpacing: 2d,
            ColumnSpacing: 4d,
            HorizontalScrollOffset: 900d,
            HorizontalCacheOrigin: -240d,
            RemainingHorizontalCacheExtent: 960d,
            FrozenColumnCount: 1));
        var constraints = new SliverConstraints(
            SliverAxis.Vertical,
            ScrollOffset: 7_200d,
            PrecedingScrollExtent: 0d,
            Overlap: 0d,
            RemainingPaintExtent: 360d,
            CrossAxisExtent: 760d,
            ViewportMainAxisExtent: 360d,
            CacheOrigin: -180d,
            RemainingCacheExtent: 720d);

        var result = layout.LayoutDataGrid(constraints);

        Assert.True(result.Rows.Count < 40);
        Assert.True(result.Columns.Count < 16);
        Assert.True(result.Cells.Count < 600);
        Assert.Contains(result.Rows, row => !row.IsCacheOnly);
        Assert.Contains(result.Rows, row => row.IsCacheOnly);
        Assert.Contains(result.Columns, column => column.IsFrozen);
        Assert.Contains(result.Columns, column => column.IsCacheOnly);
        Assert.Contains(result.Cells, cell => cell.IsHeader);
    }

    [Fact]
    public void SharedDataGridColumnMetadataMatchesGalleryTableWidth()
    {
        var columns = SliverGalleryData.CreateDataGridColumns();
        var contentWidth = columns.Sum(column => column.EffectiveWidth) +
            ((columns.Count - 1) * SliverGalleryData.DataGridColumnSpacing);

        Assert.Equal(SliverGalleryData.DataGridContentWidth, contentWidth);
        Assert.Equal(SliverGalleryData.DataGridContentWidth + SliverGalleryData.DataGridHorizontalPadding, SliverGalleryData.DataGridTableWidth);
        Assert.Contains(columns, column => column.WidthMode == "Auto");
        Assert.Contains(columns, column => column.WidthMode == "Star");
        Assert.Contains(columns, column => column.WidthMode == "Fill");
        Assert.Contains(columns, column => column.WidthMode == "LastColumnFill");
        Assert.All(columns, column =>
        {
            Assert.True(column.EffectiveWidth >= column.MinWidth);
            Assert.True(column.EffectiveWidth <= column.MaxWidth);
        });
    }
}
