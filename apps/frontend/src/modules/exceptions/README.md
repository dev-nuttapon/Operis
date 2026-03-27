# exceptions frontend module

Purpose:

* owns Phase 32 process waiver register and waiver detail UI

Public surface:

* `index.ts`

Dependencies:

* `users/public` for project option lookups
* `shared/`

Notes:

* pages stay thin and follow `Page -> Hook -> API -> HTTP client`
* workflow transitions stay server-driven
