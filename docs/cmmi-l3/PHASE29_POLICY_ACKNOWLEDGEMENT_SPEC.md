# Phase 29: Policy Acknowledgement

## Goal

Govern policy publication, acknowledgement campaigns, and overdue attestation in a way that produces stable audit evidence and reusable compliance signals.

## In Scope

* policy register
* policy workflow transitions
* acknowledgement campaign launch and close
* targeted acknowledgement records
* overdue acknowledgement reporting

## Out of Scope

* external policy distribution channels
* document authoring for policy body content
* LMS or HR-driven attestation imports

## Owning Module

* `Governance`

## Owned Tables

* `policies`
* `policy_campaigns`
* `policy_acknowledgements`

## Read-Only Upstream Dependencies

* `Users` for target user resolution by global, department, and project scopes
* `Audits` for evidence consumption only
* `Governance` approval evidence logging

## Routes

* `/app/governance/policies`
* `/app/governance/policy-acknowledgements`

## Permissions

* `governance.policies.read`
* `governance.policies.manage`
* `governance.policies.approve`

## Backend Contracts

* `GET /api/v1/governance/policies`
* `POST /api/v1/governance/policies`
* `PUT /api/v1/governance/policies/{id}`
* `POST /api/v1/governance/policies/{id}/transition`
* `GET /api/v1/governance/policy-campaigns`
* `POST /api/v1/governance/policy-campaigns`
* `PUT /api/v1/governance/policy-campaigns/{id}`
* `POST /api/v1/governance/policy-campaigns/{id}/transition`
* `GET /api/v1/governance/policy-acknowledgements`
* `POST /api/v1/governance/policy-acknowledgements`

## Frontend Structure

Follow the standard pattern:

* `Page -> Hook -> API -> HTTP Client`

Primary files:

* `apps/frontend/src/modules/governance/pages/PoliciesPage.tsx`
* `apps/frontend/src/modules/governance/pages/PolicyAcknowledgementsPage.tsx`
* `apps/frontend/src/modules/governance/hooks/useGovernance.ts`
* `apps/frontend/src/modules/governance/api/governanceApi.ts`
* `apps/frontend/src/modules/governance/types/governance.ts`

## Validation And Error Codes

* `policy_title_required`
* `policy_effective_date_required`
* `policy_campaign_scope_required`
* `policy_not_found`
* `policy_campaign_not_found`
* `policy_acknowledgement_not_found`
* `policy_code_duplicate`
* `policy_campaign_code_duplicate`
* `invalid_workflow_transition`

## Workflow States

Policy:

* `draft -> approved -> published -> retired`

Campaign:

* `draft -> launched -> closed`

Acknowledgement:

* `pending -> acknowledged`

## Business Rules

* a policy must have title and effective date before approval or publication
* a campaign can be launched only for a published policy
* a campaign must define target scope type and scope reference
* launch resolves target users and creates missing acknowledgement rows
* acknowledgement is restricted to assigned users
* overdue status is computed from `due_at` plus pending acknowledgement state

## Audit Requirements

* policy approval and publication actions must write governance approval evidence
* campaign launch and user acknowledgement must be audit-visible through normal module logging

## Tests Required

Backend:

* create policy without title returns `policy_title_required`
* create campaign without scope returns `policy_campaign_scope_required`
* handler forbids policy list access without permission

Frontend:

* acknowledgement list renders overdue state
* acknowledgement modal can open for pending items

## Quality Gates

* `dotnet build apps/backend/Operis_API/Operis_API.csproj`
* `dotnet test apps/backend/Operis_API.Tests/Operis_API.Tests.csproj`
* `node scripts/check-backend-architecture.mjs`
* `node scripts/check-module-contracts.mjs`
* `npm run check:architecture`
* `npm test`
* `npm run build:local`
* `npm run perf:bundle-report`
* `npm run perf:bundle-budget`

## Acceptance Criteria

* policy managers can create and edit policies and campaigns
* policy approvers can approve and publish policies and launch campaigns
* targeted users can view and acknowledge assigned campaigns
* overdue acknowledgements are visible in list and summary filters
* policy and campaign records remain owned by `Governance`
