import { useSupplyArrWorkspaceState } from '../../workspace/useSupplyArrWorkspaceState'
import { InventorySection } from '../../workspace/sections/InventorySection'

export function InventoryPage() {
  const state = useSupplyArrWorkspaceState()

  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return <InventorySection state={state} />
}
