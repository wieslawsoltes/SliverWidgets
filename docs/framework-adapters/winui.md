---
title: WinUI Adapter
description: Windows App SDK ItemsRepeater virtualizing layouts.
---

# WinUI Adapter

`SliverWidgets.WinUI` provides Windows App SDK `VirtualizingLayout` implementations for `ItemsRepeater`.

## Fixed Extent Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Items}">
  <ItemsRepeater.Layout>
    <slivers:SliverFixedExtentVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.WinUI"
        Axis="Vertical"
        ItemExtent="44"
        Spacing="2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

The layout uses `VirtualizingLayoutContext.VisibleRect` as the paint window and `RealizationRect` as the paint-plus-cache window. The adapter passes the difference as `SliverConstraints.CacheOrigin` so core layouts can distinguish visible slots from cache-only slots.

## Grid Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Tiles}">
  <ItemsRepeater.Layout>
    <slivers:SliverGridVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.WinUI"
        SizingMode="FixedCrossAxisCount"
        CrossAxisCount="4"
        MainAxisSpacing="12"
        CrossAxisSpacing="12"
        ChildAspectRatio="1.2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## Stack Layout

```xml
<ItemsRepeater ItemsSource="{x:Bind Cards}">
  <ItemsRepeater.Layout>
    <slivers:SliverStackVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.WinUI"
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
        xmlns:slivers="using:SliverWidgets.WinUI"
        MinItemMainAxisExtent="72"
        MaxItemMainAxisExtent="150"
        MinItemCrossAxisExtent="120"
        MaxItemCrossAxisExtent="280"
        MainAxisSpacing="10"
        CrossAxisSpacing="10" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## Windows Validation

WinUI code can be built on non-Windows hosts and should be run-tested on Windows:

```bash
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj -c Release
```

The package sets `EnableWindowsTargeting` for project compatibility and disables PRI generation on non-Windows hosts. Full Windows App SDK runtime validation belongs on Windows.

## Generated API Note

The portable docs build uses human-written API pages. WinUI is documented manually here and in the [API Guide](../api/winui.html) so non-Windows docs builds stay reliable.

## Limitations

The current WinUI gallery uses native `ItemsRepeater` sections to demonstrate fixed rows, variable-size stacks, DataGrid rows, grids, and wrap layouts. Variable-extent lists and mixed `CustomScrollView` composition are not yet one shared WinUI sliver viewport pipeline. DataGrid two-axis cell-slot math lives in core; the gallery uses native row virtualization until a framework-level two-axis host is added.
