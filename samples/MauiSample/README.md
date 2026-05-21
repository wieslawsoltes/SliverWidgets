# MAUI Sample

## Gallery app

The buildable code-only gallery lives in `../MauiGallery`. It references `SliverWidgets.Maui`, `SliverWidgets.Core`, and `SliverWidgets.GalleryData`, and demonstrates the unified sliver scenario catalog: fixed large list, variable/non-uniform list, adaptive grid, pinned/collapsible header concept, mixed composition, sectioned/sticky-header concept, fill/padding/visibility composition, and cache/performance stress.

```bash
dotnet build samples/MauiGallery/MauiGallery.csproj
```

## Inline layout sample

```xml
<ScrollView>
  <slivers:SliverStackLayout
      xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
      ItemExtent="44"
      Spacing="2">
    <Label Text="Row 1" />
    <Label Text="Row 2" />
  </slivers:SliverStackLayout>
</ScrollView>
```

This custom layout sample measures explicit children and does not virtualize large item sources.

## Native-backed virtualization

Use `SliverCollectionView` for large item sources. It wraps MAUI `CollectionView` so Android, iOS, macOS, and Windows handlers keep native item recycling and realization behavior.

```xml
<slivers:SliverCollectionView
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemsSource="{Binding Rows}"
    Axis="Vertical"
    LayoutMode="FixedExtentList"
    ItemExtent="44"
    Spacing="2">
  <slivers:SliverCollectionView.ItemTemplate>
    <DataTemplate>
      <Label Text="{Binding Title}" HeightRequest="44" />
    </DataTemplate>
  </slivers:SliverCollectionView.ItemTemplate>
</slivers:SliverCollectionView>
```

For grids, set `LayoutMode="FixedExtentGrid"` and `CrossAxisCount`. The gallery derives that count from the available width to model Flutter's max-cross-axis-extent grid concept on top of MAUI's fixed-span grid handler.

MAUI does not expose a direct per-item extent property on `CollectionView`, so `ItemExtent` records the sliver contract and the item template should set the matching `HeightRequest` for vertical layouts or `WidthRequest` for horizontal layouts. Variable/non-uniform samples use native `CollectionView` measurement instead of `SliverCollectionView`. `CacheExtent` is retained as sliver metadata; MAUI handlers own the actual realization window.
