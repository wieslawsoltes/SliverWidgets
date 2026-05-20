# SliverWidgets MAUI Gallery

Code-only MAUI Mac Catalyst gallery for SliverWidgets.

## Build

```bash
dotnet build samples/MauiGallery/MauiGallery.csproj
```

## Included Demos

- Fixed `SliverStackLayout` preview with live item extent and spacing controls.
- Native-backed `SliverCollectionView` fixed-list virtualization.
- `SliverCollectionView` grid mode with fixed cross-axis count.
- Grouped section headers that demonstrate persistent-header-style composition in MAUI.
- Shared Flutter-inspired demo cards sourced from `samples/SliverWidgets.GalleryData`.

MAUI owns the actual native realization window through `CollectionView`; `CacheExtent` is exposed as SliverWidgets metadata for parity with the other samples.
