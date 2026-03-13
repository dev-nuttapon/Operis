namespace Operis_API.Shared.Contracts;

public static class ApiErrorCodes
{
    public const string RequestValidationFailed = "request_validation_failed";
    public const string ResourceNotFound = "resource_not_found";
    public const string ExternalDependencyFailure = "external_dependency_failure";
    public const string InternalFailure = "internal_failure";

    public const string UserExists = "user_exists";
    public const string PendingInvitationExists = "pending_invitation_exists";
    public const string PendingRegistrationExists = "pending_registration_exists";
    public const string DivisionExists = "division_exists";
    public const string DepartmentExists = "department_exists";
    public const string JobTitleExists = "job_title_exists";
    public const string ProjectRoleExists = "project_role_exists";
    public const string DivisionRequired = "division_required";
    public const string DivisionNotFound = "division_not_found";
    public const string DepartmentRequired = "department_required";
    public const string DepartmentNotFound = "department_not_found";
    public const string DepartmentRequiredForDivision = "department_required_for_division";
    public const string DepartmentRequiredForJobTitle = "department_required_for_job_title";
    public const string DepartmentDivisionMismatch = "department_division_mismatch";
    public const string JobTitleRequired = "job_title_required";
    public const string JobTitleNotFound = "job_title_not_found";
    public const string JobTitleDepartmentMismatch = "job_title_department_mismatch";
    public const string RolesNotFound = "roles_not_found";
    public const string EmailRequired = "email_required";
    public const string PasswordRequired = "password_required";
    public const string PasswordMinLength = "password_min_length";
    public const string PasswordMismatch = "password_mismatch";
    public const string RegistrationReviewed = "registration_reviewed";
    public const string RegistrationNotApproved = "registration_not_approved";
    public const string PasswordSetupCompleted = "password_setup_completed";
    public const string PasswordSetupExpired = "password_setup_expired";
    public const string ExpirationFuture = "expiration_future";
    public const string InvitedByRequired = "invited_by_required";
    public const string InvitationAccepted = "invitation_accepted";
    public const string InvitationRejected = "invitation_rejected";
    public const string InvitationCancelled = "invitation_cancelled";
    public const string InvitationExpired = "invitation_expired";
    public const string InvitationCancelAccepted = "invitation_cancel_accepted";
    public const string InvitationUpdateAccepted = "invitation_update_accepted";
    public const string InvitationUpdateCancelled = "invitation_update_cancelled";
    public const string InvitationUpdateRejected = "invitation_update_rejected";

    public const string WorkflowDefinitionNameRequired = "workflow_definition_name_required";
    public const string WorkflowDefinitionNotFound = "workflow_definition_not_found";
    public const string WorkflowDefinitionAlreadyExists = "workflow_definition_already_exists";
    public const string WorkflowDefinitionAlreadyActive = "workflow_definition_already_active";
    public const string WorkflowDefinitionAlreadyArchived = "workflow_definition_already_archived";
}
