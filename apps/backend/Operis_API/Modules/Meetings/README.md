# meetings backend module

Purpose:

* owns Phase 6 meeting records, meeting minutes, attendees, and decision workflows
* enforces minutes approval prerequisites and decision apply prerequisites

Public surface:

* `MeetingsModule.cs`
* `Application/IMeetingQueries.cs`
* `Application/IMeetingCommands.cs`
* `Contracts/`

Owned data:

* meeting_records
* meeting_minutes
* meeting_attendees
* decisions

Notes:

* endpoints stay thin and delegate workflow validation to `Application/`
* restricted meetings and decisions are hidden from users who lack `meetings.restricted.read`
