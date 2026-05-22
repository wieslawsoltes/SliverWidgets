---
title: Viewport Engine
description: Compose multiple slivers into one scrollable viewport.
---

# Viewport Engine

`SliverViewportLayoutEngine` composes a list of `ISliverLayout` instances into one viewport.

It is useful for:

- custom framework adapters
- deterministic tests
- diagnostics and layout previews
- mixed sliver sample composition

## Usage

```csharp
var slivers = new ISliverLayout[]
{
    new SliverPersistentHeaderLayout(
        new SliverPersistentHeaderOptions(56, 180, Pinned: true)),
    new SliverGridLayout(
        SliverGridLayoutOptions.WithMaxCrossAxisExtent(500, 240, 12, 12)),
    new SliverFillRemainingLayout(
        new SliverFillRemainingOptions(HasScrollBody: false))
};

var result = new SliverViewportLayoutEngine().Layout(
    slivers,
    new SliverViewport(720, 1024),
    scrollOffset: 320,
    cacheExtent: 480);
```

## Result

`SliverViewportLayoutResult` contains:

| Property | Meaning |
|---|---|
| `ScrollExtent` | Total scroll extent of all slivers. |
| `MaxScrollOffset` | Maximum non-negative scroll offset for the viewport. |
| `Geometries` | One geometry result per sliver. |
| `Slots` | Sliver-indexed, viewport-offset child slots. |

## Validation

The engine validates each sliver's geometry against its constraints. Invalid negative or infinite values, excessive paint extents, or layout extents larger than paint extents fail early.

If a sliver returns a finite `ScrollOffsetCorrection`, the engine adjusts the effective scroll offset and restarts the viewport pass so estimated or corrected slivers can converge before slots are returned. Non-converging corrections fail fast instead of producing persistent jitter.

## Flutter-Style Sequencing

The viewport advances each sliver sequence by `SliverGeometry.LayoutExtent`, not by painted size or pinned obstruction. It computes the next sliver's `Overlap` from the difference between painted and laid-out extent, applies `PaintOrigin` to viewport slot offsets, and consumes `CacheExtent` so later slivers do not realize outside the remaining cache budget.

Pinned headers can therefore paint over following content while following slots still keep their natural layout positions. Adapters that want content clipped below pinned headers should clip at the pinned obstruction boundary instead of asking the core viewport to push normal slivers down.
