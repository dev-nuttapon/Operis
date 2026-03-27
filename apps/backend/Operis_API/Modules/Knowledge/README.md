# knowledge backend module

Purpose:

* owns Phase 18 lessons learned repository and publication workflow

Public surface:

* `KnowledgeModule.cs`
* `Application/IKnowledgeQueries.cs`
* `Application/IKnowledgeCommands.cs`
* `Contracts/`

Owned data:

* `lessons_learned`

Notes:

* endpoints stay thin and delegate workflow and publication validation to `Application/`
* lesson publication remains server-driven so context, summary, and evidence/source checks stay consistent
