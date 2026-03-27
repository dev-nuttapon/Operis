# operations backend module

Purpose:

* owns Phase 12 access reviews, security reviews, external dependencies, and configuration audits
* owns Phase 16 suppliers and supplier agreement governance

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

Notes:

* endpoints stay thin and delegate workflow and validation to `Application/`
* supplier archive rules and agreement evidence validation stay in `Application/`
