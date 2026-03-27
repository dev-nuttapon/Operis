# operations backend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependencies, and configuration audits
* owns Phase 16 suppliers and supplier agreement governance
* owns Phase 19 access recertification schedules and subject decision workflow
* owns Phase 21 security incidents, vulnerabilities, secret rotations, privileged access events, and classification policies

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

Notes:

* endpoints stay thin and delegate workflow and validation to `Application/`
* supplier archive rules and agreement evidence validation stay in `Application/`
* recertification completion is blocked until all scoped subjects have decisions on record
* incident closure, privileged access use, and secret rotation verification are enforced through stable backend validation codes
