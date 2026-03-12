# AGENTS.md

AI development rules for this repository.

This document defines how AI assistants should generate and modify code in this project.

---

# Project Overview

This project is an **Enterprise Platform** starting with:

* Paperless Document System
* Workflow & Approval
* Audit & Compliance

Future expansion:

* ISO / CMMI Compliance
* Asset Management
* Operations Platform

---

# Technology Stack

Frontend

* React
* TypeScript
* Ant Design
* TanStack Query
* React Hook Form
* Zod

Backend

* ASP.NET Core
* C#
* Entity Framework Core

Infrastructure

* PostgreSQL
* Redis
* MinIO
* Keycloak
* Traefik

Observability

* Prometheus
* Loki
* Tempo
* Grafana

Deployment

* Docker Compose

---

# Architecture Style

The system follows **Modular Monolith Architecture**

Rules:

* Single frontend
* Single backend
* Modules separated by domain

---

# Frontend Architecture

Root structure

```
src/
  app/
  modules/
  shared/
  providers/
  routes/
  types/
```

Each feature must be implemented as a **module**

Example

```
modules/
  documents/
  workflows/
  approvals/
  audits/
```

---

# API Access Pattern

Pages must NOT call API directly.

Correct pattern

```
Page
 → Hook
 → API Function
 → HTTP Client
 → Backend
```

---

# State Management

Server state

* Must use TanStack Query

UI state

* useState
* local component state

---

# Shared Code

Shared code must only exist if used by multiple modules.

Examples

```
shared/components/AppTable
shared/hooks/useDebounce
shared/utils/date
```

---

# Module Boundaries

Modules must not import internal files from other modules.

Correct usage:

```
import from module public API
```

Example

```
modules/documents/index.ts
```

---

# New Module Rules

Every new module must follow these rules.

Frontend minimum structure

```
modules/<module-name>/
  pages/
  hooks/
  api/
  components/
  types/
  index.ts
```

Use `public.ts` in addition to `index.ts` when route-level or bundle-level public surface must stay smaller than the full module export.

Backend minimum structure

```
Modules/<ModuleName>/
  Application/
  Contracts/
  <ModuleName>Module.cs
```

Add these folders when needed:

* `Domain/` for business concepts and domain rules
* `Infrastructure/` for persistence and external integrations

Rules

* `*Module.cs` is composition only
* endpoints must delegate to `Application/`
* API request and response contracts belong in `Contracts/`
* persistence entities and external clients belong in `Infrastructure/`
* frontend pages must stay thin and follow `Page -> Hook -> API -> HTTP Client`

---

# Module Ownership Rules

Each table, workflow, and external integration must have a single owning module.

Rules

* every table must have exactly one owning module
* non-owning modules must not write another module's tables directly
* cross-module reads should prefer application/query contracts over direct persistence access
* shared reference data must still have an owner
* auth and audit concerns may be cross-cutting, but their storage and orchestration ownership must still be explicit

---

# Backend Endpoint Rules

Backend endpoints must not accumulate business logic over time.

Rules

* endpoint handlers may validate HTTP-level concerns and map results to HTTP responses
* endpoint handlers must not become the main place for query orchestration
* endpoint handlers must not become the main place for persistence orchestration
* inline endpoint lambdas are not allowed for module features
* query and command flows must be moved into `Application/` as the default approach

---

# Performance Rules

Performance changes must be deliberate and measurable.

Rules

* optimize only when the before and after state can be explained
* do not add caching, chunk splitting, or concurrency without a concrete reason
* avoid query patterns that force unnecessary transformations on database columns when a provider-native function exists
* loops that call external systems per item must consider batching or controlled concurrency
* route, screen, and modal boundaries should support lazy loading when the UI becomes heavy
* do not trade maintainability for speculative optimization

---

# Quality Gate Rules

Frontend changes must keep these checks passing:

* `npm run check:architecture`
* `npm test`
* `npm run build:local`
* `npm run perf:bundle-report`

Backend changes must keep these checks passing:

* `node scripts/check-backend-architecture.mjs`
* `dotnet build apps/backend/Operis_API/Operis_API.csproj`

AI should prefer adding or updating guardrails when a rule is important enough to preserve across future modules.

---

# Preferred Review Checklist

When adding or refactoring a module, verify:

1. public surface exists and is explicit
2. page or endpoint layer is thin
3. business logic is owned by the module
4. cross-module access goes through public contracts
5. data ownership is clear
6. performance-sensitive paths are identified
7. required quality gates still pass

---

# Naming Convention

Pages

```
DocumentListPage.tsx
```

Components

```
DocumentTable.tsx
```

Hooks

```
useDocuments.ts
```

Schemas

```
documentForm.schema.ts
```

---

# File Upload Flow

```
Frontend → Backend → MinIO
```

Frontend must not upload directly to storage.

---

# Refactoring Rules

AI must ensure

* No circular imports
* Module boundaries preserved
* Shared code minimized
* Code readability maintained

---
