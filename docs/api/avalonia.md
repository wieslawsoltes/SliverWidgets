---
title: Avalonia API
description: Public API guide for SliverWidgets.Avalonia.
---

# Avalonia API

`SliverWidgets.Avalonia` provides panels and virtualizing panels that arrange Avalonia controls from core sliver slots.

## SliverStackPanel

Fixed-extent non-virtual panel.

| Property | Type | Default | Purpose |
|---|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` | Scroll/layout direction. |
| `ItemExtent` | `double` | `48` | Main-axis extent for every child. |
| `Spacing` | `double` | `0` | Main-axis spacing between children. |
| `ScrollOffset` | `double` | `0` | Current scroll offset used during arrange. |
| `CacheExtent` | `double` | `250` | Main-axis cache before/after visible range. |

## SliverGridPanel

Non-virtual grid panel using `SliverGridLayout`.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `SizingMode` | `SliverGridSizingMode` | `FixedCrossAxisCount` |
| `CrossAxisCount` | `int` | `2` |
| `MaxCrossAxisExtent` | `double` | `240` |
| `MainAxisSpacing` | `double` | `0` |
| `CrossAxisSpacing` | `double` | `0` |
| `ChildAspectRatio` | `double` | `1` |
| `ScrollOffset` | `double` | `0` |
| `CacheExtent` | `double` | `250` |

## SliverPersistentHeader

Single-child decorator backed by `SliverPersistentHeaderLayout`.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `MinExtent` | `double` | `48` |
| `MaxExtent` | `double` | `160` |
| `Pinned` | `bool` | `true` |
| `ScrollOffset` | `double` | `0` |

## SliverVirtualizingStackPanel

Fixed-extent `VirtualizingPanel` for `ItemsControl`.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `ItemExtent` | `double` | `48` |
| `Spacing` | `double` | `0` |
| `ScrollOffset` | `double` | `0` |
| `CacheExtent` | `double` | `250` |

## SliverVirtualizingListPanel

Variable-height `VirtualizingPanel` for `ItemsControl`.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `EstimatedItemExtent` | `double` | `48` |
| `Spacing` | `double` | `0` |
| `ScrollOffset` | `double` | `0` |
| `CacheExtent` | `double` | `250` |

## Example

```xml
<ItemsControl ItemsSource="{Binding Rows}">
  <ItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingStackPanel
          xmlns:slivers="using:SliverWidgets.Avalonia"
          ItemExtent="44"
          Spacing="2"
          CacheExtent="500" />
    </ItemsPanelTemplate>
  </ItemsControl.ItemsPanel>
</ItemsControl>
```
