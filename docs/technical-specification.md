---
title: Technical Specification
description: Detailed core algorithms and framework adapter behavior for SliverWidgets.
---

# Technical Specification

This document specifies how SliverWidgets works at the algorithm and adapter levels. It describes the framework-neutral core protocol in `SliverWidgets.Core` and the way Avalonia, MAUI, Uno, and WinUI translate native layout callbacks into that protocol.

The word "sliver" in this document means a scroll-aware layout fragment. A sliver receives viewport-local constraints and returns geometry plus child slots for the current paint and cache ranges. The core is intentionally free of Avalonia, Uno, MAUI, and WinUI types.

## Design Goals

The implementation has five primary goals:

1. Keep layout math deterministic and testable outside any UI framework.
2. Realize paint plus cache ranges, not entire item sources, for virtualized paths.
3. Use arithmetic index-to-offset mapping for fixed-extent lists and regular grids.
4. Use measured or deterministic extent caches for variable-size paths.
5. Keep framework adapters thin: they translate native viewport, measure, arrange, recycling, and scrolling APIs into the core protocol.

## Coordinate Model

The core uses logical axes instead of platform coordinates.

| Core term | Vertical mapping | Horizontal mapping |
|---|---|---|
| Main-axis offset | Y | X |
| Main-axis extent | Height | Width |
| Cross-axis offset | X | Y |
| Cross-axis extent | Width | Height |

Adapters are responsible for converting between this model and native rectangle types:

- Avalonia uses `Avalonia.Rect`.
- MAUI uses `Microsoft.Maui.Graphics.Rect`.
- Uno and WinUI use `Windows.Foundation.Rect`.

The core slot contract is `SliverLayoutSlot`:

```csharp
public readonly record struct SliverLayoutSlot(
    int Index,
    double MainAxisOffset,
    double CrossAxisOffset,
    double MainAxisExtent,
    double CrossAxisExtent,
    bool IsPinned = false,
    bool IsCacheOnly = false);
```

Slot offsets are local to the sliver. `SliverViewportLayoutEngine` translates them into viewport coordinates when multiple slivers are composed.

## Core Protocol

Every core layout implements:

```csharp
public interface ISliverLayout
{
    SliverLayoutResult Layout(in SliverConstraints constraints);
}
```

The input is `SliverConstraints`. The output is `SliverLayoutResult`, which contains `SliverGeometry` and a list of slots.

### SliverConstraints

`SliverConstraints` is the viewport state for one sliver.

| Property | Meaning |
|---|---|
| `Axis` | Logical scroll axis. |
| `ScrollOffset` | Local scroll offset into this sliver. |
| `PrecedingScrollExtent` | Total scroll extent of earlier slivers. |
| `Overlap` | Paint overlap carried from earlier slivers, especially pinned headers. |
| `RemainingPaintExtent` | Main-axis paint space available to this sliver. |
| `CrossAxisExtent` | Cross-axis size available to this sliver. |
| `ViewportMainAxisExtent` | Full viewport size in the scroll direction. |
| `CacheOrigin` | Cache start relative to `ScrollOffset`; it is zero or negative. |
| `RemainingCacheExtent` | Full cache-window size from `ScrollOffset + CacheOrigin`. |
| `GrowthDirection` | Forward or reverse growth. Reverse is represented but not broadly implemented by adapters yet. |
| `UserScrollDirection` | Idle, forward, or reverse. Advanced headers use it for floating and snap behavior. |

Validation rules:

- `ScrollOffset`, `PrecedingScrollExtent`, `RemainingPaintExtent`, `CrossAxisExtent`, `ViewportMainAxisExtent`, and `RemainingCacheExtent` must be finite and non-negative.
- `Overlap` and `CacheOrigin` must be finite.
- `CacheOrigin` must be less than or equal to zero.
- `RemainingCacheExtent` must be greater than or equal to `RemainingPaintExtent`.

### SliverGeometry

`SliverGeometry` is the layout's contract back to the viewport.

| Property | Meaning |
|---|---|
| `ScrollExtent` | Total scrollable main-axis size contributed by the sliver. |
| `PaintExtent` | Amount of sliver content that can paint in the current constraints. |
| `PaintOrigin` | Main-axis paint translation before the sliver's slots are applied. |
| `LayoutExtent` | Amount of viewport layout space consumed by this sliver in the current pass. |
| `MaxPaintExtent` | Natural maximum paint extent. |
| `MaxScrollObstructionExtent` | Pinned obstruction that later slivers must know about. |
| `HitTestExtent` | Main-axis area participating in hit testing. |
| `Visible` | Whether the sliver has visible paint. |
| `HasVisualOverflow` | Whether clipping may be required. |
| `ScrollOffsetCorrection` | A finite, non-zero correction request that restarts viewport layout. |
| `CacheExtent` | Amount of cache range consumed by this sliver. |
| `CrossAxisExtent` | Cross-axis size reported by the sliver. |

Validation rules:

- Extent fields must be finite and non-negative.
- `PaintExtent <= RemainingPaintExtent`.
- `LayoutExtent <= PaintExtent`.
- `PaintExtent <= MaxPaintExtent`.
- `PaintOrigin + PaintExtent <= RemainingPaintExtent`.
- `ScrollOffsetCorrection`, when present, must be finite and non-zero.

