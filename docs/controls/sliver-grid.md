---
title: Sliver Grids
description: Fixed-count and max-cross-axis grid layouts.
---

# Sliver Grids

`SliverGridLayout` arranges items in rows for vertical scrolling or columns for horizontal scrolling. It supports two sizing modes:

- `FixedCrossAxisCount`
- `MaxCrossAxisExtent`

## Fixed Cross-Axis Count

Use a fixed count when product design needs a known number of columns or rows:

```csharp
var layout = new SliverGridLayout(
    SliverGridLayoutOptions.FixedCrossAxisCount(
        itemCount: 1_000,
        crossAxisCount: 4,
        mainAxisSpacing: 12,
        crossAxisSpacing: 12,
        childAspectRatio: 1.2));
```

The core divides available cross-axis space evenly after subtracting spacing.

## Max Cross-Axis Extent

Use max extent for responsive galleries:

```csharp
var layout = new SliverGridLayout(
    SliverGridLayoutOptions.WithMaxCrossAxisExtent(
        itemCount: 1_000,
        maxCrossAxisExtent: 260,
        mainAxisSpacing: 12,
        crossAxisSpacing: 12,
        childAspectRatio: 1.4));
```

The core chooses the largest count that keeps tiles within the configured maximum.

## Main-Axis Extent

By default, tile main-axis extent is derived from cross-axis extent and `ChildAspectRatio`. Set `MainAxisExtent` when the tile height or width should be fixed regardless of aspect ratio:

```csharp
var options = SliverGridLayoutOptions.FixedCrossAxisCount(
    itemCount: 1_000,
    crossAxisCount: 3,
    mainAxisExtent: 180);
```

## Framework Mapping

| Framework | Grid adapter |
|---|---|
| Avalonia | `SliverGridPanel` |
| MAUI | `SliverCollectionView` with `LayoutMode = FixedExtentGrid` |
| Uno | `SliverGridVirtualizingLayout` |
| WinUI | `SliverGridVirtualizingLayout` |

## Guidance

- Prefer `MaxCrossAxisExtent` for resizable desktop windows.
- Prefer `FixedCrossAxisCount` for predictable dashboards or dense tools.
- Use explicit `MainAxisExtent` when item template height is expensive or should not depend on viewport width.
- Keep tile templates light; grid realization can produce multiple children per visible row.
