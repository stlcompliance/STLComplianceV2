import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { MyTrainingPage } from './pages/my-training/MyTrainingPage'
import { CatalogPage } from './pages/catalog/CatalogPage'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { ProgramsPage } from './pages/programs/ProgramsPage'
import { AssignmentsPage } from './pages/assignments/AssignmentsPage'
import { InstructorPage } from './pages/instructor/InstructorPage'
import { EvaluatorPage } from './pages/evaluator/EvaluatorPage'
import { RemediationPage } from './pages/remediation/RemediationPage'
import { CitationsPage } from './pages/citations/CitationsPage'
import { RulePacksPage } from './pages/rule-packs/RulePacksPage'
import { MatrixPage } from './pages/matrix/MatrixPage'
import { CertificatesPage } from './pages/certificates/CertificatesPage'
import { QualificationsPage } from './pages/qualifications/QualificationsPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { SettingsPage } from './pages/settings/SettingsPage'
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
      <BrowserRouter basename={import.meta.env.VITE_ROUTER_BASENAME}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/my-training" replace />} />
            <Route path="/my-training" element={<MyTrainingPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/catalog" element={<CatalogPage />} />
            <Route path="/programs" element={<ProgramsPage />} />
            <Route path="/programs/drawer" element={<ProgramsPage />} />
            <Route path="/programs/details" element={<ProgramsPage />} />
            <Route path="/programs/create" element={<ProgramsPage />} />
            <Route path="/assignments" element={<AssignmentsPage />} />
            <Route path="/course-player" element={<AssignmentsPage />} />
            <Route path="/assignments/manual" element={<AssignmentsPage />} />
            <Route path="/assignments/queue" element={<AssignmentsPage />} />
            <Route path="/assignments/evaluation" element={<AssignmentsPage />} />
            <Route path="/instructor" element={<InstructorPage />} />
            <Route path="/evaluator" element={<EvaluatorPage />} />
            <Route
              path="/assignments/:assignmentId/evidence"
              element={<AssignmentWorkspacePage focus="evidence" />}
            />
            <Route path="/assignments/:assignmentId" element={<AssignmentWorkspacePage />} />
            <Route path="/remediation" element={<RemediationPage />} />
            <Route path="/citations" element={<CitationsPage />} />
            <Route path="/rule-packs" element={<RulePacksPage />} />
            <Route path="/rule-packs/drawer" element={<RulePacksPage />} />
            <Route path="/rule-packs/details" element={<RulePacksPage />} />
            <Route path="/rule-packs/create" element={<RulePacksPage />} />
            <Route path="/matrix" element={<MatrixPage />} />
            <Route path="/certificates" element={<CertificatesPage />} />
            <Route path="/qualifications" element={<QualificationsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/my-training" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

