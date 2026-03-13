# workflows backend module

Purpose:

* owns workflow-facing contracts and orchestration as workflow features are introduced

Public surface:

* `WorkflowsModule.cs`
* `Application/`
* `Contracts/`

Owned data:

* `workflow_definitions`

Notes:

* keep endpoint mapping thin and push orchestration into `Application/`
