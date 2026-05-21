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

## Properties

| Layout | Properties |
|---|---|
| `SliverFixedExtentVirtualizingLayout` | `Axis`, `ItemExtent`, `Spacing` |
| `SliverGridVirtualizingLayout` | `Axis`, `SizingMode`, `CrossAxisCount`, `MaxCrossAxisExtent`, `MainAxisSpacing`, `CrossAxisSpacing`, `ChildAspectRatio`, `MainAxisExtent` |

## Validation

Uno renderer behavior should be validated on the target platforms you support. The reusable `samples/UnoGallery` project and the `samples/UnoGalleryApp` host provide a starting point.

## Limitations

- Exact `VisibleRect` paint-window mapping is not enabled until Uno implements that API.
- Variable-extent lists and mixed `CustomScrollView` composition are represented by sample-level native layouts rather than a full Uno sliver viewport adapter.
