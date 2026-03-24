# notifications backend module

Purpose:

* owns notification persistence and delivery orchestration

Public surface:

* `NotificationsModule.cs`
* `Application/`
* `Contracts/`

Owned data:

* notification-related tables (when introduced)

Notes:

* keep endpoints thin and delegate logic to `Application/`
