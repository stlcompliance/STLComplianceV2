import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from '../components/RequireAuth'
import { RequirePlatformAdmin } from '../components/RequirePlatformAdmin'
import { AppShellLayout } from '../layouts/AppShellLayout'
import { PlatformAdminLayout } from '../layouts/PlatformAdminLayout'
import { HomePage } from '../pages/HomePage'
import { ForgotPasswordPage } from '../pages/ForgotPasswordPage'
import { LoginPage } from '../pages/LoginPage'
import { ResetPasswordPage } from '../pages/ResetPasswordPage'
import { ProductShellLayout } from '../layouts/ProductShellLayout'
import { ProductSurfacePage } from '../pages/ProductSurfacePage'
import { LaunchDiagnosticsPage } from '../pages/platform-admin/LaunchDiagnosticsPage'
import { PlatformAdminDashboardPage } from '../pages/platform-admin/PlatformAdminDashboardPage'
import { ProductOverviewPage } from '../pages/platform-admin/ProductOverviewPage'
import { PlatformAuditExportPage } from '../pages/platform-admin/PlatformAuditExportPage'
import { ServiceTokenCleanupPage } from '../pages/platform-admin/ServiceTokenCleanupPage'
import { EntitlementReconciliationPage } from '../pages/platform-admin/EntitlementReconciliationPage'
import { TenantLifecyclePage } from '../pages/platform-admin/TenantLifecyclePage'
import { PlatformLifecyclePage } from '../pages/platform-admin/PlatformLifecyclePage'
import { PlatformOutboxPage } from '../pages/platform-admin/PlatformOutboxPage'
import { PlatformSessionSettingsPage } from '../pages/platform-admin/PlatformSessionSettingsPage'
import { PlatformWorkerHealthPage } from '../pages/platform-admin/PlatformWorkerHealthPage'
import { TenantOverviewPage } from '../pages/platform-admin/TenantOverviewPage'
import { HybridDataPlanePage } from '../pages/platform-admin/HybridDataPlanePage'
import { PlatformUsersPage } from '../pages/platform-admin/PlatformUsersPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route element={<RequireAuth />}>
          <Route element={<AppShellLayout />}>
            <Route path="/" element={<Navigate to="/app" replace />} />
            <Route path="/app" element={<HomePage />} />
            <Route element={<RequirePlatformAdmin />}>
              <Route path="/app/platform-admin" element={<PlatformAdminLayout />}>
                <Route index element={<PlatformAdminDashboardPage />} />
                <Route path="launch" element={<LaunchDiagnosticsPage />} />
                <Route path="tenants" element={<TenantOverviewPage />} />
                <Route path="users" element={<PlatformUsersPage />} />
                <Route path="products" element={<ProductOverviewPage />} />
                <Route path="data-plane" element={<HybridDataPlanePage />} />
                <Route path="audit-export" element={<PlatformAuditExportPage />} />
                <Route path="sessions" element={<PlatformSessionSettingsPage />} />
                <Route path="lifecycle" element={<PlatformLifecyclePage />} />
                <Route path="platform-outbox" element={<PlatformOutboxPage />} />
                <Route path="orchestration" element={<PlatformWorkerHealthPage />} />
                <Route path="service-tokens" element={<ServiceTokenCleanupPage />} />
                <Route path="entitlements" element={<EntitlementReconciliationPage />} />
                <Route path="tenant-lifecycle" element={<TenantLifecyclePage />} />
              </Route>
            </Route>
            <Route path="/app/:productKey" element={<ProductShellLayout />}>
              <Route index element={<ProductSurfacePage />} />
              <Route path=":surfaceKey" element={<ProductSurfacePage />} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/app" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
