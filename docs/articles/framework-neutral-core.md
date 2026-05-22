---
title: Designing a Framework-Neutral Sliver Core
description: How SliverWidgets separates layout math from UI framework integration.
---

# Designing a Framework-Neutral Sliver Core

The core design goal is simple: layout algorithms should be testable without a UI framework, and framework adapters should be small enough to audit.

## Coordinate System

The core uses main-axis and cross-axis coordinates. This makes every layout algorithm axis-neutral. Vertical and horizontal behavior differ only in the adapter mapping.

```text
Core slot:
  main offset
  cross offset
  main extent
  cross extent

Vertical adapter:
  x = cross offset
  y = main offset
  width = cross extent
  height = main extent

Horizontal adapter:
  x = main offset
  y = cross offset
  width = main extent
  height = cross extent
```

## Data Contracts

The protocol has three important data contracts:

- `SliverConstraints` describes input.
- `SliverGeometry` describes the sliver as a whole.
- `SliverLayoutSlot` describes realized children.

Keeping these contracts explicit makes it possible to test layout behavior without windows, dispatchers, or rendering surfaces.

## Validation

The core validates non-negative and finite geometry values. Invalid geometry should fail early because adapter-level layout failures are harder to diagnose.

Examples:

- `PaintExtent` cannot exceed `RemainingPaintExtent`.
- `LayoutExtent` cannot exceed `PaintExtent`.
- scroll, paint, layout, cache, and cross-axis extents cannot be negative.

## Adapter Size

Framework adapters should remain thin. They should not duplicate list or grid math. If an adapter needs a new behavior, the behavior should usually be expressed as a core layout first and then mapped by the adapter.

This keeps framework differences visible while still sharing the expensive reasoning.
