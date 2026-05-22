---
title: Performance Model
description: How SliverWidgets keeps layout work bounded.
---

# Performance Model

SliverWidgets improves performance by bounding work to the viewport plus cache range. The main design question for every layout is: can we find the realized range without measuring every item?

## Fixed Extent

Fixed extent lists are the ideal case. If every item is `44` units tall with `2` units of spacing, the item interval is `46`. The first candidate index for a scroll offset is a division.

This makes a million-row list cheap to query.

## Grid Rows

Grids are also efficient when tile size can be resolved from viewport width and spacing. The layout computes row count, row height, and the first visible row, then realizes only items in rows that intersect the cache range.

## Variable Extents

Variable extents require estimates. SliverWidgets uses a measured extent cache:

- exact values for measured children
- average observed extent for missing children
- binary search over estimated leading offsets

This model is not as exact as a fully measured list, but it avoids the cost that makes fully measured lists unsuitable for large data.

## Framework Costs

Sliver math is only part of total UI cost. Real application performance also depends on:

- item template complexity
- text measurement and wrapping
- image loading
- data binding overhead
- native control recycling behavior
- scroll event frequency

The gallery apps intentionally include large data counts so these costs are visible during manual profiling.

## Production Guidance

Use fixed extents where design allows. Use variable extents when content requires it. Avoid nested scroll viewers for mixed content. Tune cache extent based on actual scroll behavior rather than guessing.
