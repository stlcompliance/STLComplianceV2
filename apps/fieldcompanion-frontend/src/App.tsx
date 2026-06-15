import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { HomePage } from './pages/HomePage'
import { LaunchPage } from './pages/LaunchPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { OfflineQueuePage } from './pages/OfflineQueuePage'
import { ProfilePage } from './pages/ProfilePage'
import { ReportPage } from './pages/ReportPage'
import { ScanPage } from './pages/ScanPage'
import { SurfacesPage } from './pages/SurfacesPage'

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
            <Route path="/inbox" element={<HomePage />} />
            <Route path="/tasks" element={<HomePage />} />
            <Route path="/scan" element={<ScanPage />} />
            <Route path="/capture" element={<ScanPage />} />
            <Route path="/report" element={<ReportPage />} />
            <Route path="/surfaces" element={<SurfacesPage />} />
            <Route path="/offline-queue" element={<OfflineQueuePage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/notifications" element={<NotificationsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
