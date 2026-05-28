import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'

import { AssetsPage } from './pages/assets/AssetsPage'

import { DefectsPage } from './pages/defects/DefectsPage'

import { HistoryPage } from './pages/history/HistoryPage'
import { ReportsPage } from './pages/reports/ReportsPage'

import { InspectionTemplatesPage } from './pages/inspection-templates/InspectionTemplatesPage'

import { InspectionsPage } from './pages/inspections/InspectionsPage'

import { MetersPage } from './pages/meters/MetersPage'

import { OverviewPage } from './pages/overview/OverviewPage'

import { PmProgramsPage } from './pages/pm-programs/PmProgramsPage'

import { SettingsPage } from './pages/settings/SettingsPage'

import { WorkOrdersPage } from './pages/work-orders/WorkOrdersPage'

import { LaunchPage } from './pages/LaunchPage'

import { WorkOrderWorkspacePage } from './pages/WorkOrderWorkspacePage'



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

          <Route element={<ProductWorkspaceLayout />}>

            <Route index element={<Navigate to="/overview" replace />} />

            <Route path="/overview" element={<OverviewPage />} />

            <Route path="/assets" element={<AssetsPage />} />

            <Route path="/pm-programs" element={<PmProgramsPage />} />

            <Route path="/meters" element={<MetersPage />} />

            <Route path="/work-orders" element={<WorkOrdersPage />} />

            <Route path="/work-orders/:workOrderId" element={<WorkOrderWorkspacePage />} />

            <Route path="/defects" element={<DefectsPage />} />

            <Route path="/inspections" element={<InspectionsPage />} />

            <Route path="/inspection-templates" element={<InspectionTemplatesPage />} />

            <Route path="/history" element={<HistoryPage />} />

            <Route path="/reports" element={<ReportsPage />} />

            <Route path="/settings" element={<SettingsPage />} />

          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />

        </Routes>

      </BrowserRouter>

    </QueryClientProvider>

  )

}


