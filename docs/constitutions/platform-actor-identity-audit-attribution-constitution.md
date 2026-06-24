# STL Compliance Actor Identity and Audit Attribution Constitution

## 1. Audit drivers

SEC-008 found caller-supplied actor/person IDs. SEC-012 found NexArr user identity used as StaffArr person identity.

## 2. Prime directive

The system, not the request body, determines who performed an action. User account identity, StaffArr person identity, service identity, delegated subject, and affected record are separate concepts.

## 3. Human actions

For human actions, audit attribution is derived from validated session claims and an authoritative user-to-person link. Requests may not set `createdBy`, `updatedBy`, `approvedBy`, `uploadedBy`, or equivalent actor fields.

## 4. Delegated actions

“On behalf of” actions require:

- explicit permission
- authenticated initiating actor
- separate delegated subject
- reason code and optional note
- start/end or one-time scope
- immutable audit entry

The delegated subject never replaces the initiating actor.

## 5. Service actions

Service-generated events record:

- service client ID and product key
- tenant context
- initiating human when propagated and trusted
- source record and correlation/causation IDs
- scope used
- reason or workflow stage

## 6. Audit record minimum

Sensitive events include actor type, actor ID, person ID when resolved, tenant, product, permission/scope, target reference, previous/new state, reason, evidence refs, IP/device context where appropriate, correlation, causation, and timestamp.

## 7. Identity boundary

NexArr `userId` is an account identifier. StaffArr `personId` is the human/business-person identifier. They may be linked but are never interchangeable.

## 8. UI rule

Ordinary pages show human-readable actor names. Raw IDs are secondary technical metadata on permissioned admin/audit surfaces only.
