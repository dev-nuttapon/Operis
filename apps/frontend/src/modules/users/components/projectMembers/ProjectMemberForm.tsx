import { Checkbox, DatePicker, Form, Input, Select } from "antd";
import type { FormInstance } from "antd";
import type { TFunction } from "i18next";
import type { Dayjs } from "dayjs";

export type ProjectMemberFormValues = {
  projectId?: string;
  userId: string;
  projectRoleId: string;
  reportsToUserId?: string;
  isPrimary: boolean;
  period?: [Dayjs | null, Dayjs | null];
  reason?: string;
};

export function ProjectMemberForm({
  form,
  t,
  showProjectField,
  disableProjectField,
  projectOptions,
  projectOptionsLoading,
  onProjectSearch,
  onProjectLoadMore,
  projectHasMore,
  userOptions,
  projectRoleOptions,
  reportingOptions,
  includeReason,
  userOptionsLoading,
  onUserSearch,
  onUserLoadMore,
  userHasMore,
  roleOptionsLoading,
  onRoleSearch,
  onRoleLoadMore,
  roleHasMore,
}: {
  form: FormInstance<ProjectMemberFormValues>;
  t: TFunction;
  showProjectField?: boolean;
  disableProjectField?: boolean;
  projectOptions?: { label: string; value: string }[];
  projectOptionsLoading?: boolean;
  onProjectSearch?: (value: string) => void;
  onProjectLoadMore?: () => void;
  projectHasMore?: boolean;
  userOptions: { label: string; value: string }[];
  projectRoleOptions: { label: string; value: string }[];
  reportingOptions: { label: string; value: string }[];
  includeReason: boolean;
  userOptionsLoading?: boolean;
  onUserSearch?: (value: string) => void;
  onUserLoadMore?: () => void;
  userHasMore?: boolean;
  roleOptionsLoading?: boolean;
  onRoleSearch?: (value: string) => void;
  onRoleLoadMore?: () => void;
  roleHasMore?: boolean;
}) {
  return (
    <Form form={form} layout="vertical" initialValues={{ isPrimary: false }}>
      {showProjectField ? (
        <Form.Item name="projectId" label={t("project_members.fields.project")} rules={[{ required: true }]}>
          <Select
          allowClear
          showSearch
          filterOption={false}
          options={projectOptions ?? []}
            placeholder={t("project_members.placeholders.project")}
            loading={projectOptionsLoading}
            onSearch={onProjectSearch}
            disabled={disableProjectField}
            dropdownRender={(menu) => (
              <>
                {menu}
                {projectHasMore ? (
                  <div style={{ padding: 8 }}>
                    <button
                      type="button"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => onProjectLoadMore?.()}
                      style={{
                        width: "100%",
                        border: "none",
                        background: "transparent",
                        color: "#1677ff",
                        cursor: "pointer",
                        padding: 4,
                      }}
                    >
                      {t("projects.load_more_projects")}
                    </button>
                  </div>
                ) : null}
              </>
            )}
          />
        </Form.Item>
      ) : null}
      <Form.Item name="userId" label={t("project_members.fields.user")} rules={[{ required: true }]}>
        <Select
          allowClear
          showSearch
          filterOption={false}
          options={userOptions}
          placeholder={t("project_members.placeholders.user")}
          loading={userOptionsLoading}
          onSearch={onUserSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {userHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onUserLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_users")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="projectRoleId" label={t("project_members.fields.project_role")} rules={[{ required: true }]}>
        <Select
          showSearch
          filterOption={false}
          options={projectRoleOptions}
          placeholder={t("project_members.placeholders.project_role")}
          loading={roleOptionsLoading}
          onSearch={onRoleSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {roleHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onRoleLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_roles")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="reportsToUserId" label={t("project_members.fields.reports_to")}>
        <Select
          allowClear
          showSearch
          filterOption={false}
          options={reportingOptions}
          placeholder={t("project_members.placeholders.reports_to")}
          loading={userOptionsLoading}
          onSearch={onUserSearch}
          dropdownRender={(menu) => (
            <>
              {menu}
              {userHasMore ? (
                <div style={{ padding: 8 }}>
                  <button
                    type="button"
                    onMouseDown={(event) => event.preventDefault()}
                    onClick={() => onUserLoadMore?.()}
                    style={{
                      width: "100%",
                      border: "none",
                      background: "transparent",
                      color: "#1677ff",
                      cursor: "pointer",
                      padding: 4,
                    }}
                  >
                    {t("projects.load_more_users")}
                  </button>
                </div>
              ) : null}
            </>
          )}
        />
      </Form.Item>
      <Form.Item name="period" label={t("project_members.fields.period")}>
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
      {includeReason ? (
        <Form.Item name="reason" label={t("project_members.fields.change_reason")} rules={[{ required: true, message: t("project_members.validation.change_reason_required") }]}>
          <Input.TextArea rows={4} placeholder={t("project_members.placeholders.change_reason")} />
        </Form.Item>
      ) : null}
      <Form.Item name="isPrimary" valuePropName="checked">
        <Checkbox>{t("project_members.fields.is_primary")}</Checkbox>
      </Form.Item>
    </Form>
  );
}
