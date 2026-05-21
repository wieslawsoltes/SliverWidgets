---
title: Sample Galleries
description: Framework sample galleries and validation commands.
---

# Sample Galleries

The repository includes gallery-style samples for each supported framework. They mirror Flutter's sliver teaching examples with native .NET controls: expanded or persistent headers, adaptive grids, fixed-extent lists, variable-size content, and mixed scroll compositions.

The shared sample data lives in `samples/SliverWidgets.GalleryData` and creates deterministic rows, tiles, sections, metrics, and demo descriptions. Keeping the data shared makes virtualization behavior comparable across Avalonia, MAUI, Uno, and WinUI. The galleries also share the Avalonia reference shell: a top metrics header, short scenario tabs (`Fixed`, `Variable`, `Grid`, `Header`, `Tabs`, `Mixed`, `Sections`, `Fill`, `Cache`), a left controls/notes panel, and a right framework-native viewport.

## Coverage Matrix

| Flutter concept | SliverWidgets gallery coverage |
|---|---|
| `CustomScrollView` with mixed slivers | Avalonia mixed preview with pinned-obstruction clipping, Uno custom-scroll page, MAUI demo cards, WinUI composite pages |
| `SliverAppBar` / persistent header | Avalonia `SliverPersistentHeader` and configurable stacked or push section headers, WinUI pinned/floating header concept, MAUI grouped headers |
| `NestedScrollView` with tabs and overlap | Avalonia native `TabControl` with sliver-backed tab bodies, MAUI segmented tab buttons with native `CollectionView` bodies, Uno/WinUI segmented tab buttons with `ItemsRepeater` bodies |
| `SliverGrid` | Avalonia `SliverVirtualizingGridPanel` for large grids and `SliverGridPanel` for bounded direct children, MAUI `SliverCollectionView` grid mode, Uno/WinUI `ItemsRepeater` grid layouts |
| `SliverFixedExtentList` | Core fixed extent layout plus Avalonia, MAUI, Uno, and WinUI fixed list samples |
| Lazy child lifecycle and cache windows | Avalonia virtualizing panel, MAUI native `CollectionView`, Uno/WinUI `ItemsRepeater` realization windows |

## Projects

| Project | Purpose |
|---|---|
| `samples/SliverWidgets.GalleryData` | Shared deterministic data, metrics, and demo descriptions. |
| `samples/AvaloniaGallery` | Desktop gallery for panels, virtualizing panels, persistent headers, and mixed slivers. |
| `samples/MauiGallery` | MAUI Mac Catalyst gallery for `SliverStackLayout` and `SliverCollectionView`, using the shared Avalonia-style shell. |
| `samples/UnoGallery` | Reusable Uno gallery page with `ItemsRepeater` examples, using the shared Avalonia-style shell. |
| `samples/UnoGalleryApp` | Uno desktop host for the reusable gallery page. |
| `samples/WinUIGallery` | WinUI gallery for `ItemsRepeater` layouts, using the shared Avalonia-style shell; run validation is Windows-specific. |

## Buildable Gallery Solutions

On macOS with the installed workloads:

```bash
dotnet build SliverWidgets.Galleries.slnx
```

CI builds the no-workload gallery subset with:

```bash
dotnet build SliverWidgets.Galleries.CI.slnx
```

This builds:

- `samples/AvaloniaGallery`
- `samples/MauiGallery`
- `samples/UnoGallery`
- `samples/UnoGalleryApp`
- `samples/SliverWidgets.GalleryData`

## Per-Framework Commands

Avalonia desktop:

```bash
dotnet run --project samples/AvaloniaGallery/AvaloniaGallery.csproj
```

MAUI Mac Catalyst:

```bash
dotnet build samples/MauiGallery/MauiGallery.csproj
```

Uno gallery surface:

```bash
dotnet build samples/UnoGallery/SliverWidgets.UnoGallery.csproj
```

Uno desktop app host:

```bash
dotnet run --project samples/UnoGalleryApp/SliverWidgets.UnoGalleryApp/SliverWidgets.UnoGalleryApp.csproj
```

WinUI gallery:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj
```

The WinUI sample targets Windows App SDK. It can compile on non-Windows hosts with PRI generation disabled; run and device validation should happen on Windows.

## Platform Limitations

- Avalonia provides the most complete single-viewport sample coverage through the mixed and sectioned panels. The standalone `SliverPersistentHeader` decorator is externally coordinated by a scroll offset.
- Uno currently lacks an implemented `VirtualizingLayoutContext.VisibleRect`, so its adapter infers the visible paint range from `RealizationRect` and available size.
- MAUI delegates large-data realization to native `CollectionView`; its cache distance is platform-owned.
- WinUI, Uno, and MAUI mixed and tabbed pages demonstrate the concepts with native surfaces, but they are not yet one shared `CustomScrollView`/`NestedScrollView`-style viewport pipeline.

## What to Look For

When running a gallery:

- fixed lists should stay responsive with large item counts
- grids should adapt to viewport width without reflow jitter
- persistent headers should shrink and pin predictably; Avalonia section headers default to stacked mode and can switch to push mode
- the `Tabs` scenario should keep its header/tab strip separate from the inner tab scroll bodies
- realization counters should remain close to visible plus cache range
- mixed compositions should scroll as a single surface, with content clipped below pinned headers instead of visually overlapping them
