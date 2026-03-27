# Phase 26: Evidence Completeness Rules

Goal

* detect missing required evidence per project and process area
* persist evaluator runs so audits and compliance views can drill into exact missing items

In Scope

* evidence rule definitions
* evaluator execution and persisted results
* missing evidence register and detail drilldown
* audits UI for rule lifecycle, evaluation runs, and missing item review

Out of Scope

* assessor packaging workflows
* cross-module write orchestration into source modules
* dynamic expression language beyond first-version `required` rules

Owning Module

* backend: `Audits`
* frontend: `audits`

Owned Tables

* `evidence_rules`
* `evidence_rule_results`
* `evidence_missing_items`

Read-Only Upstream Sources

* `projects`
* `project_plans`
* `tailoring_records`
* `requirements`
* `requirement_baselines`
* `traceability_links`
* `documents`
* `change_requests`
* `baseline_registry`
* `test_plans`
* `uat_signoffs`
* `audit_findings`
* `capa_records`
* `security_reviews`

Routes

* backend
  * `GET /api/v1/audits/evidence-rules`
  * `POST /api/v1/audits/evidence-rules`
  * `PUT /api/v1/audits/evidence-rules/{ruleId}`
  * `POST /api/v1/audits/evidence-rules/evaluate`
  * `GET /api/v1/audits/evidence-results`
  * `GET /api/v1/audits/evidence-results/{resultId}`
* frontend
  * `/app/audits/evidence-completeness`
  * `/app/audits/evidence-completeness/:resultId`

Permissions

* `audits.evidence.read`
* `audits.evidence.manage`

First-Version Supported Rule Targets

* `project_plan_baseline`
* `tailoring_approval`
* `requirement_baseline`
* `requirement_test_traceability`
* `approved_document`
* `approved_change_request`
* `baseline_registry_link`
* `approved_uat_signoff`
* `resolved_audit_finding`
* `security_review_completion`

Process Areas

* `process-assets-planning`
* `requirements-traceability`
* `document-governance`
* `change-configuration`
* `verification-release`
* `audit-capa`
* `security-resilience`

Rule Model

* rule states: `draft -> active -> retired`
* rule expression: first version only supports `required`
* rules may target all projects or a specific `project_id`
* rules may target one process area and one supported artifact target

Evaluation Model

* result states: `queued -> completed -> superseded`
* command creates one result row per evaluation run
* each missing evidence record is persisted in `evidence_missing_items`
* re-running evaluation for the same scope supersedes older completed runs for that exact scope

Validation / Error Codes

* `evidence_rule_target_required`
* `evidence_rule_expression_invalid`
* `evidence_rule_process_area_invalid`
* `evidence_rule_status_invalid`
* `evidence_result_not_found`
* `evidence_missing_approval`
* `evidence_missing_traceability`
* `evidence_missing_baseline`
* `evidence_missing_document`
* `evidence_missing_security_review`

Frontend Structure

* `Page -> Hook -> API -> HTTP Client`
* pages
  * `EvidenceCompletenessPage.tsx`
  * `EvidenceCompletenessDetailPage.tsx`
* hooks
  * evidence rule list
  * evidence result list/detail
  * create/update/evaluate mutations

Acceptance Criteria

* users can define, update, and retire evidence rules
* users can run evaluation for all projects or a single project
* evaluation results persist missing evidence records with source routes
* detail view shows exact missing items, reason code, and source navigation
* all global quality gates pass
