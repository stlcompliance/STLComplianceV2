import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { AvailabilityPage } from './pages/availability/AvailabilityPage'
import { CalendarPage } from './pages/calendar/CalendarPage'
import { DispatchPage } from './pages/dispatch/DispatchPage'
import { CustomerPortalPage } from './pages/customer-portal/CustomerPortalPage'
import { DriverPortalPage } from './pages/driver-portal/DriverPortalPage'
import { RoutesPage } from './pages/routes/RoutesPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { TripsPage } from './pages/trips/TripsPage'
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
      <BrowserRouter basename={import.meta.env.VITE_ROUTER_BASENAME}>
        <Routes>
          <Route path="/launch" element={<LaunchPage />} />
          <Route path="/auth/nexarr/callback" element={<LaunchPage />} />
          <Route element={<ProductWorkspaceLayout />}>
            <Route index element={<Navigate to="/dispatch" replace />} />
            <Route path="/dispatch" element={<DispatchPage />} />
            <Route path="/driver-portal" element={<DriverPortalPage />} />
            <Route path="/customer-portal" element={<CustomerPortalPage />} />
            <Route path="/trips" element={<TripsPage />} />
            <Route path="/trips/drawer" element={<TripsPage />} />
            <Route path="/trips/details" element={<TripsPage />} />
            <Route path="/trips/create" element={<TripsPage />} />
            <Route path="/trips/:tripId" element={<TripWorkspacePage />} />
            <Route path="/routes" element={<RoutesPage />} />
            <Route path="/routes/drawer" element={<RoutesPage />} />
            <Route path="/routes/details" element={<RoutesPage />} />
            <Route path="/routes/create" element={<RoutesPage />} />
            <Route path="/availability" element={<AvailabilityPage />} />
            <Route path="/calendar" element={<CalendarPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}


