# FRONTEND_GUIDE.md

Frontend development guide.

---

# Stack

* React
* TypeScript
* Ant Design
* TanStack Query
* React Hook Form
* Zod

---

# Project Structure

```
src/
  app/
  modules/
  shared/
  providers/
```

---

# Module Structure

```
modules/
  documents/
    api/
    components/
    hooks/
    pages/
    schemas/
    services/
    types/
```

---

# Data Flow

```
Page
 → Hook
 → API
 → HTTP Client
```

---

# Form Pattern

Forms must use

* React Hook Form
* Zod schema validation

---

# Server State

All API data must be managed by

```
TanStack Query
```

---

# UI Components

Base components should be placed in

```
shared/components
```

Examples

* AppTable
* AppModal
* AppForm
* PageHeader
