---
uid: SliverWidgets.Core.SliverChildExtentCache
---

# Summary
Stores observed child extents and estimates missing extents for variable-size list layout.

# Remarks
The cache uses the average observed extent when possible and falls back to `DefaultExtent` before any child has been observed. This lets variable-size lists estimate offsets without measuring the full item set.
