import { useEffect, useRef, useState } from 'react';
import { Layout, Menu, Button, Typography, Flex, theme, Avatar, Dropdown } from 'antd';
import type { MenuProps } from 'antd';
import {
  FileTextOutlined,
  BulbOutlined,
  BulbFilled,
  LogoutOutlined,
  BellOutlined,
  LeftOutlined,
  RightOutlined,
  DashboardOutlined,
  GlobalOutlined,
  TeamOutlined,
  ApartmentOutlined,
} from '@ant-design/icons';
import { useNavigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../../modules/auth';
import { useThemeStore, ThemeMode } from '../../store/useThemeStore';
import i18n from '../../i18n/config';
import { useI18nLanguage } from '../../i18n/hooks/useI18nLanguage';

const { Header, Sider, Content } = Layout;
const { useToken } = theme;

export function MainLayout() {
  const { token } = useToken();
  const language = useI18nLanguage();
  const tr = (key: string) => i18n.t(key, { lng: language });
  const [collapsed, setCollapsed] = useState(false);
  const [notificationOpen, setNotificationOpen] = useState(false);
  const [openKeys, setOpenKeys] = useState<string[]>([]);
  const notificationRef = useRef<HTMLDivElement | null>(null);
  const [currentLanguage, setCurrentLanguage] = useState<'th' | 'en'>(
    i18n.language.startsWith('th') ? 'th' : 'en'
  );
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();
  const { theme: currentTheme, setTheme } = useThemeStore();
  const roles = Array.isArray(user?.roles) ? user.roles : [];
  const hasAdminAccess = roles.some((role) =>
    [
      'operis:super_admin',
      'operis:system_admin',
      'operis_super_admin',
      'operis_system_admin',
    ].includes(role)
  );
  const displayName = user?.name || user?.email?.split('@')[0] || tr('common.user_fallback');
  const avatarInitial = displayName.trim().charAt(0).toUpperCase() || 'U';
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
    {
      key: '/app/dashboard',
      icon: <DashboardOutlined />,
      label: tr('common.dashboard'),
    },
    {
      key: '/app/documents',
      icon: <FileTextOutlined />,
      label: tr('common.documents'),
    },
    {
      key: '/app/workflows',
      icon: <ApartmentOutlined />,
      label: tr('common.workflows'),
    },
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
                  key: '/app/admin/master/departments',
                  label: tr('common.master_departments'),
                },
                {
                  key: '/app/admin/master/job-titles',
                  label: tr('common.master_job_titles'),
                },
              ],
            },
            {
              key: '/app/admin/audit-logs',
              label: tr('common.audit_logs'),
            },
          ],
        }]
      : []),
  ];

  const profileMenuItems: MenuProps['items'] = [
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
    if (path.includes('documents')) return tr('common.documents');
    if (path.includes('workflows')) return tr('common.workflows');
    if (path.includes('admin/users')) return tr('common.user_management');
    if (path.includes('admin/master/departments')) return tr('common.master_departments');
    if (path.includes('admin/master/job-titles')) return tr('common.master_job_titles');
    if (path.includes('admin/master')) return tr('common.master_data_management');
    if (path.includes('admin/invitations')) return tr('common.user_invitations');
    if (path.includes('admin/registrations')) return tr('common.registration_approvals');
    if (path.includes('admin/audit-logs')) return tr('common.audit_logs');
    return tr('common.dashboard');
  };

  const handleMenuOpenChange = (keys: string[]) => {
    setOpenKeys(keys);
  };

  return (
    <Layout style={{ minHeight: '100vh', background: token.colorBgBase }}>
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
            <Menu
              theme={isDarkMode ? 'dark' : 'light'}
              mode="inline"
              selectedKeys={[location.pathname]}
              openKeys={openKeys}
              onOpenChange={handleMenuOpenChange}
              onClick={({ key }) => navigate(key)}
              items={menuItems}
              style={{ borderRight: 0, marginTop: 16, background: 'transparent' }}
            />
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
      
      <Layout style={{ background: 'transparent' }}>
        <Header 
          style={{ 
            padding: '0 32px 0 24px', 
            background: token.colorBgContainer,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
            height: 64,
          }}
        >
          <div>
            <Typography.Text style={{ margin: 0, fontSize: 24, fontWeight: 700, display: 'block', lineHeight: '32px' }}>
              {getPageTitle(location.pathname)}
            </Typography.Text>
            <Typography.Text type="secondary" style={{ fontSize: 12, display: 'block', marginTop: -4 }}>
              {getPageTitle(location.pathname)}
            </Typography.Text>
          </div>

          <Flex align="center" gap={40}>
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
                    <Typography.Link style={{ fontSize: 12 }}>{tr('common.mark_all_read')}</Typography.Link>
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
                    <Button type="text" block style={{ height: 48, borderRadius: 0 }}>
                      {tr('common.view_all_notifications')}
                    </Button>
                  </div>
                </div>
              ) : null}
            </div>
            
            <Dropdown menu={{ items: profileMenuItems, onClick: handleProfileMenuClick }} trigger={['click']}>
              <Flex align="center" gap="middle" style={{ cursor: 'pointer' }}>
                <Avatar 
                  size={36} 
                  style={{ backgroundColor: avatarBg, color: avatarText, fontWeight: 700 }}
                >
                  {avatarInitial}
                </Avatar>
                <div style={{ lineHeight: 1 }}>
                  <Typography.Text strong style={{ display: 'block', fontSize: 14 }}>
                    {displayName}
                  </Typography.Text>
                </div>
              </Flex>
            </Dropdown>
          </Flex>
        </Header>
        
        <Content
          style={{
            padding: '24px',
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
  if (path.startsWith('/app/admin/master/')) {
    return ['/app/admin', '/app/admin/master'];
  }

  if (path.startsWith('/app/admin/audit-logs')) {
    return ['/app/admin'];
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
