# metrics frontend module

Purpose:

* owns Phase 9 metric definitions, dashboard, schedules, and quality gate UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`
* `users/` for project option lookup

Notes:

* pages stay thin and use `Page -> Hook -> API -> HTTP client`
* dashboard widgets share the aggregated `GET /metric-results` response to avoid client fan-out
