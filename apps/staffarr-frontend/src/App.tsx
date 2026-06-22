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
import { TimekeepingPage } from './pages/timekeeping/TimekeepingPage'
import { TimesheetDetailPage } from './pages/timekeeping/TimesheetDetailPage'
import { RestrictionsPage } from './pages/restrictions/RestrictionsPage'
import { ReadinessPage } from './pages/readiness/ReadinessPage'
import { MePage } from './pages/me/MePage'
import { MyTeamPage } from './pages/my-team/MyTeamPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { AuditPackagesPage } from './pages/audit-packages/AuditPackagesPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { EmploymentApplicationsPage } from './pages/employment-applications/EmploymentApplicationsPage'
import { BenefitsCompensationPage } from './pages/benefits-compensation/BenefitsCompensationPage'
import { HrmPage } from './pages/hrm/HrmPage'
import { RecruitingPage } from './pages/recruiting/RecruitingPage'
import { PerformancePage } from './pages/performance/PerformancePage'
import { LaunchPage } from './pages/LaunchPage'
import { RolesPage } from './pages/roles/RolesPage'
import { ImportsPage } from './pages/imports/ImportsPage'

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
            <Route path="/timekeeping" element={<TimekeepingPage />} />
            <Route path="/timekeeping/my-time" element={<TimekeepingPage />} />
            <Route path="/timekeeping/team" element={<TimekeepingPage />} />
            <Route path="/timekeeping/timesheets" element={<TimekeepingPage />} />
            <Route path="/timekeeping/timesheets/:id" element={<TimesheetDetailPage />} />
            <Route path="/timekeeping/exceptions" element={<TimekeepingPage />} />
            <Route path="/timekeeping/corrections" element={<TimekeepingPage />} />
            <Route path="/timekeeping/pay-policies" element={<TimekeepingPage />} />
            <Route path="/timekeeping/pay-codes" element={<TimekeepingPage />} />
            <Route path="/timekeeping/profiles" element={<TimekeepingPage />} />
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
            <Route path="/imports" element={<ImportsPage />} />
            <Route path="/permissions" element={<Navigate to="/roles" replace />} />
            <Route path="/readiness" element={<ReadinessPage />} />
            <Route path="/incidents" element={<IncidentsPage />} />
            <Route path="/restrictions" element={<RestrictionsPage />} />
            <Route path="/incidents/create" element={<IncidentCreatePage />} />
            <Route path="/performance" element={<PerformancePage />} />
            <Route path="/benefits-compensation" element={<BenefitsCompensationPage />} />
            <Route path="/hiring" element={<RecruitingPage />} />
            <Route path="/training-acknowledgements" element={<TrainingAcknowledgementsPage />} />
            <Route path="/certifications" element={<CertificationsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/hrm" element={<HrmPage />} />
            <Route path="/audit-packages" element={<AuditPackagesPage />} />
            <Route path="/settings" element={<SettingsPage />} />
            <Route path="/applications" element={<EmploymentApplicationsPage />} />
            <Route path="/applications/drawer" element={<EmploymentApplicationsPage />} />
            <Route path="/applications/create" element={<EmploymentApplicationsPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