## Paint and Cache Range Math

The core uses two range concepts:

- Paint range: `[ScrollOffset, ScrollOffset + RemainingPaintExtent]`
- Cache range: `[ScrollOffset + CacheOrigin, ScrollOffset + CacheOrigin + RemainingCacheExtent]`, clamped at zero for realization decisions.

`SliverMath.CalculatePaintOffset(constraints, from, to)` computes the overlap between an arbitrary sliver-local interval and the paint range:

```text
paintStart = constraints.ScrollOffset
paintEnd = constraints.ScrollOffset + constraints.RemainingPaintExtent
paintOverlap = clamp(to, paintStart, paintEnd) - clamp(from, paintStart, paintEnd)
```

`SliverMath.CalculateCacheOffset(constraints, from, to)` uses the cache-origin-adjusted range:

```text
cacheStart = constraints.ScrollOffset + constraints.CacheOrigin
cacheEnd = cacheStart + constraints.RemainingCacheExtent
cacheOverlap = clamp(to, cacheStart, cacheEnd) - clamp(from, cacheStart, cacheEnd)
```

The helper `SliverMath.ResolveCacheRange` returns:

```text
cacheStart = max(0, scrollOffset + cacheOrigin)
cacheEnd = max(cacheStart, scrollOffset + cacheOrigin + remainingCacheExtent)
```

A slot is cache-only when its item range overlaps the cache range but does not overlap the paint range.

```text
isCacheOnly = !RangesOverlap(itemStart, itemEnd, visibleStart, visibleEnd)
```

Framework adapters may measure cache-only children so they are ready for fast scrolling, but visual adapters generally hide cache-only children during arrange.

## Viewport Composition Algorithm

`SliverViewportLayoutEngine` composes multiple `ISliverLayout` instances.

Inputs:

- ordered sliver list
- `SliverViewport(MainAxisExtent, CrossAxisExtent, Axis)`
- global scroll offset
- symmetric cache extent
- user scroll direction

The algorithm is:

```text
effectiveScrollOffset = requestedScrollOffset
repeat until stable, allowing up to 8 correction restarts:
    slots = []
    geometries = []
    precedingScrollExtent = 0
    scrollOffsetRemaining = effectiveScrollOffset
    layoutOffset = 0
    maxPaintOffset = 0
    cacheOrigin = -cacheExtent
    remainingCacheExtent = viewportExtent + 2 * cacheExtent

    for each sliver:
        localScrollOffset = max(0, scrollOffsetRemaining)
        correctedCacheOrigin = max(cacheOrigin, -localScrollOffset)
        cacheExtentCorrection = cacheOrigin - correctedCacheOrigin
        remainingPaintExtent = max(0, viewportExtent - layoutOffset)

        constraints = SliverConstraints(
            ScrollOffset = localScrollOffset,
            PrecedingScrollExtent = precedingScrollExtent,
            Overlap = maxPaintOffset - layoutOffset,
            RemainingPaintExtent = remainingPaintExtent,
            CrossAxisExtent = viewport.CrossAxisExtent,
            ViewportMainAxisExtent = viewport.MainAxisExtent,
            CacheOrigin = correctedCacheOrigin,
            RemainingCacheExtent = max(0, remainingCacheExtent + cacheExtentCorrection))

        result = sliver.Layout(constraints)
        validate result.Geometry

        if result.Geometry.ScrollOffsetCorrection exists:
            effectiveScrollOffset = max(0, effectiveScrollOffset + correction)
            restart whole viewport pass

        effectiveLayoutOffset = layoutOffset + result.Geometry.PaintOrigin
        translate each local slot by effectiveLayoutOffset

        maxPaintOffset = max(maxPaintOffset, effectiveLayoutOffset + PaintExtent)
        scrollOffsetRemaining -= ScrollExtent
        precedingScrollExtent += ScrollExtent
        layoutOffset += LayoutExtent

        if CacheExtent > epsilon:
            remainingCacheExtent =
                max(0, remainingCacheExtent - (CacheExtent - cacheExtentCorrection))
            cacheOrigin = min(correctedCacheOrigin + CacheExtent, 0)

    return total scroll extent, max scroll offset, geometries, slots
```

Important behaviors:

- Each sliver receives a local `ScrollOffset`. Earlier slivers reduce `scrollOffsetRemaining` by their `ScrollExtent`.
- `Overlap` is `maxPaintOffset - layoutOffset`; it lets pinned or painting-over slivers inform following slivers.
- `PaintOrigin` shifts slot translation without changing the sliver's scroll extent.
- `LayoutExtent` advances the next sliver. A pinned header can paint while consuming less layout extent than its visible size.
- Cache budget is consumed as slivers report `Geometry.CacheExtent`. The engine also accounts for cache-origin correction when a sliver is at the beginning of its scroll range and cannot consume cache before local offset zero.
- `ScrollOffsetCorrection` restarts layout. This is required for estimated or dead-reckoned slivers that need to re-anchor the viewport.

The engine permits up to 8 correction restarts. A ninth pass that still returns a correction throws instead of producing endless jitter.

## Core Layout Algorithms

### Fixed-Extent List

