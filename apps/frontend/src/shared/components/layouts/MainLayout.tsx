import { useState } from 'react';
import { Layout, Menu, Button, Space, Typography, Select, Flex } from 'antd';
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  FileTextOutlined,
  GlobalOutlined,
  BulbOutlined,
  BulbFilled,
  LogoutOutlined,
} from '@ant-design/icons';
import { useNavigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../../../modules/auth/hooks/useAuth';
import { useThemeStore, ThemeMode } from '../../store/useThemeStore';

const { Header, Sider, Content } = Layout;

export function MainLayout() {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { logout, isAuthenticated } = useAuth();
  const { theme, setTheme } = useThemeStore();

  const menuItems = [
    {
      key: '/app/documents',
      icon: <FileTextOutlined />,
      label: 'Documents',
    },
    // Future placeholders
    // { key: '/app/workflows', icon: <NodeIndexOutlined />, label: 'Workflows' }
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider 
        trigger={null} 
        collapsible 
        collapsed={collapsed}
        theme={theme === 'dark' ? 'dark' : 'light'}
        style={{ borderRight: theme === 'dark' ? '1px solid #303030' : '1px solid #f0f0f0' }}
      >
        <div 
          style={{ 
            height: 64, 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            borderBottom: theme === 'dark' ? '1px solid #303030' : '1px solid #f0f0f0' 
          }}
        >
          <Typography.Title level={4} style={{ margin: 0, fontWeight: 800 }}>
            {collapsed ? 'OP' : 'OPERIS'}
          </Typography.Title>
        </div>
        <Menu
          theme={theme === 'dark' ? 'dark' : 'light'}
          mode="inline"
          selectedKeys={[location.pathname]}
          onClick={({ key }) => navigate(key)}
          items={menuItems}
          style={{ borderRight: 0, marginTop: 16 }}
        />
      </Sider>
      
      <Layout>
        <Header 
          style={{ 
            padding: '0 24px', 
            background: theme === 'dark' ? '#141414' : '#ffffff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: theme === 'dark' ? '1px solid #303030' : '1px solid #f0f0f0',
          }}
        >
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed(!collapsed)}
            style={{ fontSize: '16px', width: 64, height: 64, marginLeft: -24 }}
          />

          <Flex align="center" gap="middle">
            <Space size="small" style={{ marginRight: 16 }}>
              <GlobalOutlined style={{ fontSize: 16 }} />
              <Select
                value="en"
                variant="borderless"
                options={[
                  { value: 'en', label: 'EN' },
                  { value: 'th', label: 'TH' }
                ]}
              />
            </Space>

            <Select 
              value={theme}
              onChange={(v: ThemeMode) => setTheme(v)}
              variant="borderless"
              options={[
                { value: 'light', label: <Space><BulbOutlined /> Light</Space> },
                { value: 'dark', label: <Space><BulbFilled /> Dark</Space> },
                { value: 'system', label: <Space>System</Space> }
              ]}
            />
            
            <div style={{ width: 1, height: 24, background: theme === 'dark' ? '#303030' : '#f0f0f0', margin: '0 8px' }} />

            {isAuthenticated ? (
              <Button type="text" onClick={() => void logout()} icon={<LogoutOutlined />} danger>
                 Logout
              </Button>
            ) : null}
          </Flex>
        </Header>
        
        <Content
          style={{
            margin: '24px',
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
