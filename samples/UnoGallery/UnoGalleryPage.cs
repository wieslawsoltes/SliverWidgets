using System.Collections.ObjectModel;
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
    private readonly IReadOnlyList<GalleryItem> _items = SliverGalleryData.CreateItems(600);
    private readonly IReadOnlyList<GalleryItem> _largeItems = SliverGalleryData.CreateItems(100_000);
    private readonly ContentControl _scenarioHost = new();
    private readonly ObservableCollection<Button> _navigationButtons = new();

    public UnoGalleryPage()
    {
        Background = Brush(0xFFF6F8FB);
        Content = BuildShell();
        ShowScenario("Fixed List", BuildFixedListScenario);
    }

    private UIElement BuildShell()
    {
        var root = new Grid
        {
            ColumnSpacing = 0,
            RowSpacing = 0
        };

        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var nav = new StackPanel
        {
            Padding = new Thickness(20),
            Spacing = 10,
            Background = Brush(0xFF101827)
        };

        nav.Children.Add(new TextBlock
        {
            Text = "SliverWidgets",
            Foreground = Brush(0xFFFFFFFF),
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 2)
        });

        nav.Children.Add(new TextBlock
        {
            Text = "Uno gallery",
            Foreground = Brush(0xFFA7B0C3),
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 18)
        });

        AddNavigation(nav, "Fixed List", BuildFixedListScenario);
        AddNavigation(nav, "Responsive Grid", BuildGridScenario);
        AddNavigation(nav, "Large Data", BuildLargeDataScenario);
        AddNavigation(nav, "Custom Scroll", BuildCustomScrollScenario);

        Grid.SetColumn(nav, 0);
        root.Children.Add(nav);

        var contentFrame = new Border
        {
            Padding = new Thickness(24),
            Background = Brush(0xFFF6F8FB),
            Child = _scenarioHost
        };

        Grid.SetColumn(contentFrame, 1);
        root.Children.Add(contentFrame);

        return root;
    }

    private void AddNavigation(StackPanel nav, string label, Func<UIElement> create)
    {
        var button = new Button
        {
            Content = label,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(14, 10, 14, 10),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = Brush(0x00101827),
            Foreground = Brush(0xFFE9EEF7),
            BorderBrush = Brush(0x00263244)
        };

        button.Click += (_, _) => ShowScenario(label, create);
        _navigationButtons.Add(button);
        nav.Children.Add(button);
    }

    private void ShowScenario(string label, Func<UIElement> create)
    {
        foreach (var button in _navigationButtons)
        {
            var selected = string.Equals(button.Content?.ToString(), label, StringComparison.Ordinal);
            button.Background = selected ? Brush(0xFF263244) : Brush(0x00101827);
            button.BorderBrush = selected ? Brush(0xFF3B82F6) : Brush(0x00263244);
        }

        _scenarioHost.Content = create();
    }

    private UIElement BuildFixedListScenario()
    {
        var layout = new SliverFixedExtentVirtualizingLayout
        {
            ItemExtent = 64,
            Spacing = 6
        };

        var repeater = CreateRepeater(_items, layout, GalleryItemFactoryKind.List);
        repeater.VerticalCacheLength = 2;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("item extent", 36, 112, layout.ItemExtent, 1, value => layout.ItemExtent = value));
        controls.Children.Add(ControlSlider("spacing", 0, 24, layout.Spacing, 1, value => layout.Spacing = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            "Fixed extent list",
            "ItemsRepeater with SliverFixedExtentVirtualizingLayout. Row offsets use arithmetic index-to-offset mapping.",
            controls,
            Viewport(repeater));
    }

    private UIElement BuildGridScenario()
    {
        var layout = new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.FixedCrossAxisCount,
            CrossAxisCount = 3,
            MainAxisSpacing = 12,
            CrossAxisSpacing = 12,
            ChildAspectRatio = 1.45
        };

        var repeater = CreateRepeater(_items, layout, GalleryItemFactoryKind.Grid);
        repeater.VerticalCacheLength = 1.5;

        var controls = new StackPanel { Spacing = 14 };
        controls.Children.Add(ControlSlider("columns", 1, 6, layout.CrossAxisCount, 1, value => layout.CrossAxisCount = Math.Max(1, (int)Math.Round(value))));
        controls.Children.Add(ControlSlider("main spacing", 0, 32, layout.MainAxisSpacing, 1, value => layout.MainAxisSpacing = value));
        controls.Children.Add(ControlSlider("cross spacing", 0, 32, layout.CrossAxisSpacing, 1, value => layout.CrossAxisSpacing = value));
        controls.Children.Add(ControlSlider("aspect ratio", 0.7, 2.4, layout.ChildAspectRatio, 0.05, value => layout.ChildAspectRatio = value));
        controls.Children.Add(ControlSlider("cache length", 0, 8, repeater.VerticalCacheLength, 0.5, value => repeater.VerticalCacheLength = value));

        return Scenario(
            "Grid layout",
            "ItemsRepeater with SliverGridVirtualizingLayout. The gallery exposes spacing, aspect, cache, and column controls.",
            controls,
            Viewport(repeater));
    }

    private UIElement BuildLargeDataScenario()
    {
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
            "Large data virtualization",
            "A 100,000 item source verifies that the layout realizes the viewport plus cache range instead of the full collection.",
            controls,
            scroller);
    }

    private UIElement BuildCustomScrollScenario()
    {
        var root = new StackPanel
        {
            Spacing = 18,
            Padding = new Thickness(0, 0, 0, 24)
        };

        root.Children.Add(HeroPanel());
        root.Children.Add(SectionHeader("SliverToBoxAdapter-style summary", "Ordinary Uno controls can sit between virtualized sliver sections."));
        root.Children.Add(SummaryBand());
        root.Children.Add(SectionHeader("Fixed extent sliver list", "A compact activity feed hosted by ItemsRepeater."));
        root.Children.Add(CreateRepeater(SliverGalleryData.CreateItems(36), new SliverFixedExtentVirtualizingLayout { ItemExtent = 58, Spacing = 4 }, GalleryItemFactoryKind.Compact));
        root.Children.Add(SectionHeader("Responsive grid sliver", "A Flutter-inspired CustomScrollView composition using normal Uno panels plus sliver layouts."));
        root.Children.Add(CreateRepeater(SliverGalleryData.CreateItems(72), new SliverGridVirtualizingLayout
        {
            SizingMode = SliverGridSizingMode.MaxCrossAxisExtent,
            MaxCrossAxisExtent = 210,
            MainAxisSpacing = 10,
            CrossAxisSpacing = 10,
            ChildAspectRatio = 1.35
        }, GalleryItemFactoryKind.Grid));

        return Scenario(
            "CustomScrollView-style panels",
            "This page composes box panels, fixed list slivers, and grid slivers in one vertical scroll surface.",
            new TextBlock
            {
                Text = "The composite view keeps platform scrolling policy in ScrollViewer and delegates realized item layout to SliverWidgets.Uno.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brush(0xFF475467)
            },
            new ScrollViewer
            {
                Content = root,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brush(0xFFFFFFFF)
            });
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

    private static UIElement Scenario(string title, string description, UIElement controls, UIElement viewport)
    {
        var root = new Grid
        {
            RowSpacing = 16
        };

        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new Grid
        {
            ColumnSpacing = 24
        };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(320) });

        var titleStack = new StackPanel { Spacing = 6 };
        titleStack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 30,
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

        Grid.SetColumn(titleStack, 0);
        header.Children.Add(titleStack);

        var controlPanel = new Border
        {
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(8),
            Background = Brush(0xFFFFFFFF),
            BorderBrush = Brush(0xFFD0D5DD),
            BorderThickness = new Thickness(1),
            Child = controls
        };

        Grid.SetColumn(controlPanel, 1);
        header.Children.Add(controlPanel);

        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var viewportFrame = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderBrush = Brush(0xFFD0D5DD),
            BorderThickness = new Thickness(1),
            Background = Brush(0xFFFFFFFF),
            Child = viewport
        };

        Grid.SetRow(viewportFrame, 1);
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
            GalleryItemFactoryKind.Compact => CreateCompactRow(),
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

    private static void UpdateElement(UIElement element, GalleryItem item)
    {
        if (element is FrameworkElement frameworkElement)
        {
            frameworkElement.Tag = item;
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

internal enum GalleryItemFactoryKind
{
    List,
    Compact,
    Grid
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
