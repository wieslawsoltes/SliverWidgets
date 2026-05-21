namespace SliverWidgets.GalleryData;

public sealed record GalleryScenario(
    string Key,
    string Title,
    string Summary,
    string FlutterReference,
    string SupportedFeature,
    GalleryScenarioKind Kind,
    int ItemCount,
    bool UsesVirtualization,
    bool UsesVariableExtents)
{
    public string TabLabel => Kind switch
    {
        GalleryScenarioKind.FixedExtentList => "Fixed",
        GalleryScenarioKind.VariableExtentList => "Variable",
        GalleryScenarioKind.AdaptiveGrid => "Grid",
        GalleryScenarioKind.VariableWrap => "Wrap",
        GalleryScenarioKind.PinnedHeader => "Header",
        GalleryScenarioKind.TabbedNestedScroll => "Tabs",
        GalleryScenarioKind.MixedComposition => "Mixed",
        GalleryScenarioKind.SectionedHeaders => "Sections",
        GalleryScenarioKind.FillPaddingVisibility => "Fill",
        GalleryScenarioKind.CacheStress => "Cache",
        _ => Title
    };
}

public enum GalleryScenarioKind
{
    FixedExtentList,
    VariableExtentList,
    AdaptiveGrid,
    VariableWrap,
    PinnedHeader,
    TabbedNestedScroll,
    SectionedHeaders,
    MixedComposition,
    FillPaddingVisibility,
    CacheStress
}
