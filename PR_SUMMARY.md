# Summary

This PR now covers a Flutter-parity pass for SliverWidgets plus the Avalonia gallery fixes that fell out of testing real scroll behavior.

The main change is that core sliver geometry now follows Flutter more closely for viewport composition, cache consumption, `PaintOrigin`, `LayoutExtent`, pinned and non-pinned persistent headers, padding, fill remaining, and adaptive grid sizing. The Avalonia gallery also uses virtualized native controls for grid samples and no longer shows blank bands when section headers or non-pinned persistent headers enter and leave the viewport.

## What Changed

- Added a Flutter source comparison and implementation plan in `plan/FLUTTER_SLIVER_COMPARISON.md`.
- Updated the spec, plan, task list, traceability matrix, changelog, and docs for the parity work.
- Fixed core viewport layout to compose slivers with Flutter-style paint origin, overlap, layout extent, scroll offset correction, and cache extent consumption.
- Fixed core sliver layouts:
  - max-cross-axis grid count uses Flutter-style ceiling math.
  - pinned headers report obstruction, paint origin, and cache geometry consistently.
  - non-pinned persistent headers shrink at the leading edge before scrolling away, without reserving an empty minimum-extent band.
  - padding propagates child scroll corrections and consumes paint/cache like Flutter.
  - fill remaining uses viewport-based scroll-body extent.
  - geometry and constraint validation reject invalid non-finite values.
- Added Avalonia `SliverVirtualizingGridPanel` and moved the adaptive grid gallery sample onto it.
- Fixed Avalonia gallery composition bugs:
  - incoming stacked section headers no longer clip rows before they reach the leading pinned-header run.
  - mixed sliver preview clipping only accounts for the active leading pinned obstruction.
  - non-pinned persistent headers report only their visible paint extent, so following rows move up smoothly.
  - unconstrained cross-axis measure no longer collapses sliver children to zero width.
  - virtualizing panels remove realized mappings before clearing item containers.
- Updated WinUI virtualizing layouts to map `VisibleRect` to paint and `RealizationRect` to cache.
- Updated Uno virtualizing layouts to infer visible range from realization data where Uno does not support `VisibleRect`.
- Expanded core and Avalonia headless tests for cache windows, header geometry, padding correction, fill remaining, adaptive grids, section headers, and gallery behavior.

## Root Cause

The earlier implementation treated several Flutter sliver concepts as simple visible extents. That missed important distinctions between paint extent, layout extent, cache extent, paint origin, overlap, and scroll obstruction. Those gaps showed up as blank bands in Avalonia because following content was laid out behind reserved header space or clipped by headers that were merely visible, not actually stacked at the leading edge.

The fix moves the core protocol closer to Flutter's `RenderViewport` and `RenderSliverPersistentHeader` behavior, then keeps framework adapters thin by translating native viewport and realization APIs into that protocol.

## Developer Impact

- Core layout behavior is more predictable and closer to Flutter for mixed sliver compositions.
- Avalonia samples now demonstrate large fixed lists, adaptive virtualized grids, persistent headers, section headers, fill behavior, cache behavior, and mixed composition with native controls.
- WinUI and Uno adapters have clearer paint/cache semantics.
- Docs and traceability now describe platform limitations instead of hiding them.

## Validation

The following checks passed locally:

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
cd docs && lunet build
git diff --check
```

Test results:

- Core tests: 35 passed.
- Framework parity tests: 5 passed.
- Avalonia headless tests: 11 passed.

## Notes

- Windows runtime validation is still needed for the WinUI package even though the project builds on macOS.
- Uno renderer-specific behavior still needs platform validation before claiming complete runtime parity.
- `plan/GALLERY_UNIFICATION_PLAN.md` is an unrelated local note and is intentionally left out of this PR.
