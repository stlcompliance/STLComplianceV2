import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { CatalogPage } from './pages/catalog/CatalogPage'
import { InventoryPage } from './pages/inventory/InventoryPage'
import { PartiesPage } from './pages/parties/PartiesPage'
import { PlanningPage } from './pages/planning/PlanningPage'
import { ReadinessPage } from './pages/readiness/ReadinessPage'
import { PricingPage } from './pages/pricing/PricingPage'
import { PurchasingPage } from './pages/purchasing/PurchasingPage'
import { ReceivingPage } from './pages/receiving/ReceivingPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { LaunchPage } from './pages/LaunchPage'
import { ReceivingWorkspacePage } from './pages/ReceivingWorkspacePage'

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
      <BrowserRouter>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/parties" replace />} />
            <Route path="/parties" element={<PartiesPage />} />
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/inventory" element={<InventoryPage />} />
            <Route path="/purchasing" element={<PurchasingPage />} />
            <Route path="/receiving" element={<ReceivingPage />} />
            <Route path="/receiving/:receivingReceiptId" element={<ReceivingWorkspacePage />} />
            <Route path="/pricing" element={<PricingPage />} />
            <Route path="/planning" element={<PlanningPage />} />
            <Route path="/readiness" element={<ReadinessPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}


