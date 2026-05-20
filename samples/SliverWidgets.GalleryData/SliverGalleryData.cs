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
            new GalleryMetric("Rows", "5,000", "Large deterministic source list", "#2563EB"),
            new GalleryMetric("Grid tiles", "1,200", "Adaptive max-extent tile demo", "#0F766E"),
            new GalleryMetric("Cache", "2x viewport", "Paint plus near-future realization", "#EA580C"),
            new GalleryMetric("Headers", "Pinned", "Persistent header shrink and obstruction", "#9333EA")
        ];
    }

    public static IReadOnlyList<GalleryDemo> CreateDemos()
    {
        return
        [
            new GalleryDemo(
                "custom-scroll",
                "Mixed sliver composition",
                "Expanded header, adaptive grid, and fixed-extent list in one gallery surface.",
                "CustomScrollView + SliverAppBar + SliverGrid + SliverFixedExtentList",
                "Shared viewport layout engine"),
            new GalleryDemo(
                "fixed-list",
                "Fixed extent virtual list",
                "Arithmetic index-to-offset mapping for smooth long-list scrolling.",
                "SliverFixedExtentList",
                "SliverFixedExtentListLayout"),
            new GalleryDemo(
                "adaptive-grid",
                "Adaptive sliver grid",
                "Max cross-axis extent and fixed column-count grid modes.",
                "SliverGridDelegateWithMaxCrossAxisExtent",
                "SliverGridLayout"),
            new GalleryDemo(
                "persistent-header",
                "Persistent headers",
                "Pinned, floating, and snap-ready header behavior expressed through sliver geometry.",
                "SliverPersistentHeader and SliverAppBar pinned/floating/snap",
                "SliverAdvancedPersistentHeaderLayout"),
            new GalleryDemo(
                "fill-padding",
                "Fill, padding, and visibility",
                "Box-to-sliver adapter composition for gaps, empty states, and replacement content.",
                "SliverFillRemaining + SliverPadding + SliverVisibility",
                "Core utility sliver layouts")
        ];
    }

    public static IReadOnlyList<string> AccentPalette => Palette;
}
