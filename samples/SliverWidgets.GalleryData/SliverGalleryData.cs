namespace SliverWidgets.GalleryData;

public static class SliverGalleryData
{
    private static readonly string[] Categories =
    [
        "Pinned headers",
        "Fixed extent",
        "Adaptive grid",
        "Variable extent",
        "Cache window",
        "Fill remaining"
    ];

    private static readonly string[] Palette =
    [
        "#0F766E",
        "#2563EB",
        "#9333EA",
        "#DB2777",
        "#EA580C",
        "#65A30D",
        "#0891B2",
        "#7C3AED"
    ];

    public static IReadOnlyList<GalleryItem> CreateItems(int count = 5000)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var items = new GalleryItem[count];

        for (var index = 0; index < count; index++)
        {
            var category = Categories[index % Categories.Length];
            var extent = 52d + ((index % 5) * 12d);

            items[index] = new GalleryItem(
                index,
                $"Sliver item {index:0000}",
                $"{category} sample with deterministic extent {extent:0}px",
                category,
                Palette[index % Palette.Length],
                extent,
                1 + (index % 100),
                index % 37 == 0);
        }

        return items;
    }

    public static IReadOnlyList<GalleryItem> CreateUniformItems(int count = 5000, double extent = 64d)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (double.IsNaN(extent) || extent < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(extent));
        }

        return CreateItems(count)
            .Select(item => item with
            {
                Subtitle = $"{item.Category} uniform fixed-extent sample {extent:0}px",
                Extent = extent
            })
            .ToArray();
    }

    public static IReadOnlyList<GalleryItem> CreateVariableItems(int count = 5000)
    {
        return CreateItems(count);
    }

    public static IReadOnlyList<GallerySection> CreateSections(int sectionCount = 8, int itemsPerSection = 80)
    {
        if (sectionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sectionCount));
        }

        if (itemsPerSection < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(itemsPerSection));
        }

        var allItems = CreateItems(sectionCount * itemsPerSection);
        var sections = new GallerySection[sectionCount];

        for (var section = 0; section < sectionCount; section++)
        {
            var items = allItems
                .Skip(section * itemsPerSection)
                .Take(itemsPerSection)
                .ToArray();

            sections[section] = new GallerySection(
                $"Section {section + 1}",
                $"Pinned header and virtualized children for {items.Length} rows.",
                Palette[section % Palette.Length],
                items);
        }

        return sections;
    }

    public static IReadOnlyList<GalleryMetric> CreateMetrics()
    {
        return
        [
            new GalleryMetric("Rows", "100,000", "Large deterministic source list", "#2563EB"),
            new GalleryMetric("Grid tiles", "1,200", "Adaptive max-extent tile demo", "#0F766E"),
            new GalleryMetric("Cache", "2x viewport", "Paint plus near-future realization", "#EA580C"),
            new GalleryMetric("Headers", "Pinned", "Persistent header shrink and obstruction", "#9333EA")
        ];
    }

    public static IReadOnlyList<GalleryDemo> CreateDemos()
    {
        return CreateScenarios()
            .Select(scenario => new GalleryDemo(
                scenario.Key,
                scenario.Title,
                scenario.Summary,
                scenario.FlutterReference,
                scenario.SupportedFeature))
            .ToArray();
    }

    public static IReadOnlyList<GalleryScenario> CreateScenarios()
    {
        return
        [
            new GalleryScenario(
                "fixed-large-list",
                "Fixed large list",
                "Uniform-height rows use arithmetic index-to-offset mapping for high-throughput long-list scrolling.",
                "SliverFixedExtentList",
                "Fixed extent sliver virtualization",
                GalleryScenarioKind.FixedExtentList,
                100_000,
                UsesVirtualization: true,
                UsesVariableExtents: false),
            new GalleryScenario(
                "variable-list",
                "Variable and non-uniform list",
                "Measured rows use deterministic mixed extents to exercise dead-reckoned offset and extent caching behavior.",
                "SliverList + SliverPrototypeExtentList + SliverVariedExtentList",
                "Variable extent list layout and native measured rows",
                GalleryScenarioKind.VariableExtentList,
                5_000,
                UsesVirtualization: true,
                UsesVariableExtents: true),
            new GalleryScenario(
                "adaptive-grid",
                "Adaptive grid",
                "Tiles adapt to available cross-axis size while preserving cache-aware grid slot computation.",
                "SliverGrid + SliverGridDelegateWithMaxCrossAxisExtent",
                "Adaptive sliver grid layout",
                GalleryScenarioKind.AdaptiveGrid,
                1_200,
                UsesVirtualization: true,
                UsesVariableExtents: false),
            new GalleryScenario(
                "pinned-header",
                "Pinned and collapsible header",
                "A header shrinks between max and min extents and pins at the viewport edge while content scrolls underneath.",
                "SliverAppBar + SliverPersistentHeader",
                "Persistent header obstruction geometry",
                GalleryScenarioKind.PinnedHeader,
                5_000,
                UsesVirtualization: true,
                UsesVariableExtents: false),
            new GalleryScenario(
                "mixed-composition",
                "Mixed CustomScrollView composition",
                "Box adapters, pinned header content, fixed rows, grid tiles, and fill regions share one scrollable surface.",
                "CustomScrollView + SliverToBoxAdapter + SliverGrid + SliverFixedExtentList",
                "Shared viewport composition",
                GalleryScenarioKind.MixedComposition,
                600,
                UsesVirtualization: true,
                UsesVariableExtents: false),
            new GalleryScenario(
                "sectioned-headers",
                "Sectioned list with sticky headers",
                "Grouped content alternates header regions and list regions to model catalog, settings, and feed layouts.",
                "SliverPersistentHeader + SliverList",
                "Section header composition",
                GalleryScenarioKind.SectionedHeaders,
                1_000,
                UsesVirtualization: true,
                UsesVariableExtents: false),
            new GalleryScenario(
                "fill-padding-visibility",
                "Fill, padding, and visibility",
                "Utility slivers demonstrate insets, empty states, replacement content, and remaining-viewport fill behavior.",
                "SliverPadding + SliverFillRemaining + SliverVisibility",
                "Core utility sliver layouts",
                GalleryScenarioKind.FillPaddingVisibility,
                20,
                UsesVirtualization: false,
                UsesVariableExtents: false),
            new GalleryScenario(
                "cache-stress",
                "Cache and performance stress",
                "A large deterministic source verifies that adapters realize only visible and cached elements.",
                "CustomScrollView cacheExtent + sliver delegates",
                "Viewport plus cache realization",
                GalleryScenarioKind.CacheStress,
                100_000,
                UsesVirtualization: true,
                UsesVariableExtents: false)
        ];
    }

    public static IReadOnlyList<string> AccentPalette => Palette;
}
