using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using SliverWidgets.Core;
using SliverWidgets.GalleryData;
using SliverWidgets.WinUI;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;

namespace SliverWidgets.WinUIGallery;

public sealed class MainWindow : Window
{
    private static readonly IReadOnlyList<GalleryScenario> Scenarios = SliverGalleryData.CreateScenarios();

    private readonly ContentControl _contentHost = new();
    private readonly List<Button> _tabButtons = [];
    private readonly List<GalleryPage> _pages;

    public MainWindow()
    {
        Title = "SliverWidgets WinUI Gallery";
        _pages =
        [
            CreatePage(GalleryScenarioKind.FixedExtentList, CreateFixedLargeListPage),
            CreatePage(GalleryScenarioKind.VariableExtentList, CreateVariableListPage),
            CreatePage(GalleryScenarioKind.VariableStack, CreateStackPage),
            CreatePage(GalleryScenarioKind.AdaptiveGrid, CreateAdaptiveGridPage),
            CreatePage(GalleryScenarioKind.VariableWrap, CreateWrapPage),
            CreatePage(GalleryScenarioKind.PinnedHeader, CreatePinnedHeaderPage),
            CreatePage(GalleryScenarioKind.TabbedNestedScroll, CreateTabbedNestedPage),
            CreatePage(GalleryScenarioKind.MixedComposition, CreateMixedCompositionPage),
            CreatePage(GalleryScenarioKind.SectionedHeaders, CreateSectionedHeaderPage),
            CreatePage(GalleryScenarioKind.FillPaddingVisibility, CreateFillVisibilityPage),
            CreatePage(GalleryScenarioKind.CacheStress, CreateCacheStressPage)
        ];

        Content = CreateShell();
        ShowPage(_pages[0]);
    }

    private Grid CreateShell()
    {
        var shell = new Grid
        {
            Background = Brush(Color.FromArgb(255, 246, 247, 249))
        };
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Border
        {
            Padding = new Thickness(20, 16, 20, 16),
            Background = Brush(Colors.White),
            BorderBrush = Brush(Color.FromArgb(255, 216, 224, 236)),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = CreateHeader()
        };
        shell.Children.Add(header);

        var tabScroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Enabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollMode = ScrollMode.Disabled,
            Content = CreateTabs()
        };
        Grid.SetRow(tabScroller, 1);
        shell.Children.Add(tabScroller);

