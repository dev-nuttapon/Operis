# MODULE_READINESS.md

Module readiness checklist for preparing future extraction without splitting services, repositories, or databases today.

---

# Purpose

This document defines what "migration-ready" means for this repository.

Use it to:

* assess a module before refactoring
* decide what to improve next
* prevent regressions while staying in a modular monolith

---

# Readiness Scale

`9-10/10`

* ready for internal package extraction with low friction
* contracts, ownership, and dependencies are clear

`7-8/10`

* strong module boundary
* some app-shell or shared coupling still exists

`5-6/10`

* module shape exists
* extraction still requires structural cleanup

`0-4/10`

* boundary is weak
* not suitable for extraction planning yet

---

# Definition Of Ready

A module is considered migration-ready when:

* it has a clear public API
* it does not import internal files from other modules
* page or controller layers stay thin
* business logic is owned by the module
* owned data is identifiable
* backend and frontend boundaries are coherent
* module-level testing is possible
* app shell composes the module without knowing its internals

---

# Frontend Checklist

Checklist items:

* public entry exists: `index.ts` or `public.ts`
* pages follow `Page -> Hook -> API -> HTTP Client`
* internal paths are not imported across modules
* module-specific components stay inside the module
* `shared/` contains only real cross-module concerns
* route composition is thin
* heavy screens are split into sections or workflows where appropriate
* key flows have targeted tests

Scoring guidance:

* add `1` point for each item that is clearly satisfied
* subtract `0.5-1` for unstable or partially enforced boundaries

---

# Backend Checklist

Checklist items:

* module folder exists under `apps/backend/Operis_API/Modules`
* contracts are separated from infrastructure
* domain types exist where business concepts are defined
* infrastructure types are owned by the module
* application/use-case layer exists or has a clear equivalent
* data ownership is identifiable
* cross-module persistence access is minimized
* module registration is explicit

Scoring guidance:

* add `1` point for each item that is clearly satisfied
* cap the score if the module still mixes orchestration and persistence heavily

---

# Project Snapshot

Current module status based on the repository state on `2026-03-12`:

| Module | Frontend | Backend | Overall | Notes |
| --- | --- | --- | --- | --- |
| `users` | `8/10` | `5.5/10` | `7/10` | Frontend boundary is the strongest in the repo. Backend still mixes module registration and infrastructure concerns. |
| `auth` | `7/10` | `n/a` | `7/10` | Public API is clearer now, but this is still a cross-cutting module with high app-shell coupling. |
| `audits` | `6.5/10` | `5/10` | `6/10` | Domain boundary is promising, but both frontend and backend are still comparatively thin and under-structured. |
| `documents` | `6/10` | `4.5/10` | `5.5/10` | Public entry now exists, but the module is still early-stage and lacks mature internal layers. |

---

# Current Priorities

Priority 1: backend boundary cleanup

* add explicit application/use-case layers for `Users` and `Audits`
* reduce orchestration inside `*Module.cs`
* make data ownership and dependency direction clearer

Priority 2: CI quality gates

* keep `check:architecture` mandatory
* keep frontend build and tests mandatory
* keep bundle reporting visible in CI output

Priority 3: performance readiness

* continue measuring bundle size through `npm run perf:bundle-report`
* optimize only when a before/after change is measurable
* prefer reducing dependencies over risky manual chunk splits

---

# Extraction Notes By Module

## users

Strengths:

* strongest frontend separation in the repo
* admin screen is split into sections, workflows, and lazy modal boundaries
* public flows follow module hooks and APIs

Needs before future extraction:

* backend application layer
* clearer data ownership notes
* more complete module-level tests

## auth

Strengths:

* public surface is clearer
* app-level consumers mostly use the module API

Needs before future extraction:

* clearer separation between auth contract and shared infra
* explicit ownership boundaries around token/session concerns

## audits

Strengths:

* domain concept is inherently bounded
* low surface area compared with `users`

Needs before future extraction:

* backend layering beyond contracts
* stronger frontend module shape if the UI expands

## documents

Strengths:

* module boundary exists
* public entry exists

Needs before future extraction:

* mature `api/` and `hooks/`
* stronger separation between page, form, and data flow
* backend module layering

---

# Review Process

Before a module is marked as "ready to move later", verify:

1. no cross-module internal imports exist
2. public contracts are documented
3. tests cover key module workflows
4. bundle impact is known if the module is frontend-heavy
5. backend ownership is documented if the module touches persistence

---

# Related Checks

Frontend architecture:

* `npm run check:architecture`

Frontend performance:

* `npm run perf:bundle-report`

Frontend quality baseline:

* `.github/workflows/frontend-quality.yml`
