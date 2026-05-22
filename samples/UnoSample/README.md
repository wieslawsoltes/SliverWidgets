# Uno Sample

The buildable code-only gallery lives in `samples/UnoGallery`. Host
`SliverWidgets.Samples.UnoGallery.UnoGalleryPage` from an Uno app shell to
exercise fixed lists, grids, large data virtualization, cache/spacing/extent
controls, and CustomScrollView-style composition.

```xml
<ScrollViewer>
  <ItemsRepeater ItemsSource="{x:Bind Items}">
    <ItemsRepeater.Layout>
      <slivers:SliverFixedExtentVirtualizingLayout
          xmlns:slivers="using:SliverWidgets.Uno"
          ItemExtent="44"
          Spacing="2" />
    </ItemsRepeater.Layout>
  </ItemsRepeater>
</ScrollViewer>
```

For grids:

```xml
<ScrollViewer>
  <ItemsRepeater ItemsSource="{x:Bind Items}">
    <ItemsRepeater.Layout>
      <slivers:SliverGridVirtualizingLayout
          xmlns:slivers="using:SliverWidgets.Uno"
          CrossAxisCount="3"
          MainAxisSpacing="8"
          CrossAxisSpacing="8" />
    </ItemsRepeater.Layout>
  </ItemsRepeater>
</ScrollViewer>
```
