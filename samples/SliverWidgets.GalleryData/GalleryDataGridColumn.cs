namespace SliverWidgets.GalleryData;

public sealed record GalleryDataGridColumn(
    string Key,
    string Header,
    string WidthMode,
    double Width,
    double MinWidth,
    double MaxWidth,
    string Description,
    double ResolvedWidth = 0d,
    double HeaderWidth = 0d,
    double CellWidth = 0d,
    double StarWeight = 1d,
    bool IsVisible = true)
{
    public double EffectiveWidth => ResolvedWidth > 0d ? ResolvedWidth : Width;
}
