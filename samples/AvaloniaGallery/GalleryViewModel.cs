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

    public GalleryViewModel()
    {
        var items = SliverGalleryData.CreateItems(10000);
        FixedRows = new ObservableCollection<GalleryItem>(items.Take(36));
        GridItems = new ObservableCollection<GalleryItem>(items.Skip(120).Take(30));
        LargeRows = new ObservableCollection<GalleryItem>(items);
        Metrics = new ObservableCollection<GalleryMetric>(SliverGalleryData.CreateMetrics());
        Demos = new ObservableCollection<GalleryDemo>(SliverGalleryData.CreateDemos());
        Sections = new ObservableCollection<GallerySection>(SliverGalleryData.CreateSections(5, 24));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<GalleryItem> FixedRows { get; }

    public ObservableCollection<GalleryItem> GridItems { get; }

    public ObservableCollection<GalleryItem> LargeRows { get; }

    public ObservableCollection<GalleryMetric> Metrics { get; }

    public ObservableCollection<GalleryDemo> Demos { get; }

    public ObservableCollection<GallerySection> Sections { get; }

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
