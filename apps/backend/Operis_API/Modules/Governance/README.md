# governance backend module

Purpose:

* owns process assets, QA checklists, project plans, stakeholder registers, tailoring records, and governance control registers
* enforces Phase 1 workflow transitions and governance audit evidence
* owns Phase 13 RACI maps, approval evidence logs, workflow override logs, SLA rules, and retention policies
* owns Phase 20 architecture records, design reviews, and integration reviews

Public surface:

* `GovernanceModule.cs`
* `Application/GovernanceQueries.cs`
* `Application/GovernanceCommands.cs`
* `Application/GovernanceOperationsQueries.cs`
* `Application/GovernanceOperationsCommands.cs`
* `Contracts/`

Owned data:

* process_assets
* process_asset_versions
* qa_checklists
* project_plans
* stakeholders
* tailoring_records
* raci_maps
* approval_evidence_logs
* workflow_override_logs
* sla_rules
* retention_policies
* architecture_records
* design_reviews
* integration_reviews

Notes:

* endpoints stay thin and delegate orchestration to `Application/`
* project ownership remains in the users module; governance reads project existence only
