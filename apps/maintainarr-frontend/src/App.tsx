import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'

import { AssetsPage } from './pages/assets/AssetsPage'
import { AssetCreatePage } from './pages/assets/AssetCreatePage'
import { AssetProfilePage } from './pages/assets/AssetProfilePage'

import { DefectsPage } from './pages/defects/DefectsPage'

import { HistoryPage } from './pages/history/HistoryPage'
import { DowntimePage } from './pages/downtime/DowntimePage'
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
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />

          <Route element={<ProductWorkspaceLayout />}>

            <Route index element={<Navigate to="/overview" replace />} />

            <Route path="/overview" element={<OverviewPage />} />

            <Route path="/assets" element={<AssetsPage />} />
            <Route path="/assets/drawer" element={<AssetsPage />} />
            <Route path="/assets/details" element={<Navigate to="/assets/drawer" replace />} />
            <Route path="/assets/create" element={<Navigate to="/assets/new" replace />} />
            <Route path="/assets/new" element={<AssetCreatePage />} />
            <Route path="/assets/:assetId" element={<AssetProfilePage />} />
            <Route path="/assets/:assetId/edit" element={<AssetProfilePage editModeDefault />} />

            <Route path="/pm-programs" element={<PmProgramsPage />} />
            <Route path="/pm-programs/drawer" element={<PmProgramsPage />} />
            <Route path="/pm-programs/details" element={<PmProgramsPage />} />
            <Route path="/pm-programs/create" element={<PmProgramsPage />} />

            <Route path="/meters" element={<MetersPage />} />
            <Route path="/meters/drawer" element={<MetersPage />} />
            <Route path="/meters/details" element={<MetersPage />} />
            <Route path="/meters/create" element={<MetersPage />} />

            <Route path="/work-orders" element={<WorkOrdersPage />} />
            <Route path="/work-orders/drawer" element={<WorkOrdersPage />} />
            <Route path="/work-orders/details" element={<WorkOrdersPage />} />
            <Route path="/work-orders/create" element={<WorkOrdersPage />} />

            <Route path="/work-orders/:workOrderId" element={<WorkOrderWorkspacePage />} />

            <Route path="/defects" element={<DefectsPage />} />
            <Route path="/defects/drawer" element={<DefectsPage />} />
            <Route path="/defects/details" element={<DefectsPage />} />
            <Route path="/defects/create" element={<DefectsPage />} />

            <Route path="/inspections" element={<InspectionsPage />} />
            <Route path="/inspections/drawer" element={<InspectionsPage />} />
            <Route path="/inspections/details" element={<InspectionsPage />} />
            <Route path="/inspections/create" element={<InspectionsPage />} />

            <Route path="/inspection-templates" element={<InspectionTemplatesPage />} />
            <Route path="/inspection-templates/drawer" element={<InspectionTemplatesPage />} />
            <Route path="/inspection-templates/details" element={<InspectionTemplatesPage />} />
            <Route path="/inspection-templates/create" element={<InspectionTemplatesPage />} />

            <Route path="/history" element={<HistoryPage />} />

            <Route path="/downtime" element={<DowntimePage />} />

            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/reports/compliance" element={<ReportsPage />} />
            <Route path="/reports/executive" element={<ReportsPage />} />
            <Route path="/reports/maintenance" element={<ReportsPage />} />
            <Route path="/reports/exports" element={<ReportsPage />} />

            <Route path="/settings" element={<SettingsPage />} />

          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />

        </Routes>

      </BrowserRouter>

    </QueryClientProvider>

  )

}



