---
uid: SliverWidgets.Avalonia.SliverItemsControl
---

# Summary
Avalonia `ItemsControl` host that exposes a sliver items panel as the logical scrollable content of an outer `ScrollViewer`.

# Remarks
Use this control instead of a plain `ItemsControl` when wrapping `SliverVirtualizingStackPanel`, `SliverVirtualizingListPanel`, or another sliver `ILogicalScrollable` items panel in a `ScrollViewer`. It forwards scroll offset, extent, viewport, and bring-into-view calls to the generated panel so the panel realizes the paint plus cache range for the actual native scroll offset.
