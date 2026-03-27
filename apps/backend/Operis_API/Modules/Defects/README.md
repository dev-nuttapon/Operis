# defects backend module

Purpose:

* owns Phase 15 defect and non-conformance records, lifecycle transitions, and quality-incident audit controls

Public surface:

* `DefectsModule.cs`
* `Application/IDefectQueries.cs`
* `Application/IDefectCommands.cs`
* `Contracts/`

Owned data:

* `defects`
* `non_conformances`

Notes:

* endpoints stay thin and delegate lifecycle validation to `Application/`
* closure rules enforce resolution summary for defects and corrective-action or accepted-disposition evidence for non-conformances
