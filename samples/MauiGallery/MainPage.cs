using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SliverWidgets.Core;
using SliverWidgets.GalleryData;
using SliverWidgets.Maui;

namespace SliverWidgets.MauiGallery;

public sealed class MainPage : ContentPage
{
    private const double InitialExtent = 64d;
    private const double InitialSpacing = 8d;
    private const double InitialCacheExtent = 240d;

    private readonly IReadOnlyList<GalleryItem> _items = SliverGalleryData.CreateItems();
    private readonly IReadOnlyList<GallerySection> _sections = SliverGalleryData.CreateSections(5, 40);
    private readonly SliverStackLayout _fixedList;
    private readonly SliverCollectionView _virtualList;
    private readonly SliverCollectionView _gridList;
    private readonly SliverCollectionView _sectionList;
    private readonly Label _extentValue;
    private readonly Label _spacingValue;
    private readonly Label _cacheValue;

    public MainPage()
    {
        Title = "SliverWidgets MAUI Gallery";
        BackgroundColor = Color.FromArgb("#F7F8FA");
        Padding = new Thickness(0);

        _fixedList = CreateFixedStackPreview();
        _virtualList = CreateVirtualList();
        _gridList = CreateGridList();
        _sectionList = CreateSectionList();
        _extentValue = CreateValueLabel($"{InitialExtent:0}px");
        _spacingValue = CreateValueLabel($"{InitialSpacing:0}px");
        _cacheValue = CreateValueLabel($"{InitialCacheExtent:0}px");

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 28,
                Padding = new Thickness(24, 22),
                Children =
                {
                    CreateHeader(),
                    CreateMetrics(),
                    CreateControls(),
                    CreateDemoCards(),
                    CreateSection(
                        "Fixed list layout",
                        "Explicit child controls measured by SliverStackLayout with arithmetic item extents.",
                        _fixedList),
                    CreateSection(
                        "Virtualized collection list",
                        "SliverCollectionView delegates realization and recycling to native MAUI CollectionView handlers.",
                        _virtualList),
                    CreateSection(
                        "Grid collection mode",
                        "Fixed cross-axis count with sliver spacing metadata and native grid item layout.",
                        _gridList),
                    CreateSection(
                        "Section headers",
                        "Grouped CollectionView headers show Flutter-inspired section composition while rows stay virtualized.",
                        _sectionList)
                }
            }
        };
    }

    private View CreateHeader()
    {
        return new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                CreateLabel("SliverWidgets MAUI", 30, FontAttributes.Bold, "#111827"),
                CreateLabel(
                    "Code-only gallery for fixed extent layout, native-backed virtualization, grids, headers, cache metadata, spacing, and extent controls.",
                    15,
                    FontAttributes.None,
                    "#4B5563",
                    margin: new Thickness(0, 8, 0, 0),
                    row: 1),
                CreateLabel(
                    "Flutter concepts: SliverFixedExtentList, SliverGrid, SliverPersistentHeader, cache extent, and mixed list/grid/header composition.",
                    13,
                    FontAttributes.None,
                    "#6B7280",
                    margin: new Thickness(0, 6, 0, 0),
                    row: 2)
            }
        };
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

    private View CreateControls()
    {
        var extentSlider = CreateSlider(44, 96, InitialExtent);
        extentSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _fixedList.ItemExtent = value;
            _virtualList.ItemExtent = value;
            _extentValue.Text = $"{value:0}px";
        };

        var spacingSlider = CreateSlider(0, 20, InitialSpacing);
        spacingSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _fixedList.Spacing = value;
            _virtualList.Spacing = value;
            _gridList.MainAxisSpacing = value;
            _gridList.CrossAxisSpacing = value;
            _sectionList.Spacing = value;
            _spacingValue.Text = $"{value:0}px";
        };

        var cacheSlider = CreateSlider(0, 640, InitialCacheExtent);
        cacheSlider.ValueChanged += (_, args) =>
        {
            var value = Math.Round(args.NewValue);
            _virtualList.CacheExtent = value;
            _gridList.CacheExtent = value;
            _sectionList.CacheExtent = value;
            _cacheValue.Text = $"{value:0}px";
        };

        return CreatePanel(
            "Sliver controls",
            "The sliders update the live MAUI sliver controls. Cache extent remains metadata because MAUI handlers own the realization window.",
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
            });
    }

    private View CreateDemoCards()
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

        var demos = SliverGalleryData.CreateDemos();
        for (var index = 0; index < demos.Count; index++)
        {
            var row = index / 2;
            if (layout.RowDefinitions.Count <= row)
            {
                layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            }

            layout.Add(CreateDemoCard(demos[index]), index % 2, row);
        }

        return CreatePanel("Flutter-inspired demos", "Shared deterministic demos projected into native MAUI controls.", layout);
    }

    private SliverStackLayout CreateFixedStackPreview()
    {
        var layout = new SliverStackLayout
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

    private SliverCollectionView CreateVirtualList()
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

    private SliverCollectionView CreateGridList()
    {
        return new SliverCollectionView
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

    private View CreateDemoCard(GalleryDemo demo)
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
                    CreateLabel(demo.Title, 16, FontAttributes.Bold, "#111827"),
                    CreateLabel(demo.Summary, 13, FontAttributes.None, "#4B5563"),
                    CreateLabel(demo.FlutterReference, 12, FontAttributes.None, "#2563EB"),
                    CreateLabel(demo.SupportedFeature, 12, FontAttributes.Bold, "#374151")
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

    private View CreateGridCell()
    {
        var grid = new Grid
        {
            HeightRequest = 118,
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
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            }
        };

        var title = CreateLabel("", 16, FontAttributes.Bold, "#111827");
        title.SetBinding(Label.TextProperty, nameof(GallerySection.Title));
        grid.Add(title);

        var summary = CreateLabel("", 12, FontAttributes.None, "#4B5563", row: 1);
        summary.SetBinding(Label.TextProperty, nameof(GallerySection.Summary));
        grid.Add(summary);

        return grid;
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
}
