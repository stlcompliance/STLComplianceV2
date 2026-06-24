# STL Compliance User Trust and Error Truthfulness Constitution

## 1. Audit drivers

CQ-005, FUNC-001, UX-001 through UX-005, and UI-004 found false success, browser-native dialogs, raw JSON/IDs, hard-coded references, and inconsistent failure preservation.

## 2. Prime directive

The UI must state what actually happened. It may never convert a failed, local, pending, stale, partial, or simulated operation into apparent completion.

## 3. Write truth

Success is shown only after durable server confirmation. Optimistic UI is allowed only when rollback is reliable and the pending state is visible. Recoverable failures preserve entered data.

## 4. Error taxonomy

Every write surface distinguishes validation, authentication, permission, not found, conflict, dependency unavailable, rate/limit, offline/pending sync, and unexpected error. Plain-language guidance is primary; correlation and technical details are secondary.

## 5. Dialogs

Use shared accessible dialogs, not `window.alert`, `window.confirm`, or `window.prompt`. Destructive actions show consequence, structured reason, permission context, loading, failure, and return-focus behavior.

## 6. Technical data

Raw JSON, payloads, stack traces, GUIDs, role keys, and internal enum names are not ordinary-user content. Advanced technical details are explicit, permissioned, and visually secondary.

## 7. Dependency truth

Owner-product data that is unavailable, stale, or snapshotted is labeled. Hard-coded options may not masquerade as live owner data.

## 8. Page states

Every page and major section has loading, empty, no-results, forbidden, not-found, conflict, stale, degraded, partial, and unexpected-error behavior appropriate to the risk.
