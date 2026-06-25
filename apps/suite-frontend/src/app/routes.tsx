import { lazy, type ComponentType, type ReactNode, Suspense } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from '../components/RequireAuth'
import { RequirePlatformAdmin } from '../components/RequirePlatformAdmin'
import { RouteLoadingState } from '../components/RouteLoadingState'
import { AppShellLayout } from '../layouts/AppShellLayout'

function createLazyPage<TProps extends object = Record<string, never>>(
  loader: () => Promise<Record<string, ComponentType<TProps>>>,
  exportName: string,
) {
  return lazy(async () => {
    const module = await loader()
    const Component = module[exportName]

    if (!Component) {
      throw new Error(`Lazy page export "${exportName}" was not found.`)
    }

    return { default: Component }
  })
}

function routePage(element: ReactNode, options: { fullScreen?: boolean } = {}) {
  return (
    <Suspense fallback={<RouteLoadingState fullScreen={options.fullScreen} />}>{element}</Suspense>
  )
}

const HomePage = createLazyPage(() => import('../pages/HomePage'), 'HomePage')
const LoginPage = createLazyPage(() => import('../pages/LoginPage'), 'LoginPage')
const ForgotPasswordPage = createLazyPage(
  () => import('../pages/ForgotPasswordPage'),
  'ForgotPasswordPage',
)
const ResetPasswordPage = createLazyPage(
  () => import('../pages/ResetPasswordPage'),
  'ResetPasswordPage',
)
const SmartImportPage = createLazyPage(() => import('../pages/SmartImportPage'), 'SmartImportPage')
const UserPreferencesPage = createLazyPage(
  () => import('../pages/UserPreferencesPage'),
  'UserPreferencesPage',
)
const ProductShellLayout = createLazyPage(
  () => import('../layouts/ProductShellLayout'),
  'ProductShellLayout',
)
const ProductSurfacePage = createLazyPage(
  () => import('../pages/ProductSurfacePage'),
  'ProductSurfacePage',
)
const PlatformAdminLayout = createLazyPage(
  () => import('../layouts/PlatformAdminLayout'),
  'PlatformAdminLayout',
)
const LaunchDiagnosticsPage = createLazyPage(
  () => import('../pages/platform-admin/LaunchDiagnosticsPage'),
  'LaunchDiagnosticsPage',
)
const PlatformAdminDashboardPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformAdminDashboardPage'),
  'PlatformAdminDashboardPage',
)
const ProductOverviewPage = createLazyPage(
  () => import('../pages/platform-admin/ProductOverviewPage'),
  'ProductOverviewPage',
)
const DatabaseNukePage = createLazyPage(
  () => import('../pages/platform-admin/DatabaseNukePage'),
  'DatabaseNukePage',
)
const PlatformAuditExportPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformAuditExportPage'),
  'PlatformAuditExportPage',
)
const PlatformStatusPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformStatusPage'),
  'PlatformStatusPage',
)
const CallbackAllowlistPage = createLazyPage(
  () => import('../pages/platform-admin/CallbackAllowlistPage'),
  'CallbackAllowlistPage',
)
const ServiceTokenCleanupPage = createLazyPage(
  () => import('../pages/platform-admin/ServiceTokenCleanupPage'),
  'ServiceTokenCleanupPage',
)
const TenantLifecyclePage = createLazyPage(
  () => import('../pages/platform-admin/TenantLifecyclePage'),
  'TenantLifecyclePage',
)
const PlatformLifecyclePage = createLazyPage(
  () => import('../pages/platform-admin/PlatformLifecyclePage'),
  'PlatformLifecyclePage',
)
const PlatformOutboxPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformOutboxPage'),
  'PlatformOutboxPage',
)
const PlatformSessionSettingsPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformSessionSettingsPage'),
  'PlatformSessionSettingsPage',
)
const PlatformWorkerHealthPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformWorkerHealthPage'),
  'PlatformWorkerHealthPage',
)
const TenantOverviewPage = createLazyPage(
  () => import('../pages/platform-admin/TenantOverviewPage'),
  'TenantOverviewPage',
)
const HybridDataPlanePage = createLazyPage(
  () => import('../pages/platform-admin/HybridDataPlanePage'),
  'HybridDataPlanePage',
)
const PlatformIntegrationsPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformIntegrationsPage'),
  'PlatformIntegrationsPage',
)
const PlatformUsersPage = createLazyPage(
  () => import('../pages/platform-admin/PlatformUsersPage'),
  'PlatformUsersPage',
)
const ReferenceDataPage = createLazyPage(
  () => import('../pages/platform-admin/ReferenceDataPage'),
  'ReferenceDataPage',
)

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={routePage(<LoginPage />, { fullScreen: true })} />
        <Route
          path="/forgot-password"
          element={routePage(<ForgotPasswordPage />, { fullScreen: true })}
        />
        <Route
          path="/reset-password"
          element={routePage(<ResetPasswordPage />, { fullScreen: true })}
        />
        <Route element={<RequireAuth />}>
          <Route element={<AppShellLayout />}>
            <Route path="/" element={<Navigate to="/app" replace />} />
            <Route path="/app" element={routePage(<HomePage />)} />
            <Route path="/app/preferences" element={<Navigate to="/app/nexarr/preferences" replace />} />
            <Route path="/app/:productKey/preferences" element={routePage(<UserPreferencesPage />)} />
            <Route path="/app/imports" element={routePage(<SmartImportPage />)} />
            <Route element={<RequirePlatformAdmin />}>
              <Route path="/app/platform-admin" element={routePage(<PlatformAdminLayout />)}>
                <Route index element={routePage(<PlatformAdminDashboardPage />)} />
                <Route path="launch" element={routePage(<LaunchDiagnosticsPage />)} />
                <Route path="tenants" element={routePage(<TenantOverviewPage />)} />
                <Route path="users" element={routePage(<PlatformUsersPage />)} />
                <Route path="products" element={routePage(<ProductOverviewPage />)} />
                <Route path="database-nuke" element={routePage(<DatabaseNukePage />)} />
                <Route path="reference-data" element={routePage(<ReferenceDataPage />)} />
                <Route
                  path="dataset-inputs"
                  element={<Navigate to="/app/platform-admin/reference-data" replace />}
                />
                <Route path="callback-allowlist" element={routePage(<CallbackAllowlistPage />)} />
                <Route path="status" element={routePage(<PlatformStatusPage />)} />
                <Route path="data-plane" element={routePage(<HybridDataPlanePage />)} />
                <Route path="integrations" element={routePage(<PlatformIntegrationsPage />)} />
                <Route path="audit-export" element={routePage(<PlatformAuditExportPage />)} />
                <Route path="sessions" element={routePage(<PlatformSessionSettingsPage />)} />
                <Route path="lifecycle" element={routePage(<PlatformLifecyclePage />)} />
                <Route path="platform-outbox" element={routePage(<PlatformOutboxPage />)} />
                <Route path="orchestration" element={routePage(<PlatformWorkerHealthPage />)} />
                <Route path="service-tokens" element={routePage(<ServiceTokenCleanupPage />)} />
                <Route path="access" element={<Navigate to="/app/platform-admin" replace />} />
                <Route path="tenant-lifecycle" element={routePage(<TenantLifecyclePage />)} />
              </Route>
            </Route>
            <Route path="/app/:productKey" element={routePage(<ProductShellLayout />)}>
              <Route index element={routePage(<ProductSurfacePage />)} />
              <Route path=":surfaceKey/*" element={routePage(<ProductSurfacePage />)} />
            </Route>
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/app" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
