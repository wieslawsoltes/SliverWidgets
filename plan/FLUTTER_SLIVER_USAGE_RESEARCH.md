# Flutter Sliver Usage Research

Date: 2026-05-21

## Sources Reviewed

| Source | Usage pattern | Local decision |
|---|---|---|
| [Flutter docs: `Using slivers to achieve fancy scrolling`](https://docs.flutter.dev/ui/layout/scrolling/slivers) | Slivers are specialized portions of scrollable areas used for custom scrolling effects. The docs point to `CustomScrollView`, `SliverAppBar`, `SliverGrid`, and `SliverList` as core APIs. | Keep the existing fixed/list/grid/header scenarios as the base and add scenarios only when they teach another real composition pattern. |
| [Flutter docs: `Advanced scrolling and slivers`](https://docs.flutter.dev/learn/pathway/tutorial/slivers) | The tutorial combines `CustomScrollView`, collapsible navigation/search, `SliverFillRemaining`, and sectioned `SliverList` content. | Existing samples cover fill and sections; add a dedicated tabbed/nested navigation sample because search/tabbed navigation is not yet represented. |
| [Flutter cookbook: floating app bar above a list](https://docs.flutter.dev/cookbook/lists/floating-app-bar) | `CustomScrollView` synchronizes a `SliverAppBar` and list content, and `SliverAppBar` can include titles, images, tabs, and flexible space. | Existing header sample covers collapse; the new `Tabs` sample should show header plus tab strip coordination. |
| [Flutter `NestedScrollView` API sample](https://github.com/flutter/flutter/blob/e8a4e7da0d812d457d2b46e6ef3a94f9e40c8fe0/examples/api/lib/widgets/nested_scroll_view/nested_scroll_view.0.dart) | Uses `NestedScrollView`, `SliverOverlapAbsorber`, `SliverAppBar`, `TabBar`, `TabBarView`, inner `CustomScrollView`, and `SliverOverlapInjector` so tab content does not slip under the app bar. | Add a `Tabs` gallery scenario across all UI samples. Frameworks use native tab or segmented controls and explicit header spacing/overlay notes until a shared nested-scroll adapter exists. |
| [Flutter Gallery `tabs_demo.dart`](https://github.com/flutter/flutter/blob/e8a4e7da0d812d457d2b46e6ef3a94f9e40c8fe0/dev/integration_tests/flutter_gallery/lib/demo/material/tabs_demo.dart) | Uses the same `NestedScrollView`/`SliverOverlapAbsorber`/`SliverAppBar`/`TabBar` pattern in a real demo app. | Treat tabbed nested scrolling as common enough to be part of the canonical gallery catalog. |

## Added Scenario

`nested-tabs` / `Tabs` demonstrates the Flutter pattern where a collapsible or pinned app bar owns a tab strip, and each tab owns its own list/grid scroll body while overlap is coordinated between the outer and inner scroll views.

Avalonia uses native `TabControl` plus sliver-backed inner scroll content. MAUI, Uno, and WinUI use native/idiomatic segmented tab buttons with native scroll viewports, keeping the limitation explicit: they demonstrate the pattern but do not yet implement a shared `NestedScrollView` equivalent with an overlap absorber/injector protocol.
