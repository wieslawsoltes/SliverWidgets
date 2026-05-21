using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SliverWidgets.GalleryData;

namespace AvaloniaGallery;

public sealed class GalleryViewModel : INotifyPropertyChanged
{
    private double _fixedItemExtent = 58d;
    private bool _headerPinned = true;
    private double _cacheExtent = 280d;
    private bool _showOptionalSliver = true;
    private bool _maintainOptionalSliverSize;

    public GalleryViewModel()
    {
        var variableItems = SliverGalleryData.CreateVariableItems(10000);
        var fixedItems = SliverGalleryData.CreateUniformItems(100_000, 64d);
        var stressItems = SliverGalleryData.CreateUniformItems(100_000, 52d);
        var sections = SliverGalleryData.CreateSections(8, 24);

        FixedRows = new ObservableCollection<GalleryItem>(fixedItems);
        PreviewRows = new ObservableCollection<GalleryItem>(variableItems.Take(36));
        VariableRows = new ObservableCollection<GalleryItem>(variableItems.Skip(360).Take(1200));
        GridItems = new ObservableCollection<GalleryItem>(SliverGalleryData.CreateItems(1200));
        StressRows = new ObservableCollection<GalleryItem>(stressItems);
        Metrics = new ObservableCollection<GalleryMetric>(SliverGalleryData.CreateMetrics());
        Demos = new ObservableCollection<GalleryDemo>(SliverGalleryData.CreateDemos());
        Sections = new ObservableCollection<GallerySection>(sections);
        ScenarioCatalog = new ObservableCollection<GalleryScenario>(SliverGalleryData.CreateScenarios());
        SectionedBlocks = new ObservableCollection<AvaloniaSliverBlock>(CreateSectionedBlocks(sections));
        CompositionBlocks = new ObservableCollection<AvaloniaSliverBlock>(CreateCompositionBlocks());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<GalleryItem> FixedRows { get; }

    public ObservableCollection<GalleryItem> PreviewRows { get; }

    public ObservableCollection<GalleryItem> VariableRows { get; }

    public ObservableCollection<GalleryItem> GridItems { get; }

    public ObservableCollection<GalleryItem> StressRows { get; }

    public ObservableCollection<GalleryMetric> Metrics { get; }

    public ObservableCollection<GalleryDemo> Demos { get; }

    public ObservableCollection<GallerySection> Sections { get; }

    public ObservableCollection<GalleryScenario> ScenarioCatalog { get; }

    public ObservableCollection<AvaloniaSliverBlock> SectionedBlocks { get; }

    public ObservableCollection<AvaloniaSliverBlock> CompositionBlocks { get; }

    public double FixedItemExtent
    {
        get => _fixedItemExtent;
        set => SetField(ref _fixedItemExtent, value);
    }

    public bool HeaderPinned
    {
        get => _headerPinned;
        set => SetField(ref _headerPinned, value);
    }

    public double CacheExtent
    {
        get => _cacheExtent;
        set => SetField(ref _cacheExtent, value);
    }

    public bool ShowOptionalSliver
    {
        get => _showOptionalSliver;
        set => SetField(ref _showOptionalSliver, value);
    }

    public bool MaintainOptionalSliverSize
    {
        get => _maintainOptionalSliverSize;
        set => SetField(ref _maintainOptionalSliverSize, value);
    }

    private static IEnumerable<AvaloniaSliverBlock> CreateSectionedBlocks(IEnumerable<GallerySection> sections)
    {
        var globalIndex = 0;
        foreach (var section in sections)
        {
            yield return new AvaloniaSliverBlock(
                AvaloniaSliverBlockKind.Header,
                section.Title,
                section.Summary,
                section.AccentColor,
                74d,
                globalIndex++);

            foreach (var item in section.Items.Take(10))
            {
                yield return new AvaloniaSliverBlock(
                    AvaloniaSliverBlockKind.Box,
                    item.Title,
                    item.Subtitle,
                    item.AccentColor,
                    54d + (item.Id % 3 * 6d),
                    globalIndex++);
            }
        }
    }

    private static IEnumerable<AvaloniaSliverBlock> CreateCompositionBlocks()
    {
        yield return new AvaloniaSliverBlock(
            AvaloniaSliverBlockKind.PaddedBox,
            "SliverPadding",
            "Inset box adapter content keeps native controls away from viewport edges.",
            "#2563EB",
            112d,
            0);
        yield return new AvaloniaSliverBlock(
            AvaloniaSliverBlockKind.Box,
            "SliverToBoxAdapter",
            "A normal Avalonia control participates as a single sliver.",
            "#0F766E",
            96d,
            1);
        yield return new AvaloniaSliverBlock(
            AvaloniaSliverBlockKind.Visibility,
            "SliverVisibility",
            "Toggle this slot to remove it or keep its scroll footprint.",
            "#DB2777",
            120d,
            2);
        yield return new AvaloniaSliverBlock(
            AvaloniaSliverBlockKind.FillRemaining,
            "SliverFillRemaining",
            "Fills slack space when preceding content is shorter than the viewport.",
            "#9333EA",
            240d,
            3);
        yield return new AvaloniaSliverBlock(
            AvaloniaSliverBlockKind.Box,
            "Trailing sliver",
            "A final fixed box proves the fill region still composes with later content.",
            "#EA580C",
            140d,
            4);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed record AvaloniaSliverBlock(
    AvaloniaSliverBlockKind Kind,
    string Title,
    string Summary,
    string AccentColor,
    double Extent,
    int Index);

public enum AvaloniaSliverBlockKind
{
    Box,
    Header,
    PaddedBox,
    FillRemaining,
    Visibility
}
