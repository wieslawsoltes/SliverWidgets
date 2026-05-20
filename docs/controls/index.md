---
title: Controls
description: Guide to core sliver layouts and framework controls.
---

# Controls

SliverWidgets has two layers of public surface:

- core layouts in `SliverWidgets.Core`
- framework controls and layouts in adapter packages

The core layer is the source of behavior. Framework packages map that behavior to native controls.

## Core Layouts

| Layout | Use when |
|---|---|
| `SliverFixedExtentListLayout` | Every item has the same main-axis extent. |
| `SliverListLayout` | You already know every item extent. |
| `SliverVariableExtentListLayout` | Item extents are discovered as children are measured. |
| `SliverGridLayout` | Items are arranged in rows or columns with a fixed count or max cross-axis extent. |
| `SliverPersistentHeaderLayout` | A header should collapse and optionally pin. |
| `SliverAdvancedPersistentHeaderLayout` | A header needs pinned, floating, and snap behavior. |
| `SliverFillRemainingLayout` | A child should occupy the rest of the viewport. |
| `SliverPaddingLayout` | A child sliver needs main/cross-axis padding. |
| `SliverVisibilityLayout` | A child sliver should be shown, replaced, hidden, or hidden while preserving size. |
| `SliverToBoxAdapterLayout` | A single fixed-size box participates in sliver composition. |

## Framework Controls

| Framework | Controls |
|---|---|
| Avalonia | `SliverStackPanel`, `SliverGridPanel`, `SliverPersistentHeader`, `SliverVirtualizingStackPanel`, `SliverVirtualizingListPanel` |
| MAUI | `SliverStackLayout`, `SliverCollectionView`, `SliverItemsLayoutFactory` |
| Uno | `SliverFixedExtentVirtualizingLayout`, `SliverGridVirtualizingLayout` |
| WinUI | `SliverFixedExtentVirtualizingLayout`, `SliverGridVirtualizingLayout` |

## Choosing the Right Layout

Use this decision table for the first implementation:

| Scenario | Recommended layout |
|---|---|
| 100,000 uniform rows | fixed extent list |
| cards with measured text height | variable extent list |
| responsive image/content tiles | grid with max cross-axis extent |
| section header that remains visible | persistent header with `Pinned = true` |
| dashboard page that ends with an empty-state region | fill remaining |
| optional filter panel in a scroll sequence | visibility with replacement |
| hero or chart inside mixed scroll content | box adapter |

Start with fixed extents unless product requirements need dynamic heights. Fixed extents are simpler, faster, and easier to reason about.
