# SliverWidgets

SliverWidgets is a high-performance sliver layout system for .NET UI frameworks. It brings Flutter-inspired sliver concepts to Avalonia, Uno, MAUI, and WinUI while preserving each framework's native controls, styling, input, accessibility, and packaging model.

## Packages

- `SliverWidgets.Core`: framework-neutral sliver constraints, geometry, layout algorithms, and viewport composition.
- `SliverWidgets.Avalonia`: Avalonia panels, persistent headers, and virtualizing panels backed by core sliver layout.
- `SliverWidgets.Maui`: MAUI `Layout` + `ILayoutManager` adapter and native-backed `SliverCollectionView`.
- `SliverWidgets.Uno`: Uno/WinUI-style row and grid `VirtualizingLayout` for `ItemsRepeater`.
- `SliverWidgets.WinUI`: Windows App SDK row and grid `VirtualizingLayout` for `ItemsRepeater`.

## Quick Start

```csharp
using SliverWidgets.Core;

var viewport = new SliverViewport(720, 1024);
var engine = new SliverViewportLayoutEngine();
var result = engine.Layout(
    new ISliverLayout[]
    {
        new SliverPersistentHeaderLayout(new SliverPersistentHeaderOptions(48, 144, Pinned: true)),
        new SliverFixedExtentListLayout(new SliverFixedExtentListOptions(10_000, 36, 4))
    },
    viewport,
    scrollOffset: 240,
    cacheExtent: 360);
```

## Avalonia

```xml
<slivers:SliverStackPanel
    xmlns:slivers="using:SliverWidgets.Avalonia"
    ItemExtent="44"
    Spacing="2"
    CacheExtent="400">
  <TextBlock Text="Row 1" />
  <TextBlock Text="Row 2" />
</slivers:SliverStackPanel>
```

For an `ItemsControl`, use `SliverVirtualizingStackPanel` as the items panel to realize only the core paint/cache range.
Use `SliverVirtualizingListPanel` when row heights vary and the panel should cache observed extents.

## WinUI / Uno

Use `SliverFixedExtentVirtualizingLayout` as an `ItemsRepeater.Layout`.
Use `SliverGridVirtualizingLayout` for fixed-count or max-cross-axis-extent grids.

```xml
<ItemsRepeater ItemsSource="{x:Bind Items}">
  <ItemsRepeater.Layout>
    <slivers:SliverFixedExtentVirtualizingLayout ItemExtent="44" Spacing="2" />
  </ItemsRepeater.Layout>
</ItemsRepeater>
```

## MAUI

```xml
<slivers:SliverStackLayout
    xmlns:slivers="clr-namespace:SliverWidgets.Maui;assembly=SliverWidgets.Maui"
    ItemExtent="44"
    Spacing="2" />
```

## Build

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

WinUI projects compile on non-Windows hosts with PRI generation disabled. Runtime validation and full Windows App SDK validation still belong on Windows.

## Sample Galleries

The repository includes gallery-style samples that mirror Flutter sliver examples with native controls and shared deterministic data:

- `samples/AvaloniaGallery`: desktop app showing `SliverStackPanel`, `SliverGridPanel`, `SliverPersistentHeader`, `SliverVirtualizingStackPanel`, and mixed viewport composition.
- `samples/MauiGallery`: Mac Catalyst MAUI app showing `SliverStackLayout`, native-backed `SliverCollectionView`, grid mode, section headers, and live controls.
- `samples/UnoGallery`: reusable Uno/WinUI-style page showing `ItemsRepeater` fixed list/grid virtualization and 100,000 item realization counters.
- `samples/UnoGalleryApp`: Uno desktop app host for the reusable gallery page.
- `samples/WinUIGallery`: WinUI app showing `ItemsRepeater` fixed list/grid virtualization and pinned/floating header concepts.

Build the macOS-supported galleries with:

```bash
dotnet build SliverWidgets.Galleries.slnx
```

CI can build the no-workload gallery subset with:

```bash
dotnet build SliverWidgets.Galleries.CI.slnx
```

Build the WinUI gallery:

```bash
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj
```

Run and validate the WinUI gallery on Windows.

## Documentation

The Lunet documentation site lives in `docs/`.

```bash
cd docs
lunet build
```

The site includes concepts, architecture, control guides, framework adapter guides, sample documentation, testing and packaging instructions, troubleshooting, migration notes from Flutter slivers, a manual API guide, and Lunet-generated API reference pages.