Type: `SliverFixedExtentListLayout`

Options:

- `ItemCount`
- `ItemExtent`
- `Spacing`

Scroll extent:

```text
if itemCount == 0:
    scrollExtent = 0
else:
    scrollExtent = itemCount * itemExtent + (itemCount - 1) * spacing
```

Index mapping:

```text
interval = itemExtent + spacing
startIndex = max(0, floor(cacheStart / interval) - 1)
itemStart = index * interval
itemEnd = itemStart + itemExtent
```

The loop starts near the first cached index and stops when `itemStart >= cacheEnd`. This is O(k), where k is the number of realized paint/cache items, after O(1) start-index math.

Geometry uses `BuildGeometry(scrollExtent, constraints)`:

```text
paintExtent = CalculatePaintOffset(constraints, 0, scrollExtent)
cacheExtent = CalculateCacheOffset(constraints, 0, scrollExtent)
layoutExtent = paintExtent
maxPaintExtent = scrollExtent
hitTestExtent = paintExtent
visible = paintExtent > epsilon
```

### Explicit Variable-Extent List

Type: `SliverListLayout`

Options:

- `ItemExtents`
- `Spacing`

The layout walks extents from index zero, accumulating a cursor until it passes the cache window. It is deterministic and exact, but start-index lookup is O(n) for deep offsets because the extents are just an arbitrary list. It is useful for small or moderate exact sources and tests. Large variable-height item controls should prefer `SliverVariableExtentListLayout` or the framework virtualizing panels that keep offset caches.

### Dead-Reckoned Variable-Extent List

Types:

- `SliverChildExtentCache`
- `SliverVariableExtentListLayout`

`SliverChildExtentCache` stores observed extents in a `SortedDictionary<int,double>`.

The current estimate for unknown children is:

```text
DeadReckonedExtent =
    ObservedCount == 0 ? DefaultExtent : ObservedExtentSum / ObservedCount
```

Leading offset for an index is computed by combining observed extents before the index with estimated extents for gaps:

```text
offset = 0
cursor = 0
for each observed extent before target index:
    offset += (observedIndex - cursor) * DeadReckonedExtent
    offset += observedExtent
    cursor = observedIndex + 1
offset += (targetIndex - cursor) * DeadReckonedExtent
offset += targetIndex * Spacing
```

`GetIndexAtScrollOffset` binary-searches indexes using `GetLeadingOffset`. The list layout starts just before that index, then realizes overlapping cache items. Total scroll extent is estimated from observed sum plus missing count times the dead-reckoned extent.

This mirrors Flutter's dead-reckoning concept: not every child is measured before scroll geometry is estimated.

### Regular Grid

Type: `SliverGridLayout`

Sizing modes:

- `FixedCrossAxisCount`
- `MaxCrossAxisExtent`

For fixed count:

```text
crossAxisCount = options.CrossAxisCount
```

For max extent:

```text
crossAxisCount = max(1, ceil(crossAxisExtent / (maxCrossAxisExtent + crossAxisSpacing)))
```

Tile size:

```text
tileCrossAxisExtent =
    max(0, (crossAxisExtent - (crossAxisCount - 1) * crossAxisSpacing) / crossAxisCount)

tileMainAxisExtent =
    options.MainAxisExtent ?? tileCrossAxisExtent / childAspectRatio
```

Row math:

```text
rowCount = ceil(itemCount / crossAxisCount)
rowInterval = tileMainAxisExtent + mainAxisSpacing
rowStart = row * rowInterval
index = row * crossAxisCount + column
crossOffset = column * (tileCrossAxisExtent + crossAxisSpacing)
```

The grid maps directly from row/column to index and realizes rows intersecting the cache range. This is O(realized rows * crossAxisCount).

### Variable-Size Stack

Type: `SliverStackLayout`

This is a linear sliver for non-uniform width and height items. It is called "stack" in this repository's samples because each item is arranged as a row-like block whose cross-axis size may vary.

Options:

- `ItemExtents`: main and cross extent per item
- `Spacing`
- `CrossAxisAlignment`: `Start`, `Center`, `End`, or `Stretch`

Metrics:

- The first layout builds an offsets array.
- Offset for item i is the sum of previous item main extents and spacing.
- The metrics object is cached on the layout instance.

Realization:

```text
startIndex = binary search first item whose end overlaps cacheStart
for index in startIndex..:
    itemStart = offsets[index]
    itemEnd = itemStart + item.MainAxisExtent
    break when itemStart >= cacheEnd
    emit slot if item overlaps cache
```

Cross-axis alignment:

```text
if Stretch:
    crossExtent = viewportCrossAxisExtent
else:
    crossExtent = min(itemCrossAxisExtent, viewportCrossAxisExtent)

Start:  crossOffset = 0
Center: crossOffset = max(0, (viewportCrossAxisExtent - crossExtent) / 2)
End:    crossOffset = max(0, viewportCrossAxisExtent - crossExtent)
```

`SliverDeterministicStackExtentList` provides allocation-conscious deterministic extents for very large samples, including 100,000-item sources.

### Variable-Size Wrap

Type: `SliverWrapLayout`

Options:

- `ItemExtents`: main and cross extent per item
- `MainAxisSpacing`
- `CrossAxisSpacing`

