import React from "react";
import { Alert, Button, Card, Typography } from "antd";

type Props = {
  children: React.ReactNode;
};

type State = {
  error: Error | null;
  errorInfo: React.ErrorInfo | null;
};

export class AppErrorBoundary extends React.Component<Props, State> {
  state: State = { error: null, errorInfo: null };

  static getDerivedStateFromError(error: Error): State {
    return { error, errorInfo: null };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    // eslint-disable-next-line no-console
    console.error("AppErrorBoundary caught an error:", error, errorInfo);
    this.setState({ error, errorInfo });
  }

  render() {
    if (!this.state.error) return this.props.children;

    return (
      <Card variant="borderless" style={{ borderRadius: 16 }}>
        <Alert
          type="error"
          showIcon
          message="UI crashed"
          description="A UI error occurred while rendering this page. Check the details below (and browser console) to fix the underlying issue."
        />
        <div style={{ marginTop: 12 }}>
          <Typography.Text strong>Message</Typography.Text>
          <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>{this.state.error.message}</pre>
        </div>
        {this.state.errorInfo?.componentStack ? (
          <div style={{ marginTop: 12 }}>
            <Typography.Text strong>Component stack</Typography.Text>
            <pre style={{ margin: "6px 0 0", whiteSpace: "pre-wrap" }}>{this.state.errorInfo.componentStack}</pre>
          </div>
        ) : null}
        <div style={{ marginTop: 12, display: "flex", gap: 8, justifyContent: "flex-end" }}>
          <Button onClick={() => this.setState({ error: null, errorInfo: null })}>Dismiss</Button>
          <Button type="primary" onClick={() => window.location.reload()}>
            Reload
          </Button>
        </div>
      </Card>
    );
  }
}

