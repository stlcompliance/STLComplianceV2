# Interim Goal Traceability Report

Date: 2026-06-25

This report records verified progress toward the active monorepo alignment objective. It is intentionally interim: it documents what is proven in the current worktree, not a final completion claim.

## Governing sources used

- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/platform-product-availability-compliancecore-access-constitution.md](../constitutions/platform-product-availability-compliancecore-access-constitution.md)
- [docs/user/how-to/platform/how-to-switch-products.md](../user/how-to/platform/how-to-switch-products.md)
- [docs/user/how-to/platform/how-to-manage-a-users-product-permissions.md](../user/how-to/platform/how-to-manage-a-users-product-permissions.md)
- [docs/user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md](../user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md)
- [docs/user/troubleshooting/product-or-feature-not-visible.md](../user/troubleshooting/product-or-feature-not-visible.md)
- [docs/products/nexarr/WORKFLOWS.md](../products/nexarr/WORKFLOWS.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

## Verified alignment completed

### 1. Compliance Core launch restriction is enforced in the shared shell

Requirement:
- Only validated platform administrators may see Compliance Core in the suite switcher and launcher shell.

Evidence:
- [packages/shared-ui/src/ProductSwitcher.tsx](../../packages/shared-ui/src/ProductSwitcher.tsx) filters `compliancecore` unless `showComplianceCore` is enabled.
- [packages/shared-ui/src/ProductAppShell.tsx](../../packages/shared-ui/src/ProductAppShell.tsx) passes `showComplianceCore={isPlatformAdmin}`.
- [packages/shared-ui/src/ProductWorkspaceFrame.tsx](../../packages/shared-ui/src/ProductWorkspaceFrame.tsx) threads platform-admin state into the shell.
- [apps/suite-frontend/src/components/ProductSwitcher.tsx](../../apps/suite-frontend/src/components/ProductSwitcher.tsx) gates the suite switcher with the authenticated user admin flag.
- Tests updated in [packages/shared-ui/src/ProductSwitcher.test.tsx](../../packages/shared-ui/src/ProductSwitcher.test.tsx) and [apps/suite-frontend/src/components/ProductSwitcher.test.tsx](../../apps/suite-frontend/src/components/ProductSwitcher.test.tsx).

Constitution coverage:
- [docs/constitutions/platform-product-availability-compliancecore-access-constitution.md](../constitutions/platform-product-availability-compliancecore-access-constitution.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/ownership.md](../constitutions/ownership.md)

### 2. Product-launch language no longer frames ordinary products as tenant-entitled

Requirement:
- Ordinary products should be presented as suite-available to active tenant members, with product-local permissions and operational state governing actions, not tenant entitlements.

Evidence:
- Updated shared launch copy and launcher behavior in the shell and suite frontend listed above.
- Updated public terms copy in [apps/stlcompliancesite/src/pages/TermsPage.tsx](../../apps/stlcompliancesite/src/pages/TermsPage.tsx) and [apps/stlcompliancesite/src/pages/TermsPage.test.tsx](../../apps/stlcompliancesite/src/pages/TermsPage.test.tsx), verified by `npm test -- --run src/pages/TermsPage.test.tsx`, `npm run build`, and `npm run audit:theme` in `apps/stlcompliancesite`.
- Updated public docs in:
  - [docs/user/how-to/platform/how-to-switch-products.md](../user/how-to/platform/how-to-switch-products.md)
  - [docs/user/how-to/platform/how-to-manage-a-users-product-permissions.md](../user/how-to/platform/how-to-manage-a-users-product-permissions.md)
  - [docs/user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md](../user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md)
  - [docs/user/troubleshooting/product-or-feature-not-visible.md](../user/troubleshooting/product-or-feature-not-visible.md)
  - [docs/products/nexarr/WORKFLOWS.md](../products/nexarr/WORKFLOWS.md)
  - [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
  - [docs/products/nexarr/FEATURESET.md](../products/nexarr/FEATURESET.md)

### 3. Live launch-denial strings were normalized to neutral handoff wording

Requirement:
- Launch-denial and handoff failures should avoid tenant-entitlement phrasing in live runtime code.

Evidence:
- Updated product handoff services:
  - [apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs](../../apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs)
  - [apps/loadarr-api/LoadArr.Api/Services/HandoffAuthService.cs](../../apps/loadarr-api/LoadArr.Api/Services/HandoffAuthService.cs)
  - [apps/recordarr-api/RecordArr.Api/Services/HandoffAuthService.cs](../../apps/recordarr-api/RecordArr.Api/Services/HandoffAuthService.cs)
  - [apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs](../../apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs)
  - [apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs](../../apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs)
  - [apps/ordarr-api/OrdArr.Api/Services/HandoffAuthService.cs](../../apps/ordarr-api/OrdArr.Api/Services/HandoffAuthService.cs)
  - [apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs](../../apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs)
- Updated Field Companion denial catalog in:
  - [apps/nexarr-api/NexArr.Api/Services/FieldCompanionDeniedReasonCatalog.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionDeniedReasonCatalog.cs)
  - [apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.ts](../../apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.ts)

### 4. Shared event/response terminology was aligned away from entitlement naming

Requirement:
- Shared launch/event terminology should use authorization language instead of entitlement language.

Evidence:
- [packages/shared-dotnet/STLCompliance.Shared/Integration/StlProductResponseFramework.cs](../../packages/shared-dotnet/STLCompliance.Shared/Integration/StlProductResponseFramework.cs) renamed the visibility scope to `AuthorizedProducts`.
- [tests/STLCompliance.Shared.Worker.Tests/IntelligentProductResponseFrameworkTests.cs](../../tests/STLCompliance.Shared.Worker.Tests/IntelligentProductResponseFrameworkTests.cs) updated to the new scope.

### 5. One product-specific copy fix was completed

Requirement:
- Live user-facing settings copy should describe authorization plainly.

Evidence:
- [apps/routarr-api/RoutArr.Api/Services/RoutArrTenantSettingsDefinitions.cs](../../apps/routarr-api/RoutArr.Api/Services/RoutArrTenantSettingsDefinitions.cs) now says `Enables carrier collaboration when authorized.`

### 6. A LoadArr feature row was lifted from scaffold to durable based on persisted settings evidence

Requirement:
- Product maturity notes should reflect actual durable behavior when the code and tests already prove it.

Evidence:
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrTenantSettingsEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrTenantSettingsEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsService.cs](../../apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsService.cs)
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)

### 7. Three LoadArr workflow/feature rows were lifted from scaffold to partial or durable based on persisted settings, receiving, and unexplained-inventory evidence

Requirement:
- Workflow maturity should reflect the real implemented slice without claiming a fully finished warehouse onboarding flow.

Evidence:
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrTenantSettingsEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrTenantSettingsEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsService.cs](../../apps/loadarr-api/LoadArr.Api/Settings/LoadArrTenantSettingsService.cs)
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrTenantSettingsTests.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionLoadArrReceivingTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionLoadArrReceivingTests.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)

### 8. Field Companion denial-copy consistency was verified

Requirement:
- Live frontend/backend denial catalogs should match and remain test-covered.

Evidence:
- [tests/STLCompliance.NexArr.Auth.Tests/FieldCompanionDeniedReasonCatalogTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/FieldCompanionDeniedReasonCatalogTests.cs)
- [apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.test.ts](../../apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.test.ts)
- Verified by focused test runs:
  - `npm test -- --run src/lib/FieldCompanionDeniedReasonCatalog.test.ts`
  - `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter FieldCompanionDeniedReasonCatalogTests`

### 9. LoadArr discrepancy handling and putaway rows were lifted from scaffold to partial

Requirement:
- Workflow maturity should follow the concrete receiving, discrepancy, and putaway surfaces already present in code and tests.

Evidence:
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrIntegrationEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrIntegrationEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [tests/STLCompliance.SupplyArr.Auth.Tests/SupplyArrHandoffApiTests.cs](../../tests/STLCompliance.SupplyArr.Auth.Tests/SupplyArrHandoffApiTests.cs)
- [tests/STLCompliance.OpenApi.Tests/OpenApiParityTests.cs](../../tests/STLCompliance.OpenApi.Tests/OpenApiParityTests.cs)

### 10. Field Companion shell, profile readiness, and launch/session bootstrap were lifted from scaffold to partial

Requirement:
- The mobile shell should reflect the implemented PWA, launch, session, and workspace bootstrap behavior already present in the frontend.

Evidence:
- [apps/fieldcompanion-frontend/src/App.tsx](../../apps/fieldcompanion-frontend/src/App.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.ts)
- [apps/fieldcompanion-frontend/src/lib/productLaunch.test.ts](../../apps/fieldcompanion-frontend/src/lib/productLaunch.test.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts)

### 11. Field Companion home and product launcher surfaces now have verified dashboard coverage

Requirement:
- The mobile home surface should expose a real My work dashboard, inbox summary, and product workspace entry points instead of only implying them.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/pages/HomePage.test.tsx`

### 12. Field Companion scan and capture handoff now have verified page coverage

Requirement:
- The scan surface should expose task resolution and the capture handoff entry point instead of only a shared panel component.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/ScanPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ScanPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ScanPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ScanPage.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/pages/ScanPage.test.tsx`

### 13. Field Companion offline queue visibility now has verified page coverage

Requirement:
- The offline queue page should expose queued actions and sync guidance rather than leaving queue behavior implicit.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/pages/OfflineQueuePage.test.tsx`

### 14. Field Companion report shortcuts now have verified page coverage

Requirement:
- The report surface should route users into the correct owning workflows instead of only exposing a shared report concept.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/ReportPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ReportPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ReportPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ReportPage.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/pages/ReportPage.test.tsx`

### 15. Field Companion notification settings now have verified page coverage

Requirement:
- The notification surface should expose administrator-facing operational delivery settings and linked workflow context.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/pages/NotificationsPage.test.tsx`

### 16. Field Companion clock surface now has verified page coverage

Requirement:
- The clock page should expose the current state, worker context, and offline punch behavior rather than relying only on lower-level queue utilities.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/ClockPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts)
- Verified by focused test run:
  - `npm test -- --run src/pages/ClockPage.test.tsx`

### 17. Field Companion shared API/session client now has verified normalization coverage

Requirement:
- The frontend should use the platform session and shared API paths for handoff, renewal, profile, inbox, and launch normalization rather than owning identity or product launch logic locally.

Evidence:
- [apps/fieldcompanion-frontend/src/api/client.ts](../../apps/fieldcompanion-frontend/src/api/client.ts)
- [apps/fieldcompanion-frontend/src/api/client.test.ts](../../apps/fieldcompanion-frontend/src/api/client.test.ts)
- Verified by focused test coverage already present in the repository:
  - `redeemHandoff`
  - `renewFieldCompanionSession`
  - `getMe`
  - `getFieldInbox`

### 18. Field Companion evidence capture now has verified panel coverage

Requirement:
- The evidence workflow should expose upload controls, capture kind selection, and submission feedback for task-attached evidence.

Evidence:
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts)
- [apps/fieldcompanion-frontend/src/lib/evidenceCapture.test.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceCapture.test.ts)
- Verified by focused test coverage already present in the repository:
  - `FieldTaskEvidencePanel` upload-failure coverage

### 19. OrdArr dashboard, order detail, and handoff navigation now have verified app coverage

Requirement:
- The OrdArr frontend should expose the core navigation and primary order tasks through its routed workspace rather than leaving them as an unverified scaffold.

Evidence:
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- Verified by focused test coverage already present in the repository:
  - dashboard console
  - order detail console
  - legacy handoff redirect

### 20. OrdArr create-order workflow now has verified routed workspace coverage

Requirement:
- The create-order workflow should be reachable through the OrdArr routed workspace and should submit into the new order detail flow.

Evidence:
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/App.test.tsx`

### 21. OrdArr hold workflow now has verified routed workspace coverage

Requirement:
- The order hold workflow should be reachable from the routed workspace and support adding a hold from order detail.

Evidence:
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/App.test.tsx`

### 22. OrdArr order triage and execution decomposition now has verified routed workspace coverage

Requirement:
- The order triage flow should expose downstream handoffs from the routed workspace so accepted orders can be decomposed into execution demand.

Evidence:
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- Verified by focused test run:
  - `npm test -- --run src/App.test.tsx`

### 11. LoadArr reservations/allocation was lifted from scaffold to partial

Requirement:
- The reservation slice should reflect the concrete create/read/release surfaces already present in the LoadArr API and route catalog.

Evidence:
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrIntegrationEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrIntegrationEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)

### 12. LoadArr cycle count and variance approval now have an auth test-backed slice

Requirement:
- The cycle-count/variance flow should reflect the implemented create, approve, adjustment, and movement behavior that the API already performs.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)

### 13. LoadArr transfer create/complete now has an auth test-backed slice

Requirement:
- The transfer workflow should reflect the implemented create and completion behavior, including movement and balance updates against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Transfer_create_then_complete_updates_balances_and_records_movement`

### 14. LoadArr hold release now has an auth test-backed slice

Requirement:
- The hold workflow should reflect the implemented hold release behavior and balance decrement against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Hold_release_updates_hold_and_balance`

### 15. LoadArr unexplained inventory resolve and quarantine now have auth test-backed slices

Requirement:
- The unexplained-inventory workflow should reflect the implemented resolve and quarantine behavior against seeded records.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs)
- Verified by focused test runs:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Unexplained_inventory_resolve_creates_origin_event_and_movement`
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Unexplained_inventory_quarantine_moves_record_to_quarantine_location`

### 16. LoadArr receiving completion now has an auth test-backed slice

Requirement:
- The receiving workflow should reflect the implemented completion behavior, including origin event, movement, balance, and putaway task generation.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Receiving_complete_creates_origin_event_movement_balance_and_putaway_task`

### 17. LoadArr truck stock return now has an auth test-backed slice

Requirement:
- The truck-stock workflow should reflect the implemented return behavior and status recalculation against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Truck_stock_return_updates_quantity_and_records_movement`

### 18. LoadArr kit return now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented return behavior and status recalculation against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_return_updates_quantity_and_records_movement`

### 19. LoadArr kit track-location now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented track-location behavior and location update against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_track_location_updates_location_and_records_movement`

### 20. LoadArr kit assign now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented assign behavior and assignee update against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_assign_updates_assignee_and_records_movement`

### 21. LoadArr kit expire-components now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented component expiration behavior and quantity reset against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_expire_components_sets_status_and_zeroes_quantity`

### 22. LoadArr kit reserve now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented reserve behavior and quantity reduction against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_reserve_updates_quantity_and_records_movement`

### 23. LoadArr kit pick now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented pick behavior and quantity reduction against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_pick_updates_quantity_and_records_movement`

### 24. LoadArr truck stock count now has an auth test-backed slice

Requirement:
- The truck-stock workflow should reflect the implemented count behavior and quantity reconciliation against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Truck_stock_count_updates_quantity_and_records_movement`

### 25. LoadArr kit break now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented break behavior and quantity reduction against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_break_updates_quantity_and_records_movement`

### 26. LoadArr kit build now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented build behavior and quantity increase against seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_build_updates_quantity_and_records_movement`

### 27. LoadArr kit inspect now has an auth test-backed slice

Requirement:
- The kit workflow should reflect the implemented inspection behavior and preserve quantity on seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_inspect_updates_status_and_records_movement`

### 28. LoadArr count completion now has an auth test-backed slice

Requirement:
- The cycle count workflow should reflect the implemented completion behavior on seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Count_complete_updates_status_and_records_completion`

### 29. LoadArr replenishment workflow row now reflects auth-backed slices

Requirement:
- The replenishment workflow should reflect the implemented truck stock issue and kit replenishment behavior on seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Truck_stock_issue_below_minimum_requests_restock`
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_replenish_promotes_status_and_records_movement`

