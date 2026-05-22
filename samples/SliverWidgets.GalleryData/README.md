# SliverWidgets Gallery Data

Shared deterministic data for the framework gallery samples.

The project provides:

- `GalleryItem` rows, variable stack cards, tiles, and wrap chips for large fixed-list, stack, grid, and variable-wrap demos.
- `GalleryDataGridRow` and `GalleryDataGridColumn` records for the shared 100,000-row DataGrid demo, including resolved native column widths used by every gallery.
- `GallerySection` groups for header samples.
- `GalleryMetric` cards for overview panels.
- `GalleryScenario` and `GalleryDemo` descriptions that map Flutter sliver concepts to SliverWidgets features.

Use `SliverGalleryData.CreateScenarios`, `CreateItems`, `CreateUniformItems`, `CreateVariableItems`, `CreateStackItems`, `CreateDataGridRows`, `CreateDataGridColumns`, `CreateWrapItems`, `CreateSections`, `CreateMetrics`, and `CreateDemos` from gallery apps instead of duplicating framework-specific sample data. `GalleryScenario.TabLabel` supplies the shared short tab labels used by the unified Avalonia-style sample shells. `DataGridColumnSpacing`, `DataGridContentWidth`, and `DataGridTableWidth` keep DataGrid row/header columns aligned across Avalonia, MAUI, Uno, and WinUI.
