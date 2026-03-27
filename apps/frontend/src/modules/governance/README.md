# governance frontend module

Purpose:

* owns Phase 1 governance screens for process assets, QA checklists, project plans, stakeholders, and tailoring records
* owns Phase 13 governance control screens for RACI maps, approval evidence, workflow override logs, SLA rules, and retention policies

Public surface:

* `index.ts`

Internal only:

* page-local modal state
* screen-specific Ant Design table and form composition

Dependencies:

* `users/public.ts` for project list lookups
* `shared/`

Notes:

* pages stay thin and delegate to hooks and API functions
* governance workflow actions must stay behind permission-aware UI controls
