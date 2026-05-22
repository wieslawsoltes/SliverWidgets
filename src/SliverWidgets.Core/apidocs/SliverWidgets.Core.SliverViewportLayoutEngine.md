---
uid: SliverWidgets.Core.SliverViewportLayoutEngine
---

# Summary
Composes multiple slivers into one viewport and returns viewport-space realized slots.

# Remarks
Use this engine for mixed scroll compositions, custom framework adapters, and tests. The engine gives each sliver local constraints, validates geometry, offsets returned slots by `PaintOrigin` and `LayoutExtent`, and consumes each sliver's cache extent before laying out the next sliver.
