# risks backend module

Purpose:

* owns Phase 5 risk register, issue log, and issue-action workflow
* enforces lifecycle validation for risk mitigation and issue closure

Public surface:

* `RisksModule.cs`
* `Application/IRiskQueries.cs`
* `Application/IRiskCommands.cs`
* `Contracts/`

Owned data:

* risks
* risk_reviews
* issues
* issue_actions

Notes:

* endpoints stay thin and delegate workflow and visibility rules to `Application/`
* sensitive issues are hidden from users who lack `risks.sensitive.read`
