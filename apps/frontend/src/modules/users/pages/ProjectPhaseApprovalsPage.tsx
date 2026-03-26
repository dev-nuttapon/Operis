import { useEffect, useMemo, useState } from "react";
import { Alert, App, Button, Card, Form, Grid, Input, Modal, Select, Skeleton, Space, Table, Tag, Typography, Flex } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { CheckOutlined, ClockCircleOutlined, CloseOutlined, PlusOutlined, SafetyCertificateOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { getApiErrorPresentation } from "../../../shared/lib/apiClient";
import { permissions } from "../../../shared/authz/permissions";
import { usePermissions } from "../../../shared/authz/usePermissions";
import { useProjectAdmin } from "../hooks/useProjectAdmin";
import { useProjectOptions } from "../hooks/useProjectOptions";
import type { PhaseApprovalRequest } from "../types/users";
import { useDebouncedValue } from "../../../shared/hooks/useDebouncedValue";
import { ActionMenu } from "../../../shared/components/ActionMenu";
import { formatDate, toApiSortOrder } from "../utils/adminUsersPresentation";

type CreateFormValues = {
  phaseCode: string;
  entryCriteriaSummary: string;
  requiredEvidenceRefs: string;
};

type DecisionFormValues = {
  decisionReason: string;
};

export function ProjectPhaseApprovalsPage() {
  const { t, i18n } = useTranslation();
  const { notification } = App.useApp();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const permissionState = usePermissions();
  const canReadProjects = permissionState.hasPermission(permissions.projects.read);
  const canManageProjects = permissionState.hasPermission(permissions.projects.manage);
  const canManageMembers = permissionState.hasPermission(permissions.projects.manageMembers);
  const canApprovePhase = permissionState.hasPermission(permissions.projects.approvePhase);
  const [selectedProjectId, setSelectedProjectId] = useState<string>();
  const [paging, setPaging] = useState({
    page: 1,
    pageSize: 10,
    search: "",
    status: "",
    sortBy: "createdAt",
    sortOrder: "desc" as "asc" | "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [decisionTarget, setDecisionTarget] = useState<PhaseApprovalRequest | null>(null);
  const [decisionAction, setDecisionAction] = useState<"approve" | "reject" | "baseline" | null>(null);
  const [createForm] = Form.useForm<CreateFormValues>();
  const [decisionForm] = Form.useForm<DecisionFormValues>();
  const debouncedSearch = useDebouncedValue(searchInput, 300);

  useEffect(() => {
    setPaging((current) => ({ ...current, page: 1, search: debouncedSearch }));
  }, [debouncedSearch]);

  const projectOptionsState = useProjectOptions({ enabled: canReadProjects });
  const {
    phaseApprovalsQuery,
    createPhaseApprovalMutation,
    submitPhaseApprovalMutation,
    approvePhaseApprovalMutation,
    rejectPhaseApprovalMutation,
    baselinePhaseApprovalMutation,
  } = useProjectAdmin({
    projectsEnabled: false,
    projects: { page: 1, pageSize: 1 },
    projectRoles: { page: 1, pageSize: 1 },
    projectAssignments: null,
    phaseApprovals: selectedProjectId ? { projectId: selectedProjectId, ...paging, search: debouncedSearch } : null,
  });

  const data = phaseApprovalsQuery.data as { items?: PhaseApprovalRequest[]; page?: number; pageSize?: number; total?: number } | undefined;

  const handleError = (fallbackTitle: string, error: unknown) => {
    const presentation = getApiErrorPresentation(error, fallbackTitle);
    notification.error({ message: presentation.title, description: presentation.description });
  };

  const columns = useMemo<ColumnsType<PhaseApprovalRequest>>(
    () => [
      {
        title: t("project_phase_approvals.columns.phase"),
        dataIndex: "phaseCode",
        sorter: true,
      },
      {
        title: t("project_phase_approvals.columns.status"),
        dataIndex: "status",
        render: (value: string) => {
          const color = value === "Approved" ? "green" : value === "Rejected" ? "red" : value === "Baseline" ? "blue" : value === "Submitted" ? "gold" : "default";
          return <Tag color={color}>{value}</Tag>;
        },
      },
      {
        title: t("project_phase_approvals.columns.submitted_by"),
        dataIndex: "submittedByDisplayName",
        render: (_, record) => record.submittedByDisplayName ?? record.submittedBy ?? "-",
      },
      {
        title: t("project_phase_approvals.columns.submitted_at"),
        dataIndex: "submittedAt",
        render: (value: string | null) => formatDate(value, i18n.language),
      },
      {
        title: t("project_phase_approvals.columns.decision"),
        dataIndex: "decision",
        render: (value: string | null, record) => value ?? record.status,
      },
      {
        title: t("admin_users.columns.actions"),
        key: "actions",
        render: (_, record) => {
          const items = [];
          if ((canManageProjects || canManageMembers) && record.status === "Draft") {
            items.push({
              key: "submit",
              icon: <ClockCircleOutlined />,
              label: t("project_phase_approvals.actions.submit"),
              onClick: () =>
                submitPhaseApprovalMutation.mutate(record.id, {
                  onSuccess: () => notification.success({ message: t("project_phase_approvals.messages.submitted", { phase: record.phaseCode }) }),
                  onError: (error) => handleError(t("project_phase_approvals.messages.submit_failed"), error),
                }),
            });
          }
          if (canApprovePhase && record.status === "Submitted") {
            items.push(
              {
                key: "approve",
                icon: <CheckOutlined />,
                label: t("project_phase_approvals.actions.approve"),
                onClick: () => {
                  setDecisionTarget(record);
                  setDecisionAction("approve");
                  decisionForm.resetFields();
                },
              },
              {
                key: "reject",
                icon: <CloseOutlined />,
                label: t("project_phase_approvals.actions.reject"),
                danger: true,
                onClick: () => {
                  setDecisionTarget(record);
                  setDecisionAction("reject");
                  decisionForm.resetFields();
                },
              },
            );
          }
          if (canApprovePhase && record.status === "Approved") {
            items.push({
              key: "baseline",
              icon: <SafetyCertificateOutlined />,
              label: t("project_phase_approvals.actions.baseline"),
              onClick: () => {
                setDecisionTarget(record);
                setDecisionAction("baseline");
                decisionForm.resetFields();
              },
            });
          }

          return items.length > 0 ? <ActionMenu items={items} /> : null;
        },
      },
    ],
    [canApprovePhase, canManageMembers, canManageProjects, decisionForm, i18n.language, notification, submitPhaseApprovalMutation, t],
  );

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Space align="start" size={16}>
          <div style={{ width: 48, height: 48, borderRadius: 14, display: "grid", placeItems: "center", background: "linear-gradient(135deg, #0f766e, #115e59)", color: "#fff" }}>
            <SafetyCertificateOutlined />
          </div>
          <div>
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t("project_phase_approvals.page_title")}
            </Typography.Title>
            <Typography.Paragraph type="secondary" style={{ margin: "4px 0 0" }}>
              {t("project_phase_approvals.page_description")}
            </Typography.Paragraph>
          </div>
        </Space>
      </Card>

      <Card variant="borderless">
        <Space direction="vertical" size={16} style={{ width: "100%" }}>
          <Select
            allowClear
            showSearch
            filterOption={false}
            placeholder={t("project_phase_approvals.select_project_placeholder")}
            options={projectOptionsState.options}
            value={selectedProjectId}
            onSearch={projectOptionsState.onSearch}
            loading={projectOptionsState.loading}
            onChange={(value) => {
              setSelectedProjectId(value);
              setPaging((current) => ({ ...current, page: 1 }));
            }}
            dropdownRender={(menu) => (
              <>
                {menu}
                {projectOptionsState.hasMore ? (
                  <div style={{ padding: 8 }}>
                    <button
                      type="button"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => projectOptionsState.onLoadMore()}
                      style={{ width: "100%", border: "none", background: "transparent", color: "#1677ff", cursor: "pointer", padding: 4 }}
                    >
                      {t("projects.load_more_projects")}
                    </button>
                  </div>
                ) : null}
              </>
            )}
          />

          {!canReadProjects ? (
            <Alert type="warning" showIcon message={t("errors.title_forbidden")} />
          ) : !selectedProjectId ? (
            <Alert type="info" showIcon message={t("project_phase_approvals.select_project_message")} />
          ) : (
            <>
              <Flex gap={12} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"} justify="space-between">
                <Input.Search
                  allowClear
                  placeholder={t("project_phase_approvals.search_placeholder")}
                  value={searchInput}
                  onChange={(event) => setSearchInput(event.target.value)}
                  onSearch={(value) => setSearchInput(value)}
                  style={{ width: isMobile ? "100%" : undefined, maxWidth: isMobile ? undefined : 360 }}
                />
                <Flex gap={8} wrap={!isMobile} vertical={isMobile} align={isMobile ? "stretch" : "center"}>
                  <Select
                    allowClear
                    value={paging.status || undefined}
                    placeholder={t("project_phase_approvals.status_placeholder")}
                    options={[
                      { label: t("project_phase_approvals.status.draft"), value: "Draft" },
                      { label: t("project_phase_approvals.status.submitted"), value: "Submitted" },
                      { label: t("project_phase_approvals.status.approved"), value: "Approved" },
                      { label: t("project_phase_approvals.status.rejected"), value: "Rejected" },
                      { label: t("project_phase_approvals.status.baseline"), value: "Baseline" },
                    ]}
                    onChange={(value) => setPaging((current) => ({ ...current, status: value ?? "", page: 1 }))}
                    style={{ width: isMobile ? "100%" : 180 }}
                  />
                  {(canManageProjects || canManageMembers) ? (
                    <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)} block={isMobile}>
                      {t("project_phase_approvals.create_action")}
                    </Button>
                  ) : null}
                </Flex>
              </Flex>

              {phaseApprovalsQuery.isLoading && (Array.isArray(data?.items) ? data.items.length : 0) === 0 ? (
                <Skeleton active paragraph={{ rows: 6 }} />
              ) : (
                <Table
                  rowKey="id"
                  columns={columns}
                  dataSource={Array.isArray(data?.items) ? data.items : []}
                  loading={phaseApprovalsQuery.isLoading}
                  scroll={{ x: "max-content" }}
                  pagination={{
                    current: data?.page ?? paging.page,
                    pageSize: data?.pageSize ?? paging.pageSize,
                    total: data?.total ?? 0,
                    showSizeChanger: true,
                    pageSizeOptions: [10, 25, 50, 100],
                  }}
                  onChange={(nextPagination, _, sorter) => {
                    const resolvedSorter = sorter as SorterResult<PhaseApprovalRequest>;
                    setPaging((current) => ({
                      ...current,
                      page: nextPagination.current ?? current.page,
                      pageSize: nextPagination.pageSize ?? current.pageSize,
                      sortBy: typeof resolvedSorter.field === "string" ? resolvedSorter.field : current.sortBy,
                      sortOrder: toApiSortOrder(resolvedSorter.order) ?? current.sortOrder,
                    }));
                  }}
                />
              )}
            </>
          )}
        </Space>
      </Card>

      <Modal
        title={t("project_phase_approvals.create_modal_title")}
        open={createOpen}
        onCancel={() => {
          setCreateOpen(false);
          createForm.resetFields();
        }}
        onOk={() => {
          createForm.validateFields().then((values) => {
            if (!selectedProjectId) {
              return;
            }
            createPhaseApprovalMutation.mutate(
              {
                projectId: selectedProjectId,
                phaseCode: values.phaseCode,
                entryCriteriaSummary: values.entryCriteriaSummary,
                requiredEvidenceRefs: values.requiredEvidenceRefs.split(/\r?\n/).map((value) => value.trim()).filter(Boolean),
              },
              {
                onSuccess: () => {
                  setCreateOpen(false);
                  createForm.resetFields();
                  notification.success({ message: t("project_phase_approvals.messages.created", { phase: values.phaseCode }) });
                },
                onError: (error) => handleError(t("project_phase_approvals.messages.create_failed"), error),
              },
            );
          }).catch(() => undefined);
        }}
        confirmLoading={createPhaseApprovalMutation.isPending}
      >
        <Form form={createForm} layout="vertical">
          <Form.Item name="phaseCode" label={t("project_phase_approvals.fields.phase")} rules={[{ required: true }]}>
            <Input placeholder={t("project_phase_approvals.placeholders.phase")} />
          </Form.Item>
          <Form.Item name="entryCriteriaSummary" label={t("project_phase_approvals.fields.entry_criteria")} rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder={t("project_phase_approvals.placeholders.entry_criteria")} />
          </Form.Item>
          <Form.Item name="requiredEvidenceRefs" label={t("project_phase_approvals.fields.required_evidence")} rules={[{ required: true }]}>
            <Input.TextArea rows={5} placeholder={t("project_phase_approvals.placeholders.required_evidence")} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={decisionAction ? t(`project_phase_approvals.${decisionAction}_modal_title`) : ""}
        open={decisionTarget !== null && decisionAction !== null}
        onCancel={() => {
          setDecisionTarget(null);
          setDecisionAction(null);
          decisionForm.resetFields();
        }}
        onOk={() => {
          if (!decisionTarget || !decisionAction) {
            return;
          }

          decisionForm.validateFields().then((values) => {
            const onSuccess = () => {
              setDecisionTarget(null);
              setDecisionAction(null);
              decisionForm.resetFields();
              notification.success({ message: t(`project_phase_approvals.messages.${decisionAction}d`, { phase: decisionTarget.phaseCode }) });
            };
            const onError = (error: unknown) => handleError(t(`project_phase_approvals.messages.${decisionAction}_failed`), error);

            if (decisionAction === "approve") {
              approvePhaseApprovalMutation.mutate({ id: decisionTarget.id, decisionReason: values.decisionReason }, { onSuccess, onError });
            } else if (decisionAction === "reject") {
              rejectPhaseApprovalMutation.mutate({ id: decisionTarget.id, decisionReason: values.decisionReason }, { onSuccess, onError });
            } else {
              baselinePhaseApprovalMutation.mutate({ id: decisionTarget.id, decisionReason: values.decisionReason }, { onSuccess, onError });
            }
          }).catch(() => undefined);
        }}
        confirmLoading={
          approvePhaseApprovalMutation.isPending ||
          rejectPhaseApprovalMutation.isPending ||
          baselinePhaseApprovalMutation.isPending
        }
      >
        <Form form={decisionForm} layout="vertical">
          <Form.Item name="decisionReason" label={t("project_phase_approvals.fields.decision_reason")} rules={[{ required: true }]}>
            <Input.TextArea rows={4} placeholder={t("project_phase_approvals.placeholders.decision_reason")} />
          </Form.Item>
        </Form>
      </Modal>
    </Space>
  );
}
