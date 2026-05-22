# WinUI Sample

The runnable gallery lives in `samples/WinUIGallery` and references:

- `src/SliverWidgets.Core`
- `src/SliverWidgets.WinUI`
- `samples/SliverWidgets.GalleryData`

It demonstrates:

- `ItemsRepeater` with `SliverFixedExtentVirtualizingLayout`
- `ItemsRepeater` with `SliverGridVirtualizingLayout`
- a 100,000 item deterministic data source
- live item extent, spacing, and `ItemsRepeater.VerticalCacheLength` controls
- pinned and floating header concepts using native WinUI composition around virtualized repeaters

Build from any configured host:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj
```

On non-Windows hosts, the C# projects compile with PRI generation disabled. Run validation still belongs on Windows because the package targets Windows App SDK.

```xml
<ScrollViewer>
  <ItemsRepeater ItemsSource="{x:Bind Items}">
    <ItemsRepeater.Layout>
      <slivers:SliverFixedExtentVirtualizingLayout
          xmlns:slivers="using:SliverWidgets.WinUI"
          ItemExtent="44"
          Spacing="2" />
    </ItemsRepeater.Layout>
  </ItemsRepeater>
</ScrollViewer>
```

Validate this package on Windows because Windows App SDK runtime behavior is Windows-only.

For grids:

```xml
<ScrollViewer>
  <ItemsRepeater ItemsSource="{x:Bind Items}">
    <ItemsRepeater.Layout>
      <slivers:SliverGridVirtualizingLayout
          xmlns:slivers="using:SliverWidgets.WinUI"
          CrossAxisCount="3"
          MainAxisSpacing="8"
          CrossAxisSpacing="8" />
    </ItemsRepeater.Layout>
  </ItemsRepeater>
</ScrollViewer>
```
