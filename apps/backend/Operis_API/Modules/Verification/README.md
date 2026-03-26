# verification backend module

Purpose:

* owns Phase 7 verification and validation workflows
* governs test plans, test cases, test executions, and UAT sign-off

Public surface:

* `VerificationModule.cs`
* `Application/IVerificationQueries.cs`
* `Application/IVerificationCommands.cs`
* `Contracts/`

Owned data:

* test_plans
* test_cases
* test_executions
* uat_signoffs

Notes:

* endpoints stay thin and delegate workflow validation to `Application/`
* requirement coverage is checked against linked test cases and execution evidence
* sensitive execution evidence is filtered unless the caller has `verification.evidence_sensitive.read`
