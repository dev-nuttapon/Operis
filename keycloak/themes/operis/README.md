Keycloak custom theme: operis

Structure:
- login/   : login UI branding and locale messages
- email/   : email templates/messages
- account/ : account console messages

Quick start:
1. Copy or mount this folder into Keycloak themes directory.
2. In Realm settings -> Themes:
   - Login theme: operis
   - Email theme: operis
   - Account theme: operis
3. In Realm settings -> Login:
   - User registration: OFF (use custom link to your app)
   - Forgot password: OFF (use custom link to your app)
4. In Clients -> `operis-web`:
   - Valid redirect URIs and Web origins must be configured correctly.
5. Clear browser cache or use private window for testing.

Custom links on login page:
- Register: `http://localhost:5173/register`
- Forgot password: Keycloak built-in reset credentials flow
- Configure base URL in: `login/theme.properties` (`oiAppBaseUrl`)

Notes:
- This theme currently uses parent templates for compatibility.
- Customization is done via CSS and message bundles.
- Add FTL overrides only when needed.


Persistent setup (recommended):
1. Do NOT rely on `docker cp` as the only deployment method for themes.
2. Mount theme from host into container so it survives recreate/reset.

Example Docker Compose override (keycloak service):
```yaml
services:
  keycloak:
    volumes:
      - ./themes/operis:/opt/keycloak/themes/operis:ro
    environment:
      KC_SPI_THEME_CACHE_THEMES: "false"
      KC_SPI_THEME_CACHE_TEMPLATES: "false"
```
