using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SliverWidgets.Core;
using SliverWidgets.GalleryData;
using SliverWidgets.Uno;
using Microsoft.UI.Text;
using Windows.UI;

namespace SliverWidgets.Samples.UnoGallery;

public sealed class UnoGalleryPage : Page
{
    private const double DataGridTableWidth = 1670d;

    private readonly IReadOnlyList<GalleryItem> _items = SliverGalleryData.CreateItems(600);
    private readonly IReadOnlyList<GalleryItem> _largeItems = SliverGalleryData.CreateItems(100_000);
    private readonly IReadOnlyList<GalleryItem> _stackItems = SliverGalleryData.CreateStackItems(100_000);
    private readonly IReadOnlyList<GalleryItem> _wrapItems = SliverGalleryData.CreateWrapItems(100_000);
    private readonly IReadOnlyList<GalleryDataGridRow> _dataGridRows = SliverGalleryData.CreateDataGridRows(100_000);
    private readonly IReadOnlyList<GallerySection> _sections = SliverGalleryData.CreateSections(6, 90);
    private readonly IReadOnlyList<GalleryScenario> _scenarios = SliverGalleryData.CreateScenarios();
    private readonly ContentControl _scenarioHost = new();
    private readonly List<Button> _navigationButtons = [];
    private readonly List<GalleryPage> _pages;

    public UnoGalleryPage()
    {
        Background = Brush(0xFFF6F8FB);
        _pages =
        [
            CreatePage(GalleryScenarioKind.FixedExtentList, BuildFixedLargeListScenario),
            CreatePage(GalleryScenarioKind.VariableExtentList, BuildVariableListScenario),
            CreatePage(GalleryScenarioKind.VariableStack, BuildStackScenario),
            CreatePage(GalleryScenarioKind.AdaptiveGrid, BuildAdaptiveGridScenario),
            CreatePage(GalleryScenarioKind.DataGrid, BuildDataGridScenario),
            CreatePage(GalleryScenarioKind.VariableWrap, BuildWrapScenario),
            CreatePage(GalleryScenarioKind.PinnedHeader, BuildPinnedHeaderScenario),
            CreatePage(GalleryScenarioKind.TabbedNestedScroll, BuildTabbedNestedScenario),
            CreatePage(GalleryScenarioKind.MixedComposition, BuildMixedCompositionScenario),
            CreatePage(GalleryScenarioKind.SectionedHeaders, BuildSectionedHeaderScenario),
            CreatePage(GalleryScenarioKind.FillPaddingVisibility, BuildFillVisibilityScenario),
            CreatePage(GalleryScenarioKind.CacheStress, BuildCacheStressScenario)
        ];
        Content = BuildShell();
        ShowScenario(_pages[0]);
    }

    private UIElement BuildShell()
    {
        var root = new Grid
        {
            Background = Brush(0xFFF3F6FA),
            RowSpacing = 0
        };

        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Border
        {
            Padding = new Thickness(20, 16, 20, 16),
            Background = Brush(0xFFFFFFFF),
            BorderBrush = Brush(0xFFD8E0EC),
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = CreateHeader()
        };
        root.Children.Add(header);

        var tabScroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = CreateTabs()
        };
        Grid.SetRow(tabScroller, 1);
        root.Children.Add(tabScroller);

        var contentFrame = new Border
        {
            Padding = new Thickness(24, 20, 24, 20),
            Background = Brush(0xFFF6F8FB),
            Child = _scenarioHost
        };

        Grid.SetRow(contentFrame, 2);
        root.Children.Add(contentFrame);

