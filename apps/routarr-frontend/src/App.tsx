import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { ProductWorkspaceLayout } from './layouts/ProductWorkspaceLayout'
import { AvailabilityPage } from './pages/availability/AvailabilityPage'
import { CalendarPage } from './pages/calendar/CalendarPage'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { DispatchPlansPage } from './pages/dispatch-plans/DispatchPlansPage'
import { DispatchPage } from './pages/dispatch/DispatchPage'
import { DockAppointmentsPage } from './pages/dock-appointments/DockAppointmentsPage'
import { CustomerPortalPage } from './pages/customer-portal/CustomerPortalPage'
import { ExceptionsPage } from './pages/exceptions/ExceptionsPage'
import { DriverPortalPage } from './pages/driver-portal/DriverPortalPage'
import { LoadVisibilityPage } from './pages/load-visibility/LoadVisibilityPage'
import { ReportsPage } from './pages/reports/ReportsPage'
import { ProofReviewPage } from './pages/proof-review/ProofReviewPage'
import { RoutePlannerPage } from './pages/route-planner/RoutePlannerPage'
import { TransportationDemandsPage } from './pages/transportation-demands/TransportationDemandsPage'
import { RoutesPage } from './pages/routes/RoutesPage'
import { SettingsPage } from './pages/settings/SettingsPage'
import { StopsPage } from './pages/stops/StopsPage'
import { TripsPage } from './pages/trips/TripsPage'
import { LaunchPage } from './pages/LaunchPage'
import { TripWorkspacePage } from './pages/TripWorkspacePage'
import { ValidationBlockersPage } from './pages/validation-blockers/ValidationBlockersPage'

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
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/dispatch" element={<DispatchPage />} />
            <Route path="/dispatch-board" element={<DispatchPage />} />
            <Route path="/dispatch-plans" element={<DispatchPlansPage />} />
            <Route path="/transportation-demands" element={<TransportationDemandsPage />} />
            <Route path="/route-planner" element={<RoutePlannerPage />} />
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
            <Route path="/stops" element={<StopsPage />} />
            <Route path="/exceptions" element={<ExceptionsPage />} />
            <Route path="/reports" element={<ReportsPage />} />
            <Route path="/proof-review" element={<ProofReviewPage />} />
            <Route path="/dock-appointments" element={<DockAppointmentsPage />} />
            <Route path="/load-visibility" element={<LoadVisibilityPage />} />
            <Route path="/validation-blockers" element={<ValidationBlockersPage />} />
            <Route path="/availability" element={<AvailabilityPage />} />
            <Route path="/calendar" element={<CalendarPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