### 30. LoadArr return workflow rows now reflect auth-backed return slices

Requirement:
- The return workflow should reflect the implemented truck-stock and kit return behavior on seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrInventoryManagementEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Truck_stock_return_updates_quantity_and_records_movement`
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Kit_return_updates_quantity_and_records_movement`

### 31. LoadArr pick and shipping workflow rows now reflect auth-backed queue slices

Requirement:
- The outbound workflow should reflect the implemented pick and shipping queue behavior on seeded stock.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Picking_and_shipping_surfaces_expose_seeded_operational_queue_records`

### 32. LoadArr putaway workflow rows now reflect auth-backed queue slices

Requirement:
- The directed putaway workflow should reflect the seeded queue and detail surface.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Picking_and_shipping_surfaces_expose_seeded_operational_queue_records`

### 33. LoadArr reservation workflow rows now reflect auth-backed queue slices

Requirement:
- The reservation workflow should reflect the seeded reservation queue and detail surface.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Picking_and_shipping_surfaces_expose_seeded_operational_queue_records`

### 34. LoadArr expected-receipt workflow row now reflects auth-backed queue slices

Requirement:
- The expected-receipt workflow should reflect the seeded expected-receipt queue and detail surface.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- [docs/products/loadarr/WORKFLOWS.md](../products/loadarr/WORKFLOWS.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Picking_and_shipping_surfaces_expose_seeded_operational_queue_records`

### 35. LoadArr stock ledger/history feature row now reflects auth-backed history slices

Requirement:
- The stock ledger/history UI should reflect the seeded stock ledger, movement history, count history, and adjustment history surfaces.

Evidence:
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrIntegrationAuthTests.cs)
- [apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrRouteSurfaceEndpoints.cs)
- [docs/products/loadarr/FEATURESET.md](../products/loadarr/FEATURESET.md)
- Verified by focused test run:
  - `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter Stock_ledger_and_history_surfaces_expose_seeded_records`

### 36. OrdArr order-line routing now reflects execution-target handoff behavior

Requirement:
- Order-line creation on the OrdArr detail page should preserve routed execution targets and hand off warehouse-managed demand to LoadArr.

Evidence:
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- [docs/products/ordarr/FEATURESET.md](../products/ordarr/FEATURESET.md)
- [docs/products/ordarr/WORKFLOWS.md](../products/ordarr/WORKFLOWS.md)
- Verified by focused test run:
  - `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`

### 37. RecordArr capture workflow now files canonical records and queued scan output

Requirement:
- The RecordArr capture route should file a canonical record from an uploaded document and surface queued OCR/extraction output.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [apps/recordarr-frontend/src/auth/sessionStorage.ts](../../apps/recordarr-frontend/src/auth/sessionStorage.ts)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 38. RecordArr controlled-document workflow now creates authoring and review records

Requirement:
- The RecordArr controlled-document workspace should create a document, add a version, and request a review from the UI.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test run:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`

### 39. RecordArr controlled-document workflow now creates distribution and acknowledgement records

Requirement:
- The RecordArr controlled-document workspace should create a distribution and an acknowledgement from the UI and render the resulting entries.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 40. RecordArr record detail workflow now creates evidence mappings and shows coverage

Requirement:
- The RecordArr record detail workspace should create an evidence mapping and render the resulting coverage evaluation.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 41. RecordArr package workflow now assembles manifests and supports export

Requirement:
- The RecordArr package workspace should create a package, expose its manifest details, lock and archive the package, and support export.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 42. RecordArr legal-hold workflow now creates, activates, and releases holds

Requirement:
- The RecordArr legal-hold workspace should create a hold, activate it, and release it from the UI.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 43. RecordArr access controls and redaction workflows now create shares, grants, policies, logs, and redacted copies

Requirement:
- The RecordArr access workspace should create external shares, access grants, access policies, access logs, and redacted copies from the UI.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
  - `npm run build` in `apps/recordarr-frontend`

### 44. OrdArr completion packet advancement now has verified UI and API coverage

Requirement:
- The OrdArr order detail workspace should advance completion, invoice-ready, and bill-ready packets through a server-owned route and show the resulting packet state in the UI.

Evidence:
- [apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs](../../apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs)
- [apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs](../../apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs)
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- [apps/ordarr-frontend/src/api/client.ts](../../apps/ordarr-frontend/src/api/client.ts)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs)
- [docs/products/ordarr/FEATURESET.md](../products/ordarr/FEATURESET.md)
- [docs/products/ordarr/WORKFLOWS.md](../products/ordarr/WORKFLOWS.md)
- Verified by focused test runs:
  - `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`
  - `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --no-restore`

### 45. Field Companion offline conflict resolution now has verified UI, queue, and sync review coverage

Requirement:
- The mobile queue should preserve rejected offline intents, surface conflict explanations, and let users retry or discard them without silently losing the original attempt.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [apps/fieldcompanion-frontend/src/hooks/useOfflineQueue.ts](../../apps/fieldcompanion-frontend/src/hooks/useOfflineQueue.ts)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/lib/offlineQueue.test.ts src/components/OfflineQueuePanel.test.tsx src/pages/OfflineQueuePage.test.tsx` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 46. Field Companion device diagnostics and logout cleanup now cover browser capability checks and push subscription revocation

Requirement:
- The profile and notification surfaces should show actionable device readiness, and logout should best-effort revoke stale push registrations.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.ts)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/lib/deviceCapabilities.test.ts src/components/DeviceCapabilityPanel.test.tsx src/pages/ProfilePage.test.tsx src/pages/NotificationsPage.test.tsx src/auth/sessionStorage.test.ts` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 47. Field Companion signature capture now renders a drawn signature pad with session attribution and uploaded-image fallback

Requirement:
- The evidence capture surface should allow a deliberate signature review flow instead of reducing signature evidence to a plain file picker.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- Verified by focused test and build:
  - `npm test -- --run src/lib/evidenceCapture.test.ts src/components/FieldTaskEvidencePanel.test.tsx src/components/FieldInboxPanel.test.tsx src/pages/HomePage.test.tsx` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 48. Field Companion shared-device mode now blocks sign-out until queued work is reviewed or discarded

Requirement:
- The shared-device shell should surface inactivity warnings, block sign-out when queue work exists, keep the current tenant/user/session visible, and clear submission-state residue on logout.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts](../../apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.ts)
- [apps/fieldcompanion-frontend/src/lib/submissionState.ts](../../apps/fieldcompanion-frontend/src/lib/submissionState.ts)
- [apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.test.tsx](../../apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.test.tsx)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.test.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.test.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/lib/sharedDeviceProtection.test.tsx src/components/SharedDeviceProtectionOverlay.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx src/auth/sessionStorage.test.ts` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 49. Field Companion profile cleanup now exposes transparent device wipe and session revocation controls

Requirement:
- The profile surface should show the current worker/tenant, pending offline work, last sync, and a deliberate local cleanup path that removes STL app data without implying full device management.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.ts)
- [apps/fieldcompanion-frontend/src/lib/submissionState.ts](../../apps/fieldcompanion-frontend/src/lib/submissionState.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/pages/ProfilePage.test.tsx src/auth/sessionStorage.test.ts src/lib/sharedDeviceProtection.test.tsx` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 50. Field Companion release safety now blocks unsupported builds and surfaces staged rollout configuration

Requirement:
- The workspace and launch flow should honor minimum supported version checks, staged rollout configuration, kill-switch metadata, and clear update-required behavior.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/lib/releaseSafety.ts](../../apps/fieldcompanion-frontend/src/lib/releaseSafety.ts)
- [apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.tsx](../../apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/lib/releaseSafety.test.ts](../../apps/fieldcompanion-frontend/src/lib/releaseSafety.test.ts)
- [apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/lib/releaseSafety.test.ts src/components/FieldCompanionReleaseSafetyBanner.test.tsx src/pages/LaunchPage.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`

### 51. Field Companion offline queue now surfaces age, staleness, sync freshness, and conflict guidance explicitly

Requirement:
- Offline-first execution should be honest about what is queued, how old it is, what remains pending, and when local actions should be reviewed before syncing.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/offlineQueueFreshness.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueueFreshness.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [apps/fieldcompanion-frontend/src/hooks/useOfflineQueue.ts](../../apps/fieldcompanion-frontend/src/hooks/useOfflineQueue.ts)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.ts](../../apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueueFreshness.test.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueueFreshness.test.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.test.ts)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.test.ts](../../apps/fieldcompanion-frontend/src/lib/FieldCompanionDeniedReasonCatalog.test.ts)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)
- Verified by focused test and build:
  - `npm test -- --run src/components/OfflineQueuePanel.test.tsx src/lib/offlineQueue.test.ts src/lib/FieldCompanionDeniedReasonCatalog.test.ts` in `apps/fieldcompanion-frontend`
  - `npm run build` in `apps/fieldcompanion-frontend`
  - `npm run audit:theme` in `apps/fieldcompanion-frontend`

### 52. Field Companion session expiry now gates access tokens and surfaces refresh controls

Requirement:
- Secure mobile authentication/session should honor short-lived tokens, expose session visibility, and provide a truthful refresh control instead of letting expired access state linger in memory.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/sessionSafety.ts](../../apps/fieldcompanion-frontend/src/lib/sessionSafety.ts)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.ts)
- [apps/fieldcompanion-frontend/src/hooks/useFieldCompanionWorkspace.ts](../../apps/fieldcompanion-frontend/src/hooks/useFieldCompanionWorkspace.ts)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/lib/sessionSafety.test.ts](../../apps/fieldcompanion-frontend/src/lib/sessionSafety.test.ts)
- [apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts](../../apps/fieldcompanion-frontend/src/auth/sessionStorage.test.ts)
- [apps/fieldcompanion-frontend/src/hooks/useFieldCompanionWorkspace.test.tsx](../../apps/fieldcompanion-frontend/src/hooks/useFieldCompanionWorkspace.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test and build:
- `npm test -- --run src/lib/sessionSafety.test.ts src/auth/sessionStorage.test.ts src/hooks/useFieldCompanionWorkspace.test.tsx src/pages/ProfilePage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 53. Field Companion inbox now groups urgent work and surfaces freshness cues

Requirement:
- Task and inbox aggregation should prioritize blocked, overdue, due-soon, and stale work while preserving owning-product identity and showing freshness cues for field workers.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/fieldInbox.ts](../../apps/fieldcompanion-frontend/src/lib/fieldInbox.ts)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/fieldInbox.test.ts](../../apps/fieldcompanion-frontend/src/lib/fieldInbox.test.ts)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test and build:
- `npm test -- --run src/lib/fieldInbox.test.ts src/components/FieldInboxPanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 54. Field Companion scan context now normalizes payloads, suppresses duplicates, and surfaces scan source

Requirement:
- Context scanning should accept typed and camera inputs, normalize deep links into task keys, suppress rapid duplicate submissions, and show the user what kind of scan context was used without obscuring the resolved task.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/scanPayload.ts](../../apps/fieldcompanion-frontend/src/lib/scanPayload.ts)
- [apps/fieldcompanion-frontend/src/components/FieldScanPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldScanPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/api/client.test.ts](../../apps/fieldcompanion-frontend/src/api/client.test.ts)
- [apps/fieldcompanion-frontend/src/pages/ScanPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ScanPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ScanPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ScanPage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/lib/scanPayload.test.ts src/components/FieldScanPanel.test.tsx src/api/client.test.ts src/pages/ScanPage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 55. Field Companion device diagnostics now surface capability gaps and shared-device protection state

Requirement:
- Device capability checks should tell the user when browser, camera, storage, push, location, or permission support is missing, while shared-device mode should keep the current user/tenant visible and prevent unsafe session handoff or cleanup.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts](../../apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.test.tsx](../../apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.test.tsx)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.test.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/lib/deviceCapabilities.test.ts src/components/DeviceCapabilityPanel.test.tsx src/lib/sharedDeviceProtection.test.tsx src/components/SharedDeviceProtectionOverlay.test.tsx src/pages/ProfilePage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 56. Field Companion diagnostics can export a sanitized support summary for degraded operation

Requirement:
- Emergency and degraded-operation handling should generate a supportable diagnostic package that captures app, browser, platform, network, and capability state without leaking personal identifiers or unrelated device data.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/lib/deviceCapabilities.test.ts src/components/DeviceCapabilityPanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 57. Field Companion notification settings now send a real test dispatch through NexArr

Requirement:
- Notification settings should offer a test notification path that exercises the same dispatch pipeline as real operational events and surfaces the result alongside dispatch history.

Evidence:
- [apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs](../../apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs)
- [apps/fieldcompanion-frontend/src/api/client.ts](../../apps/fieldcompanion-frontend/src/api/client.ts)
- [apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx](../../apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx)
- [apps/fieldcompanion-frontend/src/api/client.test.ts](../../apps/fieldcompanion-frontend/src/api/client.test.ts)
- [apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.test.tsx)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/api/client.test.ts src/components/NotificationSettingsPanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter NexArrFieldCompanionNotificationTests` in the repository root

### 58. Field Companion now surfaces degraded-operation fallback guidance in the shell and launch flow

Requirement:
- Emergency or degraded operation should explain what failed, what remains saved, whether retry is safe, what the user can do next, and provide a supportable diagnostic summary without exposing personal identifiers.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/degradedOperation.ts](../../apps/fieldcompanion-frontend/src/lib/degradedOperation.ts)
- [apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx)
- [apps/fieldcompanion-frontend/src/lib/degradedOperation.test.ts](../../apps/fieldcompanion-frontend/src/lib/degradedOperation.test.ts)
- [apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/lib/degradedOperation.test.ts src/components/DegradedOperationPanel.test.tsx src/pages/HomePage.test.tsx src/pages/LaunchPage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 59. Field Companion push notifications now register browser subscriptions and deliver tenant-scoped operational events

Requirement:
- Push notifications should register browser subscriptions, route tenant and product context safely, dedupe repeated dispatches, expire stale tokens, and present a user-facing path for browser push permission and subscription sync.

Evidence:
- [apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs](../../apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs)
- [apps/nexarr-api/NexArr.Api/Entities/FieldCompanionPushSubscriptionEntities.cs](../../apps/nexarr-api/NexArr.Api/Entities/FieldCompanionPushSubscriptionEntities.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionPushSubscriptionService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionPushSubscriptionService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs)
- [apps/fieldcompanion-frontend/src/lib/pushNotifications.ts](../../apps/fieldcompanion-frontend/src/lib/pushNotifications.ts)
- [apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx](../../apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.test.tsx)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionWebPushTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionWebPushTests.cs)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test:
- `npm test -- --run src/components/NotificationSettingsPanel.test.tsx` in `apps/fieldcompanion-frontend`

### 60. Field Companion mobile task panels now support inspections, evidence capture, and DVIR-style form submission

Requirement:
- Field Companion should provide versioned task forms and checklists with conditional steps, required evidence, draft/resumable submission, signatures, and offline-friendly execution against the owning product APIs.

Evidence:
- [apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts)
- [apps/fieldcompanion-frontend/src/lib/fieldInspection.ts](../../apps/fieldcompanion-frontend/src/lib/fieldInspection.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test:
- `npm test -- --run src/components/FieldTaskDvirPanel.test.tsx src/components/FieldTaskEvidencePanel.test.tsx src/components/FieldTaskInspectionPanel.test.tsx` in `apps/fieldcompanion-frontend`

### 61. Field Companion clock punches now explain and preserve privacy-safe location capture

Requirement:
- Location-aware work should use coarse location only for declared operational purposes, clearly explain when the browser prompt is used, and still allow punches to record without GPS when location is unavailable or declined.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/ClockPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test:
- `npm test -- --run src/pages/ClockPage.test.tsx` in `apps/fieldcompanion-frontend`

### 62. Field Companion now optimizes photo attachments and shows upload savings before evidence submission

Requirement:
- Attachment uploads should compress or transcode supported photos, generate review thumbnails, preserve originals where needed, and explain the storage/network cost before submission.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/evidenceOptimization.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceOptimization.ts)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/evidenceOptimization.test.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceOptimization.test.ts)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test:
- `npm test -- --run src/lib/evidenceOptimization.test.ts src/components/FieldTaskEvidencePanel.test.tsx` in `apps/fieldcompanion-frontend`

### 63. Field Companion task surfaces now expose larger controls and live feedback for accessibility and ergonomics

Requirement:
- Field Companion should keep primary task surfaces readable and usable with larger hit targets, reduced motion, and screen-reader-friendly live feedback for field users on keyboard, touch, or assistive technologies.

Evidence:
- [apps/fieldcompanion-frontend/src/index.css](../../apps/fieldcompanion-frontend/src/index.css)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx](../../apps/fieldcompanion-frontend/src/components/NotificationSettingsPanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskDvirPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test:
- `npm test -- --run src/components/FieldTaskEvidencePanel.test.tsx src/components/FieldTaskInspectionPanel.test.tsx src/components/FieldTaskDvirPanel.test.tsx src/pages/ClockPage.test.tsx` in `apps/fieldcompanion-frontend`

### 64. Field Companion low-data mode now adapts photo optimization and surfaces connection-aware fallback guidance

Requirement:
- Low-end devices and poor-network conditions should reduce photo payload cost, expose connection readiness, and preserve readable fallback guidance for field evidence capture.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx)
- [apps/fieldcompanion-frontend/src/lib/evidenceOptimization.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceOptimization.ts)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts)
- [apps/fieldcompanion-frontend/src/lib/evidenceOptimization.test.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceOptimization.test.ts)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused test and build:
- `npm test -- --run src/lib/deviceCapabilities.test.ts src/lib/degradedOperation.test.ts src/components/DegradedOperationPanel.test.tsx src/lib/evidenceOptimization.test.ts src/components/DeviceCapabilityPanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 65. OrdArr completion packets now advance from the order detail workspace into completion and finance-ready closeout

