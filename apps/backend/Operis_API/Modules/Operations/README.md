# operations backend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependencies, and configuration audits
* owns Phase 16 suppliers and supplier agreement governance
* owns Phase 19 access recertification schedules and subject decision workflow
* owns Phase 21 security incidents, vulnerabilities, secret rotations, privileged access events, and classification policies
* owns Phase 23 backup evidence, restore verification, DR drill records, and legal holds
* owns Phase 24 CAPA records, CAPA actions, and escalation execution
* owns Phase 33 CAPA effectiveness reviews and CAPA reopen orchestration
* owns Phase 36 operational automation jobs, job runs, and job-run evidence references

Public surface:

* `OperationsModule.cs`
* `Application/IOperationsQueries.cs`
* `Application/IOperationsCommands.cs`
* `Contracts/`

Owned data:

* `access_reviews`
* `security_reviews`
* `external_dependencies`
* `suppliers`
* `supplier_agreements`
* `configuration_audits`
* `access_recertification_schedules`
* `access_recertification_decisions`
* `security_incidents`
* `vulnerability_records`
* `secret_rotations`
* `privileged_access_events`
* `data_classification_policies`
* `backup_evidence`
* `restore_verifications`
* `dr_drills`
* `legal_holds`
* `capa_records`
* `capa_actions`
* `capa_effectiveness_reviews`
* `escalation_events`
* `automation_jobs`
* `automation_job_runs`
* `automation_job_evidence_refs`

Notes:

* endpoints stay thin and delegate workflow and validation to `Application/`
* supplier archive rules and agreement evidence validation stay in `Application/`
* recertification completion is blocked until all scoped subjects have decisions on record
* incident closure, privileged access use, and secret rotation verification are enforced through stable backend validation codes
* secret rotations carry explicit touchpoints for `keycloak`, `redis`, `minio`, or `custom`, plus evidence linkage for verified rotations
* backup, restore, DR, and legal hold release validation stay in `Application/`
* CAPA closure is blocked until all actions are complete, effectiveness reviews are post-closure only, and ineffective reviews may reopen CAPA with traceable rationale
* escalation events stay append-only from endpoint composition
* operational automation stays as controlled metadata and execution evidence inside the module; endpoint composition does not orchestrate provider jobs directly