The wrap algorithm precomputes line metrics for the current cross-axis extent. Metrics are cached until the cross-axis extent changes.

Line packing:

```text
currentLineCrossOffset = 0
currentLineMainExtent = 0
for each item:
    itemCrossExtent = min(item.CrossAxisExtent, crossAxisExtent)
    projectedCrossEnd =
        currentLineCount == 0
            ? itemCrossExtent
            : currentLineCrossOffset + crossAxisSpacing + itemCrossExtent

    if currentLineCount > 0 and projectedCrossEnd > crossAxisExtent:
        finalize current line

    set item cross offset
    add itemCrossExtent to current line
    currentLineMainExtent = max(currentLineMainExtent, item.MainAxisExtent)
```

Each finalized line records:

- start item index
- item count
- main-axis offset
- line main-axis extent

Realization:

```text
startLine = binary search first line whose end overlaps cacheStart
for each cached line:
    for each item in line:
        emit item slot if item main interval overlaps cache
```

The line model is exact for deterministic extents and stable for large data. It is O(n) when cross-axis width changes because lines must be rebuilt, then O(realized lines + realized items) per layout pass.

### Persistent Header

Types:

- `SliverPersistentHeaderLayout`
- `SliverAdvancedPersistentHeaderLayout`

Basic header options:

- `MinExtent`
- `MaxExtent`
- `Pinned`

Shrink:

```text
shrinkOffset = clamp(scrollOffset, 0, maxExtent - minExtent)
currentExtent = clamp(maxExtent - shrinkOffset, minExtent, maxExtent)
```

Pinned geometry:

```text
paintOrigin = constraints.Overlap
effectiveRemainingPaintExtent = max(0, remainingPaintExtent - overlap)
paintExtent = min(currentExtent, effectiveRemainingPaintExtent)
layoutExtent = clamp(maxExtent - scrollOffset, 0, effectiveRemainingPaintExtent)
maxScrollObstructionExtent = minExtent
mainAxisOffset = 0
```

Non-pinned geometry:

```text
remainingNaturalPaint = CalculatePaintOffset(constraints, 0, maxExtent)
paintOrigin = min(overlap, 0)
paintExtent = remainingNaturalPaint
layoutExtent = remainingNaturalPaint
mainAxisOffset = min(0, remainingNaturalPaint - currentExtent)
maxScrollObstructionExtent = 0
```

The advanced header adds:

- `Floating`
- `Snap`
- `SliverPersistentHeaderState`
- snap animation service

Floating headers adjust `CurrentExtent` from scroll delta and `UserScrollDirection`. Snap headers choose min or max target when scrolling is idle and advance through `ISliverHeaderSnapAnimationService`.

### Box Adapter

Type: `SliverToBoxAdapterLayout`

This treats one child as a sliver. The child has a main-axis extent and optional cross-axis extent. A single slot is emitted when the child overlaps the cache window. Geometry is computed through the common fixed-extent geometry helper.

### Fill Remaining

Type: `SliverFillRemainingLayout`

Options:

- `ChildExtent`
- `HasScrollBody`

Algorithm:

```text
remainingViewport = max(0, viewportMainAxisExtent - precedingScrollExtent)
naturalExtent = ChildExtent ?? remainingViewport

if HasScrollBody:
    extent = viewportMainAxisExtent
else:
    extent = max(remainingViewport, naturalExtent)
```

A single slot is emitted when visible. This covers Flutter-style "fill the remaining viewport" behavior and scroll-body behavior separately.

### Padding

Type: `SliverPaddingLayout`

`SliverPaddingLayout` rewrites constraints for a child:

```text
beforePaint = CalculatePaintOffset(parent, 0, padding.Before)
beforeCache = CalculateCacheOffset(parent, 0, padding.Before)

child.ScrollOffset = max(0, parent.ScrollOffset - padding.Before)
child.PrecedingScrollExtent = parent.PrecedingScrollExtent + padding.Before
child.Overlap = adjusted overlap after leading padding
child.CrossAxisExtent = max(0, parent.CrossAxisExtent - crossBefore - crossAfter)
child.RemainingPaintExtent = max(0, parent.RemainingPaintExtent - beforePaint)
child.CacheOrigin = min(0, parent.CacheOrigin + padding.Before)
child.RemainingCacheExtent = max(0, parent.RemainingCacheExtent - beforeCache)
```

If the child returns `ScrollOffsetCorrection`, padding propagates it immediately. Otherwise, padding rebuilds parent geometry from child geometry plus the portions of before/after padding that intersect paint and cache:

```text
scrollExtent = before + child.ScrollExtent + after
paintExtent = min(
    beforePaint + max(child.PaintExtent, child.LayoutExtent + afterPaint),
    parent.RemainingPaintExtent)
layoutExtent = min(beforePaint + afterPaint + child.LayoutExtent, paintExtent)
cacheExtent = min(beforeCache + afterCache + child.CacheExtent, parent.RemainingCacheExtent)
maxPaintExtent = before + child.MaxPaintExtent + after
maxScrollObstructionExtent = child.MaxScrollObstructionExtent
```

Child slots are offset by leading paint padding and cross-axis before padding. This means leading padding that has already scrolled away does not keep shifting child slots by the full static padding amount.