Requirement:
- Order closeout should let authorized users advance completion, invoice-ready, and bill-ready packet states from the order workspace while preserving idempotency and retaining RecordArr references.

Evidence:
- [apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs](../../apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs)
- [apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs](../../apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs)
- [apps/ordarr-frontend/src/api/client.ts](../../apps/ordarr-frontend/src/api/client.ts)
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs)

Verified by focused test and build:
- `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --no-restore`
- `npm run build` in `apps/ordarr-frontend`
- `dotnet build apps/ordarr-api/OrdArr.Api/OrdArr.Api.csproj --no-restore`

### 66. RecordArr photo evidence capture now has workspace coverage alongside the document and records navigation surface

Requirement:
- Record detail workflows should support photo evidence capture with recorded provenance, and the DMS navigation should expose the main document, package, hold, access, redaction, and retention workspaces.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 67. RecordArr signature capture now has workspace coverage for attributable evidence on the record detail page

Requirement:
- Signature evidence should be capturable from the workspace with preserved provenance and a clear request/completion flow for owned records.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)

Verified by focused test and build:
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 68. RecordArr retention workspace now recalculates retention status and completes disposal reviews

Requirement:
- Retention and disposition workflows should surface policy state, legal-hold awareness, scheduler refresh, and review completion from the RecordArr workspace.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)

Verified by full test and build:
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 69. LoadArr unexplained inventory resolution now defaults the active queue record, reviewer context, and investigation history

Requirement:
- The unexplained-inventory workspace should preselect the active queue record, reviewer, and quarantine location so resolve and quarantine actions are immediately usable, and the count and unexplained panels should surface the recent movement history that informs variance and custody investigation.

Evidence:
- [apps/loadarr-frontend/src/App.tsx](../../apps/loadarr-frontend/src/App.tsx)
- [apps/loadarr-frontend/src/App.test.tsx](../../apps/loadarr-frontend/src/App.test.tsx)
- [apps/loadarr-frontend/vite.config.ts](../../apps/loadarr-frontend/vite.config.ts)
- [apps/loadarr-frontend/package.json](../../apps/loadarr-frontend/package.json)
- [apps/loadarr-frontend/package-lock.json](../../apps/loadarr-frontend/package-lock.json)
- [apps/loadarr-frontend/src/App.tsx](../../apps/loadarr-frontend/src/App.tsx) now derives `Count investigation` and `Custody timeline` summaries from count, adjustment, receipt, transfer, and inventory ledger rows.
- [apps/loadarr-frontend/src/App.test.tsx](../../apps/loadarr-frontend/src/App.test.tsx) covers both unexplained resolution defaults and cycle-count variance investigation history.

Verified by focused frontend test and build:
- `npm test -- --run src/App.test.tsx` in `apps/loadarr-frontend`
- `npm run build` in `apps/loadarr-frontend`
- `npm test -- --run src/components/FieldCompanionReleaseSafetyBanner.test.tsx src/components/SharedDeviceProtectionOverlay.test.tsx src/components/DeviceCapabilityPanel.test.tsx src/components/DegradedOperationPanel.test.tsx src/pages/LaunchPage.test.tsx src/pages/HomePage.test.tsx src/pages/NotificationsPage.test.tsx src/pages/ProfilePage.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/components/ProductSwitcher.test.tsx src/pages/ProductSurfacePage.test.tsx src/pages/LaunchPadPage.test.tsx src/pages/HomePage.test.tsx src/lib/launchFailure.test.ts` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/loadarr-frontend`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --no-restore`
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore`
- `dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj --no-restore`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~NexArrFieldCompanionNotificationTests.Notification_settings_test_endpoint_dispatches_notification"`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~FieldCompanionDeniedReasonCatalogTests"`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/assurarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/reportarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/maintainarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/supplyarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/trainarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/compliancecore-frontend`
- `npm run build` in `apps/assurarr-frontend`
- `npm run build` in `apps/reportarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`
- `npm run build` in `apps/trainarr-frontend`
- `npm run build` in `apps/compliancecore-frontend`
- `npm run build` in `apps/routarr-frontend`

### 70. RecordArr capture workflow now defaults taxonomy from active vocabulary terms

Requirement:
- The RecordArr capture form should prefill document class, document type, and document subtype from the first active vocabulary values when the request is blank, while still letting reviewers override those defaults before filing.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [docs/products/recordarr/FEATURESET.md](../products/recordarr/FEATURESET.md)
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)

Verified by focused frontend test and build:
- `npm test -- --run src/App.test.tsx -t "files a captured record and queues OCR from the triage workflow"` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 71. Field Companion now surfaces release safety, device capability, shared-device, and degraded-operation guidance

Requirement:
- Field Companion should explain when a build is paused or blocked, expose device capability and connection diagnostics, surface shared-device cleanup state, and provide explicit degraded-operation guidance instead of silent failure.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/releaseSafety.ts](../../apps/fieldcompanion-frontend/src/lib/releaseSafety.ts)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.tsx](../../apps/fieldcompanion-frontend/src/components/FieldCompanionReleaseSafetyBanner.tsx)
- [apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx)
- [apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)

Verified by frontend test suite and build:
- `npm test` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 72. OrdArr completion packets now advance approved closeout and finance states

Requirement:
- OrdArr should let approved orders advance completion, invoice-ready, and bill-ready packet state from the order detail workspace and persist the update through the authorization-aware API.

Evidence:
- [apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs](../../apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs)
- [apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs](../../apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs)
- [apps/ordarr-frontend/src/App.tsx](../../apps/ordarr-frontend/src/App.tsx)
- [apps/ordarr-frontend/src/App.test.tsx](../../apps/ordarr-frontend/src/App.test.tsx)
- [apps/ordarr-frontend/src/api/client.ts](../../apps/ordarr-frontend/src/api/client.ts)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs)
- [docs/products/ordarr/FEATURESET.md](../products/ordarr/FEATURESET.md)
- [docs/products/ordarr/WORKFLOWS.md](../products/ordarr/WORKFLOWS.md)

Verified by frontend and API tests plus builds:
- `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`
- `npm run build` in `apps/ordarr-frontend`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --no-restore`
- `dotnet build apps/ordarr-api/OrdArr.Api/OrdArr.Api.csproj --no-restore`

### 73. NexArr Field Companion notification settings now support test dispatch

Requirement:
- NexArr should expose a notification settings test endpoint that enqueues and dispatches a Field Companion test notification with the correct payload and webhook metadata while keeping the existing notification rules and denied-reason copy aligned.

Evidence:
- [apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs](../../apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationRules.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationRules.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/FieldCompanionDeniedReasonCatalogTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/FieldCompanionDeniedReasonCatalogTests.cs)
- [docs/products/nexarr/FEATURESET.md](../products/nexarr/FEATURESET.md)

Verified by targeted API tests and build:
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter "FullyQualifiedName~NexArrFieldCompanionNotificationTests.Notification_settings_test_endpoint_dispatches_notification"` in `apps/nexarr-api`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --no-build --filter "FullyQualifiedName~FieldCompanionDeniedReasonCatalogTests"` in `apps/nexarr-api`
- `dotnet build apps/nexarr-api/NexArr.Api/NexArr.Api.csproj --no-restore`

### 74. Suite frontend now builds against canonical NexArr payload types and typed launch-context resolution

Requirement:
- The suite frontend should compile cleanly against the canonical NexArr product manifest and audit-packet response types, and the launch-context test should resolve with an explicit launch-context type instead of a fragile inferred mock type.

Evidence:
- [apps/suite-frontend/src/api/nexarrClient.ts](../../apps/suite-frontend/src/api/nexarrClient.ts)
- [apps/suite-frontend/src/pages/ProductSurfacePage.test.tsx](../../apps/suite-frontend/src/pages/ProductSurfacePage.test.tsx)
- [docs/user/how-to/platform/how-to-switch-products.md](../user/how-to/platform/how-to-switch-products.md)
- [docs/user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md](../user/how-to/platform/how-to-understand-product-launch-availability-and-permissions.md)
- [packages/shared-ui/src/ProductSwitcher.tsx](../../packages/shared-ui/src/ProductSwitcher.tsx)
- [packages/shared-ui/src/ProductAppShell.tsx](../../packages/shared-ui/src/ProductAppShell.tsx)

Verified by frontend test and build:
- `npm test -- --run src/components/ProductSwitcher.test.tsx` in `apps/suite-frontend`
- `npm test -- --run src/pages/ProductSurfacePage.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`

### 75. AssurArr, ReportArr, and RoutArr now persist platform-admin launch metadata in stored sessions

Requirement:
- AssurArr, ReportArr, and RoutArr should preserve platform-admin launch metadata in their stored session payloads so the shared shell can continue to expose Compliance Core and other admin-gated launch behavior after reload.

Evidence:
- [apps/assurarr-frontend/src/auth/sessionStorage.ts](../../apps/assurarr-frontend/src/auth/sessionStorage.ts)
- [apps/reportarr-frontend/src/auth/sessionStorage.ts](../../apps/reportarr-frontend/src/auth/sessionStorage.ts)
- [apps/routarr-frontend/src/auth/sessionStorage.ts](../../apps/routarr-frontend/src/auth/sessionStorage.ts)
- [apps/assurarr-frontend/src/App.tsx](../../apps/assurarr-frontend/src/App.tsx)
- [apps/reportarr-frontend/src/App.tsx](../../apps/reportarr-frontend/src/App.tsx)
- [apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/assurarr-frontend/src/auth/sessionStorage.test.ts](../../apps/assurarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/reportarr-frontend/src/auth/sessionStorage.test.ts](../../apps/reportarr-frontend/src/auth/sessionStorage.test.ts)

Verified by frontend builds:
- `npm run build` in `apps/assurarr-frontend`
- `npm run build` in `apps/reportarr-frontend`
- `npm run build` in `apps/routarr-frontend`

### 76. MaintainArr, SupplyArr, TrainArr, and ComplianceCore now persist platform-admin launch metadata in stored sessions

Requirement:
- MaintainArr, SupplyArr, TrainArr, and ComplianceCore should preserve platform-admin launch metadata in their stored session payloads so the shared shell can continue to expose Compliance Core and other admin-gated launch behavior after reload.

Evidence:
- [apps/maintainarr-frontend/src/auth/sessionStorage.ts](../../apps/maintainarr-frontend/src/auth/sessionStorage.ts)
- [apps/supplyarr-frontend/src/auth/sessionStorage.ts](../../apps/supplyarr-frontend/src/auth/sessionStorage.ts)
- [apps/trainarr-frontend/src/auth/sessionStorage.ts](../../apps/trainarr-frontend/src/auth/sessionStorage.ts)
- [apps/compliancecore-frontend/src/auth/sessionStorage.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.ts)
- [apps/maintainarr-frontend/src/auth/sessionStorage.test.ts](../../apps/maintainarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/supplyarr-frontend/src/auth/sessionStorage.test.ts](../../apps/supplyarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/trainarr-frontend/src/auth/sessionStorage.test.ts](../../apps/trainarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)
- [apps/compliancecore-frontend/src/auth/sessionStorage.test.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.test.ts)
- [apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx)

Verified by frontend tests and builds:
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/maintainarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/supplyarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/trainarr-frontend`
- `npm test -- --run src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/trainarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/compliancecore-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`
- `npm run build` in `apps/trainarr-frontend`
- `npm run build` in `apps/compliancecore-frontend`

### 77. LoadArr and StaffArr now persist platform-admin launch metadata in stored sessions

Requirement:
- LoadArr and StaffArr should preserve platform-admin launch metadata in their stored session payloads so reloads keep admin-gated launch behavior available in the app shell.

