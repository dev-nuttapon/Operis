# audits frontend module

Purpose:

* owns audit log viewing, evidence export, and process audit planning/finding UI

Public surface:

* `index.ts`

Internal only:

* audit table presentation details and modal/form composition

Dependencies:

* `shared/`

Notes:

* keep screens thin and follow `Page -> Hook -> API -> HTTP client`
* route-level consumers should stay on the module public surface
