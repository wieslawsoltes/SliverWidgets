---
title: Spec-Driven Development
description: How SliverWidgets uses requirements, implementation, tests, docs, and traceability.
---

# Spec-Driven Development

SliverWidgets follows a spec-driven development loop: define requirements, implement against those requirements, verify with tests and samples, and keep documentation aligned with the implementation.

## Source Artifacts

| Artifact | Purpose |
|---|---|
| `plan/SPEC.md` | Functional and non-functional requirements. |
| `plan/PLAN.md` | Implementation plan, package boundaries, milestones, validation matrix, and risks. |
| `AGENTS.md` | Agent instructions and project requirements. |
| `tests/` | Unit and parity tests that prove core behavior. |
| `samples/` | Gallery apps that exercise framework adapters. |
| `docs/` | Lunet documentation site and API guide. |

## Workflow

1. Update the spec when a behavior or platform promise changes.
2. Implement the smallest core behavior that captures the rule.
3. Add adapter code only after the core contract is clear.
4. Add tests for geometry and realization windows.
5. Add or update a sample when the behavior is user-visible.
6. Update documentation and generated API summaries.
7. Run the validation matrix.

## Traceability

Every major feature should be traceable from requirement to implementation:

| Requirement area | Implementation | Verification | Documentation |
|---|---|---|---|
| Fixed lists | `SliverFixedExtentListLayout` | core tests, parity tests | [Sliver Lists](../controls/sliver-list.html) |
| Grids | `SliverGridLayout` | core tests, galleries | [Sliver Grids](../controls/sliver-grid.html) |
| Headers | persistent header layouts | core tests, galleries | [Persistent Headers](../controls/persistent-header.html) |
| Adapters | framework packages | parity tests, gallery builds | [Framework Adapters](../framework-adapters/) |
| Packaging | project metadata and workflows | pack workflow | [Packaging](../packaging.html) |

## Definition of Done

A SliverWidgets feature is done when:

- the spec describes expected behavior
- the core or adapter implementation exists
- tests cover the behavior or a platform limitation is documented
- sample coverage exists for user-facing framework behavior
- docs explain when to use it and where the limits are
- release commands still pass
