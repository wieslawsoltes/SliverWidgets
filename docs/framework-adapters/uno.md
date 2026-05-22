---
title: Uno Adapter
description: Uno ItemsRepeater virtualizing layouts.
---

# Uno Adapter

`SliverWidgets.Uno` provides `VirtualizingLayout` implementations for `ItemsRepeater`.

## Fixed Extent Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Items}">
  <ItemsRepeater.Layout>
    <slivers:SliverFixedExtentVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        Axis="Vertical"
        ItemExtent="44"
        Spacing="2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

The layout uses `SliverFixedExtentListLayout` and `VirtualizingLayoutContext.RealizationRect` to request only the visible/cache range. Uno currently reports `VirtualizingLayoutContext.VisibleRect` as unsupported, so the adapter infers the visible paint range from the realization rectangle and available size instead of using the exact WinUI paint/cache split.

## Grid Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Tiles}">
  <ItemsRepeater.Layout>
    <slivers:SliverGridVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        SizingMode="MaxCrossAxisExtent"
        MaxCrossAxisExtent="260"
        MainAxisSpacing="12"
        CrossAxisSpacing="12"
        ChildAspectRatio="1.4" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## Stack Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Cards}">
  <ItemsRepeater.Layout>
    <slivers:SliverStackVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        MinItemMainAxisExtent="52"
        MaxItemMainAxisExtent="128"
        MinItemCrossAxisExtent="160"
        MaxItemCrossAxisExtent="640"
        CrossAxisAlignment="Center"
        Spacing="8" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## Wrap Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Chips}">
  <ItemsRepeater.Layout>
    <slivers:SliverWrapVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        MinItemMainAxisExtent="72"
        MaxItemMainAxisExtent="150"
        MinItemCrossAxisExtent="120"
        MaxItemCrossAxisExtent="280"
        MainAxisSpacing="10"
        CrossAxisSpacing="10" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## DataGrid Row Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Rows}">
  <ItemsRepeater.Layout>
    <slivers:SliverDataGridRowsVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.Uno"
        TableWidth="1670"
        DefaultRowExtent="64"
        MinRowExtent="36"
        MaxRowExtent="96" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

Set `RowExtentSelector` from code when rows have per-item heights, and call `InvalidateItems()` after replacing the projected row source.

## Properties

| Layout | Properties |
|---|---|
| `SliverFixedExtentVirtualizingLayout` | `Axis`, `ItemExtent`, `Spacing` |
| `SliverStackVirtualizingLayout` | `Axis`, `MinItemMainAxisExtent`, `MaxItemMainAxisExtent`, `MinItemCrossAxisExtent`, `MaxItemCrossAxisExtent`, `Spacing`, `CrossAxisAlignment` |
| `SliverGridVirtualizingLayout` | `Axis`, `SizingMode`, `CrossAxisCount`, `MaxCrossAxisExtent`, `MainAxisSpacing`, `CrossAxisSpacing`, `ChildAspectRatio`, `MainAxisExtent` |
| `SliverWrapVirtualizingLayout` | `Axis`, `MinItemMainAxisExtent`, `MaxItemMainAxisExtent`, `MinItemCrossAxisExtent`, `MaxItemCrossAxisExtent`, `MainAxisSpacing`, `CrossAxisSpacing` |
| `SliverDataGridRowsVirtualizingLayout` | `DefaultRowExtent`, `MinRowExtent`, `MaxRowExtent`, `RowSpacing`, `TableWidth`, `RowExtentSelector` |

## Validation

Uno renderer behavior should be validated on the target platforms you support. The reusable `samples/UnoGallery` project and the `samples/UnoGalleryApp` host provide a starting point.

## Limitations

- Exact `VisibleRect` paint-window mapping is not enabled until Uno implements that API.
- Variable-extent lists and mixed `CustomScrollView` composition are represented by sample-level native layouts rather than a full Uno sliver viewport adapter.
- The DataGrid gallery page uses `SliverDataGridRowsVirtualizingLayout` for native row-container virtualization with core sort/filter projection and shared column metadata. Core exposes two-axis DataGrid cell slots, but the Uno adapter does not yet provide a dedicated two-axis cell host.
