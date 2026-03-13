# NEW_MODULE_CHECKLIST.md

Checklist for adding a new module.

Use this as the short operational version of:

* `docs/MODULE_TEMPLATE.md`
* `docs/MODULE_CONTRACTS.md`
* `docs/DATA_OWNERSHIP.md`
* `docs/PERFORMANCE_HOTSPOTS.md`
* `docs/PERFORMANCE_STANDARDS.md`

---

# Before Coding

1. define the owning domain
2. define frontend public surface
3. define backend public surface
4. define owned data
5. define expected critical paths
6. create the module `README.md` manifest
7. update `docs/MODULE_CONTRACTS.md`
8. update `docs/DATA_OWNERSHIP.md` if new data is introduced

---

# Frontend Checklist

1. module folder exists under `apps/frontend/src/modules`
2. required directories exist:
   `api/`, `hooks/`, `pages/`, `types/`
3. `index.ts` or `public.ts` exists
4. `README.md` exists with:
   `Purpose`, `Public surface`, `Dependencies`, `Notes`
5. pages follow `Page -> Hook -> API -> HTTP Client`
6. module does not import internals from other modules
7. module-specific components stay local
8. internal-only components are not exported from the public entry
9. heavy screens use lazy boundaries when justified
10. tests exist for important flows when behavior is non-trivial

---

# Backend Checklist

1. module folder exists under `apps/backend/Operis_API/Modules`
2. `Application/` exists
3. `Contracts/` exists
4. `README.md` exists with:
   `Purpose`, `Public surface`, `Owned data`, `Notes`
5. `*Module.cs` stays thin
6. inline endpoint lambdas are not used
7. `*Module.cs` does not depend directly on:
   `OperisDbContext`, `DbContext`, `IAuditLogWriter`, `IKeycloakAdminClient`, `IReferenceDataCache`
8. `*Module.cs` does not call `SaveChangesAsync`
9. persistence ownership is explicit
10. application services own query and command orchestration

---

# Performance Checklist

1. query paths are easy to identify
2. search and sorting logic are provider-friendly
3. external calls in loops are reviewed for batching or controlled concurrency
4. frontend-heavy screens do not eagerly import code that can be lazy-loaded
5. no speculative optimization was added without reason
6. if bundle impact is relevant, check before/after output from the build

---

# Tests Checklist

1. add at least one targeted test for a key module flow
2. backend modules should have application-level tests for non-trivial logic
3. add handler/composition tests when endpoint mapping behavior is important
4. performance-sensitive paths should be covered where avoidable extra I/O is a risk

---

# Validation

Frontend:

* `npm run check:architecture`
* `npm test`
* `npm run build:local`
* `npm run perf:bundle-report`
* `npm run perf:bundle-budget`

Backend:

* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`
* `dotnet build apps/backend/Operis_API/Operis_API.csproj`
* `dotnet test apps/backend/Operis_API.Tests/Operis_API.Tests.csproj`

---

# Exit Rule

The module is ready to merge only when:

1. public surface is explicit
2. module manifest is present and complete
3. ownership is documented
4. checks pass
5. no obvious boundary or performance regression is introduced
