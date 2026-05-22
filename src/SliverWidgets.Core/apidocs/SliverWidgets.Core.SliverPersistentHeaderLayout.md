---
uid: SliverWidgets.Core.SliverPersistentHeaderLayout
---

# Summary
Lays out a collapsing header between configured minimum and maximum extents.

# Remarks
Pinned headers paint at the leading edge, report shrinking `LayoutExtent`, and expose obstruction geometry so composed viewports can clip or account for occupied pinned space.

Non-pinned headers shrink at the leading edge until they reach their minimum extent, then scroll away while reserving only the visible paint extent for following slivers.
