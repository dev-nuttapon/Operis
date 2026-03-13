# workflows frontend module

Purpose:

* owns workflow definition and workflow-facing user interfaces

Public surface:

* `index.ts`

Dependencies:

* `shared/`

Notes:

* keep workflow pages thin and route state through module hooks and API functions
* definition creation must invalidate the definitions query instead of mutating local copies
