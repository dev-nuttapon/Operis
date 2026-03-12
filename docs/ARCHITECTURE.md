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

The backend follows **Modular Monolith**

```
Modules/
  Users
  Organizations
  Documents
  Files
  Workflows
  Approvals
  Audits
  Notifications
  Reports
  Settings
```

Each module contains

* Controllers
* Services
* Repositories
* DTOs
* Domain Models

---

# Frontend Architecture

Frontend follows **Feature-Based Modular Architecture**

```
src/
  modules/
  shared/
  app/
```

Each module contains

* pages
* hooks
* api
* components
* services
* types

Readiness reference

* `docs/MODULE_READINESS.md`

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
