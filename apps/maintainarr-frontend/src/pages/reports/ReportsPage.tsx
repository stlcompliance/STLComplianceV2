import { useMaintainArrWorkspaceState } from '../../workspace/useMaintainArrWorkspaceState'
import { ReportsSection } from '../../workspace/sections/ReportsSection'

export function ReportsPage() {
  const state = useMaintainArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return <ReportsSection state={state} />
}
