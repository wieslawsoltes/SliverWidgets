---
title: Sample Galleries
description: Framework sample galleries and validation commands.
---

# Sample Galleries

The repository includes gallery-style samples for each supported framework. They mirror Flutter's sliver teaching examples with native .NET controls: expanded or persistent headers, adaptive grids, fixed-extent lists, variable-size content, and mixed scroll compositions.

The shared sample data lives in `samples/SliverWidgets.GalleryData` and creates deterministic rows, tiles, sections, metrics, and demo descriptions. Keeping the data shared makes virtualization behavior comparable across Avalonia, MAUI, Uno, and WinUI.

## Coverage Matrix

| Flutter concept | SliverWidgets gallery coverage |
|---|---|
| `CustomScrollView` with mixed slivers | Avalonia mixed preview, Uno custom-scroll page, MAUI demo cards, WinUI composite pages |
| `SliverAppBar` / persistent header | Avalonia `SliverPersistentHeader`, WinUI pinned/floating header concept, MAUI grouped headers |
| `SliverGrid` | Avalonia `SliverGridPanel`, MAUI `SliverCollectionView` grid mode, Uno/WinUI `ItemsRepeater` grid layouts |
| `SliverFixedExtentList` | Core fixed extent layout plus Avalonia, MAUI, Uno, and WinUI fixed list samples |
| Lazy child lifecycle and cache windows | Avalonia virtualizing panel, MAUI native `CollectionView`, Uno/WinUI `ItemsRepeater` realization windows |

## Projects

| Project | Purpose |
|---|---|
| `samples/SliverWidgets.GalleryData` | Shared deterministic data, metrics, and demo descriptions. |
| `samples/AvaloniaGallery` | Desktop gallery for panels, virtualizing panels, persistent headers, and mixed slivers. |
| `samples/MauiGallery` | MAUI Mac Catalyst gallery for `SliverStackLayout` and `SliverCollectionView`. |
| `samples/UnoGallery` | Reusable Uno gallery page with `ItemsRepeater` examples. |
| `samples/UnoGalleryApp` | Uno desktop host for the reusable gallery page. |
| `samples/WinUIGallery` | WinUI gallery for `ItemsRepeater` layouts; run validation is Windows-specific. |

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

## What to Look For

When running a gallery:

- fixed lists should stay responsive with large item counts
- grids should adapt to viewport width without reflow jitter
- persistent headers should shrink and pin predictably
- realization counters should remain close to visible plus cache range
- mixed compositions should scroll as a single surface, not as nested scroll views
