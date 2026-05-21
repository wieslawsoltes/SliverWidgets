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

- Fixed large list: `SliverVirtualizingStackPanel` with adjustable fixed item extent and cache extent.
- Variable/non-uniform list: `SliverVirtualizingListPanel` measuring native item templates and estimating unobserved rows.
- Adaptive grid: `SliverGridPanel` using max cross-axis extent sizing.
- Pinned/collapsible header: `SliverPersistentHeader` showing pinned min/max extent behavior.
- Mixed sliver composition: a sample-only `MixedSliverPreviewPanel` using `SliverViewportLayoutEngine` to compose a pinned header, fixed rows, padded grid, and fill-remaining region in one viewport. Direct children are clipped below the active pinned-header obstruction so partially visible slots do not render through the header.
- Sectioned/sticky headers: Avalonia-local projections of shared gallery sections rendered through `SliverScenarioStackPanel` with configurable stacked or push sticky headers. Stacked is the default mode.
- Fill, padding, and visibility: `SliverScenarioStackPanel` demonstrating `SliverPadding`, `SliverToBoxAdapter`, `SliverVisibility`, and `SliverFillRemaining`.
- Cache/performance stress: `SliverVirtualizingStackPanel` over 100,000 deterministic rows from `samples/SliverWidgets.GalleryData`.

The gallery references:

- `src/SliverWidgets.Avalonia`
- `src/SliverWidgets.Core`
- `samples/SliverWidgets.GalleryData`
