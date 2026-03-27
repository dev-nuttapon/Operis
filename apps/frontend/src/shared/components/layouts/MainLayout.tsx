import { useEffect, useRef, useState } from 'react';
import { Layout, Menu, Button, Typography, Flex, theme, Avatar, Dropdown, Grid, Drawer } from 'antd';
import type { MenuProps } from 'antd';
import {
  FileTextOutlined,
  AlertOutlined,
  MenuOutlined,
  BulbOutlined,
  BulbFilled,
  LogoutOutlined,
  BellOutlined,
  CalendarOutlined,
  CheckCircleOutlined,
  LeftOutlined,
  RightOutlined,
  GlobalOutlined,
  ProjectOutlined,
  TeamOutlined,
  UserOutlined,
  DownOutlined,
} from '@ant-design/icons';
import { useNavigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../../modules/auth';
import { useCurrentUserProfile } from '../../../modules/users';
import { usePermissions } from '../../authz/usePermissions';
import { permissions } from '../../authz/permissions';
import { useThemeStore, ThemeMode } from '../../store/useThemeStore';
import i18n from '../../i18n/config';
import { useI18nLanguage } from '../../i18n/hooks/useI18nLanguage';

const { Header, Sider, Content } = Layout;
const { useToken } = theme;

export function MainLayout() {
  const { token } = useToken();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.lg;
  const language = useI18nLanguage();
  const tr = (key: string) => i18n.t(key, { lng: language });
  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [notificationOpen, setNotificationOpen] = useState(false);
  const [openKeys, setOpenKeys] = useState<string[]>([]);
  const notificationRef = useRef<HTMLDivElement | null>(null);
  const [currentLanguage, setCurrentLanguage] = useState<'th' | 'en'>(
    i18n.language.startsWith('th') ? 'th' : 'en'
  );
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();
  const currentUserQuery = useCurrentUserProfile();
  const permissionState = usePermissions();
  const { theme: currentTheme, setTheme } = useThemeStore();
  const hasAdminAccess = permissionState.hasAnyPermission(
    permissions.admin.permissionMatrixRead,
    permissions.admin.settingsRead,
    permissions.users.read,
    permissions.masterData.read,
    permissions.projects.read,
    permissions.activityLogs.read,
  );
  const canReadPermissionMatrix = permissionState.hasPermission(permissions.admin.permissionMatrixRead);
  const canReadSettings = permissionState.hasAnyPermission(permissions.admin.settingsRead, permissions.admin.settingsManage);
  const hasDocumentAccess = permissionState.hasAnyPermission(
    permissions.documents.read,
    permissions.documents.upload,
    permissions.documents.manageVersions,
    permissions.documents.publish,
    permissions.documents.deleteDraft,
    permissions.documents.deactivate,
  );
  const hasGovernanceAccess = permissionState.hasAnyPermission(
    permissions.governance.processLibraryRead,
    permissions.governance.processLibraryManage,
    permissions.governance.qaChecklistRead,
    permissions.governance.qaChecklistManage,
    permissions.governance.projectPlanRead,
    permissions.governance.projectPlanManage,
    permissions.governance.projectPlanApprove,
    permissions.governance.stakeholderRead,
    permissions.governance.stakeholderManage,
    permissions.governance.tailoringRead,
    permissions.governance.tailoringManage,
    permissions.governance.tailoringApprove,
  );
  const hasGovernanceOperationsAccess = permissionState.hasAnyPermission(
    permissions.governance.raciRead,
    permissions.governance.raciManage,
    permissions.governance.approvalEvidenceRead,
    permissions.governance.overrideLogRead,
    permissions.governance.slaRead,
    permissions.governance.slaManage,
    permissions.governance.retentionRead,
    permissions.governance.retentionManage,
  );
  const hasRequirementsAccess = permissionState.hasAnyPermission(
    permissions.requirements.read,
    permissions.requirements.manage,
    permissions.requirements.approve,
    permissions.requirements.baseline,
    permissions.requirements.manageTraceability,
  );
  const hasChangeControlAccess = permissionState.hasAnyPermission(
    permissions.changeControl.read,
    permissions.changeControl.manage,
    permissions.changeControl.approve,
    permissions.changeControl.readConfiguration,
    permissions.changeControl.manageConfiguration,
    permissions.changeControl.manageBaselines,
    permissions.changeControl.approveBaselines,
  );
  const hasRisksAccess = permissionState.hasAnyPermission(
    permissions.risks.read,
    permissions.risks.manage,
    permissions.risks.readSensitive,
  );
  const hasMeetingsAccess = permissionState.hasAnyPermission(
    permissions.meetings.read,
    permissions.meetings.manage,
    permissions.meetings.approve,
    permissions.meetings.readRestricted,
  );
  const hasVerificationAccess = permissionState.hasAnyPermission(
    permissions.verification.read,
    permissions.verification.manage,
    permissions.verification.approve,
    permissions.verification.submitUat,
    permissions.verification.export,
    permissions.verification.readSensitiveEvidence,
  );
  const hasAuditAccess = permissionState.hasAnyPermission(
    permissions.auditLogs.read,
    permissions.auditLogs.export,
    permissions.auditLogs.manage,
  );
  const hasMetricsAccess = permissionState.hasAnyPermission(
    permissions.metrics.read,
    permissions.metrics.manage,
    permissions.metrics.overrideQualityGates,
  );
  const hasReleasesAccess = permissionState.hasAnyPermission(
    permissions.releases.read,
    permissions.releases.manage,
    permissions.releases.approve,
  );
  const hasOperationsAccess = permissionState.hasAnyPermission(
    permissions.operations.read,
    permissions.operations.manage,
    permissions.operations.approve,
  );
  const displayName = user?.name || user?.email?.split('@')[0] || tr('common.user_fallback');
  const avatarInitial = displayName.trim().charAt(0).toUpperCase() || 'U';
  const jobTitleLabel =
    currentUserQuery.data?.jobTitleName ??
    (typeof user?.jobTitleName === "string" ? user.jobTitleName : undefined);
  const positionFallback = tr('common.position_empty');
  const jobTitleText = jobTitleLabel ?? positionFallback;
  const isDarkMode = token.colorBgBase.toLowerCase() === '#020617';
  const avatarBg = isDarkMode ? '#1e293b' : '#dbeafe';
  const avatarText = isDarkMode ? '#e2e8f0' : '#1e3a8a';
  const bellBg = isDarkMode ? '#111827' : '#eff6ff';
  const bellBorder = isDarkMode ? '#334155' : '#bfdbfe';
  const bellText = isDarkMode ? '#e2e8f0' : '#1d4ed8';
  const bellDot = token.colorPrimary;
  const currentLanguageLabel = currentLanguage === 'th' ? 'TH' : 'EN';
  const currentThemeLabel =
    currentTheme === 'light'
      ? tr('common.theme.light')
      : currentTheme === 'dark'
        ? tr('common.theme.dark')
        : tr('common.theme.system');

  useEffect(() => {
    const onLanguageChanged = (lng: string) => {
      setCurrentLanguage(lng.startsWith('th') ? 'th' : 'en');
    };
    i18n.on('languageChanged', onLanguageChanged);
    return () => {
      i18n.off('languageChanged', onLanguageChanged);
    };
  }, []);

  useEffect(() => {
    if (collapsed) {
      setOpenKeys([]);
      return;
    }

    setOpenKeys(getOpenKeys(location.pathname));
  }, [collapsed, location.pathname]);

  useEffect(() => {
    if (!notificationOpen) {
      return;
    }

    const handlePointerDown = (event: MouseEvent) => {
      if (!notificationRef.current?.contains(event.target as Node)) {
        setNotificationOpen(false);
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
    };
  }, [notificationOpen]);

  const menuItems = [
    ...(hasDocumentAccess
      ? [{
          key: '/app/documents-group',
          icon: <FileTextOutlined />,
          label: tr('common.documents'),
          children: [
            {
              key: '/app/documents',
              label: 'Document Register',
            },
            ...(permissionState.hasPermission(permissions.documents.deactivate)
              ? [{
                  key: '/app/documents/types',
                  label: 'Document Type Setup',
                }]
              : []),
            {
              key: '/app/documents/templates',
              label: 'Document Templates',
            },
          ],
        }]
      : []),
    {
      key: '/app/projects-group',
      icon: <ProjectOutlined />,
      label: tr('common.my_projects'),
      children: [
        {
          key: '/app/projects/roles',
          label: tr('common.master_project_roles'),
        },
        {
          key: '/app/projects/phase-approvals',
          label: 'Project Phase Approval',
        },
        {
          key: '/app/steps',
          label: tr('common.workflows'),
        },
        {
          key: '/app/projects',
          label: tr('common.project_list'),
        },
        {
          key: '/app/workspace',
          label: tr('common.workflow_tasks'),
        },
      ],
    },
    ...[],
    ...(hasGovernanceAccess
      ? [{
          key: '/app/governance',
          icon: <ProjectOutlined />,
          label: 'Governance',
          children: [
            { key: '/app/process-library', label: 'Process Library' },
            { key: '/app/qa-review-checklists', label: 'QA Review Checklist' },
            { key: '/app/project-plans', label: 'Project Plan' },
            { key: '/app/stakeholders', label: 'Stakeholder Register' },
            { key: '/app/tailoring-records', label: 'Tailoring Record' },
          ],
        }]
      : []),
    ...(hasRequirementsAccess
      ? [{
          key: '/app/requirements-group',
          icon: <FileTextOutlined />,
          label: 'Requirements',
          children: [
            { key: '/app/requirements', label: 'Requirement Register' },
            { key: '/app/requirements/baselines', label: 'Requirement Baselines' },
            { key: '/app/requirements/traceability', label: 'Traceability Matrix' },
          ],
        }]
      : []),
    ...(hasGovernanceOperationsAccess
      ? [{
          key: '/app/governance-operations',
          icon: <BulbOutlined />,
          label: 'Governance & Operations',
          children: [
            { key: '/app/governance/raci-maps', label: 'RACI Map' },
            { key: '/app/governance/approval-evidence', label: 'Approval Evidence Log' },
            { key: '/app/governance/workflow-overrides', label: 'Workflow Override Log' },
            { key: '/app/governance/sla-rules', label: 'SLA & Escalation Rules' },
            { key: '/app/governance/retention-policies', label: 'Data Retention Policy' },
          ],
        }]
      : []),
    ...(hasChangeControlAccess
      ? [{
          key: '/app/change-control-group',
          icon: <ProjectOutlined />,
          label: 'Change Control',
          children: [
            { key: '/app/change-control/change-requests', label: 'Change Requests' },
            { key: '/app/change-control/configuration-items', label: 'Configuration Items' },
            { key: '/app/change-control/baseline-registry', label: 'Baseline Registry' },
          ],
        }]
      : []),
    ...(hasRisksAccess
      ? [{
          key: '/app/risks-group',
          icon: <AlertOutlined />,
          label: 'Risks & Issues',
          children: [
            { key: '/app/risks', label: 'Risk Register' },
            { key: '/app/issues', label: 'Issue Log' },
          ],
        }]
      : []),
    ...(hasMeetingsAccess
      ? [{
          key: '/app/meetings-group',
          icon: <CalendarOutlined />,
          label: 'Meetings',
          children: [
            { key: '/app/meetings', label: 'MOM Register' },
            { key: '/app/decisions', label: 'Decision Log' },
          ],
        }]
      : []),
    ...(hasVerificationAccess
      ? [{
          key: '/app/verification-group',
          icon: <CheckCircleOutlined />,
          label: 'Test & Validation',
          children: [
            { key: '/app/test-plans', label: 'Test Plan' },
            { key: '/app/test-cases', label: 'Test Case & Execution' },
            { key: '/app/uat-signoffs', label: 'UAT Sign-off' },
          ],
        }]
      : []),
    ...(hasAuditAccess
      ? [{
          key: '/app/audits-group',
          icon: <AlertOutlined />,
          label: 'Audits & Evidence',
          children: [
            { key: '/app/audit-logs', label: 'Audit Log' },
            { key: '/app/evidence-exports', label: 'Evidence Export' },
            { key: '/app/audit-plans', label: 'Process Audit Plan & Findings' },
          ],
        }]
      : []),
    ...(hasMetricsAccess
      ? [{
          key: '/app/metrics-group',
          icon: <BulbOutlined />,
          label: 'Metrics & Quality Gates',
          children: [
            { key: '/app/metrics/dashboard', label: 'Metrics Dashboard' },
            { key: '/app/metrics/definitions', label: 'Metric Definitions' },
            { key: '/app/metrics/quality-gates', label: 'Quality Gate Status' },
          ],
        }]
      : []),
    ...(hasReleasesAccess
      ? [{
          key: '/app/releases-group',
          icon: <CheckCircleOutlined />,
          label: 'Release Management',
          children: [
            { key: '/app/releases', label: 'Release Register' },
            { key: '/app/releases/checklists', label: 'Deployment Checklist' },
            { key: '/app/releases/notes', label: 'Release Notes' },
          ],
        }]
      : []),
    ...(hasOperationsAccess
      ? [{
          key: '/app/operations-group',
          icon: <AlertOutlined />,
          label: 'Security & Dependencies',
          children: [
            { key: '/app/operations/access-reviews', label: 'Access Review' },
            { key: '/app/operations/security-reviews', label: 'Security Review' },
            { key: '/app/operations/external-dependencies', label: 'External Dependency Register' },
            { key: '/app/operations/configuration-audits', label: 'Configuration Audit Log' },
          ],
        }]
      : []),
    ...(hasAdminAccess
      ? [{
          key: '/app/admin',
          icon: <TeamOutlined />,
          label: tr('common.admin'),
          children: [
            {
              key: '/app/admin/users-group',
              label: tr('common.user_admin_group'),
              children: [
                {
                  key: '/app/admin/users',
                  label: tr('common.user_management'),
                },
                {
                  key: '/app/admin/invitations',
                  label: tr('common.user_invitations'),
                },
                {
                  key: '/app/admin/registrations',
                  label: tr('common.registration_approvals'),
                },
              ],
            },
            {
              key: '/app/admin/master',
              label: tr('common.master_data_management'),
              children: [
                {
                  key: '/app/admin/master/catalog',
                  label: 'Catalog',
                },
                {
                  key: '/app/admin/master/permanent',
                  label: tr('common.master_permanent_structure'),
                  children: [
                    {
                      key: '/app/admin/master/divisions',
                      label: tr('common.master_divisions'),
                    },
                    {
                      key: '/app/admin/master/departments',
                      label: tr('common.master_departments'),
                    },
                    {
                      key: '/app/admin/master/positions',
                      label: tr('common.master_positions'),
                    },
                  ],
                },
              ],
            },
            {
              key: '/app/admin/activity-logs',
              label: tr('common.activity_logs'),
            },
            ...(canReadPermissionMatrix
              ? [{
                  key: '/app/admin/permissions',
                  label: 'Permission Matrix',
                }]
              : []),
            ...(canReadSettings
              ? [{
                  key: '/app/admin/settings',
                  label: tr('common.admin_settings'),
                }]
              : []),
          ],
        }]
      : []),
  ];

  const profileMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      label: tr('common.profile'),
      icon: <UserOutlined />,
    },
    { type: 'divider' },
    {
      key: 'language',
      label: `${tr('common.language')}: ${currentLanguageLabel}`,
      icon: <GlobalOutlined />,
      children: [
        { key: 'th', label: tr('common.language_th') },
        { key: 'en', label: tr('common.language_en') },
      ]
    },
    {
      key: 'theme',
      label: `${tr('common.theme_label')}: ${currentThemeLabel}`,
      icon: isDarkMode ? <BulbFilled /> : <BulbOutlined />,
      children: [
        { key: 'light', label: tr('common.theme.light') },
        { key: 'dark', label: tr('common.theme.dark') },
        { key: 'system', label: tr('common.theme.system') },
      ]
    },
    { type: 'divider' },
    {
      key: 'logout',
      label: tr('auth.logout_button'),
      icon: <LogoutOutlined />,
      danger: true,
    }
  ];

  const handleProfileMenuClick: MenuProps['onClick'] = ({ key }) => {
    if (key === 'profile') {
      navigate('/app/profile');
      return;
    }
    if (key === 'logout') void logout();
    if (['light', 'dark', 'system'].includes(key)) setTheme(key as ThemeMode);
    if (key === 'th' || key === 'en') {
      void i18n.changeLanguage(key);
    }
  };

  const notifications = [
    {
      id: 1,
      title: tr('layout.notifications.items.draft_created.title'),
      description: tr('layout.notifications.items.draft_created.description'),
      time: tr('layout.notifications.items.draft_created.time'),
      read: false,
    },
    {
      id: 2,
      title: tr('layout.notifications.items.request_returned.title'),
      description: tr('layout.notifications.items.request_returned.description'),
      time: tr('layout.notifications.items.request_returned.time'),
      read: false,
    },
    {
      id: 3,
      title: tr('layout.notifications.items.registration_completed.title'),
      description: tr('layout.notifications.items.registration_completed.description'),
      time: tr('layout.notifications.items.registration_completed.time'),
      read: true,
    },
  ];

  const getPageTitle = (path: string) => {
    if (path.includes('dashboard')) return tr('common.dashboard');
    if (path.includes('document-templates')) return tr('common.document_templates');
    if (path.includes('documents')) return tr('common.documents');
    if (path.includes('/app/profile')) return tr('common.profile');
    if (path.includes('/app/change-password')) return tr('common.change_password');
    if (path === '/app/projects') return tr('common.my_projects');
    if (path.includes('steps')) return tr('common.workflows');
    if (path.includes('admin/users')) return tr('common.user_management');
    if (path.includes('admin/master/divisions')) return tr('common.master_divisions');
    if (path.includes('admin/master/catalog')) return 'Master Data Catalog';
    if (path.includes('admin/master/departments')) return tr('common.master_departments');
    if (path.includes('admin/master/positions')) return tr('common.master_positions');
    if (path.includes('/projects/roles')) return tr('common.master_project_roles');
    if (path.includes('/projects/phase-approvals')) return 'Project Phase Approval';
    if (path.includes('/projects/') && path.includes('/workspace')) return tr('common.project_workspace');
    if (path.includes('admin/master')) return tr('common.master_data_management');
    if (path.includes('admin/invitations')) return tr('common.user_invitations');
    if (path.includes('admin/registrations')) return tr('common.registration_approvals');
    if (path.includes('admin/activity-logs')) return tr('common.activity_logs');
    if (path.includes('/app/audit-plans')) return 'Process Audit Plan & Findings';
    if (path.includes('/app/evidence-exports')) return 'Evidence Export';
    if (path.includes('/app/audit-logs')) return 'Audit Log';
    if (path.includes('/app/governance/raci-maps')) return 'RACI Map';
    if (path.includes('/app/governance/approval-evidence')) return 'Approval Evidence Log';
    if (path.includes('/app/governance/workflow-overrides')) return 'Workflow Override Log';
    if (path.includes('/app/governance/sla-rules')) return 'SLA & Escalation Rules';
    if (path.includes('/app/governance/retention-policies')) return 'Data Retention Policy';
    if (path.includes('/app/metrics/dashboard')) return 'Metrics Dashboard';
    if (path.includes('/app/metrics/definitions')) return 'Metric Definitions';
    if (path.includes('/app/metrics/quality-gates')) return 'Quality Gate Status';
    if (path.includes('/app/releases/checklists')) return 'Deployment Checklist';
    if (path.includes('/app/releases/notes')) return 'Release Notes';
    if (path.includes('/app/releases')) return 'Release Register';
    if (path.includes('/app/operations/access-reviews')) return 'Access Review';
    if (path.includes('/app/operations/security-reviews')) return 'Security Review';
    if (path.includes('/app/operations/external-dependencies')) return 'External Dependency Register';
    if (path.includes('/app/operations/configuration-audits')) return 'Configuration Audit Log';
    return tr('common.dashboard');
  };

  const handleMenuOpenChange = (keys: string[]) => {
    setOpenKeys(keys);
  };

  const handleMenuClick: MenuProps['onClick'] = ({ key }) => {
    navigate(key);
    if (isMobile) {
      setMobileMenuOpen(false);
    }
  };

  const menuNode = (
    <Menu
      theme={isDarkMode ? 'dark' : 'light'}
      mode="inline"
      selectedKeys={[getSelectedMenuKey(location.pathname)]}
      openKeys={openKeys}
      onOpenChange={handleMenuOpenChange}
      onClick={handleMenuClick}
      items={menuItems}
      style={{ borderRight: 0, marginTop: 16, background: 'transparent' }}
    />
  );

  return (
    <Layout style={{ minHeight: '100vh', background: token.colorBgBase }}>
      {!isMobile ? (
        <Sider 
          trigger={null} 
          collapsible 
          collapsed={collapsed}
          theme={isDarkMode ? 'dark' : 'light'}
          width={240}
          style={{ 
            background: token.colorBgContainer,
            borderRight: `1px solid ${token.colorBorderSecondary}`,
            position: 'relative',
          }}
        >
          <div 
            style={{ 
              height: 64, 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'space-between',
              padding: '0 16px',
              borderBottom: `1px solid ${token.colorBorderSecondary}` 
            }}
          >
            {!collapsed && (
              <Typography.Text style={{ fontWeight: 700, fontSize: 16 }}>
                {tr('common.application_name')}
              </Typography.Text>
            )}
            <Button
              type="text"
              icon={collapsed ? <RightOutlined /> : <LeftOutlined />}
              onClick={() => setCollapsed(!collapsed)}
              style={{ 
                fontSize: '12px', 
                width: 24, 
                height: 24,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            />
          </div>
          
          <div style={{ display: 'flex', flexDirection: 'column', height: 'calc(100vh - 64px)' }}>
            <div style={{ flex: 1 }}>
              {menuNode}
            </div>
            
            <div style={{ padding: '16px', borderTop: `1px solid ${token.colorBorderSecondary}` }}>
              {!collapsed ? (
                <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                  {tr('common.version')}
                </Typography.Text>
              ) : (
                <div style={{ textAlign: 'center' }}>
                  <Avatar size="small" style={{ backgroundColor: avatarBg, color: avatarText }}>
                    {avatarInitial}
                  </Avatar>
                </div>
              )}
            </div>
          </div>
        </Sider>
      ) : (
        <Drawer
          placement="left"
          closable={false}
          open={mobileMenuOpen}
          onClose={() => setMobileMenuOpen(false)}
          width={280}
          styles={{
            body: { padding: 0, background: token.colorBgContainer },
            header: { display: 'none' },
          }}
        >
          <div 
            style={{ 
              height: 64, 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'space-between',
              padding: '0 16px',
              borderBottom: `1px solid ${token.colorBorderSecondary}` 
            }}
          >
            <Typography.Text style={{ fontWeight: 700, fontSize: 16 }}>
              {tr('common.application_name')}
            </Typography.Text>
            <Button
              type="text"
              icon={<LeftOutlined />}
              onClick={() => setMobileMenuOpen(false)}
              style={{ 
                fontSize: '12px', 
                width: 24, 
                height: 24,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            />
          </div>
          {menuNode}
        </Drawer>
      )}
      
      <Layout style={{ background: 'transparent' }}>
        <Header 
          style={{ 
            padding: isMobile ? '0 16px' : '0 32px 0 24px', 
            background: token.colorBgContainer,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
            height: 64,
          }}
        >
          <Flex align="center" gap={12}>
            {isMobile ? (
              <Button
                type="text"
                icon={<MenuOutlined />}
                onClick={() => setMobileMenuOpen(true)}
                style={{ width: 40, height: 40 }}
              />
            ) : null}
            <div>
              <Typography.Text style={{ margin: 0, fontSize: 24, fontWeight: 700, display: 'block', lineHeight: '32px' }}>
                {getPageTitle(location.pathname)}
              </Typography.Text>
              <Typography.Text type="secondary" style={{ fontSize: 12, display: 'block', marginTop: -4 }}>
                {getPageTitle(location.pathname)}
              </Typography.Text>
            </div>
          </Flex>

          <Flex align="center" gap={16}>
            <div ref={notificationRef} style={{ position: 'relative' }}>
              <div style={{ position: 'relative', cursor: 'pointer' }}>
                <Button
                  aria-label={tr('common.notifications')}
                  shape="circle"
                  icon={<BellOutlined style={{ fontSize: 20, color: bellText }} />}
                  onClick={() => setNotificationOpen((current) => !current)}
                  style={{
                    width: 44,
                    height: 44,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    background: bellBg,
                    border: `1px solid ${bellBorder}`,
                  }}
                />
                <div 
                  style={{ 
                    position: 'absolute',
                    top: -2,
                    right: -2,
                    width: 12,
                    height: 12,
                    backgroundColor: bellDot,
                    borderRadius: '50%',
                    border: `2px solid ${bellBg}`,
                    zIndex: 1
                  }}
                />
              </div>

              {notificationOpen ? (
                <div
                  style={{
                    position: 'absolute',
                    top: 'calc(100% + 12px)',
                    right: 0,
                    width: 360,
                    background: token.colorBgContainer,
                    border: `1px solid ${token.colorBorderSecondary}`,
                    borderRadius: 18,
                    overflow: 'hidden',
                    boxShadow: isDarkMode
                      ? '0 24px 60px rgba(2, 6, 23, 0.55)'
                      : '0 24px 60px rgba(148, 163, 184, 0.22)',
                    zIndex: 20,
                  }}
                >
                  <Flex justify="space-between" align="center" style={{ padding: '12px 16px', borderBottom: `1px solid ${token.colorBorderSecondary}` }}>
                    <Typography.Text strong>{tr('common.notifications')}</Typography.Text>
                    <Typography.Link
                      style={{ fontSize: 12 }}
                      onClick={() => {
                        setNotificationOpen(false);
                        navigate('/app/notifications');
                      }}
                    >
                      {tr('common.mark_all_read')}
                    </Typography.Link>
                  </Flex>
                  <div style={{ maxHeight: 400, overflowY: 'auto' }}>
                    {notifications.map((item) => (
                      <div
                        key={item.id}
                        style={{
                          padding: '12px 16px',
                          cursor: 'pointer',
                          display: 'grid',
                          gridTemplateColumns: 'auto 1fr',
                          gap: 12,
                          alignItems: 'start',
                          background: item.read ? 'transparent' : (currentTheme === 'dark' ? 'rgba(14, 165, 233, 0.05)' : 'rgba(14, 165, 233, 0.02)'),
                          borderBottom: `1px solid ${token.colorBorderSecondary}`,
                        }}
                        className="notification-item"
                      >
                        <Avatar style={{ backgroundColor: item.read ? '#cbd5e1' : '#0ea5e9' }} icon={<BellOutlined />} />
                        <div>
                          <Typography.Text strong={!item.read} style={{ fontSize: 13, display: 'block' }}>{item.title}</Typography.Text>
                          <Typography.Text type="secondary" style={{ fontSize: 12, display: 'block' }}>{item.description}</Typography.Text>
                          <Typography.Text type="secondary" style={{ fontSize: 11 }}>{item.time}</Typography.Text>
                        </div>
                      </div>
                    ))}
                  </div>
                  <div style={{ borderTop: `1px solid ${token.colorBorderSecondary}` }}>
                    <Button
                      type="text"
                      block
                      style={{ height: 48, borderRadius: 0 }}
                      onClick={() => {
                        setNotificationOpen(false);
                        navigate('/app/notifications');
                      }}
                    >
                      {tr('common.view_all_notifications')}
                    </Button>
                  </div>
                </div>
              ) : null}
            </div>

            <Dropdown menu={{ items: profileMenuItems, onClick: handleProfileMenuClick }} trigger={['click']}>
              <Button
                type="text"
                style={{ padding: 0, height: 'auto' }}
                aria-label={tr('common.profile')}
              >
                <Flex align="center" gap="middle">
                  <Avatar 
                    size={36} 
                    style={{ backgroundColor: avatarBg, color: avatarText, fontWeight: 700 }}
                  >
                    {avatarInitial}
                  </Avatar>
                  <div style={{ lineHeight: 1, textAlign: 'left' }}>
                    <Typography.Text strong style={{ display: 'block', fontSize: 14 }}>
                      {displayName}
                    </Typography.Text>
                    <Typography.Text
                      type="secondary"
                      style={{
                        display: 'block',
                        fontSize: 12,
                        marginTop: 4,
                        whiteSpace: 'nowrap',
                        maxWidth: 220,
                        overflow: 'hidden',
                        textOverflow: 'ellipsis',
                      }}
                    >
                      {jobTitleText}
                    </Typography.Text>
                  </div>
                  <DownOutlined style={{ fontSize: 12, opacity: 0.6 }} />
                </Flex>
              </Button>
            </Dropdown>
          </Flex>
        </Header>
        
        <Content
          style={{
            padding: isMobile ? '16px' : '24px',
            minHeight: 280,
            background: 'transparent'
          }}
        >
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}

function getOpenKeys(path: string) {
  if (
    path.startsWith('/app/admin/master/divisions') ||
    path.startsWith('/app/admin/master/catalog') ||
    path.startsWith('/app/admin/master/departments') ||
    path.startsWith('/app/admin/master/positions')
  ) {
    return ['/app/admin', '/app/admin/master', '/app/admin/master/permanent'];
  }

  if (
    path.startsWith('/app/projects') ||
    path.startsWith('/app/projects/roles') ||
    path.startsWith('/app/projects/phase-approvals') ||
    path.startsWith('/app/steps') ||
    path.startsWith('/app/workspace')
  ) {
    return ['/app/projects-group'];
  }

  if (path.startsWith('/app/documents') || path.startsWith('/app/document-templates')) {
    return ['/app/documents-group'];
  }

  if (path.startsWith('/app/requirements')) {
    return ['/app/requirements-group'];
  }

  if (path.startsWith('/app/change-control')) {
    return ['/app/change-control-group'];
  }

  if (path.startsWith('/app/risks') || path.startsWith('/app/issues')) {
    return ['/app/risks-group'];
  }

  if (path.startsWith('/app/meetings') || path.startsWith('/app/decisions')) {
    return ['/app/meetings-group'];
  }

  if (path.startsWith('/app/test-plans') || path.startsWith('/app/test-cases') || path.startsWith('/app/uat-signoffs')) {
    return ['/app/verification-group'];
  }

  if (path.startsWith('/app/audit-logs') || path.startsWith('/app/evidence-exports') || path.startsWith('/app/audit-plans')) {
    return ['/app/audits-group'];
  }

  if (path.startsWith('/app/metrics/')) {
    return ['/app/metrics-group'];
  }

  if (path.startsWith('/app/admin/master/')) {
    return ['/app/admin', '/app/admin/master'];
  }

  if (
    path.startsWith('/app/admin/users') ||
    path.startsWith('/app/admin/invitations') ||
    path.startsWith('/app/admin/registrations')
  ) {
    return ['/app/admin', '/app/admin/users-group'];
  }

  if (path.startsWith('/app/admin')) {
    return ['/app/admin'];
  }

  return [];
}

function getSelectedMenuKey(path: string) {
  if (path.startsWith('/app/document-templates')) {
    return '/app/document-templates';
  }

  if (path.startsWith('/app/documents/templates')) {
    return '/app/document-templates';
  }

  if (path.startsWith('/app/documents')) {
    return '/app/documents';
  }

  if (path.startsWith('/app/projects/roles')) {
    return '/app/projects/roles';
  }

  if (path.startsWith('/app/projects/phase-approvals')) {
    return '/app/projects/phase-approvals';
  }

  if (path.startsWith('/app/projects/')) {
    return '/app/projects';
  }

  if (path.startsWith('/app/workspace')) {
    return '/app/workspace';
  }

  if (path.startsWith('/app/steps')) {
    return '/app/steps';
  }

  if (path.startsWith('/app/notifications')) {
    return '/app/notifications';
  }

  if (path.startsWith('/app/requirements/baselines')) {
    return '/app/requirements/baselines';
  }

  if (path.startsWith('/app/requirements/traceability')) {
    return '/app/requirements/traceability';
  }

  if (path.startsWith('/app/requirements')) {
    return '/app/requirements';
  }

  if (path.startsWith('/app/change-control/configuration-items')) {
    return '/app/change-control/configuration-items';
  }

  if (path.startsWith('/app/change-control/baseline-registry')) {
    return '/app/change-control/baseline-registry';
  }

  if (path.startsWith('/app/change-control')) {
    return '/app/change-control/change-requests';
  }

  if (path.startsWith('/app/issues')) {
    return '/app/issues';
  }

  if (path.startsWith('/app/risks')) {
    return '/app/risks';
  }

  if (path.startsWith('/app/decisions')) {
    return '/app/decisions';
  }

  if (path.startsWith('/app/meetings')) {
    return '/app/meetings';
  }

  if (path.startsWith('/app/test-cases')) {
    return '/app/test-cases';
  }

  if (path.startsWith('/app/uat-signoffs')) {
    return '/app/uat-signoffs';
  }

  if (path.startsWith('/app/test-plans')) {
    return '/app/test-plans';
  }

  if (path.startsWith('/app/audit-plans')) {
    return '/app/audit-plans';
  }

  if (path.startsWith('/app/evidence-exports')) {
    return '/app/evidence-exports';
  }

  if (path.startsWith('/app/audit-logs')) {
    return '/app/audit-logs';
  }

  if (path.startsWith('/app/metrics/quality-gates')) {
    return '/app/metrics/quality-gates';
  }

  if (path.startsWith('/app/metrics/definitions')) {
    return '/app/metrics/definitions';
  }

  if (path.startsWith('/app/metrics/dashboard')) {
    return '/app/metrics/dashboard';
  }

  if (path.startsWith('/app/admin/users')) {
    return '/app/admin/users';
  }

  if (path.startsWith('/app/admin/invitations')) {
    return '/app/admin/invitations';
  }

  if (path.startsWith('/app/admin/registrations')) {
    return '/app/admin/registrations';
  }

  if (path.startsWith('/app/admin/master/divisions')) {
    return '/app/admin/master/divisions';
  }

  if (path.startsWith('/app/admin/master/catalog')) {
    return '/app/admin/master/catalog';
  }

  if (path.startsWith('/app/admin/master/departments')) {
    return '/app/admin/master/departments';
  }

  if (path.startsWith('/app/admin/master/positions')) {
    return '/app/admin/master/positions';
  }


  if (path.startsWith('/app/admin/activity-logs')) {
    return '/app/admin/activity-logs';
  }

  if (path.startsWith('/app/admin/settings')) {
    return '/app/admin/settings';
  }

  if (path.startsWith('/app/admin/master')) {
    return '/app/admin/master';
  }

  if (path.startsWith('/app/admin')) {
    return '/app/admin';
  }

  return path;
}