        return root;
    }

    private UIElement CreateHeader()
    {
        var root = new Grid { ColumnSpacing = 24 };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var title = new StackPanel { Spacing = 4 };
        title.Children.Add(new TextBlock
        {
            Text = "SliverWidgets Uno Gallery",
            Foreground = Brush(0xFF111827),
            FontSize = 30,
            FontWeight = FontWeights.SemiBold
        });
        title.Children.Add(new TextBlock
        {
            Text = "Unified Flutter-inspired sliver scenario catalog using native Uno controls.",
            Foreground = Brush(0xFF4B5563),
            FontSize = 15,
            TextWrapping = TextWrapping.Wrap
        });
        root.Children.Add(title);

        var metrics = CreateMetrics();
        Grid.SetColumn(metrics, 1);
        root.Children.Add(metrics);
        return root;
    }

    private UIElement CreateMetrics()
    {
        var metrics = SliverGalleryData.CreateMetrics();
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        foreach (var metric in metrics)
        {
            row.Children.Add(MetricCard(metric));
        }

        return row;
    }

    private static UIElement MetricCard(GalleryMetric metric)
    {
        return new Border
        {
            MinWidth = 132,
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(6),
            Background = Brush(0xFFF8FAFC),
            BorderBrush = BrushFromHex(metric.AccentColor),
            BorderThickness = new Thickness(2, 0, 0, 0),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = metric.Label, Foreground = Brush(0xFF64748B), FontSize = 12 },
                    new TextBlock { Text = metric.Value, Foreground = Brush(0xFF0F172A), FontSize = 18, FontWeight = FontWeights.SemiBold },
                    new TextBlock { Text = metric.Detail, Foreground = Brush(0xFF64748B), FontSize = 12, TextWrapping = TextWrapping.Wrap }
                }
            }
        };
    }

    private UIElement CreateTabs()
    {
        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
            Padding = new Thickness(24, 18, 24, 14),
            Background = Brush(0xFFF3F6FA)
        };

        foreach (var page in _pages)
        {
            AddNavigation(tabs, page);
        }

        return tabs;
    }

    private void AddNavigation(StackPanel nav, GalleryPage page)
    {
        var button = new Button
        {
            Content = page.Scenario.TabLabel,
            Tag = page,
            Padding = new Thickness(10, 6, 10, 6),
            Background = Brush(0x00FFFFFF),
            Foreground = Brush(0xFF6B7280),
            BorderBrush = Brush(0x00FFFFFF),
            BorderThickness = new Thickness(0),
            FontSize = 18
        };

        button.Click += (_, _) => ShowScenario(page);
        _navigationButtons.Add(button);
        nav.Children.Add(button);
    }

    private void ShowScenario(GalleryPage page)
    {
        foreach (var button in _navigationButtons)
        {
            var selected = ReferenceEquals(button.Tag, page);
            button.Foreground = selected ? Brush(0xFF111827) : Brush(0xFF6B7280);
            button.BorderBrush = selected ? Brush(0xFF2563EB) : Brush(0x00FFFFFF);
            button.BorderThickness = selected ? new Thickness(0, 0, 0, 2) : new Thickness(0);
        }

        _scenarioHost.Content = page.Create();
    }

    private GalleryPage CreatePage(GalleryScenarioKind kind, Func<UIElement> create)
    {
        return new GalleryPage(Scenario(kind), create);
    }

    private GalleryScenario Scenario(GalleryScenarioKind kind)
    {
        return _scenarios.First(scenario => scenario.Kind == kind);
    }

    private UIElement BuildFixedLargeListScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.FixedExtentList);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 64,
            Spacing = 6
        };

        var repeater = CreateRepeater(_largeItems, layout, GalleryItemFactoryKind.List);
        repeater.VerticalCacheLength = 2;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("item extent", 36, 112, layout.ItemExtent, 1, value => layout.ItemExtent = value));
        controls.Children.Add(ControlSlider("spacing", 0, 24, layout.Spacing, 1, value => layout.Spacing = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            Viewport(repeater));
    }

    private UIElement BuildVariableListScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableExtentList);
        var layout = new StackLayout
        {
            Orientation = Orientation.Vertical,
            Spacing = 6
        };

        var repeater = CreateRepeater(_items, layout, GalleryItemFactoryKind.Variable);
        repeater.VerticalCacheLength = 2;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(new TextBlock
        {
            Text = "Uses native Uno/WinUI StackLayout virtualization for non-uniform rows until the framework adapter exposes SliverVariableExtentListLayout.",
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush(0xFF475467),
            FontSize = 13
        });
        controls.Children.Add(ControlSlider("spacing", 0, 20, layout.Spacing, 1, value => layout.Spacing = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            Viewport(repeater));
    }

    private UIElement BuildStackScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableStack);
        var layout = new SliverStackVirtualizingLayout
        {
            MinItemMainAxisExtent = SliverGalleryData.StackMinMainAxisExtent,
            MaxItemMainAxisExtent = SliverGalleryData.StackMaxMainAxisExtent,
            MinItemCrossAxisExtent = SliverGalleryData.StackMinCrossAxisExtent,
            MaxItemCrossAxisExtent = SliverGalleryData.StackMaxCrossAxisExtent,
            Spacing = 8,
            CrossAxisAlignment = SliverCrossAxisAlignment.Center
        };

        var repeater = CreateRepeater(_stackItems, layout, GalleryItemFactoryKind.Stack);
        repeater.VerticalCacheLength = 2;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("min height", 36, 96, layout.MinItemMainAxisExtent, 1, value => layout.MinItemMainAxisExtent = value));
        controls.Children.Add(ControlSlider("max height", 96, 180, layout.MaxItemMainAxisExtent, 1, value => layout.MaxItemMainAxisExtent = value));
        controls.Children.Add(ControlSlider("min width", 120, 300, layout.MinItemCrossAxisExtent, 1, value => layout.MinItemCrossAxisExtent = value));
        controls.Children.Add(ControlSlider("max width", 360, 760, layout.MaxItemCrossAxisExtent, 1, value => layout.MaxItemCrossAxisExtent = value));
        controls.Children.Add(ControlSlider("spacing", 0, 28, layout.Spacing, 1, value => layout.Spacing = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            Viewport(repeater));
    }

    private UIElement BuildAdaptiveGridScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.AdaptiveGrid);
        var layout = new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.MaxCrossAxisExtent,
            MaxCrossAxisExtent = 220,
            MainAxisSpacing = 12,
            CrossAxisSpacing = 12,
            ChildAspectRatio = 1.45
        };

        var repeater = CreateRepeater(_items, layout, GalleryItemFactoryKind.Grid);
        repeater.VerticalCacheLength = 1.5;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("max tile width", 140, 340, layout.MaxCrossAxisExtent, 10, value => layout.MaxCrossAxisExtent = value));
        controls.Children.Add(ControlSlider("main spacing", 0, 32, layout.MainAxisSpacing, 1, value => layout.MainAxisSpacing = value));
        controls.Children.Add(ControlSlider("cross spacing", 0, 32, layout.CrossAxisSpacing, 1, value => layout.CrossAxisSpacing = value));
        controls.Children.Add(ControlSlider("aspect ratio", 0.7, 2.4, layout.ChildAspectRatio, 0.05, value => layout.ChildAspectRatio = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            Viewport(repeater));
    }

    private UIElement BuildDataGridScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.DataGrid);
        var layout = new StackLayout { Orientation = Orientation.Vertical, Spacing = 0 };
        var repeater = new ItemsRepeater
        {
            Layout = layout,
            ItemTemplate = new DataGridRowElementFactory()
        };
        var filter = new TextBox
        {
            PlaceholderText = "Filter account, owner, status...",
            Text = string.Empty
        };
        var sort = new ComboBox
        {
            ItemsSource = new[] { "amount", "updated", "account", "status", "region", "progress" },
            SelectedItem = "amount"
        };
        var descending = new CheckBox
        {
            Content = "Descending",
            IsChecked = true
        };
        var visible = new TextBlock
        {
            Foreground = Brush(0xFF344054),
            FontWeight = FontWeights.SemiBold
        };

        void Refresh()
        {
            var projected = ApplyDataGridQuery(
                _dataGridRows,
                filter.Text ?? string.Empty,
                Convert.ToString(sort.SelectedItem) ?? "amount",
                descending.IsChecked == true);
            repeater.ItemsSource = projected;
            visible.Text = $"Visible rows: {projected.Count:N0}";
        }

        filter.TextChanged += (_, _) => Refresh();
        sort.SelectionChanged += (_, _) => Refresh();
        descending.Checked += (_, _) => Refresh();
        descending.Unchecked += (_, _) => Refresh();
        Refresh();

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ScenarioNote("Rows are native controls with variable heights. Core DataGrid query projection supplies sorting/filtering over the shared 100,000-row source."));
        controls.Children.Add(filter);
        controls.Children.Add(sort);
        controls.Children.Add(descending);
        controls.Children.Add(visible);

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            DataGridViewport(repeater));
    }

    private UIElement BuildWrapScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.VariableWrap);
        var layout = new SliverWrapVirtualizingLayout
        {
            MinItemMainAxisExtent = SliverGalleryData.WrapMinMainAxisExtent,
            MaxItemMainAxisExtent = SliverGalleryData.WrapMaxMainAxisExtent,
            MinItemCrossAxisExtent = SliverGalleryData.WrapMinCrossAxisExtent,
            MaxItemCrossAxisExtent = SliverGalleryData.WrapMaxCrossAxisExtent,
            MainAxisSpacing = 10,
            CrossAxisSpacing = 10
        };

        var repeater = CreateRepeater(_wrapItems, layout, GalleryItemFactoryKind.Wrap);
        repeater.VerticalCacheLength = 2;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("min height", 40, 110, layout.MinItemMainAxisExtent, 1, value => layout.MinItemMainAxisExtent = value));
        controls.Children.Add(ControlSlider("max height", 96, 190, layout.MaxItemMainAxisExtent, 1, value => layout.MaxItemMainAxisExtent = value));
        controls.Children.Add(ControlSlider("min width", 80, 200, layout.MinItemCrossAxisExtent, 1, value => layout.MinItemCrossAxisExtent = value));
        controls.Children.Add(ControlSlider("max width", 180, 380, layout.MaxItemCrossAxisExtent, 1, value => layout.MaxItemCrossAxisExtent = value));
        controls.Children.Add(ControlSlider("spacing", 0, 28, layout.MainAxisSpacing, 1, value =>
        {
            layout.MainAxisSpacing = value;
            layout.CrossAxisSpacing = value;
        }));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            Viewport(repeater));
    }

    private UIElement BuildPinnedHeaderScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.PinnedHeader);
        const double maxHeaderHeight = 148;
        const double minHeaderHeight = 58;

        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 52,
            Spacing = 4
        };

        var repeater = CreateRepeater(_largeItems, layout, GalleryItemFactoryKind.Compact);
        repeater.VerticalCacheLength = 2;

        var scroller = new ScrollViewer
        {
            Content = repeater,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(0xFFFFFFFF),
            Margin = new Thickness(0, maxHeaderHeight, 0, 0)
        };

        var title = new TextBlock
        {
            Text = "Pinned SliverAppBar concept",
            Foreground = Brush(0xFFFFFFFF),
            FontSize = 24,
            FontWeight = FontWeights.SemiBold
        };

        var subtitle = new TextBlock
        {
            Text = "Collapses to a pinned toolbar while fixed-extent rows virtualize below.",
            Foreground = Brush(0xFFDDE7FF),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap
        };

        var header = new Border
        {
            Height = maxHeaderHeight,
            Padding = new Thickness(22, 18, 22, 14),
            Background = Brush(0xFF1D4ED8),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new StackPanel
            {
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Bottom,
                Children = { title, subtitle }
            }
        };

        scroller.ViewChanged += (_, _) =>
        {
            var collapsed = Math.Min(maxHeaderHeight - minHeaderHeight, scroller.VerticalOffset);
            header.Height = maxHeaderHeight - collapsed;
            scroller.Margin = new Thickness(0, header.Height, 0, 0);
            title.FontSize = scroller.VerticalOffset > 70 ? 18 : 24;
            subtitle.Opacity = scroller.VerticalOffset > 48 ? 0 : 1;
        };

        var layered = new Grid();
        layered.Children.Add(scroller);
        layered.Children.Add(header);

        return Scenario(
            scenario.Title,
            scenario.Summary,
            ScenarioNote("Persistent header layout is available in the core protocol; this Uno gallery keeps the adapter surface thin until a native header adapter is added."),
            layered);
    }

    private UIElement BuildMixedCompositionScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.MixedComposition);
        var root = new StackPanel
        {
            Spacing = 18,
            Padding = new Thickness(0, 0, 0, 24)
        };

        root.Children.Add(HeroPanel());
        root.Children.Add(SectionHeader("SliverToBoxAdapter-style summary", "Ordinary Uno controls can sit between virtualized sliver sections."));
        root.Children.Add(SummaryBand());
        root.Children.Add(SectionHeader("Fixed extent sliver list", "A compact activity feed hosted by ItemsRepeater."));
        root.Children.Add(FixedHeightRepeater(SliverGalleryData.CreateItems(300), 260, new SliverFixedExtentVirtualizingLayout { ItemExtent = 58, Spacing = 4 }, GalleryItemFactoryKind.Compact));
        root.Children.Add(SectionHeader("Responsive grid sliver", "A Flutter-inspired CustomScrollView composition using normal Uno panels plus sliver layouts."));
        root.Children.Add(FixedHeightRepeater(SliverGalleryData.CreateItems(480), 420, new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.MaxCrossAxisExtent,
            MaxCrossAxisExtent = 210,
            MainAxisSpacing = 10,
            CrossAxisSpacing = 10,
            ChildAspectRatio = 1.35
        }, GalleryItemFactoryKind.Grid));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            ScenarioNote("Nested preview repeaters are height-bounded so ItemsRepeater still receives a finite viewport and cache window."),
            new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brush(0xFFFFFFFF)
            });
    }

    private UIElement BuildTabbedNestedScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.TabbedNestedScroll);
        var results = CreateTabbedInnerScroll(_items.Skip(40).Take(180).ToArray());
        var saved = CreateTabbedInnerScroll(_items.Skip(320).Take(180).ToArray());
        var contentHost = new ContentControl();
        var resultsButton = SegmentButton("Results");
        var savedButton = SegmentButton("Saved");

        void Select(Button selected, Button other, UIElement content)
        {
            selected.Background = Brush(0xFF2563EB);
            selected.Foreground = Brush(0xFFFFFFFF);
            other.Background = Brush(0xFFFFFFFF);
            other.Foreground = Brush(0xFF334155);
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
            Background = Brush(0xFF1E3A8A),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock { Text = "NestedScrollView-style catalog", Foreground = Brush(0xFFFFFFFF), FontSize = 20, FontWeight = FontWeights.SemiBold },
                    new TextBlock { Text = "Pinned header plus tabbed inner scroll bodies.", Foreground = Brush(0xFFDBEAFE), FontSize = 12, TextWrapping = TextWrapping.Wrap }
                }
            }
        });

        var tabs = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Padding = new Thickness(12, 10, 12, 10),
            Background = Brush(0xFFFFFFFF),
            Children = { resultsButton, savedButton }
        };
        Grid.SetRow(tabs, 1);
        viewport.Children.Add(tabs);

        Grid.SetRow(contentHost, 2);
        viewport.Children.Add(contentHost);

        return Scenario(
            scenario.Title,
            scenario.Summary,
            ScenarioNote("Flutter uses NestedScrollView with SliverOverlapAbsorber/Injector for this pattern. Uno keeps this as native segmented tabs plus separate ItemsRepeater scroll bodies until a nested-scroll adapter exists."),
            viewport);
    }

    private static UIElement CreateTabbedInnerScroll(IReadOnlyList<GalleryItem> items)
    {
        return Viewport(CreateRepeater(
            items,
            new SliverFixedExtentVirtualizingLayout { ItemExtent = 52, Spacing = 4 },
            GalleryItemFactoryKind.Compact));
    }

    private UIElement BuildSectionedHeaderScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.SectionedHeaders);
        var stickyTitle = new TextBlock
        {
            Text = _sections[0].Title,
            Foreground = Brush(0xFFFFFFFF),
            FontWeight = FontWeights.SemiBold,
            FontSize = 16
        };

        var content = new StackPanel { Spacing = 18, Padding = new Thickness(16, 62, 16, 24) };
        foreach (var section in _sections)
        {
            content.Children.Add(SectionHeader(section.Title, section.Summary));
            content.Children.Add(FixedHeightRepeater(section.Items, 280, new SliverFixedExtentVirtualizingLayout { ItemExtent = 50, Spacing = 3 }, GalleryItemFactoryKind.Compact));
        }

        var scroller = new ScrollViewer
        {
            Content = content,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(0xFFFFFFFF)
        };

        scroller.ViewChanged += (_, _) =>
        {
            var index = Math.Clamp((int)(scroller.VerticalOffset / 390), 0, _sections.Count - 1);
            stickyTitle.Text = _sections[index].Title;
        };

        var overlay = new Border
        {
            Height = 46,
            Margin = new Thickness(12),
            Padding = new Thickness(16, 0, 16, 0),
            CornerRadius = new CornerRadius(8),
            Background = Brush(0xFF0F766E),
            VerticalAlignment = VerticalAlignment.Top,
            Child = stickyTitle
        };

        var layered = new Grid();
        layered.Children.Add(scroller);
        layered.Children.Add(overlay);

        return Scenario(
            scenario.Title,
            scenario.Summary,
            ScenarioNote("The sticky header is an Uno overlay in this sample; a future adapter can translate persistent-header geometry directly."),
            layered);
    }

    private UIElement BuildFillVisibilityScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.FillPaddingVisibility);
        var showDetails = new CheckBox
        {
            Content = "show replacement sliver content",
            IsChecked = true
        };

        var detail = FillPanel("Visible content", "SliverVisibility keeps this content in the composition when enabled.", 0xFFEEF2FF, 0xFF3730A3);
        var replacement = FillPanel("Replacement content", "The hidden branch can still reserve layout space or swap an alternate child.", 0xFFFFF7ED, 0xFFC2410C);

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
        replacement.Visibility = Visibility.Collapsed;

        var content = new StackPanel
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Border { Height = 44, Background = Brush(0xFFE0F2FE), CornerRadius = new CornerRadius(8) },
                detail,
                replacement,
                new Border
                {
                    MinHeight = 260,
                    Padding = new Thickness(20),
                    Background = Brush(0xFFF8FAFC),
                    BorderBrush = Brush(0xFFE2E8F0),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Child = SectionHeader("Fill remaining", "This block stretches the composition like SliverFillRemaining after padded content.")
                }
            }
        };

        return Scenario(
            scenario.Title,
            scenario.Summary,
            showDetails,
            new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brush(0xFFFFFFFF)
            });
    }

    private UIElement BuildCacheStressScenario()
    {
        var scenario = Scenario(GalleryScenarioKind.CacheStress);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 52,
            Spacing = 2
        };

        var repeater = CreateRepeater(_largeItems, layout, GalleryItemFactoryKind.Compact);
        repeater.VerticalCacheLength = 3;

        var realized = 0;
        var preparedTotal = 0;
        var status = new TextBlock
        {
            Text = "realized: 0 | prepared: 0 | source: 100,000",
            Foreground = Brush(0xFF344054),
            FontSize = 13
        };

        repeater.ElementPrepared += (_, _) =>
        {
            realized++;
            preparedTotal++;
            status.Text = $"realized: {realized:N0} | prepared: {preparedTotal:N0} | source: {_largeItems.Count:N0}";
        };

        repeater.ElementClearing += (_, _) =>
        {
            realized = Math.Max(0, realized - 1);
            status.Text = $"realized: {realized:N0} | prepared: {preparedTotal:N0} | source: {_largeItems.Count:N0}";
        };

        var scroller = new ScrollViewer
        {
            Content = repeater,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(0xFFFFFFFF)
        };

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(status);
        controls.Children.Add(ControlSlider("item extent", 36, 96, layout.ItemExtent, 1, value => layout.ItemExtent = value));
        controls.Children.Add(ControlSlider("spacing", 0, 16, layout.Spacing, 1, value => layout.Spacing = value));
        controls.Children.Add(ControlSlider("cache length", 0, 10, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));
        controls.Children.Add(JumpButtons(scroller, layout));

        return Scenario(
            scenario.Title,
            scenario.Summary,
            controls,
            scroller);
    }

    private static ItemsRepeater CreateRepeater(IReadOnlyList<GalleryItem> items, VirtualizingLayout layout, GalleryItemFactoryKind kind)
    {
        return new ItemsRepeater
        {
            ItemsSource = items,
            Layout = layout,
            ItemTemplate = new GalleryItemElementFactory(kind)
        };
    }

    private static IReadOnlyList<GalleryDataGridRow> ApplyDataGridQuery(
        IReadOnlyList<GalleryDataGridRow> rows,
        string filter,
        string sortKey,
        bool descending)
    {
        var filters = string.IsNullOrWhiteSpace(filter)
            ? Array.Empty<SliverDataGridFilterDescriptor>()
            : new[] { new SliverDataGridFilterDescriptor("search", SliverDataGridFilterOperator.Contains, filter) };
        var projected = SliverDataGridQueryEngine.ProjectRows(
            rows,
            DataGridBindings,
            new SliverDataGridQuery(
                new[] { new SliverDataGridSortDescriptor(sortKey, descending ? SliverDataGridSortDirection.Descending : SliverDataGridSortDirection.Ascending) },
                filters));
        return projected.Select(index => rows[index]).ToArray();
    }

    private static UIElement DataGridViewport(ItemsRepeater repeater)
    {
        var headerScroller = new ScrollViewer
        {
            Content = DataGridHeader(),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(0xFFFFFFFF),
            IsTabStop = false
        };

        var rowsHost = new Grid
        {
            MinWidth = DataGridTableWidth,
            Background = Brush(0xFFFFFFFF)
        };
        rowsHost.Children.Add(repeater);

        var bodyScroller = new ScrollViewer
        {
            Content = rowsHost,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = Brush(0xFFFFFFFF)
        };
        bodyScroller.ViewChanged += (_, _) =>
        {
            headerScroller.ChangeView(bodyScroller.HorizontalOffset, null, null, disableAnimation: true);
        };

        var root = new Grid
        {
            Background = Brush(0xFFFFFFFF)
        };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.Children.Add(headerScroller);
        Grid.SetRow(bodyScroller, 1);
        root.Children.Add(bodyScroller);
        return root;
    }

    private static UIElement DataGridHeader()
    {
        var grid = CreateDataGridColumns();
        grid.MinWidth = DataGridTableWidth;
        grid.Background = Brush(0xFFE2E8F0);
        grid.Padding = new Thickness(10, 8, 10, 8);

        var headers = new[] { "ID", "Account", "Region", "Category", "Status", "Owner", "Amount", "Progress", "Updated", "Notes" };
        for (var index = 0; index < headers.Length; index++)
        {
            var text = new TextBlock
            {
                Text = headers[index],
                Foreground = Brush(0xFF334155),
                FontWeight = FontWeights.SemiBold,
                FontSize = 13
            };
            Grid.SetColumn(text, index);
            grid.Children.Add(text);
        }

        return grid;
    }

    private static Grid CreateDataGridColumns()
    {
        var grid = new Grid { ColumnSpacing = 10 };
        foreach (var width in new[] { 84d, 180d, 118d, 168d, 118d, 150d, 120d, 130d, 132d, 360d })
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width) });
        }

        return grid;
    }

    private static readonly IReadOnlyList<SliverDataGridColumnBinding<GalleryDataGridRow>> DataGridBindings =
    [
        new("account", row => row.Account),
        new("region", row => row.Region),
        new("category", row => row.Category),
        new("status", row => row.Status),
        new("owner", row => row.Owner),
        new("amount", row => row.Amount),
        new("progress", row => row.Progress),
        new("updated", row => row.Updated),
        new("search", row => $"{row.Account} {row.Region} {row.Category} {row.Status} {row.Owner} {row.Notes}")
    ];

    private static UIElement FixedHeightRepeater(IReadOnlyList<GalleryItem> items, double height, VirtualizingLayout layout, GalleryItemFactoryKind kind)
    {
        return new Border
        {
            Height = height,
            BorderBrush = Brush(0xFFE4E7EC),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = Viewport(CreateRepeater(items, layout, kind))
        };
    }

    private static UIElement ScenarioNote(string text)
    {
        return new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush(0xFF475467),
            FontSize = 13
        };
    }

    private static UIElement FillPanel(string title, string detail, uint background, uint foreground)
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
                    new TextBlock
                    {
                        Text = title,
                        Foreground = Brush(foreground),
                        FontSize = 18,
                        FontWeight = FontWeights.SemiBold
                    },
                    new TextBlock
                    {
                        Text = detail,
                        Foreground = Brush(0xFF475467),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };
    }

    private static UIElement Scenario(string title, string description, UIElement controls, UIElement viewport)
    {
        var root = new Grid
        {
            ColumnSpacing = 20
        };

        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var titleStack = new StackPanel { Spacing = 6 };
        titleStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 22,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brush(0xFF111827)
        });
        titleStack.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush(0xFF475467)
        });
        titleStack.Children.Add(controls);

        var controlPanel = new Border
        {
            Padding = new Thickness(18),
            CornerRadius = new CornerRadius(8),
            Background = Brush(0xFFFFFFFF),
            BorderBrush = Brush(0xFFD0D5DD),
            BorderThickness = new Thickness(1),
            Child = titleStack
        };

        root.Children.Add(controlPanel);

        var viewportFrame = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderBrush = Brush(0xFFD0D5DD),
            BorderThickness = new Thickness(1),
            Background = Brush(0xFFE8EEF7),
            Child = viewport
        };

        Grid.SetColumn(viewportFrame, 1);
        root.Children.Add(viewportFrame);

        return root;
    }

    private static UIElement Viewport(UIElement content)
    {
        return new ScrollViewer
        {
            Content = content,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(0xFFFFFFFF)
        };
    }

    private static UIElement ControlSlider(string label, double minimum, double maximum, double value, double step, Action<double> changed)
    {
        var valueText = new TextBlock
        {
            Text = FormatSliderValue(value, step),
            Foreground = Brush(0xFF667085),
            MinWidth = 48,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var header = new Grid();
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        header.Children.Add(new TextBlock
        {
            Text = label,
            Foreground = Brush(0xFF344054),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold
        });

        Grid.SetColumn(valueText, 1);
        header.Children.Add(valueText);

        var slider = new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            StepFrequency = step,
            SmallChange = step,
            LargeChange = Math.Max(step, (maximum - minimum) / 8)
        };

        slider.ValueChanged += (_, args) =>
        {
            var next = step >= 1 ? Math.Round(args.NewValue / step) * step : args.NewValue;
            valueText.Text = FormatSliderValue(next, step);
            changed(next);
        };

        var root = new StackPanel { Spacing = 4 };
        root.Children.Add(header);
        root.Children.Add(slider);
        return root;
    }

    private static UIElement JumpButtons(ScrollViewer scroller, SliverFixedExtentVirtualizingLayout layout)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };

        row.Children.Add(JumpButton("top", scroller, 0));
        row.Children.Add(JumpButton("middle", scroller, 50_000 * (layout.ItemExtent + layout.Spacing)));
        row.Children.Add(JumpButton("end", scroller, 99_500 * (layout.ItemExtent + layout.Spacing)));
        return row;
    }

    private static Button JumpButton(string label, ScrollViewer scroller, double offset)
    {
        var button = new Button
        {
            Content = label,
            Padding = new Thickness(12, 7, 12, 7)
        };

        button.Click += (_, _) => scroller.ChangeView(null, offset, null, disableAnimation: true);
        return button;
    }

    private static Button SegmentButton(string label)
    {
        return new Button
        {
            Content = label,
            Padding = new Thickness(12, 7, 12, 7),
            BorderBrush = Brush(0xFFCBD5E1),
            BorderThickness = new Thickness(1)
        };
    }

    private static UIElement HeroPanel()
    {
        var panel = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(22),
            Background = Brush(0xFF172033),
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Flutter-style sliver composition for Uno",
                        Foreground = Brush(0xFFFFFFFF),
                        FontSize = 24,
                        FontWeight = FontWeights.SemiBold
                    },
                    new TextBlock
                    {
                        Text = "Use normal framework controls for content, and let SliverWidgets map viewport constraints into deterministic list and grid geometry.",
                        Foreground = Brush(0xFFD0D5DD),
                        FontSize = 14,
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };

        return panel;
    }

    private static UIElement SummaryBand()
    {
        var metrics = SliverGalleryData.CreateMetrics().Take(3).ToArray();
        var grid = new Grid
        {
            ColumnSpacing = 12
        };

        for (var i = 0; i < 3; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        for (var i = 0; i < metrics.Length; i++)
        {
            AddMetric(grid, i, metrics[i]);
        }

        return grid;
    }

    private static void AddMetric(Grid grid, int column, GalleryMetric metric)
    {
        var metricCard = new Border
        {
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8),
            Background = Brush(0xFFEFF6FF),
            Child = new StackPanel
            {
                Spacing = 4,
                Children =
                {
                    new TextBlock
                    {
                        Text = metric.Label,
                        Foreground = Brush(0xFF1D4ED8),
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold
                    },
                    new TextBlock
                    {
                        Text = metric.Value,
                        Foreground = Brush(0xFF1E293B),
                        FontSize = 20,
                        FontWeight = FontWeights.SemiBold
                    }
                }
            }
        };

        Grid.SetColumn(metricCard, column);
        grid.Children.Add(metricCard);
    }

    private static UIElement SectionHeader(string title, string subtitle)
    {
        return new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    Foreground = Brush(0xFF111827),
                    FontSize = 19,
                    FontWeight = FontWeights.SemiBold
                },
                new TextBlock
                {
                    Text = subtitle,
                    Foreground = Brush(0xFF667085),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static string FormatSliderValue(double value, double step)
    {
        return step >= 1 ? value.ToString("N0") : value.ToString("0.##");
    }

    private static SolidColorBrush Brush(uint argb)
    {
        return new SolidColorBrush(Color.FromArgb(
            (byte)(argb >> 24),
            (byte)(argb >> 16),
            (byte)(argb >> 8),
            (byte)argb));
    }

    private static SolidColorBrush BrushFromHex(string hex)
    {
        var value = Convert.ToUInt32(hex.TrimStart('#'), 16);
        return Brush(0xFF000000 | value);
    }

    private sealed record GalleryPage(GalleryScenario Scenario, Func<UIElement> Create);
}