Evidence:
- [apps/loadarr-frontend/src/auth/sessionStorage.ts](../../apps/loadarr-frontend/src/auth/sessionStorage.ts)
- [apps/loadarr-frontend/src/App.tsx](../../apps/loadarr-frontend/src/App.tsx)
- [apps/loadarr-frontend/src/LaunchPage.tsx](../../apps/loadarr-frontend/src/LaunchPage.tsx)
- [apps/loadarr-frontend/src/auth/sessionStorage.test.ts](../../apps/loadarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/loadarr-frontend/src/App.test.tsx](../../apps/loadarr-frontend/src/App.test.tsx)
- [apps/staffarr-frontend/src/auth/sessionStorage.ts](../../apps/staffarr-frontend/src/auth/sessionStorage.ts)
- [apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/staffarr-frontend/src/pages/LaunchPage.tsx](../../apps/staffarr-frontend/src/pages/LaunchPage.tsx)
- [apps/staffarr-frontend/src/auth/sessionStorage.test.ts](../../apps/staffarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by frontend tests and builds:
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/loadarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/loadarr-frontend`
- `npm run build` in `apps/loadarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/staffarr-frontend`
- `npm test -- --run src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`

### 78. StaffArr delegated account-access panel now has direct workflow coverage

Requirement:
- StaffArr’s person-profile surface should keep delegated NexArr-backed account provisioning and read-only account-access state visible and testable from the person editor.

Evidence:
- [apps/staffarr-frontend/src/components/PersonAccountAccessPanel.tsx](../../apps/staffarr-frontend/src/components/PersonAccountAccessPanel.tsx)
- [apps/staffarr-frontend/src/components/PersonProfileEditorPanel.tsx](../../apps/staffarr-frontend/src/components/PersonProfileEditorPanel.tsx)
- [apps/staffarr-frontend/src/components/PersonAccountAccessPanel.test.tsx](../../apps/staffarr-frontend/src/components/PersonAccountAccessPanel.test.tsx)
- [docs/products/staffarr/FEATURESET.md](../products/staffarr/FEATURESET.md)

Verified by targeted test and build:
- `npm test -- --run src/components/PersonAccountAccessPanel.test.tsx` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`

### 79. StaffArr delegated account-access workflow now covers provision, disable, and re-enable actions

Requirement:
- StaffArr’s delegated account-access panel should keep its NexArr-backed provision, disable, and re-enable actions visible, confirmable, and regression-tested from the person profile surface.

Evidence:
- [apps/staffarr-frontend/src/components/PersonAccountAccessPanel.tsx](../../apps/staffarr-frontend/src/components/PersonAccountAccessPanel.tsx)
- [apps/staffarr-frontend/src/components/PersonAccountAccessPanel.test.tsx](../../apps/staffarr-frontend/src/components/PersonAccountAccessPanel.test.tsx)
- [packages/shared-ui/src/ConfirmDialog.tsx](../../packages/shared-ui/src/ConfirmDialog.tsx)

Verified by targeted test and build:
- `npm test -- --run src/components/PersonAccountAccessPanel.test.tsx` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`

### 80. StaffArr self-service update requests now surface field policy guidance

Requirement:
- StaffArr’s self-service update-request surface should show which fields are directly editable after approval, review-required, or restricted, matching ST-WF-013 and the backend request rules.

Evidence:
- [apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.tsx](../../apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.tsx)
- [apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.test.tsx](../../apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.test.tsx)
- [apps/staffarr-api/StaffArr.Api/Services/PersonnelUpdateRequestRules.cs](../../apps/staffarr-api/StaffArr.Api/Services/PersonnelUpdateRequestRules.cs)
- [docs/products/staffarr/WORKFLOWS.md](../products/staffarr/WORKFLOWS.md)
- [docs/products/staffarr/FEATURESET.md](../products/staffarr/FEATURESET.md)

Verified by targeted test and build:
- `npm test -- --run src/components/MeSelfServicePortalPanel.test.tsx` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`

### 81. Compliance Core workspace bootstrap now preserves platform-admin session state with regression coverage

Requirement:
- Compliance Core should preserve the stored platform-admin flag in the workspace shell session context and keep the admin surface bootstrapped through the shared product frame.

Evidence:
- [apps/compliancecore-frontend/src/auth/sessionStorage.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.ts)
- [apps/compliancecore-frontend/src/auth/sessionStorage.test.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.test.ts)
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`

### 141. MaintainArr downtime reason edits now preserve active events, append audit notes, and restore availability explicitly

Requirement:
- Authorized users should be able to revise the reason on an active downtime event, append an audit note, and then explicitly restore availability when the asset returns to service.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetDowntimeEndpoints.cs](../../apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetDowntimeEndpoints.cs)
- [apps/maintainarr-api/MaintainArr.Api/Services/AssetDowntimeService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/AssetDowntimeService.cs)
- [apps/maintainarr-frontend/src/components/AssetDowntimePanel.tsx](../../apps/maintainarr-frontend/src/components/AssetDowntimePanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetDowntimePanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetDowntimePanel.test.tsx)
- [tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs](../../tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs)

Verified by focused frontend test, frontend build, auth test, and OpenAPI parity update:
- `npm test -- --run src/components/AssetDowntimePanel.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --no-restore --filter FullyQualifiedName~MaintainArrAssetDowntimeTests` in the repository root
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --no-restore --filter FullyQualifiedName~OpenApiParityTests` in the repository root

### 142. MaintainArr asset reservation and motor-pool workflow now has a partial durable slice

Requirement:
- Asset reservation and motor-pool readiness should expose request, approval, reservation, checkout/return, conflicts, and readiness-driven handoff without duplicating owning-product truth.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [docs/products/maintainarr/FEATURESET.md](../products/maintainarr/FEATURESET.md)
- [docs/products/maintainarr/maintainarr_06_reservations_readiness_voice_inspection_and_parts_intelligence.md](../products/maintainarr/maintainarr_06_reservations_readiness_voice_inspection_and_parts_intelligence.md)
- [apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs](../../apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs)
- [apps/maintainarr-api/MaintainArr.Api/Contracts/AssetReservationContracts.cs](../../apps/maintainarr-api/MaintainArr.Api/Contracts/AssetReservationContracts.cs)
- [apps/maintainarr-api/MaintainArr.Api/Data/MaintainArrDbContext.cs](../../apps/maintainarr-api/MaintainArr.Api/Data/MaintainArrDbContext.cs)
- [apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetReservationEndpoints.cs](../../apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetReservationEndpoints.cs)
- [apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs)
- [apps/maintainarr-api/MaintainArr.Api/Services/MaintainArrAuthorizationService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/MaintainArrAuthorizationService.cs)
- [apps/maintainarr-api/MaintainArr.Api/Data/MaintainArrDesignTimeDbContextFactory.cs](../../apps/maintainarr-api/MaintainArr.Api/Data/MaintainArrDesignTimeDbContextFactory.cs)
- [apps/maintainarr-api/MaintainArr.Api/Program.cs](../../apps/maintainarr-api/MaintainArr.Api/Program.cs)
- [apps/maintainarr-api/MaintainArr.Api/MaintainArrServiceRegistration.cs](../../apps/maintainarr-api/MaintainArr.Api/MaintainArrServiceRegistration.cs)
- [apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.cs](../../apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.cs)
- [apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.Designer.cs](../../apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.Designer.cs)
- [apps/maintainarr-frontend/src/api/client.ts](../../apps/maintainarr-frontend/src/api/client.ts)
- [apps/maintainarr-frontend/src/api/types.ts](../../apps/maintainarr-frontend/src/api/types.ts)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)
- [apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.tsx](../../apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.tsx)
- [apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.test.tsx](../../apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.test.tsx)
- [tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs](../../tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs)
- [tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json](../../tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json)

Verified by focused frontend, auth, and OpenAPI parity runs:
- `npm test -- AssetReservationPanel.test.tsx AssetProfilePage.test.tsx` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "Reservation_create_get_and_conflict_block_round_trip"` in the repository root
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "MaintainArrOpenApiParityTests"` in the repository root
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "MaintainArrOpenApiParityTests"` in the repository root

### 149. MaintainArr reservation returns now persist charge notes in context

Requirement:
- Return, inspection, and close actions should capture a separate charge note so the reservation record preserves billing or cleanup context alongside the damage notes.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-api/MaintainArr.Api/Contracts/AssetReservationContracts.cs](../../apps/maintainarr-api/MaintainArr.Api/Contracts/AssetReservationContracts.cs)
- [apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs](../../apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs)
- [apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)
- [tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs](../../tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs)
- [tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json](../../tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json)

Verified by targeted frontend, auth, and OpenAPI parity tests:
- `npm test -- src/components/AssetReservationPanel.test.tsx src/pages/parts-kits/PartsKitCreatePage.test.tsx src/pages/defects/DefectCreatePage.test.tsx src/pages/work-orders/WorkOrderCreatePage.test.tsx` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter "Reservation_"` in the repository root
- `OPENAPI_UPDATE_SNAPSHOTS=1 dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter "MaintainArrOpenApiParityTests"` in the repository root

### 148. MaintainArr reservation details now show capacity and handoff notes

Requirement:
- Reservation details should surface capacity and handoff notes captured during reservation creation so the record carries the operator-facing handoff context.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/components/AssetReservationPanel.test.tsx` in `apps/maintainarr-frontend`

### 147. MaintainArr reservation details now summarize usage meter delta in context

Requirement:
- Reservation details should surface the returned-vs-checked-out meter delta so coordinators can see usage context without recalculating it from the timeline.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/components/AssetReservationPanel.test.tsx` in `apps/maintainarr-frontend`

### 146. MaintainArr reservation details now surface post-use damage and inspection context

Requirement:
- Reservation details should expose post-use damage, inspection, and no-show/cancel context so coordinators can review return quality without digging through the timeline.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/components/AssetReservationPanel.test.tsx` in `apps/maintainarr-frontend`

### 145. MaintainArr parts-kit items now quick-create SupplyArr parts in context

Requirement:
- Parts-kit authoring should let users quick-create a missing SupplyArr part reference in context and save it into the kit definition without abandoning the builder.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [docs/products/maintainarr/FEATURESET.md](../products/maintainarr/FEATURESET.md)
- [apps/maintainarr-frontend/src/pages/parts-kits/PartsKitCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/parts-kits/PartsKitCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/parts-kits/PartsKitCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/parts-kits/PartsKitCreatePage.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/pages/parts-kits/PartsKitCreatePage.test.tsx` in `apps/maintainarr-frontend`

### 144. MaintainArr defect intake now quick-creates missing assets in context

