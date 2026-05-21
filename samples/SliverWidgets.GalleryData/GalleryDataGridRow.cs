namespace SliverWidgets.GalleryData;

public sealed record GalleryDataGridRow(
    int Id,
    string Account,
    string Region,
    string Category,
    string Status,
    string Owner,
    double Amount,
    int Progress,
    DateTime Updated,
    string Notes,
    double Extent,
    string AccentColor,
    int Rank);
