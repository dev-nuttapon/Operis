# governance frontend module

Purpose:

* owns Phase 1 governance screens for process assets, QA checklists, project plans, stakeholders, and tailoring records
* owns Phase 13 governance control screens for RACI maps, approval evidence, workflow override logs, SLA rules, and retention policies
* owns Phase 20 architecture governance screens for architecture records, design reviews, and integration reviews
* owns Phase 25 compliance dashboard screens for readiness summary, process-area drilldown, and saved dashboard preferences
* owns Phase 27 management review screens for cadence tracking, follow-up actions, and close approval
* owns Phase 29 policy screens for policy register, acknowledgement campaigns, and user attestations

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
* the compliance dashboard may read project list lookups from `users/public.ts` but keeps all dashboard-specific state and API wiring inside this module
* management review list/detail pages keep form composition local while using governance hooks and API functions for persistence
* policy pages must keep acknowledge flow inside governance hooks and API functions and must not call the HTTP client directly from page components
