using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia;
using AvaloniaGallery;
using SliverWidgets.GalleryData;
using AvaloniaSlivers = SliverWidgets.Avalonia;

namespace SliverWidgets.Avalonia.Tests;

public sealed class AvaloniaGallerySmokeTests
{
    [Fact]
    public void Gallery_view_model_projects_expected_scenarios()
    {
        var viewModel = new GalleryViewModel();
        var scenarioTitles = viewModel.ScenarioCatalog.Select(scenario => scenario.Title).ToArray();
        var expectedTitles = SliverGalleryData.CreateScenarios().Select(scenario => scenario.Title).ToArray();

        Assert.Equal(expectedTitles, scenarioTitles);
        Assert.NotEmpty(viewModel.SectionedBlocks);
        Assert.NotEmpty(viewModel.CompositionBlocks);
    }

    [AvaloniaFact]
    public void Gallery_tabs_render_scrollable_viewports()
    {
        var window = new MainWindow
        {
            Width = 1180,
            Height = 780,
            DataContext = new GalleryViewModel()
        };

        window.Show();

        var tabs = window.FindControl<TabControl>("GalleryTabs")
                   ?? throw new InvalidOperationException("Gallery tab control was not found.");

        Assert.Equal(8, tabs.ItemCount);

        for (var index = 0; index < tabs.ItemCount; index++)
        {
            tabs.SelectedIndex = index;
            window.UpdateLayout();

            var scrollViewer = window
                .GetVisualDescendants()
                .OfType<ScrollViewer>()
                .Where(viewer => viewer.IsEffectivelyVisible)
                .MaxBy(viewer => viewer.Extent.Height);

            Assert.NotNull(scrollViewer);
            Assert.Equal(ScrollBarVisibility.Visible, scrollViewer!.VerticalScrollBarVisibility);
            Assert.True(scrollViewer!.Extent.Height > scrollViewer.Viewport.Height, $"Tab {index} should expose a vertical scroll extent.");
            Assert.True(scrollViewer.Viewport.Height > 0, $"Tab {index} should have a measured viewport.");
            if (index is 0 or 1 or 7)
            {
                var visibleItemTextCount = window
                    .GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Count(text => text.IsEffectivelyVisible && text.Text?.StartsWith("Sliver item", StringComparison.Ordinal) == true);
                Assert.True(visibleItemTextCount >= 7, $"Tab {index} should realize enough rows to fill the initial viewport.");
            }

            SaveScreenshot(window, $"avalonia-gallery-tab-{index}");

            var scrollTarget = Math.Min(220d, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
            scrollViewer.Offset = new Vector(0d, scrollTarget);
            window.UpdateLayout();

            Assert.True(scrollViewer.Offset.Y > 0d, $"Tab {index} should accept native vertical scroll offset.");
            SaveScreenshot(window, $"avalonia-gallery-tab-{index}-scrolled");
        }
    }

    [AvaloniaFact]
    public void Gallery_sliver_items_controls_scroll_logically_without_blank_viewports()
    {
        var window = new MainWindow
        {
            Width = 1180,
            Height = 780,
            DataContext = new GalleryViewModel()
        };

        window.Show();

        var tabs = window.FindControl<TabControl>("GalleryTabs")
                   ?? throw new InvalidOperationException("Gallery tab control was not found.");

        foreach (var tabIndex in new[] { 0, 1, 2, 7 })
        {
            tabs.SelectedIndex = tabIndex;
            window.UpdateLayout();

            var scrollViewer = GetLargestVisibleScrollViewer(window);
            var logicalContent = Assert.IsAssignableFrom<ILogicalScrollable>(scrollViewer.Content);
            Assert.True(logicalContent.IsLogicalScrollEnabled, $"Tab {tabIndex} should expose logical scrolling to the ScrollViewer.");

            foreach (var offset in new[] { 0d, 220d, 900d, 1500d })
            {
                scrollViewer.Offset = new Vector(0d, offset);
                window.UpdateLayout();

                Assert.True(
                    Math.Abs(scrollViewer.Offset.Y - logicalContent.Offset.Y) <= 0.5d,
                    $"Tab {tabIndex} should forward ScrollViewer offset {scrollViewer.Offset.Y} to its sliver panel.");
                AssertSliverPanelOffset(window, tabIndex, scrollViewer.Offset.Y);

                var visibleTextCount = CountRenderedTextInViewport(
                    scrollViewer,
                    "Sliver item");
                Assert.True(visibleTextCount > 0, $"Tab {tabIndex} should render sliver content at offset {scrollViewer.Offset.Y}.");
            }
        }
    }

    [AvaloniaFact]
    public void Gallery_header_uses_small_scroll_steps_for_smooth_collapse()
    {
        var window = new MainWindow
        {
            Width = 1180,
            Height = 780,
            DataContext = new GalleryViewModel()
        };

        window.Show();

        var tabs = window.FindControl<TabControl>("GalleryTabs")
                   ?? throw new InvalidOperationException("Gallery tab control was not found.");
        tabs.SelectedIndex = 3;
        window.UpdateLayout();

        var scrollViewer = window.FindControl<ScrollViewer>("HeaderScrollViewer")
                           ?? throw new InvalidOperationException("Header ScrollViewer was not found.");
        var header = window
            .GetVisualDescendants()
            .OfType<AvaloniaSlivers.SliverPersistentHeader>()
            .Single(control => control.IsEffectivelyVisible);

        Assert.InRange(scrollViewer.SmallChange.Height, 1d, 16d);

        var previousHeight = header.Bounds.Height;
        for (var step = 0; step < 8; step++)
        {
            scrollViewer.LineDown();
            window.UpdateLayout();

            Assert.True(
                Math.Abs(header.ScrollOffset - scrollViewer.Offset.Y) <= 0.5d,
                "Header should track the ScrollViewer offset.");

            var currentHeight = header.Bounds.Height;
            Assert.InRange(previousHeight - currentHeight, 0d, scrollViewer.SmallChange.Height + 0.5d);
            previousHeight = currentHeight;
        }
    }

    [AvaloniaFact]
    public void Mixed_panel_clips_scrolled_content_below_pinned_header()
    {
        var panel = new MixedSliverPreviewPanel
        {
            CacheExtent = 280d,
            ScrollOffset = 450d
        };

        for (var index = 0; index < 19; index++)
        {
            panel.Children.Add(new Border
            {
                Child = new TextBlock { Text = index == 0 ? "Header" : $"Child {index}" }
            });
        }

        panel.Measure(new Size(760d, 520d));
        panel.Arrange(new Rect(0d, 0d, 760d, 520d));

        var header = panel.Children[0];
        var obstructionBottom = header.Bounds.Bottom;
        var overlappingChildren = panel.Children
            .OfType<Control>()
            .Skip(1)
            .Where(child =>
                child.Opacity > 0d &&
                child.Bounds.Y < obstructionBottom &&
                child.Bounds.Bottom > obstructionBottom)
            .ToArray();

        Assert.NotEmpty(overlappingChildren);
        foreach (var child in overlappingChildren)
        {
            var clip = Assert.IsType<RectangleGeometry>(child.Clip);
            Assert.True(
                clip.Rect.Y >= obstructionBottom - child.Bounds.Y - 0.5d,
                "Content crossing the pinned header boundary should be clipped below the header.");
            Assert.True(clip.Rect.Height > 0d);
            Assert.True(clip.Rect.Bottom <= child.Bounds.Height + 0.5d);
        }
    }

    [AvaloniaFact]
    public void Section_panel_uses_single_sticky_header_without_cumulative_gap()
    {
        var panel = CreateSectionPanel(sectionCount: 2, itemCount: 10);
        panel.ScrollOffset = 760d;

        ArrangePanel(panel);

        var firstHeader = Assert.IsAssignableFrom<Control>(panel.Children[0]);
        var secondHeader = Assert.IsAssignableFrom<Control>(panel.Children[11]);
        var firstSecondSectionRow = Assert.IsAssignableFrom<Control>(panel.Children[12]);

        Assert.Equal(0d, firstHeader.Opacity);
        Assert.Equal(1d, secondHeader.Opacity);
        Assert.Equal(0d, secondHeader.Bounds.Y);
        Assert.Equal(42d, secondHeader.Bounds.Height);

        var clip = Assert.IsType<RectangleGeometry>(firstSecondSectionRow.Clip);
        Assert.True(clip.Rect.Y < 74d, "Sticky section clipping should use one active header, not cumulative previous headers.");
    }

    [AvaloniaFact]
    public void Section_header_shrinks_smoothly_before_sticking()
    {
        var panel = CreateSectionPanel(sectionCount: 1, itemCount: 10);

        panel.ScrollOffset = 0d;
        ArrangePanel(panel);
        var header = Assert.IsAssignableFrom<Control>(panel.Children[0]);
        Assert.Equal(74d, header.Bounds.Height);

        panel.ScrollOffset = 8d;
        ArrangePanel(panel);
        Assert.Equal(66d, header.Bounds.Height);

        panel.ScrollOffset = 16d;
        ArrangePanel(panel);
        Assert.Equal(58d, header.Bounds.Height);

        panel.ScrollOffset = 32d;
        ArrangePanel(panel);
        Assert.Equal(42d, header.Bounds.Height);
    }

    private static ScrollViewer GetLargestVisibleScrollViewer(TopLevel window)
    {
        return window
            .GetVisualDescendants()
            .OfType<ScrollViewer>()
            .Where(viewer => viewer.IsEffectivelyVisible)
            .MaxBy(viewer => viewer.Extent.Height)
            ?? throw new InvalidOperationException("No visible ScrollViewer was found.");
    }

    private static void AssertSliverPanelOffset(TopLevel window, int tabIndex, double expectedOffset)
    {
        var actualOffset = tabIndex switch
        {
            0 or 7 => window
                .GetVisualDescendants()
                .OfType<AvaloniaSlivers.SliverVirtualizingStackPanel>()
                .Single(panel => panel.IsEffectivelyVisible)
                .ScrollOffset,
            1 => window
                .GetVisualDescendants()
                .OfType<AvaloniaSlivers.SliverVirtualizingListPanel>()
                .Single(panel => panel.IsEffectivelyVisible)
                .ScrollOffset,
            2 => window
                .GetVisualDescendants()
                .OfType<AvaloniaSlivers.SliverGridPanel>()
                .Single(panel => panel.IsEffectivelyVisible)
                .ScrollOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(tabIndex), tabIndex, null)
        };

        Assert.True(
            Math.Abs(expectedOffset - actualOffset) <= 0.5d,
            $"Tab {tabIndex} panel offset should be {expectedOffset}, but was {actualOffset}.");
    }

