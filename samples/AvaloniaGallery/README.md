# SliverWidgets Avalonia Gallery

Avalonia desktop gallery for the SliverWidgets Avalonia adapter. The sample uses native Avalonia controls as children and drives the sliver panels with explicit scroll-offset and cache controls so the layout behavior is easy to inspect.

## Run

```bash
dotnet run --project samples/AvaloniaGallery/AvaloniaGallery.csproj
```

## Build

```bash
dotnet build samples/AvaloniaGallery/AvaloniaGallery.csproj
```

## Included Demos

- Fixed stack panel: `SliverStackPanel` with adjustable fixed item extent, scroll offset, and cache extent.
- Grid panel: `SliverGridPanel` using max cross-axis extent sizing.
- Persistent header: `SliverPersistentHeader` showing pinned and collapsible min/max extent behavior.
- Virtualizing list: `SliverVirtualizingStackPanel` over 10,000 deterministic rows from `samples/SliverWidgets.GalleryData`.
- Mixed sliver composition: a sample-only `MixedSliverPreviewPanel` using `SliverViewportLayoutEngine` to compose a pinned header, fixed rows, padded grid, and fill-remaining region in one viewport.

The gallery references:

- `src/SliverWidgets.Avalonia`
- `src/SliverWidgets.Core`
- `samples/SliverWidgets.GalleryData`
