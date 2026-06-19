import type { WorkspaceSection } from '../lib/workspaceSection'
import { useTrainArrWorkspaceState } from './useTrainArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { DashboardSection } from './sections/DashboardSection'
import { CatalogSection } from './sections/CatalogSection'
import { ProgramsSection } from './sections/ProgramsSection'
import { AssignmentsSection } from './sections/AssignmentsSection'
import { RemediationSection } from './sections/RemediationSection'
import { CitationsSection } from './sections/CitationsSection'
import { RulePacksSection } from './sections/RulePacksSection'
import { MatrixSection } from './sections/MatrixSection'
import { ReportsSection } from './sections/ReportsSection'
import { QualificationsSection } from './sections/QualificationsSection'
import { SettingsSection } from './sections/SettingsSection'

export function TrainArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useTrainArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
      <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'my-training' ? <DashboardSection state={state} /> : null}
      {section === 'dashboard' ? <DashboardSection state={state} /> : null}
      {section === 'catalog' ? <CatalogSection state={state} /> : null}
      {section === 'programs' ? <ProgramsSection state={state} /> : null}
      {section === 'assignments' ? <AssignmentsSection state={state} /> : null}
      {section === 'instructor' ? <AssignmentsSection state={state} /> : null}
      {section === 'evaluator' ? <AssignmentsSection state={state} /> : null}
      {section === 'remediation' ? <RemediationSection state={state} /> : null}
      {section === 'citations' ? <CitationsSection state={state} /> : null}
      {section === 'rule-packs' ? <RulePacksSection state={state} /> : null}
      {section === 'matrix' ? <MatrixSection state={state} /> : null}
      {section === 'certificates' ? <QualificationsSection state={state} /> : null}
      {section === 'qualifications' ? <QualificationsSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
