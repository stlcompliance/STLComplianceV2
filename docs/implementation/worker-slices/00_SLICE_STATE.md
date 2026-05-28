# Worker slice completion state



| Worker | Slice | Milestone | Status | Commit |

|--------|-------|-----------|--------|--------|

| 1 | Platform foundation (APIs, health, EF baseline, Docker, workers) | M1 | Complete | `38d9f3ef73e8d5e8564d6b92c3863270ce7d370e` |

| 2 | NexArr identity auth spine (login, sessions, /api/me*) | M2 (partial) | Complete | `7ab1a6a` |

| 3 | NexArr tenant/entitlement admin + service tokens | M2 (partial) | Complete | `6aa10c9` |

| 4 | NexArr launch context, handoff codes, callback allowlist | M2 (partial) | Complete | `db3a82f` |

| 5 | Suite frontend AppShell (auth, navigation, launch) | M3 (partial) | Complete | `87c2218` |

| 6 | NexArr platform-admin APIs + suite platform-admin UI | M2/M3 (partial) | Complete | `5c4934b` |

| 7 | Suite unified dashboard (M3 widgets on `/app`) | M3 (partial) | Complete | `5c293e8` |

| 8 | StaffArr shell + NexArr handoff redeem | M3 (partial) | Complete | `see latest Worker 8 commit` |

| 9 | StaffArr people directory + person profile core | M4 (partial) | Complete | `pending` |

| 10 | StaffArr org hierarchy management write flows | M4 (partial) | Complete | `pending` |

| 11 | StaffArr org-unit assignment primitives (site/department/team/position linkage + assignment write flows) | M4 (partial) | Complete | `pending` |

| 12 | StaffArr manager hierarchy + manager/subordinate views (hierarchy traversal, manager linkage write path, subordinate detail rollups) | M4 (partial) | Complete | `pending` |

| 13 | StaffArr role templates + permission templates/assignment foundations (tenant-scoped template modeling, role assignment write/read paths, permission-aware UI integration) | M4 (partial) | Complete | `pending` |

| 14 | StaffArr scoped effective-permission projection + permission history timeline (computed permission read API, timeline retrieval API, real UI projection/timeline surfaces) | M4 (partial) | Complete | `pending` |

| 15 | StaffArr certification visibility + manual certification grant foundations (definition catalog, person certification read/grant/update APIs, readiness baseline seed, real UI integration) | M4 (partial) | Complete | `pending` |

| 16 | StaffArr readiness calculation foundations (person readiness APIs, plain-English blockers, real UI readiness summary) | M4 (partial) | Complete | `pending` |

| 17 | StaffArr manual readiness override foundations (`staffarr.readiness.override`, override persistence, override-aware calculation, authorized UI) | M4 (partial) | Complete | `pending` |

| 18 | StaffArr incident intake foundations (`staffarr.incidents.manage`, incident records, intake/list/detail API, basic UI) | M4 (partial) | Complete | `pending` |

| 19 | StaffArr training blocker display + TrainArr publication to StaffArr (`staffarr_person_training_blockers`, integration ingest, `/api/certification-publications`, readiness UI) | M4 (partial) | Complete | `pending` |

| 20 | StaffArr incident routing to TrainArr (`training_compliance` forward, remediation intake API, routing mirror, IncidentsPanel route UI, cross-product tests) | M4 (partial) | Complete | `pending` |

| 21 | StaffArr person timeline foundations (aggregated timeline API from existing tables, paginated `GET /api/people/{personId}/timeline`, person-read auth, PersonTimelinePanel, tests) | M4 (partial) | Complete | `pending` |

| 22 | TrainArr training assignment engine (definitions + assignments DB/API, remediation linkage, blocker publish/clear, user auth, trainarr-frontend, cross-product tests) | M6 (partial) | Complete | `pending` |

