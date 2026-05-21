---
title: Avalonia Adapter
description: Avalonia panels, decorators, and virtualizing panels.
---

# Avalonia Adapter

`SliverWidgets.Avalonia` maps core sliver slots to Avalonia measurement and arrangement. It supports both simple panels and `VirtualizingPanel` implementations for large `ItemsControl` sources.

## Controls

| Control | Purpose |
|---|---|
| `SliverItemsControl` | `ItemsControl` host that forwards outer `ScrollViewer` logical offsets into sliver items panels. |
| `SliverStackPanel` | Non-virtual fixed-extent stack panel. |
| `SliverGridPanel` | Non-virtual sliver grid panel. |
| `SliverPersistentHeader` | Single-child persistent header decorator. |
| `SliverVirtualizingStackPanel` | Fixed-extent virtualizing items panel. |
| `SliverVirtualizingGridPanel` | Fixed-count or max-cross-axis-extent virtualizing grid items panel. |
| `SliverVirtualizingListPanel` | Variable-height virtualizing items panel with observed extent cache. |
| `SliverVirtualizingWrapPanel` | Variable-width/height wrap virtualizing items panel. |

## Fixed Stack Panel

```xml
<slivers:SliverStackPanel
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemExtent="44"
    Spacing="2"
    CacheExtent="400">
  <TextBlock Text="Row 1" />
  <TextBlock Text="Row 2" />
</slivers:SliverStackPanel>
```

Use this when child count is bounded or when composing custom content manually.

## Virtualizing ItemsControl

Use `SliverItemsControl` with `SliverVirtualizingStackPanel` for large fixed-height item sources:

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

Use `SliverVirtualizingListPanel` when row heights vary:

```xml
<slivers:SliverItemsControl
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemsSource="{Binding Rows}">
  <slivers:SliverItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingListPanel
          EstimatedItemExtent="56"
          Spacing="4"
          CacheExtent="600" />
    </ItemsPanelTemplate>
  </slivers:SliverItemsControl.ItemsPanel>
</slivers:SliverItemsControl>
```

## Grid Panel

Use `SliverVirtualizingGridPanel` for large item sources:

```xml
<slivers:SliverItemsControl
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemsSource="{Binding Tiles}">
  <slivers:SliverItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingGridPanel
          SizingMode="MaxCrossAxisExtent"
          MaxCrossAxisExtent="260"
          MainAxisSpacing="12"
          CrossAxisSpacing="12"
          ChildAspectRatio="1.4"
          CacheExtent="600" />
    </ItemsPanelTemplate>
  </slivers:SliverItemsControl.ItemsPanel>
</slivers:SliverItemsControl>
```

`SliverGridPanel` remains available for bounded direct-child grids:

```xml
<slivers:SliverGridPanel
    xmlns:slivers="using:SliverWidgets.Avalonia"
    SizingMode="MaxCrossAxisExtent"
    MaxCrossAxisExtent="260"
    MainAxisSpacing="12"
    CrossAxisSpacing="12"
    ChildAspectRatio="1.4" />
```

## Wrap Panel

Use `SliverVirtualizingWrapPanel` for large non-uniform chip or card sources:

```xml
<slivers:SliverItemsControl
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemsSource="{Binding Chips}">
  <slivers:SliverItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingWrapPanel
          MinItemMainAxisExtent="72"
          MaxItemMainAxisExtent="150"
          MinItemCrossAxisExtent="120"
          MaxItemCrossAxisExtent="280"
          MainAxisSpacing="10"
          CrossAxisSpacing="10"
          CacheExtent="600" />
    </ItemsPanelTemplate>
  </slivers:SliverItemsControl.ItemsPanel>
</slivers:SliverItemsControl>
```

## Persistent Header

```xml
<slivers:SliverPersistentHeader
    xmlns:slivers="using:SliverWidgets.Avalonia"
    MinExtent="56"
    MaxExtent="180"
    Pinned="True" />
```

## Notes

- `ScrollOffset` is an adapter property for surfaces that coordinate scrolling externally.
- `SliverItemsControl` is required when an outer `ScrollViewer` wraps an items panel that implements `ILogicalScrollable`; a plain `ItemsControl` does not expose the panel to `ScrollViewer`.
- Avalonia sliver panels use a 16px logical line scroll size so wheel input can drive smooth persistent-header collapse instead of jumping by an item extent.
- The gallery's sectioned sample exposes stacked and push sticky-header modes through `SliverScenarioStackPanel.SectionHeaderMode`. Stacked mode is the default; push mode keeps one active section header at the leading edge.
- `CacheExtent` is measured in main-axis units.
- Non-virtual panels still use sliver math but do not recycle controls.
- Virtualizing panels should be used for high-volume item sources.
- `SliverPersistentHeader` is a single-child decorator coordinated by an external scroll offset. Use the mixed or sectioned sample panels when you need one `CustomScrollView`-style viewport that composes headers with following slivers.
