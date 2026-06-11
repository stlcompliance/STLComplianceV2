import type { WorkspaceSection } from '../lib/workspaceSection'
import { useRoutArrWorkspaceState } from './useRoutArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { DashboardSection } from './sections/DashboardSection'
import { DispatchSection } from './sections/DispatchSection'
import { DispatchPlansSection } from './sections/DispatchPlansSection'
import { DockAppointmentsSection } from './sections/DockAppointmentsSection'
import { ExceptionsSection } from './sections/ExceptionsSection'
import { ReportsSection } from './sections/ReportsSection'
import { LoadVisibilitySection } from './sections/LoadVisibilitySection'
import { ProofReviewSection } from './sections/ProofReviewSection'
import { RoutePlannerSection } from './sections/RoutePlannerSection'
import { TripsSection } from './sections/TripsSection'
import { RoutesSection } from './sections/RoutesSection'
import { StopsSection } from './sections/StopsSection'
import { AvailabilitySection } from './sections/AvailabilitySection'
import { CalendarSection } from './sections/CalendarSection'
import { SettingsSection } from './sections/SettingsSection'
import { DriverPortalSection } from './sections/DriverPortalSection'
import { ValidationBlockersSection } from './sections/ValidationBlockersSection'

export function RoutArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useRoutArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'dashboard' ? <DashboardSection state={state} /> : null}
      {section === 'dispatch' ? <DispatchSection state={state} /> : null}
      {section === 'dispatch-plans' ? <DispatchPlansSection state={state} /> : null}
      {section === 'route-planner' ? <RoutePlannerSection state={state} /> : null}
      {section === 'driver-portal' ? <DriverPortalSection state={state} /> : null}
      {section === 'trips' ? <TripsSection state={state} /> : null}
      {section === 'routes' ? <RoutesSection state={state} /> : null}
      {section === 'stops' ? <StopsSection state={state} /> : null}
      {section === 'exceptions' ? <ExceptionsSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'proof-review' ? <ProofReviewSection state={state} /> : null}
      {section === 'dock-appointments' ? <DockAppointmentsSection state={state} /> : null}
      {section === 'load-visibility' ? <LoadVisibilitySection state={state} /> : null}
      {section === 'validation-blockers' ? <ValidationBlockersSection state={state} /> : null}
      {section === 'availability' ? <AvailabilitySection state={state} /> : null}
      {section === 'calendar' ? <CalendarSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
