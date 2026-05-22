---
uid: SliverWidgets.Avalonia.SliverPersistentHeader
---

# Summary
Avalonia single-child decorator for collapsing and pinned persistent headers.

# Remarks
The child is measured at the current collapsing extent and arranged according to the core persistent header layout for the current `ScrollOffset`. When `Pinned` is false, the decorator reports only the visible paint extent so adjacent content can move up without an empty reserved band.
