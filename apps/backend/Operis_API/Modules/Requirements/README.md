# requirements backend module

Purpose:

* owns Phase 3 requirement register, baseline, and traceability workflows
* enforces mandatory downstream traceability before governed baseline actions

Public surface:

* `RequirementsModule.cs`
* `Application/IRequirementQueries.cs`
* `Application/IRequirementCommands.cs`
* `Contracts/`

Owned data:

* requirements
* requirement_versions
* requirement_baselines
* traceability_links

Notes:

* endpoints stay thin and delegate lifecycle validation to `Application/`
* history is projected from business audit events rather than a module-local history table
