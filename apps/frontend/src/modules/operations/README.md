# operations frontend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependency register, and configuration audit UI
* owns Phase 16 supplier register and supplier agreement evidence UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`

Notes:

* pages stay thin and use `Page -> Hook -> API -> HTTP client`
* external dependencies trace to supplier ownership and governing agreements through the same module surface
