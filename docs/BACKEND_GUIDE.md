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
