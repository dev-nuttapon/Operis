namespace Operis_API.Shared.Contracts;

public static class ApiErrorCodeResolver
{
    private static readonly IReadOnlyDictionary<string, string> MessageCodes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["User already exists."] = ApiErrorCodes.UserExists,
            ["Pending invitation already exists."] = ApiErrorCodes.PendingInvitationExists,
            ["Pending registration request already exists."] = ApiErrorCodes.PendingRegistrationExists,
            ["Division already exists."] = ApiErrorCodes.DivisionExists,
            ["Department already exists."] = ApiErrorCodes.DepartmentExists,
            ["Job title already exists."] = ApiErrorCodes.JobTitleExists,
            ["Project role already exists."] = ApiErrorCodes.ProjectRoleExists,
            ["Project code and name are required."] = ApiErrorCodes.ProjectRequiredFields,
            ["Project type is required."] = ApiErrorCodes.ProjectTypeRequired,
            ["Project code already exists."] = ApiErrorCodes.ProjectCodeExists,
            ["Project does not exist."] = ApiErrorCodes.ProjectNotFound,
            ["Project owner does not exist."] = ApiErrorCodes.ProjectOwnerNotFound,
            ["Project sponsor does not exist."] = ApiErrorCodes.ProjectSponsorNotFound,
            ["Project role already exists in this project."] = ApiErrorCodes.ProjectRoleExists,
            ["Project role code already exists in this project."] = ApiErrorCodes.ProjectRoleCodeExists,
            ["Project role does not exist in this project."] = ApiErrorCodes.ProjectRoleNotFoundInProject,
            ["User does not exist."] = ApiErrorCodes.ProjectAssignmentUserNotFound,
            ["Reporting line user must already be assigned to this project."] = ApiErrorCodes.ProjectAssignmentReportingLineInvalid,
            ["Only active assignments can be updated."] = ApiErrorCodes.ProjectAssignmentActiveOnly,
            ["Change reason is required."] = ApiErrorCodes.ProjectAssignmentChangeReasonRequired,
            ["Project type template already exists."] = ApiErrorCodes.ProjectTypeTemplateExists,
            ["Project type template does not exist."] = ApiErrorCodes.ProjectTypeTemplateNotFound,
            ["Role name is required."] = ApiErrorCodes.ProjectTypeRoleRequirementRequired,
            ["Role requirement already exists for this project type."] = ApiErrorCodes.ProjectTypeRoleRequirementExists,
            ["Role requirement code already exists for this project type."] = ApiErrorCodes.ProjectTypeRoleRequirementCodeExists,
            ["Division name is required."] = ApiErrorCodes.DivisionRequired,
            ["Division does not exist."] = ApiErrorCodes.DivisionNotFound,
            ["Department name is required."] = ApiErrorCodes.DepartmentRequired,
            ["Department does not exist."] = ApiErrorCodes.DepartmentNotFound,
            ["Department is required when division is selected."] = ApiErrorCodes.DepartmentRequiredForDivision,
            ["Department is required when job title is selected."] = ApiErrorCodes.DepartmentRequiredForJobTitle,
            ["Department does not belong to the selected division."] = ApiErrorCodes.DepartmentDivisionMismatch,
            ["Job title name is required."] = ApiErrorCodes.JobTitleRequired,
            ["Job title does not exist."] = ApiErrorCodes.JobTitleNotFound,
            ["Job title does not belong to the selected department."] = ApiErrorCodes.JobTitleDepartmentMismatch,
            ["One or more selected roles do not exist."] = ApiErrorCodes.RolesNotFound,
            ["Email is required."] = ApiErrorCodes.EmailRequired,
            ["Password is required."] = ApiErrorCodes.PasswordRequired,
            ["Password must be at least 8 characters."] = ApiErrorCodes.PasswordMinLength,
            ["Password and confirmation do not match."] = ApiErrorCodes.PasswordMismatch,
            ["Registration request has already been reviewed."] = ApiErrorCodes.RegistrationReviewed,
            ["Registration request is not approved."] = ApiErrorCodes.RegistrationNotApproved,
            ["Password setup has already been completed."] = ApiErrorCodes.PasswordSetupCompleted,
            ["Password setup link has expired."] = ApiErrorCodes.PasswordSetupExpired,
            ["Expiration date must be in the future."] = ApiErrorCodes.ExpirationFuture,
            ["Invited by is required."] = ApiErrorCodes.InvitedByRequired,
            ["Invitation has already been accepted."] = ApiErrorCodes.InvitationAccepted,
            ["Invitation has already been rejected."] = ApiErrorCodes.InvitationRejected,
            ["Invitation has been cancelled."] = ApiErrorCodes.InvitationCancelled,
            ["Invitation has expired."] = ApiErrorCodes.InvitationExpired,
            ["Accepted invitation cannot be cancelled."] = ApiErrorCodes.InvitationCancelAccepted,
            ["Accepted invitation cannot be updated."] = ApiErrorCodes.InvitationUpdateAccepted,
            ["Cancelled invitation cannot be updated."] = ApiErrorCodes.InvitationUpdateCancelled,
            ["Rejected invitation cannot be updated."] = ApiErrorCodes.InvitationUpdateRejected,
            ["Workflow definition name is required."] = ApiErrorCodes.WorkflowDefinitionNameRequired,
            ["Workflow definition does not exist."] = ApiErrorCodes.WorkflowDefinitionNotFound,
            ["Workflow definition already exists."] = ApiErrorCodes.WorkflowDefinitionAlreadyExists,
            ["Workflow definition is already active."] = ApiErrorCodes.WorkflowDefinitionAlreadyActive,
            ["Workflow definition is already archived."] = ApiErrorCodes.WorkflowDefinitionAlreadyArchived,
        };

    public static string Resolve(string? message, string fallbackCode)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return fallbackCode;
        }

        return MessageCodes.TryGetValue(message, out var code) ? code : fallbackCode;
    }
}
