---
title: Uno API
description: Public API guide for SliverWidgets.Uno.
---

# Uno API

`SliverWidgets.Uno` provides WinUI-compatible `VirtualizingLayout` implementations for Uno `ItemsRepeater`.

## SliverFixedExtentVirtualizingLayout

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `ItemExtent` | `double` | `48` |
| `Spacing` | `double` | `0` |

Use this for large uniform lists.

## SliverGridVirtualizingLayout

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

When `MainAxisExtent` is `NaN` or infinity, the layout derives tile main-axis extent from cross-axis size and `ChildAspectRatio`.

## SliverStackVirtualizingLayout

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `MinItemMainAxisExtent` | `double` | `52` |
| `MaxItemMainAxisExtent` | `double` | `128` |
| `MinItemCrossAxisExtent` | `double` | `160` |
| `MaxItemCrossAxisExtent` | `double` | `640` |
| `Spacing` | `double` | `0` |
| `CrossAxisAlignment` | `SliverCrossAxisAlignment` | `Start` |

Use this for large variable-width/height cards that remain in one linear stack.

## SliverWrapVirtualizingLayout

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `MinItemMainAxisExtent` | `double` | `56` |
| `MaxItemMainAxisExtent` | `double` | `132` |
| `MinItemCrossAxisExtent` | `double` | `120` |
| `MaxItemCrossAxisExtent` | `double` | `280` |
| `MainAxisSpacing` | `double` | `0` |
| `CrossAxisSpacing` | `double` | `0` |

Use this for large variable-size chip or card flows where each item receives deterministic main/cross-axis extents.

## SliverDataGridRowsVirtualizingLayout

Native `ItemsRepeater` row virtualization for DataGrid row containers with variable heights.

| Property | Type | Default |
|---|---|---|
| `DefaultRowExtent` | `double` | `64` |
| `MinRowExtent` | `double` | `36` |
| `MaxRowExtent` | `double` | `96` |
| `RowSpacing` | `double` | `0` |
| `TableWidth` | `double` | `800` |
| `RowExtentSelector` | `Func<object?, int, double>?` | `null` |

Call `InvalidateItems()` after replacing the projected row source or changing row extent behavior.

## Example

```xml
<ItemsRepeater ItemsSource="{x:Bind Tiles}">
  <ItemsRepeater.Layout>
    <slivers:SliverGridVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        SizingMode="MaxCrossAxisExtent"
        MaxCrossAxisExtent="260"
        MainAxisSpacing="12"
        CrossAxisSpacing="12" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```
