# meetings frontend module

Purpose:

* owns Phase 6 MOM register, meeting detail, and decision log screens

Public surface:

* `index.ts`

Dependencies:

* `users/index.ts` for project lookups
* `shared/`

Notes:

* pages stay thin and delegate data access through hooks and API functions
* restricted meetings and decisions are only visible to users with `meetings.restricted.read`
