---
title: Slivers for .NET UI Frameworks
description: Why Flutter-style sliver concepts are useful in Avalonia, MAUI, Uno, and WinUI.
---

# Slivers for .NET UI Frameworks

Modern application screens often need more than a simple scrolling list. A single page may contain a collapsing header, metrics, filters, section summaries, a responsive grid, and long lists. Native .NET UI frameworks can build those screens, but developers often reach for nested scroll viewers or custom panels that accidentally measure too much content.

Slivers solve the composition problem by making every section viewport-aware.

## The Problem with Traditional Composition

A common layout starts with a scroll viewer, then stacks panels inside it:

```text
ScrollViewer
  StackPanel
    Header
    Filters
    Grid
    List
    Footer
```

This is simple, but it has poor scaling characteristics when the inner sections need thousands of children. The outer scroll viewer wants a total extent, while the inner panels often measure more content than the current viewport needs.

## The Sliver Model

A sliver receives a local scroll offset, available paint extent, and cache extent. It returns:

- total scroll contribution
- how much can paint
- which children should be realized
- where those children should be arranged

This lets a viewport compose very different content types without each content type losing scroll context.

## Native Framework Alignment

SliverWidgets does not introduce a cross-framework visual tree. Each adapter uses native primitives:

- Avalonia panels and virtualizing panels
- MAUI layouts and `CollectionView`
- Uno `ItemsRepeater`
- WinUI `ItemsRepeater`

The shared part is layout math, not rendering.

## Why This Matters

The result is a practical performance and architecture improvement:

- lists and grids can share one scroll surface
- pinned headers can be modeled in the same geometry system as lists
- large data sets avoid full child measurement
- framework packages stay thin and testable
- product teams keep native styling and accessibility

SliverWidgets is most valuable when your UI combines heterogeneous scroll sections and high-volume data.
