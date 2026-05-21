---
uid: SliverWidgets.Core.SliverGridLayout
---

# Summary
Lays out items in a fixed-count or max-cross-axis-extent grid.

# Remarks
The grid resolves visible row ranges from the cache window and returns slots for each realized item in those rows. It supports explicit `MainAxisExtent` or derived extent from `ChildAspectRatio`, and max-cross-axis sizing uses a ceiling count so tiles stay within the configured maximum.
