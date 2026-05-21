using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SliverWidgets.Core;
using SliverWidgets.GalleryData;
using SliverWidgets.Maui;
using MauiSliverStackLayout = SliverWidgets.Maui.SliverStackLayout;

namespace SliverWidgets.MauiGallery;

public sealed class MainPage : ContentPage
{
    private const double InitialExtent = 64d;
    private const double InitialSpacing = 8d;
    private const double InitialCacheExtent = 240d;
    private const double AdaptiveTileExtent = 118d;
    private const double AdaptiveTileMaxWidth = 190d;

    private readonly IReadOnlyList<GalleryScenario> _scenarios = SliverGalleryData.CreateScenarios();
    private readonly IReadOnlyList<GalleryItem> _items = SliverGalleryData.CreateItems(100_000);
    private readonly IReadOnlyList<GalleryItem> _stackItems = SliverGalleryData.CreateStackItems(100_000);
    private readonly IReadOnlyList<GalleryItem> _wrapItems = SliverGalleryData.CreateWrapItems(100_000);
    private readonly IReadOnlyList<GalleryDataGridRow> _dataGridRows = SliverGalleryData.CreateDataGridRows(100_000);
    private readonly IReadOnlyList<GalleryItem> _stressItems = SliverGalleryData.CreateUniformItems(100_000, 52d);
    private readonly IReadOnlyList<MauiGallerySection> _sections = SliverGalleryData
        .CreateSections(8, 60)
        .Select(section => new MauiGallerySection(section))
        .ToArray();
    private readonly MauiSliverStackLayout _fixedList;
    private readonly SliverCollectionView _largeList;
    private readonly CollectionView _variableList;
    private readonly CollectionView _stackList;
    private readonly SliverCollectionView _adaptiveGrid;
    private readonly CollectionView _dataGridRowsView;
    private readonly View _dataGridViewport;
    private readonly CollectionView _wrapList;
    private readonly SliverCollectionView _sectionList;
    private readonly SliverCollectionView _stressList;
    private readonly ContentView _scenarioHost = new();
    private readonly List<Button> _tabButtons = [];
    private readonly Label _extentValue;
    private readonly Label _spacingValue;
    private readonly Label _cacheValue;
    private readonly Label _gridColumnsValue;

    public MainPage()
    {
        Title = "SliverWidgets MAUI Gallery";
        BackgroundColor = Color.FromArgb("#F7F8FA");
        Padding = new Thickness(0);

        _extentValue = CreateValueLabel($"{InitialExtent:0}px");
        _spacingValue = CreateValueLabel($"{InitialSpacing:0}px");
        _cacheValue = CreateValueLabel($"{InitialCacheExtent:0}px");
        _gridColumnsValue = CreateValueLabel("3");
        _fixedList = CreateFixedStackPreview();
        _largeList = CreateLargeList();
        _variableList = CreateVariableList();
        _stackList = CreateStackList();
        _adaptiveGrid = CreateAdaptiveGrid();
        _dataGridRowsView = CreateDataGridRowsView();
        _dataGridViewport = CreateDataGridViewport(_dataGridRowsView);
        _wrapList = CreateWrapList();
        _sectionList = CreateSectionList();
        _stressList = CreateStressList();

        var pages = CreateGalleryPages();
        Content = CreateShell(pages);
        ShowScenario(pages[0]);
    }

