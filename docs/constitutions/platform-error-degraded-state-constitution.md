# STL Compliance Error, Degraded State, and Source Unavailable Constitution

## 1. Purpose

This constitution defines how STL Compliance communicates errors, partial failures, stale data, source outages, permission problems, and degraded operation without hiding risk or confusing users.

## 2. Scope

This constitution applies to:

- API errors
- UI error states
- Section-level failures
- Source product unavailable states
- External integration failures
- Stale snapshots
- Read model degradation
- Validation errors
- Permission denied/forbidden states
- Retry behavior
- Background job failures
- Sync failures

## 3. Prime directive

A source outage must be visible.

Stale data must be labeled.

Partial failure must not masquerade as healthy state.

User-facing errors must be plain business language, not raw technical payloads.

## 4. Error categories

Recommended error categories:

- `validation_error`
- `permission_denied`
- `not_found`
- `source_unavailable`
- `stale_data`
- `conflict`
- `blocked_by_rule`
- `blocked_by_workflow`
- `integration_failure`
- `sync_failure`
- `timeout`
- `rate_limited`
- `system_error`

## 5. Validation errors

Validation errors tell the user what is wrong and how to fix it.

They should identify:

- Field or section
- Problem
- Required correction
- Whether it blocks submission
- Source product/catalog when cross-product

Avoid generic messages like `Invalid input`.

## 6. Permission errors

Permission errors must not leak sensitive data.

If the record or section existence is sensitive, hide it or show a safe forbidden state.

If the user can know the record exists but not perform the action, explain the missing authority when safe.

## 7. Source unavailable

When a source product is unavailable, the UI/API should show:

- Which source is unavailable
- What data/actions are affected
- Whether a safe snapshot is shown
- Snapshot time/freshness
- Whether retry is available
- Whether the workflow is blocked or can continue pending review

Do not silently hide failed source data.

## 8. Stale data

Stale data must be labeled.

Staleness metadata should include:

- Last successful refresh
- Source product
- Expected freshness
- Staleness reason if known
- Refresh/retry option where allowed

Stale readiness or compliance data must not be displayed as current clearance.

## 9. Partial page failure

Pages should degrade by section when possible.

A failed evidence panel should not crash an entire detail page if the main record can render.

A failed chart should not prevent dashboard KPI cards from loading.

A failed cross-product signal should show a section-level warning.

## 10. Blocked actions

Blocked state-changing actions should explain:

- What is blocked
- Why it is blocked
- Source of blocker
- Required clearing action
- Whether override exists
- Who/which product owns the clearing action

Example:

`Trip cannot be dispatched because vehicle TRK-1042 is blocked by MaintainArr: annual inspection expired. Open asset readiness.`

## 11. Retry behavior

Retry must be explicit.

Do not repeatedly retry state-changing actions in a way that can duplicate work.

Retries for writes must use idempotency.

Background retries must be visible in operational/admin views when failures persist.

## 12. External integration failures

External integration failures should show:

- External system
- Last successful sync
- Last failed sync
- Affected records
- Retry/manual review state
- Business impact

External failure must not silently overwrite STL source truth.

## 13. Offline/sync failures

Mobile/offline sync failures must show:

- Operation
- Owning product
- Record/context
- Whether data remains local
- Retry option
- Conflict or rejection reason
- Whether action is confirmed or pending

## 14. API error response

Recommended API error shape:

```json
{
  "error": {
    "code": "SOURCE_UNAVAILABLE",
    "category": "source_unavailable",
    "message": "MaintainArr readiness is temporarily unavailable. Dispatch release cannot be confirmed.",
    "sourceProduct": "MaintainArr",
    "retryable": true,
    "blocked": true,
    "correlationId": "..."
  }
}
```

Do not expose stack traces, database exceptions, raw JSON payloads, or secrets to normal users.

## 15. Degraded dashboards and reports

Dashboards and reports with degraded data must show:

- Partial source status
- Missing source(s)
- Stale source(s)
- Whether metrics exclude unavailable data
- Whether values are snapshots

## 16. Error severity

Recommended severity labels:

- `critical`
- `high`
- `medium`
- `low`
- `info`

Operational labels:

- `blocked`
- `degraded`
- `stale`
- `retrying`
- `needs_review`

Severity must be text-readable.

## 17. Technical diagnostics

Technical details belong in admin/debug/logging surfaces, not ordinary user workflows.

Diagnostics may include:

- Stack trace
- Raw payload
- Request/response body
- Service-token claims
- Database error

Only authorized technical/admin users should see this data.

## 18. Anti-patterns

The following are not allowed:

- Generic full-page failure for one failed widget
- Hiding failed cross-product sources
- Showing stale data as live
- Silent retries that duplicate writes
- Raw stack traces to ordinary users
- Permission errors that leak sensitive data
- Blocking actions with no explanation
- External sync failures hidden from operational users/admins
- Treating cached readiness as current clearance without label

## 19. Minimum acceptable implementation

An error/degraded-state implementation is minimally acceptable when it has:

1. Error category
2. Plain-language message
3. Source product/integration when relevant
4. Retryability
5. Blocking/degraded/stale state
6. Safe permission behavior
7. Section-level failure where possible
8. Correlation ID for support/debug
9. No raw internals to ordinary users
