# learning frontend module

Purpose:

* owns Phase 28 training catalog, role training matrix, completion tracking, and competency review UI

Public surface:

* `index.ts`

Dependencies:

* `users/public` for project lookups
* `shared/`

Notes:

* pages stay thin and follow `Page -> Hook -> API -> HTTP client`
* role options are queried through `Learning` APIs so the module does not import `Users` internals
