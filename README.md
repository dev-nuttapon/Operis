# Frontend Architecture Guide

เอกสารนี้อธิบาย **โครงสร้าง Frontend ของระบบ Paperless Platform** เพื่อให้โค้ดมีมาตรฐาน สามารถขยายระบบได้ง่าย และรองรับการ **refactor ในระยะยาว**

โครงสร้างนี้ใช้แนวคิด **Feature / Module-based Architecture** แทนการจัดโค้ดตามประเภทไฟล์ (เช่น `components/`, `hooks/`, `services/` ทั้งโปรเจกต์)

แนวทางนี้ช่วยให้

* แยก **business domain** ชัดเจน
* ลด dependency ข้าม module
* refactor ได้ง่าย
* เพิ่ม feature ใหม่โดยไม่กระทบระบบเดิม

---

# Tech Stack

Frontend ใช้ stack ดังนี้

* React
* TypeScript
* React Router
* Ant Design
* TanStack Query
* react-hook-form
* Zod

---

# Project Structure

```
src/
  app/
  modules/
  shared/
  providers/
  routes/
  types/
  styles/
```

---

# Directory Explanation

## app/

ใช้สำหรับ bootstrap application

```
app/
  App.tsx
  router.tsx
  routes.tsx
  layout/
    AppLayout.tsx
    AuthLayout.tsx
```

หน้าที่หลัก

* root component
* router configuration
* layout หลักของระบบ
* route guards

---

## modules/

เป็นหัวใจของระบบ
ทุก feature หรือ business domain ต้องอยู่ใน modules

ตัวอย่าง

```
modules/
  auth/
  dashboard/
  documents/
  workflows/
  approvals/
  audits/
  settings/
  users/
  organizations/
```

แต่ละ module เป็น **self-contained feature**

---

## shared/

สำหรับโค้ดที่ใช้ร่วมกันหลาย module

```
shared/
  api/
  components/
  hooks/
  utils/
  constants/
  types/
```

ตัวอย่าง

```
shared/
  api/
    httpClient.ts
    queryClient.ts

  components/
    PageHeader/
    AppTable/
    AppForm/
    AppModal/

  hooks/
    useDebounce.ts
    usePagination.ts

  utils/
    date.ts
    download.ts

  constants/
    roles.ts
    app.ts
```

**กฎสำคัญ**

ห้ามนำโค้ดเข้า shared หากยังใช้แค่ module เดียว

---

## providers/

รวบรวม React Providers

```
providers/
  AppProviders.tsx
  QueryProvider.tsx
  ThemeProvider.tsx
```

เช่น

* QueryClientProvider
* Ant Design ConfigProvider
* AuthProvider

---

## routes/

ถ้าแยก route configuration ออกจาก `app`

```
routes/
  index.ts
  protectedRoutes.ts
```

---

## types/

Global types ที่ใช้ข้าม module

```
types/
  api.ts
  common.ts
```

---

## styles/

Global styling

```
styles/
  global.css
  theme.ts
```

---

# Module Structure

แต่ละ module ควรมีโครงสร้างคล้ายกันเพื่อให้ง่ายต่อการพัฒนา

ตัวอย่าง module `documents`

```
modules/
  documents/
    api/
    components/
    hooks/
    pages/
    schemas/
    services/
    store/
    types/
    utils/
    index.ts
```

---

# Folder Responsibility

## api/

ฟังก์ชันเรียก backend API

```
api/
  getDocuments.ts
  createDocument.ts
  updateDocument.ts
```

---

## components/

UI components ของ module

```
components/
  DocumentTable.tsx
  DocumentForm.tsx
  DocumentStatusTag.tsx
```

---

## hooks/

Custom hooks

```
hooks/
  useDocuments.ts
  useCreateDocument.ts
```

---

## pages/

React pages

```
pages/
  DocumentListPage.tsx
  DocumentCreatePage.tsx
  DocumentDetailPage.tsx
```

---

## schemas/

Form validation schema

```
schemas/
  documentForm.schema.ts
```

ใช้ Zod

---

## services/

Business logic ที่ไม่ใช่ UI

```
services/
  mapDocumentFormToRequest.ts
```

---

## store/

State เฉพาะ module (ถ้าจำเป็น)

---

## types/

Types ของ module

```
types/
  document.ts
```

---

## utils/

Helper functions

---

## index.ts

Public API ของ module

ตัวอย่าง

```ts
export * from "./pages/DocumentListPage";
export * from "./pages/DocumentDetailPage";
export * from "./hooks/useDocuments";
```

---

# Data Flow Pattern

Page ห้ามเรียก API ตรง

ถูกต้อง

```
Page
 -> Hook
 -> API
 -> HTTP Client
 -> Backend
```

ตัวอย่าง

```
DocumentListPage
 -> useDocuments()
 -> getDocuments()
 -> httpClient
```

---

# Server State vs UI State

ต้องแยก state ให้ชัด

## Server State

ข้อมูลจาก backend

ตัวอย่าง

* document list
* workflow detail
* user profile

ใช้

```
TanStack Query
```

---

## UI State

สถานะของ UI

ตัวอย่าง

* modal open
* filter form
* selected row

ใช้

```
useState
```

---

# Import Rules

เพื่อป้องกัน dependency chaos

กฎคือ

```
modules A ห้าม import internal code ของ modules B
```

ต้อง import ผ่าน

```
modules/B/index.ts
```

---

# Naming Convention

เพื่อให้โค้ดอ่านง่าย

| Type      | Example                |
| --------- | ---------------------- |
| Page      | DocumentListPage.tsx   |
| Component | DocumentTable.tsx      |
| Hook      | useDocuments.ts        |
| Schema    | documentForm.schema.ts |
| Type      | document.ts            |

---

# Routing Convention

```
/documents
/documents/create
/documents/:id
/workflows
/approvals/inbox
/audits/logs
```

---

# Best Practices

1. แยกตาม module/domain ก่อน
2. page ห้ามเรียก API ตรง
3. server state ใช้ TanStack Query
4. form ใช้ react-hook-form + zod
5. shared ใช้เมื่อ shared จริง
6. ทุก module มี index.ts
7. import ข้าม module ต้องผ่าน public API

---

# Goal of This Architecture

โครงสร้างนี้ออกแบบมาเพื่อให้

* scalable
* maintainable
* easy to refactor
* suitable for large enterprise systems

เหมาะสำหรับระบบ

* Paperless Platform
* Workflow Platform
* Compliance / ISO / CMMI Platform
