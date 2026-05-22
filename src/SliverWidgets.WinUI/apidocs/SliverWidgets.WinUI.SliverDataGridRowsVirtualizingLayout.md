---
uid: SliverWidgets.WinUI.SliverDataGridRowsVirtualizingLayout
---

# Summary
Windows App SDK `ItemsRepeater` layout for virtualized variable-height DataGrid row containers.

# Remarks
The layout uses `VisibleRect` for the paint window and `RealizationRect` for cache. Set `TableWidth` to the full row container width and `RowExtentSelector` to return the per-row height. Call `InvalidateItems()` after replacing the projected row source.