        _contentHost.Margin = new Thickness(24, 20, 24, 20);
        Grid.SetRow(_contentHost, 2);
        shell.Children.Add(_contentHost);
        return shell;
    }

    private FrameworkElement CreateHeader()
    {
        var root = new Grid { ColumnSpacing = 24 };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        root.Children.Add(new StackPanel
        {
            Spacing = 4,
            Children =
            {
                Text("SliverWidgets WinUI Gallery", 30, FontWeights.SemiBold, Color.FromArgb(255, 17, 24, 39)),
                Text("Unified Flutter-inspired sliver scenario catalog using native WinUI controls.", 15, FontWeights.Normal, Color.FromArgb(255, 75, 85, 99))
            }
        });

        var metrics = CreateMetrics();
        Grid.SetColumn(metrics, 1);
        root.Children.Add(metrics);
        return root;
    }

    private static FrameworkElement CreateMetrics()
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        foreach (var metric in SliverGalleryData.CreateMetrics())
        {
            row.Children.Add(new Border
            {
                MinWidth = 132,
                Padding = new Thickness(10),
                CornerRadius = new CornerRadius(6),
                Background = Brush(Color.FromArgb(255, 248, 250, 252)),
                BorderBrush = Brush(ColorFromHex(metric.AccentColor)),
                BorderThickness = new Thickness(2, 0, 0, 0),
                Child = new StackPanel
                {
                    Children =
                    {
                        Text(metric.Label, 12, FontWeights.Normal, Color.FromArgb(255, 100, 116, 139)),
                        Text(metric.Value, 18, FontWeights.SemiBold, Color.FromArgb(255, 15, 23, 42)),
                        Text(metric.Detail, 12, FontWeights.Normal, Color.FromArgb(255, 100, 116, 139))
                    }
                }
            });
        }

        return row;
    }

    private FrameworkElement CreateTabs()
    {
        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            Padding = new Thickness(24, 18, 24, 14),
            Background = Brush(Color.FromArgb(255, 243, 246, 250))
        };

        foreach (var page in _pages)
        {
            var button = new Button
            {
                Content = page.TabLabel,
                Tag = page,
                Padding = new Thickness(10, 6, 10, 6),
                Background = Brush(Colors.Transparent),
                Foreground = Brush(Color.FromArgb(255, 107, 114, 128)),
                BorderBrush = Brush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                FontSize = 18
            };
            button.Click += (_, _) => ShowPage(page);
            _tabButtons.Add(button);
            tabs.Children.Add(button);
        }

        return tabs;
    }

    private void ShowPage(GalleryPage page)
    {
        foreach (var button in _tabButtons)
        {
            var selected = ReferenceEquals(button.Tag, page);
            button.Foreground = selected
                ? Brush(Color.FromArgb(255, 17, 24, 39))
                : Brush(Color.FromArgb(255, 107, 114, 128));
            button.BorderBrush = selected
                ? Brush(Color.FromArgb(255, 37, 99, 235))
                : Brush(Colors.Transparent);
            button.BorderThickness = selected ? new Thickness(0, 0, 0, 2) : new Thickness(0);
        }

        _contentHost.Content = page.Create();
    }

    private static FrameworkElement CreateFixedLargeListPage()
    {
        var scenario = Scenario(GalleryScenarioKind.FixedExtentList);
        var items = SliverGalleryData.CreateItems(100_000);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 64,
            Spacing = 6
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.Row));
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        AddSlider(controls, "Item extent", 36, 104, layout.ItemExtent, value => layout.ItemExtent = value);
        AddSlider(controls, "Spacing", 0, 20, layout.Spacing, value => layout.Spacing = value);
        AddSlider(controls, "Vertical cache", 0, 5, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateVariableListPage()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableExtentList);
        var items = SliverGalleryData.CreateItems(1_200);
        var layout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 6
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.VariableRow));
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            $"{scenario.Summary} This uses native StackLayout virtualization until the WinUI adapter exposes SliverVariableExtentListLayout.");
        AddSlider(controls, "Spacing", 0, 20, layout.Spacing, value => layout.Spacing = value);
        AddSlider(controls, "Vertical cache", 0, 5, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateStackPage()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableStack);
        var items = SliverGalleryData.CreateStackItems(100_000);
        var layout = new SliverStackVirtualizingLayout
        {
            MinItemMainAxisExtent = SliverGalleryData.StackMinMainAxisExtent,
            MaxItemMainAxisExtent = SliverGalleryData.StackMaxMainAxisExtent,
            MinItemCrossAxisExtent = SliverGalleryData.StackMinCrossAxisExtent,
            MaxItemCrossAxisExtent = SliverGalleryData.StackMaxCrossAxisExtent,
            Spacing = 8,
            CrossAxisAlignment = SliverCrossAxisAlignment.Center
        };
        var factory = new GalleryItemElementFactory(GalleryItemVisualMode.StackCard);
        var repeater = CreateRepeater(items, layout, factory);
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        var realized = Text("Realized elements: 0", 13, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55));
        factory.ActiveCountChanged += count => realized.Text = $"Realized elements: {count}";
        controls.Children.Add(realized);
        AddSlider(controls, "Min height", 36, 96, layout.MinItemMainAxisExtent, value => layout.MinItemMainAxisExtent = value);
        AddSlider(controls, "Max height", 96, 180, layout.MaxItemMainAxisExtent, value => layout.MaxItemMainAxisExtent = value);
        AddSlider(controls, "Min width", 120, 300, layout.MinItemCrossAxisExtent, value => layout.MinItemCrossAxisExtent = value);
        AddSlider(controls, "Max width", 360, 760, layout.MaxItemCrossAxisExtent, value => layout.MaxItemCrossAxisExtent = value);
        AddSlider(controls, "Spacing", 0, 24, layout.Spacing, value => layout.Spacing = value);
        AddSlider(controls, "Vertical cache", 0, 8, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateAdaptiveGridPage()
    {
        var scenario = Scenario(GalleryScenarioKind.AdaptiveGrid);
        var items = SliverGalleryData.CreateItems(480);
        var layout = new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.MaxCrossAxisExtent,
            MaxCrossAxisExtent = 220,
            MainAxisSpacing = 12,
            CrossAxisSpacing = 12,
            ChildAspectRatio = 1.4
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.Tile));
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        AddSlider(controls, "Max tile width", 140, 340, layout.MaxCrossAxisExtent, value => layout.MaxCrossAxisExtent = value);
        AddSlider(controls, "Main spacing", 0, 24, layout.MainAxisSpacing, value => layout.MainAxisSpacing = value);
        AddSlider(controls, "Cross spacing", 0, 24, layout.CrossAxisSpacing, value => layout.CrossAxisSpacing = value);
        AddSlider(controls, "Vertical cache", 0, 5, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateWrapPage()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableWrap);
        var items = SliverGalleryData.CreateWrapItems(100_000);
        var layout = new SliverWrapVirtualizingLayout
        {
            MinItemMainAxisExtent = SliverGalleryData.WrapMinMainAxisExtent,
            MaxItemMainAxisExtent = SliverGalleryData.WrapMaxMainAxisExtent,
            MinItemCrossAxisExtent = SliverGalleryData.WrapMinCrossAxisExtent,
            MaxItemCrossAxisExtent = SliverGalleryData.WrapMaxCrossAxisExtent,
            MainAxisSpacing = 10,
            CrossAxisSpacing = 10
        };
        var factory = new GalleryItemElementFactory(GalleryItemVisualMode.WrapChip);
        var repeater = CreateRepeater(items, layout, factory);
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        var realized = Text("Realized elements: 0", 13, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55));
        factory.ActiveCountChanged += count => realized.Text = $"Realized elements: {count}";
        controls.Children.Add(realized);
        AddSlider(controls, "Min height", 40, 100, layout.MinItemMainAxisExtent, value => layout.MinItemMainAxisExtent = value);
        AddSlider(controls, "Max height", 96, 180, layout.MaxItemMainAxisExtent, value => layout.MaxItemMainAxisExtent = value);
        AddSlider(controls, "Min width", 80, 180, layout.MinItemCrossAxisExtent, value => layout.MinItemCrossAxisExtent = value);
        AddSlider(controls, "Max width", 180, 360, layout.MaxItemCrossAxisExtent, value => layout.MaxItemCrossAxisExtent = value);
        AddSlider(controls, "Spacing", 0, 24, layout.MainAxisSpacing, value =>
        {
            layout.MainAxisSpacing = value;
            layout.CrossAxisSpacing = value;
        });
        AddSlider(controls, "Vertical cache", 0, 8, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateCacheStressPage()
    {
        var scenario = Scenario(GalleryScenarioKind.CacheStress);
        var items = SliverGalleryData.CreateItems(100_000);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 52,
            Spacing = 2
        };
        var factory = new GalleryItemElementFactory(GalleryItemVisualMode.DenseRow);
        var repeater = CreateRepeater(items, layout, factory);
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        var realized = Text("Realized elements: 0", 13, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55));
        factory.ActiveCountChanged += count => realized.Text = $"Realized elements: {count}";
        controls.Children.Add(realized);
        AddSlider(controls, "Item extent", 36, 76, layout.ItemExtent, value => layout.ItemExtent = value);
        AddSlider(controls, "Spacing", 0, 10, layout.Spacing, value => layout.Spacing = value);
        AddSlider(controls, "Vertical cache", 0, 8, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        var jump = new Button
        {
            Content = "Jump to record 50,000",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 8, 0, 0)
        };
        jump.Click += (_, _) => scrollViewer.ChangeView(null, 50_000 * (layout.ItemExtent + layout.Spacing), null);
        controls.Children.Add(jump);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreatePinnedHeaderPage()
    {
        var scenario = Scenario(GalleryScenarioKind.PinnedHeader);
        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        controls.Children.Add(Text(
            "Persistent header geometry exists in core; this gallery keeps WinUI scrolling policy outside the layout until a native header adapter lands.",
            13,
            FontWeights.Normal,
            Color.FromArgb(255, 75, 85, 99)));

        return CreateSampleLayout(controls, CreateHeaderDemo("Pinned SliverAppBar concept", "Collapses to a pinned toolbar while rows virtualize beneath it.", floating: false));
    }

    private static FrameworkElement CreateMixedCompositionPage()
    {
        var scenario = Scenario(GalleryScenarioKind.MixedComposition);
        var root = new StackPanel
        {
            Spacing = 18,
            Padding = new Thickness(0, 0, 0, 24)
        };

        root.Children.Add(HeroPanel("WinUI CustomScrollView composition", "Box content, fixed sliver rows, and adaptive grid tiles share one conceptual scroll catalog."));
        root.Children.Add(SectionHeader("SliverToBoxAdapter-style summary", "Normal WinUI controls can sit between bounded virtualized sliver previews."));
        root.Children.Add(SummaryBand());
        root.Children.Add(SectionHeader("Fixed extent sliver list", "A compact activity feed hosted by ItemsRepeater."));
        root.Children.Add(FixedHeightRepeater(SliverGalleryData.CreateItems(300), 260, new SliverFixedExtentVirtualizingLayout { ItemExtent = 58, Spacing = 4 }, GalleryItemVisualMode.DenseRow));
        root.Children.Add(SectionHeader("Responsive grid sliver", "Adaptive max-extent grid composition using the WinUI adapter."));
        root.Children.Add(FixedHeightRepeater(SliverGalleryData.CreateItems(480), 420, new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.MaxCrossAxisExtent,
            MaxCrossAxisExtent = 210,
            MainAxisSpacing = 10,
            CrossAxisSpacing = 10,
            ChildAspectRatio = 1.35
        }, GalleryItemVisualMode.Tile));

        var controls = CreateControlPanel(
            scenario.Title,
            $"{scenario.Summary} Nested preview repeaters are height-bounded so ItemsRepeater receives a finite viewport and cache window.");

        return CreateSampleLayout(controls, CreateScrollViewer(root));
    }

    private static FrameworkElement CreateTabbedNestedPage()
    {
        var scenario = Scenario(GalleryScenarioKind.TabbedNestedScroll);
        var results = CreateTabbedInnerScroll(SliverGalleryData.CreateItems(180));
        var saved = CreateTabbedInnerScroll(SliverGalleryData.CreateItems(180).Reverse().ToArray());
        var contentHost = new ContentControl();
        var resultsButton = SegmentButton("Results");
        var savedButton = SegmentButton("Saved");

        void Select(Button selected, Button other, UIElement content)
        {
            selected.Background = Brush(Color.FromArgb(255, 37, 99, 235));
            selected.Foreground = Brush(Colors.White);
            other.Background = Brush(Colors.White);
            other.Foreground = Brush(Color.FromArgb(255, 51, 65, 85));
            contentHost.Content = content;
        }

        resultsButton.Click += (_, _) => Select(resultsButton, savedButton, results);
        savedButton.Click += (_, _) => Select(savedButton, resultsButton, saved);
        Select(resultsButton, savedButton, results);

        var viewport = new Grid();
        viewport.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        viewport.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        viewport.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        viewport.Children.Add(new Border
        {
            Padding = new Thickness(18, 14, 18, 14),
            Background = Brush(Color.FromArgb(255, 30, 58, 138)),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    Text("NestedScrollView-style catalog", 20, FontWeights.SemiBold, Colors.White),
                    Text("Pinned header plus tabbed inner scroll bodies.", 12, FontWeights.Normal, Color.FromArgb(255, 219, 234, 254))
                }
            }
        });

        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Padding = new Thickness(12, 10, 12, 10),
            Background = Brush(Colors.White),
            Children = { resultsButton, savedButton }
        };
        Grid.SetRow(tabs, 1);
        viewport.Children.Add(tabs);

        Grid.SetRow(contentHost, 2);
        viewport.Children.Add(contentHost);

        var controls = CreateControlPanel(
            scenario.Title,
            $"{scenario.Summary} Flutter uses NestedScrollView plus overlap absorber/injector for this pattern; the WinUI sample keeps separate native scroll bodies until a nested-scroll adapter exists.");

        return CreateSampleLayout(controls, viewport);
    }

    private static FrameworkElement CreateTabbedInnerScroll(IReadOnlyList<GalleryItem> items)
    {
        return CreateScrollViewer(CreateRepeater(
            items,
            new SliverFixedExtentVirtualizingLayout { ItemExtent = 52, Spacing = 4 },
            new GalleryItemElementFactory(GalleryItemVisualMode.DenseRow)));
    }

    private static FrameworkElement CreateSectionedHeaderPage()
    {
        var scenario = Scenario(GalleryScenarioKind.SectionedHeaders);
        var sections = SliverGalleryData.CreateSections(6, 90);
        var stickyTitle = Text(sections[0].Title, 16, FontWeights.SemiBold, Colors.White);
        var content = new StackPanel
        {
            Spacing = 18,
            Padding = new Thickness(16, 62, 16, 24)
        };

        foreach (var section in sections)
        {
            content.Children.Add(SectionHeader(section.Title, section.Summary));
            content.Children.Add(FixedHeightRepeater(section.Items, 280, new SliverFixedExtentVirtualizingLayout { ItemExtent = 50, Spacing = 3 }, GalleryItemVisualMode.DenseRow));
        }

        var scroller = CreateScrollViewer(content);
        scroller.Padding = new Thickness(0);
        scroller.ViewChanged += (_, _) =>
        {
            var index = Math.Clamp((int)(scroller.VerticalOffset / 390), 0, sections.Count - 1);
            stickyTitle.Text = sections[index].Title;
        };

        var layered = new Grid();
        layered.Children.Add(scroller);
        layered.Children.Add(new Border
        {
            Height = 46,
            Margin = new Thickness(12),
            Padding = new Thickness(16, 0, 16, 0),
            CornerRadius = new CornerRadius(8),
            Background = Brush(Color.FromArgb(255, 15, 118, 110)),
            VerticalAlignment = VerticalAlignment.Top,
            Child = stickyTitle
        });

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        controls.Children.Add(Text(
            "The sticky header is a WinUI overlay in this sample; a future adapter can translate persistent-header geometry directly.",
            13,
            FontWeights.Normal,
            Color.FromArgb(255, 75, 85, 99)));

        return CreateSampleLayout(controls, layered);
    }

    private static FrameworkElement CreateFillVisibilityPage()
    {
        var scenario = Scenario(GalleryScenarioKind.FillPaddingVisibility);
        var showDetails = new CheckBox
        {
            Content = "show replacement sliver content",
            IsChecked = true
        };

        var detail = FillPanel("Visible content", "SliverVisibility keeps this content in the composition when enabled.", Color.FromArgb(255, 238, 242, 255), Color.FromArgb(255, 55, 48, 163));
        var replacement = FillPanel("Replacement content", "The hidden branch can still reserve layout space or swap an alternate child.", Color.FromArgb(255, 255, 247, 237), Color.FromArgb(255, 194, 65, 12));
        replacement.Visibility = Visibility.Collapsed;

        showDetails.Checked += (_, _) =>
        {
            detail.Visibility = Visibility.Visible;
            replacement.Visibility = Visibility.Collapsed;
        };
        showDetails.Unchecked += (_, _) =>
        {
            detail.Visibility = Visibility.Collapsed;
            replacement.Visibility = Visibility.Visible;
        };

        var content = new StackPanel
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Border { Height = 44, Background = Brush(Color.FromArgb(255, 224, 242, 254)), CornerRadius = new CornerRadius(8) },
                detail,
                replacement,
                new Border
                {
                    MinHeight = 260,
                    Padding = new Thickness(20),
                    Background = Brush(Color.FromArgb(255, 248, 250, 252)),
                    BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Child = SectionHeader("Fill remaining", "This block stretches the composition like SliverFillRemaining after padded content.")
                }
            }
        };

        var controls = CreateControlPanel(
            scenario.Title,
            scenario.Summary);
        controls.Children.Add(showDetails);

        return CreateSampleLayout(controls, CreateScrollViewer(content));
    }

    private static FrameworkElement CreateHeaderDemo(string title, string description, bool floating)
    {
        const double maxHeaderHeight = 148;
        const double minHeaderHeight = 58;
        var items = SliverGalleryData.CreateItems(1_200);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 48,
            Spacing = 4
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.DenseRow));
        var scrollViewer = CreateScrollViewer(repeater);
        scrollViewer.Margin = new Thickness(0, maxHeaderHeight, 0, 0);

        var root = new Grid();
        var frame = new Border
        {
            Background = Brush(Colors.White),
            CornerRadius = new CornerRadius(8),
            Child = root
        };
        root.Children.Add(scrollViewer);

        var transform = new TranslateTransform();
        var header = new Border
        {
            Height = maxHeaderHeight,
            Padding = new Thickness(18, 14, 18, 14),
            Background = Brush(floating ? Color.FromArgb(255, 20, 83, 45) : Color.FromArgb(255, 30, 64, 175)),
            VerticalAlignment = VerticalAlignment.Top,
            RenderTransform = transform,
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    Text(title, 19, FontWeights.SemiBold, Colors.White),
                    Text(description, 12, FontWeights.Normal, Color.FromArgb(230, 255, 255, 255))
                }
            }
        };
        root.Children.Add(header);

        var previousOffset = 0d;
        scrollViewer.ViewChanged += (_, _) =>
        {
            var currentOffset = scrollViewer.VerticalOffset;
            if (floating)
            {
                var scrollingDown = currentOffset > previousOffset;
                transform.Y = scrollingDown && currentOffset > maxHeaderHeight ? -maxHeaderHeight : 0;
                header.Opacity = transform.Y < 0 ? 0.08 : 1;
            }
            else
            {
                var collapsed = Math.Min(maxHeaderHeight - minHeaderHeight, currentOffset);
                header.Height = maxHeaderHeight - collapsed;
                scrollViewer.Margin = new Thickness(0, header.Height, 0, 0);
            }

            previousOffset = currentOffset;
        };

        return frame;
    }

    private static FrameworkElement FixedHeightRepeater(IReadOnlyList<GalleryItem> items, double height, VirtualizingLayout layout, GalleryItemVisualMode mode)
    {
        return new Border
        {
            Height = height,
            BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = CreateScrollViewer(CreateRepeater(items, layout, new GalleryItemElementFactory(mode)))
        };
    }

    private static FrameworkElement HeroPanel(string title, string subtitle)
    {
        return new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(22),
            Background = Brush(Color.FromArgb(255, 23, 32, 51)),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    Text(title, 24, FontWeights.SemiBold, Colors.White),
                    Text(subtitle, 14, FontWeights.Normal, Color.FromArgb(255, 208, 213, 221))
                }
            }
        };
    }

    private static FrameworkElement SummaryBand()
    {
        var metrics = SliverGalleryData.CreateMetrics().Take(3).ToArray();
        var grid = new Grid { ColumnSpacing = 12 };

        for (var i = 0; i < metrics.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var metric = metrics[i];
            var card = new Border
            {
                Padding = new Thickness(16),
                CornerRadius = new CornerRadius(8),
                Background = Brush(Color.FromArgb(255, 239, 246, 255)),
                Child = new StackPanel
                {
                    Spacing = 4,
                    Children =
                    {
                        Text(metric.Label, 12, FontWeights.SemiBold, Color.FromArgb(255, 29, 78, 216)),
                        Text(metric.Value, 20, FontWeights.SemiBold, Color.FromArgb(255, 30, 41, 59))
                    }
                }
            };
            Grid.SetColumn(card, i);
            grid.Children.Add(card);
        }

        return grid;
    }

    private static FrameworkElement SectionHeader(string title, string subtitle)
    {
        return new StackPanel
        {
            Spacing = 4,
            Children =
            {
                Text(title, 19, FontWeights.SemiBold, Color.FromArgb(255, 17, 24, 39)),
                Text(subtitle, 13, FontWeights.Normal, Color.FromArgb(255, 102, 112, 133))
            }
        };
    }

    private static FrameworkElement FillPanel(string title, string detail, Color background, Color foreground)
    {
        return new Border
        {
            MinHeight = 118,
            Padding = new Thickness(20),
            Background = Brush(background),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    Text(title, 18, FontWeights.SemiBold, foreground),
                    Text(detail, 13, FontWeights.Normal, Color.FromArgb(255, 75, 85, 99))
                }
            }
        };
    }

    private static Grid CreateSampleLayout(StackPanel controls, FrameworkElement viewport)
    {
        var layout = new Grid { ColumnSpacing = 20 };
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var controlFrame = new Border
        {
            Background = Brush(Colors.White),
            BorderBrush = Brush(Color.FromArgb(255, 203, 213, 225)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = controls
        };

        var viewportFrame = new Border
        {
            Background = Brush(Color.FromArgb(255, 232, 238, 247)),
            BorderBrush = Brush(Color.FromArgb(255, 203, 213, 225)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = viewport
        };

        Grid.SetColumn(controlFrame, 0);
        Grid.SetColumn(viewportFrame, 1);
        layout.Children.Add(controlFrame);
        layout.Children.Add(viewportFrame);
        return layout;
    }

    private static GalleryPage CreatePage(GalleryScenarioKind kind, Func<FrameworkElement> create)
    {
        var scenario = Scenario(kind);
        return new GalleryPage(scenario.TabLabel, scenario.Title, scenario.SupportedFeature, create);
    }

    private static GalleryScenario Scenario(GalleryScenarioKind kind)
    {
        return Scenarios.First(scenario => scenario.Kind == kind);
    }

    private static StackPanel CreateControlPanel(string title, string description)
    {
        return new StackPanel
        {
            Spacing = 12,
            Padding = new Thickness(18),
            Background = Brush(Colors.White),
            Children =
            {
                Text(title, 22, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                Text(description, 13, FontWeights.Normal, Color.FromArgb(255, 75, 85, 99))
            }
        };
    }

    private static void AddSlider(StackPanel panel, string title, double minimum, double maximum, double value, Action<double> valueChanged)
    {
        var valueText = Text(FormatSliderValue(value), 12, FontWeights.SemiBold, Color.FromArgb(255, 55, 65, 81));
        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            StepFrequency = maximum - minimum <= 10 ? 0.25 : 1,
            TickFrequency = maximum - minimum <= 10 ? 1 : 10,
            TickPlacement = TickPlacement.Outside,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        slider.ValueChanged += (_, args) =>
        {
            valueText.Text = FormatSliderValue(args.NewValue);
            valueChanged(args.NewValue);
        };

        panel.Children.Add(new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Children =
            {
                Text(title, 13, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                WithColumn(valueText, 1)
            }
        });
        panel.Children.Add(slider);
    }

    private static ItemsRepeater CreateRepeater(IReadOnlyList<GalleryItem> items, VirtualizingLayout layout, IElementFactory factory)
    {
        return new ItemsRepeater
        {
            ItemsSource = items,
            Layout = layout,
            ItemTemplate = factory,
            VerticalCacheLength = 2
        };
    }

    private static ScrollViewer CreateScrollViewer(UIElement content)
    {
        return new ScrollViewer
        {
            Content = content,
            Background = Brush(Colors.White),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            HorizontalScrollMode = ScrollMode.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollMode = ScrollMode.Enabled,
            ZoomMode = ZoomMode.Disabled,
            Padding = new Thickness(14)
        };
    }

    private static TextBlock Text(string text, double fontSize, FontWeight weight, Color color)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = weight,
            Foreground = Brush(color),
            TextWrapping = TextWrapping.WrapWholeWords
        };
    }

    private static T WithColumn<T>(T element, int column)
        where T : FrameworkElement
    {
        Grid.SetColumn(element, column);
        return element;
    }

    private static Button SegmentButton(string label)
    {
        return new Button
        {
            Content = label,
            Padding = new Thickness(12, 7, 12, 7),
            BorderBrush = Brush(Color.FromArgb(255, 203, 213, 225)),
            BorderThickness = new Thickness(1)
        };
    }

    private static SolidColorBrush Brush(Color color) => new(color);

    private static Color ColorFromHex(string hex)
    {
        var value = Convert.ToUInt32(hex.TrimStart('#'), 16);
        return Color.FromArgb(
            255,
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value);
    }

    private static string FormatSliderValue(double value)
    {
        return Math.Abs(value - Math.Round(value)) < 0.01 ? value.ToString("0") : value.ToString("0.0");
    }

    private sealed record GalleryPage(string TabLabel, string Title, string Description, Func<FrameworkElement> Create);
}

