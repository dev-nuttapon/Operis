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
    public const string ProjectRequiredFields = "project_required_fields";
    public const string ProjectTypeRequired = "project_type_required";
    public const string ProjectCodeExists = "project_code_exists";
    public const string ProjectNotFound = "project_not_found";
    public const string ProjectOwnerNotFound = "project_owner_not_found";
    public const string ProjectSponsorNotFound = "project_sponsor_not_found";
    public const string ProjectRoleCodeExists = "project_role_code_exists";
    public const string ProjectRoleNotFoundInProject = "project_role_not_found_in_project";
    public const string ProjectAssignmentUserNotFound = "project_assignment_user_not_found";
    public const string ProjectAssignmentReportingLineInvalid = "project_assignment_reporting_line_invalid";
    public const string ProjectAssignmentActiveOnly = "project_assignment_active_only";
    public const string ProjectAssignmentChangeReasonRequired = "project_assignment_change_reason_required";
    public const string ProjectTypeTemplateExists = "project_type_template_exists";
    public const string ProjectTypeTemplateNotFound = "project_type_template_not_found";
    public const string ProjectTypeRoleRequirementRequired = "project_type_role_requirement_required";
    public const string ProjectTypeRoleRequirementExists = "project_type_role_requirement_exists";
    public const string ProjectTypeRoleRequirementCodeExists = "project_type_role_requirement_code_exists";
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

    public static class Documents
    {
        public const string NameRequired = "document_name_required";
        public const string FileRequired = "document_file_required";
        public const string FileEmpty = "document_file_empty";
        public const string FileTooLarge = "document_file_too_large";
        public const string FileTypeNotAllowed = "document_file_type_not_allowed";
        public const string VersionCodeRequired = "document_version_code_required";
        public const string VersionCodeExists = "document_version_code_exists";
        public const string DocumentNotFound = "document_not_found";
        public const string DocumentNameExists = "document_name_exists";
        public const string DeleteReasonRequired = "document_delete_reason_required";
    }
}
