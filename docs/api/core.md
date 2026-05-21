---
title: Core API
description: Public API guide for SliverWidgets.Core.
---

# Core API

`SliverWidgets.Core` contains no UI framework dependency. It is suitable for tests, custom adapters, diagnostics, and shared layout computation.

## Primitives

| API | Kind | Purpose |
|---|---|---|
| `SliverAxis` | enum | Vertical or horizontal layout direction. |
| `SliverGrowthDirection` | enum | Forward or reverse growth direction. |
| `SliverUserScrollDirection` | enum | Idle, forward, or reverse user scroll state. |
| `SliverViewport` | record struct | Viewport main/cross-axis size and axis. |
| `SliverConstraints` | record struct | Input contract for one sliver layout pass. |
| `SliverGeometry` | record | Output geometry contract for one sliver. |
| `SliverLayoutSlot` | record struct | Realized child slot in main/cross-axis coordinates. |
| `SliverLayoutResult` | record | Geometry plus slots. |
| `ISliverLayout` | interface | Layout contract implemented by all core slivers. |
| `SliverMath` | static class | Shared validation and clamping helpers. |

## List APIs

| API | Purpose |
|---|---|
| `SliverFixedExtentListOptions` | Options for uniform row lists. |
| `SliverFixedExtentListLayout` | Arithmetic fixed-extent list layout. |
| `SliverListOptions` | Options for lists with known item extents. |
| `SliverListLayout` | Variable list layout when all extents are known. |
| `SliverChildExtentCache` | Observed extent cache with dead-reckoned missing item sizes. |
| `SliverVariableExtentListLayout` | Variable list layout backed by `SliverChildExtentCache`. |

## Grid APIs

| API | Purpose |
|---|---|
| `SliverGridSizingMode` | Fixed count or max cross-axis extent mode. |
| `SliverGridLayoutOptions` | Factory-based grid options. |
| `SliverGridLayout` | Grid layout implementation. |

Use `SliverGridLayoutOptions.FixedCrossAxisCount` for fixed column/row counts and `SliverGridLayoutOptions.WithMaxCrossAxisExtent` for responsive tile sizes. Max-extent mode uses a ceiling count so generated tiles do not exceed the configured maximum cross-axis extent.

## DataGrid APIs

| API | Purpose |
|---|---|
| `SliverDataGridColumnWidthMode` | Fixed, auto, size-to-header, size-to-cells, star, fill, and last-column-fill sizing. |
| `SliverDataGridColumnDefinition` | Column metadata and sizing inputs. |
| `SliverDataGridLayoutOptions` | Row extents, columns, horizontal cache state, frozen columns, spacing, and source-row projection. |
| `SliverDataGridLayout` | Two-axis DataGrid cell-slot layout with vertical sliver geometry. |
| `SliverDataGridLayoutResult` | DataGrid geometry plus row, column, and cell slots. |
| `SliverDeterministicDataGridRowExtentList` | Allocation-light deterministic variable row heights for large grids. |
| `SliverDataGridQueryEngine` | Sort/filter projection that returns source-row indexes before layout. |

`SliverDataGridLayout` keeps layout hot paths separate from data predicates. Use `SliverDataGridQueryEngine.ProjectRows` to produce `SourceRowIndexes`, then pass that map into `SliverDataGridLayoutOptions`.

## Stack APIs

| API | Purpose |
|---|---|
| `SliverCrossAxisAlignment` | Start, center, end, or stretch cross-axis alignment. |
| `SliverStackItemExtent` | Main-axis and cross-axis extent for one variable-size stack item. |
| `SliverStackLayoutOptions` | Stack item extents, spacing, and cross-axis alignment. |
| `SliverDeterministicStackExtentList` | Allocation-light deterministic extent source for large repeatable stack samples. |
| `SliverStackLayout` | Variable-size linear stack layout with cache-aware slot realization. |

`SliverStackLayout` is for feeds where both item height and item width vary but children remain in one linear scroll sequence.

## Wrap APIs

| API | Purpose |
|---|---|
| `SliverWrapItemExtent` | Main-axis and cross-axis extent for one variable-size wrap item. |
| `SliverWrapLayoutOptions` | Wrap item extents and main/cross-axis spacing. |
| `SliverDeterministicWrapExtentList` | Allocation-light deterministic extent source for large repeatable samples. |
| `SliverWrapLayout` | Variable-size wrap layout that packs items into cache-aware lines. |

`SliverWrapLayout` is for chip clouds, tag pickers, non-uniform cards, and other wrap/flow surfaces. It computes line breaks from the current cross-axis extent, reports total scroll extent, and returns only slots whose line intersects the paint plus cache window.

## Header APIs

| API | Purpose |
|---|---|
| `SliverPersistentHeaderOptions` | Basic min/max/pinned header options. |
| `SliverPersistentHeaderLayout` | Collapsing and pinned header layout. |
| `SliverAdvancedPersistentHeaderOptions` | Pinned/floating/snap options. |
| `SliverPersistentHeaderState` | Stateful floating/snap header state. |
| `SliverHeaderSnapStatus` | Idle, snapping to min, snapping to max. |
| `SliverHeaderSnapAnimationContext` | Context passed to snap animation services. |
| `ISliverHeaderSnapAnimationService` | Framework-neutral snap progression contract. |
| `SliverInstantHeaderSnapAnimationService` | Immediate snap service. |
| `SliverStepHeaderSnapAnimationService` | Step-based deterministic snap service. |
| `SliverAdvancedPersistentHeaderLayout` | Header layout with pinned, floating, and snap behavior. |

## Utility Slivers

| API | Purpose |
|---|---|
| `SliverToBoxAdapterOptions` | Size options for one box. |
| `SliverToBoxAdapterLayout` | Single-box sliver adapter. |
| `SliverFillRemainingOptions` | Fill-remaining options. |
| `SliverFillRemainingLayout` | Layout that fills remaining viewport space. |
| `SliverEdgeInsets` | Main/cross-axis padding. |
| `SliverPaddingLayout` | Padding wrapper around a child sliver. |
| `SliverVisibilityLayout` | Visibility, replacement, or maintain-size wrapper. |

## Viewport APIs

| API | Purpose |
|---|---|
| `SliverViewportSlot` | Slot with sliver index and viewport-space offset. |
| `SliverViewportLayoutResult` | Combined viewport layout result. |
| `SliverViewportLayoutEngine` | Mixed sliver composition engine. |

`SliverViewportLayoutEngine` restarts layout when a sliver reports a finite `SliverGeometry.ScrollOffsetCorrection`. This lets variable or estimated slivers request a corrected scroll position and prevents stale viewport offsets from being returned.

The engine also follows Flutter-style sliver sequencing: slots are offset by `PaintOrigin`, later slivers advance by `LayoutExtent`, overlap is derived from painted versus laid-out extent, and the cache window is consumed as each sliver reports `CacheExtent`.

## Example

```csharp
var slivers = new ISliverLayout[]
{
    new SliverPersistentHeaderLayout(
        new SliverPersistentHeaderOptions(56, 180, Pinned: true)),
    new SliverFixedExtentListLayout(
        new SliverFixedExtentListOptions(10_000, 44, 2))
};

var result = new SliverViewportLayoutEngine().Layout(
    slivers,
    new SliverViewport(720, 1024),
    scrollOffset: 500,
    cacheExtent: 360);
```
