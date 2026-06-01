import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { AdminPage } from './pages/admin/AdminPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { CertificationsPage } from './pages/certifications/CertificationsPage'
import { IncidentsPage } from './pages/incidents/IncidentsPage'
import { TrainingAcknowledgementsPage } from './pages/training-acknowledgements/TrainingAcknowledgementsPage'
import { OrgPage } from './pages/org/OrgPage'
import { PeoplePage } from './pages/people/PeoplePage'
import { PermissionsPage } from './pages/permissions/PermissionsPage'
import { ReadinessPage } from './pages/readiness/ReadinessPage'
import { MePage } from './pages/me/MePage'
import { MyTeamPage } from './pages/my-team/MyTeamPage'
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
      <BrowserRouter>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/people" replace />} />
            <Route path="/me" element={<MePage />} />
            <Route path="/my-team" element={<MyTeamPage />} />
            <Route path="/people" element={<PeoplePage />} />
            <Route path="/people/drawer" element={<PeoplePage />} />
            <Route path="/people/create" element={<PeoplePage />} />
            <Route path="/people/onboarding-blocked" element={<PeoplePage />} />
            <Route path="/org" element={<OrgPage />} />
            <Route path="/permissions" element={<PermissionsPage />} />
            <Route path="/readiness" element={<ReadinessPage />} />
            <Route path="/incidents" element={<IncidentsPage />} />
            <Route path="/training-acknowledgements" element={<TrainingAcknowledgementsPage />} />
            <Route path="/certifications" element={<CertificationsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}


