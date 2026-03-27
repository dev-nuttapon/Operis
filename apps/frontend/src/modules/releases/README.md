# releases frontend module

Purpose:

* owns Phase 14 release register, deployment checklist, and release notes screens

Public surface:

* `index.ts`

Dependencies:

* `users/` for project option lookups
* `shared/`

Notes:

* pages stay thin and follow `Page -> Hook -> API -> HTTP client`
* release execution and release-note publication stay behind permission-aware UI controls
