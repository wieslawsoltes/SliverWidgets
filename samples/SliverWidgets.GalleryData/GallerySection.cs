namespace SliverWidgets.GalleryData;

public sealed record GallerySection(
    string Title,
    string Summary,
    string AccentColor,
    IReadOnlyList<GalleryItem> Items);
