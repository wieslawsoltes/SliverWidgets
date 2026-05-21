# Flutter DataGrid and TableView Research

## Sources

- Flutter `DataTable` API docs: https://api.flutter.dev/flutter/material/DataTable-class.html
- Flutter `TwoDimensionalScrollView` API docs: https://api.flutter.dev/flutter/widgets/TwoDimensionalScrollView/TwoDimensionalScrollView.html
- Flutter first-party `two_dimensional_scrollables` package: https://pub.dev/packages/two_dimensional_scrollables
- Flutter `TableView` source: https://flutter.googlesource.com/mirrors/packages/+/refs/tags/camera_avfoundation-v0.9.20/packages/two_dimensional_scrollables/lib/src/table_view/table.dart
- Flutter `TableCellBuilderDelegate` source: https://flutter.googlesource.com/mirrors/packages/+/refs/tags/path_provider_foundation-v2.4.2/packages/two_dimensional_scrollables/lib/src/table_view/table_delegate.dart
- Syncfusion Flutter DataGrid overview: https://help.syncfusion.com/flutter/datagrid/overview
- Syncfusion Flutter DataGrid column sizing: https://help.syncfusion.com/flutter/datagrid/columns-sizing
- Syncfusion Flutter DataGrid filtering: https://help.syncfusion.com/flutter/datagrid/filtering
- Syncfusion Flutter DataGrid API index: https://pub.dev/documentation/syncfusion_flutter_datagrid/latest/datagrid/

## Findings

Flutter's built-in `DataTable` is not the large-data baseline. Its documentation notes that automatic column sizing requires measuring table contents twice and that wrapping it in `SingleChildScrollView` mounts and paints the entire child. The same docs point large tables toward `TableView`, `PaginatedDataTable`, or `CustomScrollView`.

Flutter's first-party `two_dimensional_scrollables` `TableView` is the closest sliver-adjacent primitive. It subclasses `TwoDimensionalScrollView`, lazily builds table cells through `TableCellBuilderDelegate`, supports finite or infinite row/column counts, has separate row and column span builders, and supports pinned rows and pinned columns. Its renderer lays out pinned row/column regions separately from the scrolled body.

Production Flutter data-grid packages add the application behaviors expected from a grid. Syncfusion `SfDataGrid` documents widget-valued columns, column sizing, row-height customization, editing, sorting, selection, filtering, column drag/drop, column resizing, stacked headers, load more, paging, freeze panes, footer, refresh, theming, accessibility, and RTL. Its column-sizing guidance names auto, fit-by-cell, fit-by-column-name, and last-column-fill modes; its filtering guidance covers contains/equality/comparison-style filter conditions and case-sensitive text filtering. Its API exposes column-width modes, sort details, filter conditions, query-row-height callbacks, and row/cell details.

## SliverWidgets Direction

The core should split the DataGrid feature into two parts:

- Layout: deterministic hot-path row and column windowing. It should accept vertical sliver constraints plus horizontal viewport/cache state, resolve variable row heights and column width modes, and return cell slots for paint plus cache ranges.
- Query projection: sorting and filtering should produce a visible source-row map before layout. Predicates, comparisons, and data extraction should stay out of the layout hot path.

Framework adapters should stay native:

- Avalonia gets a `VirtualizingPanel` row adapter and sample rows built from normal controls.
- Uno and WinUI use `ItemsRepeater`/native row templates for the sample, backed by the same shared data and core query/layout model.
- MAUI uses native `CollectionView` row virtualization and documents that portable two-axis cell virtualization requires handler-backed work.

## Required Sample Coverage

- 100,000 deterministic rows.
- Mixed column width modes: fixed, auto, size-to-header, size-to-cells, star, fill, and last-column-fill.
- Variable row heights and dynamic text content.
- Text filtering plus sort toggles.
- Horizontal scrolling for wide data.
- Clear platform limitations in adapter docs.
