# Gallery Unification Plan

## Research Baseline

The shared sample catalog is based on the common Flutter sliver scenarios documented by Flutter:

- `CustomScrollView` hosts ordered slivers in one viewport and is the conceptual root for mixed samples: https://api.flutter.dev/flutter/widgets/CustomScrollView/slivers.html
- `SliverFixedExtentList` is the high-performance uniform row path because child main-axis extents are known without measuring every child: https://api.flutter.dev/flutter/widgets/SliverFixedExtentList-class.html
- `SliverAppBar` integrates with `CustomScrollView` and changes height with scroll offset: https://api.flutter.dev/flutter/material/SliverAppBar-class.html
- `NestedScrollView` documents the common collapsible app bar plus tabs pattern: https://api.flutter.dev/flutter/widgets/NestedScrollView-class.html
- Flutter's sliver tutorial documents that regular box widgets need sliver adapters such as `SliverToBoxAdapter` or `SliverFillRemaining`: https://docs.flutter.dev/learn/tutorial/slivers

## Unified Scenario Catalog

Every framework gallery must project the same scenario list from `SliverGalleryData.CreateScenarios()`:

| Key | Scenario | Flutter model | Required proof |
|---|---|---|---|
| `fixed-large-list` | Fixed large list | `SliverFixedExtentList` | Uniform rows, explicit item extent, large source, visible scrollbar, jump or cache controls. |
| `variable-list` | Variable and non-uniform list | `SliverList`, `SliverPrototypeExtentList`, `SliverVariedExtentList` | Mixed deterministic row heights and measured layout behavior. |
| `adaptive-grid` | Adaptive grid | `SliverGridDelegateWithMaxCrossAxisExtent` | Responsive columns, spacing controls, large enough data to scroll. |
| `pinned-header` | Pinned and collapsible header | `SliverAppBar`, `SliverPersistentHeader` | Header min/max extent, visible obstruction behavior, list scroll underneath. |
| `nested-tabs` | Tabbed nested scroll | `NestedScrollView`, `SliverOverlapAbsorber`, `SliverAppBar`, `TabBar` | Pinned header area, tab strip, and independent scroll bodies that do not render under the header. |
| `mixed-composition` | Mixed CustomScrollView composition | `CustomScrollView` with multiple slivers | Box content, fixed rows, grid, fill region in one scroll surface. |
| `sectioned-headers` | Sectioned list with sticky headers | `SliverPersistentHeader` plus `SliverList` | Grouped data, current section affordance, virtualized rows where available. |
| `fill-padding-visibility` | Fill, padding, and visibility | `SliverPadding`, `SliverFillRemaining`, `SliverVisibility` | Insets, replacement visibility, fill remaining behavior. |
| `cache-stress` | Cache and performance stress | sliver delegate cache windows | Large deterministic source and tunable cache/extent settings. |

## Framework Projection Rules

- Avalonia is the reference gallery because it has fixed, grid, persistent header, and variable virtualizing panels. It should use native `ScrollViewer` scrollbars and avoid nested scroll owners inside one tab.
- Uno and WinUI should use `ItemsRepeater` plus SliverWidgets virtualizing layouts for fixed lists and grids. Variable-height samples may use native measured repeater layout until dedicated variable-extent adapters exist, but the sample must label this clearly.
- MAUI should use `SliverCollectionView` for virtualized fixed lists, grids, sections, and cache demos. Non-uniform rows can use native measured `CollectionView` sizing, while fixed-list performance remains covered by `SliverCollectionView`.
- All galleries must use the same scenario names, summaries, and Avalonia-derived shell structure. Framework-specific copy belongs in small notes, not in divergent navigation structures.

## Implementation Steps

1. Add `GalleryScenario` and `GalleryScenarioKind` to `samples/SliverWidgets.GalleryData`.
2. Expand `SliverGalleryData.CreateDemos()` to derive from the shared scenario list.
3. Update Avalonia tabs to match the nine shared scenario names, including non-uniform rows, tabbed nested scroll, sectioned headers, fill/visibility, and cache stress.
4. Update MAUI, Uno, and WinUI navigation pages to the same nine short scenario tabs and use the shared scenario data for descriptions.
5. Update MAUI to the same nine sections, with bounded viewport heights so long lists and grids visibly scroll.
6. Add or update tests that assert the shared catalog count and Avalonia tabs stay aligned with the catalog.
7. Run the default test suite plus gallery builds and inspect generated Avalonia screenshots for scrollbars, overlapping text, and scroll movement.

## Acceptance Criteria

- `SliverGalleryData.CreateScenarios()` returns all nine scenarios in a stable order.
- Avalonia, Uno, WinUI, and MAUI expose all nine scenario names through the same short tabs: `Fixed`, `Variable`, `Grid`, `Header`, `Tabs`, `Mixed`, `Sections`, `Fill`, and `Cache`.
- Avalonia smoke tests render and scroll every tab without overlapping first rows or missing scroll extent.
- Core and parity tests pass.
- Framework sample projects compile on this host where platform tooling allows it.

## Implementation Notes

- `samples/SliverWidgets.GalleryData` now owns the canonical `GalleryScenario` catalog and derives legacy `GalleryDemo` cards from it.
- Avalonia, Uno, WinUI, and MAUI now consume `CreateScenarios()` for navigation, section labels, and scenario-card content instead of maintaining separate scenario vocabularies.
- MAUI, Uno, and WinUI now follow Avalonia's sample shell: top metric cards, a horizontal short-tab selector, a left scenario controls/notes panel, and a right native viewport.
- Avalonia is the most complete adapter-backed gallery: fixed and variable virtualizing panels, max-extent grid, persistent header, tabbed nested-scroll projection, mixed sliver preview, sectioned sliver blocks, fill/padding/visibility, and cache stress.
- Uno and WinUI keep fixed/grid/cache/tabbed paths on `ItemsRepeater` and SliverWidgets virtualizing layouts; variable-height, nested-scroll, and persistent-header samples are labeled as native/conceptual where dedicated adapters do not exist yet.
- MAUI keeps fixed/grid/section/cache paths on native-backed `SliverCollectionView`; measured variable rows, tabbed nested-scroll bodies, header overlays, and utility sliver composition are represented through native MAUI layout primitives with limitations documented in the sample README.
- Avalonia virtualized panels now use a finite initial viewport fallback when `ScrollViewer` measures with an infinite main-axis size, which prevents first-paint blank space and realizes enough rows to fill the viewport.
