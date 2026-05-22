namespace SliverWidgets.GalleryData;

public sealed record GalleryItem(
    int Id,
    string Title,
    string Subtitle,
    string Category,
    string AccentColor,
    double Extent,
    int Rank,
    bool IsPinnedCandidate);
