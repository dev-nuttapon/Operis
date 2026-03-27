# learning backend module

Purpose:

* owns Phase 28 training catalog, role training requirements, completion tracking, and competency review workflow

Public surface:

* `LearningModule.cs`
* `Application/ILearningQueries.cs`
* `Application/ILearningCommands.cs`
* `Contracts/`

Owned data:

* `training_courses`
* `role_training_requirements`
* `training_completions`
* `competency_reviews`

Notes:

* endpoints stay thin and delegate validation and workflow logic to `Application/`
* role-to-training readiness reads `Users` tables, but ownership of training records remains inside `Learning`
