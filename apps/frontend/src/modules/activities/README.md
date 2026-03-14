# audits frontend module

Purpose:

* owns audit log viewing and filtering UI

Public surface:

* `index.ts`

Internal only:

* audit table presentation details and filter form composition

Dependencies:

* `shared/`

Notes:

* keep filtering and data retrieval behind module hooks and API functions
