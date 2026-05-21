---
title: Avalonia API
description: Public API guide for SliverWidgets.Avalonia.
---

# Avalonia API

`SliverWidgets.Avalonia` provides panels and virtualizing panels that arrange Avalonia controls from core sliver slots.

## SliverItemsControl

`SliverItemsControl` derives from `ItemsControl` and exposes the logical scrolling surface of its generated sliver items panel to an outer Avalonia `ScrollViewer`. Use it when a `ScrollViewer` wraps an `ItemsControl` whose `ItemsPanel` is `SliverVirtualizingStackPanel`, `SliverVirtualizingGridPanel`, `SliverVirtualizingWrapPanel`, `SliverVirtualizingListPanel`, `SliverStackPanel`, `SliverGridPanel`, or another `ILogicalScrollable` sliver panel.

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

## SliverVirtualizingGridPanel

Grid `VirtualizingPanel` for large `ItemsControl` sources.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `SizingMode` | `SliverGridSizingMode` | `FixedCrossAxisCount` |
| `CrossAxisCount` | `int` | `2` |
| `MaxCrossAxisExtent` | `double` | `240` |
| `MainAxisSpacing` | `double` | `0` |
| `CrossAxisSpacing` | `double` | `0` |
| `ChildAspectRatio` | `double` | `1` |
| `MainAxisExtent` | `double` | `NaN` |
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

## SliverVirtualizingWrapPanel

Variable-width/height wrap `VirtualizingPanel` for large `ItemsControl` sources.

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `MinItemMainAxisExtent` | `double` | `56` |
| `MaxItemMainAxisExtent` | `double` | `132` |
| `MinItemCrossAxisExtent` | `double` | `120` |
| `MaxItemCrossAxisExtent` | `double` | `280` |
| `MainAxisSpacing` | `double` | `0` |
| `CrossAxisSpacing` | `double` | `0` |
| `ScrollOffset` | `double` | `0` |
| `CacheExtent` | `double` | `250` |

## Example

```xml
<ScrollViewer VerticalScrollBarVisibility="Visible">
  <slivers:SliverItemsControl
      xmlns:slivers="using:SliverWidgets.Avalonia"
      ItemsSource="{Binding Rows}">
    <slivers:SliverItemsControl.ItemsPanel>
      <ItemsPanelTemplate>
        <slivers:SliverVirtualizingStackPanel
            ItemExtent="44"
            Spacing="2"
            CacheExtent="500" />
      </ItemsPanelTemplate>
    </slivers:SliverItemsControl.ItemsPanel>
  </slivers:SliverItemsControl>
</ScrollViewer>
```

The `ScrollViewer` content should be `SliverItemsControl`, not a plain `ItemsControl`, when using a logical sliver items panel. Otherwise Avalonia physically scrolls the `ItemsControl` and can move beyond the realized item window.

Avalonia sliver panels report a 16px logical line scroll size. This keeps wheel-driven persistent-header collapse smooth while preserving fixed-extent arithmetic for realization and bring-into-view.

```xml
<slivers:SliverItemsControl
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemsSource="{Binding Rows}">
  <slivers:SliverItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingStackPanel
          ItemExtent="44"
          Spacing="2"
          CacheExtent="500" />
    </ItemsPanelTemplate>
  </slivers:SliverItemsControl.ItemsPanel>
</slivers:SliverItemsControl>
```
