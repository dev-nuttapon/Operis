# documents backend module

Purpose:

* owns document listing contracts and document query orchestration

Public surface:

* `DocumentsModule.cs`
* `Application/DocumentQueries.cs`
* `Contracts/`

Owned data:

* documents

Notes:

* keep document reads bounded and no-tracking
* future write flows should stay behind application services
