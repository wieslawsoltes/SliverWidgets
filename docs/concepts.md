---
title: Concepts
description: Core sliver concepts used throughout SliverWidgets.
---

# Concepts

A sliver is a scrollable layout fragment. Instead of asking every child to measure itself for the full scrollable content, a sliver receives viewport-aware constraints and returns enough geometry to describe the scroll extent plus slots for the paint/cache range.

This model is inspired by Flutter slivers, but SliverWidgets is not a Flutter runtime. It translates the useful ideas into .NET layout systems while keeping native controls.

## Sliver

A sliver owns one piece of a scrollable composition. Examples include:

- a fixed-height list
- a variable-height list
- a variable-width/height stack
- a grid
- a variable-size wrap surface
- a pinned header
- a single box
- a padding wrapper
- a fill-remaining region

All core slivers implement `ISliverLayout`.

## Constraints

`SliverConstraints` describes the current viewport state for one sliver. Important values are:

| Property | Meaning |
|---|---|
| `ScrollOffset` | Local scroll offset into the current sliver. |
| `PrecedingScrollExtent` | Total scroll extent of slivers before this one. |
| `RemainingPaintExtent` | Main-axis space available for currently visible content. |
| `CrossAxisExtent` | Width for vertical layouts, height for horizontal layouts. |
| `ViewportMainAxisExtent` | Full viewport size in the scroll direction. |
| `CacheOrigin` | Offset before the paint range where look-ahead realization begins. |
| `RemainingCacheExtent` | Main-axis size of the full cache window. |
| `UserScrollDirection` | Direction used by floating and snap header logic. |

## Geometry

`SliverGeometry` is the sliver's contract with the viewport:

- `ScrollExtent` tells the viewport how much scrollable content this sliver contributes.
- `PaintExtent` tells the viewport how much content can be painted now.
- `LayoutExtent` tells the viewport how much layout space the sliver occupies in the current pass.
- `MaxPaintExtent` records the largest natural paint extent.
- `MaxScrollObstructionExtent` records pinned header obstruction.
- `CacheExtent` describes cache-window contribution.
- `HasVisualOverflow` tells adapters that clipping may be required.

## Slots

`SliverLayoutSlot` identifies a realized child:

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

The slot is intentionally independent of any UI framework. Avalonia maps it to `Rect`, MAUI maps it to `Microsoft.Maui.Graphics.Rect`, and Uno/WinUI map it to `Windows.Foundation.Rect`.

## Viewport Composition

`SliverViewportLayoutEngine` lets multiple slivers participate in one scrollable surface. It gives each sliver a local offset, validates returned geometry, and translates child slots into viewport coordinates.

Use it when building custom adapters or tests:

```csharp
var result = new SliverViewportLayoutEngine().Layout(
    slivers,
    new SliverViewport(720, 1024),
    scrollOffset: 320,
    cacheExtent: 480);
```

## Flutter Alignment

SliverWidgets intentionally mirrors Flutter names where the concept is portable:

| Flutter concept | SliverWidgets concept |
|---|---|
| `SliverConstraints` | `SliverConstraints` |
| `SliverGeometry` | `SliverGeometry` |
| `RenderSliver` | `ISliverLayout` |
| `SliverFixedExtentList` | `SliverFixedExtentListLayout` |
| `SliverList` | `SliverListLayout` and `SliverVariableExtentListLayout` |
| Variable-size linear stack | `SliverStackLayout` |
| `SliverGrid` | `SliverGridLayout` |
| Custom wrap/flow sliver | `SliverWrapLayout` |
| `SliverPersistentHeader` | `SliverPersistentHeaderLayout` and `SliverAdvancedPersistentHeaderLayout` |
| `SliverToBoxAdapter` | `SliverToBoxAdapterLayout` |
| `SliverPadding` | `SliverPaddingLayout` |
| `SliverVisibility` | `SliverVisibilityLayout` |

The implementation remains native to .NET. Framework adapters do not emulate Flutter widgets; they use Flutter's layout ideas to improve native layout performance and composition.
