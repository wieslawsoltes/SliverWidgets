# SliverWidgets Gallery Data

Shared deterministic data for the framework gallery samples.

The project provides:

- `GalleryItem` rows and tiles for large fixed-list and grid demos.
- `GallerySection` groups for header samples.
- `GalleryMetric` cards for overview panels.
- `GalleryScenario` and `GalleryDemo` descriptions that map Flutter sliver concepts to SliverWidgets features.

Use `SliverGalleryData.CreateScenarios`, `CreateItems`, `CreateUniformItems`, `CreateVariableItems`, `CreateSections`, `CreateMetrics`, and `CreateDemos` from gallery apps instead of duplicating framework-specific sample data. `GalleryScenario.TabLabel` supplies the shared short tab labels used by the unified Avalonia-style sample shells.
