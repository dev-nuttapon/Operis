interface StatusPanelProps {
  status?: "error" | "success" | "warning";
  subtitle?: string;
  title: string;
}

const toneMap = {
  error: {
    accent: "#dc2626",
    background: "rgba(254, 226, 226, 0.9)",
    border: "rgba(248, 113, 113, 0.45)",
  },
  success: {
    accent: "#16a34a",
    background: "rgba(220, 252, 231, 0.9)",
    border: "rgba(74, 222, 128, 0.45)",
  },
  warning: {
    accent: "#d97706",
    background: "rgba(254, 243, 199, 0.9)",
    border: "rgba(251, 191, 36, 0.45)",
  },
} as const;

export function StatusPanel({ status = "error", subtitle, title }: StatusPanelProps) {
  const tone = toneMap[status];

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        padding: 24,
        background: "linear-gradient(180deg, #f8fafc 0%, #e2e8f0 100%)",
      }}
    >
      <div
        style={{
          width: "100%",
          maxWidth: 520,
          padding: 32,
          borderRadius: 24,
          background: tone.background,
          border: `1px solid ${tone.border}`,
          boxShadow: "0 24px 60px rgba(148, 163, 184, 0.18)",
          textAlign: "center",
        }}
      >
        <div
          aria-hidden="true"
          style={{
            width: 56,
            height: 56,
            margin: "0 auto 16px",
            borderRadius: "50%",
            background: tone.accent,
            opacity: 0.14,
          }}
        />
        <h1 style={{ margin: 0, color: "#0f172a", fontSize: 28, lineHeight: 1.2 }}>
          {title}
        </h1>
        {subtitle ? (
          <p style={{ margin: "12px 0 0", color: "#475569", fontSize: 15, lineHeight: 1.6 }}>
            {subtitle}
          </p>
        ) : null}
      </div>
    </div>
  );
}
