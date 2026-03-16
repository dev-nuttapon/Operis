# documents backend module

Purpose:

* owns document metadata, upload orchestration, download orchestration, and document listing
* stores file binaries in MinIO through infrastructure services, not directly from the frontend

Public surface:

* `DocumentsModule.cs`
* `Application/DocumentQueries.cs`
* `Application/DocumentCommands.cs`
* `Application/DocumentDownloads.cs`
* `Contracts/`

Owned data:

* documents

Infrastructure:

* `MinioDocumentObjectStorage`
* `DocumentStorageOptions`

Notes:

* frontend upload flow must stay `Frontend -> Backend -> MinIO`
* metadata writes stay in the documents module
* object storage access stays in `Infrastructure/`
