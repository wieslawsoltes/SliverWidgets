# Research Notes

Flutter separates box layout and sliver layout. Sliver layout answers scroll extent, paint extent, layout extent, cache extent, obstruction, hit testing, and overflow independently. SliverWidgets mirrors that separation in `SliverConstraints` and `SliverGeometry`.

Avalonia and WinUI/Uno expose the strongest virtualization hooks. MAUI exposes strong custom layout APIs but not a direct virtualized item realization protocol for arbitrary layouts.

The implementation therefore uses:

- core geometry and realization math in `SliverWidgets.Core`;
- Avalonia `Panel` and `Decorator` adapters for layout MVP;
- WinUI/Uno `VirtualizingLayout` for `ItemsRepeater`;
- MAUI `Layout` + `ILayoutManager` for layout MVP.
