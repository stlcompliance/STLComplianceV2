import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { CatalogPage } from './pages/catalog/CatalogPage'
import { ContractsPage } from './pages/contracts/ContractsPage'
import { CorrectiveActionsPage } from './pages/corrective-actions/CorrectiveActionsPage'
import { DocumentsPage } from './pages/documents/DocumentsPage'
import { OnboardingPage } from './pages/onboarding/OnboardingPage'
import { PerformancePage } from './pages/performance/PerformancePage'
import { PurchaseOrdersPage } from './pages/purchase-orders/PurchaseOrdersPage'
import { QuotesPage } from './pages/quotes/QuotesPage'
import { RfqsPage } from './pages/rfqs/RfqsPage'
import { RiskPage } from './pages/risk/RiskPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { SupplierPortalPage } from './pages/supplier-portal/SupplierPortalPage'
import { SuppliersPage } from './pages/suppliers/SuppliersPage'
import { SupplierQuotePortalPage } from './pages/supplier-portal/SupplierQuotePortalPage'
import { SupplierOrderCreatePage } from './pages/supplier-orders/SupplierOrderCreatePage'
import { SupplierOrderDetailPage } from './pages/supplier-orders/SupplierOrderDetailPage'
import { SupplierOrderPortalPage } from './pages/supplier-orders/SupplierOrderPortalPage'
import { SupplierOrdersPage } from './pages/supplier-orders/SupplierOrdersPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { ImportsPage } from './pages/imports/ImportsPage'
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
          <Route path="/supplier-quote-portal" element={<SupplierQuotePortalPage />} />
          <Route path="/supplier-order-portal/orders/:token" element={<SupplierOrderPortalPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/suppliers" element={<SuppliersPage />} />
            <Route path="/suppliers/drawer" element={<SuppliersPage />} />
            <Route path="/suppliers/details" element={<SuppliersPage />} />
            <Route path="/suppliers/create" element={<SuppliersPage />} />
            <Route path="/imports" element={<ImportsPage />} />
            <Route path="/onboarding" element={<OnboardingPage />} />
            <Route path="/rfqs" element={<RfqsPage />} />
            <Route path="/quotes" element={<QuotesPage />} />
            <Route path="/purchase-orders" element={<PurchaseOrdersPage />} />
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/contracts" element={<ContractsPage />} />
            <Route path="/documents" element={<DocumentsPage />} />
            <Route path="/performance" element={<PerformancePage />} />
            <Route path="/risk" element={<RiskPage />} />
            <Route path="/corrective-actions" element={<CorrectiveActionsPage />} />
            <Route path="/supplier-portal" element={<SupplierPortalPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/purchasing" element={<PurchaseOrdersPage />} />
            <Route path="/purchasing/procurement" element={<PurchaseOrdersPage />} />
            <Route path="/purchasing/approvals" element={<PurchaseOrdersPage />} />
            <Route path="/purchasing/exceptions" element={<PurchaseOrdersPage />} />
            <Route path="/purchasing/supplier-orders" element={<SupplierOrdersPage />} />
            <Route path="/purchasing/supplier-orders/create" element={<SupplierOrderCreatePage />} />
            <Route path="/purchasing/supplier-orders/:supplierOrderId" element={<SupplierOrderDetailPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
