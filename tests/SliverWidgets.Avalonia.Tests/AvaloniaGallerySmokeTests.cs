using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Avalonia;
using AvaloniaGallery;

namespace SliverWidgets.Avalonia.Tests;

public sealed class AvaloniaGallerySmokeTests
{
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
            Assert.True(scrollViewer!.Extent.Height > scrollViewer.Viewport.Height, $"Tab {index} should expose a vertical scroll extent.");
            Assert.True(scrollViewer.Viewport.Height > 0, $"Tab {index} should have a measured viewport.");

            SaveScreenshot(window, $"avalonia-gallery-tab-{index}");

            var scrollTarget = Math.Min(220d, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
            scrollViewer.Offset = new Vector(0d, scrollTarget);
            window.UpdateLayout();

            Assert.True(scrollViewer.Offset.Y > 0d, $"Tab {index} should accept native vertical scroll offset.");
            SaveScreenshot(window, $"avalonia-gallery-tab-{index}-scrolled");
        }
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
