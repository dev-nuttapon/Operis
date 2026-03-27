# PHASE31_ADOPTION_SCORING_SPEC

Goal

* measure actual process adoption by project from workflow evidence already owned by upstream modules

In Scope

* adoption rules
* adoption scorecards
* adoption anomaly register
* backend evaluation endpoint
* frontend scorecard and rule management screen

Out of Scope

* predictive scoring
* cross-project benchmarking beyond rule threshold evaluation
* assessor packaging

Owning Module

* `Metrics`

Owned Tables

* `adoption_rules`
* `adoption_scores`
* `adoption_anomalies`

Read Sources

* `projects`
* `project_plans`
* `tailoring_records`
* `requirements`
* `test_plans`
* `change_requests`
* `metric_reviews`

Routes

* `GET /api/v1/metrics/adoption-rules`
* `POST /api/v1/metrics/adoption-rules`
* `PUT /api/v1/metrics/adoption-rules/{adoptionRuleId}`
* `POST /api/v1/metrics/adoption-rules/evaluate`
* `GET /api/v1/metrics/adoption-scorecards`
* `/app/metrics/adoption-scorecards`

Permissions

* `metrics.adoption.read`
* `metrics.adoption.manage`

Validation / Error Codes

* `adoption_rule_scope_required`
* `adoption_rule_threshold_invalid`
* `adoption_rule_not_found`
* `adoption_rule_code_duplicate`

Workflow States

* adoption rule: `draft -> active -> archived`
* adoption score state: `meets_threshold | below_threshold`
* anomaly status: `open` in first version

Acceptance Criteria

* metrics module owns all adoption scoring writes
* evaluation reads upstream evidence without writing to non-owned tables
* scorecards are visible per project and process area
* scorecards surface open anomalies when a score falls below threshold
