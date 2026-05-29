import type { WorkspaceSection } from '../lib/workspaceSection'
import { useComplianceCoreWorkspaceState } from './useComplianceCoreWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { RegistrySection } from './sections/RegistrySection'
import { MappingsSection } from './sections/MappingsSection'
import { FindingsSection } from './sections/FindingsSection'
import { EvaluationSection } from './sections/EvaluationSection'
import { FactSourcesSection } from './sections/FactSourcesSection'
import { OperatorSection } from './sections/OperatorSection'
import { ReportsSection } from './sections/ReportsSection'
import { AdminSection } from './sections/AdminSection'

export function ComplianceCoreWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useComplianceCoreWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'registry' ? <RegistrySection state={state} /> : null}
      {section === 'mappings' ? <MappingsSection state={state} /> : null}
      {section === 'findings' ? <FindingsSection state={state} /> : null}
      {section === 'evaluation' ? <EvaluationSection state={state} /> : null}
      {section === 'fact-sources' ? <FactSourcesSection state={state} /> : null}
      {section === 'operator' ? <OperatorSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'admin' ? <AdminSection state={state} /> : null}
    </WorkspaceShell>
  )
}
