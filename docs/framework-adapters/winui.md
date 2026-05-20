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

The layout uses `VirtualizingLayoutContext.VisibleRect` and `RealizationRect` to translate platform realization data into `SliverConstraints`.

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

## Windows Validation

WinUI code can be built on non-Windows hosts and should be run-tested on Windows:

```bash
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj -c Release
```

The package sets `EnableWindowsTargeting` for project compatibility and disables PRI generation on non-Windows hosts. Full Windows App SDK runtime validation belongs on Windows.

## Generated API Note

The portable docs build uses human-written API pages. WinUI is documented manually here and in the [API Guide](../api/winui.html) so non-Windows docs builds stay reliable.
