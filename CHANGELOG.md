# Changelog

## 0.1.0

- Added framework-neutral sliver constraints, geometry, layout slots, and viewport composition.
- Added fixed-extent list, variable list, grid, persistent header, fill remaining, padding, and visibility layouts.
- Added Avalonia, MAUI, Uno, and WinUI adapter projects.
- Added Avalonia fixed and variable extent virtualizing panels.
- Added Uno and WinUI grid virtualizing layouts.
- Added MAUI native-backed `SliverCollectionView`.
- Added variable extent cache/dead reckoning, sliver-to-box adapter, and floating/snap header core services.
- Added Avalonia, MAUI, Uno, and WinUI gallery samples with shared deterministic gallery data.
- Added a runnable Uno desktop gallery host and gallery sample solution files.
- Added core and framework parity tests.
- Added spec-driven plan, docs shell, sample docs, package metadata, and CI workflows.
- Fixed variable-list scroll extent reporting when realization stops before the full item source is scanned.
- Fixed cache-window realization for negative cache origins, exact paint/cache boundaries, and padded child slivers.
- Fixed Avalonia grid panel child measurement to use computed tile dimensions.
- Fixed Avalonia gallery scrolling by adding a logical `SliverItemsControl` host that forwards `ScrollViewer` offsets to sliver panels instead of physically scrolling realized item windows.
- Fixed Avalonia persistent-header collapse smoothness by using pixel-sized logical scroll increments instead of item-sized wheel jumps.
- Fixed Avalonia mixed gallery composition so scrolled fixed rows, grid tiles, and fill regions are clipped below pinned header obstruction instead of visually rendering through the header.
- Fixed Avalonia sectioned gallery scrolling so one sticky section header shrinks smoothly, pushes out for the next header, and does not leave cumulative empty header gaps.
- Fixed core viewport composition to honor finite `ScrollOffsetCorrection` relayout requests and reject infinite non-negative geometry values.
- Fixed MAUI `SliverStackLayout` measurement so unconstrained cross-axis layout uses child desired size instead of collapsing to zero.
- Fixed non-Windows WinUI library and gallery builds by disabling PRI generation for code-only validation.
- Pinned sample transitive `Tmds.DBus.Protocol` dependency to a patched version.
