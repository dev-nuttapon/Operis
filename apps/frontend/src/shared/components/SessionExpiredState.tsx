import { Button, Result } from "antd";
import { login } from "../../modules/auth";

export function SessionExpiredState() {
  return (
    <Result
      status="warning"
      title="Session expired"
      subTitle="Your secure session expired or could not be refreshed. Sign in again to continue."
      extra={[
        <Button key="login" type="primary" onClick={() => void login("/app")}>
          Sign in again
        </Button>,
      ]}
    />
  );
}
