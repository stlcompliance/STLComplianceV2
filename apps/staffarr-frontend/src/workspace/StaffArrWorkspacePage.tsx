import type { WorkspaceSection } from '../lib/workspaceSection'
import { useStaffArrWorkspaceState } from './useStaffArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { PeopleSection } from './sections/PeopleSection'
import { OrgSection } from './sections/OrgSection'
import { PermissionsSection } from './sections/PermissionsSection'
import { ReadinessSection } from './sections/ReadinessSection'
import { IncidentsSection } from './sections/IncidentsSection'
import { CertificationsSection } from './sections/CertificationsSection'
import { AdminSection } from './sections/AdminSection'

export function StaffArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useStaffArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'people' ? <PeopleSection state={state} /> : null}
      {section === 'org' ? <OrgSection state={state} /> : null}
      {section === 'permissions' ? <PermissionsSection state={state} /> : null}
      {section === 'readiness' ? <ReadinessSection state={state} /> : null}
      {section === 'incidents' ? <IncidentsSection state={state} /> : null}
      {section === 'certifications' ? <CertificationsSection state={state} /> : null}
      {section === 'admin' ? <AdminSection state={state} /> : null}
    </WorkspaceShell>
  )
}
