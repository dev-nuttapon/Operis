# users backend module

Purpose:

* owns user lifecycle, invitations, registration, preferences, and reference data

Public surface:

* `UsersModule.cs`
* `Application/` query and command services
* `Contracts/`

Owned data:

* users
* user registration requests
* user invitations
* departments
* job titles
* divisions
* project roles
* user org assignments
* reporting lines
* projects
* user project assignments
* application roles

Notes:

* `UsersModule.cs` stays as endpoint composition only
* persistence, caching, and Keycloak orchestration stay in `Application/` or `Infrastructure/`
