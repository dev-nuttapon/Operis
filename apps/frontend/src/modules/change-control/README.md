# change-control frontend module

Purpose:

* owns Phase 4 change request, configuration item, and baseline registry screens

Public surface:

* `index.ts`

Dependencies:

* `users/index.ts` for project lookups
* `shared/`

Notes:

* pages stay thin and delegate data access through hooks and API functions
* baseline registry UI enforces approved change-request linkage and explicit emergency override rationale
