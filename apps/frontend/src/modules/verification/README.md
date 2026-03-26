# verification frontend module

Purpose:

* owns Phase 7 test plan, test case and execution, and UAT sign-off screens

Public surface:

* `index.ts`

Dependencies:

* `users/index.ts` for project lookups
* `requirements/index.ts` for requirement selectors
* `shared/`

Notes:

* pages stay thin and delegate data access through hooks and API functions
* sensitive execution evidence is visible only when the user has `verification.evidence_sensitive.read`
