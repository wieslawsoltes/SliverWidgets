using Microsoft.Maui.Controls;

namespace SliverWidgets.Maui;

public class SliverDataGridCollectionView : CollectionView
{
    public static readonly BindableProperty CacheExtentProperty =
        BindableProperty.Create(
            nameof(CacheExtent),
            typeof(double),
            typeof(SliverDataGridCollectionView),
            0d,
            validateValue: (_, value) => value is double extent && !double.IsNaN(extent) && !double.IsInfinity(extent) && extent >= 0d);

    public SliverDataGridCollectionView()
    {
        ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
        ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical);
    }

    public double CacheExtent
    {
        get => (double)GetValue(CacheExtentProperty);
        set => SetValue(CacheExtentProperty, value);
    }

    public bool UsesNativeVirtualization => true;
}
