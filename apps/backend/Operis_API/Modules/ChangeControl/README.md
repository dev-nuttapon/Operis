# change control backend module

Purpose:

* owns Phase 4 change requests, configuration items, and baseline registry workflows
* enforces approved change request linkage before governed baseline changes

Public surface:

* `ChangeControlModule.cs`
* `Application/IChangeControlQueries.cs`
* `Application/IChangeControlCommands.cs`
* `Contracts/`

Owned data:

* change_requests
* change_impacts
* configuration_items
* baseline_registry

Notes:

* endpoints stay thin and delegate lifecycle validation to `Application/`
* emergency override for baseline supersede requires elevated permission and explicit rationale