    private View CreateHeader()
    {
        return new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        CreateLabel("SliverWidgets MAUI Gallery", 30, FontAttributes.Bold, "#111827"),
                        CreateLabel(
                            "Unified Flutter-inspired sliver scenario catalog using native MAUI controls.",
                            15,
                            FontAttributes.None,
                            "#4B5563")
                    }
                },
                WithColumn(CreateMetrics(), 1)
            }
        };
    }

    private View CreateShell(IReadOnlyList<MauiGalleryPage> pages)
    {
        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        root.Add(new Border
        {
            Padding = new Thickness(20, 16),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#D8E0EC"),
            StrokeThickness = 1,
            Content = CreateHeader()
        }, 0, 0);

        root.Add(CreateTabs(pages), 0, 1);
        root.Add(new Grid
        {
            Padding = new Thickness(24, 20),
            Children = { _scenarioHost }
        }, 0, 2);

        return root;
    }

    private View CreateTabs(IReadOnlyList<MauiGalleryPage> pages)
    {
        var tabs = new HorizontalStackLayout
        {
            Spacing = 18,
            Padding = new Thickness(24, 18, 24, 14)
        };

        foreach (var page in pages)
        {
            var button = new Button
            {
                Text = page.Scenario.TabLabel,
                FontSize = 18,
                Padding = new Thickness(10, 6),
                BackgroundColor = Colors.Transparent,
                TextColor = Color.FromArgb("#6B7280"),
                BorderColor = Colors.Transparent,
                BorderWidth = 0
            };

            button.Clicked += (_, _) => ShowScenario(page);
            _tabButtons.Add(button);
            tabs.Children.Add(button);
        }

        return new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            BackgroundColor = Color.FromArgb("#F3F6FA"),
            Content = tabs
        };
    }

    private IReadOnlyList<MauiGalleryPage> CreateGalleryPages()
    {
        return
        [
            CreatePage(GalleryScenarioKind.FixedExtentList, CreateFixedControls(), _largeList),
            CreatePage(
                GalleryScenarioKind.VariableExtentList,
                CreateNote("Native MAUI CollectionView measures each preview row for this non-uniform sample."),
                _variableList),
            CreatePage(
                GalleryScenarioKind.VariableStack,
                CreateNote("MAUI uses native CollectionView virtualization for 100,000 variable-width/height stack cards from the shared deterministic stack feed."),
                _stackList),
            CreatePage(
                GalleryScenarioKind.AdaptiveGrid,
                CreateGridControls(),
                _adaptiveGrid),
            CreatePage(
                GalleryScenarioKind.DataGrid,
                CreateDataGridControls(),
                _dataGridViewport),
            CreatePage(
                GalleryScenarioKind.VariableWrap,
                CreateNote("MAUI projects the same 100,000-item wrap feed as virtualized native rows; each row contains variable-size chip controls produced from shared deterministic extents."),
                _wrapList),
            CreatePage(
                GalleryScenarioKind.PinnedHeader,
                CreateNote("A native overlay follows CollectionView.Scrolled offsets while the adapter surface stays thin."),
                CreatePinnedHeaderDemo()),
            CreatePage(
                GalleryScenarioKind.TabbedNestedScroll,
                CreateNote("Flutter uses NestedScrollView with SliverOverlapAbsorber/Injector for this pattern. MAUI projects it with native segmented tab buttons and separate CollectionView scroll bodies."),
                CreateTabbedNestedDemo()),
            CreatePage(
                GalleryScenarioKind.MixedComposition,
                CreateNote("Ordinary MAUI controls, fixed rows, grid tiles, and fill content are composed in one native ScrollView."),
                CreateMixedComposition()),
            CreatePage(
                GalleryScenarioKind.SectionedHeaders,
                CreateNote("Grouped SliverCollectionView rows use native handler behavior for platform-specific header stickiness."),
                _sectionList),
            CreatePage(
                GalleryScenarioKind.FillPaddingVisibility,
                CreateNote("MAUI layout primitives model utility sliver behavior for padding, replacement content, and fill remaining."),
                CreateFillPaddingVisibilityDemo()),
            CreatePage(
                GalleryScenarioKind.CacheStress,
                CreateCacheControls(),
                CreateStressDemo())
        ];
    }

    private MauiGalleryPage CreatePage(GalleryScenarioKind kind, View controls, View viewport)
    {
        var scenario = Scenario(kind);
        return new MauiGalleryPage(scenario, CreateScenarioPage(scenario, controls, viewport));
    }

    private View CreateScenarioPage(GalleryScenario scenario, View controls, View viewport)
    {
        var layout = new Grid
        {
            ColumnSpacing = 20,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(300)),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star)
            }
        };

        layout.Add(CreateScenarioPanel(scenario, controls), 0, 0);
        layout.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#E8EEF7"),
            Stroke = Color.FromArgb("#CBD5E1"),
            StrokeThickness = 1,
            Content = viewport
        }, 1, 0);

        return layout;
    }

    private View CreateScenarioPanel(GalleryScenario scenario, View controls)
    {
        return new Border
        {
            Padding = new Thickness(18),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#CBD5E1"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    CreateLabel(scenario.Title, 20, FontAttributes.Bold, "#111827"),
                    CreateLabel(scenario.Summary, 13, FontAttributes.None, "#4B5563"),
                    controls
                }
            }
        };
    }

    private void ShowScenario(MauiGalleryPage page)
    {
        for (var index = 0; index < _tabButtons.Count; index++)
        {
            var button = _tabButtons[index];
            var selected = string.Equals(button.Text, page.Scenario.TabLabel, StringComparison.Ordinal);
            button.TextColor = selected ? Color.FromArgb("#111827") : Color.FromArgb("#6B7280");
            button.BorderWidth = selected ? 2 : 0;
            button.BorderColor = selected ? Color.FromArgb("#2563EB") : Colors.Transparent;
        }

        _scenarioHost.Content = page.Content;
    }

    private View CreateMetrics()
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 12
        };

        for (var column = 0; column < 4; column++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        var metrics = SliverGalleryData.CreateMetrics();
        for (var index = 0; index < metrics.Count; index++)
        {
            grid.Add(CreateMetricCard(metrics[index]), index, 0);
        }

        return grid;
    }

    private View CreateFixedControls()
    {
        var extentSlider = CreateSlider(44, 96, InitialExtent);
        extentSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _fixedList.ItemExtent = value;
            _largeList.ItemExtent = value;
            _stressList.ItemExtent = value;
            _extentValue.Text = $"{value:0}px";
        };

        var spacingSlider = CreateSlider(0, 20, InitialSpacing);
        spacingSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _fixedList.Spacing = value;
            _largeList.Spacing = value;
            ((LinearItemsLayout)_variableList.ItemsLayout).ItemSpacing = value;
            _adaptiveGrid.MainAxisSpacing = value;
            _adaptiveGrid.CrossAxisSpacing = value;
            _sectionList.Spacing = value;
            _stressList.Spacing = value;
            _spacingValue.Text = $"{value:0}px";
        };

        var cacheSlider = CreateSlider(0, 640, InitialCacheExtent);
        cacheSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _largeList.CacheExtent = value;
            _adaptiveGrid.CacheExtent = value;
            _sectionList.CacheExtent = value;
            _stressList.CacheExtent = value;
            _cacheValue.Text = $"{value:0}px";
        };

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateNote("The sliders update live MAUI sliver controls. Cache extent remains metadata because MAUI handlers own the realization window."),
                new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition(GridLength.Auto),
                        new RowDefinition(GridLength.Auto),
                        new RowDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        CreateControlRow("Extent", _extentValue, extentSlider, 0),
                        CreateControlRow("Spacing", _spacingValue, spacingSlider, 1),
                        CreateControlRow("Cache", _cacheValue, cacheSlider, 2)
                    }
                }
            }
        };
    }

    private View CreateGridControls()
    {
        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateNote("The MAUI sample recalculates fixed column count from available width while keeping the shared adaptive-grid scenario data."),
                new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        CreateReadOnlyRow("Grid columns", _gridColumnsValue, 0)
                    }
                }
            }
        };
    }

    private View CreateDataGridControls()
    {
        var filter = new Entry
        {
            Placeholder = "Filter account, status, owner..."
        };
        var sort = new Picker
        {
            ItemsSource = new List<string> { "amount", "updated", "account", "status", "region", "progress" },
            SelectedItem = "amount"
        };
        var descending = new CheckBox
        {
            IsChecked = true
        };
        var visible = CreateValueLabel("0");

        void Refresh()
        {
            var projected = ApplyDataGridQuery(
                _dataGridRows,
                filter.Text ?? string.Empty,
                Convert.ToString(sort.SelectedItem, CultureInfo.InvariantCulture) ?? "amount",
                descending.IsChecked);
            _dataGridRowsView.ItemsSource = projected;
            visible.Text = projected.Count.ToString("N0", CultureInfo.InvariantCulture);
        }

        filter.TextChanged += (_, _) => Refresh();
        sort.SelectedIndexChanged += (_, _) => Refresh();
        descending.CheckedChanged += (_, _) => Refresh();
        Refresh();

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateNote("Rows use native CollectionView virtualization with variable heights. Core DataGrid query projection supplies sorting/filtering over the shared 100,000-row source."),
                CreateLabel("Filter", 12, FontAttributes.None, "#4B5563"),
                filter,
                CreateLabel("Sort column", 12, FontAttributes.None, "#4B5563"),
                sort,
                new HorizontalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        descending,
                        CreateLabel("Descending", 13, FontAttributes.None, "#111827")
                    }
                },
                CreateReadOnlyRow("Visible rows", visible, 0)
            }
        };
    }

    private View CreateCacheControls()
    {
        var cacheValue = CreateValueLabel($"{InitialCacheExtent:0}px");
        var cacheSlider = CreateSlider(0, 640, InitialCacheExtent);
        cacheSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _stressList.CacheExtent = value;
            cacheValue.Text = $"{value:0}px";
        };

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateNote("SliverCollectionView keeps MAUI's native recycling path while exposing sliver cache intent for parity."),
                new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        CreateControlRow("Cache", cacheValue, cacheSlider, 0)
                    }
                }
            }
        };
    }

    private static View CreateNote(string text)
    {
        return CreateLabel(text, 13, FontAttributes.None, "#4B5563");
    }

    private View CreateScenarioCatalog()
    {
        var layout = new Grid
        {
            ColumnSpacing = 12,
            RowSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        for (var index = 0; index < _scenarios.Count; index++)
        {
            var row = index / 2;
            if (layout.RowDefinitions.Count <= row)
            {
                layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            layout.Add(CreateScenarioCard(_scenarios[index]), index % 2, row);
        }

        return CreatePanel("Unified scenario catalog", "MAUI exposes the same scenario vocabulary as the other galleries while documenting where native platform behavior owns the details.", layout);
    }

    private MauiSliverStackLayout CreateFixedStackPreview()
    {
        var layout = new MauiSliverStackLayout
        {
            Axis = SliverAxis.Vertical,
            ItemExtent = InitialExtent,
            Spacing = InitialSpacing,
            HeightRequest = 430
        };

        foreach (var item in _items.Take(9))
        {
            layout.Children.Add(CreateFixedRow(item));
        }

        return layout;
    }

    private SliverCollectionView CreateLargeList()
    {
        return new SliverCollectionView
        {
            Axis = SliverAxis.Vertical,
            LayoutMode = SliverCollectionLayoutMode.FixedExtentList,
            ItemExtent = InitialExtent,
            Spacing = InitialSpacing,
            CacheExtent = InitialCacheExtent,
            HeightRequest = 480,
            ItemsSource = _items,
            ItemTemplate = new DataTemplate(CreateListCell)
        };
    }

    private CollectionView CreateVariableList()
    {
        return new CollectionView
        {
            HeightRequest = 480,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = InitialSpacing
            },
            ItemsSource = _items.Take(220).ToArray(),
            ItemTemplate = new DataTemplate(CreateVariableListCell)
        };
    }

    private CollectionView CreateStackList()
    {
        return new CollectionView
        {
            HeightRequest = 520,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = InitialSpacing
            },
            ItemsSource = _stackItems,
            ItemTemplate = new DataTemplate(CreateStackCell)
        };
    }

    private SliverCollectionView CreateAdaptiveGrid()
    {
        var grid = new SliverCollectionView
        {
            Axis = SliverAxis.Vertical,
            LayoutMode = SliverCollectionLayoutMode.FixedExtentGrid,
            CrossAxisCount = 3,
            MainAxisSpacing = InitialSpacing,
            CrossAxisSpacing = InitialSpacing,
            CacheExtent = InitialCacheExtent,
            HeightRequest = 430,
            ItemsSource = _items.Take(1200).ToArray(),
            ItemTemplate = new DataTemplate(CreateGridCell)
        };

        grid.SizeChanged += (_, _) =>
        {
            if (grid.Width <= 0d)
            {
                return;
            }

            var columns = Math.Clamp((int)Math.Floor(grid.Width / AdaptiveTileMaxWidth), 1, 6);
            if (columns == grid.CrossAxisCount)
            {
                return;
            }

            grid.CrossAxisCount = columns;
            _gridColumnsValue.Text = columns.ToString(CultureInfo.InvariantCulture);
        };

        return grid;
    }

    private CollectionView CreateDataGridRowsView()
    {
        return new CollectionView
        {
            HeightRequest = 520,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
            ItemsSource = ApplyDataGridQuery(_dataGridRows, string.Empty, "amount", descending: true),
            ItemTemplate = new DataTemplate(CreateDataGridRowCell)
        };
    }

    private static View CreateDataGridViewport(CollectionView rows)
    {
        return new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = new Grid
            {
                WidthRequest = 1580,
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star)
                },
                Children =
                {
                    CreateDataGridHeader(),
                    WithRow(rows, 1)
                }
            }
        };
    }

    private static Grid CreateDataGridHeader()
    {
        var grid = CreateDataGridColumns();
        grid.Padding = new Thickness(10, 8);
        grid.BackgroundColor = Color.FromArgb("#E2E8F0");

        var headers = new[] { "ID", "Account", "Region", "Category", "Status", "Owner", "Amount", "Progress", "Updated", "Notes" };
        for (var index = 0; index < headers.Length; index++)
        {
            grid.Add(CreateLabel(headers[index], 12, FontAttributes.Bold, "#334155", column: index));
        }

        return grid;
    }

    private static Grid CreateDataGridColumns()
    {
        var grid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(84)),
                new ColumnDefinition(new GridLength(180)),
                new ColumnDefinition(new GridLength(118)),
                new ColumnDefinition(new GridLength(168)),
                new ColumnDefinition(new GridLength(118)),
                new ColumnDefinition(new GridLength(150)),
                new ColumnDefinition(new GridLength(120)),
                new ColumnDefinition(new GridLength(130)),
                new ColumnDefinition(new GridLength(132)),
                new ColumnDefinition(new GridLength(360))
            }
        };

        return grid;
    }

    private CollectionView CreateWrapList()
    {
        return new CollectionView
        {
            HeightRequest = 520,
            ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 10
            },
            ItemsSource = CreateWrapRows(_wrapItems, 760d),
            ItemTemplate = new DataTemplate(() => new WrapRowView())
        };
    }

    private SliverCollectionView CreateSectionList()
    {
        return new SliverCollectionView
        {
            Axis = SliverAxis.Vertical,
            LayoutMode = SliverCollectionLayoutMode.FixedExtentList,
            ItemExtent = InitialExtent,
            Spacing = InitialSpacing,
            CacheExtent = InitialCacheExtent,
            IsGrouped = true,
            HeightRequest = 520,
            ItemsSource = _sections,
            GroupHeaderTemplate = new DataTemplate(CreateSectionHeader),
            ItemTemplate = new DataTemplate(CreateListCell)
        };
    }

    private SliverCollectionView CreateStressList()
    {
        return new SliverCollectionView
        {
            Axis = SliverAxis.Vertical,
            LayoutMode = SliverCollectionLayoutMode.FixedExtentList,
            ItemExtent = 52d,
            Spacing = 2d,
            CacheExtent = InitialCacheExtent,
            HeightRequest = 520,
            ItemsSource = _stressItems,
            ItemTemplate = new DataTemplate(CreateDenseListCell)
        };
    }

    private View CreatePinnedHeaderDemo()
    {
        const double minHeaderExtent = 64d;
        const double maxHeaderExtent = 152d;
        var rows = new CollectionView
        {
            HeightRequest = 520,
            Margin = new Thickness(0, minHeaderExtent, 0, 0),
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 6
            },
            ItemsSource = _items.Take(260).ToArray(),
            ItemTemplate = new DataTemplate(CreateDenseListCell)
        };

        var detail = CreateLabel(
            "Pinned overlay: 152px max extent, 64px min extent",
            12,
            FontAttributes.None,
            "#DBEAFE",
            row: 1);
        var header = new Border
        {
            HeightRequest = maxHeaderExtent,
            Padding = new Thickness(18, 14),
            BackgroundColor = Color.FromArgb("#172554"),
            Stroke = Color.FromArgb("#1D4ED8"),
            StrokeThickness = 1,
            Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Auto)
                },
                Children =
                {
                    CreateLabel("Persistent header concept", 20, FontAttributes.Bold, "#FFFFFF"),
                    detail
                }
            }
        };

        rows.Scrolled += (_, args) =>
        {
            var consumed = Math.Clamp(args.VerticalOffset, 0d, maxHeaderExtent - minHeaderExtent);
            var extent = maxHeaderExtent - consumed;
            header.HeightRequest = extent;
            header.Opacity = 0.86d + (0.14d * ((extent - minHeaderExtent) / (maxHeaderExtent - minHeaderExtent)));
            detail.IsVisible = extent > 92d;
        };

        return new Grid
        {
            HeightRequest = 520,
            Children =
            {
                rows,
                header
            }
        };
    }

    private View CreateMixedComposition()
    {
        var content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(16),
            Children =
            {
                CreateHeroBand("CustomScrollView-style composition", "Header, list rows, grid tiles, and fill content are ordinary MAUI children composed in one ScrollView."),
                CreateMiniSectionHeader("Fixed extent rows", "Small SliverStackLayout preview inside a mixed scroll surface."),
                CreateMiniFixedRows(),
                CreateMiniSectionHeader("Adaptive grid tiles", "Grid content follows the same shared gallery item source."),
                CreateMiniGrid(),
                CreateFillRemainingCard("Fill remaining", "A terminal region fills the viewport remainder when preceding content is short.")
            }
        };

        return new ScrollView
        {
            HeightRequest = 620,
            Content = content
        };
    }

    private View CreateTabbedNestedDemo()
    {
        var results = CreateTabbedCollection(_items.Skip(40).Take(180).ToArray());
        var saved = CreateTabbedCollection(_items.Skip(420).Take(180).ToArray());
        var contentHost = new ContentView();
        var resultsButton = CreateSegmentButton("Results");
        var savedButton = CreateSegmentButton("Saved");

        void Select(Button selected, Button other, View content)
        {
            selected.BackgroundColor = Color.FromArgb("#2563EB");
            selected.TextColor = Colors.White;
            other.BackgroundColor = Colors.White;
            other.TextColor = Color.FromArgb("#334155");
            contentHost.Content = content;
        }

        resultsButton.Clicked += (_, _) => Select(resultsButton, savedButton, results);
        savedButton.Clicked += (_, _) => Select(savedButton, resultsButton, saved);
        Select(resultsButton, savedButton, results);

        return new Grid
        {
            HeightRequest = 560,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                new Border
                {
                    Padding = new Thickness(18, 14),
                    BackgroundColor = Color.FromArgb("#1E3A8A"),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children =
                        {
                            CreateLabel("NestedScrollView-style catalog", 20, FontAttributes.Bold, "#FFFFFF"),
                            CreateLabel("Pinned header plus tabbed inner scroll bodies.", 12, FontAttributes.None, "#DBEAFE")
                        }
                    }
                },
                WithRow(new HorizontalStackLayout
                {
                    Spacing = 8,
                    Padding = new Thickness(12, 10),
                    BackgroundColor = Colors.White,
                    Children =
                    {
                        resultsButton,
                        savedButton
                    }
                }, 1),
                WithRow(contentHost, 2)
            }
        };
    }

    private CollectionView CreateTabbedCollection(IReadOnlyList<GalleryItem> items)
    {
        return new CollectionView
        {
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 6
            },
            ItemsSource = items,
            ItemTemplate = new DataTemplate(CreateDenseListCell)
        };
    }

    private static Button CreateSegmentButton(string text)
    {
        return new Button
        {
            Text = text,
            Padding = new Thickness(14, 8),
            BorderColor = Color.FromArgb("#CBD5E1"),
            BorderWidth = 1
        };
    }

    private View CreateFillPaddingVisibilityDemo()
    {
        var hidden = CreateCompositionCard("Visibility off", "This panel remains in the tree as the SliverVisibility analogue, but MAUI does not measure it while hidden.", "#FCE7F3", "#BE185D");
        hidden.IsVisible = false;

        return new ScrollView
        {
            HeightRequest = 420,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Padding = new Thickness(24),
                Children =
                {
                    CreateCompositionCard("SliverPadding", "Outer padding is supplied by the MAUI container rather than a dedicated sliver adapter.", "#EFF6FF", "#1D4ED8"),
                    hidden,
                    CreateCompositionCard("Replacement content", "Visible fallback content occupies the hidden slot.", "#FDF2F8", "#BE185D"),
                    CreateFillRemainingCard("SliverFillRemaining", "The card reserves extra height so short content still fills the remaining viewport.", height: 210)
                }
            }
        };
    }

    private View CreateStressDemo()
    {
        var status = CreateLabel(
            $"Source: {_stressItems.Count:N0} rows | native recycling | cache metadata {_stressList.CacheExtent:0}px",
            13,
            FontAttributes.Bold,
            "#374151");
        var jump = new Button
        {
            Text = "Jump to row 50,000",
            BackgroundColor = Color.FromArgb("#111827"),
            TextColor = Colors.White,
            Padding = new Thickness(12, 8)
        };
        jump.Clicked += (_, _) => _stressList.ScrollTo(50_000, position: ScrollToPosition.Center, animate: true);

        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Grid
                {
                    ColumnSpacing = 12,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Children =
                    {
                        status,
                        WithColumn(jump, 1)
                    }
                },
                _stressList
            }
        };
    }

    private View CreateMetricCard(GalleryMetric metric)
    {
        var accent = Color.FromArgb(metric.AccentColor);
        return new Border
        {
            Padding = 14,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new BoxView { Color = accent, HeightRequest = 3 },
                    CreateLabel(metric.Value, 22, FontAttributes.Bold, "#111827"),
                    CreateLabel(metric.Label, 13, FontAttributes.Bold, "#374151"),
                    CreateLabel(metric.Detail, 12, FontAttributes.None, "#6B7280")
                }
            }
        };
    }

    private View CreateScenarioCard(GalleryScenario scenario)
    {
        return new Border
        {
            Padding = 14,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 7,
                Children =
                {
                    CreateLabel(scenario.Title, 16, FontAttributes.Bold, "#111827"),
                    CreateLabel(scenario.FlutterReference, 12, FontAttributes.Bold, "#2563EB"),
                    CreateLabel(scenario.Summary, 13, FontAttributes.None, "#4B5563"),
                    CreateLabel(scenario.SupportedFeature, 12, FontAttributes.Bold, "#374151")
                }
            }
        };
    }

    private View CreateFixedRow(GalleryItem item)
    {
        return new Grid
        {
            Padding = new Thickness(14, 8),
            BackgroundColor = Colors.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(5)),
                new ColumnDefinition(new GridLength(12)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            Children =
            {
                new BoxView { Color = Color.FromArgb(item.AccentColor) },
                CreateListText(item, 2),
                CreateLabel($"#{item.Rank}", 12, FontAttributes.Bold, "#6B7280", column: 3)
            }
        };
    }

    private View CreateListCell()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12, 8),
            BackgroundColor = Colors.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(5)),
                new ColumnDefinition(new GridLength(12)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(GalleryItem.AccentColor), converter: new ColorStringConverter()));
        grid.Add(accent, 0, 0);
        grid.Add(CreateBoundListText(), 2, 0);

        var rank = CreateLabel("", 12, FontAttributes.Bold, "#6B7280");
        rank.SetBinding(Label.TextProperty, new Binding(nameof(GalleryItem.Rank), stringFormat: "#{0}"));
        grid.Add(rank, 3, 0);

        return grid;
    }

    private View CreateDenseListCell()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12, 6),
            BackgroundColor = Colors.White,
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(4)),
                new ColumnDefinition(new GridLength(10)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(GalleryItem.AccentColor), converter: new ColorStringConverter()));
        grid.Add(accent, 0, 0);

        var title = CreateLabel("", 13, FontAttributes.Bold, "#111827");
        title.SetBinding(Label.TextProperty, nameof(GalleryItem.Title));
        grid.Add(title, 2, 0);

        var category = CreateLabel("", 12, FontAttributes.None, "#6B7280");
        category.SetBinding(Label.TextProperty, nameof(GalleryItem.Category));
        grid.Add(category, 3, 0);
        return grid;
    }

    private View CreateVariableListCell()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12, 8),
            BackgroundColor = Colors.White,
            RowDefinitions =
            {
                new RowDefinition(new GridLength(4)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };
        grid.SetBinding(VisualElement.HeightRequestProperty, nameof(GalleryItem.Extent));

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(GalleryItem.AccentColor), converter: new ColorStringConverter()));
        grid.Add(accent, 0, 0);

        var title = CreateLabel("", 14, FontAttributes.Bold, "#111827", row: 1);
        title.SetBinding(Label.TextProperty, nameof(GalleryItem.Title));
        grid.Add(title);

        var subtitle = CreateLabel("", 12, FontAttributes.None, "#6B7280", row: 2);
        subtitle.LineBreakMode = LineBreakMode.TailTruncation;
        subtitle.SetBinding(Label.TextProperty, nameof(GalleryItem.Subtitle));
        grid.Add(subtitle);

        var extent = CreateLabel("", 11, FontAttributes.Bold, "#6B7280", row: 3);
        extent.VerticalOptions = LayoutOptions.End;
        extent.SetBinding(Label.TextProperty, new Binding(nameof(GalleryItem.Extent), stringFormat: "measured extent {0:0}px"));
        grid.Add(extent);
        return grid;
    }

    private View CreateStackCell()
    {
        var frame = new Border
        {
            Padding = new Thickness(10),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#D8E0EC"),
            StrokeThickness = 1,
            HorizontalOptions = LayoutOptions.Center
        };
        frame.SetBinding(VisualElement.WidthRequestProperty, new Binding(".", converter: new StackWidthConverter()));
        frame.SetBinding(VisualElement.HeightRequestProperty, new Binding(nameof(GalleryItem.Extent)));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(4)),
                new ColumnDefinition(new GridLength(10)),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(GalleryItem.AccentColor), converter: new ColorStringConverter()));
        grid.Add(accent, 0, 0);

        var text = CreateBoundListText();
        grid.Add(text, 2, 0);

        var rank = CreateLabel("", 12, FontAttributes.Bold, "#6B7280");
        rank.SetBinding(Label.TextProperty, new Binding(nameof(GalleryItem.Rank), stringFormat: "#{0}"));
        grid.Add(rank, 3, 0);

        frame.Content = grid;
        return frame;
    }

    private View CreateDataGridRowCell()
    {
        var border = new Border
        {
            Padding = new Thickness(10, 6),
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1
        };
        border.SetBinding(VisualElement.HeightRequestProperty, nameof(GalleryDataGridRow.Extent));

        var grid = CreateDataGridColumns();
        AddDataGridCell(grid, 0, nameof(GalleryDataGridRow.Id), "{0:N0}", FontAttributes.Bold, "#111827");
        AddDataGridCell(grid, 1, nameof(GalleryDataGridRow.Account), null, FontAttributes.Bold, "#111827");
        AddDataGridCell(grid, 2, nameof(GalleryDataGridRow.Region));
        AddDataGridCell(grid, 3, nameof(GalleryDataGridRow.Category));
        AddDataGridCell(grid, 4, nameof(GalleryDataGridRow.Status));
        AddDataGridCell(grid, 5, nameof(GalleryDataGridRow.Owner));
        AddDataGridCell(grid, 6, nameof(GalleryDataGridRow.Amount), "{0:C0}", FontAttributes.Bold, "#111827");
        AddDataGridCell(grid, 7, nameof(GalleryDataGridRow.Progress), "{0}%");
        AddDataGridCell(grid, 8, nameof(GalleryDataGridRow.Updated), "{0:yyyy-MM-dd}");
        AddDataGridCell(grid, 9, nameof(GalleryDataGridRow.Notes), null, FontAttributes.None, "#475569", LineBreakMode.WordWrap);

        border.Content = grid;
        return border;
    }

    private static void AddDataGridCell(
        Grid grid,
        int column,
        string path,
        string? stringFormat = null,
        FontAttributes attributes = FontAttributes.None,
        string color = "#475569",
        LineBreakMode lineBreakMode = LineBreakMode.TailTruncation)
    {
        var label = CreateLabel(string.Empty, 12, attributes, color, column: column);
        label.VerticalOptions = LayoutOptions.Center;
        label.LineBreakMode = lineBreakMode;
        label.SetBinding(Label.TextProperty, stringFormat is null ? new Binding(path) : new Binding(path, stringFormat: stringFormat));
        grid.Add(label);
    }

    private View CreateGridCell()
    {
        var grid = new Grid
        {
            HeightRequest = AdaptiveTileExtent,
            Padding = 12,
            BackgroundColor = Colors.White,
            RowDefinitions =
            {
                new RowDefinition(new GridLength(4)),
                new RowDefinition(new GridLength(10)),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(GalleryItem.AccentColor), converter: new ColorStringConverter()));
        grid.Add(accent, 0, 0);

        var title = CreateLabel("", 14, FontAttributes.Bold, "#111827", row: 2);
        title.LineBreakMode = LineBreakMode.TailTruncation;
        title.SetBinding(Label.TextProperty, nameof(GalleryItem.Title));
        grid.Add(title);

        var category = CreateLabel("", 12, FontAttributes.None, "#6B7280", row: 3);
        category.SetBinding(Label.TextProperty, nameof(GalleryItem.Category));
        grid.Add(category);

        return grid;
    }

    private View CreateSectionHeader()
    {
        var grid = new Grid
        {
            Padding = new Thickness(12, 14),
            BackgroundColor = Color.FromArgb("#EEF2FF"),
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(5)),
                new ColumnDefinition(new GridLength(12)),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        var accent = new BoxView();
        accent.SetBinding(BoxView.ColorProperty, new Binding(nameof(MauiGallerySection.AccentColor), converter: new ColorStringConverter()));
        Grid.SetRowSpan(accent, 2);
        grid.Add(accent, 0, 0);

        var title = CreateLabel("", 16, FontAttributes.Bold, "#111827", column: 2);
        title.SetBinding(Label.TextProperty, nameof(MauiGallerySection.Title));
        grid.Add(title);

        var summary = CreateLabel("", 12, FontAttributes.None, "#4B5563", row: 1, column: 2);
        summary.SetBinding(Label.TextProperty, nameof(MauiGallerySection.Summary));
        grid.Add(summary);

        return grid;
    }

    private View CreateMiniFixedRows()
    {
        var rows = new MauiSliverStackLayout
        {
            Axis = SliverAxis.Vertical,
            ItemExtent = 54d,
            Spacing = 6d,
            HeightRequest = 234
        };

        foreach (var item in _items.Take(4))
        {
            rows.Children.Add(CreateFixedRow(item));
        }

        return rows;
    }

    private View CreateMiniGrid()
    {
        var grid = new Grid
        {
            ColumnSpacing = 10,
            RowSpacing = 10,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        var tiles = _items.Skip(12).Take(9).ToArray();
        for (var index = 0; index < tiles.Length; index++)
        {
            var row = index / 3;
            if (grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            grid.Add(CreateMiniGridTile(tiles[index]), index % 3, row);
        }

        return grid;
    }

    private View CreateMiniGridTile(GalleryItem item)
    {
        return new Border
        {
            HeightRequest = 96,
            Padding = 10,
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new BoxView { Color = Color.FromArgb(item.AccentColor), HeightRequest = 4 },
                    CreateLabel(item.Category, 12, FontAttributes.Bold, "#111827"),
                    CreateLabel(item.Title, 11, FontAttributes.None, "#6B7280")
                }
            }
        };
    }

    private View CreateHeroBand(string title, string summary)
    {
        return new Border
        {
            Padding = 18,
            BackgroundColor = Color.FromArgb("#111827"),
            Stroke = Color.FromArgb("#374151"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    CreateLabel(title, 20, FontAttributes.Bold, "#FFFFFF"),
                    CreateLabel(summary, 13, FontAttributes.None, "#D1D5DB")
                }
            }
        };
    }

    private View CreateMiniSectionHeader(string title, string summary)
    {
        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                CreateLabel(title, 16, FontAttributes.Bold, "#111827"),
                CreateLabel(summary, 12, FontAttributes.None, "#6B7280")
            }
        };
    }

    private View CreateCompositionCard(string title, string summary, string background, string foreground)
    {
        return new Border
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb(background),
            Stroke = Color.FromArgb(foreground),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    CreateLabel(title, 16, FontAttributes.Bold, foreground),
                    CreateLabel(summary, 13, FontAttributes.None, foreground)
                }
            }
        };
    }

    private View CreateFillRemainingCard(string title, string summary, double height = 180d)
    {
        return new Border
        {
            HeightRequest = height,
            Padding = 18,
            BackgroundColor = Color.FromArgb("#ECFDF5"),
            Stroke = Color.FromArgb("#059669"),
            StrokeThickness = 1,
            Content = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 6,
                Children =
                {
                    CreateLabel(title, 16, FontAttributes.Bold, "#065F46"),
                    CreateLabel(summary, 13, FontAttributes.None, "#047857")
                }
            }
        };
    }

    private View CreateBoundListText()
    {
        var stack = new VerticalStackLayout { Spacing = 2 };

        var title = CreateLabel("", 14, FontAttributes.Bold, "#111827");
        title.SetBinding(Label.TextProperty, nameof(GalleryItem.Title));

        var subtitle = CreateLabel("", 12, FontAttributes.None, "#6B7280");
        subtitle.LineBreakMode = LineBreakMode.TailTruncation;
        subtitle.SetBinding(Label.TextProperty, nameof(GalleryItem.Subtitle));

        stack.Children.Add(title);
        stack.Children.Add(subtitle);
        return stack;
    }

    private View CreateListText(GalleryItem item, int column)
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 2,
            Children =
            {
                CreateLabel(item.Title, 14, FontAttributes.Bold, "#111827"),
                CreateLabel(item.Subtitle, 12, FontAttributes.None, "#6B7280")
            }
        };
        Grid.SetColumn(stack, column);

        return stack;
    }

    private View CreateSection(string title, string summary, View content)
    {
        return CreatePanel(title, summary, content);
    }

    private View CreatePanel(string title, string summary, View content)
    {
        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                CreateLabel(title, 20, FontAttributes.Bold, "#111827"),
                CreateLabel(summary, 13, FontAttributes.None, "#4B5563"),
                content
            }
        };
    }

    private static Grid CreateControlRow(string label, Label valueLabel, Slider slider, int row)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(0, 4),
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(90)),
                new ColumnDefinition(new GridLength(72)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(CreateLabel(label, 13, FontAttributes.Bold, "#374151"), 0, 0);
        grid.Add(valueLabel, 1, 0);
        grid.Add(slider, 2, 0);
        Grid.SetRow(grid, row);
        return grid;
    }

    private static Grid CreateReadOnlyRow(string label, Label valueLabel, int row)
    {
        var grid = new Grid
        {
            ColumnSpacing = 12,
            Padding = new Thickness(0, 4),
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(90)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        grid.Add(CreateLabel(label, 13, FontAttributes.Bold, "#374151"), 0, 0);
        grid.Add(valueLabel, 1, 0);
        Grid.SetRow(grid, row);
        return grid;
    }

    private static Slider CreateSlider(double minimum, double maximum, double value)
    {
        return new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Value = value,
            MinimumTrackColor = Color.FromArgb("#2563EB"),
            MaximumTrackColor = Color.FromArgb("#D1D5DB"),
            ThumbColor = Color.FromArgb("#111827")
        };
    }

    private static Label CreateValueLabel(string text)
    {
        return CreateLabel(text, 13, FontAttributes.Bold, "#111827");
    }

    private static T WithColumn<T>(T view, int column)
        where T : View
    {
        Grid.SetColumn(view, column);
        return view;
    }

    private static T WithRow<T>(T view, int row)
        where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }

    private static Label CreateLabel(
        string text,
        double size,
        FontAttributes attributes,
        string color,
        Thickness? margin = null,
        int row = 0,
        int column = 0)
    {
        var label = new Label
        {
            Text = text,
            FontSize = size,
            FontAttributes = attributes,
            TextColor = Color.FromArgb(color),
            Margin = margin ?? Thickness.Zero
        };
        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);

        return label;
    }

    private static IReadOnlyList<MauiWrapRow> CreateWrapRows(IReadOnlyList<GalleryItem> items, double crossAxisExtent)
    {
        var rows = new List<MauiWrapRow>();
        var current = new List<GalleryItem>();
        var crossCursor = 0d;
        var rowHeight = 0d;

        foreach (var item in items)
        {
            var itemWidth = SliverGalleryData.GetWrapCrossAxisExtent(item.Id);
            var itemHeight = SliverGalleryData.GetWrapMainAxisExtent(item.Id);
            var projectedCrossEnd = current.Count == 0
                ? itemWidth
                : crossCursor + InitialSpacing + itemWidth;

            if (current.Count > 0 && projectedCrossEnd > crossAxisExtent)
            {
                rows.Add(new MauiWrapRow(current.ToArray(), rowHeight));
                current.Clear();
                crossCursor = 0d;
                rowHeight = 0d;
            }

            if (current.Count > 0)
            {
                crossCursor += InitialSpacing;
            }

            current.Add(item);
            crossCursor += itemWidth;
            rowHeight = Math.Max(rowHeight, itemHeight);
        }

        if (current.Count > 0)
        {
            rows.Add(new MauiWrapRow(current.ToArray(), rowHeight));
        }

        return rows;
    }

    private GalleryScenario Scenario(GalleryScenarioKind kind)
    {
        return _scenarios.First(scenario => scenario.Kind == kind);
    }

    private sealed record MauiGalleryPage(GalleryScenario Scenario, View Content);

    private sealed record MauiWrapRow(IReadOnlyList<GalleryItem> Items, double Height);

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

    private sealed class WrapRowView : HorizontalStackLayout
    {
        public WrapRowView()
        {
            Spacing = InitialSpacing;
            Padding = new Thickness(8, 0);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            Children.Clear();

            if (BindingContext is not MauiWrapRow row)
            {
                return;
            }

            HeightRequest = row.Height;
            foreach (var item in row.Items)
            {
                Children.Add(CreateChip(item));
            }
        }

        private static View CreateChip(GalleryItem item)
        {
            var accent = Color.FromArgb(item.AccentColor);
            return new Border
            {
                WidthRequest = SliverGalleryData.GetWrapCrossAxisExtent(item.Id),
                HeightRequest = SliverGalleryData.GetWrapMainAxisExtent(item.Id),
                Padding = new Thickness(8),
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#D8E0EC"),
                StrokeThickness = 1,
                Content = new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new BoxView
                        {
                            WidthRequest = 28,
                            HeightRequest = 4,
                            HorizontalOptions = LayoutOptions.Start,
                            Color = accent
                        },
                        new Label
                        {
                            Text = item.Title,
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#111827"),
                            LineBreakMode = LineBreakMode.TailTruncation
                        },
                        new Label
                        {
                            Text = $"{item.Category} {item.Extent:0}px",
                            FontSize = 11,
                            TextColor = accent,
                            LineBreakMode = LineBreakMode.TailTruncation
                        }
                    }
                }
            };
        }
    }

    private sealed class MauiGallerySection : List<GalleryItem>
    {
        public MauiGallerySection(GallerySection section)
            : base(section.Items)
        {
            Title = section.Title;
            Summary = section.Summary;
            AccentColor = section.AccentColor;
        }

        public string Title { get; }

        public string Summary { get; }

        public string AccentColor { get; }
    }

    private sealed class ColorStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is string color ? Color.FromArgb(color) : Colors.Transparent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class StackWidthConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is GalleryItem item
                ? SliverGalleryData.GetStackCrossAxisExtent(item.Id)
                : SliverGalleryData.StackMinCrossAxisExtent;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