### Visibility

Type: `SliverVisibilityLayout`

When visible, it delegates to the child. When hidden:

- `MaintainSize = true`: child geometry is preserved but slots are dropped.
- Replacement exists: replacement layout is used.
- Otherwise: empty result.

## DataGrid Algorithm

Types:

- `SliverDataGridLayout`
- `SliverDataGridLayoutOptions`
- `SliverDataGridColumnDefinition`
- `SliverDataGridQueryEngine`

The DataGrid core has two separate concerns:

1. Query projection: sort and filter row indexes.
2. Layout: compute row slots, column slots, and cell slots from projected row indexes and column definitions.

`SliverDataGridLayout.LayoutDataGrid` returns the full DataGrid-specific result: row slots, column slots, cell slots, total cross-axis extent, frozen cross-axis extent, and sliver geometry. The `ISliverLayout.Layout` implementation adapts the same layout to the generic sliver contract by returning row slots only. Framework row-virtualization adapters use row containers today; full per-cell host virtualization is a future layer.

### Row Projection

`SliverDataGridQueryEngine.ProjectRows<T>` accepts:

- source rows
- column bindings with value selectors
- sort descriptors
- filter descriptors

Filtering walks all rows and includes indexes that match every filter. Sorting then sorts projected indexes with each descriptor in order. Sort comparison uses a column-specific comparer when provided, then a default comparer that supports numbers, dates, strings, and same-type `IComparable` values.

The output is an `IReadOnlyList<int>` of source row indexes. The layout consumes it as `SourceRowIndexes`, so layout never evaluates row predicates or reflection.

### Column Width Resolution

Column definition fields:

- `Key`
- `Header`
- `WidthMode`
- `Width`
- `MinWidth`
- `MaxWidth`
- `StarWeight`
- `HeaderWidth`
- `CellWidth`
- `IsVisible`

Base width:

```text
Fixed:         Width
Auto:          max(Width, HeaderWidth, CellWidth)
SizeToHeader:  max(Width, HeaderWidth)
SizeToCells:   max(Width, CellWidth)
Star:          MinWidth
Fill:          MinWidth
LastColumnFill:max(Width, HeaderWidth, CellWidth)
```

Every base width is clamped to `[MinWidth, MaxWidth]`.

Star distribution:

```text
remaining = max(0, viewportCrossAxisExtent - occupiedWidth)
columnWidth += remaining * StarWeight / totalStarWeight
```

Fill distribution:

```text
remaining = max(0, viewportCrossAxisExtent - totalWidth)
each Fill column gets remaining / fillColumnCount
```

Last-column fill:

```text
lastFillWidth = max(currentWidth, viewportCrossAxisExtent - widthOfAllOtherColumnsAndSpacing)
```

Frozen columns are the first visible `FrozenColumnCount` columns. Their cross-axis offsets are not affected by horizontal scroll.

### Row Metrics

`SliverDataGridLayout` builds an offsets array with `rowCount + 1` entries:

```text
offsets[row] = cursor
cursor += rowExtent(sourceRowIndex)
cursor += rowSpacing if not last row
offsets[rowCount] = cursor
```

Metrics are cached by a key that includes:

- viewport cross-axis extent
- source row count
- visible row count
- source column count
- visible column keys
- sampled row extent signature
- sampled source row index signature
- full column signature
- row spacing
- column spacing
- frozen column count

The sampled signatures prevent common stale-cache cases when arrays are mutated in place, including the first, middle, or last row extent/source index changing. They are not a full checksum of every row. For fully deterministic invalidation, callers should replace mutated option lists or recreate the layout.

### Row Slots

Rows start after the header:

```text
rowsStart = HeaderExtent + RowSpacing
rowLocalCacheStart = max(0, cacheStart - rowsStart)
startRow = binary search first row whose end overlaps rowLocalCacheStart
```

For each row:

```text
rowStart = rowsStart + rowOffsets[row]
rowEnd = rowStart + rowExtent
slot.MainAxisOffset = rowStart - constraints.ScrollOffset
slot.IsCacheOnly = row does not overlap visible paint range
```

### Column Slots

Column realization has a visible range and cache range in the cross axis.

```text
viewportAfterFrozen = max(0, viewportCrossAxisExtent - frozenCrossAxisExtent)
visibleStart = frozenCrossAxisExtent + horizontalScrollOffset
visibleEnd = visibleStart + viewportAfterFrozen
cacheStart = max(frozenCrossAxisExtent, visibleStart + horizontalCacheOrigin)
cacheEnd = max(visibleEnd, cacheStart + max(horizontalCacheExtent, viewportAfterFrozen))
```

Frozen columns are always included. Non-frozen columns are included when their natural column interval overlaps `[cacheStart, cacheEnd]`.

Non-frozen columns are translated by horizontal scroll:

```text
displayOffset =
    frozenCrossAxisExtent
    + (columnNaturalOffset - frozenCrossAxisExtent)
    - horizontalScrollOffset
```

### Cell Slots

Cell slots are the cross-product of realized row slots and realized column slots, plus header slots for each realized column.

Header:

```text
headerOffset = PinHeader ? 0 : -constraints.ScrollOffset
```

