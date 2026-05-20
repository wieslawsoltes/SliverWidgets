---
title: Avalonia Adapter
description: Avalonia panels, decorators, and virtualizing panels.
---

# Avalonia Adapter

`SliverWidgets.Avalonia` maps core sliver slots to Avalonia measurement and arrangement. It supports both simple panels and `VirtualizingPanel` implementations for large `ItemsControl` sources.

## Controls

| Control | Purpose |
|---|---|
| `SliverStackPanel` | Non-virtual fixed-extent stack panel. |
| `SliverGridPanel` | Non-virtual sliver grid panel. |
| `SliverPersistentHeader` | Single-child persistent header decorator. |
| `SliverVirtualizingStackPanel` | Fixed-extent virtualizing items panel. |
| `SliverVirtualizingListPanel` | Variable-height virtualizing items panel with observed extent cache. |

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

Use `SliverVirtualizingStackPanel` for large fixed-height item sources:

```xml
<ItemsControl ItemsSource="{Binding Rows}">
  <ItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingStackPanel
          ItemExtent="44"
          Spacing="2"
          CacheExtent="500" />
    </ItemsPanelTemplate>
  </ItemsControl.ItemsPanel>
</ItemsControl>
```

Use `SliverVirtualizingListPanel` when row heights vary:

```xml
<ItemsControl ItemsSource="{Binding Rows}">
  <ItemsControl.ItemsPanel>
    <ItemsPanelTemplate>
      <slivers:SliverVirtualizingListPanel
          EstimatedItemExtent="56"
          Spacing="4"
          CacheExtent="600" />
    </ItemsPanelTemplate>
  </ItemsControl.ItemsPanel>
</ItemsControl>
```

## Grid Panel

```xml
<slivers:SliverGridPanel
    xmlns:slivers="using:SliverWidgets.Avalonia"
    SizingMode="MaxCrossAxisExtent"
    MaxCrossAxisExtent="260"
    MainAxisSpacing="12"
    CrossAxisSpacing="12"
    ChildAspectRatio="1.4" />
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
- `CacheExtent` is measured in main-axis units.
- Non-virtual panels still use sliver math but do not recycle controls.
- Virtualizing panels should be used for high-volume item sources.
