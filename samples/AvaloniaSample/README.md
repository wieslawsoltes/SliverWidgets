# Avalonia Sample

For the full desktop gallery, run:

```bash
dotnet run --project samples/AvaloniaGallery/AvaloniaGallery.csproj
```

```xml
<ScrollViewer>
  <slivers:SliverStackPanel
      xmlns:slivers="using:SliverWidgets.Avalonia"
      ItemExtent="44"
      Spacing="2"
      CacheExtent="400">
    <TextBlock Text="Row 1" />
    <TextBlock Text="Row 2" />
  </slivers:SliverStackPanel>
</ScrollViewer>
```

Bind `ScrollOffset` to the hosting scroll state when using the panel as a clipped viewport.
