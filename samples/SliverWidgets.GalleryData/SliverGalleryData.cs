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

    private static readonly string[] Regions =
    [
        "North",
        "South",
        "East",
        "West",
        "Central",
        "International"
    ];

    private static readonly string[] Statuses =
    [
        "Open",
        "Review",
        "Blocked",
        "Closed",
        "Escalated"
    ];

    private static readonly string[] Owners =
    [
        "Avery",
        "Blake",
        "Casey",
        "Devon",
        "Emerson",
        "Finley",
        "Harper",
        "Jordan"
    ];

    public const double WrapMinMainAxisExtent = 72d;

    public const double WrapMaxMainAxisExtent = 150d;

    public const double WrapMinCrossAxisExtent = 120d;

    public const double WrapMaxCrossAxisExtent = 280d;

    public const double StackMinMainAxisExtent = 52d;

    public const double StackMaxMainAxisExtent = 128d;

    public const double StackMinCrossAxisExtent = 160d;

    public const double StackMaxCrossAxisExtent = 640d;

    public const double DataGridMinRowExtent = 36d;

    public const double DataGridMaxRowExtent = 96d;

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

    public static IReadOnlyList<GalleryItem> CreateWrapItems(int count = 100_000)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var items = new GalleryItem[count];

        for (var index = 0; index < count; index++)
        {
            var mainExtent = GetWrapMainAxisExtent(index);
            var crossExtent = GetWrapCrossAxisExtent(index);
            var category = Categories[index % Categories.Length];

            items[index] = new GalleryItem(
                index,
                $"Wrap item {index:0000}",
                $"{category} variable wrap chip {crossExtent:0}x{mainExtent:0}px",
                category,
                Palette[index % Palette.Length],
                mainExtent,
                1 + (index % 100),
                index % 37 == 0);
        }

        return items;
    }

    public static IReadOnlyList<GalleryItem> CreateStackItems(int count = 100_000)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var items = new GalleryItem[count];

        for (var index = 0; index < count; index++)
        {
            var mainExtent = GetStackMainAxisExtent(index);
            var crossExtent = GetStackCrossAxisExtent(index);
            var category = Categories[index % Categories.Length];

            items[index] = new GalleryItem(
                index,
                $"Stack item {index:0000}",
                $"{category} variable stack card {crossExtent:0}x{mainExtent:0}px",
                category,
                Palette[index % Palette.Length],
                mainExtent,
                1 + (index % 100),
                index % 37 == 0);
        }

        return items;
    }

    public static IReadOnlyList<GalleryDataGridRow> CreateDataGridRows(int count = 100_000)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var rows = new GalleryDataGridRow[count];
        var start = new DateTime(2026, 1, 1);

        for (var index = 0; index < count; index++)
        {
            var category = Categories[index % Categories.Length];
            var region = Regions[(index * 7) % Regions.Length];
            var status = Statuses[(index * 11) % Statuses.Length];
            var owner = Owners[(index * 13) % Owners.Length];
            var amount = 1_000d + (((index * 7919) % 250_000) / 10d);
            var progress = (index * 17) % 101;
            var extent = GetDataGridRowExtent(index);

            rows[index] = new GalleryDataGridRow(
                index,
                $"Account {index:000000}",
                region,
                category,
                status,
                owner,
                amount,
                progress,
                start.AddDays(index % 365),
                $"{category} {status.ToLowerInvariant()} record with dynamic row content and deterministic height {extent:0}px.",
                extent,
                Palette[index % Palette.Length],
                1 + (index % 1000));
        }

        return rows;
    }

    public static IReadOnlyList<GalleryDataGridColumn> CreateDataGridColumns()
    {
        return
        [
            new GalleryDataGridColumn("id", "ID", "Fixed", 84d, 64d, 110d, "Stable row identity."),
            new GalleryDataGridColumn("account", "Account", "Auto", 180d, 140d, 280d, "Auto column using header and cell content."),
            new GalleryDataGridColumn("region", "Region", "SizeToHeader", 118d, 96d, 160d, "Header-sized text column."),
            new GalleryDataGridColumn("category", "Category", "SizeToCells", 168d, 128d, 240d, "Cell-sized category column."),
            new GalleryDataGridColumn("status", "Status", "Fixed", 118d, 104d, 160d, "Filterable status column."),
            new GalleryDataGridColumn("owner", "Owner", "Star", 150d, 120d, 260d, "Weighted star owner column."),
            new GalleryDataGridColumn("amount", "Amount", "Fixed", 120d, 112d, 160d, "Sortable numeric column."),
            new GalleryDataGridColumn("progress", "Progress", "Fill", 130d, 120d, 220d, "Fill column for progress."),
            new GalleryDataGridColumn("updated", "Updated", "Fixed", 132d, 120d, 160d, "Sortable date column."),
            new GalleryDataGridColumn("notes", "Notes", "LastColumnFill", 360d, 220d, 900d, "Dynamic text content column.")
        ];
    }

    public static double GetWrapMainAxisExtent(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Interpolate(WrapMinMainAxisExtent, WrapMaxMainAxisExtent, ((index * 37) + 17) % 101);
    }

    public static double GetWrapCrossAxisExtent(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Interpolate(WrapMinCrossAxisExtent, WrapMaxCrossAxisExtent, ((index * 53) + 29) % 101);
    }

    public static double GetStackMainAxisExtent(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Interpolate(StackMinMainAxisExtent, StackMaxMainAxisExtent, ((index * 43) + 11) % 101);
    }

    public static double GetStackCrossAxisExtent(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Interpolate(StackMinCrossAxisExtent, StackMaxCrossAxisExtent, ((index * 61) + 23) % 101);
    }

    public static double GetDataGridRowExtent(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Interpolate(DataGridMinRowExtent, DataGridMaxRowExtent, ((index * 47) + 19) % 101);
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
                "variable-stack",
                "Variable stack layout",
                "Non-uniform width and height cards stack linearly with cache-aware offsets over a 100,000-item source.",
                "SliverList with custom variable-size child delegate",
                "Variable-size sliver stack layout",
                GalleryScenarioKind.VariableStack,
                100_000,
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
                "data-grid",
                "DataGrid sliver",
                "A 100,000-row grid projects sorting, filtering, variable row heights, and mixed column sizing through native row controls.",
                "TableView + DataTable + SfDataGrid patterns",
                "DataGrid sliver layout and query projection",
                GalleryScenarioKind.DataGrid,
                100_000,
                UsesVirtualization: true,
                UsesVariableExtents: true),
            new GalleryScenario(
                "variable-wrap",
                "Variable wrap layout",
                "Non-uniform width and height chips flow into cache-aware wrap lines without realizing the 100,000-item source.",
                "Custom RenderSliver / SliverLayoutBuilder with wrap-style line packing",
                "Variable-size sliver wrap layout",
                GalleryScenarioKind.VariableWrap,
                100_000,
                UsesVirtualization: true,
                UsesVariableExtents: true),
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
                "nested-tabs",
                "Tabbed nested scroll",
                "A pinned header owns a tab strip while each tab keeps its own scroll body and overlap spacing.",
                "NestedScrollView + SliverOverlapAbsorber + SliverAppBar + TabBar",
                "Tabbed nested scroll and overlap coordination",
                GalleryScenarioKind.TabbedNestedScroll,
                900,
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

    private static double Interpolate(double minimum, double maximum, int bucket)
    {
        if (Math.Abs(maximum - minimum) <= 0.0001d)
        {
            return minimum;
        }

        return minimum + ((maximum - minimum) * bucket / 100d);
    }
}