internal sealed class GalleryItemElementFactory : ElementFactory
{
    private readonly GalleryItemFactoryKind _kind;
    private readonly Stack<UIElement> _recyclePool = new();

    public GalleryItemElementFactory(GalleryItemFactoryKind kind)
    {
        _kind = kind;
    }

    protected override UIElement GetElementCore(Microsoft.UI.Xaml.Controls.ElementFactoryGetArgs args)
    {
        var element = _recyclePool.Count > 0 ? _recyclePool.Pop() : CreateElement();
        if (args.Data is GalleryItem item)
        {
            UpdateElement(element, item);
        }

        return element;
    }

    protected override void RecycleElementCore(Microsoft.UI.Xaml.Controls.ElementFactoryRecycleArgs args)
    {
        if (args.Element is not null)
        {
            _recyclePool.Push(args.Element);
        }
    }

    private UIElement CreateElement()
    {
        return _kind switch
        {
            GalleryItemFactoryKind.Grid => CreateGridTile(),
            GalleryItemFactoryKind.Variable => CreateVariableRow(),
            GalleryItemFactoryKind.Compact => CreateCompactRow(),
            GalleryItemFactoryKind.Stack => CreateStackCard(),
            GalleryItemFactoryKind.Wrap => CreateWrapChip(),
            _ => CreateListRow()
        };
    }

