# Flutter Sliver Comparison and Remediation Plan

Date: 2026-05-21

Flutter source reviewed from `/tmp/sliverwidgets-flutter` at commit `34319ed829e598cbdde9c85c304564f19030e5a6`.

## Flutter Reference Points

| Area | Flutter source | Key behavior |
|---|---|---|
| Core protocol | `packages/flutter/lib/src/rendering/sliver.dart` | `SliverConstraints` carry local `scrollOffset`, `overlap`, paint/cache windows, axis directions, and viewport extent. `SliverGeometry` separates `paintExtent`, `paintOrigin`, `layoutExtent`, `cacheExtent`, obstruction, and correction. |
| Viewport driver | `packages/flutter/lib/src/rendering/viewport.dart` | `RenderViewportBase.layoutChildSequence` advances `scrollOffset` by `scrollExtent`, advances child placement by `layoutExtent`, computes `overlap` from painted versus laid-out space, applies `paintOrigin`, consumes `cacheExtent`, and retries on `scrollOffsetCorrection`. |
| Fixed extent list | `packages/flutter/lib/src/rendering/sliver_fixed_extent_list.dart` | Index/offset mapping is arithmetic; realized children cover paint plus cache, not the entire source. |
| Variable list | `packages/flutter/lib/src/rendering/sliver_list.dart` | Uses dead reckoning from realized children and reports `scrollOffsetCorrection` when discovered leading offsets are inconsistent. |
| Grid | `packages/flutter/lib/src/rendering/sliver_grid.dart` | `SliverGridDelegateWithMaxCrossAxisExtent` uses a ceiling count so tiles do not exceed the configured max extent. |
| Persistent header | `packages/flutter/lib/src/rendering/sliver_persistent_header.dart` | Pinned headers paint at the leading edge, report `maxScrollObstructionExtent`, and use `layoutExtent = clamp(maxExtent - scrollOffset, 0, remainingPaintExtent)`. |
| Padding/fill | `packages/flutter/lib/src/rendering/sliver_padding.dart`, `sliver_fill.dart` | Padding consumes paint/cache before delegating and propagates correction. Scroll-body fill remaining reports viewport scroll extent. |
| Widget composition | `packages/flutter/lib/src/widgets/scroll_view.dart`, `sliver.dart` | `CustomScrollView` hosts slivers in one viewport; box content is wrapped by sliver adapters. |

## Comparison Findings

| Priority | Finding | Local impact | Resolution |
|---|---|---|---|
| P1 | `SliverViewportLayoutEngine` did not honor `PaintOrigin`/`LayoutExtent` and reset cache for every sliver. | Pinned headers pushed all later slots down and later slivers could realize outside the viewport cache budget. | Implemented Flutter-style sequence layout: carry `layoutOffset`, `overlap`, `cacheOrigin`, and `remainingCacheExtent`; apply `PaintOrigin`; advance by `LayoutExtent`; consume `CacheExtent`. |
| P1 | Pinned header geometry reported `LayoutExtent == PaintExtent` and moved while partially collapsed. | Custom slivers could not rely on Flutter-style header layout semantics. | Implemented pinned `layoutExtent` formula, `paintOrigin`, obstruction, and leading-edge placement. |
| P1 | Max-cross-axis grid count used floor math. | Adaptive tiles could exceed `MaxCrossAxisExtent`. | Switched to Flutter-style ceiling count and added regression coverage. |
| P1 | Padding did not mirror Flutter cache/paint consumption and did not propagate child correction. | Wrapped correcting slivers could not relayout the viewport; cache realization before padding was over-broad. | Implemented Flutter-style padding transform and correction propagation. |
| P1 | WinUI adapters mapped `RealizationRect` as the visible paint range. | Cache-before elements could be treated as visible and trailing visible elements could be missed. | WinUI now maps `VisibleRect` to paint and `RealizationRect` to cache via `CacheOrigin`. |
| P2 | Uno exposes `VisibleRect` in API shape but reports it as not implemented. | Exact WinUI mapping cannot be trusted on Uno renderers. | Uno now avoids the unsupported API and infers a visible range from `RealizationRect` plus available size; docs call this out as a platform limitation. |
| P2 | Avalonia adaptive grid sample used non-virtual `SliverGridPanel` with a large item source. | The gallery claimed virtualization while materializing all grid containers. | Added `SliverVirtualizingGridPanel` and switched the Avalonia adaptive grid sample to it. |
| P2 | `SliverFillRemainingLayout` scroll-body behavior diverged from Flutter. | Scroll-body fill could report zero or child-sized scroll extent after preceding content exceeded the viewport. | Scroll-body fill now reports viewport main-axis extent; non-scroll-body fill keeps child/remaining behavior. |
| P2 | Geometry/constraint validation missed Flutter invariants. | Invalid positive cache origins, non-finite overlap/paint origin, and paint beyond max paint could pass. | Added validation and tests for those cases. |
| P3 | WinUI/Uno/MAUI mixed composition and variable-list samples remain adapter-limited. | Samples demonstrate concepts but not one full `CustomScrollView` equivalent on every platform. | Docs now state these limitations instead of claiming full parity. |

## Implemented Fix Plan

1. Core viewport protocol parity
   - Honor `PaintOrigin`, `LayoutExtent`, and computed `overlap`.
   - Consume cache across slivers and correct cache origin per local sliver.
   - Preserve correction relayout behavior.

2. Core layout parity
   - Correct pinned/advanced persistent-header geometry.
   - Use ceiling max-cross-axis grid math.
   - Transform padding constraints like Flutter and propagate child corrections.
   - Correct scroll-body fill remaining.
   - Strengthen validation and constructor null checks.

3. Framework adapter parity
   - Map WinUI `VisibleRect`/`RealizationRect` into paint/cache constraints.
   - Gate Uno behavior where `VisibleRect` is unsupported and document the inference.
   - Add Avalonia `SliverVirtualizingGridPanel` for adaptive grid samples.

4. Spec, docs, and tests
   - Add focused core tests for grid max extent, viewport overlap/cache behavior, fill remaining, padding correction, and validation.
   - Update Avalonia smoke tests for the virtualizing grid and Flutter-style pinned overlap.
   - Update docs and traceability.

## Remaining Documented Limitations

- Uno cannot provide exact `VisibleRect`-based paint/cache translation until the renderer implements that API.
- MAUI `SliverCollectionView.CacheExtent` remains a public hint; MAUI does not expose a cross-platform cache extent hook equivalent to Flutter's viewport cache.
- WinUI, Uno, and MAUI gallery mixed-composition pages still use platform-native sample composition rather than a single shared sliver viewport pipeline.
- The Avalonia `SliverPersistentHeader` decorator is an externally coordinated single-child adapter. Full `CustomScrollView`-style header composition is demonstrated by the mixed and sectioned panels.
