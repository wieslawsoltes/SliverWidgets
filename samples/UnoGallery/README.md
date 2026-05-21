# SliverWidgets Uno Gallery

This code-only sample exposes a WinUI-style `UnoGalleryPage` that can be hosted from an Uno app shell:

```csharp
window.Content = new SliverWidgets.Samples.UnoGallery.UnoGalleryPage();
```

The gallery demonstrates:

- The same Avalonia-derived shell used by the other samples: top metrics, short scenario tabs, left controls/notes, and a right native viewport.
- Fixed large list virtualization through `SliverFixedExtentVirtualizingLayout`.
- Variable/non-uniform rows using native Uno `StackLayout` virtualization until a sliver variable-extent adapter is available.
- Adaptive max-cross-axis grid virtualization through `SliverGridVirtualizingLayout`.
- Pinned/collapsible header, tabbed nested-scroll projection, mixed composition, sectioned sticky-header, fill/padding/visibility, and cache stress scenarios.
- Runtime controls for extent, spacing, aspect ratio, adaptive tile width, and `ItemsRepeater.VerticalCacheLength`.

The sample targets `net10.0` as a buildable gallery surface on macOS and references `src/SliverWidgets.Uno`, `src/SliverWidgets.Core`, and `samples/SliverWidgets.GalleryData` directly.

For a runnable Uno desktop app host, use:

```bash
dotnet run --project samples/UnoGalleryApp/SliverWidgets.UnoGalleryApp/SliverWidgets.UnoGalleryApp.csproj
```

`VerticalCacheLength` is applied on `ItemsRepeater`, so exact cache behavior can vary by Uno target and renderer. The SliverWidgets Uno layouts still consume the realization rectangle reported by `ItemsRepeater` and lay out only the requested paint/cache slots.
