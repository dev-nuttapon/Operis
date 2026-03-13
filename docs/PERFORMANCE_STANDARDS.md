# PERFORMANCE_STANDARDS.md

Performance standards for this repository.

This document defines the minimum performance discipline that new code must follow.

---

# Purpose

Use these standards to:

* keep module growth from silently degrading performance
* make optimizations measurable
* keep extraction-ready code from becoming operationally expensive

---

# General Rules

1. optimize only when the path is identifiable
2. every optimization must have a measurable reason
3. do not add speculative caching
4. do not add speculative indexes
5. do not add risky code splitting without evidence
6. keep performance ownership inside the owning module

---

# Frontend Standards

## Required

* pages must stay thin
* heavy screens should split sections or modal workflows when justified
* route-level lazy loading should be used for large screens
* public pages should avoid unnecessary heavy UI dependencies
* bundle changes must be checked with:
  * `npm run build:local`
  * `npm run perf:bundle-report`
  * `npm run perf:bundle-budget`

## Budget Baseline

Current enforced bundle budgets:

* `antd-core-vendor <= 1050 kB`
* `react-vendor <= 240 kB`
* `index <= 90 kB`
* `AdminUsersPage <= 26 kB`
* `AuditLogsPage <= 10 kB`
* `DocumentDashboardPage <= 6 kB`
* `WorkflowDefinitionsPage <= 6 kB`
* `PublicRegistrationPage <= 8 kB`
* `InvitationAcceptPage <= 8 kB`
* `RegistrationPasswordSetupPage <= 7 kB`

## Do Not Regress

* do not eagerly import modal-heavy code into initial screen render
* do not export internal-only UI through module public entries
* do not introduce large UI dependencies without checking bundle impact

---

# Backend Standards

## Required

* query filtering, sorting, and paging stay in owner query services
* use provider-friendly query patterns where available
* avoid unbounded reads for list endpoints
* external calls inside loops must use controlled concurrency or be justified
* composition layers must stay free of persistence orchestration

## Current Baseline

* user identity enrichment uses controlled concurrency
* text search paths use `ILIKE` instead of `ToLower().Contains()`
* document listing is bounded to latest `50`
* audit log filtering and projection stay inside a single query service

## Do Not Regress

* do not move query logic back into `*Module.cs`
* do not add per-item database lookups inside loops
* do not remove paging from list paths

---

# Validation

Frontend:

* `npm run build:local`
* `npm run perf:bundle-report`
* `npm run perf:bundle-budget`

Backend:

* `dotnet build apps/backend/Operis_API/Operis_API.csproj`
* `dotnet test apps/backend/Operis_API.Tests/Operis_API.Tests.csproj`

Architecture:

* `npm run check:architecture`
* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`

---

# Decision Rule

If a change does not improve:

* a real hotspot
* a real regression
* or a real user-facing path

prefer simplicity over optimization.
