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
        GalleryScenarioKind.PinnedHeader => "Header",
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
    PinnedHeader,
    SectionedHeaders,
    MixedComposition,
    FillPaddingVisibility,
    CacheStress
}
