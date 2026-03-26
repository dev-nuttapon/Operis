# requirements frontend module

Purpose:

* owns Phase 3 requirement register, detail, baseline, and traceability matrix screens

Public surface:

* `index.ts`

Dependencies:

* `users/index.ts` for project lookups
* `shared/`

Notes:

* pages stay thin and delegate data access through hooks and API functions
* traceability matrix remains read-only while link changes are driven from requirement detail
