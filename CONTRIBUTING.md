# Contributing

Use the spec-driven workflow in `AGENTS.md`.

Before submitting changes:

```bash
dotnet build SliverWidgets.slnx
dotnet test SliverWidgets.slnx
dotnet pack SliverWidgets.slnx -c Release -o artifacts/packages
```

Update specs, tests, docs, samples, and `CHANGELOG.md` for public behavior changes.
