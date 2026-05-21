---
title: MAUI Adapter
description: MAUI layout and CollectionView integration.
---

# MAUI Adapter

`SliverWidgets.Maui` provides two integration paths:

- `SliverStackLayout` for fixed-extent layout composition
- `SliverCollectionView` for large-data native virtualization
- `SliverDataGridCollectionView` for variable-height DataGrid row containers backed by native virtualization

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

## SliverDataGridCollectionView

Use `SliverDataGridCollectionView` for DataGrid row containers whose heights vary by item. It uses native `CollectionView` realization with `ItemSizingStrategy.MeasureAllItems`.

```xml
<slivers:SliverDataGridCollectionView
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemsSource="{Binding Rows}"
    CacheExtent="240" />
```

## Limitations

MAUI custom layouts do not provide a general item container virtualization protocol. `SliverStackLayout` is not the large-data path. Use `SliverCollectionView` when the data source can grow beyond a small bounded set.

`SliverCollectionView.CacheExtent` is a cross-platform hint for API symmetry with other adapters. MAUI does not expose a portable per-view cache extent equivalent to Flutter's viewport cache, so platform prefetch behavior remains owned by the native `CollectionView` implementation.

The gallery demonstrates the shared 100,000-card variable stack scenario with native `CollectionView` rows. Each row is a normal MAUI control with deterministic width and height from shared gallery data, while native `CollectionView` owns realization.

The gallery demonstrates the shared 100,000-row DataGrid scenario with `SliverDataGridCollectionView`, horizontal scrolling, dynamic text cells, variable row heights, real native grid columns from shared metadata, and core sort/filter projection. Portable two-axis cell recycling still requires a handler-backed grid surface and is not claimed by the MAUI adapter.

MAUI does not expose a portable variable-size wrap `CollectionView` layout. The gallery keeps the shared 100,000-chip wrap scenario honest by pre-packing deterministic wrap lines and virtualizing those lines with native `CollectionView`; each realized row contains normal MAUI chip controls with variable width and height.

Mixed `CustomScrollView` composition and variable-extent dead-reckoning are represented conceptually in the MAUI gallery rather than by a single MAUI sliver viewport adapter.