    private static int CountRenderedTextInViewport(ScrollViewer scrollViewer, string textPrefix)
    {
        var viewportBounds = new Rect(scrollViewer.Bounds.Size);
        return scrollViewer
            .GetVisualDescendants()
            .OfType<TextBlock>()
            .Count(textBlock =>
                textBlock.Opacity > 0d &&
                textBlock.IsEffectivelyVisible &&
                textBlock.Text?.StartsWith(textPrefix, StringComparison.Ordinal) == true &&
                textBlock.TranslatePoint(default, scrollViewer) is { } position &&
                new Rect(position, textBlock.Bounds.Size).Intersects(viewportBounds));
    }

    private static SliverScenarioStackPanel CreateSectionPanel(int sectionCount, int itemCount)
    {
        var panel = new SliverScenarioStackPanel
        {
            CacheExtent = 280d,
            CanVerticallyScroll = true
        };

        var childIndex = 0;
        for (var section = 0; section < sectionCount; section++)
        {
            panel.Children.Add(CreateBlockControl(new AvaloniaSliverBlock(
                AvaloniaSliverBlockKind.Header,
                $"Section {section + 1}",
                "Header",
                "#2563EB",
                74d,
                childIndex++)));

            for (var item = 0; item < itemCount; item++)
            {
                panel.Children.Add(CreateBlockControl(new AvaloniaSliverBlock(
                    AvaloniaSliverBlockKind.Box,
                    $"Item {section + 1}.{item + 1}",
                    "Row",
                    "#0F766E",
                    60d,
                    childIndex++)));
            }
        }

        return panel;
    }

    private static Control CreateBlockControl(AvaloniaSliverBlock block)
    {
        return new Border
        {
            DataContext = block,
            Child = new TextBlock { Text = block.Title }
        };
    }

    private static void ArrangePanel(Control panel)
    {
        panel.Measure(new Size(760d, 520d));
        panel.Arrange(new Rect(0d, 0d, 760d, 520d));
    }

    private static void SaveScreenshot(TopLevel topLevel, string name)
    {
        var frame = topLevel.CaptureRenderedFrame()
                    ?? throw new InvalidOperationException("Headless renderer did not capture a frame.");
        var root = Environment.GetEnvironmentVariable("AVALONIA_SCREENSHOT_DIR");
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.Combine(AppContext.BaseDirectory, "headless-screenshots");
        }

        Directory.CreateDirectory(root);
        frame.Save(Path.Combine(root, $"{name}.png"));
    }
}
