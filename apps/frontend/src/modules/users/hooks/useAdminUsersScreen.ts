import { useState } from "react";
import { App, Form } from "antd";
import { useAdminUsers } from "./useAdminUsers";
import { ApiError, getApiErrorPresentation, hasApiErrorCode } from "../../../shared/lib/apiClient";
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
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";

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
  const debouncedUsersSearch = useDebouncedValue(usersPaging.search, 300);
  const debouncedRegistrationSearch = useDebouncedValue(registrationPaging.search, 300);
  const debouncedInvitationSearch = useDebouncedValue(invitationPaging.search, 300);
  const debouncedDivisionSearch = useDebouncedValue(divisionPaging.search, 300);
  const debouncedDepartmentSearch = useDebouncedValue(departmentPaging.search, 300);
  const debouncedJobTitleSearch = useDebouncedValue(jobTitlePaging.search, 300);

  const adminUsers = useAdminUsers({
    users: { ...usersPaging, search: debouncedUsersSearch },
    registrationRequests: { ...registrationPaging, search: debouncedRegistrationSearch },
    invitations: { ...invitationPaging, search: debouncedInvitationSearch },
    divisions: { ...divisionPaging, search: debouncedDivisionSearch },
    departments: { ...departmentPaging, search: debouncedDepartmentSearch },
    jobTitles: { ...jobTitlePaging, search: debouncedJobTitleSearch },
  });

  const [inviteForm] = Form.useForm();
  const [editInvitationForm] = Form.useForm();
  const [createUserForm] = Form.useForm();
  const [editUserForm] = Form.useForm();
  const [deleteUserForm] = Form.useForm();
  const [reviewRegistrationForm] = Form.useForm();
  const [createDivisionForm] = Form.useForm();
  const [createDepartmentForm] = Form.useForm();
  const [editDivisionForm] = Form.useForm();
  const [editDepartmentForm] = Form.useForm();
  const [createJobTitleForm] = Form.useForm();
  const [editJobTitleForm] = Form.useForm();
  const [deleteDivisionForm] = Form.useForm();
  const [deleteDepartmentForm] = Form.useForm();
  const [deleteJobTitleForm] = Form.useForm();

  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deletingUser, setDeletingUser] = useState<User | null>(null);
  const [creatingInvitation, setCreatingInvitation] = useState(false);
  const [editingInvitation, setEditingInvitation] = useState<Invitation | null>(null);
  const [viewingInvitation, setViewingInvitation] = useState<Invitation | null>(null);
  const [viewingRegistrationLink, setViewingRegistrationLink] = useState<RegistrationRequest | null>(null);
  const [editingDivision, setEditingDivision] = useState<MasterDataItem | null>(null);
  const [editingDepartment, setEditingDepartment] = useState<MasterDataItem | null>(null);
  const [editingJobTitle, setEditingJobTitle] = useState<MasterDataItem | null>(null);
  const [creatingUser, setCreatingUser] = useState(false);
  const [creatingDivision, setCreatingDivision] = useState(false);
  const [creatingDepartment, setCreatingDepartment] = useState(false);
  const [deletingDivision, setDeletingDivision] = useState<MasterDataItem | null>(null);
  const [deletingDepartment, setDeletingDepartment] = useState<MasterDataItem | null>(null);
  const [creatingJobTitle, setCreatingJobTitle] = useState(false);
  const [deletingJobTitle, setDeletingJobTitle] = useState<MasterDataItem | null>(null);
  const [managingRegistration, setManagingRegistration] = useState<RegistrationRequest | null>(null);

  const handleSuccess = (message: string) => {
    notification.success({ message });
  };

  const getAdminErrorNotification = (error: unknown, fallbackTitle: string) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);

    if (!(error instanceof ApiError)) {
      return presentation;
    }

    if (hasApiErrorCode(error, "user_exists", "keycloak_user_exists")) {
      return {
        title: input.t("admin_users.notifications.email_in_use_title"),
        description: input.t("admin_users.notifications.email_in_use_description"),
      };
    }

    if (hasApiErrorCode(error, "pending_invitation_exists")) {
      return {
        title: input.t("admin_users.notifications.pending_invitation_title"),
        description: input.t("admin_users.notifications.pending_invitation_description"),
      };
    }

    if (hasApiErrorCode(error, "pending_registration_exists")) {
      return {
        title: input.t("admin_users.notifications.pending_request_title"),
        description: input.t("admin_users.notifications.pending_request_description"),
      };
    }

    if (hasApiErrorCode(error, "invitation_accepted")) {
      return {
        title: input.t("admin_users.notifications.invitation_accepted_title"),
        description: input.t("admin_users.notifications.invitation_accepted_description"),
      };
    }

    if (hasApiErrorCode(error, "invitation_expired")) {
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
    createUserForm,
    creatingDivision,
    creatingDepartment,
    creatingInvitation,
    creatingJobTitle,
    creatingUser,
    currentSection,
    deleteDivisionForm,
    deleteDepartmentForm,
    deleteJobTitleForm,
    deleteUserForm,
    deletingDivision,
    deletingDepartment,
    deletingJobTitle,
    deletingUser,
    divisionPaging,
    editDivisionForm,
    departmentPaging,
    editDepartmentForm,
    editInvitationForm,
    editJobTitleForm,
    editUserForm,
    editingDivision,
    editingDepartment,
    editingInvitation,
    editingJobTitle,
    editingUser,
    handleError,
    handleSuccess,
    invitationPaging,
    inviteForm,
    jobTitlePaging,
    managingRegistration,
    registrationPaging,
    reviewRegistrationForm,
    setCreatingDivision,
    setCreatingDepartment,
    setCreatingInvitation,
    setCreatingJobTitle,
    setCreatingUser,
    setDeletingDivision,
    setDeletingDepartment,
    setDeletingJobTitle,
    setDeletingUser,
    setDivisionPaging,
    setDepartmentPaging,
    setEditingDivision,
    setEditingDepartment,
    setEditingInvitation,
    setEditingJobTitle,
    setEditingUser,
    setInvitationPaging,
    setJobTitlePaging,
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
