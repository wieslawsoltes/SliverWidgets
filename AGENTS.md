# SliverWidgets Agent Requirements

This repository uses agentic spec-driven development. Do not add feature code unless the behavior is represented in a spec, plan, task list, and traceability row.

## Mission

Build a high-performance sliver widget system for .NET UI frameworks. The core must model Flutter-style sliver constraints and geometry while integrating with Avalonia, Uno, .NET MAUI, and WinUI in idiomatic framework-specific packages.

## Non-Negotiable Requirements

- Shared behavior lives in `src/SliverWidgets.Core`.
- Framework packages stay thin and translate native measure, arrange, viewport, recycling, and animation APIs into the core protocol.
- Never place Avalonia, Uno, MAUI, or WinUI types in `SliverWidgets.Core`.
- Layout hot paths must be deterministic and allocation-conscious.
- Virtualized layouts must realize paint plus cache ranges, not entire item sources.
- Fixed-extent list and grid paths must use arithmetic index-to-offset mapping.
- Variable-extent paths must cache observed extents and support dead-reckoning behavior.
- Public API changes require docs, tests, traceability updates, and a changelog entry.
- Samples must use existing framework controls as children rather than custom rendering everything from scratch.
- Gallery samples must share `samples/SliverWidgets.GalleryData` unless a platform build constraint makes a local fallback unavoidable.
- Gallery samples must demonstrate Flutter-inspired `CustomScrollView`, `SliverGrid`, `SliverFixedExtentList`, and persistent app-bar/header concepts using framework-native controls.
- Do not hide platform limitations. Document them in the adapter matrix.

## Required Validation

Run these for default cross-platform work:

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

Run this on Windows for the WinUI package:

```bash
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
```

Run this when docs change and Lunet is installed:

```bash
cd docs
lunet build
```

## Spec Workflow

1. Update or create `plan/SPEC.md`.
2. Update or create `plan/PLAN.md`.
3. Update `specs/<feature>/tasks.md`.
4. Update `specs/<feature>/traceability.md`.
5. Implement tests before or with implementation.
6. Implement in the smallest package that owns the behavior.
7. Validate build, tests, packaging, and docs.

## Framework Rules

- Avalonia: use `Panel`, `Decorator`, and eventually `VirtualizingPanel` plus `ItemContainerGenerator` for true item virtualization.
- WinUI: use `VirtualizingLayout` for `ItemsRepeater` and keep scrolling policy outside the layout.
- Uno: mirror the WinUI API surface, but gate behavior where Uno reports unsupported APIs.
- MAUI: use `Layout` and `ILayoutManager` for layout-only surfaces; true lazy item realization requires a `CollectionView` or handler-backed track.

## Quality Bar

- Public methods validate inputs.
- Tests include edge cases: zero items, overscroll, cache-only slots, pinned headers, horizontal and vertical axes.
- Docs include migration guidance from Flutter concepts to .NET framework terms.
- Packages include README, license metadata, repository metadata, and deterministic builds.
