# Endpoint Authorization Map Template

## Rule

Every production endpoint, hub, worker command, upload URL, callback, portal route, and service action must appear in a machine-checkable authorization inventory. Missing inventory entries fail CI.

## Required columns

| Field | Meaning |
|---|---|
| Product/host | Owning executable and product |
| Method + route/action | Exact route template or message action |
| Exposure | browser, service, portal, public intake, worker, internal callback |
| Authentication | required scheme or intentionally anonymous reason |
| Tenant source | validated claim, service token, signed portal grant, or owner context |
| Actor source | account/person/service/delegated actor chain |
| Product permission | product-local permission key or `none` with justification |
| Scope rule | tenant/site/department/team/record/customer/supplier/etc. |
| Workflow rule | allowed record states and blocker/qualification checks |
| Sensitive data class | ordinary, confidential, credential, financial, evidence, regulated |
| Rate/size limit | applicable request and abuse controls |
| Audit event | success/denial/transition event requirement |
| Negative tests | anonymous, forbidden, wrong tenant, spoofed actor, invalid state |
| Owner | accountable product/team |

## Example

| Product/host | Method + route | Exposure | Authentication | Tenant source | Actor source | Permission | Workflow | Negative tests |
|---|---|---|---|---|---|---|---|---|
| AssurArr API | `POST /api/v1/nonconformances/{id}/holds` | browser | NexArr session | validated tenant claim | account + StaffArr person mapping | `assurarr.holds.place` | issue open; object not released | 401, 403, wrong tenant, closed issue, spoofed actor |
| RecordArr API | `POST /api/v1/records/{id}/files` | browser/service | session or scoped service token | validated context | human or service delegation | `recordarr.files.upload` | record writable; no purge | oversize, bad type, wrong tenant, no policy, quarantined delivery |

## CI contract

Static route discovery is compared with this inventory. A newly discovered route without an approved row fails. Rows referring to nonexistent routes fail. Every sensitive row has executable negative tests and a permission registered in the canonical product permission catalog.
