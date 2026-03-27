# releases backend module

Purpose:

* owns Phase 14 release registry, deployment checklist tracking, and release notes publication workflow

Public surface:

* `ReleasesModule.cs`
* `Application/IReleaseQueries.cs`
* `Application/IReleaseCommands.cs`
* `Contracts/`

Owned data:

* `releases`
* `deployment_checklists`
* `release_notes`

Notes:

* endpoints stay thin and delegate checklist, approval, and release gating rules to `Application/`
* release execution validates checklist completion and latest release readiness gate evidence before moving to `released`
