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
    private readonly ContentControl _contentHost = new();
    private readonly List<GalleryPage> _pages;

    public MainWindow()
    {
        Title = "SliverWidgets WinUI Gallery";
        _pages =
        [
            new GalleryPage("Fixed list", "ItemsRepeater rows", CreateFixedListPage),
            new GalleryPage("Grid", "Fixed-count grid", CreateGridPage),
            new GalleryPage("Large data", "100,000 virtualized rows", CreateLargeDataPage),
            new GalleryPage("Headers", "Pinned and floating concepts", CreateHeaderConceptsPage)
        ];

        Content = CreateShell();
        _contentHost.Content = _pages[0].Create();
    }

    private Grid CreateShell()
    {
        var shell = new Grid
        {
            Background = Brush(Color.FromArgb(255, 246, 247, 249))
        };
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var navigation = new StackPanel
        {
            Padding = new Thickness(20, 18, 16, 18),
            Spacing = 14,
            Background = Brush(Colors.White)
        };

        navigation.Children.Add(Text("SliverWidgets", 24, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)));
        navigation.Children.Add(Text("WinUI gallery", 13, FontWeights.Normal, Color.FromArgb(255, 96, 112, 132)));

        var list = new ListView
        {
            SelectionMode = ListViewSelectionMode.Single,
            IsItemClickEnabled = true,
            Margin = new Thickness(0, 10, 0, 0)
        };

        foreach (var page in _pages)
        {
            list.Items.Add(new ListViewItem
            {
                Tag = page,
                Content = new StackPanel
                {
                    Spacing = 2,
                    Children =
                    {
                        Text(page.Title, 15, FontWeights.SemiBold, Color.FromArgb(255, 31, 41, 55)),
                        Text(page.Description, 12, FontWeights.Normal, Color.FromArgb(255, 107, 114, 128))
                    }
                }
            });
        }

        list.SelectionChanged += (_, args) =>
        {
            if (args.AddedItems.FirstOrDefault() is ListViewItem { Tag: GalleryPage selected })
            {
                _contentHost.Content = selected.Create();
            }
        };
        list.SelectedIndex = 0;
        navigation.Children.Add(list);

        Grid.SetColumn(navigation, 0);
        shell.Children.Add(navigation);

        _contentHost.Margin = new Thickness(24);
        Grid.SetColumn(_contentHost, 1);
        shell.Children.Add(_contentHost);
        return shell;
    }

    private static FrameworkElement CreateFixedListPage()
    {
        var items = SliverGalleryData.CreateItems(220);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 64,
            Spacing = 6
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.Row));
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            "Fixed extent list",
            "Rows are measured with arithmetic index-to-offset mapping through SliverFixedExtentVirtualizingLayout.");
        AddSlider(controls, "Item extent", 36, 104, layout.ItemExtent, value => layout.ItemExtent = value);
        AddSlider(controls, "Spacing", 0, 20, layout.Spacing, value => layout.Spacing = value);
        AddSlider(controls, "Vertical cache", 0, 5, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateGridPage()
    {
        var items = SliverGalleryData.CreateItems(480);
        var layout = new SliverGridVirtualizingLayout
        {
            CrossAxisCount = 3,
            MainAxisExtent = 128,
            MainAxisSpacing = 12,
            CrossAxisSpacing = 12,
            ChildAspectRatio = 1.4
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.Tile));
        var scrollViewer = CreateScrollViewer(repeater);

        var controls = CreateControlPanel(
            "Grid layout",
            "The grid keeps the adapter thin: WinUI owns realization and the core computes slots.");
        AddSlider(controls, "Columns", 2, 6, layout.CrossAxisCount, value => layout.CrossAxisCount = Math.Max(1, (int)Math.Round(value)));
        AddSlider(controls, "Main extent", 84, 196, layout.MainAxisExtent, value => layout.MainAxisExtent = value);
        AddSlider(controls, "Main spacing", 0, 24, layout.MainAxisSpacing, value => layout.MainAxisSpacing = value);
        AddSlider(controls, "Cross spacing", 0, 24, layout.CrossAxisSpacing, value => layout.CrossAxisSpacing = value);
        AddSlider(controls, "Vertical cache", 0, 5, repeater.VerticalCacheLength, value => repeater.VerticalCacheLength = value);

        return CreateSampleLayout(controls, scrollViewer);
    }

    private static FrameworkElement CreateLargeDataPage()
    {
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
            "Large data virtualization",
            "The source has 100,000 records. ItemsRepeater requests only the visible and cached range.");
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

    private static FrameworkElement CreateHeaderConceptsPage()
    {
        var panel = new Grid { ColumnSpacing = 18 };
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var pinned = CreateHeaderDemo("Pinned header", "Header remains at the viewport edge while rows virtualize beneath it.", floating: false);
        var floating = CreateHeaderDemo("Floating header", "Header returns as the user reverses scroll direction.", floating: true);

        Grid.SetColumn(pinned, 0);
        Grid.SetColumn(floating, 1);
        panel.Children.Add(pinned);
        panel.Children.Add(floating);
        return panel;
    }

    private static FrameworkElement CreateHeaderDemo(string title, string description, bool floating)
    {
        const double headerHeight = 86;
        var items = SliverGalleryData.CreateItems(1_200);
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 48,
            Spacing = 4
        };
        var repeater = CreateRepeater(items, layout, new GalleryItemElementFactory(GalleryItemVisualMode.DenseRow));
        var scrollViewer = CreateScrollViewer(repeater);
        scrollViewer.Margin = new Thickness(0, headerHeight, 0, 0);

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
            Height = headerHeight,
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

        if (floating)
        {
            var previousOffset = 0d;
            scrollViewer.ViewChanged += (_, _) =>
            {
                var currentOffset = scrollViewer.VerticalOffset;
                var scrollingDown = currentOffset > previousOffset;
                transform.Y = scrollingDown && currentOffset > headerHeight ? -headerHeight : 0;
                header.Opacity = transform.Y < 0 ? 0.08 : 1;
                previousOffset = currentOffset;
            };
        }

        return frame;
    }

    private static Grid CreateSampleLayout(StackPanel controls, ScrollViewer viewport)
    {
        var layout = new Grid { ColumnSpacing = 20 };
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(300) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Grid.SetColumn(controls, 0);
        Grid.SetColumn(viewport, 1);
        layout.Children.Add(controls);
        layout.Children.Add(viewport);
        return layout;
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

    private static SolidColorBrush Brush(Color color) => new(color);

    private static string FormatSliderValue(double value)
    {
        return Math.Abs(value - Math.Round(value)) < 0.01 ? value.ToString("0") : value.ToString("0.0");
    }

    private sealed record GalleryPage(string Title, string Description, Func<FrameworkElement> Create);
}

internal enum GalleryItemVisualMode
{
    Row,
    DenseRow,
    Tile
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
            GalleryItemVisualMode.DenseRow => CreateDenseRow(item),
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
