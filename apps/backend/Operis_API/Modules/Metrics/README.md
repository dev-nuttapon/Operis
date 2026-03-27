# metrics backend module

Purpose:

* owns Phase 9 metrics, collection schedules, metric results, and quality gate enforcement
* owns Phase 17 metric reviews and trend reports
* owns Phase 22 performance baselines, capacity reviews, slow operation reviews, and performance regression gates

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
* `metric_reviews`
* `trend_reports`
* `performance_baselines`
* `capacity_reviews`
* `slow_operation_reviews`
* `performance_gate_results`

Notes:

* endpoints stay thin and delegate threshold evaluation and override rules to `Application/`
* quality gate evaluation stores the metric measurements used for the decision
* review-close and trend-approval validation stay in `Application/`
* performance gate overrides require explicit reasons and stay server-driven
* slow operation closure requires verification evidence in `Application/`
