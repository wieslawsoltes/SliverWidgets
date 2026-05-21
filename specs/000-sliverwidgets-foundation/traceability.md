# 000 SliverWidgets Foundation Traceability

| Requirement | Acceptance Criteria | Implementation | Tests | Docs | Package | Status |
|---|---|---|---|---|---|---|
| `SW-FR-001` | `AC-001`, `AC-002` | `src/SliverWidgets.Core/SliverPrimitives.cs` | Core tests | Architecture docs | `SliverWidgets.Core` | Done |
| `SW-FR-002` | `AC-003` | `SliverConstraints` | Core tests | Architecture docs | `SliverWidgets.Core` | Done |
| `SW-FR-003` | `AC-003` | `SliverGeometry` | Core tests | Architecture docs | `SliverWidgets.Core` | Done |
| `SW-FR-004` | `AC-003` | `SliverViewportLayoutEngine`, including scroll-offset correction relayouts | Core tests | Getting started and viewport docs | `SliverWidgets.Core` | Done |
| `SW-FR-005` | `AC-003`, `AC-004` | `SliverFixedExtentListLayout` | Core and parity tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-006` | `AC-003` | `SliverListLayout`, `SliverChildExtentCache`, `SliverVariableExtentListLayout` | Core tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-007` | `AC-003` | `SliverGridLayout`, Uno/WinUI grid virtualizing layouts | Core tests | Controls docs | Core, Uno, WinUI packages | Done |
| `SW-FR-008` | `AC-003` | `SliverPersistentHeaderLayout`, `SliverAdvancedPersistentHeaderLayout`, including Flutter-style non-pinned shrink then scroll-away geometry | Core tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-009` | `AC-003` | `SliverFillRemainingLayout` | Parity tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-010` | `AC-003` | `SliverPaddingLayout` | Core tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-011` | `AC-003` | `SliverVisibilityLayout` | Core tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-012` | `AC-005` | Framework adapter projects | Build validation | Adapter docs | Adapter packages | Done |
| `SW-FR-013` | `AC-005`, `AC-006` | Uno/WinUI row and grid virtualizing layouts | Parity tests | Adapter docs | Uno/WinUI packages | Done |
| `SW-FR-014` | `AC-005`, `AC-010` | Avalonia panels, `SliverItemsControl`, smooth logical scroll steps, non-pinned persistent-header visible extent mapping, mixed sample pinned-obstruction clipping, configurable stacked/push section sticky headers with leading-edge obstruction clipping, and fixed/variable extent virtualizing panels | Build validation and Avalonia logical-scroll/header-collapse/mixed-clipping/section-sticky headless tests | Adapter and sample docs | `SliverWidgets.Avalonia` | Done |
| `SW-FR-015` | `AC-005` | MAUI layout manager with unconstrained cross-axis measurement and native-backed `SliverCollectionView` | Build validation | Adapter docs | `SliverWidgets.Maui` | Done |
| `SW-FR-016` | `AC-003` | `SliverToBoxAdapterLayout` | Core tests | Controls docs | `SliverWidgets.Core` | Done |
| `SW-FR-017` | `AC-009`, `AC-010` | Framework gallery samples, including Avalonia header shrink behavior, mixed composition clipping under pinned headers, and configurable section sticky-header behavior with stacked mode as the default without incoming-header blank bands | Sample builds and Avalonia header/mixed-clipping/section-sticky headless tests | Samples docs | Not packaged | Done |
| `SW-FR-018` | `AC-009`, `AC-010` | `samples/SliverWidgets.GalleryData` | Sample build | Samples docs | Not packaged | Done |
| `SW-FR-019` | `AC-003`, `AC-012` | `SliverViewportLayoutEngine` honors `PaintOrigin`, `LayoutExtent`, `Overlap`, cache-origin correction, and cache consumption | Core viewport tests | Flutter comparison and viewport docs | `SliverWidgets.Core` | Done |
| `SW-FR-020` | `AC-003`, `AC-012` | `SliverGridLayout.ResolveCrossAxisCount` max-extent ceiling count | Core grid tests | Flutter comparison and grid docs | `SliverWidgets.Core` | Done |
| `SW-FR-021` | `AC-010`, `AC-012` | `SliverVirtualizingGridPanel` and Avalonia adaptive grid sample | Avalonia smoke tests | Avalonia adapter docs | `SliverWidgets.Avalonia` | Done |
