import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { RegistryPage } from './pages/registry/RegistryPage'
import { MappingsPage } from './pages/mappings/MappingsPage'
import { FindingsPage } from './pages/findings/FindingsPage'
import { EvaluationPage } from './pages/evaluation/EvaluationPage'
import { FactSourcesPage } from './pages/fact-sources/FactSourcesPage'
import { OperatorPage } from './pages/operator/OperatorPage'
import { AdminPage } from './pages/admin/AdminPage'
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
            <Route index element={<Navigate to="/registry" replace />} />
            <Route path="/registry" element={<RegistryPage />} />
            <Route path="/registry/drawer" element={<RegistryPage />} />
            <Route path="/registry/details" element={<RegistryPage />} />
            <Route path="/registry/create" element={<RegistryPage />} />
            <Route path="/mappings" element={<MappingsPage />} />
            <Route path="/findings" element={<FindingsPage />} />
            <Route path="/evaluation" element={<EvaluationPage />} />
            <Route path="/fact-sources" element={<FactSourcesPage />} />
            <Route path="/operator" element={<OperatorPage />} />
            <Route path="/admin" element={<AdminPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

