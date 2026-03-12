import { useState } from 'react';
import { Layout, Menu, Button, Typography, Flex, theme, Avatar, Dropdown, Popover, List } from 'antd';
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
} from '@ant-design/icons';
import { useNavigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../../modules/auth/hooks/useAuth';
import { useThemeStore, ThemeMode } from '../../store/useThemeStore';
import i18n from '../../i18n/config';

const { Header, Sider, Content } = Layout;
const { useToken } = theme;

export function MainLayout() {
  const { token } = useToken();
  const tr = (key: string, fallback: string) => i18n.t(key, { defaultValue: fallback });
  const [collapsed, setCollapsed] = useState(false);
  const [currentLanguage, setCurrentLanguage] = useState<'th' | 'en'>(
    i18n.language.startsWith('th') ? 'th' : 'en'
  );
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();
  const { theme: currentTheme, setTheme } = useThemeStore();
  const displayName = user?.name || user?.email?.split('@')[0] || 'User';
  const avatarInitial = displayName.trim().charAt(0).toUpperCase() || 'U';
  const avatarBg = currentTheme === 'dark' ? '#334155' : '#cbd5e1';
  const avatarText = currentTheme === 'dark' ? '#e2e8f0' : '#334155';

  const menuItems = [
    {
      key: '/app/dashboard',
      icon: <DashboardOutlined />,
      label: tr('common.dashboard', 'Dashboard'),
    },
    {
      key: '/app/documents',
      icon: <FileTextOutlined />,
      label: tr('common.documents', 'Documents'),
    },
  ];

  const profileMenuItems: MenuProps['items'] = [
    {
      key: 'language',
      label: `${currentLanguage.toUpperCase()} - ${tr('common.language', 'Language')}`,
      icon: <GlobalOutlined />,
      children: [
        { key: 'th', label: 'Thai' },
        { key: 'en', label: 'English' },
      ]
    },
    {
      key: 'theme',
      label: tr('common.theme_label', 'Theme'),
      icon: currentTheme === 'dark' ? <BulbFilled /> : <BulbOutlined />,
      children: [
        { key: 'light', label: tr('common.theme.light', 'Light') },
        { key: 'dark', label: tr('common.theme.dark', 'Dark') },
        { key: 'system', label: tr('common.theme.system', 'System') },
      ]
    },
    { type: 'divider' },
    {
      key: 'logout',
      label: 'Logout',
      icon: <LogoutOutlined />,
      danger: true,
    }
  ];

  const handleProfileMenuClick: MenuProps['onClick'] = ({ key }) => {
    if (key === 'logout') void logout();
    if (['light', 'dark', 'system'].includes(key)) setTheme(key as ThemeMode);
    if (key === 'th' || key === 'en') {
      void i18n.changeLanguage(key);
      setCurrentLanguage(key);
    }
  };

  const notifications = [
    { id: 1, title: 'แบบร่างเอกสารใหม่ถูกสร้างขึ้น', description: 'เอกสาร "รายงานประจำเดือน" ถูกสร้างขึ้นเรียบร้อยแล้ว', time: '5 นาทีที่แล้ว', read: false },
    { id: 2, title: 'คำขอกลับไปยังผู้สร้าง', description: 'คำขอเลขอ้างอิง REF-2024-001 ถูกตีกลับ', time: '1 ชั่วโมงที่แล้ว', read: false },
    { id: 3, title: 'การลงทะเบียนสำเร็จ', description: 'ยืนยันตัวตนผู้ใช้งานใหม่เรียบร้อยแล้ว', time: '2 ชั่วโมงที่แล้ว', read: true },
  ];

  const getPageTitle = (path: string) => {
    if (path.includes('dashboard')) return tr('common.dashboard', 'Dashboard');
    if (path.includes('documents')) return tr('common.documents', 'Documents');
    return tr('common.dashboard', 'Dashboard');
  };

  const notificationContent = (
    <div style={{ width: 360 }}>
      <Flex justify="space-between" align="center" style={{ padding: '12px 16px', borderBottom: `1px solid ${token.colorBorderSecondary}` }}>
        <Typography.Text strong>{tr('common.notifications', 'Notifications')}</Typography.Text>
        <Typography.Link style={{ fontSize: 12 }}>{tr('common.mark_all_read', 'Mark all as read')}</Typography.Link>
      </Flex>
      <div style={{ maxHeight: 400, overflowY: 'auto' }}>
        <List
          itemLayout="horizontal"
          dataSource={notifications}
          renderItem={(item) => (
            <List.Item 
              style={{ 
                padding: '12px 16px', 
                cursor: 'pointer',
                background: item.read ? 'transparent' : (currentTheme === 'dark' ? 'rgba(14, 165, 233, 0.05)' : 'rgba(14, 165, 233, 0.02)')
              }}
              className="notification-item"
            >
              <List.Item.Meta
                avatar={<Avatar style={{ backgroundColor: item.read ? '#cbd5e1' : '#0ea5e9' }} icon={<BellOutlined />} />}
                title={<Typography.Text strong={!item.read} style={{ fontSize: 13 }}>{item.title}</Typography.Text>}
                description={
                  <div>
                    <Typography.Text type="secondary" style={{ fontSize: 12, display: 'block' }}>{item.description}</Typography.Text>
                    <Typography.Text type="secondary" style={{ fontSize: 11 }}>{item.time}</Typography.Text>
                  </div>
                }
              />
            </List.Item>
          )}
        />
      </div>
      <div style={{ borderTop: `1px solid ${token.colorBorderSecondary}` }}>
        <Button type="text" block style={{ height: 48, borderRadius: 0 }}>
          {tr('common.view_all_notifications', 'View all (3)')}
        </Button>
      </div>
    </div>
  );

  return (
    <Layout style={{ minHeight: '100vh', background: token.colorBgBase }}>
      <Sider 
        trigger={null} 
        collapsible 
        collapsed={collapsed}
        theme={currentTheme === 'dark' ? 'dark' : 'light'}
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
              Office Inventory
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
              theme={currentTheme === 'dark' ? 'dark' : 'light'}
              mode="inline"
              selectedKeys={[location.pathname]}
              onClick={({ key }) => navigate(key)}
              items={menuItems}
              style={{ borderRight: 0, marginTop: 16, background: 'transparent' }}
            />
          </div>
          
          <div style={{ padding: '16px', borderTop: `1px solid ${token.colorBorderSecondary}` }}>
            {!collapsed ? (
              <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                {tr('common.version', 'v1.0.0')}
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
            <Popover 
              content={notificationContent} 
              trigger="click" 
              placement="bottomRight"
              overlayInnerStyle={{ padding: 0 }}
            >
              <div style={{ position: 'relative', cursor: 'pointer' }}>
                <Button 
                  shape="circle" 
                  icon={<BellOutlined style={{ fontSize: 20, color: currentTheme === 'dark' ? '#cbd5e1' : '#334155' }} />} 
                  style={{ 
                    width: 44, 
                    height: 44, 
                    display: 'flex', 
                    alignItems: 'center', 
                    justifyContent: 'center',
                    background: currentTheme === 'dark' ? '#0f172a' : '#ffffff', 
                    border: `1px solid ${currentTheme === 'dark' ? '#334155' : '#cbd5e1'}`,
                    pointerEvents: 'none' // Let the parent div handle the click
                  }}
                />
                {/* Notification Indicator Dot */}
                <div 
                  style={{ 
                    position: 'absolute',
                    top: -2,
                    right: -2,
                    width: 12,
                    height: 12,
                    backgroundColor: currentTheme === 'dark' ? '#38bdf8' : '#0ea5e9',
                    borderRadius: '50%',
                    border: `2px solid ${currentTheme === 'dark' ? '#0f172a' : '#ffffff'}`,
                    zIndex: 1
                  }}
                />
              </div>
            </Popover>
            
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
