# auth frontend module

Purpose:

* owns authentication UI and auth-facing client hooks for the frontend shell

Public surface:

* `index.ts`

Internal only:

* implementation details under `components/`, `hooks/`, `pages/`, `services/`

Dependencies:

* `shared/`
* Keycloak client integration

Notes:

* treat this module as cross-cutting
* app shell must consume auth through the public entry only
