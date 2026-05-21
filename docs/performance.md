---
title: Performance
description: Performance model, tradeoffs, and guidance for high-volume SliverWidgets surfaces.
---

# Performance

SliverWidgets is optimized around a simple rule: layout work should be proportional to the paint/cache window, not the full item count.

## Hot Path Principles

- Fixed-extent lists compute the first realized index with arithmetic.
- Grids compute row ranges rather than scanning every item.
- Wrap layouts cache line metrics by cross-axis extent and realize only lines intersecting the paint/cache window.
- Cache windows are explicit and bounded.
- Variable-extent lists use observed measurements and estimates instead of forcing complete pre-measurement.
- Framework adapters keep native control creation inside framework-supported realization APIs where possible.

## Prefer Fixed Extents for Very Large Lists

`SliverFixedExtentListLayout` is the fastest list path. It calculates `ScrollExtent`, start index, and end index directly:

```csharp
var layout = new SliverFixedExtentListLayout(
    new SliverFixedExtentListOptions(
        ItemCount: 1_000_000,
        ItemExtent: 36,
        Spacing: 2));
```

Use fixed extents for log viewers, table-like rows, feeds with constrained card heights, and command palettes.

## Use Variable Extents When Content Requires It

`SliverVariableExtentListLayout` uses `SliverChildExtentCache`:

- observed rows use measured extents
- missing rows use the current average observed extent
- scroll extent improves as more rows are measured
- realization remains bounded to the cache range

This is a pragmatic model for dynamic content. It avoids forcing every row to measure before the user reaches it.

## Tune Cache Extent

Cache extent trades memory and CPU for scroll smoothness:

| Cache setting | Behavior |
|---|---|
| Small | Lower memory, more frequent realization during fast scrolling. |
| Medium | Good default for most desktop and mobile surfaces. |
| Large | Smoother fast scroll at the cost of more live containers. |

Adapters expose cache in framework-appropriate ways. Avalonia panels expose `CacheExtent`; Uno/WinUI use the platform realization rectangle; MAUI `CollectionView` uses native virtualization.

## Avoid Expensive Child Measurement

Sliver layout can reduce how many children are measured, but each realized child can still be expensive. Keep item templates predictable:

- avoid unbounded nested scroll viewers inside rows
- avoid expensive synchronous image decoding during measure
- cache derived text and metrics in the view model
- prefer fixed item dimensions where the product design allows it

## Framework-Specific Notes

Avalonia:

- `SliverVirtualizingStackPanel` is the fixed-row high-volume path.
- `SliverVirtualizingListPanel` handles variable row heights through observed measurements.
- `SliverVirtualizingWrapPanel` handles large variable-size chip/card flows with deterministic extents.
- Non-virtual `SliverStackPanel` and `SliverGridPanel` are useful for bounded child counts and custom composition.

MAUI:

- Use `SliverCollectionView` for large item counts.
- `SliverStackLayout` is a layout manager path and does not provide item recycling.

Uno and WinUI:

- Use `ItemsRepeater` with `SliverFixedExtentVirtualizingLayout` or `SliverGridVirtualizingLayout`.
- Keep item templates light because the platform owns element realization and recycling.

## Validation Targets

Performance-sensitive changes should be checked with:

```bash
dotnet test SliverWidgets.slnx
dotnet build SliverWidgets.Galleries.CI.slnx -c Release
```

For UI profiling, run the framework gallery and verify realized element counts stay close to the visible range plus cache.
