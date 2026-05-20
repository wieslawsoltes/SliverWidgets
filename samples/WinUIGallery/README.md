# SliverWidgets WinUI Gallery

WinUI gallery for SliverWidgets.

## Build

Build from any configured host:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj
```

Run and validate the app on Windows with Windows App SDK support. Non-Windows builds disable PRI generation for this code-only sample so compilation can still be checked.

## Included Demos

- Fixed extent `ItemsRepeater` list using `SliverFixedExtentVirtualizingLayout`.
- Grid `ItemsRepeater` using `SliverGridVirtualizingLayout`.
- 100,000 item source with realized element counts and jump controls.
- Pinned and floating header concepts over virtualized content.
