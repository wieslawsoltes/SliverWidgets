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
- Added `SliverWrapLayout`, deterministic 100,000-item wrap extents, Avalonia/Uno/WinUI virtualizing wrap adapters, and a shared `Wrap` gallery scenario across all framework samples.
- Added `SliverStackLayout`, deterministic 100,000-item variable stack extents, Avalonia/Uno/WinUI virtualizing stack adapters, and a shared `Stack` gallery scenario across all framework samples.
- Added `SliverDataGridLayout`, DataGrid sort/filter query projection, Avalonia DataGrid row virtualization, and shared 100,000-row `DataGrid` gallery samples across all framework samples.
