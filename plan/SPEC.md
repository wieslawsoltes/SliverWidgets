# SliverWidgets Specification

## Problem

.NET UI frameworks have strong layout systems, but there is no shared Flutter-compatible sliver model that can compose pinned headers, lazy lists, grids, fill-remaining regions, padding, and visibility in one scrollable surface while still using native framework controls.

SliverWidgets provides a framework-neutral core protocol and thin framework adapters for Avalonia, Uno, MAUI, and WinUI.

## References

- Flutter Slivers Demystified: https://blog.flutter.dev/slivers-demystified-6ff68ab0296f
- Flutter sliver docs: https://docs.flutter.dev/ui/layout/scrolling/slivers
- Flutter `SliverConstraints`: https://api.flutter.dev/flutter/rendering/SliverConstraints-class.html
- Flutter `SliverGeometry`: https://api.flutter.dev/flutter/rendering/SliverGeometry-class.html
- Microsoft spec-driven development article: https://techcommunity.microsoft.com/blog/azuredevcommunityblog/moving-beyond-prompts-a-practical-introduction-to-spec-driven-development/4511012
- Avalonia layout and virtualization APIs: https://docs.avaloniaui.net/docs/custom-controls/custom-panel
- WinUI ItemsRepeater and VirtualizingLayout: https://learn.microsoft.com/windows/apps/develop/ui/controls/items-repeater
- MAUI custom layouts: https://learn.microsoft.com/dotnet/maui/user-interface/layouts/custom
- Uno ItemsRepeater support: https://platform.uno/docs/articles/implemented/microsoft-ui-xaml-controls-itemsrepeater.html

## Goals

- Provide a core sliver protocol with `SliverConstraints`, `SliverGeometry`, `ISliverLayout`, layout slots, and viewport composition.
- Implement foundational slivers: fixed-extent list, variable-extent list, grid, persistent header, fill remaining, padding, and visibility.
- Provide framework packages:
  - `SliverWidgets.Avalonia`
  - `SliverWidgets.Uno`
  - `SliverWidgets.Maui`
  - `SliverWidgets.WinUI`
- Support existing controls as sliver children wherever framework layout APIs allow it.
- Provide gallery-style samples for each supported framework that mirror Flutter sliver teaching examples using native controls and large deterministic data.
- Package all libraries for NuGet.
- Provide samples, tests, integration/parity tests, and Lunet documentation.

## Non-Goals

- Do not clone Flutter rendering internals.
- Do not replace native accessibility, focus, input, or styling systems.
- Do not force all frameworks into one UI element abstraction.
- Do not claim MAUI has true sliver item virtualization without a handler-backed or `CollectionView` integration path.
- Do not hide sample host limitations, especially WinUI Windows runtime validation and platform-specific MAUI heads.

## Functional Requirements

- `SW-FR-001`: Core shall expose a framework-neutral `ISliverLayout` protocol.
- `SW-FR-002`: Core shall model Flutter-style constraints including local scroll offset, paint extent, cache extent, viewport extent, cross-axis extent, preceding scroll extent, overlap, growth direction, and user scroll direction.
- `SW-FR-003`: Core shall model geometry separately for scroll extent, paint extent, layout extent, cache extent, hit testing, obstruction, overflow, and visibility.
- `SW-FR-004`: Core shall compose mixed slivers in one viewport and honor finite scroll-offset correction requests by relaying out from the corrected offset.
- `SW-FR-005`: Fixed-extent lists shall compute scroll extents and visible indexes using arithmetic.
- `SW-FR-006`: Variable lists shall support observed child extents, extent caching, and dead-reckoned missing extents.
- `SW-FR-007`: Grids shall support fixed cross-axis count and max cross-axis extent delegates.
- `SW-FR-008`: Persistent headers shall support min/max extents, pinned behavior, floating behavior, and deterministic snap services.
- `SW-FR-009`: Fill-remaining shall size a child to remaining viewport space.
- `SW-FR-010`: Padding shall transform child constraints and slot offsets.
- `SW-FR-011`: Visibility shall remove, replace, or maintain sliver size.
- `SW-FR-012`: Framework adapters shall arrange existing controls using core layout slots.
- `SW-FR-013`: WinUI and Uno shall provide `ItemsRepeater` `VirtualizingLayout` paths for fixed-extent rows and grids.
- `SW-FR-014`: Avalonia shall provide non-virtual panels, fixed/variable extent `VirtualizingPanel` adapters, smooth pixel-sized logical scroll increments, direct-child mixed composition clipping below pinned header obstruction, configurable stacked or push section sticky headers, and an items host that exposes logical panel scrolling to native `ScrollViewer` hosts.
- `SW-FR-015`: MAUI shall provide a layout manager path that preserves unconstrained cross-axis desired size and a native-backed `CollectionView` virtualization integration.
- `SW-FR-016`: Core shall provide a sliver-to-box adapter for single fixed box content.
- `SW-FR-017`: Each supported framework shall provide a gallery-style sample app or sample surface that demonstrates fixed lists, adaptive grids, persistent headers, mixed sliver composition without visual overlap through pinned headers, and large-data virtualization.
- `SW-FR-018`: Gallery samples shall use shared deterministic data so virtualization behavior, row counts, and visual content are comparable across frameworks.