Requirement:
- Work request and defect intake should let users quick-create a missing asset reference in context and resume the intake flow without abandoning the request.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [docs/products/maintainarr/FEATURESET.md](../products/maintainarr/FEATURESET.md)
- [apps/maintainarr-frontend/src/pages/defects/DefectCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/defects/DefectCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/defects/DefectCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/defects/DefectCreatePage.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/pages/defects/DefectCreatePage.test.tsx` in `apps/maintainarr-frontend`

### 143. MaintainArr asset reservation now suggests ready alternatives for blocked assets

Requirement:
- Asset reservation should surface explainable conflicts and suitable alternatives when the current asset is blocked or conflicting.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)

Verified by targeted frontend test:
- `npm test -- src/components/AssetReservationPanel.test.tsx` in `apps/maintainarr-frontend`

### 140. MaintainArr recall dashboard now summarizes campaign coverage and residual risk

Requirement:
- Recall monitoring should surface tracked campaign coverage, open and verified-open concentration, and the highest-risk campaigns without relying on raw provider payloads.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/workspace/sections/RecallsSection.tsx](../../apps/maintainarr-frontend/src/workspace/sections/RecallsSection.tsx)
- [apps/maintainarr-frontend/src/workspace/sections/RecallsSection.test.tsx](../../apps/maintainarr-frontend/src/workspace/sections/RecallsSection.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/workspace/sections/RecallsSection.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 139. MaintainArr vendor coordination now summarizes quote, approval, scheduling, and closeout

Requirement:
- Vendor/contractor maintenance work should surface the external-work handoff state, progress, and next step without confusing approval, scheduling, completion, and invoice context.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.tsx)
- [apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.test.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/components/WorkOrderVendorWorkPanel.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 138. MaintainArr parts demand now explains shortage, procurement, and receiving state

Requirement:
- Parts demand should make shortage, procurement progress, receiving progress, and next steps visible without duplicating LoadArr or SupplyArr truth.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/WorkOrderPartsDemandPanel.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderPartsDemandPanel.tsx)
- [apps/maintainarr-frontend/src/components/WorkOrderPartsDemandPanel.test.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderPartsDemandPanel.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/components/WorkOrderPartsDemandPanel.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 137. MaintainArr work-order create now surfaces emergency containment and rapid-dispatch guidance

Requirement:
- Emergency breakdown work requests should visibly call out containment, rapid dispatch, minimum controls, and recovery decisions before final action.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/pages/work-orders/WorkOrderCreatePage.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 136. MaintainArr work-order review now explains request source, route, urgency, and duplicate risk

Requirement:
- The work-order create review step should surface request triage context so operators can see source, route, urgency, readiness, and duplicate risk before choosing the final action.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/pages/work-orders/WorkOrderCreatePage.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 135. MaintainArr asset readiness now explains AssurArr quality holds in blocker detail

Requirement:
- MaintainArr should surface active quality-hold blockers on the asset readiness detail surface so return-to-service stays blocked until AssurArr releases the matching hold.

Evidence:
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [apps/maintainarr-frontend/src/components/AssetReadinessDetailPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReadinessDetailPanel.tsx)
- [apps/maintainarr-frontend/src/components/AssetReadinessDetailPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReadinessDetailPanel.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/components/AssetReadinessDetailPanel.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 134. AssurArr quality review now surfaces evidence-package readiness and missing refs

Requirement:
- The quality review detail page should explain whether the RecordArr evidence package is complete, partial, or still awaiting submissions so reviewers can see the blocker before approval or closure.

Evidence:
- [docs/products/assurarr/WORKFLOWS.md](../products/assurarr/WORKFLOWS.md)
- [apps/assurarr-frontend/src/App.tsx](../../apps/assurarr-frontend/src/App.tsx)
- [apps/assurarr-frontend/src/App.test.tsx](../../apps/assurarr-frontend/src/App.test.tsx)

Verified by focused frontend test:
- `npm test -- --run src/App.test.tsx` in `apps/assurarr-frontend`

### 133. RecordArr evidence mapping now surfaces coverage review state on the record detail page

Requirement:
- The record detail workspace should show whether mappings and coverage evaluations are covered, partial, missing, stale, or conflicting so evidence review is visible instead of hiding behind list rows.

Evidence:
- [docs/products/recordarr/WORKFLOWS.md](../products/recordarr/WORKFLOWS.md)
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 132. CustomArr commercial handoff review now surfaces proposal and agreement readiness

Requirement:
- The commercial workspace should explain whether proposal review, agreement approval, or OrdArr handoff is the next step instead of leaving proposals and agreements as flat record lists.

Evidence:
- [docs/products/customarr/WORKFLOWS.md](../products/customarr/WORKFLOWS.md)
- [apps/customarr-frontend/src/App.tsx](../../apps/customarr-frontend/src/App.tsx)
- [apps/customarr-frontend/src/App.test.tsx](../../apps/customarr-frontend/src/App.test.tsx)

Verified by focused frontend test and build:
- `npm test -- --run src/App.test.tsx` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/ordarr-frontend`
- `npm run build` in `apps/ordarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/reportarr-frontend`
- `npm run build` in `apps/reportarr-frontend`

### 82. MaintainArr workspace bootstrap now preserves platform-admin session state with regression coverage

Requirement:
- MaintainArr should preserve the stored platform-admin flag in the workspace shell session context and keep the admin-capable product frame bootstrapped consistently.

Evidence:
- [apps/maintainarr-frontend/src/auth/sessionStorage.ts](../../apps/maintainarr-frontend/src/auth/sessionStorage.ts)
- [apps/maintainarr-frontend/src/auth/sessionStorage.test.ts](../../apps/maintainarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 83. SupplyArr workspace bootstrap now preserves platform-admin session state with regression coverage

Requirement:
- SupplyArr should preserve the stored platform-admin flag in the workspace shell session context and keep the admin-capable product frame bootstrapped consistently.

Evidence:
- [apps/supplyarr-frontend/src/auth/sessionStorage.ts](../../apps/supplyarr-frontend/src/auth/sessionStorage.ts)
- [apps/supplyarr-frontend/src/auth/sessionStorage.test.ts](../../apps/supplyarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`

### 84. RoutArr workspace bootstrap now preserves platform-admin session state with regression coverage

Requirement:
- RoutArr should preserve the stored platform-admin flag in the workspace shell session context and keep the dispatch workspace bootstrapped consistently.

Evidence:
- [apps/routarr-frontend/src/auth/sessionStorage.ts](../../apps/routarr-frontend/src/auth/sessionStorage.ts)
- [apps/routarr-frontend/src/auth/sessionStorage.test.ts](../../apps/routarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`

### 85. OrdArr session storage now preserves platform-admin handoff state with regression coverage

Requirement:
- OrdArr should preserve the stored platform-admin flag and launchable products in its durable handoff session payload.

Evidence:
- [apps/ordarr-frontend/src/auth/sessionStorage.ts](../../apps/ordarr-frontend/src/auth/sessionStorage.ts)
- [apps/ordarr-frontend/src/auth/sessionStorage.test.ts](../../apps/ordarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/ordarr-frontend/src/api/client.ts](../../apps/ordarr-frontend/src/api/client.ts)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/ordarr-frontend`
- `npm run build` in `apps/ordarr-frontend`

### 86. LedgArr and RecordArr session storage now preserve platform-admin handoff state with regression coverage

Requirement:
- LedgArr and RecordArr should preserve the stored platform-admin flag and launchable products in their durable handoff session payloads.

Evidence:
- [apps/ledgarr-frontend/src/auth/sessionStorage.ts](../../apps/ledgarr-frontend/src/auth/sessionStorage.ts)
- [apps/ledgarr-frontend/src/auth/sessionStorage.test.ts](../../apps/ledgarr-frontend/src/auth/sessionStorage.test.ts)
- [apps/recordarr-frontend/src/auth/sessionStorage.ts](../../apps/recordarr-frontend/src/auth/sessionStorage.ts)
- [apps/recordarr-frontend/src/auth/sessionStorage.test.ts](../../apps/recordarr-frontend/src/auth/sessionStorage.test.ts)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`

### 87. CustomArr app bootstrap now preserves platform-admin workspace state and handoff routing

Requirement:
- CustomArr should preserve the stored platform-admin flag in the workspace shell session context and continue to redirect handoff routes into launch while bootstrapping the dashboard from live API data.

Evidence:
- [apps/customarr-frontend/src/App.tsx](../../apps/customarr-frontend/src/App.tsx)
- [apps/customarr-frontend/src/App.test.tsx](../../apps/customarr-frontend/src/App.test.tsx)
- [apps/customarr-frontend/src/auth/sessionStorage.ts](../../apps/customarr-frontend/src/auth/sessionStorage.ts)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`

### 88. ReportArr app bootstrap now preserves platform-admin workspace state and handoff routing

Requirement:
- ReportArr should preserve the stored platform-admin flag in the workspace shell session context and continue to route launch views while bootstrapping live summary data.

Evidence:
- [apps/reportarr-frontend/src/App.tsx](../../apps/reportarr-frontend/src/App.tsx)
- [apps/reportarr-frontend/src/App.test.tsx](../../apps/reportarr-frontend/src/App.test.tsx)
- [apps/reportarr-frontend/src/auth/sessionStorage.ts](../../apps/reportarr-frontend/src/auth/sessionStorage.ts)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/reportarr-frontend`
- `npm run build` in `apps/reportarr-frontend`

### 89. AssurArr app bootstrap now preserves platform-admin workspace state and handoff routing

Requirement:
- AssurArr should preserve the stored platform-admin flag in the workspace shell session context and continue to route launch views while bootstrapping live dashboard data.

Evidence:
- [apps/assurarr-frontend/src/App.tsx](../../apps/assurarr-frontend/src/App.tsx)
- [apps/assurarr-frontend/src/App.test.tsx](../../apps/assurarr-frontend/src/App.test.tsx)
- [apps/assurarr-frontend/src/auth/sessionStorage.ts](../../apps/assurarr-frontend/src/auth/sessionStorage.ts)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/assurarr-frontend`
- `npm run build` in `apps/assurarr-frontend`

### 90. AssurArr session storage now preserves platform-admin handoff state with regression coverage

Requirement:
- AssurArr should round-trip the stored handoff session, including the platform-admin flag, and remove it cleanly on sign-out or clear.

Evidence:
- [apps/assurarr-frontend/src/auth/sessionStorage.ts](../../apps/assurarr-frontend/src/auth/sessionStorage.ts)
- [apps/assurarr-frontend/src/auth/sessionStorage.test.ts](../../apps/assurarr-frontend/src/auth/sessionStorage.test.ts)

Verified by targeted test and build:
- `npm test -- --run src/auth/sessionStorage.test.ts` in `apps/assurarr-frontend`
- `npm run build` in `apps/assurarr-frontend`

### 91. Field Companion app routes now preserve the mobile launch and home handoff split

Requirement:
- Field Companion should route the workspace root to the home surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/fieldcompanion-frontend/src/App.tsx](../../apps/fieldcompanion-frontend/src/App.tsx)
- [apps/fieldcompanion-frontend/src/App.test.tsx](../../apps/fieldcompanion-frontend/src/App.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx](../../apps/fieldcompanion-frontend/src/pages/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 92. TrainArr app routes now preserve the my-training default and launch split

Requirement:
- TrainArr should redirect the workspace root to the my-training surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/trainarr-frontend/src/App.tsx](../../apps/trainarr-frontend/src/App.tsx)
- [apps/trainarr-frontend/src/App.test.tsx](../../apps/trainarr-frontend/src/App.test.tsx)
- [apps/trainarr-frontend/src/pages/my-training/MyTrainingPage.tsx](../../apps/trainarr-frontend/src/pages/my-training/MyTrainingPage.tsx)
- [apps/trainarr-frontend/src/pages/LaunchPage.tsx](../../apps/trainarr-frontend/src/pages/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/trainarr-frontend`
- `npm run build` in `apps/trainarr-frontend`

### 93. SupplyArr app routes now preserve the dashboard default and launch split

Requirement:
- SupplyArr should redirect the workspace root to the dashboard surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/supplyarr-frontend/src/App.tsx](../../apps/supplyarr-frontend/src/App.tsx)
- [apps/supplyarr-frontend/src/App.test.tsx](../../apps/supplyarr-frontend/src/App.test.tsx)
- [apps/supplyarr-frontend/src/pages/dashboard/DashboardPage.tsx](../../apps/supplyarr-frontend/src/pages/dashboard/DashboardPage.tsx)
- [apps/supplyarr-frontend/src/pages/LaunchPage.tsx](../../apps/supplyarr-frontend/src/pages/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`

### 94. RoutArr app routes now preserve the dashboard default and launch split

Requirement:
- RoutArr should redirect the workspace root to the dashboard surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/routarr-frontend/src/App.tsx](../../apps/routarr-frontend/src/App.tsx)
- [apps/routarr-frontend/src/App.test.tsx](../../apps/routarr-frontend/src/App.test.tsx)
- [apps/routarr-frontend/src/pages/dashboard/DashboardPage.tsx](../../apps/routarr-frontend/src/pages/dashboard/DashboardPage.tsx)
- [apps/routarr-frontend/src/pages/LaunchPage.tsx](../../apps/routarr-frontend/src/pages/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`

### 95. LoadArr app routes now preserve the dashboard default and launch split

Requirement:
- LoadArr should redirect the workspace root to the dashboard surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/loadarr-frontend/src/App.tsx](../../apps/loadarr-frontend/src/App.tsx)
- [apps/loadarr-frontend/src/App.test.tsx](../../apps/loadarr-frontend/src/App.test.tsx)
- [apps/loadarr-frontend/src/LaunchPage.tsx](../../apps/loadarr-frontend/src/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/loadarr-frontend`
- `npm run build` in `apps/loadarr-frontend`

### 96. LedgArr app routes now preserve the ERP control center default and launch split

Requirement:
- LedgArr should redirect the workspace root to the ERP control center surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/ledgarr-frontend/src/App.tsx](../../apps/ledgarr-frontend/src/App.tsx)
- [apps/ledgarr-frontend/src/App.test.tsx](../../apps/ledgarr-frontend/src/App.test.tsx)
- [apps/ledgarr-frontend/src/LaunchPage.tsx](../../apps/ledgarr-frontend/src/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`

### 97. MaintainArr app routes now preserve the overview default and launch split

Requirement:
- MaintainArr should redirect the workspace root to the overview surface and preserve the separate launch entrypoint for handoff redemption.

Evidence:
- [apps/maintainarr-frontend/src/App.tsx](../../apps/maintainarr-frontend/src/App.tsx)
- [apps/maintainarr-frontend/src/App.test.tsx](../../apps/maintainarr-frontend/src/App.test.tsx)
- [apps/maintainarr-frontend/src/pages/overview/OverviewPage.tsx](../../apps/maintainarr-frontend/src/pages/overview/OverviewPage.tsx)
- [apps/maintainarr-frontend/src/pages/LaunchPage.tsx](../../apps/maintainarr-frontend/src/pages/LaunchPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`

### 98. Suite app routes now preserve the /app default and platform-admin split

Requirement:
- The suite shell should redirect the workspace root to `/app` and preserve the separate platform-admin route surface.

Evidence:
- [apps/suite-frontend/src/App.tsx](../../apps/suite-frontend/src/App.tsx)
- [apps/suite-frontend/src/App.test.tsx](../../apps/suite-frontend/src/App.test.tsx)
- [apps/suite-frontend/src/app/routes.tsx](../../apps/suite-frontend/src/app/routes.tsx)
- [apps/suite-frontend/src/pages/platform-admin/PlatformAdminDashboardPage.tsx](../../apps/suite-frontend/src/pages/platform-admin/PlatformAdminDashboardPage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`

### 99. MaintainArr and suite shell bootstrap routes now have regression coverage

Requirement:
- MaintainArr and the suite shell should preserve their default workspace routes and launch/platform-admin splits with verified smoke coverage.

Evidence:
- [apps/maintainarr-frontend/src/App.tsx](../../apps/maintainarr-frontend/src/App.tsx)
- [apps/maintainarr-frontend/src/App.test.tsx](../../apps/maintainarr-frontend/src/App.test.tsx)
- [apps/maintainarr-frontend/src/pages/LaunchPage.tsx](../../apps/maintainarr-frontend/src/pages/LaunchPage.tsx)
- [apps/suite-frontend/src/App.tsx](../../apps/suite-frontend/src/App.tsx)
- [apps/suite-frontend/src/App.test.tsx](../../apps/suite-frontend/src/App.test.tsx)
- [apps/suite-frontend/src/app/routes.tsx](../../apps/suite-frontend/src/app/routes.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`

### 100. Suite app routes now preserve product-surface routing alongside platform-admin and home

Requirement:
- The suite shell should render a product-surface route in addition to the home redirect and platform-admin route surface.

Evidence:
- [apps/suite-frontend/src/App.tsx](../../apps/suite-frontend/src/App.tsx)
- [apps/suite-frontend/src/App.test.tsx](../../apps/suite-frontend/src/App.test.tsx)
- [apps/suite-frontend/src/app/routes.tsx](../../apps/suite-frontend/src/app/routes.tsx)
- [apps/suite-frontend/src/pages/ProductSurfacePage.tsx](../../apps/suite-frontend/src/pages/ProductSurfacePage.tsx)

Verified by targeted test and build:
- `npm test -- --run src/App.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`

### 101. AssurArr auth handoff redeem routes now have regression coverage

Requirement:
- AssurArr should preserve its auth handoff redeem aliases and reject missing or misrouted upstream handoff payloads with verified smoke coverage.

Evidence:
- [apps/assurarr-api/AssurArr.Api/Endpoints/AuthEndpoints.cs](../../apps/assurarr-api/AssurArr.Api/Endpoints/AuthEndpoints.cs)
- [apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs](../../apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs)
- [tests/STLCompliance.AssurArr.Api.Tests/AssurArrAuthEndpointsTests.cs](../../tests/STLCompliance.AssurArr.Api.Tests/AssurArrAuthEndpointsTests.cs)

Verified by targeted test and build:
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --filter FullyQualifiedName~AssurArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj`

### 102. LoadArr auth handoff redeem routes now have regression coverage

Requirement:
- LoadArr should preserve its auth handoff redeem aliases and reject missing or misrouted upstream handoff payloads with verified smoke coverage.

Evidence:
- [apps/loadarr-api/LoadArr.Api/Endpoints/AuthEndpoints.cs](../../apps/loadarr-api/LoadArr.Api/Endpoints/AuthEndpoints.cs)
- [apps/loadarr-api/LoadArr.Api/Services/HandoffAuthService.cs](../../apps/loadarr-api/LoadArr.Api/Services/HandoffAuthService.cs)
- [tests/STLCompliance.LoadArr.Auth.Tests/LoadArrAuthEndpointsTests.cs](../../tests/STLCompliance.LoadArr.Auth.Tests/LoadArrAuthEndpointsTests.cs)

Verified by targeted test and build:
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --filter FullyQualifiedName~LoadArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj`

### 103. ReportArr print and report preview surfaces now have regression coverage

Requirement:
- ReportArr should preserve purpose-built printable report surfaces for dashboards, report runs, schedules, and audit packets with verified smoke coverage.

Evidence:
- [apps/reportarr-frontend/src/App.tsx](../../apps/reportarr-frontend/src/App.tsx)
- [apps/reportarr-frontend/src/components/ReportPrint.tsx](../../apps/reportarr-frontend/src/components/ReportPrint.tsx)
- [apps/reportarr-frontend/src/components/ReportPrint.test.tsx](../../apps/reportarr-frontend/src/components/ReportPrint.test.tsx)
- [packages/shared-ui/src/print/PrintActionBar.test.tsx](../../packages/shared-ui/src/print/PrintActionBar.test.tsx)

Verified by targeted test and build:
- `npm test -- --run src/components/ReportPrint.test.tsx` in `apps/reportarr-frontend`
- `npm test -- --run src/App.test.tsx src/components/ReportPrint.test.tsx` in `apps/reportarr-frontend`
- `npm test -- --run src/print/PrintActionBar.test.tsx` in `packages/shared-ui`
- `npm run build` in `apps/reportarr-frontend`

### 104. RecordArr controlled-document periodic review refresh now has regression coverage

Requirement:
- RecordArr should move due effective controlled documents into review, keep reminder surfacing intact, and preserve an audit trail when the workflow refresh runs.

Evidence:
- [apps/recordarr-api/RecordArr.Api/Endpoints/RecordArrIntegrationEndpoints.cs](../../apps/recordarr-api/RecordArr.Api/Endpoints/RecordArrIntegrationEndpoints.cs)
- [apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs](../../apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs)
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.OpenApi.Tests/RecordArrIntegrationEndpointTests.cs](../../tests/STLCompliance.OpenApi.Tests/RecordArrIntegrationEndpointTests.cs)
- [apps/recordarr-api/RecordArr.Api/Data/recordarr-documents/11111111111111111111111111111111/aa48d7f7cef5424c9f94e23d23ce4e6b/55a86cb7867241328008dd54d14ffb56/packet.txt](../../apps/recordarr-api/RecordArr.Api/Data/recordarr-documents/11111111111111111111111111111111/aa48d7f7cef5424c9f94e23d23ce4e6b/55a86cb7867241328008dd54d14ffb56/packet.txt)

Verified by targeted test and build:
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~RecordArrIntegrationEndpointTests.Refresh_workflows_moves_due_controlled_documents_into_review_and_surfaces_reminders`
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~RecordArrIntegrationEndpointTests`
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`

### 105. RecordArr file tombstones now preserve owner-scoped inspection after purge

Requirement:
- RecordArr should keep purge tombstones inspectable by the record owner without reopening normal file access for other principals.

Evidence:
- [apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs](../../apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs)
- [tests/STLCompliance.OpenApi.Tests/RecordArrStoreTests.cs](../../tests/STLCompliance.OpenApi.Tests/RecordArrStoreTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~RecordArrStoreTests.CreateRecord_can_attach_a_single_initial_file_without_placeholder_duplication|FullyQualifiedName~RecordArrStoreTests.PurgeRecord_marks_file_objects_as_deleted|FullyQualifiedName~RecordArrStoreTests.Reminders_include_expiring_records`

### 106. Field Companion mobile execution now includes offline safety and device guidance coverage

Requirement:
- Field Companion should provide a mobile task inbox, scan and evidence capture paths, shared-device protection, offline queue handling, device capability guidance, and release safety warnings without exposing sensitive state.

Evidence:
- [apps/fieldcompanion-frontend/src/App.tsx](../../apps/fieldcompanion-frontend/src/App.tsx)
- [apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx)
- [apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx](../../apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx)
- [apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts](../../apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [apps/fieldcompanion-frontend/src/lib/releaseSafety.ts](../../apps/fieldcompanion-frontend/src/lib/releaseSafety.ts)
- [apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts](../../apps/fieldcompanion-frontend/src/lib/sharedDeviceProtection.ts)

Verified by targeted test and build:
- `npm test -- --run` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 107. NexArr tenant lifecycle now remains a retired compatibility shell with regression coverage

Requirement:
- NexArr tenant lifecycle automation should no longer drive license-based suspend/reactivate processing, and the compatibility surfaces should return empty pending actions while preserving the retired settings and overview model.

Evidence:
- [apps/nexarr-api/NexArr.Api/Services/TenantLifecycleWorkerService.cs](../../apps/nexarr-api/NexArr.Api/Services/TenantLifecycleWorkerService.cs)
- [apps/nexarr-api/NexArr.Api/Services/PlatformLifecycleOverviewService.cs](../../apps/nexarr-api/NexArr.Api/Services/PlatformLifecycleOverviewService.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantLifecycleTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantLifecycleTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter FullyQualifiedName~NexArrTenantLifecycleTests`

### 108. TrainArr qualification workflows now avoid internal ownership labels in validation copy

Requirement:
- TrainArr qualification, remediation, and manual assignment flows should keep StaffArr ownership in the data model while presenting generic person language in user-facing validation messages.

Evidence:
- [apps/trainarr-frontend/src/workspace/useTrainArrWorkspaceState.tsx](../../apps/trainarr-frontend/src/workspace/useTrainArrWorkspaceState.tsx)
- [apps/trainarr-frontend/src/components/ManualAssignmentPanel.tsx](../../apps/trainarr-frontend/src/components/ManualAssignmentPanel.tsx)
- [apps/trainarr-frontend/src/components/RemediationAssignmentPanel.tsx](../../apps/trainarr-frontend/src/components/RemediationAssignmentPanel.tsx)
- [apps/trainarr-frontend/src/components/BatchQualificationCheckPanel.tsx](../../apps/trainarr-frontend/src/components/BatchQualificationCheckPanel.tsx)
- [apps/trainarr-frontend/src/components/AuthorizationCheckOperationsPanel.tsx](../../apps/trainarr-frontend/src/components/AuthorizationCheckOperationsPanel.tsx)
- [apps/trainarr-frontend/src/components/QualificationReportsPanel.tsx](../../apps/trainarr-frontend/src/components/QualificationReportsPanel.tsx)

Verified by targeted test and build:
- `npm test -- --run` in `apps/trainarr-frontend`
- `npm run build` in `apps/trainarr-frontend`

### 109. Remaining ordinary-product auth handoff surfaces preserve cross-product launch context

Requirement:
- CustomArr, LedgArr, ReportArr, RecordArr, and OrdArr auth/session bootstrap routes should accept valid non-target launch context and preserve each product's own permission gate while using the shared NexArr handoff session.

Evidence:
- [apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs](../../apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs)
- [apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs](../../apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs)
- [apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs](../../apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs)
- [apps/recordarr-api/RecordArr.Api/Services/HandoffAuthService.cs](../../apps/recordarr-api/RecordArr.Api/Services/HandoffAuthService.cs)
- [apps/ordarr-api/OrdArr.Api/Services/HandoffAuthService.cs](../../apps/ordarr-api/OrdArr.Api/Services/HandoffAuthService.cs)
- [tests/STLCompliance.CustomArr.Api.Tests/CustomArrAuthEndpointsTests.cs](../../tests/STLCompliance.CustomArr.Api.Tests/CustomArrAuthEndpointsTests.cs)
- [tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs](../../tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs)
- [tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs](../../tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs)
- [tests/STLCompliance.RecordArr.Auth.Tests/RecordArrAuthEndpointsTests.cs](../../tests/STLCompliance.RecordArr.Auth.Tests/RecordArrAuthEndpointsTests.cs)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter FullyQualifiedName~CustomArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --filter FullyQualifiedName~LedgArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --filter FullyQualifiedName~ReportArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --filter FullyQualifiedName~RecordArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter FullyQualifiedName~OrdArrAuthEndpointsTests`

### 110. Remaining ordinary-product shell bootstraps now have passing app-level regression coverage

Requirement:
- AssurArr, CustomArr, LedgArr, and Compliance Core app bootstraps should preserve the shared shell/session behavior and continue to build successfully after the current launch and handoff changes.

Evidence:
- [apps/assurarr-frontend/src/App.tsx](../../apps/assurarr-frontend/src/App.tsx)
- [apps/assurarr-frontend/src/App.test.tsx](../../apps/assurarr-frontend/src/App.test.tsx)
- [apps/customarr-frontend/src/App.tsx](../../apps/customarr-frontend/src/App.tsx)
- [apps/customarr-frontend/src/App.test.tsx](../../apps/customarr-frontend/src/App.test.tsx)
- [apps/ledgarr-frontend/src/App.tsx](../../apps/ledgarr-frontend/src/App.tsx)
- [apps/ledgarr-frontend/src/App.test.tsx](../../apps/ledgarr-frontend/src/App.test.tsx)
- [apps/compliancecore-frontend/src/App.test.tsx](../../apps/compliancecore-frontend/src/App.test.tsx)
- [apps/compliancecore-frontend/src/auth/sessionStorage.test.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.test.ts)
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test and build:
- `npm test -- --run` in `apps/assurarr-frontend`
- `npm run build` in `apps/assurarr-frontend`
- `npm test -- --run` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`
- `npm test -- --run` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`
- `npm test -- --run` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`

### 111. Remaining ordinary-product workspace shells now have passing regression coverage

Requirement:
- StaffArr, MaintainArr, RoutArr, and SupplyArr workspace shells should continue to build and run their app-level regression suites successfully after the current shell and handoff changes.

Evidence:
- [apps/staffarr-frontend/src/App.tsx](../../apps/staffarr-frontend/src/App.tsx)
- [apps/staffarr-frontend/src/App.test.tsx](../../apps/staffarr-frontend/src/App.test.tsx)
- [apps/maintainarr-frontend/src/App.tsx](../../apps/maintainarr-frontend/src/App.tsx)
- [apps/maintainarr-frontend/src/App.test.tsx](../../apps/maintainarr-frontend/src/App.test.tsx)
- [apps/routarr-frontend/src/App.tsx](../../apps/routarr-frontend/src/App.tsx)
- [apps/routarr-frontend/src/App.test.tsx](../../apps/routarr-frontend/src/App.test.tsx)
- [apps/supplyarr-frontend/src/App.tsx](../../apps/supplyarr-frontend/src/App.tsx)
- [apps/supplyarr-frontend/src/App.test.tsx](../../apps/supplyarr-frontend/src/App.test.tsx)

Verified by targeted test and build:
- `npm test -- --run` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`
- `npm test -- --run` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm test -- --run` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`
- `npm test -- --run` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`

### 112. NexArr field-companion notifications now have dispatch regression coverage

Requirement:
- NexArr field-companion notification settings, handoff dispatch, field inbox refresh, and worker processing should remain permissioned, enqueue durable dispatch records, and post webhook notifications when enabled.

Evidence:
- [apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs](../../apps/nexarr-api/NexArr.Api/Endpoints/FieldCompanionNotificationEndpoints.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationDispatchService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationEnqueueService.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationRules.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionNotificationRules.cs)
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionWebPushPayloadBuilder.cs)
- [apps/nexarr-api/NexArr.Api/Entities/FieldCompanionNotificationEntities.cs](../../apps/nexarr-api/NexArr.Api/Entities/FieldCompanionNotificationEntities.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionNotificationTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter FullyQualifiedName~NexArrFieldCompanionNotificationTests`

### 113. Shared product response framework now has end-to-end worker regression coverage

Requirement:
- The shared product response, event envelope, loop-prevention, status, and contract rules must continue to enforce the cross-product integration contract used by the suite.

Evidence:
- [packages/shared-dotnet/STLCompliance.Shared/Integration/StlProductResponseFramework.cs](../../packages/shared-dotnet/STLCompliance.Shared/Integration/StlProductResponseFramework.cs)
- [tests/STLCompliance.Shared.Worker.Tests/IntelligentProductResponseFrameworkTests.cs](../../tests/STLCompliance.Shared.Worker.Tests/IntelligentProductResponseFrameworkTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj`
- `dotnet build packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj --no-restore`

### 114. Field Companion degraded and evidence capture UI now comply with the shared theme audit

Requirement:
- Field Companion degraded-operation and evidence-capture surfaces must use shared semantic tokens, avoid raw palette values, and remain readable in both themes.

Evidence:
- [apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx](../../apps/fieldcompanion-frontend/src/components/DegradedOperationPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskEvidencePanel.tsx)
- [packages/shared-ui/src/theme.css](../../packages/shared-ui/src/theme.css)
- [docs/constitutions/ui.md](../../docs/constitutions/ui.md)
- [docs/constitutions/pages/empty-loading-error-degraded.md](../../docs/constitutions/pages/empty-loading-error-degraded.md)

Verified by targeted test/build:
- `npm run audit:theme` in `packages/shared-ui`
- `npm test -- --run src/components/DegradedOperationPanel.test.tsx src/components/FieldTaskEvidencePanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`

### 115. RecordArr controlled-document refresh and tombstone access rules now have regression coverage

Requirement:
- RecordArr must keep controlled-document review refreshes, reminder generation, and owner-allowed tombstone access behavior aligned with the document/retention workflow.

Evidence:
- [apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs](../../apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs)
- [tests/STLCompliance.OpenApi.Tests/RecordArrStoreTests.cs](../../tests/STLCompliance.OpenApi.Tests/RecordArrStoreTests.cs)
- [tests/STLCompliance.OpenApi.Tests/RecordArrIntegrationEndpointTests.cs](../../tests/STLCompliance.OpenApi.Tests/RecordArrIntegrationEndpointTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~RecordArr`

### 116. OrdArr completion packets now coordinate closeout and finance readiness

Requirement:
- OrdArr must expose durable completion-packet updates that advance order closeout and finance-readiness states after approval.

Evidence:
- [apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs](../../apps/ordarr-api/OrdArr.Api/Data/OrdArrStore.cs)
- [apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs](../../apps/ordarr-api/OrdArr.Api/Endpoints/WorkspaceEndpoints.cs)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrAuthEndpointsTests.cs)
- [tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs](../../tests/STLCompliance.OrdArr.Auth.Tests/OrdArrStoreTests.cs)

Verified by targeted test:
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter FullyQualifiedName~Completion_packets`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter FullyQualifiedName~OrdArrStoreTests.Completion_packets_advance_closeout_and_finance_states`
- `npm run audit:theme` in `apps/ordarr-frontend`

### 117. Shared product switcher now hides Compliance Core from ordinary users and exposes it to platform admins

Requirement:
- The suite product switcher must present the ordinary product catalog to all users while keeping Compliance Core studio hidden unless the caller is a platform administrator.

Evidence:
- [packages/shared-ui/src/ProductSwitcher.tsx](../../packages/shared-ui/src/ProductSwitcher.tsx)
- [packages/shared-ui/src/ProductSwitcher.test.tsx](../../packages/shared-ui/src/ProductSwitcher.test.tsx)
- [apps/suite-frontend/src/components/ProductSwitcher.tsx](../../apps/suite-frontend/src/components/ProductSwitcher.tsx)
- [apps/suite-frontend/src/components/ProductSwitcher.test.tsx](../../apps/suite-frontend/src/components/ProductSwitcher.test.tsx)

Verified by targeted test/build:
- `npm test -- --run src/ProductSwitcher.test.tsx` in `packages/shared-ui`
- `npm test -- --run src/components/ProductSwitcher.test.tsx src/pages/ProductSurfacePage.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`
- `npm run audit:theme` in `apps/suite-frontend`

### 118. NexArr tenant lifecycle processing now stays inert for ordinary product access

Requirement:
- Product availability must remain nonvariable for ordinary tenant users, so tenant lifecycle processing cannot silently suspend or reactivate access as part of the standard product-launch path.

Evidence:
- [apps/nexarr-api/NexArr.Api/Services/FieldCompanionAuthService.cs](../../apps/nexarr-api/NexArr.Api/Services/FieldCompanionAuthService.cs)
- [tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantLifecycleTests.cs](../../tests/STLCompliance.NexArr.Auth.Tests/NexArrTenantLifecycleTests.cs)
- [docs/constitutions/ownership.md](../../docs/constitutions/ownership.md)
- [docs/constitutions/ui.md](../../docs/constitutions/ui.md)

Verified by targeted test:
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrTenantLifecycleTests|FullyQualifiedName~NexArrFieldCompanionNotificationTests"`

### 119. LoadArr now persists platform-admin session metadata and stabilizes workspace fallback selection

Requirement:
- LoadArr must carry platform-admin session metadata through the frontend session model and keep the unresolved-workflow fallback form populated from current inventory and location context.

Evidence:
- [apps/loadarr-frontend/src/App.tsx](../../apps/loadarr-frontend/src/App.tsx)
- [apps/loadarr-frontend/src/auth/sessionStorage.ts](../../apps/loadarr-frontend/src/auth/sessionStorage.ts)
- [apps/loadarr-frontend/vite.config.ts](../../apps/loadarr-frontend/vite.config.ts)
- [apps/loadarr-frontend/src/App.test.tsx](../../apps/loadarr-frontend/src/App.test.tsx)
- [apps/loadarr-frontend/src/auth/sessionStorage.test.ts](../../apps/loadarr-frontend/src/auth/sessionStorage.test.ts)

Verified by targeted test/build:
- `npm test -- --run` in `apps/loadarr-frontend`
- `npm run build` in `apps/loadarr-frontend`
- `npm run audit:theme` in `apps/loadarr-frontend`

### 120. StaffArr self-service portal now distinguishes direct, review-required, and restricted update fields

Requirement:
- StaffArr self-service requests must clearly distinguish which profile fields can be updated directly, which require review, and which remain HR-restricted.

Evidence:
- [apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.tsx](../../apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.tsx)
- [apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.test.tsx](../../apps/staffarr-frontend/src/components/MeSelfServicePortalPanel.test.tsx)
- [apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/staffarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test/build:
- `npm test -- --run src/components/MeSelfServicePortalPanel.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`
- `npm run audit:theme` in `apps/staffarr-frontend`

### 121. TrainArr qualification and settings surfaces now speak in person-centric, platform-admin-aware terms

Requirement:
- TrainArr workspace and validation copy must treat people as shared platform references and carry platform-admin session context through the workspace shell.

Evidence:
- [apps/trainarr-frontend/src/components/TenantSettingsPanel.tsx](../../apps/trainarr-frontend/src/components/TenantSettingsPanel.tsx)
- [apps/trainarr-frontend/src/workspace/useTrainArrWorkspaceState.tsx](../../apps/trainarr-frontend/src/workspace/useTrainArrWorkspaceState.tsx)
- [apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/trainarr-frontend/src/components/AuthorizationCheckOperationsPanel.test.tsx](../../apps/trainarr-frontend/src/components/AuthorizationCheckOperationsPanel.test.tsx)
- [apps/trainarr-frontend/src/components/BatchQualificationCheckPanel.test.tsx](../../apps/trainarr-frontend/src/components/BatchQualificationCheckPanel.test.tsx)
- [apps/trainarr-frontend/src/components/ManualAssignmentPanel.test.tsx](../../apps/trainarr-frontend/src/components/ManualAssignmentPanel.test.tsx)
- [apps/trainarr-frontend/src/components/QualificationReportsPanel.test.tsx](../../apps/trainarr-frontend/src/components/QualificationReportsPanel.test.tsx)
- [apps/trainarr-frontend/src/components/TenantSettingsPanel.test.tsx](../../apps/trainarr-frontend/src/components/TenantSettingsPanel.test.tsx)
- [apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/trainarr-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test/build:
- `npm test -- --run src/components/AuthorizationCheckOperationsPanel.test.tsx src/components/BatchQualificationCheckPanel.test.tsx src/components/ManualAssignmentPanel.test.tsx src/components/QualificationReportsPanel.test.tsx src/components/TenantSettingsPanel.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/trainarr-frontend`
- `npm run build` in `apps/trainarr-frontend`
- `npm run audit:theme` in `apps/trainarr-frontend`

### 122. ReportArr workspace now persists platform-admin session context and keeps print/report coverage green

Requirement:
- ReportArr must preserve platform-admin workspace context through session storage and continue to render the print/report surfaces that depend on it.

Evidence:
- [apps/reportarr-frontend/src/App.tsx](../../apps/reportarr-frontend/src/App.tsx)
- [apps/reportarr-frontend/src/auth/sessionStorage.ts](../../apps/reportarr-frontend/src/auth/sessionStorage.ts)
- [apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs](../../apps/reportarr-api/ReportArr.Api/Services/HandoffAuthService.cs)
- [apps/reportarr-frontend/src/App.test.tsx](../../apps/reportarr-frontend/src/App.test.tsx)
- [apps/reportarr-frontend/src/components/ReportPrint.test.tsx](../../apps/reportarr-frontend/src/components/ReportPrint.test.tsx)
- [tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs](../../tests/STLCompliance.ReportArr.Auth.Tests/ReportArrAuthEndpointsTests.cs)

Verified by targeted test/build:
- `npm test -- --run` in `apps/reportarr-frontend`
- `npm run build` in `apps/reportarr-frontend`
- `npm run audit:theme` in `apps/reportarr-frontend`
- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --filter FullyQualifiedName~ReportArrAuthEndpointsTests`

### 123. CustomArr workspace now persists platform-admin session context and keeps launch handoff messaging aligned

Requirement:
- CustomArr must preserve platform-admin workspace context through session storage and keep handoff denial messaging clear and non-entitlement-based.

Evidence:
- [apps/customarr-frontend/src/App.tsx](../../apps/customarr-frontend/src/App.tsx)
- [apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs](../../apps/customarr-api/CustomArr.Api/Services/HandoffAuthService.cs)
- [apps/customarr-frontend/src/App.test.tsx](../../apps/customarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.CustomArr.Api.Tests/CustomArrAuthEndpointsTests.cs](../../tests/STLCompliance.CustomArr.Api.Tests/CustomArrAuthEndpointsTests.cs)

Verified by targeted test/build:
- `npm test -- --run` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`
- `npm run audit:theme` in `apps/customarr-frontend`
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter FullyQualifiedName~CustomArrAuthEndpointsTests`

### 124. AssurArr workspace now persists platform-admin session context and keeps handoff denial language aligned

Requirement:
- AssurArr must preserve platform-admin workspace context through session storage and use neutral handoff denial language for unavailable codes.

Evidence:
- [apps/assurarr-frontend/src/App.tsx](../../apps/assurarr-frontend/src/App.tsx)
- [apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs](../../apps/assurarr-api/AssurArr.Api/Services/HandoffAuthService.cs)
- [apps/assurarr-frontend/src/App.test.tsx](../../apps/assurarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.AssurArr.Api.Tests/AssurArrAuthEndpointsTests.cs](../../tests/STLCompliance.AssurArr.Api.Tests/AssurArrAuthEndpointsTests.cs)

Verified by targeted test/build:
- `npm test -- --run` in `apps/assurarr-frontend`
- `npm run build` in `apps/assurarr-frontend`
- `npm run audit:theme` in `apps/assurarr-frontend`
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --filter FullyQualifiedName~AssurArrAuthEndpointsTests`
- `dotnet build apps/assurarr-api/AssurArr.Api/AssurArr.Api.csproj --no-restore`

### 125. LedgArr workspace now persists platform-admin session context and keeps handoff denial language aligned

Requirement:
- LedgArr must preserve platform-admin workspace context through session storage and use neutral handoff denial language for unavailable codes.

Evidence:
- [apps/ledgarr-frontend/src/App.tsx](../../apps/ledgarr-frontend/src/App.tsx)
- [apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs](../../apps/ledgarr-api/LedgArr.Api/Services/HandoffAuthService.cs)
- [apps/ledgarr-frontend/src/App.test.tsx](../../apps/ledgarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs](../../tests/STLCompliance.LedgArr.Tests/LedgArrAuthEndpointsTests.cs)

Verified by targeted test/build:
- `npm test -- --run` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`
- `npm run audit:theme` in `apps/ledgarr-frontend`
- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --filter FullyQualifiedName~LedgArrAuthEndpointsTests`
- `dotnet build apps/ledgarr-api/LedgArr.Api/LedgArr.Api.csproj --no-restore`

### 126. RoutArr workspace now persists platform-admin session context and clarifies carrier portal authorization

Requirement:
- RoutArr must preserve platform-admin workspace context through session storage and describe carrier collaboration as authorization-based rather than entitlement-based.

Evidence:
- [apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/routarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/routarr-frontend/src/auth/sessionStorage.ts](../../apps/routarr-frontend/src/auth/sessionStorage.ts)
- [apps/routarr-api/RoutArr.Api/Services/RoutArrTenantSettingsDefinitions.cs](../../apps/routarr-api/RoutArr.Api/Services/RoutArrTenantSettingsDefinitions.cs)
- [apps/routarr-frontend/src/App.test.tsx](../../apps/routarr-frontend/src/App.test.tsx)
- [tests/STLCompliance.RoutArr.Auth.Tests/RoutArrTenantSettingsServiceTests.cs](../../tests/STLCompliance.RoutArr.Auth.Tests/RoutArrTenantSettingsServiceTests.cs)

Verified by targeted test/build:
- `npm test -- --run` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`
- `npm run audit:theme` in `apps/routarr-frontend`
- `dotnet test tests/STLCompliance.RoutArr.Auth.Tests/STLCompliance.RoutArr.Auth.Tests.csproj --filter FullyQualifiedName~RoutArrTenantSettingsServiceTests`
- `dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj --no-restore`

### 127. SupplyArr workspace now persists platform-admin session context and aligns purchasing report wording

Requirement:
- SupplyArr must preserve platform-admin workspace context through session storage and keep purchasing-report language aligned with receiving exception workflows.

Evidence:
- [apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/supplyarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/supplyarr-frontend/src/auth/sessionStorage.ts](../../apps/supplyarr-frontend/src/auth/sessionStorage.ts)
- [apps/supplyarr-frontend/src/components/PurchasingReportsPanel.test.tsx](../../apps/supplyarr-frontend/src/components/PurchasingReportsPanel.test.tsx)
- [apps/supplyarr-frontend/src/App.test.tsx](../../apps/supplyarr-frontend/src/App.test.tsx)

Verified by targeted test/build:
- `npm test -- --run` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`
- `npm run audit:theme` in `apps/supplyarr-frontend`

### 128. Compliance Core workspace now persists platform-admin session context through the shell

Requirement:
- Compliance Core must preserve platform-admin workspace context through session storage and workspace shell hydration.

Evidence:
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/compliancecore-frontend/src/auth/sessionStorage.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.ts)
- [apps/compliancecore-frontend/src/App.test.tsx](../../apps/compliancecore-frontend/src/App.test.tsx)
- [apps/compliancecore-frontend/src/auth/sessionStorage.test.ts](../../apps/compliancecore-frontend/src/auth/sessionStorage.test.ts)
- [apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx](../../apps/compliancecore-frontend/src/layouts/ProductWorkspaceLayout.test.tsx)

Verified by targeted test/build:
- `npm test -- --run src/App.test.tsx src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`
- `npm run audit:theme` in `apps/compliancecore-frontend`

### 129. MaintainArr workspace now persists platform-admin session context through the shell

Requirement:
- MaintainArr must preserve platform-admin workspace context through session storage and workspace shell hydration.

Evidence:
- [apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx](../../apps/maintainarr-frontend/src/layouts/ProductWorkspaceLayout.tsx)
- [apps/maintainarr-frontend/src/auth/sessionStorage.ts](../../apps/maintainarr-frontend/src/auth/sessionStorage.ts)
- [apps/maintainarr-frontend/src/App.test.tsx](../../apps/maintainarr-frontend/src/App.test.tsx)

Verified by targeted test/build:
- `npm test -- --run` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm run audit:theme` in `apps/maintainarr-frontend`

### 130. Field Companion inbox, scan, offline, clock, and profile flows now surface richer operational state

Requirement:
- Field Companion must present operational urgency, scan normalization, offline conflict handling, clock location guidance, and session/device recovery in the mobile field-execution surface.

Evidence:
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldScanPanel.tsx](../../apps/fieldcompanion-frontend/src/components/FieldScanPanel.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx)
- [apps/fieldcompanion-frontend/src/lib/fieldInbox.ts](../../apps/fieldcompanion-frontend/src/lib/fieldInbox.ts)
- [apps/fieldcompanion-frontend/src/lib/offlineQueue.ts](../../apps/fieldcompanion-frontend/src/lib/offlineQueue.ts)
- [apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts](../../apps/fieldcompanion-frontend/src/lib/evidenceCapture.ts)
- [apps/fieldcompanion-frontend/src/App.test.tsx](../../apps/fieldcompanion-frontend/src/App.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldInboxPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/FieldTaskInspectionPanel.test.tsx)
- [apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx](../../apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/NotificationsPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/NotificationsPage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/OfflineQueuePage.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx)

Verified by targeted test/build:
- `npm test -- --run` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm run audit:theme` in `apps/fieldcompanion-frontend`

### 131. RecordArr frontend now supports evidence mapping and coverage review on record detail

Requirement:
- RecordArr must let users create evidence mappings, inspect coverage evaluations, and carry default capture/access context through the workspace.

Evidence:
- [apps/recordarr-frontend/src/App.tsx](../../apps/recordarr-frontend/src/App.tsx)
- [apps/recordarr-frontend/src/auth/sessionStorage.ts](../../apps/recordarr-frontend/src/auth/sessionStorage.ts)
- [apps/recordarr-frontend/vite.config.ts](../../apps/recordarr-frontend/vite.config.ts)
- [apps/recordarr-frontend/src/App.test.tsx](../../apps/recordarr-frontend/src/App.test.tsx)
- [apps/recordarr-frontend/package.json](../../apps/recordarr-frontend/package.json)
- [apps/recordarr-frontend/package-lock.json](../../apps/recordarr-frontend/package-lock.json)
- [apps/recordarr-frontend/src/test/setup.ts](../../apps/recordarr-frontend/src/test/setup.ts)

Verified by targeted test/build:
- `npm test -- --run` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`
- `npm run audit:theme` in `apps/recordarr-frontend`

### 132. MaintainArr asset reservation and motor-pool readiness now has verified request-to-close coverage on the asset profile surface

Requirement:
- MaintainArr must let authorized users request, approve, reserve, check out, return, inspect, and close shared asset reservations while preserving StaffArr location ownership and TrainArr qualification checks.

Evidence:
- [apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/AssetReservationService.cs)
- [apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetReservationEndpoints.cs](../../apps/maintainarr-api/MaintainArr.Api/Endpoints/AssetReservationEndpoints.cs)
- [apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs](../../apps/maintainarr-api/MaintainArr.Api/Entities/AssetReservationEntities.cs)
- [apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.cs](../../apps/maintainarr-api/MaintainArr.Api/Migrations/20260626235457_AddAssetReservations.cs)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.tsx)
- [apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.tsx](../../apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.tsx)
- [apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx](../../apps/maintainarr-frontend/src/components/AssetReservationPanel.test.tsx)
- [apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.test.tsx](../../apps/maintainarr-frontend/src/pages/assets/AssetProfilePage.test.tsx)
- [tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs](../../tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrAssetDowntimeTests.cs)

Constitution coverage:
- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/platform-security-tenancy-authority-constitution.md](../constitutions/platform-security-tenancy-authority-constitution.md)
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)

Verified by targeted test/build:
- `npm test -- src/components/AssetReservationPanel.test.tsx src/pages/assets/AssetProfilePage.test.tsx src/pages/vendor-portal/VendorPortalPage.test.tsx src/components/WorkOrderVendorWorkPanel.test.tsx` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter FullyQualifiedName~Reservation_create_get_and_conflict_block_round_trip`
- `dotnet build apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj -c Debug`

### 133. MaintainArr vendor work now exposes scoped external portal invitations and a public status update flow

Requirement:
- MaintainArr must issue time-scoped vendor portal links, let the invited vendor update their assigned work status, and revoke access without exposing unrelated tenant data.

Evidence:
- [apps/maintainarr-api/MaintainArr.Api/Services/MaintenanceVendorWorkService.cs](../../apps/maintainarr-api/MaintainArr.Api/Services/MaintenanceVendorWorkService.cs)
- [apps/maintainarr-api/MaintainArr.Api/Endpoints/MaintenanceVendorWorkEndpoints.cs](../../apps/maintainarr-api/MaintainArr.Api/Endpoints/MaintenanceVendorWorkEndpoints.cs)
- [apps/maintainarr-api/MaintainArr.Api/Endpoints/MaintenanceVendorWorkPortalEndpoints.cs](../../apps/maintainarr-api/MaintainArr.Api/Endpoints/MaintenanceVendorWorkPortalEndpoints.cs)
- [apps/maintainarr-api/MaintainArr.Api/Contracts/VendorWorkContracts.cs](../../apps/maintainarr-api/MaintainArr.Api/Contracts/VendorWorkContracts.cs)
- [apps/maintainarr-api/MaintainArr.Api/Entities/MaintenanceVendorWorkEntities.cs](../../apps/maintainarr-api/MaintainArr.Api/Entities/MaintenanceVendorWorkEntities.cs)
- [apps/maintainarr-api/MaintainArr.Api/Migrations/20260627002831_AddMaintenanceVendorWorkPortalAccess.cs](../../apps/maintainarr-api/MaintainArr.Api/Migrations/20260627002831_AddMaintenanceVendorWorkPortalAccess.cs)
- [apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.tsx)
- [apps/maintainarr-frontend/src/pages/vendor-portal/VendorPortalPage.tsx](../../apps/maintainarr-frontend/src/pages/vendor-portal/VendorPortalPage.tsx)
- [apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.test.tsx](../../apps/maintainarr-frontend/src/components/WorkOrderVendorWorkPanel.test.tsx)
- [apps/maintainarr-frontend/src/pages/vendor-portal/VendorPortalPage.test.tsx](../../apps/maintainarr-frontend/src/pages/vendor-portal/VendorPortalPage.test.tsx)
- [tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrWorkOrderTests.cs](../../tests/STLCompliance.MaintainArr.Auth.Tests/MaintainArrWorkOrderTests.cs)
- [tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json](../../tests/STLCompliance.OpenApi.Tests/snapshots/maintainarr.openapi.json)

Constitution coverage:
- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/platform-external-portal-access-constitution.md](../constitutions/platform-external-portal-access-constitution.md)
- [docs/constitutions/platform-security-tenancy-authority-constitution.md](../constitutions/platform-security-tenancy-authority-constitution.md)
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)

Verified by targeted test/build:
- `npm test -- src/components/AssetReservationPanel.test.tsx src/pages/assets/AssetProfilePage.test.tsx src/pages/vendor-portal/VendorPortalPage.test.tsx src/components/WorkOrderVendorWorkPanel.test.tsx` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter FullyQualifiedName~Vendor_portal_issue_open_update_and_revoke_round_trip`
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~MaintainArrOpenApiParityTests`
- `dotnet build apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj -c Debug`

### 134. MaintainArr emergency breakdown drafts now persist emergency classification for manual out-of-service requests

Requirement:
- The emergency breakdown workflow should keep manual out-of-service requests visibly classified as emergency work orders so the downstream record and reporting retain the rapid-dispatch context.

Evidence:
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/work-orders/WorkOrderCreatePage.test.tsx)
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)

Constitution coverage:
- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/platform-security-tenancy-authority-constitution.md](../constitutions/platform-security-tenancy-authority-constitution.md)

Verified by targeted test/build:
- `npm test -- src/pages/work-orders/WorkOrderCreatePage.test.tsx` in `apps/maintainarr-frontend`

## Remaining deferred work

- The repository still contains large scaffold/prototype areas documented in product FEATURESET files, especially around durable persistence, workflow completion, and cross-product vertical slices.
- Legacy compatibility surfaces under `apps/nexarr-api` still exist by design for retired endpoints and historical contracts.
- This report does not yet cover the full product-by-product workflow and feature matrix required by the objective.

## Verification already run for this pass

- `npm test -- --run src/pages/TermsPage.test.tsx`
- `npm run build` in `apps/stlcompliancesite`
- `npm run audit:theme` in `apps/stlcompliancesite`
- `npm test -- --run src/ProductSwitcher.test.tsx` in `packages/shared-ui`
- `npm test -- --run src/print/PrintActionBar.test.tsx` in `packages/shared-ui`
- `npm test -- --run src/components/ProductSwitcher.test.tsx` in `apps/suite-frontend`
- `npm run build` in `apps/suite-frontend`
- `npm run audit:theme` in `apps/suite-frontend`
- `npm test -- --run src/App.test.tsx src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`
- `npm run audit:theme` in `apps/compliancecore-frontend`
- `npm test -- --run src/lib/FieldCompanionDeniedReasonCatalog.test.ts`
- `npx tsc -p tsconfig.json --noEmit` in `apps/fieldcompanion-frontend`
- `npm test -- --run` in `apps/trainarr-frontend`
- `npm run build` in `apps/trainarr-frontend`
- `npm test -- --run` in `apps/assurarr-frontend`
- `npm run build` in `apps/assurarr-frontend`
- `npm test -- --run` in `apps/customarr-frontend`
- `npm run build` in `apps/customarr-frontend`
- `npm test -- --run` in `apps/ledgarr-frontend`
- `npm run build` in `apps/ledgarr-frontend`
- `npm test -- --run` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`
- `npm test -- --run` in `apps/staffarr-frontend`
- `npm run build` in `apps/staffarr-frontend`
- `npm test -- --run` in `apps/maintainarr-frontend`
- `npm run build` in `apps/maintainarr-frontend`
- `npm test -- --run` in `apps/routarr-frontend`
- `npm run build` in `apps/routarr-frontend`
- `npm test -- --run` in `apps/supplyarr-frontend`
- `npm run build` in `apps/supplyarr-frontend`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter FullyQualifiedName~NexArrFieldCompanionNotificationTests`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrFieldCompanionNotificationTests|FullyQualifiedName~NexArrTenantLifecycleTests"`
- `dotnet build apps/nexarr-api/NexArr.Api/NexArr.Api.csproj --no-restore`
- `dotnet test tests/STLCompliance.Shared.Worker.Tests/STLCompliance.Shared.Worker.Tests.csproj`
- `dotnet build packages/shared-dotnet/STLCompliance.Shared/STLCompliance.Shared.csproj --no-restore`
- `dotnet test tests/STLCompliance.CustomArr.Api.Tests/STLCompliance.CustomArr.Api.Tests.csproj --filter FullyQualifiedName~CustomArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.LedgArr.Tests/STLCompliance.LedgArr.Tests.csproj --filter FullyQualifiedName~LedgArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.ReportArr.Auth.Tests/STLCompliance.ReportArr.Auth.Tests.csproj --filter FullyQualifiedName~ReportArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --filter FullyQualifiedName~RecordArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --filter FullyQualifiedName~OrdArrAuthEndpointsTests`
- `dotnet build apps/assurarr-api/AssurArr.Api/AssurArr.Api.csproj --no-restore`
- `dotnet build apps/customarr-api/CustomArr.Api/CustomArr.Api.csproj --no-restore`
- `dotnet build apps/ledgarr-api/LedgArr.Api/LedgArr.Api.csproj --no-restore`
- `dotnet build apps/reportarr-api/ReportArr.Api/ReportArr.Api.csproj --no-restore`
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --no-restore`
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj --filter FullyQualifiedName~AssurArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.AssurArr.Api.Tests/STLCompliance.AssurArr.Api.Tests.csproj`
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --filter FullyQualifiedName~LoadArrAuthEndpointsTests`
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj`
- `npm test -- --run src/components/ReportPrint.test.tsx` in `apps/reportarr-frontend`
- `npm test -- --run src/App.test.tsx src/components/ReportPrint.test.tsx` in `apps/reportarr-frontend`
- `npm run build` in `apps/reportarr-frontend`
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --no-restore --filter FieldCompanionDeniedReasonCatalogTests`
- `dotnet test tests/STLCompliance.LoadArr.Auth.Tests/STLCompliance.LoadArr.Auth.Tests.csproj --no-restore --filter LoadArrTenantSettingsTests`
- `dotnet build apps/loadarr-api/LoadArr.Api/LoadArr.Api.csproj --no-restore`
- `dotnet build apps/recordarr-api/RecordArr.Api/RecordArr.Api.csproj --no-restore`
- `dotnet build apps/customarr-api/CustomArr.Api/CustomArr.Api.csproj --no-restore`
- `dotnet build apps/routarr-api/RoutArr.Api/RoutArr.Api.csproj --no-restore`
- `dotnet build apps/ledgarr-api/LedgArr.Api/LedgArr.Api.csproj --no-restore`
- `dotnet build apps/ordarr-api/OrdArr.Api/OrdArr.Api.csproj --no-restore`
- `dotnet build apps/reportarr-api/ReportArr.Api/ReportArr.Api.csproj --no-restore`
- `npm test -- --run src/App.test.tsx` in `apps/ordarr-frontend`
- `npm run build` in `apps/ordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend` after adding the package workflow slice
- `npm run build` in `apps/recordarr-frontend` after adding the package workflow slice
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend` after adding the legal-hold workflow slice
- `npm run build` in `apps/recordarr-frontend` after adding the legal-hold workflow slice
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend` after adding the access-controls and redaction slice
- `npm run build` in `apps/recordarr-frontend` after adding the access-controls and redaction slice
- `dotnet test tests/STLCompliance.OrdArr.Auth.Tests/STLCompliance.OrdArr.Auth.Tests.csproj --no-restore`
- `npm test -- --run src/lib/offlineQueue.test.ts src/components/OfflineQueuePanel.test.tsx src/pages/OfflineQueuePage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/lib/sharedDeviceProtection.test.tsx src/components/SharedDeviceProtectionOverlay.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx src/auth/sessionStorage.test.ts` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/pages/ProfilePage.test.tsx src/auth/sessionStorage.test.ts src/lib/sharedDeviceProtection.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/lib/releaseSafety.test.ts src/components/FieldCompanionReleaseSafetyBanner.test.tsx src/pages/LaunchPage.test.tsx src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/lib/offlineQueueFreshness.test.ts src/components/OfflineQueuePanel.test.tsx src/pages/OfflineQueuePage.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/lib/deviceCapabilities.test.ts src/lib/degradedOperation.test.ts src/components/DegradedOperationPanel.test.tsx src/lib/evidenceOptimization.test.ts src/components/DeviceCapabilityPanel.test.tsx` in `apps/fieldcompanion-frontend`
- `npm run build` in `apps/fieldcompanion-frontend`
- `npm test -- --run src/App.test.tsx -t "recalculates retention and completes a disposal review from the retention workspace"` in `apps/recordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/recordarr-frontend`
- `npm run build` in `apps/recordarr-frontend`
- `npm test -- --run src/App.test.tsx` in `apps/loadarr-frontend`
- `npm run build` in `apps/loadarr-frontend`
- `npm test -- --run src/auth/sessionStorage.test.ts src/layouts/ProductWorkspaceLayout.test.tsx` in `apps/compliancecore-frontend`
- `npm run build` in `apps/compliancecore-frontend`
- `npm test -- src/components/AssetReservationPanel.test.tsx src/pages/assets/AssetProfilePage.test.tsx src/pages/vendor-portal/VendorPortalPage.test.tsx src/components/WorkOrderVendorWorkPanel.test.tsx` in `apps/maintainarr-frontend`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter FullyQualifiedName~Reservation_create_get_and_conflict_block_round_trip`
- `dotnet test tests/STLCompliance.MaintainArr.Auth.Tests/STLCompliance.MaintainArr.Auth.Tests.csproj --filter FullyQualifiedName~Vendor_portal_issue_open_update_and_revoke_round_trip`
- `dotnet test tests/STLCompliance.OpenApi.Tests/STLCompliance.OpenApi.Tests.csproj --filter FullyQualifiedName~MaintainArrOpenApiParityTests`
- `dotnet build apps/maintainarr-api/MaintainArr.Api/MaintainArr.Api.csproj -c Debug`

### 30. MaintainArr PM owning-site quick-create now uses live StaffArr site references

Requirement:
- MaintainArr PM program create should select owning sites from StaffArr with a live owner-backed reference picker and preserve context when a valid site does not yet exist.

Evidence:
- [apps/staffarr-api/StaffArr.Api/Endpoints/ReferenceIntegrationEndpoints.cs](../../apps/staffarr-api/StaffArr.Api/Endpoints/ReferenceIntegrationEndpoints.cs)
- [apps/maintainarr-frontend/src/pages/pm-programs/PmProgramCreatePage.tsx](../../apps/maintainarr-frontend/src/pages/pm-programs/PmProgramCreatePage.tsx)
- [apps/maintainarr-frontend/src/pages/pm-programs/PmProgramCreatePage.test.tsx](../../apps/maintainarr-frontend/src/pages/pm-programs/PmProgramCreatePage.test.tsx)
- [tests/STLCompliance.StaffArr.Auth.Tests/StaffArrIntegrationSurfaceTests.cs](../../tests/STLCompliance.StaffArr.Auth.Tests/StaffArrIntegrationSurfaceTests.cs)
- [tests/STLCompliance.OpenApi.Tests/OpenApiParityTests.cs](../../tests/STLCompliance.OpenApi.Tests/OpenApiParityTests.cs)
- [docs/products/maintainarr/FEATURESET.md](../products/maintainarr/FEATURESET.md)
- [docs/products/maintainarr/WORKFLOWS.md](../products/maintainarr/WORKFLOWS.md)
- [docs/products/staffarr/FEATURESET.md](../products/staffarr/FEATURESET.md)

Constitution coverage:
- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/pages/recordcreate.md](../constitutions/pages/recordcreate.md)
- [docs/constitutions/pages/cross-product-reference.md](../constitutions/pages/cross-product-reference.md)

### 31. StaffArr person-create placement now quick-creates missing sites and home-base locations without losing context

Requirement:
- The StaffArr person-create workflow should keep placement controlled and allow a missing site or site-scoped home-base location to be quick-created inline instead of forcing a context reset.

Evidence:
- [apps/staffarr-frontend/src/components/CreatePersonPanel.tsx](../../apps/staffarr-frontend/src/components/CreatePersonPanel.tsx)
- [apps/staffarr-frontend/src/components/CreatePersonPanel.test.tsx](../../apps/staffarr-frontend/src/components/CreatePersonPanel.test.tsx)
- [packages/shared-ui/src/forms/ReferencePicker.tsx](../../packages/shared-ui/src/forms/ReferencePicker.tsx)
- [packages/shared-ui/src/forms/forms.components.test.tsx](../../packages/shared-ui/src/forms/forms.components.test.tsx)
- [apps/staffarr-api/StaffArr.Api/Endpoints/ReferenceIntegrationEndpoints.cs](../../apps/staffarr-api/StaffArr.Api/Endpoints/ReferenceIntegrationEndpoints.cs)
- [tests/STLCompliance.StaffArr.Auth.Tests/StaffArrIntegrationSurfaceTests.cs](../../tests/STLCompliance.StaffArr.Auth.Tests/StaffArrIntegrationSurfaceTests.cs)
- [docs/products/staffarr/FEATURESET.md](../products/staffarr/FEATURESET.md)

Verified by focused test runs:
- `npm test -- --run src/components/CreatePersonPanel.test.tsx` in `apps/staffarr-frontend`
- `npm test -- --run src/workspace/sections/PeopleSection.test.tsx` in `apps/staffarr-frontend`
- `npm test -- --run src/forms/forms.components.test.tsx` in `packages/shared-ui`
- `dotnet test tests/STLCompliance.StaffArr.Auth.Tests/STLCompliance.StaffArr.Auth.Tests.csproj --filter "FullyQualifiedName~StaffArrIntegrationSurfaceTests"`

Constitution coverage:
- [docs/constitutions/ownership.md](../constitutions/ownership.md)
- [docs/constitutions/ui.md](../constitutions/ui.md)
- [docs/constitutions/pages/recordcreate.md](../constitutions/pages/recordcreate.md)
- [docs/constitutions/pages/cross-product-reference.md](../constitutions/pages/cross-product-reference.md)

### 32. Field Companion submission activity banner now auto-dismisses queued toast updates

Requirement:
- Submission feedback should surface the newest local submission toast and allow manual or automatic dismissal without obscuring the queue state.

Evidence:
- [apps/fieldcompanion-frontend/src/lib/submissionState.ts](../../apps/fieldcompanion-frontend/src/lib/submissionState.ts)
- [apps/fieldcompanion-frontend/src/hooks/useFieldTaskSubmissionState.ts](../../apps/fieldcompanion-frontend/src/hooks/useFieldTaskSubmissionState.ts)
- [apps/fieldcompanion-frontend/src/components/SubmissionActivityBanner.tsx](../../apps/fieldcompanion-frontend/src/components/SubmissionActivityBanner.tsx)
- [apps/fieldcompanion-frontend/src/components/SubmissionActivityBanner.test.tsx](../../apps/fieldcompanion-frontend/src/components/SubmissionActivityBanner.test.tsx)
- [apps/fieldcompanion-frontend/src/pages/HomePage.tsx](../../apps/fieldcompanion-frontend/src/pages/HomePage.tsx)
- [docs/products/fieldcompanion/WORKFLOWS.md](../products/fieldcompanion/WORKFLOWS.md)

Verified by focused frontend test:
- `npm test -- --run src/components/SubmissionActivityBanner.test.tsx` in `apps/fieldcompanion-frontend`

### 150. Field Companion workspace launcher now shows available product cards and direct links

Requirement:
- The shared surfaces page should present each available workspace with inbox counts, a direct launch link when configured, and a launch action that routes through the shared product-launch hook.

Evidence:
- [apps/fieldcompanion-frontend/src/pages/SurfacesPage.tsx](../../apps/fieldcompanion-frontend/src/pages/SurfacesPage.tsx)
- [apps/fieldcompanion-frontend/src/pages/SurfacesPage.test.tsx](../../apps/fieldcompanion-frontend/src/pages/SurfacesPage.test.tsx)
- [apps/fieldcompanion-frontend/src/hooks/useFieldCompanionProductLaunch.ts](../../apps/fieldcompanion-frontend/src/hooks/useFieldCompanionProductLaunch.ts)
- [apps/fieldcompanion-frontend/src/lib/fieldInbox.ts](../../apps/fieldcompanion-frontend/src/lib/fieldInbox.ts)
- [docs/products/fieldcompanion/FEATURESET.md](../products/fieldcompanion/FEATURESET.md)

Verified by focused frontend test:
- `npm test -- --run src/pages/SurfacesPage.test.tsx` in `apps/fieldcompanion-frontend`
