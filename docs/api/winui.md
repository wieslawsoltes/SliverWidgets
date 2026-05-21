---
title: WinUI API
description: Public API guide for SliverWidgets.WinUI.
---

# WinUI API

`SliverWidgets.WinUI` provides Windows App SDK `VirtualizingLayout` implementations for `ItemsRepeater`.

## SliverFixedExtentVirtualizingLayout

| Property | Type | Default |
|---|---|---|
| `Axis` | `SliverAxis` | `Vertical` |
| `ItemExtent` | `double` | `48` |
| `Spacing` | `double` | `0` |

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
<ItemsRepeater ItemsSource="{x:Bind Items}">
  <ItemsRepeater.Layout>
    <slivers:SliverFixedExtentVirtualizingLayout
        xmlns:slivers="using:SliverWidgets.WinUI"
        ItemExtent="44"
        Spacing="2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## Build Note

Build WinUI code on the current host:

```bash
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj -c Release
```

Run and device-test WinUI on Windows. The manual API guide includes WinUI even though generated API extraction is limited to cross-platform packages for portable docs builds.
