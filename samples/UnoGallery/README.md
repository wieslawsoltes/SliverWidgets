# SliverWidgets Uno Gallery

This code-only sample exposes a WinUI-style `UnoGalleryPage` that can be hosted from an Uno app shell:

```csharp
window.Content = new SliverWidgets.Samples.UnoGallery.UnoGalleryPage();
```

The gallery demonstrates:

- `ItemsRepeater` fixed-list virtualization through `SliverFixedExtentVirtualizingLayout`.
- `ItemsRepeater` grid virtualization through `SliverGridVirtualizingLayout`.
- A 100,000 item source with realized/prepared counters.
- Runtime controls for extent, spacing, aspect ratio, grid columns, and `ItemsRepeater.VerticalCacheLength`.
- A Flutter-inspired CustomScrollView-style composition that combines box panels, fixed-list slivers, and grid slivers in one scroll surface.

The sample targets `net10.0` as a buildable gallery surface on macOS and references `src/SliverWidgets.Uno`, `src/SliverWidgets.Core`, and `samples/SliverWidgets.GalleryData` directly.

For a runnable Uno desktop app host, use:

```bash
dotnet run --project samples/UnoGalleryApp/SliverWidgets.UnoGalleryApp/SliverWidgets.UnoGalleryApp.csproj
```

`VerticalCacheLength` is applied on `ItemsRepeater`, so exact cache behavior can vary by Uno target and renderer. The SliverWidgets Uno layouts still consume the realization rectangle reported by `ItemsRepeater` and lay out only the requested paint/cache slots.
