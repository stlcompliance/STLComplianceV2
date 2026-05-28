import type { WorkspaceSection } from '../lib/workspaceSection'
import { useSupplyArrWorkspaceState } from './useSupplyArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { PartiesSection } from './sections/PartiesSection'
import { CatalogSection } from './sections/CatalogSection'
import { InventorySection } from './sections/InventorySection'
import { PurchasingSection } from './sections/PurchasingSection'
import { ReceivingSection } from './sections/ReceivingSection'
import { PricingSection } from './sections/PricingSection'
import { PlanningSection } from './sections/PlanningSection'
import { SettingsSection } from './sections/SettingsSection'

export function SupplyArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useSupplyArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-slate-400">{state.loadingMessage}</p>

  return (
    <WorkspaceShell section={section} me={state.me} apiError={state.apiError}>
      {section === 'parties' ? <PartiesSection state={state} /> : null}
      {section === 'catalog' ? <CatalogSection state={state} /> : null}
      {section === 'inventory' ? <InventorySection state={state} /> : null}
      {section === 'purchasing' ? <PurchasingSection state={state} /> : null}
      {section === 'receiving' ? <ReceivingSection state={state} /> : null}
      {section === 'pricing' ? <PricingSection state={state} /> : null}
      {section === 'planning' ? <PlanningSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