## Non-Functional Requirements

- `SW-NFR-001`: Layout hot paths must avoid unnecessary per-item work.
- `SW-NFR-002`: All public APIs must validate invalid negative and NaN geometry values.
- `SW-NFR-003`: Core must be usable from any .NET UI framework without UI dependencies.
- `SW-NFR-004`: Default solution must build on macOS with the installed .NET SDK.
- `SW-NFR-005`: Windows App SDK validation must be documented as a Windows lane.
- `SW-NFR-006`: Documentation must use Lunet and provide architecture, quickstart, samples, testing, and packaging pages.

## Acceptance Criteria

- `AC-001`: `dotnet build SliverWidgets.slnx` succeeds.
- `AC-002`: `dotnet test SliverWidgets.slnx` succeeds.
- `AC-003`: Core tests cover fixed list, variable list, variable extent cache, grid, pinned/floating/snap headers, box adapter, padding, visibility, and mixed viewport composition.
- `AC-004`: Framework parity tests prove fixed-extent realization windows are axis-neutral and cache-aware.
- `AC-005`: Avalonia, MAUI, and Uno adapter projects compile in the default solution.
- `AC-006`: WinUI source compiles on non-Windows hosts with PRI generation disabled; full runtime validation is documented for Windows hosts.
- `AC-007`: NuGet pack creates package artifacts for default solution projects.
- `AC-008`: `plan/PLAN.md`, `plan/SPEC.md`, `AGENTS.md`, docs, samples, and traceability exist.
- `AC-009`: Avalonia, MAUI, Uno, and WinUI gallery samples exist and document platform-specific run/build commands.
- `AC-010`: Gallery samples include Flutter-inspired examples covering fixed large lists, variable/non-uniform lists, adaptive grids, pinned/collapsible headers, sectioned headers, mixed `CustomScrollView` composition, fill/padding/visibility, and cache/performance stress.
- `AC-011`: Avalonia, MAUI, Uno, and WinUI galleries shall project the same scenario list from `SliverGalleryData.CreateScenarios()` and document framework-specific limitations where a native adapter is conceptual rather than fully implemented.

## Current Implementation Status

- Core protocol, foundational layouts, scroll-offset correction relayouts, finite geometry validation, variable extent cache/dead reckoning, sliver-to-box adapter, and floating/snap header service: implemented.
- Avalonia panels, decorator, fixed-extent `VirtualizingPanel`, variable-extent `VirtualizingPanel`, smooth logical scroll increments, mixed composition pinned-obstruction clipping, configurable section sticky-header modes with stacked as the gallery default, and logical `SliverItemsControl` scroll host: implemented.
- MAUI stack layout manager with unconstrained cross-axis measurement and native-backed `SliverCollectionView`: implemented.
- Uno fixed-extent and grid virtualizing layouts: implemented using `RealizationRect`; renderer-specific validation remains required.
- WinUI fixed-extent and grid virtualizing layouts: implemented; build works on non-Windows hosts with PRI generation disabled, and full runtime validation remains a Windows lane.
- Shared gallery scenario data and framework gallery apps: implemented. Avalonia, MAUI, Uno, and WinUI expose the same eight scenario catalog entries. Avalonia, MAUI, Uno, and WinUI compile on macOS; WinUI runtime validation remains a Windows lane.