| 23 | Compliance Core controlled vocabulary spine (14 vocabulary type keys, terms/aliases, compliance/material keys, read/manage APIs, auth, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |

| 24 | Compliance Core regulatory registries + rule pack foundations (governing body, jurisdiction, regulatory program, rule packs with version/status, read/manage APIs, auth, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |

| 25 | Compliance Core citation registry + fact catalog foundations (regulatory citations with versioning/supersession, fact definitions/requirements, read/manage APIs, auth, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |

| 26 | Compliance Core regulatory mappings (compliance/material keys linked to programs/rule packs/citations/facts, read/manage APIs, auth, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |

| 27 | TrainArr program builder / evidence capture (programs linked to definitions, assignment evidence with storage, program CRUD, evidence APIs, trainarr-frontend panels, completion/blocker integration, tests) | M6 (partial) | Complete | `pending` |

| 28 | TrainArr signoffs / evaluations (evaluation + signoff records per assignment, submit/list APIs, JWT trainer/trainee/admin scopes, completion gate before StaffArr blocker clear, trainarr-frontend panel, tests) | M6 (partial) | Complete | `pending` |
| 29 | TrainArr qualification issue + StaffArr certification grant (`trainarr_qualification_issues`, qualification grant publication, StaffArr certification ingest, completion flow, frontends, cross-product tests) | M6 (partial) | Complete | `pending` |
| 30 | Compliance Core rule version content + evaluation foundations (rule content JSON on rule packs, evaluation runs, evaluate/list APIs, JWT auth, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |
| 31 | TrainArr qualification suspend/revoke/expire (lifecycle actions, StaffArr certification lifecycle ingest, training blocker on suspend, trainarr/staffarr frontends, cross-product tests) | M6 (partial) | Complete | `pending` |
| 32 | Compliance Core fact source registry + internal resolve API (fact sources DB/API, `/api/internal/resolve` + `/api/internal/validate`, service token scopes, static_config resolve, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |
| 33 | TrainArr qualification authorization check API (`POST /api/qualification-checks`, Compliance Core `/api/internal/evaluate`, allow/warn/block merge, trainarr-frontend check UI, cross-product tests) | M6 (partial) | Complete | `pending` |
| 34 | Compliance Core findings + workflow gate API (findings from evaluations, workflow gate check allow/warn/block, internal gate check service token, compliancecore-frontend, tests) | M5 (partial) | Complete | `pending` |
| 35 | Compliance Core 9-CSV import/export (nine-file bundle export ZIP, validate/upsert import with audit, SDS references table, compliancecore-frontend CSV tab, tests) | M5 (partial) | Complete | `pending` |
| 36 | TrainArr batch qualification checks (`POST /api/qualification-checks/batch`, parallel Compliance Core evaluate, batch audit, trainarr-frontend batch panel, cross-product tests) | M10 (partial) | Complete | `pending` |
| 37 | Compliance Core audit package export (`GET /api/audit-packages/export` ZIP/JSON, date filters, audit events/findings/evaluations/rule packs, compliancecore-frontend panel, tests) | M5/M12 (partial) | Complete | `pending` |
| 38 | TrainArr citation attachment (`trainarr_training_citation_attachments`, attach/list/remove on definitions/programs/assignments, Compliance Core internal citation lookup, JWT auth, trainarr-frontend panels, tests) | M10 (partial) | Complete | `pending` |
| 39 | Compliance Core batch workflow gate checks (`POST /api/workflow-gates/check/batch`, internal batch gate check, real per-gate evaluation, JWT + service token, compliancecore-frontend batch panel, tests) | M5/M10 (partial) | Complete | `pending` |

| 40 | TrainArr rule-pack requirement intake (`trainarr_training_rule_pack_requirements`, CRUD on definitions/programs, Compliance Core internal rule pack lookup, qualification-check auto-resolve, trainarr-frontend panels, cross-product tests) | M6/M10 (partial) | Complete | `pending` |
| 41 | Compliance Core cross-product batch evaluate API (`POST /api/internal/evaluate/batch`, per-item real evaluation + summary, TrainArr batch qualification uses batch endpoint, service token tests, docs) | M5/M10 (partial) | Complete | `pending` |
| 42 | TrainArr rule change impact (`RulePackImpactService`, GET/POST `/api/rule-pack-impact`, requirement baseline version capture, trainarr-frontend impact panel, audit, cross-product tests) | M6/M10 (partial) | Complete | `pending` |
| 43 | Compliance Core admin batch evaluate UI (`POST /api/rule-packs/evaluate/batch`, `RulePackBatchEvaluationService`, `BatchRuleEvaluationPanel`, JWT integration + frontend tests, docs) | M5/M10 (partial) | Complete | `pending` |
| 44 | TrainArr expiration scanning worker (`shared-worker` scheduled scan, TrainArr internal process-expirations API, `ExpiresAt` on qualification issues, service token `trainarr.qualifications.expire`, unit + cross-product tests, docs) | M12 (partial) | Complete | `pending` |
| 45 | Compliance Core operator dashboards (`GET /api/dashboards/operator`, real DB aggregates, JWT compliance roles, compliancecore-frontend Dashboard tab, integration + frontend tests, docs) | M5/M12 (partial) | Complete | `pending` |
| 46 | StaffArr certification expiration worker (`shared-worker` scheduled scan, StaffArr internal process-expirations API, `staffarr.certifications.expire`, index on person certifications expiry, unit + cross-product tests, docs) | M12 (partial) | Complete | `pending` |
| 47 | Compliance Core scheduled evaluation worker (`shared-worker` periodic scan, internal pending/process-batch APIs, `compliancecore.rules.evaluate.scheduled`, scheduled run audit table, `LastScheduledEvaluationAt`, real RuleEvaluator, unit + integration tests, docs) | M12 (partial) | Complete | `pending` |
| 48 | StaffArr readiness rollup worker (`shared-worker` scheduled refresh, `staffarr_readiness_rollups`, internal pending/process-batch APIs, `staffarr.readiness.rollup`, public rollup read APIs, supervisor frontend panel, unit + integration tests, docs) | M12 (partial) | Complete | `pending` |

| 49 | StaffArr permission projection worker (`shared-worker` scheduled refresh, `staffarr_person_permission_projections`, internal pending/process-batch APIs, `staffarr.permissions.project`, materialized-first effective permission read, unit + integration tests, docs) | M12 (partial) | Complete | `pending` |
| 50 | MaintainArr asset registry foundations (`maintainarr_asset_classes`, `maintainarr_asset_types`, `maintainarr_assets`, CRUD APIs, auth, maintainarr-frontend shell, unit + integration tests, docs) | M7 (partial) | Complete | `pending` |
| 51 | MaintainArr PM due scan worker (`maintainarr_pm_schedules`, PM CRUD + `/due` APIs, internal pending/process-due-scan, `maintainarr.pm.scan`, `shared-worker` `MaintainArrPmDueScanJob`, maintainarr-frontend due panel, unit + integration tests, docs) | M7/M12 (partial) | Complete | `pending` |
| 52 | MaintainArr inspection template builder (`maintainarr_inspection_templates`, categories, checklist items, asset-type links, CRUD `/api/inspection-templates`, JWT auth, maintainarr-frontend builder UI, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |
| 53 | MaintainArr inspection runner foundations (`maintainarr_inspection_runs`, run answers, `/api/inspections` start/answers/complete/list, JWT execute + manager view-all, maintainarr-frontend runner UI, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |
| 54 | MaintainArr defect capture (`maintainarr_defects`, auto-create on failed inspection complete, `/api/defects` CRUD/status, `/api/inspections/{id}/defects`, JWT auth, maintainarr-frontend defects panel, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |
| 55 | MaintainArr meter tracking (`maintainarr_asset_meters`, `maintainarr_meter_readings`, meter-based PM schedule fields, reading/forecast APIs, JWT auth, maintainarr-frontend meter panel, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |
| 56 | MaintainArr work-order lifecycle (`maintainarr_work_orders`, CRUD/status APIs, create from defect, JWT auth with personId assignment, maintainarr-frontend work orders panel, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |
| 57 | MaintainArr auto WO generation on PM due (`WorkOrderService.EnsureForDuePmScheduleAsync`, extended `process-due-scan`, linked WO on `/due` API, maintainarr-frontend due panel, migration index, unit + integration tests, docs) | M7/M12 (partial) | Complete | `pending` |

| 58 | MaintainArr PM program builder (`maintainarr_pm_programs`, program-schedule junction, CRUD `/api/preventive-maintenance/programs`, assign schedules, JWT auth, maintainarr-frontend builder panel, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |

| 59 | MaintainArr maintenance history (`MaintenanceHistoryService`, aggregated timeline from inspections/defects/work orders/PM, `GET /api/maintenance-history`, JWT asset-read auth, maintainarr-frontend history panel, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |

| 60 | MaintainArr asset readiness endpoint (`AssetReadinessService`, real computation from defects/work orders/PM/inspections, `GET /api/asset-readiness`, JWT asset-read auth, maintainarr-frontend readiness column on asset list, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |

| 61 | MaintainArr labor/evidence capture (`maintainarr_work_order_task_lines`, `maintainarr_work_order_labor_entries`, `maintainarr_work_order_evidence`, tasks/labor/evidence APIs, evidence file storage, JWT auth, maintainarr-frontend labor/evidence on work order detail, integration + frontend tests, docs) | M7 (partial) | Complete | `pending` |

| 62 | SupplyArr vendor/procurement foundations (`supplyarr_external_parties`, `supplyarr_party_contacts`, `/api/parties` `/api/vendors` `/api/dealers` `/api/suppliers`, JWT auth, supplyarr-frontend shell, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 63 | SupplyArr part catalog foundations (`supplyarr_part_catalogs`, `supplyarr_parts`, `supplyarr_part_manufacturer_aliases`, `supplyarr_part_vendor_links`, `/api/catalogs` `/api/parts`, vendor links, JWT auth, supplyarr-frontend part catalog panel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 64 | SupplyArr inventory location foundations (`supplyarr_inventory_locations`, `supplyarr_inventory_bins`, `supplyarr_part_stock_levels`, `/api/inventory` locations/bins/stock, JWT auth, supplyarr-frontend inventory panel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 65 | SupplyArr purchase request foundations (`supplyarr_purchase_requests`, `supplyarr_purchase_request_lines`, `/api/purchase-requests` CRUD + submit/approve/reject, JWT auth, supplyarr-frontend purchase request panel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 66 | SupplyArr purchase order foundations (`supplyarr_purchase_orders`, `supplyarr_purchase_order_lines`, `/api/purchase-orders` from approved PR + approve/issue, JWT auth, supplyarr-frontend purchase order panel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 67 | SupplyArr receiving foundations (`supplyarr_receiving_receipts`, `supplyarr_receiving_receipt_lines`, PO line `QuantityReceived`, `/api/receiving` draft/post against issued PO + stock increment, JWT auth, supplyarr-frontend receiving panel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 68 | SupplyArr receiving exceptions (`supplyarr_receiving_exceptions`, line `QuantityExpected`, short/over/damage APIs + post validation, supplyarr-frontend exception UI, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 69 | RoutArr trip/dispatch foundations (`routarr_trips`, `routarr_trip_loads`, `/api/trips` create/list/assign-driver/status, JWT auth, NexArr handoff, routarr-frontend shell on 5180, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 70 | RoutArr route/stop foundations (`routarr_routes`, `routarr_route_stops`, `/api/routes` create/list/link-trip/reorder, `/api/stops` list/status, JWT auth, routarr-frontend routes panel, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 71 | RoutArr dispatch board foundations (`DispatchBoardService`, `GET /api/dispatch/board` daily/weekly scope, work queue + late/at-risk aggregates, JWT auth, routarr-frontend DispatchBoardPanel, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 72 | RoutArr route calendar (`RouteCalendarService`, `GET /api/dispatch/calendar` daily/weekly/custom date range, day-bucketed trips/routes/stops, JWT auth, routarr-frontend RouteCalendarPanel, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 73 | SupplyArr backorders (`supplyarr_backorders`, auto-sync on receiving post, `/api/backorders` list/create/fulfill/cancel, PR/PO line links, JWT auth, supplyarr-frontend BackordersPanel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 74 | RoutArr driver availability panel (`routarr_driver_availability`, CRUD `/api/driver-availability`, conflict with assigned trips, `GET /api/dispatch/driver-availability`, JWT auth, routarr-frontend DriverAvailabilityPanel, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 75 | SupplyArr returns (`supplyarr_vendor_returns`, `supplyarr_vendor_return_lines`, `/api/returns` from stock/PO line, RMA, stock decrement on post, JWT auth, supplyarr-frontend ReturnsPanel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 76 | RoutArr equipment availability panel (`routarr_equipment_availability`, CRUD `/api/equipment-availability`, conflict with assigned trips, `GET /api/dispatch/equipment-availability`, JWT auth, routarr-frontend EquipmentAvailabilityPanel, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 77 | SupplyArr pricing/lead-time snapshots (`supplyarr_part_vendor_pricing_snapshots`, `supplyarr_part_vendor_lead_time_snapshots`, `/api/pricing-snapshots` `/api/lead-time-snapshots`, effective dating, JWT auth, supplyarr-frontend PricingLeadTimePanel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 78 | RoutArr drag-and-drop assignment (`DispatchAssignmentService`, `POST /api/dispatch/assignments/preview`, enhanced assign-driver + assign-vehicle with conflict checks, `DispatchAssignmentPanel` HTML5 DnD UI, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 79 | SupplyArr availability snapshots (`supplyarr_part_vendor_availability_snapshots`, `/api/availability-snapshots`, effective dating, qty/status history, JWT auth, supplyarr-frontend AvailabilitySnapshotsPanel, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 80 | RoutArr bulk dispatch (`BulkDispatchService`, `POST /api/dispatch/bulk/preview` + `/bulk/apply`, intra-batch conflict preview, `BulkDispatchPanel`, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 81 | SupplyArr reorder evaluation (`reorder_point`/`reorder_quantity` on parts, `ReorderEvaluationService`, `/api/reorder-evaluation`, internal worker API, `SupplyArrReorderEvaluationJob`, `ReorderEvaluationPanel`, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 82 | RoutArr dispatch closeout (`DispatchCloseoutService`, `GET/POST /api/dispatch/closeout/summary|preview|apply`, complete/cancel trips + close routes/stops, JWT assign auth, `DispatchCloseoutPanel`, integration + frontend tests, docs) | M9 (partial) | Complete | `pending` |

| 83 | SupplyArr demand intake from MaintainArr (`maintainarr_work_order_parts_demand_lines`, `supplyarr_maintainarr_demand_refs`, integration ingest + publish, optional PR draft, `/api/demand-refs`, frontends, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 84 | RoutArr driver eligibility (`DriverEligibilityService`, TrainArr/StaffArr integration endpoints, `POST /api/driver-eligibility/check`, assign/preview/bulk gates, routarr-frontend warnings, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 85 | SupplyArr demand status callbacks to MaintainArr (`maintainarr_work_order_parts_demand_status_events`, procurement fields on demand lines, `POST /api/integrations/supplyarr-demand-status`, SupplyArr PR/PO/receiving hooks, frontends, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 86 | RoutArr asset dispatchability (`AssetDispatchabilityService`, MaintainArr `GET /api/integrations/routarr-asset-readiness`, `POST /api/asset-dispatchability/check`, assign/preview/bulk gates, routarr-frontend warnings, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 87 | RoutArr Compliance Core dispatch workflow gates (`DispatchWorkflowGateService`, Compliance Core internal batch gate check, `POST /api/dispatch-workflow-gates/check`, assign/preview/bulk gates + `ignoreWorkflowGateBlocks`, optional Compliance Core dispatch gate seed, routarr-frontend warnings, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 88 | Suite frontend shell enhancements (`ProductSurfaceCatalog`, navigation surfaces + permission hints on `/api/me/navigation`, `navIcons.ts`, `ProductShellLayout`, `ProductSurfaceNav`, `AppTopBar`, nested product route composition, suite-frontend + NexArr tests, docs) | M3 (partial) | Complete | `pending` |

| 89 | Render V1 deployment hardening (`render.yaml` full inventory, env groups, 7 static frontends, `shared-worker`, internal API URLs, health checks, `ENV_VARS_V1.md`, `StlServiceUrl`, JWT env aliases, docs) | M1/M13 (partial) | Complete | `pending` |

| 90 | Companion app field inbox (`FieldInboxContracts`, per-product `GET /api/field-inbox`, NexArr `/api/companion/field-inbox` + handoff auth, `companion-frontend` mobile inbox, `render.yaml` companion static site, integration + frontend tests, docs) | M11 (partial) | Complete | `pending` |

| 91 | M13 E2E verification harness (`STLCompliance.E2E` cross-product integration flows, optional live docker-compose smoke tests, CI skip for Live category, `tests/STLCompliance.E2E/README.md`, gap analysis, docs) | M13 (partial) | Complete | `pending` |

| 92 | M13 OpenAPI parity CI (`STLCompliance.OpenApi.Tests` snapshot gate for 7 APIs, Testing env OpenAPI exposure in `StlApiHost`, checked-in `snapshots/*.openapi.json`, CI OpenAPI step, docs) | M13 (partial) | Complete | `pending` |

| 93 | M13 platform health aggregation (`GET /api/platform/health` probing product `/health/ready`, `PlatformHealthService`, NexArr tests, OpenAPI snapshot, admin test fix, docs) | M13 (partial) | Complete | `pending` |

| 94 | M13 Playwright browser smoke scaffold (`tests/e2e-playwright` suite login→handoff→StaffArr redirect, E2E_LIVE skip semantics), `FINAL_IMPLEMENTATION_REPORT.md`, Release test sweep (575 pass), docs) | M13 (partial) | Complete | `pending` |

| 95 | M13 multi-tenant isolation E2E battery (`TenantIsolationFlowTests`, `TenantIsolationLiveTests`, nightly `e2e-nightly.yml`, docs) | M13 (partial) | Complete | `pending` |

| 96 | M13 SupplyArr tenant isolation E2E (`SupplyArr` cross-tenant GET/list, MaintainArr demand ingest 403, live SupplyArr probe, docs) | M13 (partial) | Complete | `pending` |

| 97 | Shared NexArr handoff client dedup (`StlNexArrHandoffClient` in Shared, remove 6 duplicate clients/contracts, product DI + test wiring, docs) | M13 (partial) | Complete | `75ab4b5` |

| 98 | M13 OTEL smoke checks (`StlOpenTelemetryExtensions`, platform metrics, `/health/observability`, `STLCompliance.Otel.Tests`, `scripts/ops/otel-smoke.ps1`, CI, docs) | M13 (partial) | Complete | `48a6dc0` |
| 99 | M13 DR restore drill (`StlProductDatabaseCatalog`, `StlDrRestoreDrillValidator`, `scripts/ops/dr-restore-drill.ps1|.sh`, `STLCompliance.Dr.Tests`, nightly live drill, docs) | M13 (partial) | Complete | `a407120` |

| 100 | M13 load-test harness (`StlLoadTestSloCatalog`, k6 scenarios, SLO evaluator, `scripts/ops/load-test-run.*`, `STLCompliance.Load.Tests`, nightly live k6, docs) | M13 (partial) | Complete | `a081ee0` |
| 101 | M13 Playwright compose e2e profile (`docker-compose.e2e.yml`, `Dockerfile.frontend-e2e`, all product frontend previews 5174–5180, six-product handoff Playwright smokes, `StlE2eFrontendCatalog`, nightly CI, docs) | M13 (partial) | Complete | `44ec92f` |

| 102 | M13 seven-database DR nightly drill (`DrRestoreDrillLiveRunner`, `[Theory]` live restore for all `StlProductDatabaseCatalog` databases, nightly e2e job label, docs) | M13 (partial) | Complete | `15e76e7` |

| 103 | M13 Render staging snapshot DR drill (`StlRenderStagingDrillCatalog`, staging URL env conventions, `render-staging-snapshot-fetch` + `render-staging-dr-restore-drill` scripts, `RenderStagingDrillLiveRunner`, `dr-staging-render.yml` workflow_dispatch, docs) | M13 (partial) | Complete | `ad6ece7` |

| 104 | M13 authenticated k6 load-test flows (`StlLoadTestAuthDefaults`, `nexarr-auth-me` + `product-auth-handoff-me` k6 scenarios, shared `stl-auth.js` helpers, extended SLO catalog, operator scripts, Load.Tests unit + live probes, docs) | M13 (partial) | Complete | `b2991d0` |

| 105 | Product workspace shell session bootstrap (`ProductWorkspaceFrame`, `/api/me` bootstrap in all product `ProductWorkspaceLayout`s, compact companion variant, shared-ui + StaffArr tests, docs) | M3 (partial) | Complete | `98c09cb` |

| 106 | StaffArr audit package export foundations (`AuditPackageService`, `/api/audit-packages` manifest + ZIP/JSON export, `staffarr.audit.export` auth, StaffArr frontend panel, integration + frontend tests, docs) | M12 (partial) | Complete | `2778bde` |

| 107 | StaffArr person update/deactivate workflows (`PUT /api/people/{personId}`, `PATCH /api/people/{personId}/employment-status`, validation + audit, `PersonProfileEditorPanel`, integration + frontend tests, docs) | M4 (partial) | Complete | `34ef1e0` |

| 108 | M13 product-owner SLO adoption (`PRODUCT_OWNER_LOAD_SLO_V1.md`, `ProductOwnerTargets` profile, journey k6 scenarios for TrainArr qualification + RoutArr dispatch gates, operator scripts, Load.Tests, docs) | M13 (partial) | Complete | `383f3ae` |

| 109 | StaffArr bulk person onboarding import (`POST /api/people/import`, row-level validation + dry run, managerEmail resolution, `PersonBulkImportPanel`, integration + frontend tests, docs) | M4 (partial) | Complete | `9edd755` |

| 110 | M13 nightly live k6 all PO scenarios (`StlLoadTestLiveScenarioCatalog`, theory-based Load live tests ×7, seven-API health gate, e2e-nightly workflow, docs) | M13 (partial) | Complete | `8ef05f6` |

| 111 | StaffArr person export bundle (`GET /api/people/export` CSV/JSON/ZIP, import-compatible CSV, managerEmail resolution, `PersonExportPanel`, integration + frontend tests, docs) | M4 (partial) | Complete | `9abb7d9` |
| 112 | M13 Render staging load soak (`StlRenderStagingLoadTestCatalog`, staging URL env conventions, `render-staging-load-soak` scripts, `RenderStagingLoadSoakLiveTests`, `load-staging-render.yml` workflow_dispatch, docs) | M13 (partial) | Complete | `2c5e359` |
| 113 | StaffArr person export org-unit filter UI (`PersonExportPanel` org-unit dropdown, `orgUnitId` export filter wiring, integration + frontend tests, docs) | M4 (partial) | Complete | `fc6d732` |
| 114 | M13 weekly Render staging load soak CI (`StlRenderStagingLoadSoakScheduleCatalog`, `load-staging-render.yml` schedule + secret gate, docs) | M13 (partial) | Complete | `7e7dd3b` |
| 115 | StaffArr person export filter presets (`personExportFilterPresets`, `PersonExportPanel` quick presets, combined filter integration + frontend tests, docs) | M4 (partial) | Complete | `475226f` |
| 116 | Compliance Core journey k6 seeds (`LoadTestJourneySeedService`, `POST /api/load-test-journey/seed`, staging journey seed scripts, load-staging pre-step, tests, docs) | M5/M13 (partial) | Complete | `37e44f8` |

| 117 | StaffArr tenant person export preset persistence (`TenantPersonExportPreset`, GET/PUT `/api/people/export/preset`, audit, `PersonExportPanel` tenant default UI, tests, docs) | M4 (partial) | Complete | `fcb5a84` |

| 118 | TrainArr staging qualification mirror seed (`LoadTestJourneySeedService`, POST `/api/load-test-journey/seed`, issued qualification mirror for k6, operator scripts, load-staging pre-step, tests, docs) | M6/M13 (partial) | Complete | `eb97cea` |

| 119 | StaffArr export scheduled delivery foundations (`TenantPersonExportSchedule`, internal delivery batch API, delivery run audit, shared-worker job, `PersonExportPanel` schedule UI, tests, docs) | M4/M12 (partial) | Complete | `a70db60` |

| 120 | RoutArr staging trip mirror seed for dispatch gate k6 journeys (`LoadTestJourneySeedService`, `POST /api/load-test-journey/seed`, planned trip mirror for demo subject, operator scripts, load-staging pre-step, tests, docs) | M9/M13 (partial) | Complete | `6a2cd3f` |

| 121 | StaffArr export delivery notification hooks (`PersonExportDeliveryNotificationService`, webhook URL on schedule, success/failure HTTPS hooks, notification audit + list API, `PersonExportPanel` UI, tests, docs) | M4/M12 (partial) | Complete | `193620a` |

| 122 | k6 optional `STL_LOAD_JOURNEY_TRIP_ID` from RoutArr seed (`resolveJourneyTripId`, `GET /api/load-test-journey/trip`, seed scripts export env, staging soak catalog, tests, docs) | M13 (partial) | Complete | `ebf326f` |

| 123 | TrainArr notification settings foundations (`trainarr_tenant_training_notification_settings`, dispatch outbox, GET/PUT settings + dispatch list, internal process-batch, shared-worker job, assignment/expire hooks, trainarr-frontend panel, tests, docs) | M6/M12 (partial) | Complete | `a78fad9` |

| 124 | Playwright shell tenant chrome after handoff (`TenantDisplayName` on handoff redeem, `WorkspaceUserChrome` test ids, suite + six-product Playwright specs, docs) | M3/M13 (partial) | Complete | `fdac1d7` |

| 125 | MaintainArr notification settings foundations (`maintainarr_tenant_notification_settings`, dispatch outbox, GET/PUT settings + dispatch list, internal process-batch, shared-worker job, work order/PM hooks, maintainarr-frontend panel, tests, docs) | M12 (partial) | Complete | `20f4ed2` |

| 126 | StaffArr audit package export + timeline (`GET /api/audit-packages/export` ZIP/JSON, `GET /api/audit-packages/timeline` paginated audit browse, date filters, `AuditPackageExportPanel` timeline preview, tests, docs; builds on W106 `2778bde`) | M4/M12 (partial) | Complete | `6e3bf19` |

| 127 | RoutArr dispatch notification hooks (`routarr_tenant_notification_settings`, dispatch outbox, GET/PUT settings + dispatch list, internal process-batch, shared-worker job, trip assign/status hooks, routarr-frontend panel, tests, docs) | M9/M12 (partial) | Complete | `a99c193` |

| 128 | StaffArr async audit package generation worker (`staffarr_audit_package_generation_jobs`, POST/GET job APIs + download, internal process-batch, shared-worker job, `AuditPackageExportPanel` background ZIP + status, tests, docs; builds on W106/W126) | M4/M12 (partial) | Complete | `72d6e2a` |

| 129 | SupplyArr notification settings (`supplyarr_tenant_notification_settings`, dispatch outbox, GET/PUT settings + dispatch list, internal process-batch, shared-worker job, PR/PO/receiving hooks, supplyarr-frontend panel, tests, docs) | M7/M12 (partial) | Complete | `9a59adf` |

| 130 | Compliance Core async audit package generation (`compliancecore_audit_package_generation_jobs`, POST/GET job APIs + download, internal process-batch, shared-worker job, `AuditPackageExportPanel` background ZIP + status, tests, docs) | M5/M12 (partial) | Complete | `ec55449` |

| 131 | Companion operational notification hooks (`nexarr_companion_notification_*`, companion notification-settings APIs, internal process-batch, shared-worker job, handoff + field-inbox hooks, `companion-frontend` admin panel, tests, docs) | M12 (partial) | Complete | `fc16d20` |

| 132 | MaintainArr async audit package generation (`maintainarr_audit_package_generation_jobs`, sync manifest/export + POST/GET job APIs + download, internal process-batch, shared-worker job, `AuditPackageExportPanel` background ZIP + status, tests, docs) | M6/M12 (partial) | Complete | `968b63c` |

| 133 | TrainArr field-inbox deep links (`deepLinkPath` + `deepLinkUrl` on field inbox items, `/assignments/{id}` + `/evidence` TrainArr routes, companion inbox prefers API URL, tests, docs) | M6/M13 (partial) | Complete | `68984fa` |

| 134 | M13 Playwright deep-link E2E (`companion-field-inbox-trainarr-deep-link`, `product-trainarr-assignment-deep-link`, `StlE2ePlaywrightSpecCatalog`, companion preview 5181, `e2eApi` journey seed, `Category=E2e` catalog tests, docs) | M13 (partial) | Complete | `4783cb9` |

| 135 | STLComplianceSite marketing spine (`apps/stlcompliancesite`, homepage + product pages + demo/contact + privacy/terms + security/data ownership, branding, `render.yaml` static site, CI, vitest, docs) | M3 (partial) | Complete | `2989c35` |

| 136 | NexArr platform audit export (`/api/platform-admin/audit-packages` manifest/timeline/export + async jobs, internal process-batch, shared-worker job, suite platform-admin audit export UI, tests, docs) | M12 (partial) | Complete | `414454e` |

| 137 | STLComplianceSite SEO / products hub hardening (resources section, richer products hub + maturity table, OG/Twitter/canonical SEO, build-time sitemap/robots, vitest, Render headers + `VITE_SITE_BASE_URL`, docs) | M12 (partial) | Complete | `9e40d40` |

| 138 | M13 Playwright platform-admin audit export smoke (`platform-admin-audit-export-smoke.spec.ts`, suite `/app/platform-admin/audit-export`, manifest/timeline/sync ZIP+JSON+background job via internal process-batch helper, `StlE2ePlaywrightSpecCatalog`, panel test ids, docs) | M13 (partial) | Complete | `2293d6a` |

| 139 | STLComplianceSite pricing narrative (`/pricing` licensing page, no checkout, nav/footer/resources links, SEO + sitemap, vitest, docs) | M3 (partial) | Complete | `eafc0f6` |

| 140 | M13 operations deep-link E2E (MaintainArr/RoutArr/SupplyArr SPA workspace routes, field inbox `deepLinkUrl`, companion Playwright spec, e2eApi fixtures, catalog, docker/render frontend base URLs, docs) | M13 (partial) | Complete | `45c6b22` |

| 141 | STLComplianceSite comparison content (`/compare` vs spreadsheets/point tools, nav/footer/resources, SEO + sitemap, vitest, docs) | M3/M12 (partial) | Complete | `d9897a9` |

| 142 | STLComplianceSite implementation maturity status (`/maturity` product + milestone snapshot, nav/footer/resources, SEO + sitemap, vitest, docs) | M3/M12 (partial) | Complete | `be0e623` |

| 143 | M13 k6 journey extensions (StaffArr readiness, SupplyArr procurement PR, MaintainArr work order, Compliance Core evaluate, stl-journey/config, SLO/live/staging catalogs, ops scripts, Load.Tests, docs) | M13 (partial) | Complete | `d4217e4` |

| 144 | M13 Playwright/E2E expansion (Compliance Core operator rule evaluate smoke, suite multi-product handoff journey, e2eApi/handoff helpers, catalog, compliancecore test ids, docs) | M13 (partial) | Complete | `4c0ac21` |

| 145 | M13 ship-gate hardening (`StlM13ShipGateCatalog`, entitlement-denial E2E + live probes, NexArr tenant isolation, OpenAPI ship-gate catalog tests, docs) | M13 (partial) | Complete | `8178be2` |

| 146 | Companion offline queue + notification Playwright E2E (`nexarr_companion_offline_actions`, companion offline sync UI, push readiness, Playwright spec, e2eApi, catalog, tests, docs) | M11/M13 (partial) | Complete | `0e434f9` |

| 147 | Companion field evidence capture (`POST /api/companion/field-tasks/evidence`, TrainArr proxy, photo/document/signature UI, Playwright spec, tests, docs) | M11 (partial) | Complete | `e68e2e8` |

| 148 | Companion clear submission state (`nexarr_companion_field_submissions`, submission-status API, inbox status chips + activity toasts, offline/evidence integration, tests, docs) | M11 (partial) | Complete | `da42bf4` |

| 149 | Companion product switcher (`ProductSwitcher` handoff mode, launch context + handoff APIs, `useCompanionProductLaunch`, tests, docs) | M11 (partial) | Complete | `f385251` |

| 150 | Companion QR/barcode scan (`POST /api/companion/scan/resolve`, payload parser, `FieldScanPanel` + `@zxing/browser`, inbox highlight, tests, docs) | M11 (partial) | Complete | `5841339` |

| 151 | Companion server-side field task validation (`POST /api/companion/field-tasks/validate`, `CompanionFieldTaskValidationService`, plain denied catalog, offline sync + evidence enforcement, companion preflight + plain error UI, tests, docs) | M11 (partial) | Complete | `38b45c4` |

| 152 | Companion offline sync hardening (per-item sync rejections, partial batch results, queue cap 50, retryable vs permanent client handling, tests, docs) | M11 (partial) | Complete | `e0b8bd2` |

| 153 | Companion Web Push subscription and delivery (`nexarr_companion_push_subscriptions`, VAPID APIs, Web Push on notification dispatch, service worker + companion subscribe UI, render VAPID env, tests, docs) | M11 (partial) | Complete | `b5bbd69` |

| 154 | StaffArr personnel notes + documents foundations (`staffarr_personnel_notes`, `staffarr_personnel_documents`, note/document APIs with visibility + file storage, person timeline integration, People workspace UI, tests, docs) | M4 (partial) | Complete | `0a0e25f` |

| 155 | StaffArr product-facing person lookup API (`PersonLookupService`, `/api/person-lookup` + nested lookup routes, TrainArr `/api/integrations/person-lookup`, `staffarr.person.lookup`, `PersonLookupPanel`, tests, docs) | M4 (partial) | Complete | `99c8d67` |

| 156 | StaffArr personnel history rollup worker (`staffarr_personnel_history_rollups` + `staffarr_personnel_history_events`, internal pending/process-batch, `staffarr.personnel.history.rollup`, `/api/person-history` + TrainArr integration read, `StaffArrPersonnelHistoryRollupJob`, `PersonHistorySummaryPanel`, tests, docs) | M12 (partial) | Complete | `cbfa6db` |

| 157 | TrainArr recertification assignment worker (`trainarr_tenant_recertification_settings`, `trainarr_recertification_assignment_runs`, `SourceQualificationIssueId`, internal pending/process-batch, `trainarr.recertification.assign`, `/api/recertification-settings`, `TrainArrRecertificationAssignmentJob`, `RecertificationSettingsPanel`, tests, docs) | M12 (partial) | Complete | `62301b0` |

| 158 | TrainArr qualification recalculation worker (`trainarr_tenant_qualification_recalculation_settings`, `trainarr_qualification_recalculation_states`, `trainarr_qualification_recalculation_runs`, internal pending/process-batch, `trainarr.qualifications.recalculate`, `/api/qualification-recalculation-settings`, `TrainArrQualificationRecalculationJob`, `QualificationRecalculationSettingsPanel`, tests, docs) | M12 (partial) | Complete | `f3ecd04` |

| 159 | TrainArr StaffArr publish retry worker (`trainarr_tenant_staffarr_publication_settings`, `trainarr_staffarr_publication_deliveries`, outbox delivery on certification publications, internal pending/process-batch, `trainarr.staffarr_publications.retry`, `/api/staffarr-publication-settings`, `TrainArrStaffarrPublicationRetryJob`, `StaffarrPublicationSettingsPanel`, tests, docs) | M12 (partial) | Complete | `e6e63dc` |

| 160 | TrainArr event processing worker (`trainarr_tenant_event_processing_settings`, `trainarr_training_domain_events`, `trainarr_person_training_history_entries`, internal pending/process-batch, `trainarr.events.process`, `/api/event-processing-settings`, `/api/person-training-history`, `TrainArrEventProcessingJob`, `EventProcessingSettingsPanel`, `PersonTrainingHistoryPanel`, tests, docs) | M12 (partial) | Complete | `0a4b8ca` |

| 161 | TrainArr notification dispatch worker enhancements (retry policy on settings/dispatches, expanded lifecycle event kinds + webhooks, domain-event fan-out to notification outbox, `NotificationSettingsPanel` updates, tests, docs; builds on W123) | M12 (partial) | Complete | `b2d4d2a` |

| 162 | TrainArr rule-pack impact worker (`trainarr_tenant_rule_pack_impact_settings`, `trainarr_rule_pack_impact_states`, `trainarr_rule_pack_impact_runs`, internal pending/process-batch, `trainarr.rulepack_impact.scan`, `/api/rule-pack-impact-settings`, `TrainArrRulePackImpactJob`, `RulePackImpactSettingsPanel`, tests, docs; builds on W42) | M12 (partial) | Complete | `a3e992d` |

| 163 | TrainArr evidence retention worker (`trainarr_tenant_evidence_retention_settings`, `trainarr_evidence_retention_runs`, internal pending/process-batch, `trainarr.evidence.retention.purge`, `/api/evidence-retention-settings`, `TrainArrEvidenceRetentionJob`, `EvidenceRetentionSettingsPanel`, storage purge, tests, docs) | M12 (partial) | Complete | `be584af` |

| 164 | TrainArr orphan reference detection worker (`trainarr_tenant_orphan_reference_settings`, `trainarr_orphan_reference_findings`, `trainarr_orphan_reference_runs`, internal pending/process-batch, `trainarr.orphan_references.scan`, `/api/orphan-reference-settings`, `TrainArrOrphanReferenceJob`, `OrphanReferenceSettingsPanel`, StaffArr/Compliance Core validation, tests, docs) | M12 (partial) | Complete | `51db338` |

| 165 | TrainArr training audit package (`AuditPackageService`, `/api/audit-packages` manifest + ZIP/JSON export, `trainarr.audit.export` auth, trainarr-frontend `AuditPackageExportPanel`, integration + frontend tests, docs) | M12 (partial) | Complete | `55bc9e1` |

| 166 | TrainArr integration settings (`trainarr_tenant_integration_settings`, GET/PUT `/api/integration-settings` + probes, enforcement on StaffArr/Compliance Core/RoutArr paths, `IntegrationSettingsPanel`, integration + frontend tests, docs) | M6 (partial) | Complete | `ea3e198` |

| 167 | TrainArr async audit package generation (`trainarr_audit_package_generation_jobs`, sync manifest/export + POST/GET job APIs + download, internal process-batch, shared-worker job, `AuditPackageExportPanel` background ZIP + status, tests, docs; builds on W165) | M12 (partial) | Complete | `4a45dfd` |

| 168 | NexArr service-token cleanup worker (`nexarr_platform_service_token_cleanup_settings`, `nexarr_service_token_cleanup_runs`, internal pending/process-batch, `nexarr.service_tokens.cleanup.purge`, `NexArrServiceTokenCleanupJob`, platform-admin service token cleanup UI, tests, docs) | M12 (partial) | Complete | `c0f0189` |

| 169 | NexArr entitlement reconciliation worker (`nexarr_tenant_product_licenses`, `nexarr_platform_entitlement_reconciliation_settings`, `nexarr_entitlement_reconciliation_runs`, internal pending/process-batch, `nexarr.entitlements.reconcile`, `NexArrEntitlementReconciliationJob`, platform-admin entitlement reconciliation UI, tests, docs) | M12 (partial) | Complete | `381ba05` |

| 170 | NexArr tenant lifecycle worker (`nexarr_platform_tenant_lifecycle_settings`, `nexarr_tenant_lifecycle_runs`, internal pending/process-batch, `nexarr.tenants.lifecycle.process`, `NexArrTenantLifecycleJob`, platform-admin tenant lifecycle UI, session revoke on suspend, tests, docs) | M12 (partial) | Complete | `389221d` |

| 171 | MaintainArr defect escalation worker (`maintainarr_tenant_defect_escalation_settings`, `maintainarr_defect_escalation_runs`, `maintainarr_defect_escalation_events`, internal pending/process-batch, `maintainarr.defects.escalate`, `MaintainArrDefectEscalationJob`, `DefectEscalationSettingsPanel`, defect `LastEscalatedAt`/`EscalationCount`, tests, docs) | M12 (partial) | Complete | `b01745d` |

| 172 | MaintainArr asset status rollup worker (`maintainarr_tenant_asset_status_rollup_settings`, `maintainarr_asset_status_rollups`, `maintainarr_asset_status_scope_rollups`, `maintainarr_asset_status_rollup_runs`, internal pending/process-batch, `maintainarr.asset_status.rollup`, `MaintainArrAssetStatusRollupJob`, materialized-first asset readiness reads, `AssetStatusRollupSettingsPanel`, tests, docs) | M12 (partial) | Complete | `7d60d7b` |

| 173 | MaintainArr maintenance history rollup worker (`maintainarr_tenant_maintenance_history_rollup_settings`, `maintainarr_maintenance_history_rollups`, `maintainarr_maintenance_history_events`, `maintainarr_maintenance_history_rollup_runs`, internal pending/process-batch, `maintainarr.maintenance_history.rollup`, `MaintainArrMaintenanceHistoryRollupJob`, materialized-first maintenance history reads + summary API, `MaintenanceHistoryRollupSettingsPanel`, tests, docs) | M12 (partial) | Complete | `d7d9328` |

| 174 | SupplyArr price snapshot worker (`supplyarr_tenant_price_snapshot_settings`, `supplyarr_part_vendor_price_capture_states`, `supplyarr_price_snapshot_runs`, vendor link catalog price fields, internal pending/process-batch, `supplyarr.pricing.snapshots.capture`, `SupplyArrPriceSnapshotJob`, `/api/price-snapshot-settings`, `PriceSnapshotSettingsPanel`, tests, docs) | M12 (partial) | Complete | `41bcb89` |

| 175 | SupplyArr lead-time snapshot worker (`supplyarr_tenant_lead_time_snapshot_settings`, `supplyarr_part_vendor_lead_time_capture_states`, `supplyarr_lead_time_snapshot_runs`, vendor link catalog lead time field, internal pending/process-batch, `supplyarr.leadtime.snapshots.capture`, `SupplyArrLeadTimeSnapshotJob`, `/api/lead-time-snapshot-settings`, `LeadTimeSnapshotSettingsPanel`, tests, docs) | M12 (partial) | Complete | `118137c` |

| 176 | RoutArr trip completion rollup worker (`routarr_tenant_trip_completion_rollup_settings`, `routarr_trip_completion_rollups`, `routarr_trip_completion_events`, `routarr_trip_completion_rollup_runs`, internal pending/process-batch, `routarr.trips.completion.rollup`, `/api/trip-completions` + `/api/route-completions`, `RoutArrTripCompletionRollupJob`, `TripCompletionRollupSettingsPanel`, tests, docs) | M12 (partial) | Complete | `b786225` |

| 177 | SupplyArr procurement coordination worker (`supplyarr_tenant_procurement_coordination_settings`, `supplyarr_procurement_coordination_records`, `supplyarr_procurement_coordination_events`, `supplyarr_procurement_coordination_runs`, internal pending/process-batch, `supplyarr.procurement.coordination`, `/api/procurement-coordination` + settings, `SupplyArrProcurementCoordinationJob`, `ProcurementCoordinationSettingsPanel`, `ProcurementCoordinationPanel`, tests, docs) | M12 (partial) | Complete | `0cddc33` |

| 178 | SupplyArr approval reminder worker (`supplyarr_tenant_approval_reminder_settings`, `supplyarr_approval_reminder_states`, `supplyarr_approval_reminder_runs`, internal pending/process-batch, `supplyarr.approval_reminders.dispatch`, `/api/approval-reminders` + settings, `SupplyArrApprovalRemindersJob`, `ApprovalReminderSettingsPanel`, `ApprovalRemindersPanel`, notification outbox integration, tests, docs) | M12 (partial) | Complete | `8c325d8` |

| 179 | SupplyArr demand processing worker (`supplyarr_tenant_demand_processing_settings`, `supplyarr_demand_processing_states`, `supplyarr_demand_processing_runs`, internal pending/process-batch, `supplyarr.demand.process`, `/api/demand-processing` + settings, `SupplyArrDemandProcessingJob`, `DemandProcessingSettingsPanel`, `DemandProcessingPanel`, stock check + auto PR draft + notification integration, tests, docs) | M12 (partial) | Complete | `bd876e4` |

| 180 | SupplyArr availability snapshot worker (`supplyarr_tenant_availability_snapshot_settings`, `supplyarr_part_vendor_availability_capture_states`, `supplyarr_availability_snapshot_runs`, vendor link catalog availability fields, internal pending/process-batch, `supplyarr.availability.snapshots.capture`, `SupplyArrAvailabilitySnapshotJob`, `/api/availability-snapshot-settings`, `AvailabilitySnapshotSettingsPanel`, tests, docs) | M12 (partial) | Complete | `4d068b7` |

| 181 | SupplyArr vendor reports (`VendorReportService`, `/api/reports/vendors/summary|{id}|summary/export`, audit events, `VendorReportsPanel`, Reports workspace route, integration + frontend tests, docs) | M12 (partial) | Complete | `pending` |

| 182 | SupplyArr parts/inventory reports (`PartsInventoryReportService`, `/api/reports/parts-inventory/summary|parts/{id}|locations/{id}|summary/export`, audit events, `PartsInventoryReportsPanel`, Reports workspace extension, integration + frontend tests, docs) | M12 (partial) | Complete | `pending` |

| 183 | SupplyArr purchasing reports (`PurchasingReportService`, `/api/reports/purchasing/summary|purchase-requests/{id}|purchase-orders/{id}|summary/export`, audit events, `PurchasingReportsPanel`, Reports workspace extension, integration + frontend tests, docs) | M12 (partial) | Complete | `pending` |

| 184 | SupplyArr compliance reports (`supplyarr_party_compliance_documents`, `ComplianceReportService`, `/api/reports/compliance/summary|parties/{id}|summary/export`, audit events, `ComplianceReportsPanel`, Reports workspace extension, integration + frontend tests, docs) | M12 (partial) | Complete | `pending` |

| 185 | SupplyArr forgiving search (`ForgivingSearchService`, `GET /api/search/forgiving`, normalized cross-entity matching, audit events, `ForgivingSearchBar` workspace shell, integration + frontend tests, docs) | M12 (partial) | Complete | `pending` |

| 186 | SupplyArr audit history (`AuditHistoryService`, `GET /api/audit-history` cursor pagination + filters, meta-audit read, `AuditHistoryPanel` Reports workspace, integration + frontend tests, docs) | M12 (SupplyArr reporting group) | Complete | `pending` |

| 187 | SupplyArr integration event outbox/inbox (`supplyarr_integration_outbox_events`, `supplyarr_integration_inbox_events`, tenant settings + processing runs, internal pending/process-batch + inbox enqueue, `SupplyArrIntegrationEventsJob`, publish hooks on parties/parts/procurement/demand, `maintainarr.demand.ingest` inbox handler, `IntegrationEventSettingsPanel`, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 188 | SupplyArr RFQs + quote comparison (`supplyarr_rfqs`, `supplyarr_rfq_lines`, `supplyarr_rfq_vendor_invitations`, `supplyarr_vendor_quotes`, `supplyarr_vendor_quote_lines`, `/api/rfqs` CRUD/submit/invite/quotes/compare/award/create-purchase-request, outbox hooks, `RfqPanel` Purchasing workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 189 | SupplyArr supplier onboarding (`supplyarr_party_supplier_onboarding`, `supplyarr_tenant_supplier_onboarding_settings`, party compliance doc register/approve API, `/api/supplier-onboarding` + `/api/parties/{id}/compliance-documents`, outbox hooks, `SupplierOnboardingPanel` Parties workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 190 | SupplyArr emergency purchase workflow (emergency fields on `supplyarr_purchase_requests`, `/api/emergency-purchases` create/expedited-submit/manager-override-approve/issue-purchase-order, stricter auth, outbox hooks, `EmergencyPurchasePanel` Purchasing workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 191 | SupplyArr demand intake from RoutArr (`routarr_trip_parts_demand_lines`, `supplyarr_routarr_demand_refs`, integration ingest + publish, inbox `routarr.demand.ingest`, `/api/routarr-demand-refs`, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 192 | SupplyArr demand intake from TrainArr + StaffArr (`trainarr_training_assignment_material_demand_lines`, `staffarr_incident_supply_demand_lines`, `supplyarr_trainarr_demand_refs`, `supplyarr_staffarr_demand_refs`, integration ingest + inbox handlers, demand-ref APIs, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 193 | SupplyArr demand status callbacks to all sources (extend W85 to RoutArr/TrainArr/StaffArr, coordinator on PR/PO/receiving, per-product ingest + status event tables, idempotency, RoutArr + StaffArr + MaintainArr tests, docs) | M10 (partial) | Complete | `pending` |

| 194 | SupplyArr demand processing multi-source auto-PR (W179 extended to RoutArr/TrainArr/StaffArr/MaintainArr, per-source tenant flags, drop MaintainArr-only FK on processing states, dashboard multi-source, supplyarr-frontend source toggles, RoutArr auto-PR test, docs) | M8/M10/M12 (partial) | Complete | `pending` |

| 195 | SupplyArr M8 vendor restrictions (`supplyarr_vendor_restrictions`, scoped enforcement on PR/PO/RFQ/receiving, `/api/vendor-restrictions` + party routes, outbox + audit, `VendorRestrictionsPanel` Parties workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 196 | SupplyArr M8 supplier incidents (`supplyarr_supplier_incidents`, status workflow, party/procurement links, apply-procurement-restriction via W195, `/api/supplier-incidents` + party routes, outbox + audit, `SupplierIncidentsPanel` Parties workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 197 | SupplyArr M8 procurement exceptions (`supplyarr_procurement_exceptions`, PR/PO/RFQ subject workflow, investigate/resolve/waive-with-approval, `/api/procurement-exceptions` + subject routes, outbox + audit, `ProcurementExceptionsPanel` Purchasing workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 198 | SupplyArr M10 approval authority from StaffArr (StaffArr `/api/integrations/procurement-approval-authority`, SupplyArr `supplyarr_staffarr_procurement_approval_authority_mirrors`, enforce PR submit/approve + PO issue, `/api/me/procurement-approval-authority`, `ProcurementApprovalAuthorityBanner` Purchasing workspace, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 199 | SupplyArr M10 Compliance Core fact publishing (`compliancecore_product_fact_mirrors`, `POST /api/integrations/product-facts/ingest`, `product_mirror` fact sources, SupplyArr outbox publisher + `ComplianceCoreFactPublicationClient`, `supplyarr-compliancecore` token profile, cross-product tests, docs) | M10 (partial) | Complete | `pending` |

| 200 | SupplyArr M8 supply readiness dashboard (`SupplyReadinessService`, `GET /api/supply-readiness/dashboard`, audit `supplyarr.supply_readiness.dashboard`, `RequireSupplyReadinessRead`, `/readiness` workspace + `SupplyReadinessDashboardPanel`, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |

| 201 | SupplyArr M8 warranty claims (`supplyarr_warranty_claims`, `/api/warranty-claims` CRUD + submit/vendor-response/close/deny/cancel, audit + outbox, `WarrantyClaimsPanel` Receiving workspace, integration + frontend tests, docs) | M8 (partial) | Complete | `pending` |
| 202 | TrainArr M10 StaffArr acknowledgement tracking (`staffarr_person_training_acknowledgements`, integration ingest/status, JWT list/acknowledge, TrainArr mirror + evidence gate, field inbox, staffarr/trainarr frontends, cross-product tests, docs) | M10 (partial) | Complete | `pending` |
| 203 | MaintainArr M12 maintenance reports (`MaintenanceReportService`, `/api/reports/maintenance` summary/detail/export, audit, `MaintenanceReportsPanel` Reports workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 204 | MaintainArr M12 executive reports (`ExecutiveReportService`, `/api/reports/executive` summary/export, fleet/scope readiness + SupplyArr demand rollups, audit, `ExecutiveReportsPanel` Reports workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 205 | MaintainArr M12 compliance reports (`maintainarr_compliance_regulatory_key_mirrors`, `ComplianceReportService`, `/api/reports/compliance` summary/template/export, audit, `ComplianceReportsPanel` Reports workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 206 | MaintainArr M12 bulk asset imports (`maintainarr_import_batches`, `AssetBulkImportService`, `/api/imports/assets/validate` + `/commit`, CSV/JSON upload, audit, `AssetBulkImportPanel` Settings workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 207 | MaintainArr M12 data exports (`EntityBulkExportService`, `/api/exports` manifest + assets/work-orders/inspection-runs CSV, audit, `DataExportsPanel` Reports workspace coordinated with W203–205 report CSV + W132 audit packages, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 208 | NexArr M12 platform lifecycle umbrella (`PlatformLifecycleOverviewService`, `GET /api/platform-admin/platform-lifecycle/overview`, audit, suite `PlatformLifecycleOverviewPanel` + lifecycle nav; consolidates W168–170 workers in shared-worker, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 209 | RoutArr M9 dispatch command center (`routarr_tenant_dispatch_board_states`, `routarr_staffarr_person_refs`, `DispatchCommandCenterService`, `GET /api/dispatch/command-center`, board-state + driver-refs APIs, assign-driver mirror upsert, `DispatchCommandCenterPanel`, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 210 | RoutArr M9 dispatch exception queue (`routarr_dispatch_exceptions`, `DispatchExceptionService`, `GET/POST /api/dispatch/exceptions`, assign/resolve/link-trip, auth + audit, `DispatchExceptionQueuePanel`, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 211 | RoutArr M9 active trip map/list (`ActiveTripsService`, `GET /api/dispatch/active-trips`, timeline positioning, late/at-risk from board rules, `ActiveTripsPanel` list+map, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 212 | RoutArr M9 unassigned work queue panel (`UnassignedWorkQueueService`, `GET /api/dispatch/unassigned-work-queue`, driver refs, quick assign via assign-driver + bulk apply, `UnassignedWorkQueuePanel`, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 213 | RoutArr M9 trip execution / driver portal (`DriverPortalService`, `GET/POST /api/driver-portal/*`, personId-scoped schedule + dispatch/start/complete/close, auth + audit, `DriverPortalPanel` + `/driver-portal` route, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 214 | RoutArr M12 dispatch/transportation reporting (`DispatchReportService`, `GET /api/reports/dispatch/*` summary/detail/export on trips + exceptions + delay category, auth + audit, `DispatchReportsPanel` + `/reports` workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 215 | RoutArr M12 route/stop execution reporting (`RouteReportService`, `GET /api/reports/routes/*` summary/detail/export on routes + route_stops + completion metrics, auth + audit, `RouteReportsPanel` on Reports workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 216 | RoutArr M12 entity bulk export (`RoutArrEntityBulkExportService`, `GET /api/exports/*` manifest + trips/routes/dispatch-exceptions CSV, auth + audit, `DataExportsPanel` on Reports workspace; proof/DVIR tables absent — W207 pattern, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 217 | RoutArr M9 proof/DVIR persistence & workflow (`TripProofRecord`, `TripDvirInspection`, migration `RoutArrTripProofDvir`, `TripProofDvirService`, trip + driver-portal APIs, personId-scoped driver write + dispatcher read, `DriverPortalPanel` capture + `TripProofDvirReadPanel` on Dispatch, integration + Vitest tests, docs) | M9 (partial) | Complete | `pending` |
| 218 | RoutArr M12 proof/DVIR reporting (`ProofDvirReportService`, `GET /api/reports/proof-dvir/*` summary/detail/export on trip proof + DVIR tables, auth + audit like W214, `ProofDvirReportsPanel` on Reports workspace, manifest entry, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 219 | TrainArr M12 / StaffArr integration — person training history read (`GET /api/integrations/person-training-history`, StaffArr `GET /api/people/{id}/trainarr-training-history`, `staffarr-trainarr` token scope, `PersonTrainarrTrainingHistoryPanel` on People workspace, cross-product + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 220 | Compliance Core M12 source ingestion workflow (`compliancecore_source_ingestion_batches` + jobs, `SourceIngestionService`, fact-sources validate/commit + product-facts integration validate/commit, `compliancecore.sources.ingest`, audit, `SourceIngestionPanel` Admin workspace, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 221 | Compliance Core M12 rule change monitoring (`compliancecore_rule_change_events`, monitor snapshots, `RuleChangeMonitoringService`, API hooks on pack create/status/content, internal scan + `ComplianceCoreRuleChangeMonitorJob`, `RuleChangeMonitoringPanel`, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 222 | Compliance Core M12 risk scoring (`compliancecore_risk_score_runs` + scores, `RiskScoringService` on fact mirrors + rule evaluation, evaluate/list/summary APIs, `RiskScoringPanel`, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 223 | Compliance Core M12 predictive missing-evidence warnings (`compliancecore_missing_evidence_warning_runs` + warnings, `MissingEvidenceWarningService` on rule packs + fact mirrors, evaluate/list/summary APIs, `MissingEvidenceWarningsPanel`, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 224 | Compliance Core M12 control effectiveness tracking (`compliancecore_control_effectiveness_runs` + records, `ControlEffectivenessService` on rule pack outcomes, evaluate/list/summary APIs, `ControlEffectivenessPanel`, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 225 | Compliance Core M12 readiness forecasting (`compliancecore_readiness_forecast_runs` + forecasts, `ReadinessForecastService` from risk/missing-evidence/control-effectiveness, evaluate/list/summary APIs, `ReadinessForecastPanel`, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 226 | NexArr M12 audit export enhancements (rich filters, CSV/JSON export packages, filter-options/summary APIs, manifest v2 + ZIP CSV section, `FilterJson` on generation jobs, `PlatformAuditPackageExportPanel` UX, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 227 | RoutArr M12 audit package export (tenant audit events, rich filters, CSV/JSON/ZIP, filter-options/summary/timeline APIs, async jobs + `FilterJson`, `AuditPackageExportPanel` on Reports, shared-worker job, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 228 | StaffArr M12 personnel audit export enhancements (rich audit-event filters, CSV/JSON/ZIP, filter-options/summary APIs, manifest v2 + ZIP CSV section, `FilterJson` on generation jobs, `AuditPackageExportPanel` UX, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 229 | TrainArr M12 notification settings + scheduled workers (assignment due reminder + overdue escalation settings/runs/events, notification toggles, internal process-batch APIs, shared-worker jobs, `AssignmentReminderEscalationSettingsPanel`, integration + Vitest tests, docs; builds on W123/W161) | M12 (partial) | Complete | `pending` |
| 230 | MaintainArr M12 audit export filter/summary parity (rich audit-event filters, CSV/JSON/ZIP manifest v2, filter-options/summary/timeline APIs, `FilterJson` on generation jobs, `AuditPackageExportPanel` UX, integration + Vitest tests, docs; builds on W132) | M12 (partial) | Complete | `pending` |
| 231 | Compliance Core M12 scheduled analytics batch workers (tenant M12 worker settings, internal pending/process-batch for risk/missing-evidence/control/readiness batches, optional audit package delivery hook, `ComplianceCoreM12AnalyticsBatchJob`, `M12AnalyticsWorkerSettingsPanel`, integration + Vitest tests, docs; builds on W222–225) | M12 (partial) | Complete | `pending` |
| 232 | M13 Playwright product admin smokes (MaintainArr Settings audit export W230 panel, Compliance Core Admin M12 worker settings W231 panel, `e2eApi` worker helpers, `StlE2ePlaywrightSpecCatalog.ProductAdminSmokeSpecs`, catalog tests, docs) | M13 (partial) | Complete | `pending` |
| 233 | TrainArr M12 demand callback visibility (assignment material demand panel with SupplyArr procurement status + status-event timeline, `GET .../material-demand/status-events`, cross-product callback tests, Vitest, docs; builds on W193) | M12 (partial) | Complete | `pending` |
| 234 | M13 Playwright TrainArr material demand smoke (suite handoff → assignment workspace panel, API fixture for procurement badge/timeline, UI add/publish fallback, `e2eApi` helpers, catalog + README, docs; builds on W232–W233) | M13 (partial) | Complete | `pending` |
| 235 | M13 Playwright RoutArr dispatch command center smoke (suite handoff → `/dispatch`, `dispatch-command-center-panel`, scope toggle, status columns or empty state, catalog + README, docs; builds on W209/W232) | M13 (partial) | Complete | `pending` |
| 236 | M13 Playwright SupplyArr admin/settings smoke (suite handoff → Settings integration event panel save + Readiness dashboard, catalog + README, docs; builds on W187/W232) | M13 (partial) | Complete | `pending` |
| 237 | M13 Playwright SupplyArr Reports workspace smoke (suite handoff → `/reports` vendor + purchasing panels, filter interaction, Export CSV present, catalog + README, docs; builds on W181/W183/W236) | M13 (partial) | Complete | `pending` |
| 238 | M13 Playwright StaffArr admin audit export smoke (suite handoff → `/admin` audit package panel manifest/summary/filters/sync+background ZIP, `processStaffArrAuditPackageGenerationBatch`, catalog + README, docs; builds on W228/W232) | M13 (partial) | Complete | `pending` |
| 239 | M13 Playwright TrainArr settings audit export smoke (suite handoff → `/settings` training audit package panel manifest/date filters/JSON summary/sync+background ZIP, `processTrainArrAuditPackageGenerationBatch`, catalog + README, docs; builds on W165/W167/W238) | M13 (partial) | Complete | `pending` |
| 240 | Compliance Core M12 audit delivery orchestration UI (Admin panel tying W47 scheduled eval + W231 M12 batch + audit package jobs; status/pending read APIs, admin manual triggers, integration + Vitest tests, docs) | M12 (partial) | Complete | `pending` |
| 241 | M13 Playwright RoutArr Reports audit export smoke (suite handoff → `/reports` audit package panel manifest/summary/filters/sync+background ZIP, `processRoutArrAuditPackageGenerationBatch`, catalog + README, docs; builds on W227/W238) | M13 (partial) | Complete | `pending` |
| 242 | M13 Playwright Compliance Core audit delivery orchestration smoke (suite handoff → `/admin` orchestration panel status sections + trigger controls visible, catalog + README, docs; builds on W240/W232; no live triggers) | M13 (partial) | Complete | `pending` |
| 243 | M13 Playwright RoutArr dispatch exception queue smoke (suite handoff → `/dispatch` `dispatch-exception-queue-panel`, create form or empty/open rows, catalog + README, docs; builds on W210/W235; no live triage mutations) | M13 (partial) | Complete | `pending` |
| 244 | M13 Playwright RoutArr dispatch active trips smoke (suite handoff → `/dispatch` `active-trips-panel`, list/map toggle, summary tiles, trip rows/map blocks or empty states, catalog + README, docs; builds on W211/W243) | M13 (partial) | Complete | `pending` |
| 245 | M13 Playwright RoutArr dispatch unassigned work queue smoke (suite handoff → `/dispatch` `unassigned-work-queue-panel`, assign/bulk controls when rows present or empty state, catalog + README, docs; builds on W212/W244; no live assignments) | M13 (partial) | Complete | `pending` |
| 246 | SupplyArr M8/M10 procurement coordination operator UX (demand source settings validation + help, purchasing demand-processing retry/PR draft/view-status APIs + panel actions, auth + audit, integration + Vitest tests, docs; builds on W177/W194) | M8/M10 (partial) | Complete | `pending` |
| 247 | M13 Playwright RoutArr driver portal smoke (suite handoff → `/driver-portal` `driver-portal-panel`, Today/Upcoming schedule sections, trip cards or empty state, catalog + README, docs; builds on W213/W245; no dispatch/start/complete clicks) | M13 (partial) | Complete | `pending` |
| 248 | M13 Playwright RoutArr proof/DVIR dispatch read smoke (suite handoff → `/dispatch` `trip-proof-dvir-read-panel`, trip ID lookup, load execution summary, proof/DVIR rows or empty lists, catalog + README, docs; builds on W217/W247; read-only, no capture) | M13 (partial) | Complete | `pending` |
| 249 | MaintainArr PM due-scan worker observability (tenant settings + runs tables, settings/pending/runs/trigger APIs, `PmDueScanSettingsPanel`, shared-worker enabled-tenant batching + run recording, integration + Vitest tests, docs; builds on W51/W57) | M7/M12 (partial) | Complete | `pending` |
| 250 | SupplyArr M8 procurement exception resolution depth (SLA/due, resolver assign API, resolution templates, PR/PO link actions, overdue list, `ProcurementExceptionsPanel` improvements, migration + tests, docs; builds on W197) | M8 (partial) | Complete | `pending` |
| 251 | RoutArr M9 dispatch closeout depth (per-trip checklist API, bulk closeout via `TripIds`, closeout audit list, `DispatchCloseoutPanel` checklist/bulk/audit UX, integration + Vitest tests, docs; builds on W82) | M9 (partial) | Complete | `pending` |

## M12 SupplyArr reporting group

Workers **181–186** complete the SupplyArr M12 reporting slice group (vendor, parts/inventory, purchasing, compliance reports, forgiving search, audit history).

## M8 SupplyArr procurement cluster

Workers **195–197** complete vendor restrictions, supplier incidents, and procurement exceptions.

## Next slice (Worker 230+)

Workers **209–251** complete RoutArr M9/M12 dispatch workspace (API + UI + M13 Playwright for command center, exception queue, active trips, unassigned queue, driver portal, proof/DVIR dispatch read, dispatch closeout depth, Reports audit export), SupplyArr M8/M10 demand-processing operator UX, StaffArr/TrainArr/MaintainArr M12 export and notification workers, MaintainArr PM due-scan worker observability, Compliance Core M12 analytics + audit delivery orchestration UI + Playwright, NexArr platform audit export enhancements, M13 product-admin Playwright smokes across suite products, and TrainArr assignment material demand callback visibility. Recommended backlog next (product features — not M13 Playwright):
- **RoutArr** — drag-assign depth (W78) or dispatch exception triage depth
- **M13 Playwright** — dispatch closeout panel smoke (optional)
- **NexArr M12** — platform-admin service token / worker health orchestration UI (if scoped)