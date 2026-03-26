# operations frontend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependency register, and configuration audit UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`

Notes:

* pages stay thin and use `Page -> Hook -> API -> HTTP client`
