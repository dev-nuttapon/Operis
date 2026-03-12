import { useState } from 'react';
import { Layout, Menu, Button, Typography, Flex, theme, Avatar, Badge, Dropdown } from 'antd';
import type { MenuProps } from 'antd';
import {
  FileTextOutlined,
  BulbOutlined,
  BulbFilled,
  LogoutOutlined,
  BellOutlined,
  UserOutlined,
  LeftOutlined,
  RightOutlined,
  DashboardOutlined,
  GlobalOutlined,
} from '@ant-design/icons';
import { useNavigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../../modules/auth/hooks/useAuth';
import { useThemeStore, ThemeMode } from '../../store/useThemeStore';

const { Header, Sider, Content } = Layout;
const { useToken } = theme;

export function MainLayout() {
  const { token } = useToken();
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, user } = useAuth();
  const { theme: currentTheme, setTheme } = useThemeStore();

  const menuItems = [
    {
      key: '/app/dashboard',
      icon: <DashboardOutlined />,
      label: 'แดชบอร์ด',
    },
    {
      key: '/app/documents',
      icon: <FileTextOutlined />,
      label: 'งานเอกสาร',
    },
  ];

  const profileMenuItems: MenuProps['items'] = [
    {
      key: 'language',
      label: 'TH - เปลี่ยนภาษา',
      icon: <GlobalOutlined />,
      children: [
        { key: 'th', label: 'Thai' },
        { key: 'en', label: 'English' },
      ]
    },
    {
      key: 'theme',
      label: 'ธีม',
      icon: currentTheme === 'dark' ? <BulbFilled /> : <BulbOutlined />,
      children: [
        { key: 'light', label: 'Light' },
        { key: 'dark', label: 'Dark' },
        { key: 'system', label: 'Auto' },
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
  };

  // Map path to title
  const getPageTitle = (path: string) => {
    if (path.includes('dashboard')) return 'Dashboard';
    if (path.includes('documents')) return 'Documents';
    return 'Dashboard';
  };

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
                v1.0.0
              </Typography.Text>
            ) : (
              <div style={{ textAlign: 'center' }}>
                <Avatar size="small" icon={<UserOutlined />} />
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
            <Badge dot color={token.colorPrimary} offset={[-2, 2]}>
              <Button 
                shape="circle" 
                icon={<BellOutlined style={{ fontSize: 20 }} />} 
                style={{ 
                  width: 44, 
                  height: 44, 
                  display: 'flex', 
                  alignItems: 'center', 
                  justifyContent: 'center',
                  background: currentTheme === 'dark' ? '#1e293b' : '#ffffff', 
                  border: `1px solid ${token.colorBorderSecondary}` 
                }}
              />
            </Badge>
            
            <Dropdown menu={{ items: profileMenuItems, onClick: handleProfileMenuClick }} trigger={['click']}>
              <Flex align="center" gap="middle" style={{ cursor: 'pointer' }}>
                <Avatar 
                  size={36} 
                  style={{ backgroundColor: '#cbd5e1' }}
                  icon={<UserOutlined />}
                />
                <div style={{ lineHeight: 1 }}>
                  <Typography.Text strong style={{ display: 'block', fontSize: 14 }}>
                    {user?.name || user?.email?.split('@')[0] || 'User'}
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

