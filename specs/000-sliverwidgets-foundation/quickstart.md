# Quickstart

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

Use `SliverWidgets.Core` directly for framework-neutral layout tests and use the adapter package that matches the UI framework.
