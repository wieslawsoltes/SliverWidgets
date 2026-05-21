# Changelog

## Unreleased

- Added `SliverVirtualizingGridPanel` for Avalonia `ItemsControl` grid virtualization.
- Updated core viewport composition to honor Flutter-style `PaintOrigin`, `LayoutExtent`, `Overlap`, and per-sliver cache consumption.
- Corrected max-cross-axis grid sizing, pinned header geometry, padding correction propagation, and scroll-body fill remaining behavior.
- Fixed non-pinned persistent header geometry so headers shrink before scrolling away without reserving an empty minimum-extent gap.
- Fixed Avalonia sectioned gallery clipping so incoming stacked headers no longer blank rows before they reach the leading pinned-header run.
- Fixed Avalonia unconstrained cross-axis measure handling and realized-container clearing order for virtualizing panels.
- Updated WinUI paint/cache mapping and documented Uno, MAUI, and gallery composition limitations.
- Unified MAUI, Uno, and WinUI gallery shells with the Avalonia reference header, metric cards, scenario tabs, and left-controls/right-viewport layout.
- Added a shared `Tabs` gallery scenario based on Flutter `NestedScrollView`/`SliverOverlapAbsorber`/`SliverAppBar` tabbed usage and projected it across Avalonia, MAUI, Uno, and WinUI samples.
