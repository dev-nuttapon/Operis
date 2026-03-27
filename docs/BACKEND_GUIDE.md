# BACKEND_GUIDE.md

Backend development guide.

---

# Backend Stack

* ASP.NET Core
* Entity Framework Core
* PostgreSQL

---

# Project Structure

```
src/
  Modules/
  Infrastructure/
  Shared/
```

---

# Module Structure

```
Modules/
  Documents/
    Controllers
    Services
    Repositories
    DTOs
    Models
```

---

# Database

Primary database

```
PostgreSQL
```

---

# Caching

```
Redis
```

---

# File Storage

```
MinIO
```

Files stored in object storage.

Metadata stored in PostgreSQL.

---

# Authentication

Handled by

```
Keycloak
```

Backend validates JWT tokens.

---

# Logging

Logs exported to

* Loki
* Prometheus metrics
* Tempo traces

---

# EF Tooling

Keep the local `dotnet-ef` tool version aligned with the `Microsoft.EntityFrameworkCore` package version used by [Operis_API.csproj](/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Operis_API.csproj).

Current repository note:

* `apps/backend/Operis_API/dotnet-tools.json` is pinned to `10.0.5`
* several newer phase migrations were authored manually before the tooling version was aligned
* do not scaffold a bulk "sync" migration on top of the current snapshot without auditing the generated diff first, because EF may try to restate historical schema changes
