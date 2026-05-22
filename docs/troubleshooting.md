---
title: Troubleshooting
description: Common SliverWidgets build, layout, and documentation issues.
---

# Troubleshooting

## The WinUI Sample Builds but Does Not Run on macOS or Linux

WinUI targets Windows App SDK. The project compiles on non-Windows hosts with PRI generation disabled, but run validation belongs on Windows:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj -c Release
```

Non-Windows hosts are not the source of truth for Windows App SDK runtime behavior.

## MAUI Workload Restore Fails

MAUI sample heads require platform workloads. If workload restore fails because of local SDK manifest permissions, fix the local SDK installation or run on a clean environment. The CI gallery solution avoids MAUI workload-dependent restore paths.

## A List Measures Too Many Children

Check whether you are using a non-virtual panel. For large Avalonia item sources, use `SliverVirtualizingStackPanel` or `SliverVirtualizingListPanel`. For MAUI, use `SliverCollectionView`. For Uno/WinUI, use `ItemsRepeater` with a sliver virtualizing layout.

## Rows Jump During Variable-Height Scrolling

Variable-height layouts estimate unobserved item extents. Improve behavior by:

- setting a realistic `EstimatedItemExtent`
- observing measured row heights promptly
- keeping row templates stable after first measure
- using fixed extents where the design allows

## Grid Tiles Have Unexpected Sizes

Check:

- `SizingMode`
- `CrossAxisCount`
- `MaxCrossAxisExtent`
- `ChildAspectRatio`
- optional `MainAxisExtent`
- cross-axis spacing

Remember that vertical grids use width as cross-axis extent, while horizontal grids use height.

## Lunet API Extraction Fails

The default docs site uses manual API pages. Generated API extraction is opt-in:

```bash
lunet -d generate_dotnet_api=true build
```

If that command fails:

1. Run `dotnet build SliverWidgets.slnx -c Release`.
2. Confirm .NET 10 SDK is installed.
3. Confirm package restore works.
4. Run `cd docs && lunet --stacktrace -d generate_dotnet_api=true build`.
5. Check for whitespace-sensitive project path issues in the local workspace path.

WinUI generated API is intentionally omitted from portable extraction. Use the manual [WinUI API guide](api/winui.html) and Windows validation lane.
