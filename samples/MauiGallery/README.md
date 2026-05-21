# SliverWidgets MAUI Gallery

Code-only MAUI Mac Catalyst gallery for SliverWidgets. The gallery projects the shared
Flutter-inspired scenario catalog onto MAUI-native controls without duplicating the
shared gallery data.

## Build

```bash
dotnet build samples/MauiGallery/MauiGallery.csproj
```

## Included Demos

- Fixed large list through native-backed `SliverCollectionView` fixed-list virtualization.
- Variable/non-uniform list through native MAUI `CollectionView` measured row templates.
- Adaptive grid through `SliverCollectionView` grid mode with width-derived column count.
- Pinned/collapsible header concept driven by native `CollectionView.Scrolled` offsets.
- Mixed `CustomScrollView`-style composition with MAUI `ScrollView`, fixed rows, grid tiles, and fill content.
- Sectioned/sticky-header concept through grouped `SliverCollectionView` rows.
- Fill remaining, padding, and visibility composition through native MAUI layout primitives.
- Cache/performance stress with a 100,000-row source and live cache metadata.

MAUI owns the actual native realization window through `CollectionView`; `CacheExtent` is exposed as SliverWidgets metadata for parity with the other samples. Group header stickiness and exact cache realization remain platform-handler behavior, so the gallery labels those surfaces as concepts instead of hiding the limitation.
