# operations frontend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependency register, and configuration audit UI
* owns Phase 16 supplier register and supplier agreement evidence UI
* owns Phase 19 access recertification schedule and subject decision UI
* owns Phase 21 security incident, vulnerability, secret rotation, privileged access, and classification policy UI
* owns Phase 23 backup evidence, restore verification, DR drill, and legal hold UI
* owns Phase 24 CAPA register and escalation history UI

Public surface:

* `index.ts`

Dependencies:

* `shared/`

Notes:

* pages stay thin and use `Page -> Hook -> API -> HTTP client`
* external dependencies trace to supplier ownership and governing agreements through the same module surface
* recertification completion stays behind backend pending-decision validation
* security operations screens reuse the same operations permission surface for read/manage access
* secret rotation UI exposes explicit Keycloak, Redis, MinIO, and custom touchpoints with evidence references
* backup, restore, and legal hold flows keep evidence and release-rationale validation on the backend
* CAPA verification and closure stay behind backend workflow and open-action validation
