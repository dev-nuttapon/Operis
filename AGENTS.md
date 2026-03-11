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
