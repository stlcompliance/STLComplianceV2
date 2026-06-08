# Browser E2E (Playwright)

Optional smoke tests for **suite-frontend**: NexArr login → unified dashboard → product launch surfaces → handoff redirect to each Arr product frontend (5175–5180 and 5182).

## Quick start (host Vite previews — recommended for CI)

```powershell
# APIs
./scripts/ops/e2e-stack-up.ps1

# Suite + seven product frontends (5175–5180 and 5182) and Field Companion (5181)
./scripts/ops/e2e-frontends-preview.ps1

cd tests/e2e-playwright
npm install
npx playwright install chromium
$env:E2E_LIVE = "1"
npm test
```

## Full docker-compose e2e profile

Builds and serves all frontends in containers (slower, self-contained):

```powershell
./scripts/ops/e2e-stack-up.ps1 -BuildFrontends
$env:E2E_LIVE = "1"
cd tests/e2e-playwright; npm test
```

```bash
./scripts/ops/e2e-stack-up.sh --build-frontends
export E2E_LIVE=1
cd tests/e2e-playwright && npm test
```

Compose files: `docker-compose.yml` + `docker-compose.e2e.yml` with profile `e2e`.

## Specs

| Test file | Coverage |
|-----------|----------|
| `suite-login-handoff-smoke.spec.ts` | Login, StaffArr launch surface, StaffArr handoff redirect |
| `product-handoff-smoke.spec.ts` | Handoff redirect for all seven product frontends |
| `FieldCompanion-field-inbox-trainarr-deep-link.spec.ts` | Field Companion field inbox → TrainArr assignment deep link (W133) |
| `product-trainarr-assignment-deep-link.spec.ts` | TrainArr `/assignments/{id}/evidence` route smoke |
| `platform-admin-audit-export-smoke.spec.ts` | Suite platform-admin audit export manifest/timeline/sync ZIP + background job (W138) |
| `platform-admin-worker-health-orchestration-smoke.spec.ts` | Suite platform-admin orchestration panel: product health, token inventory, lifecycle worker sections + trigger controls visible (W260/W262; no live triggers) |
| `maintainarr-settings-audit-export-smoke.spec.ts` | MaintainArr Settings audit export panel (W230): manifest, filters, ZIP/JSON, background job (W232) |
| `maintainarr-settings-admin-workspace-smoke.spec.ts` | MaintainArr handoff → `/settings` `maintainarr-settings-admin-workspace`: all five product-admin panels visible with save controls; notification dispatches + worker run/history sections loaded (W321; no save mutations) |
| `maintainarr-reports-workspace-smoke.spec.ts` | MaintainArr handoff → `/reports` `maintainarr-reports-workspace`: compliance, executive, maintenance report panels + data exports with filters, summary/empty states, export controls (W203–W207/W322; audit export separate W230; no CSV download clicks) |
| `maintainarr-asset-readiness-detail-smoke.spec.ts` | MaintainArr handoff → `/assets` `maintainarr-assets-workspace`: asset registry selectable rows + readiness detail panel with blockers/signals after asset select (Worker 12; no create mutations) |
| `compliancecore-m12-worker-settings-smoke.spec.ts` | Compliance Core Admin M12 analytics worker settings save (W231/W232) |
| `compliancecore-audit-delivery-orchestration-smoke.spec.ts` | Compliance Core Admin audit delivery orchestration panel: status sections + trigger controls visible (W240/W242; no live triggers) |
| `trainarr-assignment-material-demand-smoke.spec.ts` | TrainArr handoff → assignment workspace material demand panel: lines, optional publish, procurement badge/timeline (W233/W234) |
| `trainarr-routarr-qualification-issue-publication-journey-smoke.spec.ts` | TrainArr handoff → `/assignments/{id}` completion: evaluation + signoffs issue a qualification grant publication, then RoutArr `/dispatch` assign confirms driver eligibility stays unlocked (W330; browser-completed publication chain) |
| `routarr-dispatch-command-center-smoke.spec.ts` | RoutArr handoff → `/dispatch` command center panel: daily/weekly scope toggle, status columns or empty state (W209/W235) |
| `routarr-dispatch-exception-queue-smoke.spec.ts` | RoutArr handoff → `/dispatch` exception queue panel: heading, create form when triage allowed, open rows or empty state (W210/W243) |
| `routarr-dispatch-exception-triage-depth-smoke.spec.ts` | RoutArr handoff → `/dispatch` exception queue triage depth: bulk actions panel, resolution template picker, row selection enables bulk controls, overdue filter + SLA badge when fixture seeded (W254/W256; no bulk assign/resolve clicks) |
| `routarr-dispatch-active-trips-smoke.spec.ts` | RoutArr handoff → `/dispatch` active trips panel: list/map toggle, summary tiles, trip rows or empty state (W211/W244) |
| `routarr-dispatch-unassigned-work-queue-smoke.spec.ts` | RoutArr handoff → `/dispatch` unassigned work queue: heading, per-trip assign controls, bulk assign when rows present, or empty state (W212/W245) |
| `routarr-dispatch-unassigned-queue-preview-depth-smoke.spec.ts` | RoutArr handoff → `/dispatch` unassigned queue preview depth: urgent summary, attention filter, bulk row selection + driver pick, preview-before-assign cancel path (W255/W258; no assign/bulk apply clicks) |
| `routarr-driver-portal-smoke.spec.ts` | RoutArr handoff → `/driver-portal` schedule panel: Today/Upcoming sections, trip cards or empty state (W213/W247; no dispatch/start clicks) |
| `routarr-driver-portal-proof-dvir-capture-depth-smoke.spec.ts` | RoutArr handoff → `/driver-portal` capture depth: readiness blockers, disabled Start trip, pre-trip DVIR form, fail-without-notes validation alert (W257/W259; no pass DVIR/proof/start mutations) |
| `routarr-settings-trip-execution-smoke.spec.ts` | RoutArr handoff → `/settings` `trip-execution-settings-panel`: core + attachment policy toggles, save + reload persistence, restore original toggle (W257/W261/W263) |
| `routarr-settings-admin-workspace-smoke.spec.ts` | RoutArr handoff → `/settings` `routarr-settings-admin-workspace`: all four product-admin panels visible with save controls; recent dispatches + worker/retention runs sections loaded (W316; no save mutations) |
| `routarr-reports-workspace-smoke.spec.ts` | RoutArr handoff → `/reports` `routarr-reports-workspace`: dispatch, route, proof-DVIR report panels (scope filter, summary/empty state, Export CSV); data exports manifest + Download CSV controls (W317; audit export covered by W241) |
| `routarr-settings-attachment-retention-smoke.spec.ts` | RoutArr handoff → `/settings` `attachment-retention-settings-panel`: enable toggle + retention days save/reload, recent runs empty/list section visible, restore original settings (W276/W277; no live purge batch) |
| `routarr-settings-trip-completion-rollup-smoke.spec.ts` | RoutArr handoff → `/settings` `trip-completion-rollup-settings-panel`: enable toggle + staleness hours save/reload, recent worker runs empty/list section visible, restore original settings (W176/W263/W278; no live rollup batch) |
| `routarr-settings-notification-hooks-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: enable toggle + webhook URL + trip-assigned event toggle save/reload, recent dispatches empty/list section visible, restore original settings (W127/W263/W279; no live dispatch batch) |
| `routarr-notification-dispatch-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture trip status change to dispatched enqueues pending outbox row in Recent dispatches, optional internal process-batch moves row off pending (W127/W279/W280; read-only webhook to example.com) |
| `routarr-notification-dispatch-trip-assigned-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture assign-driver only enqueues pending `trip_assigned` outbox row in Recent dispatches, optional internal process-batch moves row off pending (W127/W280/W281; read-only webhook to example.com) |
| `routarr-notification-dispatch-trip-in-progress-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture status change to in_progress only enqueues pending `trip_in_progress` outbox row in Recent dispatches, optional internal process-batch moves row off pending (W127/W281/W282; read-only webhook to example.com) |
| `routarr-notification-dispatch-trip-completed-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture status change to completed only enqueues pending `trip_completed` outbox row in Recent dispatches, optional internal process-batch moves row off pending (W127/W282/W283; read-only webhook to example.com) |
| `routarr-notification-dispatch-trip-cancelled-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture status change to cancelled only enqueues pending `trip_cancelled` outbox row in Recent dispatches, optional internal process-batch moves row off pending (W127/W283/W284; read-only webhook to example.com) |
| `routarr-notification-dispatch-multi-event-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: API fixture completed path (assign → dispatched → in_progress → completed with selective toggles) plus cancelled branch (cancelled-only toggle) verifies only enabled event kinds enqueue pending rows, optional internal process-batch (W127/W284/W285; read-only webhook to example.com) |
| `routarr-settings-notification-dispatch-negative-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: live UI save with only trip-dispatched enabled (all other event toggles off), reload persistence, API lifecycle on new trip, Recent dispatches shows `trip_dispatched` only (no assigned/in_progress/completed/cancelled rows), restore original settings (W127/W279/W285/W286; read-only webhook to example.com) |
| `routarr-settings-notification-dispatch-selective-disable-second-lifecycle-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: live UI save with all event toggles on, first API lifecycle enqueues assigned/dispatched/in_progress/completed, post-save selective disable to trip-dispatched only, second API lifecycle enqueues `trip_dispatched` only for second trip, Recent dispatches verifies both trips, restore original settings (W127/W279/W285/W286/W287; read-only webhook to example.com) |
| `routarr-notification-dispatch-all-events-process-batch-journey-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: live UI save with all event toggles on, completed-path API lifecycle + cancelled-branch API lifecycle enqueue all five event kinds as pending in Recent dispatches, internal process-batch moves every row off pending (sent/failed/skipped), restore original settings (W127/W279/W285/W287/W288; read-only webhook to example.com) |
| `routarr-settings-notification-webhook-persistence-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: two-step webhook URL change (W289 primary → alternate) with save + reload UI persistence and API GET verification after each reload, restore original webhook (W127/W279/W288; settings-only, no trip mutations) |
| `routarr-settings-notification-webhook-validation-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: enable + empty webhook shows required error (reload unchanged), invalid URL shows absolute URL error (reload unchanged), valid URL saves, API PUT rejection helpers, restore original settings (W127/W279/W289/W290; settings-only) |
| `routarr-settings-notification-disable-clears-validation-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: enable + empty webhook shows required error, unchecking enable clears error without save, reload unchanged, disable-then-save persists disabled without invalid enabled+empty state, API match + disabled upsert helpers, restore original settings (W127/W289/W290/W291; settings-only) |
| `routarr-settings-notification-re-enable-preserve-webhook-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: seed saved webhook via API, clear field + validation error, disable clears error, re-enable restores last saved webhook from API in UI without persisting cleared field, restore original settings (W127/W289/W290/W291/W292; settings-only) |
| `routarr-settings-notification-disable-save-preserve-webhook-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: seed saved webhook via API, disable + save keeps webhook in API while disabled, reload and re-enable show preserved URL, restore original settings (W127/W289/W290/W291/W292/W293; settings-only) |
| `routarr-settings-notification-disable-explicit-clear-webhook-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: seed saved webhook via API, disable + check clear-on-disable + save removes webhook from API/UI, reload and re-enable show empty field, restore original settings (W127/W293/W298; settings-only) |
| `routarr-settings-notification-disable-preserve-vs-explicit-clear-contrast-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: one session — phase 1 disable without clear preserves webhook (W293), phase 2 re-enable then disable with clear-on-disable removes webhook (W298), API GET assertions after each save, restore original settings (W127/W293/W298/W299; settings-only) |
| `routarr-settings-notification-re-enable-after-explicit-clear-empty-webhook-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: seed explicit-clear disabled state (null webhook in API), reload, re-enable shows empty field (not prior URL), save without URL shows required error, new URL saves and persists, restore original settings (W127/W298/W299/W300; settings-only) |
| `routarr-settings-notification-re-enable-new-webhook-reload-persistence-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: after explicit clear + new URL save, second reload + toggle off/on keeps W305 new webhook (not pre-clear original) in UI + API GET, restore original settings (W127/W298/W300/W305; settings-only) |
| `routarr-settings-notification-disable-save-re-enable-reload-persistence-smoke.spec.ts` | RoutArr handoff → `/settings` `notification-settings-panel`: disable+save preserves webhook in API, reload while disabled, re-enable+save, second reload + toggle off/on keeps W307 preserved webhook in UI + API GET, restore original settings (W127/W293/W305/W307; settings-only) |
| `routarr-driver-portal-attachment-upload-smoke.spec.ts` | RoutArr handoff → `/driver-portal` attachment panels: pickup photo file upload satisfies start gate, start trip, delivery signature pad upload (W261/W264) |
| `routarr-driver-portal-document-attachment-upload-smoke.spec.ts` | RoutArr handoff → `/driver-portal` attachment panels: pickup BOL document file upload via Document control, `document:` row visible, Start trip enabled (W261/W266; no photo/signature path) |
| `routarr-dispatch-proof-dvir-read-smoke.spec.ts` | RoutArr handoff → `/dispatch` `trip-proof-dvir-read-panel`: trip ID lookup, load execution summary, proof/DVIR rows or empty lists (W217/W248; read-only, no proof/DVIR capture) |
| `routarr-dispatch-proof-dvir-read-attachment-download-smoke.spec.ts` | RoutArr handoff → `/dispatch` `trip-proof-dvir-read-panel`: fixture trip with proof photo attachment, load execution, click `proof-attachment-*` download link (W261/W265; read-only dispatcher path) |
| `routarr-dispatch-proof-dvir-read-dvir-attachment-download-smoke.spec.ts` | RoutArr handoff → `/dispatch` `trip-proof-dvir-read-panel`: fixture trip with pre-trip DVIR photo attachment, load execution, click `dvir-attachment-*` download link (W261/W267; read-only dispatcher path) |
| `routarr-dispatch-proof-dvir-read-document-attachment-download-smoke.spec.ts` | RoutArr handoff → `/dispatch` `trip-proof-dvir-read-panel`: fixture trip with pickup proof document (PDF) attachment, load execution, click `proof-attachment-*` download link with `document:` label (W261/W266/W268; read-only dispatcher path) |
| `routarr-document-attachment-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` pickup BOL document upload via Document control, then `/dispatch` `trip-proof-dvir-read-panel` load execution + `proof-attachment-*` document download in one session (W266/W268/W269; end-to-end driver upload → dispatcher read) |
| `routarr-photo-attachment-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` pickup photo upload via image file input, then `/dispatch` `trip-proof-dvir-read-panel` load execution + `proof-attachment-*` photo download in one session (W264/W265/W270; end-to-end driver upload → dispatcher read) |
| `routarr-signature-attachment-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` start trip + delivery signature pad save, then `/dispatch` `trip-proof-dvir-read-panel` load execution + `proof-attachment-*` signature download in one session (W264/W270/W271; end-to-end driver upload → dispatcher read) |
| `routarr-dvir-photo-attachment-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` pre-trip DVIR submit + photo upload, then `/dispatch` `trip-proof-dvir-read-panel` load execution + `dvir-attachment-*` photo download in one session (W267/W270/W272; end-to-end driver upload → dispatcher read) |
| `routarr-post-trip-dvir-photo-attachment-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` start trip + post-trip DVIR submit + photo upload, then `/dispatch` `trip-proof-dvir-read-panel` load execution + `dvir-attachment-*` photo download in one session (W267/W272/W273; end-to-end driver upload → dispatcher read) |
| `routarr-trip-complete-after-capture-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` start trip + post-trip DVIR submit + photo upload + **Complete** (photo gate), then `/dispatch` `trip-proof-dvir-read-panel` post DVIR yes + `dvir-attachment-*` download in one session (W273/W274; end-to-end capture → complete → dispatcher read) |
| `routarr-trip-close-after-complete-journey-smoke.spec.ts` | RoutArr handoff → `/driver-portal` start trip + post-trip DVIR submit + photo upload + **Complete** + **Close**, then `/dispatch` `trip-proof-dvir-read-panel` status completed + driver closed yes + post DVIR + `dvir-attachment-*` download in one session (W274/W275; end-to-end capture → complete → close → dispatcher read) |
| `routarr-dispatch-closeout-panel-smoke.spec.ts` | RoutArr handoff → `/dispatch` `dispatch-closeout-panel`: disposition selects, trip checklist when open trips exist, preview closeout only (W251/W253; no apply closeout) |
| `loadarr-workspace-smoke.spec.ts` | LoadArr handoff → workspace shell: grouped work surface, warehouse execution header, metrics, receiving workflow, expected receipts, transfers, backorders, vendor returns, holds, cycle counts, adjustments, exceptions, permissions, and route/product handoffs sections plus seeded detail routes visible |
| `supplyarr-settings-integration-events-smoke.spec.ts` | SupplyArr handoff → Settings integration event outbox/inbox save + Readiness dashboard metrics (W236) |
| `supplyarr-settings-admin-workspace-smoke.spec.ts` | SupplyArr handoff → `/settings` `supplyarr-settings-admin-workspace`: all nine product-admin panels visible with save controls; notification dispatches + escalation runs + integration outbox/inbox sections loaded (W318; no save mutations) |
| `supplyarr-reports-workspace-smoke.spec.ts` | SupplyArr handoff → `/reports` `supplyarr-reports-workspace`: all five M12 report panels (vendor, parts/inventory, purchasing, compliance, audit history) with filters, summary/empty states, export controls (W237/W319; no CSV download clicks) |
| `supplyarr-pricing-snapshots-smoke.spec.ts` | SupplyArr handoff → sidebar **Pricing** → `/pricing` `supplyarr-pricing-snapshots-workspace`: pricing/lead-time + availability history panels, current-only filter, record controls when permitted, direct URL reload (Worker 7; no snapshot create mutations) |
| `supplyarr-purchasing-po-workflow-smoke.spec.ts` | SupplyArr handoff → sidebar **Purchasing** → `/purchasing` `supplyarr-purchasing-po-workspace`: PO list/detail with line quantities, cancel reason input when draft PO selected, create-from-PR form, direct URL reload (Worker 8; no cancel mutation) |
| `supplyarr-purchasing-pr-workflow-smoke.spec.ts` | SupplyArr handoff → sidebar **Purchasing** → `/purchasing` `supplyarr-purchasing-pr-workspace`: PR list/detail with line quantities, submit/approve/reject controls, rejection reason input when submitted PR selected, create draft form, direct URL reload (Worker 9; no workflow mutations) |
| `supplyarr-purchasing-demand-processing-smoke.spec.ts` | SupplyArr handoff → `/purchasing` demand processing panel: journey seed fixture, pending queue row, operator controls, view-status line availability (W246/W294; no retry/PR clicks) |
| `supplyarr-purchasing-procurement-exceptions-smoke.spec.ts` | SupplyArr handoff → `/purchasing` procurement exceptions panel: fixture PR subject, SLA breached badge, resolution template picker, investigate + assign controls visible (W250/W295/W256; no investigate/resolve/assign clicks) |
| `supplyarr-settings-procurement-exception-escalation-smoke.spec.ts` | SupplyArr handoff → `/settings` `procurement-exception-escalation-settings-panel`: enable + cooldown/max/notify save/reload, API pending preview when enabled with overdue fixture, recent runs/events empty/list sections, restore original settings (W296/W297/W279; no live escalation batch) |
| `supplyarr-settings-procurement-exception-escalation-journey-smoke.spec.ts` | SupplyArr handoff → `/settings` `procurement-exception-escalation-settings-panel`: overdue fixture pending row, internal process-batch escalates exception, Recent escalation events + runs UI, pending `procurement_exception_sla_escalation` notification dispatch (W296/W297/W301; no notification process-batch) |
| `supplyarr-settings-procurement-notification-process-batch-journey-smoke.spec.ts` | SupplyArr handoff → `/settings` `notification-settings-panel`: escalation batch enqueues pending SLA escalation dispatch, internal notification process-batch moves row to sent/failed/skipped in Recent dispatches (W301/W129/W302) |
| `supplyarr-purchasing-procurement-exception-investigate-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Resolve** with PR resubmit template, status badges + API asserts (W250/W295/W303) |
| `supplyarr-purchasing-procurement-exception-waive-close-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Request waive** → **Approve waive** → **Close**, waive justification + status badges + API asserts (W250/W303/W304) |
| `supplyarr-purchasing-procurement-exception-reject-waive-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Request waive** → **Reject waive**, status returns to investigating + waive rejection reason API assert + resolve/request-waive controls visible (W250/W304/W306) |
| `supplyarr-purchasing-procurement-exception-cancel-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → fill cancel reason → **Cancel**, status cancelled + cancellationReason API assert + workflow controls hidden (W250/W303/W308) |
| `supplyarr-purchasing-procurement-exception-post-cancel-reopen-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Cancel** → fill reopen reason → **Reopen**, status investigating + lastReopenReason/reopenCount API asserts + resolve control visible (W250/W303/W311) |
| `supplyarr-purchasing-procurement-exception-post-reject-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Request waive** → **Reject waive** → **Resolve** with PR resubmit template, resolved status + template + waiveRejectionReason API asserts (W250/W303/W306/W309) |
| `supplyarr-purchasing-procurement-exception-assign-link-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open unassigned fixture → select detail → **Assign to me** → link follow-up PR → **Save PR/PO links**, assigned + linked PR API asserts (W250/W295/W310) |
| `supplyarr-purchasing-procurement-exception-investigate-link-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → link follow-up PR → **Save PR/PO links** → **Resolve** with PR resubmit template, resolved + linked PR + template API asserts (W250/W303/W310/W311) |
| `supplyarr-purchasing-procurement-exception-investigate-link-po-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → link issued follow-up PO → **Save PR/PO links** → **Resolve** with PO reissue template, resolved + linked PO + template API asserts (W250/W311/W312) |
| `supplyarr-purchasing-procurement-exception-close-after-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → **Resolve** with PR resubmit template → **Close**, resolved + closed + template + closedAt API asserts (W250/W303/W313) |
| `supplyarr-purchasing-procurement-exception-close-after-link-pr-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → link follow-up PR → **Save PR/PO links** → **Resolve** with PR resubmit template → **Close**, closed + linked PR + template + closedAt API asserts (W250/W311/W313/W314) |
| `supplyarr-purchasing-procurement-exception-close-after-link-po-resolve-journey-smoke.spec.ts` | SupplyArr handoff → `/purchasing` `procurement-exceptions-panel`: open fixture → **Investigate** → link issued follow-up PO → **Save PR/PO links** → **Resolve** with PO reissue template → **Close**, closed + linked PO + template + closedAt API asserts (W250/W312/W313/W315) |
| `staffarr-admin-audit-export-smoke.spec.ts` | StaffArr handoff → Admin audit package panel: manifest, summary, filters, sync ZIP/JSON/CSV, background job (W228/W238) |
| `trainarr-settings-audit-export-smoke.spec.ts` | TrainArr handoff → Settings training audit package panel: manifest, date filters, JSON summary counts, sync + background ZIP (W165/W167/W239) |
| `trainarr-settings-admin-workspace-smoke.spec.ts` | TrainArr handoff → `/settings` `trainarr-settings-admin-workspace`: all ten product-admin panels visible with save controls; notification dispatches + worker/history sections loaded (W320; no save mutations) |
| `trainarr-reports-workspace-smoke.spec.ts` | TrainArr handoff → `/reports` `trainarr-reports-workspace`: assignment, qualification, compliance report panels + data exports with filters, summary/empty states, export controls (W323; audit export separate W239; no CSV download clicks) |
| `staffarr-reports-workspace-smoke.spec.ts` | StaffArr handoff → `/reports` `staffarr-reports-workspace`: personnel, readiness, incident report panels + data exports with filters, summary/empty states, export controls (W324; audit export separate W238; no CSV download clicks) |
| `staffarr-workforce-onboarding-journey-smoke.spec.ts` | StaffArr handoff → `/people`: CreatePersonPanel + docs/23 `workforce-onboarding-journey-panel` with step list (GET `/api/people/{id}/workforce-onboarding-journey`; Worker 6) |
| `staffarr-reports-audit-export-smoke.spec.ts` | StaffArr handoff → `/reports` audit package export: manifest, summary, timeline, filters, background ZIP job (Worker 5) |
| `staffarr-settings-admin-workspace-smoke.spec.ts` | StaffArr handoff → `/admin` `staffarr-settings-admin-workspace`: all six product-admin panels visible with save controls; export delivery pending/runs/notifications + worker pending/run sections loaded (W325; audit export panel outside wrapper W238; no save mutations) |
| `routarr-reports-audit-export-smoke.spec.ts` | RoutArr handoff → Reports audit package panel: manifest, summary, filters, sync ZIP/JSON/CSV, background job (W227/W241) |
| `FieldCompanion-field-inbox-operations-deep-links.spec.ts` | Field Companion → MaintainArr / RoutArr / SupplyArr field inbox deep links (W140) |
| `FieldCompanion-offline-queue-notification.spec.ts` | Field Companion offline acknowledge queue sync + notification/push readiness surfaces (W146) |
| `FieldCompanion-field-task-evidence.spec.ts` | TrainArr field-inbox photo evidence upload via Field Companion API (W147) |
| `FieldCompanion-field-scan.spec.ts` | Field Companion manual scan resolve → field inbox task highlight (M11) |
| `compliancecore-operator-rule-evaluate-smoke.spec.ts` | Compliance Core handoff → rule pack seed + evaluate (operator path) |
| `compliancecore-operator-workflow-gate-journey-smoke.spec.ts` | Compliance Core journey seed → `/findings` dispatch gate check (allow) + `/operator` dashboard summary sections (W326) |
| `compliancecore-operator-batch-evaluate-findings-emit-journey-smoke.spec.ts` | Compliance Core journey seed → `/evaluation` batch rule evaluate (allow) + `/findings` gate check with emit-on-block finding (W327) |
| `compliancecore-operator-batch-workflow-gate-check-journey-smoke.spec.ts` | Compliance Core journey seed → `/findings` batch workflow gate check: multi-gate selection, shared facts, batch summary allow + emit-on-block findings (W328) |
| `compliancecore-routarr-dispatch-gate-assign-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate check → suite → RoutArr `/dispatch` unassigned assign blocked (cancel), block override assign, and allow path without gate dialog (W331) |
| `trainarr-routarr-qualification-gate-assign-journey-smoke.spec.ts` | Cross-product journey: TrainArr `/qualifications` authorization check (block/allow) → RoutArr `/dispatch` unassigned assign gated by TrainArr driver eligibility via `POST /api/integrations/routarr-qualification-check` (block cancel, override assign, allow without eligibility dialog) |
| `compliancecore-routarr-dispatch-gate-command-center-dnd-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate check → suite → RoutArr `/dispatch` command-center drag-and-drop assign block/cancel, block override, allow without dialog, and warn cancel/confirm paths (W332; builds on W331/W78/W252) |
| `compliancecore-routarr-dispatch-gate-bulk-dispatch-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate check → suite → RoutArr `/dispatch` bulk dispatch panel preview workflow gate block, apply cancel/override with `ignoreWorkflowGateBlocks`, and warn cancel/confirm paths (W334; builds on W331/W332/W333) |
| `compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate check → suite → RoutArr `/dispatch` unassigned work queue bulk assign workflow gate block cancel/override with `ignoreWorkflowGateBlocks`, and warn cancel/confirm paths (W335; builds on W331/W332/W333/W334) |
| `compliancecore-routarr-dispatch-gate-trip-assigned-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → `/settings` trip_assigned notification pending row + optional internal process-batch verify (W336; builds on W331/W281/W127) |
| `compliancecore-routarr-dispatch-gate-command-center-trip-assigned-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` command-center drag-and-drop assign override → `/settings` trip_assigned notification pending row + optional internal process-batch verify (W337; builds on W332/W336/W281/W127) |
| `compliancecore-routarr-dispatch-gate-bulk-dispatch-trip-assigned-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` bulk dispatch panel override assign → `/settings` trip_assigned notification pending row + optional internal process-batch verify (W338; builds on W334/W336/W281/W127) |
| `compliancecore-routarr-dispatch-gate-unassigned-bulk-assign-trip-assigned-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned work queue bulk assign override → `/settings` trip_assigned notification pending row + optional internal process-batch verify (W339; builds on W335/W336/W338/W281/W127) |
| `compliancecore-routarr-dispatch-gate-trip-dispatched-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → command-center Dispatch → `/settings` trip_dispatched notification pending row + optional internal process-batch verify (W341; builds on W336/W280/W127) |
| `compliancecore-routarr-dispatch-gate-trip-in-progress-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → command-center Dispatch → bulk dispatch in_progress status change → `/settings` trip_in_progress notification pending row + optional internal process-batch verify (W342; builds on W341/W282/W127) |
| `compliancecore-routarr-dispatch-gate-trip-completed-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → command-center Dispatch → bulk dispatch in_progress then completed status changes → `/settings` trip_completed notification pending row + optional internal process-batch verify (W343; builds on W342/W283/W127) |
| `compliancecore-routarr-dispatch-gate-trip-cancelled-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → command-center Dispatch → bulk dispatch cancelled status change → `/settings` trip_cancelled notification pending row + optional internal process-batch verify (W344; builds on W343/W284/W127) |
| `compliancecore-routarr-dispatch-gate-multi-event-notification-journey-smoke.spec.ts` | Cross-product journey: Compliance Core `/findings` dispatch gate block check → suite → RoutArr `/dispatch` unassigned assign override → command-center Dispatch → bulk in_progress → bulk completed → `/settings` Recent dispatches shows pending rows for trip_assigned, trip_dispatched, trip_in_progress, and trip_completed (no trip_cancelled) + optional internal process-batch verify (W351; builds on W344/W285/W343) |
| `suite-multi-product-handoff-journey.spec.ts` | Suite session chains StaffArr → TrainArr → Compliance Core handoffs |

