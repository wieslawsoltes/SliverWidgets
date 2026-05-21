---
title: Framework Adapters
description: How each supported UI framework integrates with SliverWidgets.
---

# Framework Adapters

Avalonia, Uno, MAUI, and WinUI adapters preserve native controls and use the core only for sliver math. The adapters are intentionally thin: they convert native viewport data to `SliverConstraints`, call the core, and arrange native controls from returned slots.

## Adapter Matrix

| Framework | Package | Integration surface | Best high-volume path |
|---|---|---|---|
| Avalonia | `SliverWidgets.Avalonia` | `Panel`, `Decorator`, `VirtualizingPanel` | `SliverVirtualizingStackPanel`, `SliverVirtualizingStackLayoutPanel`, `SliverVirtualizingDataGridRowsPanel`, `SliverVirtualizingListPanel`, `SliverVirtualizingWrapPanel` |
| MAUI | `SliverWidgets.Maui` | `Layout`, `ILayoutManager`, `CollectionView` | `SliverCollectionView`, `SliverDataGridCollectionView` |
| Uno | `SliverWidgets.Uno` | `ItemsRepeater` `VirtualizingLayout` | `SliverFixedExtentVirtualizingLayout`, `SliverStackVirtualizingLayout`, `SliverGridVirtualizingLayout`, `SliverWrapVirtualizingLayout`, `SliverDataGridRowsVirtualizingLayout` |
| WinUI | `SliverWidgets.WinUI` | `ItemsRepeater` `VirtualizingLayout` | `SliverFixedExtentVirtualizingLayout`, `SliverStackVirtualizingLayout`, `SliverGridVirtualizingLayout`, `SliverWrapVirtualizingLayout`, `SliverDataGridRowsVirtualizingLayout` |

## Framework Guides

- [Avalonia Adapter](avalonia.html)
- [MAUI Adapter](maui.html)
- [Uno Adapter](uno.html)
- [WinUI Adapter](winui.html)

## Design Rule

Adapters should not invent a new widget system. They should make existing controls participate in sliver-aware layout while retaining native framework behavior.

## Known Differences

MAUI does not expose a direct arbitrary item virtualization protocol through `ILayoutManager`; large-data virtualization uses native `CollectionView` recycling through `SliverCollectionView` and `SliverDataGridCollectionView`. The MAUI wrap gallery projects pre-packed wrap rows into native row virtualization because portable item-level variable wrap is not exposed by `CollectionView`. The DataGrid gallery uses native row containers while core exposes the two-axis row/column cell-slot model.

Uno and WinUI use `RealizationRect` and `VisibleRect` behavior from `ItemsRepeater`; their DataGrid row adapters virtualize row containers and leave full two-axis cell container recycling to a future host. Renderer-specific validation is required before claiming identical runtime behavior on every platform.
