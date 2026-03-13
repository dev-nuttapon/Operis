# users frontend module

Purpose:

* owns user administration, invitations, registration flows, and user-facing preferences

Public surface:

* `index.ts`
* `public.ts` for route-level composition where bundle boundaries matter

Internal only:

* admin sections, modals, presentation helpers, and page-local orchestration

Dependencies:

* `auth` via public surface only
* `shared/`

Notes:

* preserve `Page -> Hook -> API -> HTTP Client`
* keep admin workflows split by section and lazy boundaries
