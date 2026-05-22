---
title: Sliver Lists
description: Fixed, known-variable, and measured-variable list layouts.
---

# Sliver Lists

SliverWidgets provides three list models:

- `SliverFixedExtentListLayout` for uniform rows
- `SliverListLayout` for known variable extents
- `SliverVariableExtentListLayout` for measured variable extents

## Fixed Extent Lists

`SliverFixedExtentListLayout` is the preferred path for uniform rows because it computes visible indexes by arithmetic.

```csharp
var layout = new SliverFixedExtentListLayout(
    new SliverFixedExtentListOptions(
        ItemCount: 100_000,
        ItemExtent: 44,
        Spacing: 2));
```

The total scroll extent is:

```text
(itemCount * itemExtent) + ((itemCount - 1) * spacing)
```

Use this for tables, command lists, log rows, chat rows with constrained height, or any feed where product design can normalize row height.

## Known Variable Lists

`SliverListLayout` accepts a full list of item extents:

```csharp
var layout = new SliverListLayout(
    new SliverListOptions(
        ItemExtents: new[] { 40d, 64d, 52d, 80d },
        Spacing: 4));
```

This model is useful when item sizes are known from data or precomputed metadata. It does not need to measure children to estimate scroll extent.

## Measured Variable Lists

`SliverVariableExtentListLayout` combines `SliverChildExtentCache` with average-based dead reckoning for missing item extents. Framework adapters can observe measured child sizes and refine the cache over time.

```csharp
var cache = new SliverChildExtentCache(10_000, defaultExtent: 56, spacing: 4);
cache.Observe(0, 72);
cache.Observe(1, 48);

var layout = new SliverVariableExtentListLayout(cache);
```

This is the right model for dynamic text, expandable rows, and content cards where exact height is not known until measure.

## Slots

All list layouts return slots for the paint plus cache ranges:

```csharp
foreach (var slot in result.Slots)
{
    if (!slot.IsCacheOnly)
    {
        // Visible now.
    }
}
```

Framework adapters use slots to decide which children should be realized.

## Adapter Mapping

| Framework | Fixed extent | Variable extent |
|---|---|---|
| Avalonia | `SliverStackPanel`, `SliverVirtualizingStackPanel` | `SliverVirtualizingListPanel` |
| MAUI | `SliverStackLayout`, `SliverCollectionView` fixed list mode | native `CollectionView` measurement strategy |
| Uno | `SliverFixedExtentVirtualizingLayout` | not currently exposed as a variable-height virtualizing layout |
| WinUI | `SliverFixedExtentVirtualizingLayout` | not currently exposed as a variable-height virtualizing layout |
