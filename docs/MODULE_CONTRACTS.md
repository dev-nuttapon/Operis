# MODULE_CONTRACTS.md

Public contract map for current modules.

Use this document to keep extraction boundaries explicit while the codebase is still small.

---

# Frontend Public Surfaces

## auth

Public entry:

* [apps/frontend/src/modules/auth/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/auth/index.ts)

Notes:

* app shell should import auth capabilities only through this surface
* auth is cross-cutting and should stay thin at the composition layer

## users

Public entries:

* [apps/frontend/src/modules/users/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/index.ts)
* [apps/frontend/src/modules/users/public.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/users/public.ts)

Notes:

* route-level composition should prefer `public.ts` where bundle boundaries matter
* pages should keep using `Page -> Hook -> API -> HTTP client`

## documents

Public entry:

* [apps/frontend/src/modules/documents/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/documents/index.ts)

## audits

Public entry:

* [apps/frontend/src/modules/audits/index.ts](/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/audits/index.ts)

---

# Backend Public Surfaces

## users

Module entry:

* [apps/backend/Operis_API/Modules/Users/UsersModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Users/UsersModule.cs)

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

Application surfaces:

* audit log queries

## documents

Module entry:

* [apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/Documents/DocumentsModule.cs)

Application surfaces:

* document queries

---

# Enforcement

Frontend:

* `npm run check:architecture`

Backend:

* `node scripts/check-backend-architecture.mjs`

CI:

* [/.github/workflows/frontend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/frontend-quality.yml)
* [/.github/workflows/backend-quality.yml](/Users/nuttapon/Github-dev/Operis/.github/workflows/backend-quality.yml)