internal enum GalleryItemVisualMode
{
    Row,
    DenseRow,
    Tile,
    VariableRow,
    StackCard,
    WrapChip
}

internal sealed class GalleryItemElementFactory : IElementFactory
{
    private readonly HashSet<UIElement> _activeElements = [];
    private readonly GalleryItemVisualMode _mode;

    public GalleryItemElementFactory(GalleryItemVisualMode mode)
    {
        _mode = mode;
    }

    public event Action<int>? ActiveCountChanged;

    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        var item = args.Data as GalleryItem ?? EmptyItem;
        var element = _mode switch
        {
            GalleryItemVisualMode.Tile => CreateTile(item),
            GalleryItemVisualMode.VariableRow => CreateVariableRow(item),
            GalleryItemVisualMode.DenseRow => CreateDenseRow(item),
            GalleryItemVisualMode.StackCard => CreateStackCard(item),
            GalleryItemVisualMode.WrapChip => CreateWrapChip(item),
            _ => CreateRow(item)
        };

        _activeElements.Add(element);
        ActiveCountChanged?.Invoke(_activeElements.Count);
        return element;
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        if (_activeElements.Remove(args.Element))
        {
            ActiveCountChanged?.Invoke(_activeElements.Count);
        }
    }

    private static UIElement CreateRow(GalleryItem item)
    {
        var root = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(12, 8, 12, 8),
            Background = Brush(Colors.White)
        };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        root.Children.Add(CreateIndexBadge(item));

        var text = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                Text(item.Title, 15, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                Text(item.Subtitle, 12, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128))
            }
        };
        Grid.SetColumn(text, 1);
        root.Children.Add(text);

        var status = Text(item.Category, 12, FontWeights.SemiBold, Accent(item));
        status.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(status, 2);
        root.Children.Add(status);
        return root;
    }

    private static UIElement CreateDenseRow(GalleryItem item)
    {
        var root = new Grid
        {
            ColumnSpacing = 10,
            Padding = new Thickness(10, 6, 10, 6),
            Background = Brush(Colors.White)
        };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        root.Children.Add(CreateIndexBadge(item, 28));

        var text = new StackPanel
        {
            Spacing = 1,
            Children =
            {
                Text(item.Title, 13, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                Text(item.Subtitle, 11, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128))
            }
        };
        Grid.SetColumn(text, 1);
        root.Children.Add(text);
        return root;
    }

    private static UIElement CreateVariableRow(GalleryItem item)
    {
        return new Border
        {
            MinHeight = item.Extent,
            Margin = new Thickness(0, 0, 0, 6),
            Padding = new Thickness(12, 10, 12, 10),
            Background = Brush(Colors.White),
            BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new Grid
            {
                ColumnSpacing = 12,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(44) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    CreateIndexBadge(item),
                    WithColumn(new StackPanel
                    {
                        Spacing = 2,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            Text(item.Title, 15, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                            Text(item.Subtitle, 12, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128)),
                            Text($"Observed extent {item.Extent:0}px", 11, FontWeights.SemiBold, Accent(item))
                        }
                    }, 1),
                    WithColumn(Text(item.Category, 12, FontWeights.SemiBold, Accent(item)), 2)
                }
            }
        };
    }

    private static UIElement CreateTile(GalleryItem item)
    {
        return new Border
        {
            Margin = new Thickness(0),
            Padding = new Thickness(14),
            Background = Brush(Color.FromArgb(255, 249, 250, 251)),
            BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    CreateIndexBadge(item, 36),
                    Text(item.Title, 16, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                    Text(item.Subtitle, 12, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128)),
                    Text(item.Category, 12, FontWeights.SemiBold, Accent(item))
                }
            }
        };
    }

    private static UIElement CreateStackCard(GalleryItem item)
    {
        var accent = Accent(item);
        return new Border
        {
            Padding = new Thickness(10),
            Background = Brush(Colors.White),
            BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new Grid
            {
                ColumnSpacing = 10,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(4) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    new Border
                    {
                        Background = Brush(accent),
                        CornerRadius = new CornerRadius(2)
                    },
                    WithColumn(new StackPanel
                    {
                        Spacing = 2,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            Text(item.Title, 14, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                            Text(item.Subtitle, 11, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128))
                        }
                    }, 1),
                    WithColumn(Text(item.Rank.ToString(), 12, FontWeights.SemiBold, accent), 2)
                }
            }
        };
    }

    private static UIElement CreateWrapChip(GalleryItem item)
    {
        var accent = Accent(item);
        return new Border
        {
            Padding = new Thickness(10),
            Background = Brush(Color.FromArgb(255, 249, 250, 251)),
            BorderBrush = Brush(Color.FromArgb(255, 226, 232, 240)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new Border
                    {
                        Width = 28,
                        Height = 4,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        CornerRadius = new CornerRadius(2),
                        Background = Brush(accent)
                    },
                    Text(item.Title, 14, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                    Text(item.Category, 11, FontWeights.SemiBold, accent),
                    Text(item.Subtitle, 11, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128))
                }
            }
        };
    }

    private static Border CreateIndexBadge(GalleryItem item, double size = 36)
    {
        var accent = Accent(item);
        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(size / 2),
            Background = Brush(Color.FromArgb(28, accent.R, accent.G, accent.B)),
            Child = new TextBlock
            {
                Text = item.Id.ToString("00"),
                FontSize = size > 30 ? 12 : 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brush(accent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            }
        };
    }

    private static T WithColumn<T>(T element, int column)
        where T : FrameworkElement
    {
        Grid.SetColumn(element, column);
        return element;
    }

    private static TextBlock Text(string text, double fontSize, FontWeight weight, Color color)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            FontWeight = weight,
            Foreground = Brush(color),
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap
        };
    }

    private static SolidColorBrush Brush(Color color) => new(color);

    private static Color Accent(GalleryItem item) => ParseColor(item.AccentColor);

    private static Color ParseColor(string value)
    {
        if (value.Length == 7 &&
            value[0] == '#' &&
            byte.TryParse(value.AsSpan(1, 2), System.Globalization.NumberStyles.HexNumber, null, out var red) &&
            byte.TryParse(value.AsSpan(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var green) &&
            byte.TryParse(value.AsSpan(5, 2), System.Globalization.NumberStyles.HexNumber, null, out var blue))
        {
            return Color.FromArgb(255, red, green, blue);
        }

        return Colors.Gray;
    }

    private static readonly GalleryItem EmptyItem = new(0, "Item", "No data", "None", "#6B7280", 52, 0, false);
}
