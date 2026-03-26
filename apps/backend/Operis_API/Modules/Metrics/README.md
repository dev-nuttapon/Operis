# metrics backend module

Purpose:

* owns Phase 9 metrics, collection schedules, metric results, and quality gate enforcement

Public surface:

* `MetricsModule.cs`
* `Application/IMetricsQueries.cs`
* `Application/IMetricsCommands.cs`
* `Contracts/`

Owned data:

* `metric_definitions`
* `metric_collection_schedules`
* `metric_results`
* `quality_gate_results`

Notes:

* endpoints stay thin and delegate threshold evaluation and override rules to `Application/`
* quality gate evaluation stores the metric measurements used for the decision
