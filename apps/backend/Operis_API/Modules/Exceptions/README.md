# exceptions backend module

Purpose:

* owns Phase 32 process waiver requests, compensating controls, and waiver review history

Public surface:

* `ExceptionsModule.cs`
* `Application/IExceptionQueries.cs`
* `Application/IExceptionCommands.cs`
* `Contracts/`

Owned data:

* `waivers`
* `compensating_controls`
* `waiver_reviews`

Notes:

* the module owns all waiver workflow writes
* cross-module reads are limited to project name lookups from `Users`
