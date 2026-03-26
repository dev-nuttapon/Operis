# risks frontend module

Purpose:

* owns Phase 5 risk register, issue log, and issue action screens

Public surface:

* `index.ts`

Dependencies:

* `users/index.ts` for project lookups
* `shared/`

Notes:

* pages stay thin and delegate data access through hooks and API functions
* sensitive issues are only creatable and visible to users with `risks.sensitive.read`
