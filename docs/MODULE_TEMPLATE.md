# MODULE_TEMPLATE.md

Template for adding a new module in this repository.

Use this template before writing code so the module starts with the correct boundary.

---

# Module Summary

Module name:

* `<module-name>`

Purpose:

* what business problem this module owns

Frontend owner surface:

* `apps/frontend/src/modules/<module-name>/index.ts`
* optional `public.ts`

Backend owner surface:

* `apps/backend/Operis_API/Modules/<ModuleName>/<ModuleName>Module.cs`

---

# Frontend Structure

Minimum structure:

```
apps/frontend/src/modules/<module-name>/
  api/
  hooks/
  pages/
  components/
  types/
  index.ts
```

Questions:

* what page composes the module
* what hooks own server state and workflow state
* what API functions call backend endpoints
* what types are public contracts versus local UI-only shapes

Rules:

* pages stay thin
* hooks own query and mutation orchestration
* API functions call the shared HTTP client
* cross-module imports use only public module surfaces

---

# Backend Structure

Minimum structure:

```
apps/backend/Operis_API/Modules/<ModuleName>/
  Application/
  Contracts/
  <ModuleName>Module.cs
```

Add when needed:

* `Domain/`
* `Infrastructure/`

Questions:

* what HTTP endpoints belong to this module
* what application services own query flows
* what application services own command flows
* what contracts cross the API boundary
* what persistence or external integrations belong to the module

Rules:

* `*Module.cs` is composition only
* endpoint handlers map HTTP to application results
* persistence and external system logic do not stay in endpoints

---

# Ownership

Owned data:

* list the tables or storage records owned by the module

Owned integrations:

* list external services coordinated by the module

Cross-module dependencies:

* list allowed read dependencies
* list forbidden write dependencies

---

# Performance Notes

Critical paths:

* list high-frequency queries
* list expensive external calls

Guardrails:

* where lazy loading is needed
* where caching is acceptable
* where concurrency or batching may matter

---

# Definition Of Done

Module is acceptable only if:

1. public surface is explicit
2. endpoint/page layer is thin
3. ownership is documented
4. quality gates pass
5. obvious performance-sensitive paths are called out
