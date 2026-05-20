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

Use `SliverGridLayoutOptions.FixedCrossAxisCount` for fixed column/row counts and `SliverGridLayoutOptions.WithMaxCrossAxisExtent` for responsive tile sizes.

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
