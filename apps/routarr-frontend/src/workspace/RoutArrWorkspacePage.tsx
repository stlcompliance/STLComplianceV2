import type { WorkspaceSection } from '../lib/workspaceSection'
import { useRoutArrWorkspaceState } from './useRoutArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { DispatchSection } from './sections/DispatchSection'
import { TripsSection } from './sections/TripsSection'
import { RoutesSection } from './sections/RoutesSection'
import { AvailabilitySection } from './sections/AvailabilitySection'
import { CalendarSection } from './sections/CalendarSection'
import { SettingsSection } from './sections/SettingsSection'
import { DriverPortalSection } from './sections/DriverPortalSection'
import { ReportsSection } from './sections/ReportsSection'

export function RoutArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useRoutArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'dispatch' ? <DispatchSection state={state} /> : null}
      {section === 'driver-portal' ? <DriverPortalSection state={state} /> : null}
      {section === 'trips' ? <TripsSection state={state} /> : null}
      {section === 'routes' ? <RoutesSection state={state} /> : null}
      {section === 'availability' ? <AvailabilitySection state={state} /> : null}
      {section === 'calendar' ? <CalendarSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