Each cell carries:

- visible row index
- source row index
- column index
- column key
- main-axis offset and extent
- cross-axis offset and extent
- header flag
- frozen-column flag
- row cache-only flag
- column cache-only flag

Framework samples currently virtualize row containers. Full two-axis cell-container recycling is part of a future host-control layer.

`PinHeader` pins DataGrid header cell slots inside the DataGrid-specific cell result. It does not currently report a viewport-level `MaxScrollObstructionExtent`; adapters that compose DataGrid headers separately are responsible for their own native header placement.

## Avalonia Adapter Specification

Package: `SliverWidgets.Avalonia`

Avalonia provides the richest custom layout surface in this repository. The adapter uses `Panel`, `Decorator`, `VirtualizingPanel`, and `ILogicalScrollable`.

### Shared Avalonia Primitives

`SliverAvaloniaPrimitives` owns axis conversion and common policies:

- `Main(Size, axis)` and `Cross(Size, axis)`
- logical `Size` and `Rect` construction
- `ClampScrollOffset`
- `ArrangeSlot`
- `HideArrangedChild`
- fallback viewport resolution for unconstrained measure

`ArrangeSlot` hides cache-only or non-intersecting children:

```text
if slot.IsCacheOnly or slot not in viewport:
    opacity = 0
    hit test = false
    arrange at default rect
else:
    opacity = 1
    hit test = true
    arrange native rect
```

### SliverItemsControl

`SliverItemsControl` wraps an `ItemsControl` and forwards `ILogicalScrollable` to the panel inside its presenter. It synchronizes:

- `CanHorizontallyScroll`
- `CanVerticallyScroll`
- `Extent`
- `Viewport`
- `Offset`
- `ScrollInvalidated`

This lets a `ScrollViewer` see the panel's logical scroll extent while the item presenter still owns native item generation.

### Non-Virtual Avalonia Panels

`SliverStackPanel` and `SliverGridPanel` are `Panel` implementations for direct children.

`SliverStackPanel`:

- Measures every child at fixed `ItemExtent`.
- Computes total extent with `SliverFixedExtentListLayout.GetScrollExtent`.
- During arrange, calls `SliverFixedExtentListLayout` with current scroll and cache.
- Arranges returned slots and hides all other children.

`SliverGridPanel`:

- Creates a core `SliverGridLayout`.
- During measure, uses unbounded paint/cache to compute total scroll extent.
- Measures children in returned tile sizes.
- During arrange, calls the core grid with actual scroll/cache constraints.
- Arranges returned slots and hides the rest.

These panels are useful for finite direct-child samples. They are not the preferred path for 100,000 item sources.

### Virtualizing Avalonia Panels

Virtualizing panels derive from `VirtualizingPanel` and implement `ILogicalScrollable`.

Common behavior:

- Maintain `_containersByIndex` and `_indexesByContainer`.
- Implement `ContainerFromIndex`, `IndexFromContainer`, and `GetRealizedContainers`.
- Realize only indexes intersecting paint plus cache.
- Clear unrealized containers before measuring new realized indexes.
- Hide previously realized containers that are omitted from the arrange pass.
- Clear all realized containers on item collection changes.
- Track direct-control self-containers and skip generator prepare/clear calls for those items.

Direct-control lifecycle rule:

```text
if ItemContainerGenerator.NeedsContainer(item) == false:
    container = item as Control
    do not call PrepareItemContainer
    do not call ItemContainerPrepared
    do not call ClearItemContainer on unrealize
else:
    create, prepare, prepared, clear through generator
```

`SliverVirtualizingListPanel`:

- Supports variable measured item heights.
- Uses `EstimatedItemExtent` until an item is measured.
- Stores measured extents in `_extentCache`.
- Builds `_offsetCache` as prefix offsets.
- Finds the first overlapping item by binary search.
- Measures realized containers with infinite main-axis constraint and finite cross-axis constraint.
- Updates measured extents and invalidates the offset cache when they change.

`SliverVirtualizingGridPanel`:

- Uses core regular grid math with Avalonia item containers.
- Realizes and arranges only cached grid slots.

`SliverVirtualizingStackPanel`:

- Uses fixed-extent virtualized linear stack behavior.

`SliverVirtualizingStackLayoutPanel`:

- Uses deterministic non-uniform stack layout for large stack samples.

`SliverVirtualizingWrapPanel`:

- Uses deterministic non-uniform wrap layout and line packing.

`SliverVirtualizingDataGridRowsPanel`:

- Extends `SliverVirtualizingListPanel`.
- It is a named row-virtualization surface for DataGrid samples.
- Column sizing and horizontal cell-window math come from `SliverDataGridLayout`, while Avalonia row containers remain native controls.

### Avalonia Persistent Header

`SliverPersistentHeader` is a `Decorator`.

Measure:

- Computes current extent from `ScrollOffset`, `MinExtent`, and `MaxExtent`.
- Measures child at current main-axis extent.
- For non-pinned headers, reports only the visible scroll-away extent.

Arrange:

- Builds a core `SliverPersistentHeaderLayout`.
- Arranges the child from the returned header slot.
- Hides child when the header has no visible slot.

## Uno Adapter Specification

Package: `SliverWidgets.Uno`

