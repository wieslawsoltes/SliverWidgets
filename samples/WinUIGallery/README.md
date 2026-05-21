# SliverWidgets WinUI Gallery

WinUI gallery for SliverWidgets.

The shell matches the Avalonia gallery: top metric cards, short scenario tabs
(`Fixed`, `Variable`, `Grid`, `Wrap`, `Header`, `Tabs`, `Mixed`, `Sections`, `Fill`, `Cache`),
left-side controls/notes, and a right-side WinUI-native viewport.

## Build

Build from any configured host:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj
```

Run and validate the app on Windows with Windows App SDK support. Non-Windows builds disable PRI generation for this code-only sample so compilation can still be checked.

## Included Demos

- Fixed large list `ItemsRepeater` virtualization using `SliverFixedExtentVirtualizingLayout`.
- Variable/non-uniform rows using native WinUI `StackLayout` virtualization until a sliver variable-extent adapter is available.
- Adaptive max-cross-axis grid virtualization using `SliverGridVirtualizingLayout`.
- Variable-size wrap virtualization using `SliverWrapVirtualizingLayout` over 100,000 deterministic chips.
- Pinned/collapsible header, tabbed nested-scroll projection, mixed composition, sectioned sticky-header, fill/padding/visibility, and cache stress scenarios.
- 100,000 item source with realized element counts, cache controls, and jump controls.
