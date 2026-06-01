import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { ProgramsPage } from './pages/programs/ProgramsPage'
import { AssignmentsPage } from './pages/assignments/AssignmentsPage'
import { RemediationPage } from './pages/remediation/RemediationPage'
import { CitationsPage } from './pages/citations/CitationsPage'
import { RulePacksPage } from './pages/rule-packs/RulePacksPage'
import { QualificationsPage } from './pages/qualifications/QualificationsPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { AssignmentWorkspacePage } from './pages/AssignmentWorkspacePage'
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
            <Route index element={<Navigate to="/programs" replace />} />
            <Route path="/programs" element={<ProgramsPage />} />
            <Route path="/assignments" element={<AssignmentsPage />} />
            <Route path="/assignments/manual" element={<AssignmentsPage />} />
            <Route path="/assignments/queue" element={<AssignmentsPage />} />
            <Route path="/assignments/evaluation" element={<AssignmentsPage />} />
            <Route
              path="/assignments/:assignmentId/evidence"
              element={<AssignmentWorkspacePage focus="evidence" />}
            />
            <Route path="/assignments/:assignmentId" element={<AssignmentWorkspacePage />} />
            <Route path="/remediation" element={<RemediationPage />} />
            <Route path="/citations" element={<CitationsPage />} />
            <Route path="/rule-packs" element={<RulePacksPage />} />
            <Route path="/qualifications" element={<QualificationsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

