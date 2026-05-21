---
title: MAUI API
description: Public API guide for SliverWidgets.Maui.
---

# MAUI API

`SliverWidgets.Maui` provides a fixed-extent layout manager and a `CollectionView` wrapper for native virtualization.

## SliverStackLayout

MAUI `Layout` backed by a sliver-aware `ILayoutManager`.

| Property | Type | Default | Purpose |
|---|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` | Layout direction. |
| `ItemExtent` | `double` | `48` | Main-axis extent for every child. |
| `Spacing` | `double` | `0` | Main-axis child spacing. |

## SliverCollectionLayoutMode

| Value | Purpose |
|---|---|
| `FixedExtentList` | Native linear `CollectionView` layout. |
| `FixedExtentGrid` | Native grid `CollectionView` layout. |

## SliverCollectionView

Native-backed `CollectionView` for high-volume data.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `LayoutMode` | `SliverCollectionLayoutMode` | `FixedExtentList` |
| `ItemExtent` | `double` | `48` |
| `Spacing` | `double` | `0` |
| `CrossAxisCount` | `int` | `1` |
| `MainAxisSpacing` | `double` | `0` |
| `CrossAxisSpacing` | `double` | `0` |
| `CacheExtent` | `double` | `0` |
| `UsesNativeVirtualization` | `bool` | `true` |
| `EffectiveMainAxisSpacing` | `double` | derived |

Methods:

| Method | Purpose |
|---|---|
| `CreateItemsLayout()` | Creates the native `ItemsLayout` for the current sliver settings. |

## SliverDataGridCollectionView

Native-backed `CollectionView` for variable-height DataGrid row containers.

| Property | Type | Default |
|---|---|---|
| `CacheExtent` | `double` | `0` |
| `UsesNativeVirtualization` | `bool` | `true` |

This adapter intentionally keeps MAUI's `CollectionView` realization path. It is the large-row-count DataGrid sample path because `ILayoutManager` does not expose arbitrary item container virtualization.

## SliverItemsLayoutFactory

Static factory for native MAUI layout objects.

| Method | Returns |
|---|---|
| `CreateFixedExtentList(SliverAxis axis, double spacing)` | `LinearItemsLayout` |
| `CreateFixedExtentGrid(SliverAxis axis, int crossAxisCount, double mainAxisSpacing, double crossAxisSpacing)` | `GridItemsLayout` |
| `ToItemsLayoutOrientation(SliverAxis axis)` | `ItemsLayoutOrientation` |

## Example

```csharp
var view = new SliverCollectionView
{
    ItemsSource = items,
    LayoutMode = SliverCollectionLayoutMode.FixedExtentGrid,
    CrossAxisCount = 2,
    MainAxisSpacing = 12,
    CrossAxisSpacing = 12
};
```
