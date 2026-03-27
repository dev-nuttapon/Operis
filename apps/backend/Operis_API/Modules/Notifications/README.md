# notifications backend module

Purpose:

* owns notification persistence and delivery orchestration
* owns Phase 24 notification queue management endpoints

Public surface:

* `NotificationsModule.cs`
* `Application/`
* `Contracts/`

Owned data:

* `notifications`
* `notification_queue`

Notes:

* keep endpoints thin and delegate logic to `Application/`
* queue retry validation and lifecycle state changes stay in `Application/`