Uno adapters are `VirtualizingLayout` implementations intended for `ItemsRepeater`.

Common steps:

1. Read `VirtualizingLayoutContext.RealizationRect`.
2. Infer a paint range from available size and realization size.
3. Convert to `SliverConstraints`.
4. Call the core layout.
5. Call `context.GetOrCreateElementAt(index)` only for returned slots.
6. Measure and arrange those elements in content coordinates.

Uno does not expose the same reliable `VisibleRect` semantics as WinUI in this implementation, so the adapter derives an approximate visible range from `RealizationRect`:

```text
remainingCacheExtent = realization main size
remainingPaintExtent = finite available main size, clamped to realization size
leadingCacheExtent = realizationStart <= 0 ? 0 : (remainingCacheExtent - remainingPaintExtent) / 2
scrollOffset = realizationStart + leadingCacheExtent
cacheOrigin = realizationStart - scrollOffset
```

Adapters:

- `SliverFixedExtentVirtualizingLayout`: core fixed-extent list.
- `SliverGridVirtualizingLayout`: core regular grid.
- `SliverStackVirtualizingLayout`: core deterministic non-uniform stack.
- `SliverWrapVirtualizingLayout`: core deterministic non-uniform wrap.
- `SliverDataGridRowsVirtualizingLayout`: package-level row virtualization for DataGrid rows.

Stack and wrap adapters cache the core layout instance by a key containing item count and layout properties. When the key changes, the core metrics are rebuilt.

DataGrid row adapter:

- Stores row offsets in a `double[]`.
- Uses `RowExtentSelector` to get item-specific row heights.
- Clamps row heights between `MinRowExtent` and `MaxRowExtent`.
- Recomputes metrics when item count, sampled item hashes, or layout properties change.
- Binary-searches the first cached row.
- Realizes only row containers intersecting the vertical cache window.
- Returns `TableWidth` as the realized content width so the native horizontal scroll host can scroll columns while the vertical scrollbar remains at the viewport edge.

## WinUI Adapter Specification

Package: `SliverWidgets.WinUI`

WinUI adapters are also `VirtualizingLayout` implementations for `ItemsRepeater`. They are structurally similar to Uno, but WinUI uses both:

- `VirtualizingLayoutContext.VisibleRect` for paint
- `VirtualizingLayoutContext.RealizationRect` for cache

Viewport conversion:

```text
scrollOffset = visible main start
cacheOrigin = realization main start - visible main start
remainingPaintExtent = visible main size
remainingCacheExtent = realization main size
crossAxisExtent = finite available cross size, otherwise visible cross size
```

This maps directly to the core paint/cache contract.

Adapters:

- `SliverFixedExtentVirtualizingLayout`
- `SliverGridVirtualizingLayout`
- `SliverStackVirtualizingLayout`
- `SliverWrapVirtualizingLayout`
- `SliverDataGridRowsVirtualizingLayout`

Measure and arrange use content coordinates:

```text
contentMainOffset = slot.MainAxisOffset + viewport.ScrollOffset
```

The adapter sets `context.LayoutOrigin = (0, 0)` and returns total content size from core `ScrollExtent`.

The DataGrid rows layout follows the same algorithm as Uno, except its viewport conversion comes from real `VisibleRect` plus `RealizationRect`.

## MAUI Adapter Specification

Package: `SliverWidgets.Maui`

MAUI has two adapter styles:

1. `CollectionView`-based virtualization for large item sources.
2. `Layout`/`ILayoutManager` for finite direct-child layout.

### SliverCollectionView

`SliverCollectionView` derives from `CollectionView`.

Supported modes:

- `FixedExtentList`
- `FixedExtentGrid`

It creates native MAUI layouts through `SliverItemsLayoutFactory`:

- `SliverFixedExtentLinearItemsLayout` for fixed lists.
- `GridItemsLayout` for grids.

Fixed-list mode wraps the user `ItemTemplate` in a `ContentView` and binds either `HeightRequest` or `WidthRequest` to `ItemExtent`, depending on axis. This gives MAUI native measurement an actual fixed extent and lets native `CollectionView` virtualization do the recycling.

MAUI cache extent is exposed for API parity but native prefetch distance is controlled by handlers/platforms rather than portable sliver math.

### SliverStackLayout

`SliverStackLayout` is a `Layout` with a custom `ILayoutManager`.

Measure:

- Resolves finite or infinite cross-axis constraints.
- Measures every child at fixed `ItemExtent` in the main axis.
- Returns full content extent from `SliverFixedExtentListLayout.GetScrollExtent`.

Arrange:

- Arranges every child at deterministic fixed intervals.
- This is intentionally non-virtual because MAUI `Layout` does not expose arbitrary child realization/recycling.

### SliverDataGridCollectionView

`SliverDataGridCollectionView` derives from `CollectionView`.

It sets:

- `ItemSizingStrategy = MeasureAllItems`
- vertical `LinearItemsLayout`

This supports variable-height native row containers in DataGrid samples. It does not implement two-axis cell recycling; horizontal column behavior is composed by the sample row templates and native scroll views.

## Framework Capability Matrix

