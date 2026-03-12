# NEW_MODULE_CHECKLIST.md

Checklist for adding a new module.

---

# Before Coding

1. define the owning domain
2. define frontend public surface
3. define backend public surface
4. define owned data
5. define expected critical paths

---

# Frontend Checklist

1. module folder exists under `apps/frontend/src/modules`
2. `index.ts` exists
3. pages follow `Page -> Hook -> API -> HTTP Client`
4. module does not import internals from other modules
5. module-specific components stay local
6. tests exist for important flows when behavior is non-trivial

---

# Backend Checklist

1. module folder exists under `apps/backend/Operis_API/Modules`
2. `Application/` exists
3. `Contracts/` exists
4. `*Module.cs` stays thin
5. inline endpoint lambdas are not used
6. persistence ownership is explicit

---

# Performance Checklist

1. query paths are easy to identify
2. search and sorting logic are provider-friendly
3. external calls in loops are reviewed for batching or controlled concurrency
4. no speculative optimization was added without reason

---

# Validation

Frontend:

* `npm run check:architecture`
* `npm test`
* `npm run build:local`
* `npm run perf:bundle-report`

Backend:

* `node scripts/check-backend-architecture.mjs`
* `dotnet build apps/backend/Operis_API/Operis_API.csproj`
