namespace SliverWidgets.GalleryData;

public sealed record GalleryDataGridColumn(
    string Key,
    string Header,
    string WidthMode,
    double Width,
    double MinWidth,
    double MaxWidth,
    string Description);
