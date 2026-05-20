---
title: Testing
description: Unit, parity, gallery, package, and docs validation.
---

# Testing

## Default Validation

```bash
dotnet test SliverWidgets.slnx
```

The default test suite covers:

- core geometry validation
- fixed and variable list realization
- variable extent cache behavior
- grid sizing and slot placement
- pinned, floating, and snapping header behavior
- padding, visibility, fill-remaining, and box adapter behavior
- mixed viewport composition
- framework parity for axis-neutral fixed realization windows

## Build Validation

```bash
dotnet build SliverWidgets.slnx -c Release
dotnet build SliverWidgets.Galleries.CI.slnx -c Release
```

Use the full gallery solution on hosts with the required platform workloads:

```bash
dotnet build SliverWidgets.Galleries.slnx -c Release
```

## Package Validation

```bash
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

Package validation is enabled for packable libraries. Sample projects opt out.

## Documentation Validation

```bash
cd docs
lunet build
```

The default documentation build uses the human-written API guide. Lunet `api.dotnet` extraction is configured as an opt-in path:

```bash
lunet -d generate_dotnet_api=true build
```

Use that command in environments where project paths can be passed safely to MSBuild.

## WinUI Validation

Build the WinUI package and sample:

```bash
dotnet build src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release
dotnet build samples/WinUIGallery/SliverWidgets.WinUIGallery.csproj -c Release
```

Non-Windows builds disable PRI generation for these code-only projects. Run and device validation still belong on Windows.
