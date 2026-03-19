import { useEffect, useState } from "react";
import { Button, Card, Collapse, DatePicker, Form, Input, Select, Space, Table, Tag, Typography, Skeleton, Flex, Grid } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { SorterResult } from "antd/es/table/interface";
import { DeleteOutlined, EditOutlined, UserAddOutlined } from "@ant-design/icons";
import { permissions } from "../../../../shared/authz/permissions";
import { usePermissions } from "../../../../shared/authz/usePermissions";
import dayjs from "dayjs";
import type { Dayjs } from "dayjs";
import { useLocation, useNavigate } from "react-router-dom";
import {
  formatDate,
  toApiSortOrder,
} from "../../utils/adminUsersPresentation";
import type { User, UserStatus } from "../../types/users";
import { useDepartmentFilterOptions } from "../../hooks/useDepartmentFilterOptions";
import { useDivisionOptions } from "../../hooks/useDivisionOptions";
import { useJobTitleOptions } from "../../hooks/useJobTitleOptions";
import { ActionMenu } from "../../../../shared/components/ActionMenu";

type AdvancedFilterValues = {
  status?: UserStatus;
  divisionId?: string;
  departmentId?: string;
  jobTitleId?: string;
  dateRange?: [Dayjs | null, Dayjs | null];
};

interface AdminUsersDirectorySectionProps {
  currentLanguage: string;
  data: User[];
  deleteUserLoading: boolean;
  loading: boolean;
  paging: {
    page: number;
    pageSize: number;
    search: string;
    status: UserStatus | undefined;
    divisionId: string | undefined;
    departmentId: string | undefined;
    jobTitleId: string | undefined;
    from: string | undefined;
    to: string | undefined;
    sortBy: string;
    sortOrder: "asc" | "desc";
  };
  pagination?: {
    page?: number;
    pageSize?: number;
    total?: number;
  };
  setDeletingUser: (user: User | null) => void;
  setPaging: (
    updater: (current: AdminUsersDirectorySectionProps["paging"]) => AdminUsersDirectorySectionProps["paging"]
  ) => void;
  t: (key: string, options?: Record<string, unknown>) => string;
  deleteUserForm: {
    resetFields: () => void;
  };
}