    private static UIElement CreateListRow()
    {
        return new Border
        {
            Margin = new Thickness(8, 0, 8, 0),
            Padding = new Thickness(14, 8, 14, 8),
            CornerRadius = new CornerRadius(8),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(1),
            Child = new Grid
            {
                ColumnSpacing = 12,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    CreateBadge(),
                    CreateTextStack(),
                    CreateMetricText()
                }
            }
        };
    }

    private static UIElement CreateCompactRow()
    {
        return new Border
        {
            Margin = new Thickness(8, 0, 8, 0),
            Padding = new Thickness(12, 6, 12, 6),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = new Grid
            {
                ColumnSpacing = 10,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    CreateBadge(),
                    CreateTextStack(),
                    CreateMetricText()
                }
            }
        };
    }

    private static UIElement CreateVariableRow()
    {
        return new Border
        {
            Margin = new Thickness(8, 0, 8, 0),
            Padding = new Thickness(14, 10, 14, 10),
            CornerRadius = new CornerRadius(8),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(1),
            Child = new Grid
            {
                ColumnSpacing = 12,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Children =
                {
                    CreateBadge(),
                    CreateTextStack(),
                    CreateMetricText()
                }
            }
        };
    }


    private static UIElement CreateGridTile()
    {
        return new Border
        {
            Margin = new Thickness(0),
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(8),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(1),
            Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    CreateBadge(),
                    CreateTextStack(),
                    CreateMetricText()
                }
            }
        };
    }

    private static UIElement CreateStackCard()
    {
        return new Border
        {
            Margin = new Thickness(0),
            Padding = new Thickness(10),
            CornerRadius = new CornerRadius(8),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(1),
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
                        CornerRadius = new CornerRadius(2),
                        Background = UnoGalleryPageBrushes.BadgeText
                    },
                    CreateTextStack(),
                    CreateMetricText()
                }
            }
        };
    }

    private static UIElement CreateWrapChip()
    {
        return new Border
        {
            Margin = new Thickness(0),
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(1),
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
                        Background = UnoGalleryPageBrushes.BadgeText
                    },
                    CreateTextStack()
                }
            }
        };
    }

    private static Border CreateBadge()
    {
        return new Border
        {
            Width = 38,
            Height = 30,
            CornerRadius = new CornerRadius(6),
            Background = UnoGalleryPageBrushes.Badge,
            Child = new TextBlock
            {
                Name = "IdText",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = UnoGalleryPageBrushes.BadgeText,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12
            }
        };
    }

    private static StackPanel CreateTextStack()
    {
        var stack = new StackPanel
        {
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center
        };

        stack.Children.Add(new TextBlock
        {
            Name = "TitleText",
            Foreground = UnoGalleryPageBrushes.Title,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        stack.Children.Add(new TextBlock
        {
            Name = "SubtitleText",
            Foreground = UnoGalleryPageBrushes.Subtitle,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        Grid.SetColumn(stack, 1);
        return stack;
    }

    private static TextBlock CreateMetricText()
    {
        var metric = new TextBlock
        {
            Name = "MetricText",
            Foreground = UnoGalleryPageBrushes.Metric,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(metric, 2);
        return metric;
    }

    private void UpdateElement(UIElement element, GalleryItem item)
    {
        if (element is FrameworkElement frameworkElement)
        {
            frameworkElement.Tag = item;
            if (frameworkElement is Border border)
            {
                border.MinHeight = _kind == GalleryItemFactoryKind.Variable ? item.Extent : 0d;
            }
        }

        if (FindByName<TextBlock>(element, "IdText") is { } id)
        {
            id.Text = item.Id.ToString("N0");
        }

        if (FindByName<TextBlock>(element, "TitleText") is { } title)
        {
            title.Text = item.Title;
        }

        if (FindByName<TextBlock>(element, "SubtitleText") is { } subtitle)
        {
            subtitle.Text = item.Subtitle;
        }

        if (FindByName<TextBlock>(element, "MetricText") is { } metric)
        {
            metric.Text = $"{item.Extent:0}px";
        }
    }

    private static T? FindByName<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        if (root is T element && element.Name == name)
        {
            return element;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var match = FindByName<T>(child, name);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}

internal sealed class DataGridRowElementFactory : ElementFactory
{
    private readonly Stack<UIElement> _recyclePool = new();

    protected override UIElement GetElementCore(Microsoft.UI.Xaml.Controls.ElementFactoryGetArgs args)
    {
        var element = _recyclePool.Count > 0 ? _recyclePool.Pop() : CreateRow();
        if (args.Data is GalleryDataGridRow row)
        {
            UpdateRow(element, row);
        }

        return element;
    }

    protected override void RecycleElementCore(Microsoft.UI.Xaml.Controls.ElementFactoryRecycleArgs args)
    {
        if (args.Element is not null)
        {
            _recyclePool.Push(args.Element);
        }
    }

    private static UIElement CreateRow()
    {
        var border = new Border
        {
            Padding = new Thickness(10, 6, 10, 6),
            Background = UnoGalleryPageBrushes.White,
            BorderBrush = UnoGalleryPageBrushes.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Child = CreateGrid()
        };
        return border;
    }

    private static Grid CreateGrid()
    {
        var grid = new Grid { ColumnSpacing = 10 };
        foreach (var width in new[] { 84d, 180d, 118d, 168d, 118d, 150d, 120d, 130d, 132d, 360d })
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(width) });
        }

        var names = new[] { "Id", "Account", "Region", "Category", "Status", "Owner", "Amount", "Progress", "Updated", "Notes" };
        for (var index = 0; index < names.Length; index++)
        {
            var text = new TextBlock
            {
                Name = names[index],
                Foreground = index is 1 or 6 ? UnoGalleryPageBrushes.Title : UnoGalleryPageBrushes.Subtitle,
                FontWeight = index is 1 or 6 ? FontWeights.SemiBold : FontWeights.Normal,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = index == 9 ? TextWrapping.Wrap : TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(text, index);
            grid.Children.Add(text);
        }

        return grid;
    }

    private static void UpdateRow(UIElement element, GalleryDataGridRow row)
    {
        if (element is FrameworkElement frameworkElement)
        {
            frameworkElement.MinHeight = row.Extent;
            frameworkElement.Tag = row;
        }

        SetText(element, "Id", row.Id.ToString("N0"));
        SetText(element, "Account", row.Account);
        SetText(element, "Region", row.Region);
        SetText(element, "Category", row.Category);
        SetText(element, "Status", row.Status);
        SetText(element, "Owner", row.Owner);
        SetText(element, "Amount", row.Amount.ToString("C0"));
        SetText(element, "Progress", $"{row.Progress}%");
        SetText(element, "Updated", row.Updated.ToString("yyyy-MM-dd"));
        SetText(element, "Notes", row.Notes);
    }

    private static void SetText(UIElement element, string name, string value)
    {
        if (FindByName<TextBlock>(element, name) is { } text)
        {
            text.Text = value;
        }
    }

    private static T? FindByName<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        if (root is T element && element.Name == name)
        {
            return element;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            var match = FindByName<T>(child, name);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }
}

internal enum GalleryItemFactoryKind
{
    List,
    Compact,
    Grid,
    Variable,
    Stack,
    Wrap
}

internal static class UnoGalleryPageBrushes
{
    public static readonly SolidColorBrush White = Brush(0xFFFFFFFF);
    public static readonly SolidColorBrush Border = Brush(0xFFE4E7EC);
    public static readonly SolidColorBrush Badge = Brush(0xFFEFF6FF);
    public static readonly SolidColorBrush BadgeText = Brush(0xFF1D4ED8);
    public static readonly SolidColorBrush Title = Brush(0xFF1F2937);
    public static readonly SolidColorBrush Subtitle = Brush(0xFF667085);
    public static readonly SolidColorBrush Metric = Brush(0xFF0F766E);

    private static SolidColorBrush Brush(uint argb)
    {
        return new SolidColorBrush(Color.FromArgb(
            (byte)(argb >> 24),
            (byte)(argb >> 16),
            (byte)(argb >> 8),
            (byte)argb));
    }
}
