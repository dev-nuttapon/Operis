# notifications frontend module

Purpose:

* owns notification inbox and Phase 24 notification queue UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`

Notes:

* keep notification pages thin and route state through module hooks and API functions
* queue retry and enqueue actions go through module hooks instead of page-level API calls
