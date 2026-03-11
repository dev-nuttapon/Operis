# MODULE_TEMPLATE.md

Template for creating a new module.

---

# Module Folder

```
modules/
  module-name/
    api/
    components/
    hooks/
    pages/
    schemas/
    services/
    types/
    index.ts
```

---

# Example

```
modules/documents/
  api/getDocuments.ts
  components/DocumentTable.tsx
  hooks/useDocuments.ts
  pages/DocumentListPage.tsx
  schemas/documentForm.schema.ts
  services/documentService.ts
  types/document.ts
```

---

# Rules

1. Pages must be thin.
2. Business logic belongs in services or hooks.
3. API calls must be isolated in api/.
4. Shared code must not live inside module.
