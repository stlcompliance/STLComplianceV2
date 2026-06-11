import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { CatalogPage } from './pages/catalog/CatalogPage'
import { PartiesPage } from './pages/parties/PartiesPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { PlanningPage } from './pages/planning/PlanningPage'
import { ReadinessPage } from './pages/readiness/ReadinessPage'
import { PricingPage } from './pages/pricing/PricingPage'
import { PurchasingPage } from './pages/purchasing/PurchasingPage'
import { VendorPortalPage } from './pages/vendor-portal/VendorPortalPage'
import { VendorOrderCreatePage } from './pages/vendor-orders/VendorOrderCreatePage'
import { VendorOrderDetailPage } from './pages/vendor-orders/VendorOrderDetailPage'
import { VendorOrderPortalPage } from './pages/vendor-orders/VendorOrderPortalPage'
import { VendorOrdersPage } from './pages/vendor-orders/VendorOrdersPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { LaunchPage } from './pages/LaunchPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter basename={import.meta.env.VITE_ROUTER_BASENAME}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />
          <Route path="/vendor-portal" element={<VendorPortalPage />} />
          <Route path="/vendor-portal/orders/:token" element={<VendorOrderPortalPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/parties" replace />} />
            <Route path="/parties" element={<PartiesPage />} />
            <Route path="/parties/drawer" element={<PartiesPage />} />
            <Route path="/parties/details" element={<PartiesPage />} />
            <Route path="/parties/create" element={<PartiesPage />} />
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/purchasing" element={<PurchasingPage />} />
            <Route path="/purchasing/procurement" element={<PurchasingPage />} />
            <Route path="/purchasing/approvals" element={<PurchasingPage />} />
            <Route path="/purchasing/exceptions" element={<PurchasingPage />} />
            <Route path="/purchasing/vendor-orders" element={<VendorOrdersPage />} />
            <Route path="/purchasing/vendor-orders/create" element={<VendorOrderCreatePage />} />
            <Route path="/purchasing/vendor-orders/:vendorOrderId" element={<VendorOrderDetailPage />} />
            <Route path="/pricing" element={<PricingPage />} />
            <Route path="/planning" element={<PlanningPage />} />
            <Route path="/readiness" element={<ReadinessPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

