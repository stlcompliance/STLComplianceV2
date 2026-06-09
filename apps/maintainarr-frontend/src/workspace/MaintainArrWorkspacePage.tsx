import type { WorkspaceSection } from '../lib/workspaceSection'
import { useMaintainArrWorkspaceState } from './useMaintainArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { OverviewSection } from './sections/OverviewSection'
import { AssetsSection } from './sections/AssetsSection'
import { PmProgramsSection } from './sections/PmProgramsSection'
import { RecallsSection } from './sections/RecallsSection'
import { MetersSection } from './sections/MetersSection'
import { WorkOrdersSection } from './sections/WorkOrdersSection'
import { DefectsSection } from './sections/DefectsSection'
import { InspectionsSection } from './sections/InspectionsSection'
import { InspectionTemplatesSection } from './sections/InspectionTemplatesSection'
import { PartsKitsSection } from './sections/PartsKitsSection'
import { HistorySection } from './sections/HistorySection'
import { DowntimeSection } from './sections/DowntimeSection'
import { SettingsSection } from './sections/SettingsSection'

export function MaintainArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useMaintainArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'overview' ? <OverviewSection state={state} /> : null}
      {section === 'assets' ? <AssetsSection state={state} /> : null}
      {section === 'pm-programs' ? <PmProgramsSection state={state} /> : null}
      {section === 'recalls' ? <RecallsSection state={state} /> : null}
      {section === 'meters' ? <MetersSection state={state} /> : null}
      {section === 'work-orders' ? <WorkOrdersSection state={state} /> : null}
      {section === 'defects' ? <DefectsSection state={state} /> : null}
      {section === 'inspections' ? <InspectionsSection state={state} /> : null}
      {section === 'inspection-templates' ? <InspectionTemplatesSection state={state} /> : null}
      {section === 'parts-kits' ? <PartsKitsSection state={state} /> : null}
      {section === 'history' ? <HistorySection state={state} /> : null}
      {section === 'downtime' ? <DowntimeSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
