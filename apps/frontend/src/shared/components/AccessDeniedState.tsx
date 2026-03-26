import { Alert, Button, Result } from "antd";
import { useNavigate } from "react-router-dom";

export function AccessDeniedState() {
  const navigate = useNavigate();

  return (
    <Result
      status="403"
      title="Access denied"
      subTitle="You do not have permission to open this screen."
      extra={[
        <Button key="back" onClick={() => navigate("/app", { replace: true })}>
          Back to app
        </Button>,
      ]}
    >
      <Alert
        type="warning"
        showIcon
        message="Phase 0 authorization is enforced for protected routes and actions."
      />
    </Result>
  );
}
