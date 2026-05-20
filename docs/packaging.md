---
title: Packaging
description: NuGet package layout, metadata, and release commands.
---

# Packaging

SliverWidgets ships as one core package and one package per UI framework. The packages share repository metadata, MIT license metadata, tags, deterministic build settings, package validation, and the root README as the NuGet package README.

## Packages

| Package | Target framework | Notes |
|---|---|---|
| `SliverWidgets.Core` | `net10.0` | Pure layout engine, no UI framework dependency. |
| `SliverWidgets.Avalonia` | `net10.0` | Depends on Avalonia 11.3.12. |
| `SliverWidgets.Maui` | `net10.0` | Depends on MAUI Controls 10.0.20. |
| `SliverWidgets.Uno` | `net10.0` | Depends on Uno.WinUI 6.5.237. |
| `SliverWidgets.WinUI` | `net10.0-windows10.0.19041.0` | Depends on Windows App SDK 1.8 and uses the Windows package lane. |

## Create Packages

```bash
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

The package workflow has a Windows lane for the WinUI package:

```bash
dotnet pack src/SliverWidgets.WinUI/SliverWidgets.WinUI.csproj -c Release -o artifacts/packages
```

## Metadata

Shared metadata is configured in `Directory.Build.props`:

- `PackageProjectUrl`
- `RepositoryUrl`
- `RepositoryType`
- `PackageLicenseExpression`
- `PackageTags`
- `PackageReadmeFile`
- `VersionPrefix`
- `ContinuousIntegrationBuild`
- `Deterministic`
- `EnablePackageValidation`
- `GenerateDocumentationFile`

## Release Checklist

1. Run `dotnet build SliverWidgets.slnx -c Release`.
2. Run `dotnet test SliverWidgets.slnx -c Release --no-build`.
3. Run `dotnet build SliverWidgets.Galleries.CI.slnx -c Release`.
4. Run `cd docs && lunet build`.
5. Pack with `dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages`.
6. Validate WinUI on Windows.
7. Publish packages from a signed release/tag workflow.
