# SliverWidgets Constitution

## Principles

1. Core purity: `SliverWidgets.Core` has no UI framework dependency.
2. Native integration: framework adapters use native layout, scrolling, accessibility, and styling concepts.
3. Performance first: item realization is range-based and cache-aware.
4. Traceability: every feature maps requirement IDs to tests, docs, and package artifacts.
5. Honest portability: platform gaps are documented and tested before parity claims.

## Gates

- A feature needs a spec, plan, tasks, and traceability before implementation.
- Public APIs need docs and tests.
- Adapter behavior must be checked against the core layout results.
- Release candidates must build, test, pack, and build docs.
