import { DatePicker, Form, Input, Select } from "antd";
import type { FormInstance } from "antd";
import dayjs from "dayjs";
import type { CreateProjectInput, Project } from "../../types/users";

export type ProjectFormValues = {
  code: string;
  name: string;
  projectType: string;
  ownerUserId?: string;
  sponsorUserId?: string;
  methodology?: string;
  phase?: string;
  status: string;
  statusReason?: string;
  plannedPeriod?: [dayjs.Dayjs | null, dayjs.Dayjs | null];
  actualPeriod?: [dayjs.Dayjs | null, dayjs.Dayjs | null];
};

export function normalizeProjectPayload(values: ProjectFormValues): Omit<CreateProjectInput, "id"> {
  return {
    code: values.code,
    name: values.name,
    projectType: values.projectType,
    ownerUserId: values.ownerUserId,
    sponsorUserId: values.sponsorUserId,
    methodology: values.methodology,
    phase: values.phase,
    status: values.status,
    statusReason: values.statusReason,
    plannedStartAt: values.plannedPeriod?.[0]?.startOf("day").toISOString(),
    plannedEndAt: values.plannedPeriod?.[1]?.endOf("day").toISOString(),
    startAt: values.actualPeriod?.[0]?.startOf("day").toISOString(),
    endAt: values.actualPeriod?.[1]?.endOf("day").toISOString(),
  };
}

export function toInitialValues(project: Project): ProjectFormValues {
  return {
    code: project.code,
    name: project.name,
    projectType: project.projectType,
    ownerUserId: project.ownerUserId ?? undefined,
    sponsorUserId: project.sponsorUserId ?? undefined,
    methodology: project.methodology ?? undefined,
    phase: project.phase ?? undefined,
    status: project.status,
    statusReason: project.statusReason ?? undefined,
    plannedPeriod:
      project.plannedStartAt || project.plannedEndAt
        ? [project.plannedStartAt ? dayjs(project.plannedStartAt) : null, project.plannedEndAt ? dayjs(project.plannedEndAt) : null]
        : undefined,
    actualPeriod:
      project.startAt || project.endAt
        ? [project.startAt ? dayjs(project.startAt) : null, project.endAt ? dayjs(project.endAt) : null]
        : undefined,
  };
}

export function ProjectForm({
  form,
  t,
  userOptions,
  projectTypeOptions,
  userOptionsLoading,
  onUserSearch,
  onUserLoadMore,
  userHasMore,
}: {
  form: FormInstance<ProjectFormValues>;
  t: (key: string) => string;
  userOptions: { label: string; value: string }[];
  projectTypeOptions: { label: string; value: string }[];
  userOptionsLoading?: boolean;
  onUserSearch?: (value: string) => void;
  onUserLoadMore?: () => void;
  userHasMore?: boolean;
}) {
  const projectStatusOptions = [
    { value: "planned", label: t("projects.options.status.planned") },
    { value: "active", label: t("projects.options.status.active") },
    { value: "onhold", label: t("projects.options.status.on_hold") },
    { value: "completed", label: t("projects.options.status.completed") },
    { value: "cancelled", label: t("projects.options.status.cancelled") },
  ];
  const methodologyOptions = [
    { value: "Agile", label: t("projects.options.methodology.agile") },
    { value: "Waterfall", label: t("projects.options.methodology.waterfall") },
    { value: "Hybrid", label: t("projects.options.methodology.hybrid") },
  ];
  const phaseOptions = [
    { value: "Initiation", label: t("projects.options.phase.initiation") },
    { value: "Planning", label: t("projects.options.phase.planning") },
    { value: "Execution", label: t("projects.options.phase.execution") },
    { value: "Monitoring", label: t("projects.options.phase.monitoring") },
    { value: "Closure", label: t("projects.options.phase.closure") },
  ];

  return (
    <Form form={form} layout="vertical">
      <Form.Item name="code" label={t("projects.fields.code")} rules={[{ required: true }]}>
        <Input placeholder={t("projects.placeholders.code")} />
      </Form.Item>
      <Form.Item name="name" label={t("projects.fields.name")} rules={[{ required: true }]}>
        <Input placeholder={t("projects.placeholders.name")} />
      </Form.Item>
      <Form.Item name="projectType" label={t("projects.fields.project_type")} initialValue="Internal" rules={[{ required: true }]}>
        <Select options={projectTypeOptions} />
      </Form.Item>
      <Form.Item name="ownerUserId" label={t("projects.fields.owner")}>
        <Select
          allowClear
          showSearch
          filterOption={false}
          optionFilterProp="label"
          options={userOptions}
          placeholder={t("projects.placeholders.owner")}
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
      <Form.Item name="sponsorUserId" label={t("projects.fields.sponsor")}>
        <Select
          allowClear
          showSearch
          filterOption={false}
          optionFilterProp="label"
          options={userOptions}
          placeholder={t("projects.placeholders.sponsor")}
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
      <Form.Item name="methodology" label={t("projects.fields.methodology")}>
        <Select allowClear options={methodologyOptions} placeholder={t("projects.placeholders.methodology")} />
      </Form.Item>
      <Form.Item name="phase" label={t("projects.fields.phase")}>
        <Select allowClear options={phaseOptions} placeholder={t("projects.placeholders.phase")} />
      </Form.Item>
      <Form.Item name="status" label={t("projects.fields.status")} initialValue="planned" rules={[{ required: true }]}>
        <Select options={projectStatusOptions} />
      </Form.Item>
      <Form.Item name="statusReason" label={t("projects.fields.status_reason")}>
        <Input.TextArea rows={3} placeholder={t("projects.placeholders.status_reason")} />
      </Form.Item>
      <Form.Item name="plannedPeriod" label={t("projects.fields.planned_period")}>
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
      <Form.Item name="actualPeriod" label={t("projects.fields.actual_period")}>
        <DatePicker.RangePicker style={{ width: "100%" }} />
      </Form.Item>
    </Form>
  );
}
