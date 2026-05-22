---
title: SliverWidgets
description: Professional documentation for the SliverWidgets sliver layout system.
---

# SliverWidgets

SliverWidgets is a Flutter-inspired sliver layout system for .NET UI frameworks. It provides a framework-neutral layout core plus native adapters for Avalonia, Uno, MAUI, and WinUI.

The library is designed for high-performance scrolling surfaces where pinned headers, lazy lists, adaptive grids, fill-remaining regions, padding, and visibility should compose in one viewport without replacing each framework's controls, styling, input, or accessibility stack.

## What SliverWidgets Provides

- A pure .NET layout protocol in `SliverWidgets.Core`.
- Arithmetic fixed-extent lists for very large row counts.
- Variable-extent list support with observed extent caching and dead reckoning.
- Grid layouts with fixed cross-axis counts or max cross-axis extents.
- Persistent, pinned, floating, and snapping header primitives.
- Viewport composition for mixed sliver sequences.
- Avalonia panels and virtualizing panels.
- MAUI layouts and native-backed `CollectionView` integration.
- Uno and WinUI `ItemsRepeater` `VirtualizingLayout` adapters.
- Gallery sample apps that demonstrate large-data scenarios across frameworks.

## Documentation Map

Start with [Concepts](concepts.html) if you are new to slivers. Use [Getting Started](getting-started.html) for installation and first code, then move to [Controls](controls/) and [Framework Adapters](framework-adapters/) for framework-specific usage.

The [API Guide](api/) explains the public API by package. API enrichment files under each project `apidocs/` folder are available for Lunet `api.dotnet` extraction when generated reference output is enabled.

## Package Set

| Package | Purpose |
|---|---|
| `SliverWidgets.Core` | Framework-neutral constraints, geometry, layouts, caches, and viewport composition. |
| `SliverWidgets.Avalonia` | Avalonia panels, persistent headers, fixed virtualizing panels, and variable-height virtualizing panels. |
| `SliverWidgets.Maui` | MAUI `Layout` integration and native-backed `SliverCollectionView` virtualization. |
| `SliverWidgets.Uno` | Uno `ItemsRepeater` row and grid virtualizing layouts. |
| `SliverWidgets.WinUI` | Windows App SDK `ItemsRepeater` row and grid virtualizing layouts. |

## First Example

```csharp
using SliverWidgets.Core;

var viewport = new SliverViewport(
    MainAxisExtent: 720,
    CrossAxisExtent: 1024);

var engine = new SliverViewportLayoutEngine();
var result = engine.Layout(
    new ISliverLayout[]
    {
        new SliverPersistentHeaderLayout(
            new SliverPersistentHeaderOptions(48, 144, Pinned: true)),
        new SliverFixedExtentListLayout(
            new SliverFixedExtentListOptions(100_000, 36, 4))
    },
    viewport,
    scrollOffset: 12_000,
    cacheExtent: 480);
```

`result.Slots` contains the realized paint/cache window. Framework adapters translate those slots into native measurement and arrangement calls.
