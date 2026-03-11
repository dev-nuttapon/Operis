# Frontend

## Source Structure

```text
src/
  app/
  modules/
  shared/
  providers/
  routes/
  types/
  styles/
```

## Environment Modes

รองรับ 3 environment:
- `local`
- `dev`
- `prod`

ไฟล์ที่ใช้:
- `.env.app-local`
- `.env.dev`
- `.env.prod`

## Commands

- `npm run dev:local`
- `npm run dev:dev`
- `npm run dev:prod`

- `npm run build:local`
- `npm run build:dev`
- `npm run build:prod`

- `npm run preview:local`
- `npm run preview:dev`
- `npm run preview:prod`

## Notes

- ให้ frontend เรียก API ผ่าน Traefik route เท่านั้น (`/api`, `/auth`)
- อย่าเรียก backend container โดยตรง
- Keycloak client สำหรับ frontend: `operis-inventory-web` (realm: `saas`)
- ห้ามเก็บ `operis-inventory-admin-api` client secret ใน frontend
