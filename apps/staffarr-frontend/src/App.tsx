import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { AdminPage } from './pages/admin/AdminPage'
import { CertificationsPage } from './pages/certifications/CertificationsPage'
import { IncidentCreatePage } from './pages/incidents/IncidentCreatePage'
import { IncidentsPage } from './pages/incidents/IncidentsPage'
import { TrainingAcknowledgementsPage } from './pages/training-acknowledgements/TrainingAcknowledgementsPage'
import { OrganizationStructurePage } from './pages/organization-structure/OrganizationStructurePage'
import { PeoplePage } from './pages/people/PeoplePage'
import { RestrictionsPage } from './pages/restrictions/RestrictionsPage'
import { ReadinessPage } from './pages/readiness/ReadinessPage'
import { MePage } from './pages/me/MePage'
import { MyTeamPage } from './pages/my-team/MyTeamPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { LaunchPage } from './pages/LaunchPage'
import { RolesPage } from './pages/roles/RolesPage'

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
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/people" replace />} />
            <Route path="/me" element={<MePage />} />
            <Route path="/my-team" element={<MyTeamPage />} />
            <Route path="/people" element={<PeoplePage />} />
            <Route path="/people/drawer" element={<PeoplePage />} />
            <Route path="/people/details" element={<PeoplePage />} />
            <Route path="/people/create" element={<PeoplePage />} />
            <Route path="/organization-structure" element={<OrganizationStructurePage />} />
            <Route path="/org" element={<Navigate to="/organization-structure?tab=organization" replace />} />
            <Route path="/locations" element={<Navigate to="/organization-structure?tab=locations" replace />} />
            <Route path="/setup/organization" element={<Navigate to="/organization-structure?tab=organization" replace />} />
            <Route path="/roles" element={<RolesPage />} />
            <Route path="/roles/new" element={<RolesPage />} />
            <Route path="/roles/:roleId" element={<RolesPage />} />
            <Route path="/roles/:roleId/edit" element={<RolesPage />} />
            <Route path="/staffarr/roles" element={<RolesPage />} />
            <Route path="/staffarr/roles/new" element={<RolesPage />} />
            <Route path="/staffarr/roles/:roleId" element={<RolesPage />} />
            <Route path="/staffarr/roles/:roleId/edit" element={<RolesPage />} />
            <Route path="/permissions" element={<Navigate to="/roles" replace />} />
            <Route path="/readiness" element={<ReadinessPage />} />
            <Route path="/incidents" element={<IncidentsPage />} />
            <Route path="/restrictions" element={<RestrictionsPage />} />
            <Route path="/incidents/create" element={<IncidentCreatePage />} />
            <Route path="/training-acknowledgements" element={<TrainingAcknowledgementsPage />} />
            <Route path="/certifications" element={<CertificationsPage />} />
            <Route path="/reports" element={<Navigate to="/people" replace />} />
            <Route path="/audit-packages" element={<Navigate to="/people" replace />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}


