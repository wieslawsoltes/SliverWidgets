---
uid: SliverWidgets.Core.SliverFixedExtentListLayout
---

# Summary
Lays out uniform-size list items using arithmetic visible-range calculation.

# Remarks
This is the fastest list layout. It is appropriate for large row counts when each item has the same main-axis extent and spacing.

# Example
```csharp
var layout = new SliverFixedExtentListLayout(
    new SliverFixedExtentListOptions(100_000, 44, 2));
```
