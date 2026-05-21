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
- `SliverItemsControl` is required when an outer `ScrollViewer` wraps an items panel that implements `ILogicalScrollable`; a plain `ItemsControl` does not expose the panel to `ScrollViewer`.
- Avalonia sliver panels use a 16px logical line scroll size so wheel input can drive smooth persistent-header collapse instead of jumping by an item extent.
- The gallery's sectioned sample uses a sample-local push-style sticky header so only one section header reserves the leading edge at a time; this avoids cumulative empty gaps between sections.
- `CacheExtent` is measured in main-axis units.
- Non-virtual panels still use sliver math but do not recycle controls.
- Virtualizing panels should be used for high-volume item sources.
