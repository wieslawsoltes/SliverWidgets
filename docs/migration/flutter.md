---
title: Flutter Sliver Mapping
description: Conceptual migration guide from Flutter slivers to SliverWidgets.
---

# Flutter Sliver Mapping

SliverWidgets borrows the sliver layout model from Flutter, but it maps the concepts to native .NET controls.

## Concept Map

| Flutter | SliverWidgets |
|---|---|
| `CustomScrollView` | `SliverViewportLayoutEngine` for custom adapters; framework scroll surfaces for controls |
| `RenderSliver` | `ISliverLayout` |
| `SliverConstraints` | `SliverConstraints` |
| `SliverGeometry` | `SliverGeometry` |
| `SliverFixedExtentList` | `SliverFixedExtentListLayout` |
| `SliverList` | `SliverListLayout`, `SliverVariableExtentListLayout` |
| `SliverGrid` | `SliverGridLayout` |
| `SliverPersistentHeader` | `SliverPersistentHeaderLayout`, `SliverAdvancedPersistentHeaderLayout` |
| `SliverToBoxAdapter` | `SliverToBoxAdapterLayout` |
| `SliverFillRemaining` | `SliverFillRemainingLayout` |
| `SliverPadding` | `SliverPaddingLayout` |
| `SliverVisibility` | `SliverVisibilityLayout` |

## Widget Tree vs Native Controls

Flutter slivers are widgets and render objects. SliverWidgets core types are layout algorithms. The visual tree still belongs to Avalonia, MAUI, Uno, or WinUI.

This difference matters:

- styling stays native
- input stays native
- accessibility stays native
- item recycling follows framework rules
- not every Flutter sliver has a direct framework adapter yet

## Practical Porting Guidance

When porting a Flutter sliver composition:

1. Identify each sliver section.
2. Replace fixed lists with `SliverFixedExtentListLayout` or framework fixed list adapters.
3. Replace grids with `SliverGridLayout` or framework grid adapters.
4. Replace app bars and section headers with persistent header layouts.
5. Use utility slivers for boxes, padding, fill, and visibility.
6. Keep framework-specific controls and templates native.

## Differences to Expect

`SliverWidgets.Core` can model mixed sliver layout directly, but each framework has different public hooks for scroll owner integration. Avalonia exposes custom panels and virtualizing panels. Uno and WinUI expose `ItemsRepeater` layouts. MAUI's high-volume path is native `CollectionView`.

Core viewport composition follows Flutter's render-sliver sequencing for `PaintOrigin`, `LayoutExtent`, `Overlap`, cache-origin correction, cache consumption, and scroll-offset correction retries. Framework adapters may still differ where the native platform does not expose the same hooks; those limitations are documented in the adapter pages and in `plan/FLUTTER_SLIVER_COMPARISON.md`.
