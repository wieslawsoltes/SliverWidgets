---
title: API Guide
description: Human-written API guide for SliverWidgets packages.
---

# API Guide

This guide explains the public API by package and intended usage. The default site keeps API documentation as human-written pages so the docs build is portable across local paths and CI hosts.

The repository also includes `apidocs/` enrichment files beside the source projects. Enable Lunet `api.dotnet` extraction with `-d generate_dotnet_api=true` in environments where project paths can be passed safely to MSBuild.

## Package Pages

- [Core API](core.html)
- [Avalonia API](avalonia.html)
- [MAUI API](maui.html)
- [Uno API](uno.html)
- [WinUI API](winui.html)

## Stability Notes

SliverWidgets is currently versioned as `0.1.0`. The core protocol is intentionally small, but API names may still evolve before a stable release. Prefer coding against the documented package entry points instead of internal helpers.

## Namespace Map

| Namespace | Package |
|---|---|
| `SliverWidgets.Core` | `SliverWidgets.Core` |
| `SliverWidgets.Avalonia` | `SliverWidgets.Avalonia` |
| `SliverWidgets.Maui` | `SliverWidgets.Maui` |
| `SliverWidgets.Uno` | `SliverWidgets.Uno` |
| `SliverWidgets.WinUI` | `SliverWidgets.WinUI` |

## Cross-Package Rule

Adapter packages depend on `SliverWidgets.Core`. Application code can use adapter controls directly without calling the core, but custom adapters, diagnostics, and tests should use the core API.
