# defects frontend module

Purpose:

* owns Phase 15 defect and non-conformance log/detail screens

Public surface:

* `index.ts`

Dependencies:

* `users/` for project option lookups
* `shared/`

Notes:

* pages stay thin and follow `Page -> Hook -> API -> HTTP client`
* resolve and close actions stay behind permission-aware UI controls
