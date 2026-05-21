# Flutter DataGrid and TableView Research

## Sources

- Flutter `DataTable` API docs: https://api.flutter.dev/flutter/material/DataTable-class.html
- Flutter `TwoDimensionalScrollView` API docs: https://api.flutter.dev/flutter/widgets/TwoDimensionalScrollView-class.html
- Flutter first-party `two_dimensional_scrollables` package: https://pub.dev/packages/two_dimensional_scrollables
- Flutter `TableView` API docs: https://pub.dev/documentation/two_dimensional_scrollables/latest/two_dimensional_scrollables/TableView-class.html
- Flutter `TableView` source: https://github.com/flutter/packages/blob/main/packages/two_dimensional_scrollables/lib/src/table_view/table.dart
- Flutter `DataTable` source: https://github.com/flutter/flutter/blob/master/packages/flutter/lib/src/material/data_table.dart
- Syncfusion Flutter DataGrid overview: https://help.syncfusion.com/flutter/datagrid/overview
- Syncfusion Flutter DataGrid column sizing: https://help.syncfusion.com/flutter/datagrid/columns-sizing
- Syncfusion Flutter DataGrid filtering: https://help.syncfusion.com/flutter/datagrid/filtering
- Syncfusion Flutter DataGrid API index: https://pub.dev/documentation/syncfusion_flutter_datagrid/latest/datagrid/

## Findings

Flutter's built-in `DataTable` is not the large-data baseline. Its documentation notes that automatic column sizing requires measuring table contents twice and that wrapping it in `SingleChildScrollView` mounts and paints the entire child. The same docs point large tables toward `TableView`, `PaginatedDataTable`, or `CustomScrollView`.

Flutter's first-party `two_dimensional_scrollables` `TableView` is the closest sliver-adjacent primitive. It subclasses `TwoDimensionalScrollView`, which composes a two-axis scrollable, a two-dimensional viewport, and a child delegate. The table API defines row/column spans separately from cell children, builds cells on demand for the visible plus cache area, supports finite or open-ended row/column counts, and exposes pinned leading/trailing row and column counts in builder/list constructors.

Production Flutter data-grid packages add the application behaviors expected from a grid. Syncfusion `SfDataGrid` documents widget-valued columns, column sizing, row-height customization, editing, sorting, selection, filtering, column drag/drop, column resizing, stacked headers, load more, paging, freeze panes, footer, refresh, theming, accessibility, and RTL. Its column-sizing guidance names auto, fit-by-cell, fit-by-column-name, and last-column-fill modes; its filtering guidance covers contains/equality/comparison-style filter conditions and case-sensitive text filtering. Its API exposes column-width modes, sort details, filter conditions, query-row-height callbacks, and row/cell details.

The implementation consequence for SliverWidgets is that a large DataGrid should not be represented as one monolithic stacked grid visual. The core should own row/column geometry and query projection; each framework sample should expose real native row containers with native column definitions, and adapters should virtualize row containers through the framework's recycling protocol. Full two-axis cell container virtualization remains a separate host-control problem, but the samples must no longer fake a DataGrid as generic stacked content with duplicated fixed widths.

## SliverWidgets Direction

The core should split the DataGrid feature into two parts:

- Layout: deterministic hot-path row and column windowing. It should accept vertical sliver constraints plus horizontal viewport/cache state, resolve variable row heights and column width modes, and return cell slots for paint plus cache ranges.
- Query projection: sorting and filtering should produce a visible source-row map before layout. Predicates, comparisons, and data extraction should stay out of the layout hot path.

Framework adapters should stay native:

- Avalonia gets a `VirtualizingPanel` row adapter and XAML-defined row/header `Grid` columns built from the same shared column model.
- Uno and WinUI expose package-level `SliverDataGridRowsVirtualizingLayout` adapters for `ItemsRepeater` row containers. Samples use the shared column metadata instead of hardcoded width arrays.
- MAUI uses a named `SliverDataGridCollectionView` native row virtualization adapter and documents that portable two-axis cell virtualization requires handler-backed work.

## Required Sample Coverage

- 100,000 deterministic rows.
- Mixed column width modes: fixed, auto, size-to-header, size-to-cells, star, fill, and last-column-fill.
- Variable row heights and dynamic text content.
- Text filtering plus sort toggles.
- Horizontal scrolling for wide data.
- Clear platform limitations in adapter docs.
