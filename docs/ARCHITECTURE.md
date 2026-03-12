# ARCHITECTURE.md

System architecture overview.

---

# High-Level Architecture

```
User Browser
     |
     v
   Traefik
     |
  ---------
  |       |
Frontend  Backend API
     |
     v
Infrastructure Services
```

---

# Infrastructure

Core infrastructure

* PostgreSQL
* Redis
* MinIO
* Keycloak
* Traefik

Observability

* Prometheus
* Loki
* Tempo
* Grafana

---

# Backend Architecture

The backend follows **Modular Monolith**.

Current module shape:

```
Modules/
  Users/
    Application/
    Contracts/
    Domain/
    Infrastructure/
    UsersModule.cs
  Audits/
    Application/
    Contracts/
    AuditsModule.cs
  Documents/
    Application/
    Contracts/
    Infrastructure/
    DocumentsModule.cs
```

Module rules:

* `*Module.cs` is composition only
* `Application/` owns query and command orchestration
* `Contracts/` owns API-facing request/response contracts
* `Domain/` holds business concepts when needed
* `Infrastructure/` owns persistence and external integrations

---

# Frontend Architecture

Frontend follows **Feature-Based Modular Architecture**

```
src/
  modules/
  shared/
  app/
```

Each module should contain:

* `pages`
* `hooks`
* `api`
* `components`
* `types`
* `index.ts` or `public.ts`

Readiness reference

* `docs/MODULE_READINESS.md`
* `docs/MODULE_CONTRACTS.md`
* `docs/DATA_OWNERSHIP.md`

```

---

# Security Architecture

Authentication handled by **Keycloak**

Flow

```

User
→ Keycloak Login
→ JWT Token
→ Backend validation

```

---

# Storage

Files stored in

```

MinIO

```

Metadata stored in

```

PostgreSQL

```

---

# Observability

Logs

```

Loki

```

Metrics

```

Prometheus

```

Tracing

```

Tempo

```

Visualization

```

Grafana

```
```
