---
title: Getting Started
description: Install, build, and use the first SliverWidgets layout.
---

# Getting Started

## Prerequisites

- .NET 10 SDK.
- A framework-specific package for the UI stack you are using.
- Windows for full WinUI validation and sample execution.
- Platform workloads for MAUI sample heads.

The default solution builds the cross-platform packages and tests. The WinUI package can compile on non-Windows hosts with PRI generation disabled, but runtime validation remains a Windows lane.

## Build from Source

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
```

Create local packages:

```bash
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

Build the gallery set supported by the current host:

```bash
dotnet build SliverWidgets.Galleries.slnx
```

The CI-friendly gallery solution excludes platform heads that require extra workloads:

```bash
dotnet build SliverWidgets.Galleries.CI.slnx
```

## Choose a Package

| Framework | Package | Typical entry point |
|---|---|---|
| Core-only layout engine | `SliverWidgets.Core` | `SliverViewportLayoutEngine` |
| Avalonia | `SliverWidgets.Avalonia` | `SliverStackPanel`, `SliverGridPanel`, `SliverVirtualizingStackPanel`, `SliverVirtualizingStackLayoutPanel`, `SliverVirtualizingDataGridRowsPanel`, `SliverVirtualizingWrapPanel` |
| MAUI | `SliverWidgets.Maui` | `SliverStackLayout`, `SliverCollectionView` |
| Uno | `SliverWidgets.Uno` | `SliverFixedExtentVirtualizingLayout`, `SliverStackVirtualizingLayout`, `SliverGridVirtualizingLayout`, `SliverWrapVirtualizingLayout` |
| WinUI | `SliverWidgets.WinUI` | `SliverFixedExtentVirtualizingLayout`, `SliverStackVirtualizingLayout`, `SliverGridVirtualizingLayout`, `SliverWrapVirtualizingLayout` |

## Core Layout Example

Use the core when you need deterministic layout math, tests, or a custom adapter.

```csharp
using SliverWidgets.Core;

var layout = new SliverGridLayout(
    SliverGridLayoutOptions.WithMaxCrossAxisExtent(
        itemCount: 10_000,
        maxCrossAxisExtent: 240,
        mainAxisSpacing: 12,
        crossAxisSpacing: 12,
        childAspectRatio: 1.4));

var result = layout.Layout(new SliverConstraints(
    Axis: SliverAxis.Vertical,
    ScrollOffset: 4_800,
    PrecedingScrollExtent: 0,
    Overlap: 0,
    RemainingPaintExtent: 720,
    CrossAxisExtent: 1024,
    ViewportMainAxisExtent: 720,
    CacheOrigin: -360,
    RemainingCacheExtent: 1_440));
```

The returned slots identify which items should exist and where they should be arranged.

## Avalonia Example

```xml
<slivers:SliverStackPanel
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemExtent="44"
    Spacing="2"
    CacheExtent="400">
  <TextBlock Text="Row 1" />
  <TextBlock Text="Row 2" />
</slivers:SliverStackPanel>
```

For large `ItemsControl` sources, use `SliverVirtualizingStackPanel`, `SliverVirtualizingStackLayoutPanel`, `SliverVirtualizingDataGridRowsPanel`, `SliverVirtualizingListPanel`, or `SliverVirtualizingWrapPanel` as the items panel.

## Uno and WinUI Example

```xml
<ItemsRepeater ItemsSource="{x:Bind Items}">
  <ItemsRepeater.Layout>
    <slivers:SliverFixedExtentVirtualizingLayout
        ItemExtent="44"
        Spacing="2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

Use `SliverStackVirtualizingLayout` for variable-width/height cards, `SliverGridVirtualizingLayout` for adaptive tile views, and `SliverWrapVirtualizingLayout` for variable-size chip flows.

## MAUI Example

```xml
<slivers:SliverCollectionView
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemsSource="{Binding Items}"
    LayoutMode="FixedExtentList"
    ItemExtent="52"
    Spacing="4" />
```

Use `SliverCollectionView` for large data sets because it keeps MAUI's native `CollectionView` virtualization path.
