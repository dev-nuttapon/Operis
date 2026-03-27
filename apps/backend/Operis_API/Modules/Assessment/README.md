# assessment backend module

Purpose:

* owns Phase 34 assessor workspace APIs
* owns Phase 35 control catalog, control mapping, and control coverage APIs
* packages evidence references into stable review bundles for assessor use

Public surface:

* `AssessmentModule.cs`
* `Application/IAssessmentQueries.cs`
* `Application/IAssessmentCommands.cs`

Owned data:

* `assessment_packages`
* `assessment_findings`
* `assessment_notes`
* `control_catalog`
* `control_mappings`
* `control_coverage_snapshots`

Notes:

* evidence references are copied into package scope at creation time to keep assessor review stable
* findings must reference evidence already included in the package snapshot
* control mappings stay inside the assessment boundary and reference governed artifacts by stable route/entity identifiers rather than direct cross-module writes
* control coverage snapshots are generated from current active mappings to support review and dashboard drilldowns
