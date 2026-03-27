# governance backend module

Purpose:

* owns process assets, QA checklists, project plans, stakeholder registers, tailoring records, and governance control registers
* enforces Phase 1 workflow transitions and governance audit evidence
* owns Phase 13 RACI maps, approval evidence logs, workflow override logs, SLA rules, and retention policies
* owns Phase 20 architecture records, design reviews, and integration reviews
* owns Phase 25 compliance dashboard snapshots and user dashboard preferences
* owns Phase 27 management reviews, review agenda items, and follow-up actions
* owns Phase 29 policy registers, acknowledgement campaigns, and user attestation records

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
* compliance_snapshots
* compliance_dashboard_preferences
* management_reviews
* management_review_items
* management_review_actions
* policies
* policy_campaigns
* policy_acknowledgements

Notes:

* endpoints stay thin and delegate orchestration to `Application/`
* project ownership remains in the users module; governance reads project existence only
* compliance dashboard scoring is read-heavy and aggregates owned and read-only upstream process data into snapshot records
* management review close is blocked until mandatory follow-up actions are closed and minutes are recorded
* policy campaign launch resolves target users from `Users`-owned records but stores all campaign and acknowledgement state inside `Governance`
