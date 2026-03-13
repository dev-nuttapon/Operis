# MODULE_CONTRACTS.md

Public contract map for current modules.

Use this document to keep extraction boundaries explicit while the codebase is still small.

---

# Frontend Public Surfaces

## auth

Public entry:

* [apps/frontend/src/modules/auth/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/auth/index.ts)
* [apps/frontend/src/modules/auth/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/auth/README.md)

Notes:

* app shell should import auth capabilities only through this surface
* auth is cross-cutting and should stay thin at the composition layer

## users

Public entries:

* [apps/frontend/src/modules/users/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/index.ts)
* [apps/frontend/src/modules/users/public.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/public.ts)
* [apps/frontend/src/modules/users/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/README.md)

Notes:

* route-level composition should prefer `public.ts` where bundle boundaries matter
* pages should keep using `Page -> Hook -> API -> HTTP client`

## documents

Public entry:

* [apps/frontend/src/modules/documents/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/documents/index.ts)
* [apps/frontend/src/modules/documents/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/documents/README.md)

## audits

Public entry:

* [apps/frontend/src/modules/audits/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/audits/index.ts)
* [apps/frontend/src/modules/audits/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/audits/README.md)

## workflows

Public entry:

* [apps/frontend/src/modules/workflows/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/workflows/index.ts)
* [apps/frontend/src/modules/workflows/README.md](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/workflows/README.md)

---

# Backend Public Surfaces

## users

Module entry:

* [apps/backend/Operis_API/Modules/Users/UsersModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/UsersModule.cs)
* [apps/backend/Operis_API/Modules/Users/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/README.md)

Application surfaces:

* user queries and commands
* registration queries and commands
* invitation queries and commands
* reference data queries and commands

Rule:

* endpoints delegate to `Application/`

## audits

Module entry:

* [apps/backend/Operis_API/Modules/Audits/AuditsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Audits/AuditsModule.cs)
* [apps/backend/Operis_API/Modules/Audits/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Audits/README.md)

Application surfaces:

* audit log queries

## documents

Module entry:

* [apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs)
* [apps/backend/Operis_API/Modules/Documents/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/README.md)

Application surfaces:

* document queries

## workflows

Module entry:

* [apps/backend/Operis_API/Modules/Workflows/WorkflowsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Workflows/WorkflowsModule.cs)
* [apps/backend/Operis_API/Modules/Workflows/README.md](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Workflows/README.md)

Application surfaces:

* workflow definition queries
* workflow definition commands

---

# Enforcement

Frontend:

* `npm run check:architecture`
* `node scripts/check-module-contracts.mjs`

Backend:

* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`

CI:

* [/.github/workflows/frontend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/frontend-quality.yml)
* [/.github/workflows/backend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/backend-quality.yml)
