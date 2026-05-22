---
title: Virtualization Guide
description: How SliverWidgets realizes, caches, and releases UI children.
---

# Virtualization Guide

Virtualization is the practice of creating UI elements only for the part of the data set that matters to the current viewport. SliverWidgets models the realization range in the core and lets each framework adapter map it to native controls.

## Paint Range and Cache Range

The paint range is the currently visible portion of a sliver. The cache range extends before and after the paint range. Layouts return slots for both ranges:

- visible slots have `IsCacheOnly == false`
- cache-only slots have `IsCacheOnly == true`

Adapters may measure cache-only children to prepare them for near-future scrolling.

## Fixed-Extent Realization

Fixed extent lists compute realization with:

```text
interval = itemExtent + spacing
startIndex = floor(cacheStart / interval) - 1
```

This avoids scanning from item zero. It is the preferred model when each row or tile row has a known main-axis size.

## Grid Realization

Grid layouts realize by row. The core first resolves a cross-axis count:

- `FixedCrossAxisCount` uses the configured count.
- `MaxCrossAxisExtent` derives the largest count that keeps tiles within the requested max extent.

The core then realizes only rows intersecting the cache window.

## Variable-Extent Realization

Variable extent layouts need estimates. `SliverChildExtentCache` stores observed child sizes and uses a dead-reckoned average for missing items.

```csharp
var cache = new SliverChildExtentCache(
    itemCount: 50_000,
    defaultExtent: 52,
    spacing: 4);

cache.Observe(index: 120, extent: 68);

var layout = new SliverVariableExtentListLayout(cache);
```

When a framework adapter measures a realized child, it should call `Observe` or an equivalent local cache update so future scroll math gets more accurate.

## Container Lifecycle

Framework adapters should treat slots as the source of truth:

1. Compute the current slot set.
2. Realize native containers for new slots.
3. Measure and arrange realized containers.
4. Clear or recycle containers no longer represented by slots.

Avalonia `VirtualizingPanel` adapters implement this directly. Uno and WinUI delegate most lifecycle behavior to `ItemsRepeater`. MAUI delegates large-data recycling to `CollectionView`.

## Scroll Into View

Fixed-extent `ScrollIntoView` can jump directly to:

```text
index * (itemExtent + spacing)
```

Variable-extent `ScrollIntoView` should use cached or estimated leading offsets. If exact positioning matters for a not-yet-measured item, scroll to the estimate first and refine after measurement.

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Measuring all children to compute scroll extent | Use fixed extents or an extent cache. |
| Ignoring cache range | Users see empty content during fast scroll. |
| Letting negative or NaN values enter geometry | Validate options and clamp framework input. |
| Using custom MAUI `Layout` for huge item counts | Use `SliverCollectionView` instead. |
| Claiming WinUI runtime parity from macOS-only builds | Run the Windows lane for Windows App SDK validation. |
