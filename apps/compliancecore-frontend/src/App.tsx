import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { RegistryPage } from './pages/registry/RegistryPage'
import { MappingsPage } from './pages/mappings/MappingsPage'
import { FindingsPage } from './pages/findings/FindingsPage'
import { EvaluationPage } from './pages/evaluation/EvaluationPage'
import { TheoreticalSituationPage } from './pages/theoretical-situation/TheoreticalSituationPage'
import { FactSourcesPage } from './pages/fact-sources/FactSourcesPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { ChangeImpactPage } from './pages/change-impact/ChangeImpactPage'
import { ImportsPage } from './pages/imports/ImportsPage'
import { RulePackDiffPage } from './pages/rulepack-diff/RulePackDiffPage'
import { RulePackDetailPage } from './pages/rulepacks/RulePackDetailPage'
import { RequirementDetailPage } from './pages/requirements/RequirementDetailPage'
import { ExceptionExemptionsPage } from './pages/exception-exemptions/ExceptionExemptionsPage'
import { GoverningBodiesPage } from './pages/governing-bodies/GoverningBodiesPage'
import { JurisdictionsPage } from './pages/jurisdictions/JurisdictionsPage'
import { CitationsPage } from './pages/citations/CitationsPage'
import { EvidenceTypesPage } from './pages/evidence-types/EvidenceTypesPage'
import { EvidenceRequirementsPage } from './pages/evidence-requirements/EvidenceRequirementsPage'
import { RetentionRulesPage } from './pages/retention-rules/RetentionRulesPage'
import { EvidenceMappingPage } from './pages/evidence-mapping/EvidenceMappingPage'
import { OperatorPage } from './pages/operator/OperatorPage'
import { AdminPage } from './pages/admin/AdminPage'
import { QuestionnairesPage } from './pages/questionnaires/QuestionnairesPage'
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
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="/overview" element={<DashboardPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/registry" element={<RegistryPage />} />
            <Route path="/registry/drawer" element={<RegistryPage />} />
            <Route path="/registry/details" element={<RegistryPage />} />
            <Route path="/registry/create" element={<RegistryPage />} />
            <Route path="/mappings" element={<MappingsPage />} />
            <Route path="/mappings/coverage" element={<MappingsPage />} />
            <Route path="/mappings/facts" element={<MappingsPage />} />
            <Route path="/mappings/evidence" element={<MappingsPage />} />
            <Route path="/mappings/vocabulary" element={<MappingsPage />} />
            <Route path="/mappings/subjects" element={<MappingsPage />} />
            <Route path="/mappings/outputs" element={<MappingsPage />} />
            <Route path="/findings" element={<FindingsPage />} />
            <Route path="/evaluation" element={<EvaluationPage />} />
            <Route path="/evaluation/recent" element={<EvaluationPage />} />
            <Route path="/evaluation/tester" element={<TheoreticalSituationPage />} />
            <Route path="/evaluation/traces" element={<EvaluationPage />} />
            <Route path="/applicability-logic" element={<EvaluationPage />} />
            <Route path="/applicability-logic-builder" element={<EvaluationPage />} />
            <Route path="/theoretical-situation" element={<TheoreticalSituationPage />} />
            <Route path="/questionnaires" element={<QuestionnairesPage />} />
            <Route path="/fact-sources" element={<FactSourcesPage />} />
            <Route path="/evidence-mapping" element={<EvidenceMappingPage />} />
            <Route path="/imports" element={<ImportsPage />} />
            <Route path="/rulepack-diff" element={<RulePackDiffPage />} />
            <Route path="/rulepacks/detail" element={<RulePackDetailPage />} />
            <Route path="/requirements/detail" element={<RequirementDetailPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/change-impact" element={<ChangeImpactPage />} />
            <Route path="/exception-exemptions" element={<ExceptionExemptionsPage />} />
            <Route path="/operator" element={<OperatorPage />} />
            <Route path="/admin" element={<AdminPage />} />
            <Route path="/governing-bodies" element={<GoverningBodiesPage />} />
            <Route path="/jurisdictions" element={<JurisdictionsPage />} />
            <Route path="/regulation-sources" element={<CitationsPage />} />
            <Route path="/citations" element={<CitationsPage />} />
            <Route path="/rulepacks" element={<RulePackDetailPage />} />
            <Route path="/rulepacks/installed" element={<RulePackDetailPage />} />
            <Route path="/rulepacks/library" element={<RulePackDetailPage />} />
            <Route path="/rulepacks/updates" element={<RulePackDiffPage />} />
            <Route path="/rulepacks/imports" element={<ImportsPage />} />
            <Route path="/requirements" element={<RequirementDetailPage />} />
            <Route path="/evidence-types" element={<EvidenceTypesPage />} />
            <Route path="/evidence-requirements" element={<EvidenceRequirementsPage />} />
            <Route path="/exceptions" element={<ReportsPage />} />
            <Route path="/exemptions" element={<ReportsPage />} />
            <Route path="/waivers" element={<ReportsPage />} />
            <Route path="/retention-rules" element={<RetentionRulesPage />} />
            <Route path="/settings" element={<AdminPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

