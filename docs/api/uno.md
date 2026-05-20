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