| Feature | Core | Avalonia | Uno | WinUI | MAUI |
|---|---:|---:|---:|---:|---:|
| Fixed-extent list math | Yes | Panel and virtual paths | ItemsRepeater layout | ItemsRepeater layout | CollectionView fixed list |
| Variable measured list | Yes, via extent cache | `SliverVirtualizingListPanel` | Not general-purpose yet | Not general-purpose yet | Native CollectionView sizing only |
| Regular grid | Yes | Panel and virtual path | ItemsRepeater layout | ItemsRepeater layout | CollectionView grid |
| Persistent header | Yes | Decorator and samples | Sample-level composition | Sample-level composition | Sample-level composition |
| Variable-size stack | Yes | Virtual panel | ItemsRepeater layout | ItemsRepeater layout | Non-virtual Layout/sample projection |
| Variable-size wrap | Yes | Virtual panel | ItemsRepeater layout | ItemsRepeater layout | Native row projection in sample |
| DataGrid row/column math | Yes | Row virtualization panel and XAML columns | Row virtualizing layout | Row virtualizing layout | CollectionView row virtualization |
| Full two-axis cell recycling | Core slots only | Future host | Future host | Future host | Future host/platform-specific |

## Performance Characteristics

| Algorithm | Startup metric cost | Per-pass realization cost | Notes |
|---|---:|---:|---|
| Fixed extent list | O(1) | O(k) | Arithmetic index mapping. |
| Explicit variable list | O(1) | O(n until cache end) | Exact but not ideal for deep huge lists. |
| Child extent cache list | O(observed gaps) | O(log n + k) plus offset lookup cost | Dead-reckoned estimates avoid measuring all children. |
| Grid | O(1) | O(realized rows * columns) | Arithmetic row/column mapping. |
| Stack | O(n) first metrics build | O(log n + k) | Metrics cached. |
| Wrap | O(n) per cross-axis-width change | O(log lines + realized items) | Line metrics cached per cross-axis extent. |
| DataGrid | O(rows + columns) metrics build | O(log rows + realized rows * realized columns) | Row offsets and column metrics cached. |
| Avalonia virtual panels | O(realized containers) plus metric cache | O(realized containers) | Native generator handles container creation/clear. |
| Uno/WinUI virtual layouts | Depends on core layout | O(returned slots) | `ItemsRepeater` owns element recycling. |
| MAUI CollectionView | Native | Native | Core math is not in the native virtualization loop. |

`k` means the number of slots intersecting paint plus cache.

## Invalidation Rules

Core layouts are immutable at the option-object level but may cache derived metrics. Callers should recreate the layout or replace option lists when source data changes. Existing mutable-list safeguards include:

- DataGrid sampled signatures for row extents and source row maps.
- DataGrid full column signature.
- Stack and wrap adapter keys in Uno/WinUI.
- Avalonia item collection change handlers that clear realized containers and offset caches.

Framework property changes call native invalidation methods:

- Avalonia styled properties call `AffectsMeasure` and `AffectsArrange`, then panels raise `ScrollInvalidated`.
- Uno and WinUI dependency-property changes call `InvalidateMeasure`.
- MAUI bindable-property changes call `InvalidateMeasure` or rebuild `ItemsLayout`.

## Adapter Responsibilities

Adapters must:

1. Clamp invalid native inputs before constructing core constraints.
2. Preserve finite values required by core validation.
3. Map native viewport paint and cache ranges to `ScrollOffset`, `CacheOrigin`, `RemainingPaintExtent`, and `RemainingCacheExtent`.
4. Measure only returned slots when virtualization is available.
5. Arrange returned slots in native content coordinates.
6. Hide or unrealize omitted containers.
7. Keep native focus, data templates, accessibility, and styles intact.
8. Surface platform limitations in docs instead of claiming unsupported behavior.

Adapters must not:

- Put framework types in `SliverWidgets.Core`.
- Realize entire large item sources in virtualized paths.
- Call item-container generator lifecycle methods for direct-control self-containers.
- Treat cache-only slots as visible content.
- Use global scroll offsets where the core requires local sliver offsets.

## Test Coverage Expectations

Core tests should cover:

- zero items
- overscroll-safe constraints
- cache-only slots
- fixed arithmetic index mapping
- grid max-cross-axis count
- variable extent dead reckoning
- wrap line packing
- stack cross-axis alignment
- pinned and non-pinned headers
- padding correction propagation
- fill remaining behavior
- DataGrid row/column windows, frozen columns, sorting, filtering, and mutable-input invalidation

Adapter tests should cover:

- no blank bands during header transitions
- stale realized containers hidden after arrange
- item collection mutations clear generator maps
- direct-control items skip generator clear/prepare lifecycle
- horizontal and vertical scrollbar placement in DataGrid samples
- 100,000-row/item samples realizing bounded windows

## Current Limitations

- Reverse growth is modeled by the core enum but not broadly implemented in adapter behavior.
- Full two-axis DataGrid cell recycling is represented by core cell slots but not yet implemented as a reusable host control in each framework.
- MAUI arbitrary item-level virtualization is constrained by `CollectionView` and platform handlers.
- Uno uses inferred visible range from realization data in these adapters; renderer-specific validation is still required.
- WinUI package runtime behavior should be validated on Windows even when it builds on non-Windows hosts.
