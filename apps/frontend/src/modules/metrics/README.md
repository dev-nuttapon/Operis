# metrics frontend module

Purpose:

* owns Phase 9 metric definitions, dashboard, schedules, and quality gate UI
* owns Phase 17 metrics review log and trend analysis report UI
* owns Phase 22 performance baseline, capacity review, slow operation review, and performance regression gate UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`
* `users/` for project option lookup

Notes:

* pages stay thin and use `Page -> Hook -> API -> HTTP client`
* dashboard widgets share the aggregated `GET /metric-results` response to avoid client fan-out
* trend approval and review closure validation stay server-driven
* performance gate override and slow-operation verification errors are surfaced from backend error codes