Catalog: `StlE2ePlaywrightSpecCatalog` + `StlE2eFrontendCatalog.FieldCompanionFrontend` in shared .NET (`Category=E2e` tests).

## Skip behavior

- `E2E_LIVE` not `1`/`true` → tests **skipped** (CI-safe)
- Suite (`5174`) or NexArr (`5101`) unreachable → **skipped**
- Individual product frontend unreachable → that product test **skipped**
- Default `npm test` without live stack: all tests skipped, exit 0

## Environment

| Variable | Default |
|----------|---------|
| `E2E_LIVE` | unset — tests skipped |
| `E2E_SUITE_URL` | `http://localhost:5174` |
| `E2E_NEXARR_URL` | `http://localhost:5101` |
| `E2E_STAFFARR_URL` | `http://localhost:5175` (frontend preview) |
| `E2E_TRAINARR_URL` | `http://localhost:5176` |
| `E2E_COMPLIANCECORE_URL` | `http://localhost:5177` |
| `E2E_MAINTAINARR_URL` | `http://localhost:5178` |
| `E2E_SUPPLYARR_URL` | `http://localhost:5179` |
| `E2E_ROUTARR_URL` | `http://localhost:5180` |
| `E2E_LOADARR_URL` | `http://localhost:5182` |
| `E2E_FIELDCOMPANION_URL` | `http://localhost:5181` |
| `E2E_LOADARR_API_URL` | `http://localhost:5108` |
| `E2E_TRAINARR_API_URL` | `http://localhost:5103` |
| `E2E_STAFFARR_API_URL` | `http://localhost:5102` |
| `E2E_COMPLIANCECORE_API_URL` | `http://localhost:5107` |
| `E2E_DEMO_EMAIL` | `admin@demo.stl` |
| `E2E_DEMO_PASSWORD` | `ChangeMe!Demo2026` |
