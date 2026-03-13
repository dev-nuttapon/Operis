# documents frontend module

Purpose:

* owns document dashboard UI and document list retrieval flows

Public surface:

* `index.ts`

Internal only:

* `DocumentTestForm`
* page-level local form state

Dependencies:

* `auth` via public surface only
* `shared/`

Notes:

* page must stay thin and delegate to hooks/API functions
* do not export internal form components through the module public entry
