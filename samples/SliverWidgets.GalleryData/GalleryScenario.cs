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
    bool UsesVariableExtents);

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
