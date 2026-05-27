import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from '../components/RequireAuth'
import { RequirePlatformAdmin } from '../components/RequirePlatformAdmin'
import { AppShellLayout } from '../layouts/AppShellLayout'
import { PlatformAdminLayout } from '../layouts/PlatformAdminLayout'
import { HomePage } from '../pages/HomePage'
import { LoginPage } from '../pages/LoginPage'
import { ProductHubPage } from '../pages/ProductHubPage'
import { LaunchDiagnosticsPage } from '../pages/platform-admin/LaunchDiagnosticsPage'
import { PlatformAdminDashboardPage } from '../pages/platform-admin/PlatformAdminDashboardPage'
import { ProductOverviewPage } from '../pages/platform-admin/ProductOverviewPage'
import { TenantOverviewPage } from '../pages/platform-admin/TenantOverviewPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<RequireAuth />}>
          <Route element={<AppShellLayout />}>
            <Route path="/" element={<Navigate to="/app" replace />} />
            <Route path="/app" element={<HomePage />} />
            <Route element={<RequirePlatformAdmin />}>
              <Route path="/app/platform-admin" element={<PlatformAdminLayout />}>
                <Route index element={<PlatformAdminDashboardPage />} />
                <Route path="launch" element={<LaunchDiagnosticsPage />} />
                <Route path="tenants" element={<TenantOverviewPage />} />
                <Route path="products" element={<ProductOverviewPage />} />
              </Route>
            </Route>
            <Route path="/app/:productKey" element={<ProductHubPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/app" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
