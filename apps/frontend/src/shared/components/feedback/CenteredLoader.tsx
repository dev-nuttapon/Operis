interface CenteredLoaderProps {
  label?: string;
  minHeight?: string;
}

export function CenteredLoader({ label, minHeight = "100vh" }: CenteredLoaderProps) {
  return (
    <div
      aria-live="polite"
      style={{
        minHeight,
        display: "grid",
        placeItems: "center",
        padding: 24,
      }}
    >
      <div style={{ display: "grid", gap: 12, justifyItems: "center" }}>
        <div
          aria-label={label ?? "Loading"}
          data-testid="centered-loader"
          style={{
            width: 36,
            height: 36,
            borderRadius: "50%",
            border: "3px solid rgba(148, 163, 184, 0.28)",
            borderTopColor: "#0ea5e9",
            animation: "operis-spin 0.9s linear infinite",
          }}
        />
        {label ? (
          <p style={{ margin: 0, color: "#64748b", fontSize: 14 }}>
            {label}
          </p>
        ) : null}
      </div>
      <style>{`
        @keyframes operis-spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
}
