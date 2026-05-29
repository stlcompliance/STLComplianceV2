import type { WorkspaceSection } from '../lib/workspaceSection'
import { useTrainArrWorkspaceState } from './useTrainArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { ProgramsSection } from './sections/ProgramsSection'
import { AssignmentsSection } from './sections/AssignmentsSection'
import { RemediationSection } from './sections/RemediationSection'
import { CitationsSection } from './sections/CitationsSection'
import { RulePacksSection } from './sections/RulePacksSection'
import { QualificationsSection } from './sections/QualificationsSection'
import { SettingsSection } from './sections/SettingsSection'
import { ReportsSection } from './sections/ReportsSection'

export function TrainArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useTrainArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'programs' ? <ProgramsSection state={state} /> : null}
      {section === 'assignments' ? <AssignmentsSection state={state} /> : null}
      {section === 'remediation' ? <RemediationSection state={state} /> : null}
      {section === 'citations' ? <CitationsSection state={state} /> : null}
      {section === 'rule-packs' ? <RulePacksSection state={state} /> : null}
      {section === 'qualifications' ? <QualificationsSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
