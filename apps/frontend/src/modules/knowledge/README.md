# knowledge frontend module

Purpose:

* owns Phase 18 lessons learned repository and publication UI

Public surface:

* `index.ts`

Dependencies:

* `users/` for project option lookups
* `shared/`

Notes:

* pages stay thin and follow `Page -> Hook -> API -> HTTP client`
* publication validation remains server-driven so source and evidence rules stay consistent
