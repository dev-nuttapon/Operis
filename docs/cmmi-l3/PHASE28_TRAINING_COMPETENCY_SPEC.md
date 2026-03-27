# Phase 28: Training & Competency

Goal:

* track mandatory training readiness by role, project, and user
* let training managers maintain the catalog, role matrix, completions, and competency reviews

Owning module:

* `Learning`

Owned tables:

* `training_courses`
* `role_training_requirements`
* `training_completions`
* `competency_reviews`

Routes:

* `/app/learning/training-catalog`
* `/app/learning/role-training-matrix`
* `/app/learning/completions`

API contracts:

* `GET /api/v1/learning/courses`
* `POST /api/v1/learning/courses`
* `PUT /api/v1/learning/courses/{courseId}`
* `POST /api/v1/learning/courses/{courseId}/transition`
* `GET /api/v1/learning/role-matrix`
* `POST /api/v1/learning/role-matrix`
* `PUT /api/v1/learning/role-matrix/{requirementId}`
* `GET /api/v1/learning/completions`
* `POST /api/v1/learning/completions`
* `PUT /api/v1/learning/completions/{completionId}`
* `GET /api/v1/learning/competency-reviews`
* `POST /api/v1/learning/competency-reviews`
* `PUT /api/v1/learning/competency-reviews/{reviewId}`
* `GET /api/v1/learning/project-roles`

Permissions:

* `learning.training.read`
* `learning.training.manage`
* `learning.training.approve`

Validation and error codes:

* `training_course_title_required`
* `training_requirement_role_required`
* `training_completion_date_required`
* `training_course_not_found`
* `training_course_code_duplicate`
* `training_requirement_not_found`
* `training_requirement_exists`
* `training_completion_not_found`
* `competency_review_not_found`

Workflow states:

* course: `draft -> active -> retired`
* requirement: `active -> archived`
* completion: `assigned -> completed -> expired`
* competency review: `planned -> in_progress -> completed -> archived`

Acceptance criteria:

* training managers can maintain the catalog and role matrix without using `Users` internals directly
* completion queries show assigned, overdue, and expired status from project-role requirements and project assignments
* competency reviews are managed inside the same module and exposed through the shared learning routes
* frontend follows `Page -> Hook -> API -> HTTP client`
