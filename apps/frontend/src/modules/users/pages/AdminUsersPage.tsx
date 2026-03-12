import { Suspense, lazy, type ReactNode } from "react";
import {
  Alert,
  Card,
  Modal,
  Space,
  Tag,
  Typography,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import { TeamOutlined } from "@ant-design/icons";
import dayjs from "dayjs";
import { useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../../auth";
import { useAdminUsersScreen } from "../hooks/useAdminUsersScreen";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { formatDate } from "../utils/adminUsersPresentation";
import type { Invitation } from "../types/users";

const { Paragraph, Text } = Typography;

const AdminInvitationModals = lazy(() =>
  import("../components/adminUsers/AdminInvitationModals").then((module) => ({ default: module.AdminInvitationModals }))
);
const AdminInvitationsSection = lazy(() =>
  import("../components/adminUsers/AdminInvitationsSection").then((module) => ({ default: module.AdminInvitationsSection }))
);
const AdminMasterDataSection = lazy(() =>
  import("../components/adminUsers/AdminMasterDataSection").then((module) => ({ default: module.AdminMasterDataSection }))
);
const AdminRegistrationsSection = lazy(() =>
  import("../components/adminUsers/AdminRegistrationsSection").then((module) => ({ default: module.AdminRegistrationsSection }))
);
const AdminUserModals = lazy(() =>
  import("../components/adminUsers/AdminUserModals").then((module) => ({ default: module.AdminUserModals }))
);
const AdminUsersDirectorySection = lazy(() =>
  import("../components/adminUsers/AdminUsersDirectorySection").then((module) => ({ default: module.AdminUsersDirectorySection }))
);
const AdminRegistrationModals = lazy(() =>
  import("../components/adminUsers/AdminRegistrationModals").then((module) => ({ default: module.AdminRegistrationModals }))
);
const AdminMasterDataModals = lazy(() =>
  import("../components/adminUsers/AdminMasterDataModals").then((module) => ({ default: module.AdminMasterDataModals }))
);

export function AdminUsersPage() {
  const { t, i18n: i18nInstance } = useTranslation();
  const location = useLocation();
  const { user } = useAuth();
  const currentLanguage = i18nInstance.language;
  const {
    actor,
    approveRegistrationMutation,
    cancelInvitationMutation,
    copyToClipboard,
    createDepartmentForm,
    createDepartmentMutation,
    createInvitationMutation,
    createJobTitleForm,
    createJobTitleMutation,
    createUserForm,
    createUserMutation,
    creatingDepartment,
    creatingInvitation,
    creatingJobTitle,
    creatingUser,
    currentSection,
    deleteDepartmentForm,
    deleteDepartmentMutation,
    deleteJobTitleForm,
    deleteJobTitleMutation,
    deleteUserForm,
    deleteUserMutation,
    deletingDepartment,
    deletingJobTitle,
    deletingUser,
    departmentOptionsQuery,
    departmentPaging,
    departmentsQuery,
    editDepartmentForm,
    editInvitationForm,
    editJobTitleForm,
    editUserForm,
    editingDepartment,
    editingInvitation,
    editingJobTitle,
    editingUser,
    handleError,
    handleSuccess,
    invitationPaging,
    invitationsQuery,
    inviteForm,
    jobTitleOptionsQuery,
    jobTitlePaging,
    jobTitlesQuery,
    managingRegistration,
    registrationPaging,
    registrationRequestsQuery,
    rejectRegistrationMutation,
    reviewRegistrationForm,
    rolesQuery,
    setCreatingDepartment,
    setCreatingInvitation,
    setCreatingJobTitle,
    setCreatingUser,
    setDeletingDepartment,
    setDeletingJobTitle,
    setDeletingUser,
    setDepartmentPaging,
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
    updateDepartmentMutation,
    updateInvitationMutation,
    updateJobTitleMutation,
    updateUserMutation,
    usersPaging,
    usersQuery,
    viewingInvitation,
    viewingRegistrationLink,
  } = useAdminUsersScreen({
    pathname: location.pathname,
    t,
    user,
  });

  const userRoleOptions = (rolesQuery.data ?? []).map((item) => ({
    label: (
      <Space direction="vertical" size={0}>
        <Text>{item.name}</Text>
        {item.description ? <Text type="secondary">{item.description}</Text> : null}
      </Space>
    ),
    value: item.id,
  }));
  const departmentOptions = (departmentOptionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));
  const jobTitleOptions = (jobTitleOptionsQuery.data?.items ?? []).map((item) => ({ label: item.name, value: item.id }));

  const invitationColumns: ColumnsType<Invitation> = [
    {
      title: t("admin_users.columns.email"),
      dataIndex: "email",
      sorter: true,
    },
    {
      title: t("admin_users.columns.invited_by"),
      dataIndex: "invitedBy",
    },
    {
      title: t("admin_users.columns.department"),
      dataIndex: "departmentName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.job_title"),
      dataIndex: "jobTitleName",
      render: (value: string | null) => value || "-",
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: Invitation["status"]) => (
        <Tag
          color={
            status === "Pending"
              ? "gold"
              : status === "Accepted"
                ? "green"
                : status === "Cancelled"
                  ? "volcano"
                  : status === "Rejected"
                    ? "red"
                    : "default"
          }
        >
          {status}
        </Tag>
      ),
    },
    {
      title: t("admin_users.columns.expires_at"),
      dataIndex: "expiresAt",
      sorter: true,
      render: (value: string | null) => formatDate(value, currentLanguage),
    },
    {
      title: t("admin_users.columns.actions"),
      key: "actions",
      render: () => null,
    },
  ];

  let pageTitle = t("admin_users.directory.page_title");
  let pageDescription = t("admin_users.directory.page_description");
  let pageContent: ReactNode;

  if (currentSection === "invitations") {
    pageTitle = t("admin_users.invitations.page_title");
    pageDescription = t("admin_users.invitations.page_description");
    pageContent = (
      <Suspense fallback={null}>
        <AdminInvitationsSection
          cancelInvitationLoading={cancelInvitationMutation.isPending}
          columns={invitationColumns}
          data={invitationsQuery.data?.items ?? []}
          loading={invitationsQuery.isLoading}
          paging={invitationPaging}
          pagination={invitationsQuery.data}
          setCreatingInvitation={setCreatingInvitation}
          setEditingInvitation={setEditingInvitation}
          setPaging={setInvitationPaging}
          setViewingInvitation={setViewingInvitation}
          t={t}
          onPrefillInvitationEdit={(record) => {
            editInvitationForm.setFieldsValue({
              email: record.email,
              departmentId: record.departmentId ?? undefined,
              jobTitleId: record.jobTitleId ?? undefined,
              expiresAt: record.expiresAt ? dayjs(record.expiresAt) : undefined,
            });
          }}
          onCancelInvitation={(record) => {
            Modal.confirm({
              title: t("admin_users.invitations.cancel_title", { email: record.email }),
              content: t("admin_users.invitations.cancel_description"),
              okText: t("common.actions.confirm"),
              cancelText: t("common.actions.cancel"),
              okButtonProps: { danger: true },
              onOk: () =>
                new Promise<void>((resolve, reject) => {
                  cancelInvitationMutation.mutate(record.id, {
                    onSuccess: () => {
                      handleSuccess(t("admin_users.messages.invitation_cancelled", { email: record.email }));
                      resolve();
                    },
                    onError: (error) => {
                      handleError(t("errors.cancel_invitation_failed"), error);
                      reject(error);
                    },
                  });
                }),
            });
          }}
        />
      </Suspense>
    );
  } else if (currentSection === "approvals") {
    pageTitle = t("admin_users.registration.page_title");
    pageDescription = t("admin_users.registration.page_description");
    pageContent = (
      <Suspense fallback={null}>
        <AdminRegistrationsSection
          currentLanguage={currentLanguage}
          data={registrationRequestsQuery.data?.items ?? []}
          loading={registrationRequestsQuery.isLoading}
          paging={registrationPaging}
          pagination={registrationRequestsQuery.data}
          reviewRegistrationForm={reviewRegistrationForm}
          setManagingRegistration={setManagingRegistration}
          setPaging={setRegistrationPaging}
          setViewingRegistrationLink={setViewingRegistrationLink}
          t={t}
        />
      </Suspense>
    );
  } else {
    if (currentSection === "master-departments") {
      pageTitle = t("admin_users.master.page_title");
      pageDescription = t("admin_users.master.departments_page_description");
      pageContent = (
        <Suspense fallback={null}>
          <AdminMasterDataSection
            createLabel={t("admin_users.master.create_department")}
            data={departmentsQuery.data?.items ?? []}
            deleting={deleteDepartmentMutation.isPending}
            description={t("admin_users.master.departments_description")}
            itemLabel={t("admin_users.columns.department")}
            loading={departmentsQuery.isLoading}
            paging={departmentPaging}
            pagination={departmentsQuery.data}
            searchPlaceholder={t("admin_users.placeholders.search_departments")}
            setCreating={setCreatingDepartment}
            setDeleting={setDeletingDepartment}
            setEditing={setEditingDepartment}
            setPaging={setDepartmentPaging}
            t={t}
            title={t("admin_users.master.departments_title")}
            onDeletePrepare={() => {
              deleteDepartmentForm.resetFields();
            }}
            onEdit={(item) => {
              editDepartmentForm.setFieldValue("editName", item.name);
              editDepartmentForm.setFieldValue("editDisplayOrder", item.displayOrder);
            }}
          />
        </Suspense>
      );
    } else if (currentSection === "master-job-titles") {
      pageTitle = t("admin_users.master.page_title");
      pageDescription = t("admin_users.master.job_titles_page_description");
      pageContent = (
        <Suspense fallback={null}>
          <AdminMasterDataSection
            createLabel={t("admin_users.master.create_job_title")}
            data={jobTitlesQuery.data?.items ?? []}
            deleting={deleteJobTitleMutation.isPending}
            description={t("admin_users.master.job_titles_description")}
            itemLabel={t("admin_users.columns.job_title")}
            loading={jobTitlesQuery.isLoading}
            paging={jobTitlePaging}
            pagination={jobTitlesQuery.data}
            searchPlaceholder={t("admin_users.placeholders.search_job_titles")}
            setCreating={setCreatingJobTitle}
            setDeleting={setDeletingJobTitle}
            setEditing={setEditingJobTitle}
            setPaging={setJobTitlePaging}
            t={t}
            title={t("admin_users.master.job_titles_title")}
            onDeletePrepare={() => {
              deleteJobTitleForm.resetFields();
            }}
            onEdit={(item) => {
              editJobTitleForm.setFieldValue("editName", item.name);
              editJobTitleForm.setFieldValue("editDisplayOrder", item.displayOrder);
            }}
          />
        </Suspense>
      );
    } else {
      pageContent = (
        <Suspense fallback={null}>
          <AdminUsersDirectorySection
            currentLanguage={currentLanguage}
            data={usersQuery.data?.items ?? []}
            deleteUserForm={deleteUserForm}
            deleteUserLoading={deleteUserMutation.isPending}
            editUserForm={editUserForm}
            loading={usersQuery.isLoading}
            paging={usersPaging}
            pagination={usersQuery.data}
            roleItems={rolesQuery.data ?? []}
            setCreatingUser={setCreatingUser}
            setDeletingUser={setDeletingUser}
            setEditingUser={setEditingUser}
            setPaging={setUsersPaging}
            t={t}
          />
        </Suspense>
      );
    }
  }

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div
            style={{
              width: 48,
              height: 48,
              borderRadius: 14,
              display: "grid",
              placeItems: "center",
              background: "linear-gradient(135deg, #0ea5e9, #1d4ed8)",
              color: "#fff",
            }}
          >
            <TeamOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {pageTitle}
            </Typography.Title>
            <Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {pageDescription}
            </Paragraph>
          </div>
        </Space>
      </Card>

      {(usersQuery.error || registrationRequestsQuery.error || invitationsQuery.error) ? (
        <Alert
          type="error"
          showIcon
          message={t("errors.load_admin_data")}
          description={(() => {
            const sourceError = usersQuery.error ?? registrationRequestsQuery.error ?? invitationsQuery.error;
            const presentation = getApiErrorPresentation(sourceError, t("errors.load_admin_data"));
            return presentation.description;
          })()}
        />
      ) : null}

      {pageContent}

      {(managingRegistration !== null || viewingRegistrationLink !== null) ? (
        <Suspense fallback={null}>
          <AdminRegistrationModals
            approveLoading={approveRegistrationMutation.isPending}
            currentLanguage={currentLanguage}
            formatDate={formatDate}
            linkItem={viewingRegistrationLink}
            manageItem={managingRegistration}
            onApprove={() => {
              if (!managingRegistration) {
                return;
              }

              reviewRegistrationForm
                .validateFields()
                .then(() => {
                  approveRegistrationMutation.mutate(
                    { requestId: managingRegistration.id, input: { reviewedBy: actor } },
                    {
                      onSuccess: (approvedRequest) => {
                        handleSuccess(t("admin_users.messages.registration_approved", { email: managingRegistration.email }));
                        setViewingRegistrationLink(approvedRequest);
                        setManagingRegistration(null);
                        reviewRegistrationForm.resetFields();
                      },
                      onError: (error) => handleError(t("errors.approve_registration_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onCloseLink={() => {
              setViewingRegistrationLink(null);
            }}
            onCloseManage={() => {
              setManagingRegistration(null);
              reviewRegistrationForm.resetFields();
            }}
            onCopyLink={() => {
              if (!viewingRegistrationLink?.passwordSetupLink) {
                return;
              }

              const registrationUrl = `${window.location.origin}${viewingRegistrationLink.passwordSetupLink}`;
              void copyToClipboard(
                registrationUrl,
                t("admin_users.messages.registration_link_copied", { email: viewingRegistrationLink.email })
              );
            }}
            onReject={() => {
              if (!managingRegistration) {
                return;
              }

              reviewRegistrationForm
                .validateFields()
                .then((values: { reason?: string }) => {
                  const reason = values.reason?.trim() || t("admin_users.registration.default_rejection_reason");
                  rejectRegistrationMutation.mutate(
                    { requestId: managingRegistration.id, input: { reviewedBy: actor, reason } },
                    {
                      onSuccess: () => {
                        handleSuccess(t("admin_users.messages.registration_rejected", { email: managingRegistration.email }));
                        setManagingRegistration(null);
                        reviewRegistrationForm.resetFields();
                      },
                      onError: (error) => handleError(t("errors.reject_registration_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            rejectLoading={rejectRegistrationMutation.isPending}
            reviewForm={reviewRegistrationForm}
            t={t}
          />
        </Suspense>
      ) : null}

      {(creatingInvitation || editingInvitation !== null || viewingInvitation !== null) ? (
        <Suspense fallback={null}>
          <AdminInvitationModals
            createLoading={createInvitationMutation.isPending}
            creatingInvitation={creatingInvitation}
            departmentOptions={departmentOptions}
            editForm={editInvitationForm}
            editingInvitation={editingInvitation}
            invitationForm={inviteForm}
            jobTitleOptions={jobTitleOptions}
            onCloseEdit={() => {
              setEditingInvitation(null);
              editInvitationForm.resetFields();
            }}
            onCloseView={() => {
              setViewingInvitation(null);
            }}
            onCopyViewLink={() => {
              if (!viewingInvitation) {
                return;
              }

              const invitationUrl = `${window.location.origin}${viewingInvitation.invitationLink}`;
              void copyToClipboard(
                invitationUrl,
                t("admin_users.messages.invitation_link_copied", { email: viewingInvitation.email })
              );
            }}
            onCreate={() => {
              inviteForm
                .validateFields()
                .then((values: { email: string; departmentId?: string; jobTitleId?: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
                  createInvitationMutation.mutate(
                    {
                      email: values.email,
                      invitedBy: actor,
                      departmentId: values.departmentId,
                      jobTitleId: values.jobTitleId,
                      expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                    },
                    {
                      onSuccess: () => {
                        setCreatingInvitation(false);
                        inviteForm.resetFields();
                        handleSuccess(t("admin_users.messages.invitation_sent", { email: values.email }));
                      },
                      onError: (error) => handleError(t("errors.invite_user_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onEdit={() => {
              if (!editingInvitation) {
                return;
              }

              editInvitationForm
                .validateFields()
                .then((values: { email: string; departmentId?: string; jobTitleId?: string; expiresAt?: { endOf: (unit: string) => { toISOString: () => string } } }) => {
                  updateInvitationMutation.mutate(
                    {
                      id: editingInvitation.id,
                      email: values.email,
                      departmentId: values.departmentId,
                      jobTitleId: values.jobTitleId,
                      expiresAt: values.expiresAt ? values.expiresAt.endOf("day").toISOString() : undefined,
                    },
                    {
                      onSuccess: () => {
                        setEditingInvitation(null);
                        editInvitationForm.resetFields();
                        handleSuccess(t("admin_users.messages.invitation_updated", { email: values.email }));
                      },
                      onError: (error) => handleError(t("errors.update_invitation_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onOpenChangeCreate={setCreatingInvitation}
            t={t}
            updateLoading={updateInvitationMutation.isPending}
            viewingInvitation={viewingInvitation}
          />
        </Suspense>
      ) : null}

      {(creatingUser || editingUser !== null || deletingUser !== null) ? (
        <Suspense fallback={null}>
          <AdminUserModals
            createForm={createUserForm}
            createLoading={createUserMutation.isPending}
            creatingUser={creatingUser}
            deleteForm={deleteUserForm}
            deleteLoading={deleteUserMutation.isPending}
            deletingUser={deletingUser}
            departmentOptions={departmentOptions}
            departmentsLoading={departmentsQuery.isLoading}
            editForm={editUserForm}
            editLoading={updateUserMutation.isPending}
            editingUser={editingUser}
            jobTitleOptions={jobTitleOptions}
            jobTitlesLoading={jobTitlesQuery.isLoading}
            onCloseCreate={() => {
              setCreatingUser(false);
              createUserForm.resetFields();
            }}
            onCloseDelete={() => {
              setDeletingUser(null);
              deleteUserForm.resetFields();
            }}
            onCloseEdit={() => {
              setEditingUser(null);
              editUserForm.resetFields();
            }}
            onCreate={() => {
              createUserForm
                .validateFields()
                .then((values: {
                  email: string;
                  firstName: string;
                  lastName: string;
                  password: string;
                  confirmPassword: string;
                  departmentId?: string;
                  jobTitleId?: string;
                  roles?: string[];
                }) => {
                  createUserMutation.mutate(
                    {
                      email: values.email,
                      firstName: values.firstName,
                      lastName: values.lastName,
                      password: values.password,
                      confirmPassword: values.confirmPassword,
                      createdBy: actor,
                      departmentId: values.departmentId,
                      jobTitleId: values.jobTitleId,
                      roleIds: values.roles,
                    },
                    {
                      onSuccess: () => {
                        setCreatingUser(false);
                        createUserForm.resetFields();
                        handleSuccess(t("admin_users.messages.user_created", { email: values.email }));
                      },
                      onError: (error) => handleError(t("errors.create_user_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onDelete={() => {
              if (!deletingUser) {
                return;
              }

              deleteUserForm
                .validateFields(["reason"])
                .then((values: { reason: string }) => {
                  deleteUserMutation.mutate(
                    { id: deletingUser.id, reason: values.reason },
                    {
                      onSuccess: () => {
                        handleSuccess(t("admin_users.messages.user_deleted", { email: deletingUser.keycloak?.email || deletingUser.id }));
                        setDeletingUser(null);
                        deleteUserForm.resetFields();
                      },
                      onError: (error) => handleError(t("errors.delete_user_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onEdit={() => {
              editUserForm
                .validateFields()
                .then((values: {
                  email: string;
                  firstName: string;
                  lastName: string;
                  departmentId?: string;
                  jobTitleId?: string;
                  roleIds?: string[];
                }) => {
                  if (!editingUser) {
                    return;
                  }

                  updateUserMutation.mutate(
                    {
                      id: editingUser.id,
                      email: values.email,
                      firstName: values.firstName,
                      lastName: values.lastName,
                      departmentId: values.departmentId,
                      jobTitleId: values.jobTitleId,
                      roleIds: values.roleIds,
                    },
                    {
                      onSuccess: () => {
                        handleSuccess(t("admin_users.messages.user_updated", { email: values.email }));
                        setEditingUser(null);
                        editUserForm.resetFields();
                      },
                      onError: (error) => handleError(t("errors.update_user_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            roleOptions={userRoleOptions}
            rolesLoading={rolesQuery.isLoading}
            t={t}
          />
        </Suspense>
      ) : null}

      {(creatingDepartment || editingDepartment !== null || deletingDepartment !== null || creatingJobTitle || editingJobTitle !== null || deletingJobTitle !== null) ? (
        <Suspense fallback={null}>
          <AdminMasterDataModals
            createDepartmentForm={createDepartmentForm}
            createDepartmentLoading={createDepartmentMutation.isPending}
            createJobTitleForm={createJobTitleForm}
            createJobTitleLoading={createJobTitleMutation.isPending}
            creatingDepartment={creatingDepartment}
            creatingJobTitle={creatingJobTitle}
            deleteDepartmentForm={deleteDepartmentForm}
            deleteDepartmentLoading={deleteDepartmentMutation.isPending}
            deleteJobTitleForm={deleteJobTitleForm}
            deleteJobTitleLoading={deleteJobTitleMutation.isPending}
            deletingDepartment={deletingDepartment}
            deletingJobTitle={deletingJobTitle}
            editDepartmentForm={editDepartmentForm}
            editDepartmentLoading={updateDepartmentMutation.isPending}
            editJobTitleForm={editJobTitleForm}
            editJobTitleLoading={updateJobTitleMutation.isPending}
            editingDepartment={editingDepartment}
            editingJobTitle={editingJobTitle}
            onCloseCreateDepartment={() => {
              setCreatingDepartment(false);
              createDepartmentForm.resetFields();
            }}
            onCloseCreateJobTitle={() => {
              setCreatingJobTitle(false);
              createJobTitleForm.resetFields();
            }}
            onCloseDeleteDepartment={() => {
              setDeletingDepartment(null);
              deleteDepartmentForm.resetFields();
            }}
            onCloseDeleteJobTitle={() => {
              setDeletingJobTitle(null);
              deleteJobTitleForm.resetFields();
            }}
            onCloseEditDepartment={() => {
              setEditingDepartment(null);
              editDepartmentForm.resetFields();
            }}
            onCloseEditJobTitle={() => {
              setEditingJobTitle(null);
              editJobTitleForm.resetFields();
            }}
            onCreateDepartment={() => {
              createDepartmentForm
                .validateFields(["name", "displayOrder"])
                .then((values: { displayOrder: number; name: string }) => {
                  createDepartmentMutation.mutate(
                    { name: values.name, displayOrder: values.displayOrder },
                    {
                      onSuccess: () => {
                        setCreatingDepartment(false);
                        createDepartmentForm.resetFields();
                        handleSuccess(t("admin_users.messages.department_created", { name: values.name }));
                      },
                      onError: (error) => handleError(t("errors.create_department_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onCreateJobTitle={() => {
              createJobTitleForm
                .validateFields(["name", "displayOrder"])
                .then((values: { displayOrder: number; name: string }) => {
                  createJobTitleMutation.mutate(
                    { name: values.name, displayOrder: values.displayOrder },
                    {
                      onSuccess: () => {
                        setCreatingJobTitle(false);
                        createJobTitleForm.resetFields();
                        handleSuccess(t("admin_users.messages.job_title_created", { name: values.name }));
                      },
                      onError: (error) => handleError(t("errors.create_job_title_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onDeleteDepartment={() => {
              if (!deletingDepartment) {
                return;
              }

              deleteDepartmentForm
                .validateFields(["reason"])
                .then((values: { reason: string }) => {
                  deleteDepartmentMutation.mutate(
                    { id: deletingDepartment.id, reason: values.reason },
                    {
                      onSuccess: () => {
                        setDeletingDepartment(null);
                        deleteDepartmentForm.resetFields();
                        handleSuccess(t("admin_users.messages.department_deleted", { name: deletingDepartment.name }));
                      },
                      onError: (error) => handleError(t("errors.delete_department_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onDeleteJobTitle={() => {
              if (!deletingJobTitle) {
                return;
              }

              deleteJobTitleForm
                .validateFields(["reason"])
                .then((values: { reason: string }) => {
                  deleteJobTitleMutation.mutate(
                    { id: deletingJobTitle.id, reason: values.reason },
                    {
                      onSuccess: () => {
                        setDeletingJobTitle(null);
                        deleteJobTitleForm.resetFields();
                        handleSuccess(t("admin_users.messages.job_title_deleted", { name: deletingJobTitle.name }));
                      },
                      onError: (error) => handleError(t("errors.delete_job_title_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onEditDepartment={() => {
              editDepartmentForm
                .validateFields(["editName", "editDisplayOrder"])
                .then((values: { editDisplayOrder: number; editName: string }) => {
                  if (!editingDepartment) {
                    return;
                  }

                  updateDepartmentMutation.mutate(
                    { id: editingDepartment.id, name: values.editName, displayOrder: values.editDisplayOrder },
                    {
                      onSuccess: () => {
                        setEditingDepartment(null);
                        handleSuccess(t("admin_users.messages.department_updated", { name: values.editName }));
                      },
                      onError: (error) => handleError(t("errors.update_department_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            onEditJobTitle={() => {
              editJobTitleForm
                .validateFields(["editName", "editDisplayOrder"])
                .then((values: { editDisplayOrder: number; editName: string }) => {
                  if (!editingJobTitle) {
                    return;
                  }

                  updateJobTitleMutation.mutate(
                    { id: editingJobTitle.id, name: values.editName, displayOrder: values.editDisplayOrder },
                    {
                      onSuccess: () => {
                        setEditingJobTitle(null);
                        handleSuccess(t("admin_users.messages.job_title_updated", { name: values.editName }));
                      },
                      onError: (error) => handleError(t("errors.update_job_title_failed"), error),
                    }
                  );
                })
                .catch(() => undefined);
            }}
            t={t}
          />
        </Suspense>
      ) : null}
    </Space>
  );
}
