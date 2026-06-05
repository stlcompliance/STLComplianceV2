import { useSupplyArrWorkspaceState } from '../../workspace/useSupplyArrWorkspaceState'
import { ReceivingSection } from '../../workspace/sections/ReceivingSection'

export function ReceivingPage() {
  const state = useSupplyArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return <ReceivingSection state={state} />
}
