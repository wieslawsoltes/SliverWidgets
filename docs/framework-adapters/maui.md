---
title: MAUI Adapter
description: MAUI layout and CollectionView integration.
---

# MAUI Adapter

`SliverWidgets.Maui` provides two integration paths:

- `SliverStackLayout` for fixed-extent layout composition
- `SliverCollectionView` for large-data native virtualization

## SliverStackLayout

`SliverStackLayout` is a MAUI `Layout` backed by an `ILayoutManager`.

```xml
<slivers:SliverStackLayout
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    Axis="Vertical"
    ItemExtent="48"
    Spacing="4">
  <Label Text="One" />
  <Label Text="Two" />
</slivers:SliverStackLayout>
```

Use it for bounded child counts or custom content where you want sliver-compatible layout math.

When MAUI measures the cross-axis with an unconstrained value, `SliverStackLayout` measures children with the fixed main-axis extent and reports the largest child desired cross-axis size. This keeps horizontal stacks from collapsing to height zero under containers that pass infinite height.

## SliverCollectionView

For large item counts, use `SliverCollectionView`. It keeps MAUI's native `CollectionView` realization and recycling behavior.

```xml
<slivers:SliverCollectionView
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemsSource="{Binding Items}"
    LayoutMode="FixedExtentList"
    ItemExtent="52"
    Spacing="4" />
```

Grid mode:

```xml
<slivers:SliverCollectionView
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemsSource="{Binding Tiles}"
    LayoutMode="FixedExtentGrid"
    CrossAxisCount="2"
    MainAxisSpacing="12"
    CrossAxisSpacing="12" />
```

## Items Layout Factory

`SliverItemsLayoutFactory` creates native MAUI `ItemsLayout` instances:

```csharp
var list = SliverItemsLayoutFactory.CreateFixedExtentList(
    SliverAxis.Vertical,
    spacing: 4);

var grid = SliverItemsLayoutFactory.CreateFixedExtentGrid(
    SliverAxis.Vertical,
    crossAxisCount: 2,
    mainAxisSpacing: 12,
    crossAxisSpacing: 12);
```

## Limitations

MAUI custom layouts do not provide a general item container virtualization protocol. `SliverStackLayout` is not the large-data path. Use `SliverCollectionView` when the data source can grow beyond a small bounded set.
