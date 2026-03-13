import { useState } from "react";
import { App, Form } from "antd";
import { useAdminUsers } from "./useAdminUsers";
import { ApiError, getApiErrorPresentation } from "../../../shared/lib/apiClient";
import {
  getCurrentAdminUsersSection,
  getDisplayActor,
} from "../utils/adminUsersPresentation";
import type {
  Invitation,
  InvitationStatus,
  MasterDataItem,
  RegistrationRequest,
  RegistrationRequestStatus,
  User,
  UserStatus,
} from "../types/users";

type Translate = (key: string, options?: Record<string, unknown>) => string;

export function useAdminUsersScreen(input: {
  pathname: string;
  t: Translate;
  user: { name?: string | null; email?: string | null } | null | undefined;
}) {
  const { notification } = App.useApp();
  const actor = getDisplayActor(input.user);
  const currentSection = getCurrentAdminUsersSection(input.pathname);

  const [usersPaging, setUsersPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as UserStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [registrationPaging, setRegistrationPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as RegistrationRequestStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "requestedAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [invitationPaging, setInvitationPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: undefined as InvitationStatus | undefined,
    from: undefined as string | undefined,
    to: undefined as string | undefined,
    sortBy: "invitedAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [departmentPaging, setDepartmentPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [divisionPaging, setDivisionPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [jobTitlePaging, setJobTitlePaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });
  const [projectRolePaging, setProjectRolePaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    sortBy: "displayOrder",
    sortOrder: "asc" as "asc" | "desc",
  });

  const adminUsers = useAdminUsers({
    users: usersPaging,
    registrationRequests: registrationPaging,
    invitations: invitationPaging,
    divisions: divisionPaging,
    departments: departmentPaging,
    jobTitles: jobTitlePaging,
    projectRoles: projectRolePaging,
  });

  const [inviteForm] = Form.useForm();
  const [editInvitationForm] = Form.useForm();
  const [createUserForm] = Form.useForm();
  const [editUserForm] = Form.useForm();
  const [orgAssignmentForm] = Form.useForm();
  const [deleteUserForm] = Form.useForm();
  const [reviewRegistrationForm] = Form.useForm();
  const [createDivisionForm] = Form.useForm();
  const [createDepartmentForm] = Form.useForm();
  const [editDivisionForm] = Form.useForm();
  const [editDepartmentForm] = Form.useForm();
  const [createJobTitleForm] = Form.useForm();
  const [editJobTitleForm] = Form.useForm();
  const [createProjectRoleForm] = Form.useForm();
  const [editProjectRoleForm] = Form.useForm();
  const [deleteDivisionForm] = Form.useForm();
  const [deleteDepartmentForm] = Form.useForm();
  const [deleteJobTitleForm] = Form.useForm();
  const [deleteProjectRoleForm] = Form.useForm();

  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [editingUserAffiliation, setEditingUserAffiliation] = useState<User | null>(null);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [creatingInvitation, setCreatingInvitation] = useState(false);
  const [editingInvitation, setEditingInvitation] = useState<Invitation | null>(null);
  const [viewingInvitation, setViewingInvitation] = useState<Invitation | null>(null);
  const [viewingRegistrationLink, setViewingRegistrationLink] = useState<RegistrationRequest | null>(null);
  const [editingDivision, setEditingDivision] = useState<MasterDataItem | null>(null);
  const [editingDepartment, setEditingDepartment] = useState<MasterDataItem | null>(null);
  const [editingJobTitle, setEditingJobTitle] = useState<MasterDataItem | null>(null);
  const [editingProjectRole, setEditingProjectRole] = useState<MasterDataItem | null>(null);
  const [creatingUser, setCreatingUser] = useState(false);
  const [creatingDivision, setCreatingDivision] = useState(false);
  const [creatingDepartment, setCreatingDepartment] = useState(false);
  const [deletingDivision, setDeletingDivision] = useState<MasterDataItem | null>(null);
  const [deletingDepartment, setDeletingDepartment] = useState<MasterDataItem | null>(null);
  const [creatingJobTitle, setCreatingJobTitle] = useState(false);
  const [deletingJobTitle, setDeletingJobTitle] = useState<MasterDataItem | null>(null);
  const [creatingProjectRole, setCreatingProjectRole] = useState(false);
  const [deletingProjectRole, setDeletingProjectRole] = useState<MasterDataItem | null>(null);
  const [managingRegistration, setManagingRegistration] = useState<RegistrationRequest | null>(null);

  const handleSuccess = (message: string) => {
    notification.success({ message });
  };

  const getAdminErrorNotification = (error: unknown, fallbackTitle: string) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);

    if (!(error instanceof ApiError)) {
      return presentation;
    }

    if (error.message === input.t("errors.user_exists") || error.message === input.t("errors.keycloak_user_exists")) {
      return {
        title: input.t("admin_users.notifications.email_in_use_title"),
        description: input.t("admin_users.notifications.email_in_use_description"),
      };
    }

    if (error.message === input.t("errors.pending_invitation_exists")) {
      return {
        title: input.t("admin_users.notifications.pending_invitation_title"),
        description: input.t("admin_users.notifications.pending_invitation_description"),
      };
    }

    if (error.message === input.t("errors.pending_registration_exists")) {
      return {
        title: input.t("admin_users.notifications.pending_request_title"),
        description: input.t("admin_users.notifications.pending_request_description"),
      };
    }

    if (error.message === input.t("errors.invitation_accepted")) {
      return {
        title: input.t("admin_users.notifications.invitation_accepted_title"),
        description: input.t("admin_users.notifications.invitation_accepted_description"),
      };
    }

    if (error.message === input.t("errors.invitation_expired")) {
      return {
        title: input.t("admin_users.notifications.invitation_expired_title"),
        description: input.t("admin_users.notifications.invitation_expired_description"),
      };
    }

    if (error.category === "network") {
      return {
        title: input.t("admin_users.notifications.server_unavailable_title"),
        description: input.t("admin_users.notifications.server_unavailable_description"),
      };
    }

    return presentation;
  };

  const handleError = (message: string, error: unknown) => {
    const presentation = getAdminErrorNotification(error, message);
    notification.error({
      message: presentation.title,
      description: presentation.description,
    });
  };

  const copyToClipboard = async (value: string, successMessage: string) => {
    await navigator.clipboard.writeText(value);
    handleSuccess(successMessage);
  };

  return {
    actor,
    copyToClipboard,
    createDivisionForm,
    createDepartmentForm,
    createJobTitleForm,
    createProjectRoleForm,
    createUserForm,
    creatingDivision,
    creatingDepartment,
    creatingInvitation,
    creatingJobTitle,
    creatingProjectRole,
    creatingUser,
    currentSection,
    deleteDivisionForm,
    deleteDepartmentForm,
    deleteJobTitleForm,
    deleteProjectRoleForm,
    deleteUserForm,
    deletingDivision,
    deletingDepartment,
    deletingJobTitle,
    deletingProjectRole,
    deletingUser,
    divisionPaging,
    editDivisionForm,
    departmentPaging,
    editDepartmentForm,
    editInvitationForm,
    editJobTitleForm,
    editProjectRoleForm,
    editUserForm,
    orgAssignmentForm,
    editingDivision,
    editingDepartment,
    editingInvitation,
    editingJobTitle,
    editingProjectRole,
    editingUser,
    editingUserAffiliation,
    handleError,
    handleSuccess,
    invitationPaging,
    inviteForm,
    jobTitlePaging,
    projectRolePaging,
    managingRegistration,
    registrationPaging,
    reviewRegistrationForm,
    setCreatingDivision,
    setCreatingDepartment,
    setCreatingInvitation,
    setCreatingJobTitle,
    setCreatingProjectRole,
    setCreatingUser,
    setDeletingDivision,
    setDeletingDepartment,
    setDeletingJobTitle,
    setDeletingProjectRole,
    setDeletingUser,
    setDivisionPaging,
    setDepartmentPaging,
    setEditingDivision,
    setEditingDepartment,
    setEditingInvitation,
    setEditingJobTitle,
    setEditingProjectRole,
    setEditingUser,
    setEditingUserAffiliation,
    setInvitationPaging,
    setJobTitlePaging,
    setProjectRolePaging,
    setManagingRegistration,
    setRegistrationPaging,
    setUsersPaging,
    setViewingInvitation,
    setViewingRegistrationLink,
    usersPaging,
    viewingInvitation,
    viewingRegistrationLink,
    ...adminUsers,
  };
}
