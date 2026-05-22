---
uid: SliverWidgets.Core.ISliverLayout
---

# Summary
Defines the framework-neutral layout contract implemented by every core sliver.

# Remarks
Implement `ISliverLayout` when creating a new sliver algorithm. The implementation receives local `SliverConstraints` and returns a `SliverLayoutResult` containing aggregate geometry and realized child slots.
