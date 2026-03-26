# governance backend module

Purpose:

* owns process assets, QA checklists, project plans, stakeholder registers, and tailoring records
* enforces Phase 1 workflow transitions and governance audit evidence

Public surface:

* `GovernanceModule.cs`
* `Application/GovernanceQueries.cs`
* `Application/GovernanceCommands.cs`
* `Contracts/`

Owned data:

* process_assets
* process_asset_versions
* qa_checklists
* project_plans
* stakeholders
* tailoring_records

Notes:

* endpoints stay thin and delegate orchestration to `Application/`
* project ownership remains in the users module; governance reads project existence only
