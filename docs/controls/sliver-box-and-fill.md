---
title: Box, Fill, Padding, and Visibility
description: Utility slivers for mixed scroll compositions.
---

# Box, Fill, Padding, and Visibility

Utility slivers let normal content participate in a sliver sequence without special framework logic.

## SliverToBoxAdapterLayout

Use `SliverToBoxAdapterLayout` for a single fixed-size element:

```csharp
var hero = new SliverToBoxAdapterLayout(
    new SliverToBoxAdapterOptions(
        MainAxisExtent: 240,
        CrossAxisExtent: null));
```

This is useful for hero regions, charts, banners, and other one-off content inside a mixed viewport.

## SliverFillRemainingLayout

Use `SliverFillRemainingLayout` when the last child should occupy remaining viewport space:

```csharp
var fill = new SliverFillRemainingLayout(
    new SliverFillRemainingOptions(
        ChildExtent: null,
        HasScrollBody: false));
```

Common examples are empty states, detail panes, and footer regions.

When `HasScrollBody` is `true`, the layout follows Flutter's scrollable fill-remaining path and reports the viewport main-axis extent as its scroll extent. When `HasScrollBody` is `false`, it uses the larger of the remaining viewport space and the child extent.

## SliverPaddingLayout

`SliverPaddingLayout` transforms child constraints and offsets returned slots:

```csharp
var padded = new SliverPaddingLayout(
    new SliverEdgeInsets(
        Before: 16,
        After: 24,
        CrossBefore: 20,
        CrossAfter: 20),
    child);
```

Padding contributes to total scroll extent, reduces the child cross-axis extent, consumes leading/trailing paint and cache, and propagates child scroll-offset corrections to the viewport.

## SliverVisibilityLayout

Use `SliverVisibilityLayout` to show, hide, replace, or maintain a child:

```csharp
var layout = new SliverVisibilityLayout(
    isVisible: filtersOpen,
    child: filtersSliver,
    replacement: null,
    maintainSize: false);
```

When `maintainSize` is `true`, the child is laid out but its slots are removed. This preserves scroll extent while hiding realized content.