export function AdminUsersDirectorySection({
  currentLanguage,
  data,
  deleteUserForm,
  deleteUserLoading,
  loading,
  paging,
  pagination,
  setDeletingUser,
  setPaging,
  t,
}: AdminUsersDirectorySectionProps) {
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();
  const location = useLocation();
  const permissionState = usePermissions();
  const canCreateUsers = permissionState.hasPermission(permissions.users.create);
  const canUpdateUsers = permissionState.hasPermission(permissions.users.update);
  const canDeleteUsers = permissionState.hasPermission(permissions.users.delete);
  const canReadMasterData =
    permissionState.hasPermission(permissions.masterData.managePermanentOrg) || permissionState.hasPermission(permissions.users.read);

  const [searchDraft, setSearchDraft] = useState(paging.search);
  const [advancedOpen, setAdvancedOpen] = useState(false);
  const [advancedForm] = Form.useForm<AdvancedFilterValues>();

  const divisionOptionsState = useDivisionOptions({ enabled: canReadMasterData, pageSize: 5 });
  const selectedDivisionId = Form.useWatch("divisionId", advancedForm) as string | undefined;
  const selectedDepartmentId = Form.useWatch("departmentId", advancedForm) as string | undefined;
  const departmentOptionsState = useDepartmentFilterOptions({ enabled: canReadMasterData, divisionId: selectedDivisionId, pageSize: 5 });
  const jobTitleOptionsState = useJobTitleOptions({ enabled: canReadMasterData, departmentId: selectedDepartmentId, pageSize: 5 });

  useEffect(() => {
    setSearchDraft(paging.search);
  }, [paging.search]);

  useEffect(() => {
    if (!advancedOpen) return;
    advancedForm.setFieldsValue({
      status: paging.status,
      divisionId: paging.divisionId,
      departmentId: paging.departmentId,
      jobTitleId: paging.jobTitleId,
      dateRange: [paging.from ? dayjs(paging.from) : null, paging.to ? dayjs(paging.to) : null],
    });
  }, [advancedForm, advancedOpen, paging.departmentId, paging.divisionId, paging.from, paging.jobTitleId, paging.status, paging.to]);
  const columns: ColumnsType<User> = [
    {
      title: t("admin_users.columns.name"),
      key: "name",
      render: (_, record) => (
        <Space direction="vertical" size={0}>
          <Typography.Text strong>{`${record.keycloak?.firstName || "-"} ${record.keycloak?.lastName || ""}`.trim()}</Typography.Text>
          <Typography.Text type="secondary">{record.keycloak?.email || record.keycloak?.username || record.id || "-"}</Typography.Text>
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.status"),
      dataIndex: "status",
      sorter: true,
      render: (status: User["status"]) => (
        <Tag color={status === "Active" ? "green" : status === "Deleted" ? "red" : "default"}>
          {status}
        </Tag>
      ),
    },
    {
      title: t("admin_users.columns.division"),
      dataIndex: "divisionName",
      render: (value: string | null) => value || "-",
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
      title: t("admin_users.columns.roles"),
      dataIndex: "roles",
      render: (roles: string[]) => (
        <Space wrap>
          {roles.length === 0 ? <Typography.Text type="secondary">-</Typography.Text> : roles.map((role) => <Tag key={role}>{role}</Tag>)}
        </Space>
      ),
    },
    {
      title: t("admin_users.columns.created_at"),
      dataIndex: "createdAt",
      sorter: true,
      render: (value: string) => formatDate(value, currentLanguage),
    },
    {
      title: t("admin_users.columns.actions"),
      key: "actions",
      render: (_, record) => (
        <ActionMenu
          items={[
            {
              key: "edit",
              icon: <EditOutlined />,
              label: t("common.actions.edit"),
              disabled: !canUpdateUsers,
              onClick: () =>
                navigate(`/app/admin/users/${record.id}/edit`, {
                  state: { from: `${location.pathname}${location.search}` },
                }),
            },
            {
              key: "delete",
              icon: <DeleteOutlined />,
              label: t("common.actions.delete"),
              danger: true,
              disabled: !canDeleteUsers,
              onClick: () => {
                setDeletingUser(record);
                deleteUserForm.resetFields();
              },
            },
          ]}
          loading={deleteUserLoading}
        />
      ),
    },
  ];

  return (
    <Space direction="vertical" size={20} style={{ width: "100%" }}>
      <Card variant="borderless">
        <Typography.Title level={5}>{t("admin_users.directory.list_title")}</Typography.Title>
        <Flex
          gap={12}
          wrap={!isMobile}
          vertical={isMobile}
          align={isMobile ? "stretch" : "center"}
          justify="space-between"
          style={{ width: "100%", marginBottom: 16 }}
        >
          <Flex gap={8} vertical={isMobile} style={{ width: isMobile ? "100%" : undefined }}>
            <Input
              allowClear
              style={{ width: isMobile ? "100%" : 360 }}
              placeholder={t("admin_users.directory.search_name_or_email")}
              value={searchDraft}
              onChange={(event) => setSearchDraft(event.target.value)}
              onPressEnter={() => setPaging((current) => ({ ...current, page: 1, search: searchDraft.trim() }))}
            />
            <Flex gap={8} wrap>
              <Button
                onClick={() => setPaging((current) => ({ ...current, page: 1, search: searchDraft.trim() }))}
                block={isMobile}
              >
                {t("admin_users.directory.search_action")}
              </Button>
            </Flex>
          </Flex>
          {canCreateUsers ? (
            <Button
              type="primary"
              icon={<UserAddOutlined />}
              size="large"
              onClick={() => navigate("/app/admin/users/new", { state: { from: location.pathname } })}
              block={isMobile}
            >
              {t("admin_users.directory.create_user")}
            </Button>
          ) : null}
        </Flex>

        <Collapse
          activeKey={advancedOpen ? ["advanced"] : []}
          onChange={(keys) => setAdvancedOpen(Array.isArray(keys) ? keys.includes("advanced") : keys === "advanced")}
          style={{ marginTop: 12, marginBottom: 16 }}
          items={[
            {
              key: "advanced",
              label: t("admin_users.directory.advanced_search_title"),
              children: (
                <Form
                  form={advancedForm}
                  layout="vertical"
                  onFinish={(values) => {
                    const from = values.dateRange?.[0]?.startOf("day").toISOString();
                    const to = values.dateRange?.[1]?.endOf("day").toISOString();
                    setPaging((current) => ({
                      ...current,
                      page: 1,
                      status: values.status,
                      divisionId: values.divisionId,
                      departmentId: values.departmentId,
                      jobTitleId: values.jobTitleId,
                      from,
                      to,
                    }));
                    setAdvancedOpen(false);
                  }}
                >
                  <Form.Item label={t("admin_users.columns.status")} name="status">
                    <Select
                      allowClear
                      options={[
                        { value: "Active", label: "Active" },
                        { value: "Rejected", label: "Rejected" },
                        { value: "Deleted", label: "Deleted" },
                      ]}
                      placeholder={t("admin_users.placeholders.select_status")}
                    />
                  </Form.Item>

                  <Form.Item label={t("admin_users.columns.division")} name="divisionId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      options={divisionOptionsState.options}
                      loading={divisionOptionsState.loading}
                      placeholder={t("admin_users.placeholders.search_divisions")}
                      onSearch={divisionOptionsState.onSearch}
                      onChange={() => {
                        // changing division should reset dependent fields
                        advancedForm.setFieldValue("departmentId", undefined);
                        advancedForm.setFieldValue("jobTitleId", undefined);
                      }}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {divisionOptionsState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={() => divisionOptionsState.onLoadMore?.()}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_divisions")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                    />
                  </Form.Item>

                  <Form.Item label={t("admin_users.columns.department")} name="departmentId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      options={departmentOptionsState.options}
                      loading={departmentOptionsState.loading}
                      placeholder={t("admin_users.placeholders.search_departments")}
                      onSearch={departmentOptionsState.onSearch}
                      onChange={() => {
                        advancedForm.setFieldValue("jobTitleId", undefined);
                      }}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {departmentOptionsState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={() => departmentOptionsState.onLoadMore?.()}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_departments")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                    />
                  </Form.Item>

                  <Form.Item label={t("admin_users.columns.job_title")} name="jobTitleId">
                    <Select
                      allowClear
                      showSearch
                      filterOption={false}
                      disabled={!selectedDepartmentId}
                      options={jobTitleOptionsState.options}
                      loading={jobTitleOptionsState.loading}
                      placeholder={t("admin_users.placeholders.search_positions")}
                      onSearch={jobTitleOptionsState.onSearch}
                      dropdownRender={(menu) => (
                        <>
                          {menu}
                          {jobTitleOptionsState.hasMore ? (
                            <div style={{ padding: 8 }}>
                              <button
                                type="button"
                                onMouseDown={(event) => event.preventDefault()}
                                onClick={() => jobTitleOptionsState.onLoadMore?.()}
                                style={{
                                  width: "100%",
                                  border: "none",
                                  background: "transparent",
                                  color: "#1677ff",
                                  cursor: "pointer",
                                  padding: 4,
                                }}
                              >
                                {t("admin_users.load_more_job_titles")}
                              </button>
                            </div>
                          ) : null}
                        </>
                      )}
                    />
                  </Form.Item>

                  <Form.Item label={t("admin_users.directory.created_at_range")} name="dateRange">
                    <DatePicker.RangePicker style={{ width: "100%" }} />
                  </Form.Item>

                  <Flex gap={12} justify="flex-end">
                    <Button
                      onClick={() => {
                        advancedForm.resetFields();
                        setPaging((current) => ({
                          ...current,
                          page: 1,
                          status: undefined,
                          divisionId: undefined,
                          departmentId: undefined,
                          jobTitleId: undefined,
                          from: undefined,
                          to: undefined,
                        }));
                        setAdvancedOpen(false);
                      }}
                    >
                      {t("common.actions.reset")}
                    </Button>
                    <Button type="primary" htmlType="submit">
                      {t("common.actions.apply")}
                    </Button>
                  </Flex>
                </Form>
              ),
            },
          ]}
        />
        {loading && data.length === 0 ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <Table
            rowKey="id"
            columns={columns}
            dataSource={data}
            loading={loading}
            scroll={{ x: "max-content" }}
            pagination={{
              current: pagination?.page ?? paging.page,
              pageSize: pagination?.pageSize ?? paging.pageSize,
              total: pagination?.total ?? 0,
              showSizeChanger: true,
              pageSizeOptions: [10, 25, 50, 100],
            }}
            onChange={(nextPagination, _, sorter) => {
              const sort = sorter as SorterResult<User>;
              setPaging((current) => ({
                ...current,
                page: nextPagination.current ?? current.page,
                pageSize: nextPagination.pageSize ?? current.pageSize,
                sortBy: typeof sort.field === "string" ? sort.field : current.sortBy,
                sortOrder: toApiSortOrder(sort.order) ?? current.sortOrder,
              }));
            }}
          />
        )}
      </Card>
    </Space>
  );
}
