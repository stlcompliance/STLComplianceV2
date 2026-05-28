import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { HomePage } from './pages/HomePage'
import { LaunchPage } from './pages/LaunchPage'
import { TripWorkspacePage } from './pages/TripWorkspacePage'

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
            <Route path="/" element={<HomePage />} />
            <Route path="/trips/:tripId" element={<TripWorkspacePage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
