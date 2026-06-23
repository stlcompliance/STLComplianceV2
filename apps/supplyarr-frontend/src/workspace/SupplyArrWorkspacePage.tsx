import type { WorkspaceSection } from '../lib/workspaceSection'
import { useSupplyArrWorkspaceState } from './useSupplyArrWorkspaceState'
import { WorkspaceShell } from './WorkspaceShell'
import { DashboardSection } from './sections/DashboardSection'
import { PartiesSection } from './sections/PartiesSection'
import { ImportsSection } from './sections/ImportsSection'
import { OnboardingSection } from './sections/OnboardingSection'
import { CatalogSection } from './sections/CatalogSection'
import { ReportsSection } from './sections/ReportsSection'
import { PurchasingSection } from './sections/PurchasingSection'
import { ContractsSection } from './sections/ContractsSection'
import { DocumentsSection } from './sections/DocumentsSection'
import { PerformanceSection } from './sections/PerformanceSection'
import { RiskSection } from './sections/RiskSection'
import { CorrectiveActionsSection } from './sections/CorrectiveActionsSection'
import { SupplierPortalSection } from './sections/SupplierPortalSection'
import { SettingsSection } from './sections/SettingsSection'

export function SupplyArrWorkspacePage({ section }: { section: WorkspaceSection }) {
  const state = useSupplyArrWorkspaceState()
  if (state.handoffRedirect) return state.handoffRedirect
  if (!state.ready) return <p className="text-sm text-[var(--color-text-muted)]">{state.loadingMessage}</p>

  return (
    <WorkspaceShell
      section={section}
      me={state.me}
      apiError={state.apiError}
      accessToken={state.accessToken}
      canSearch={state.canUseForgivingSearch}
    >
      {section === 'dashboard' ? <DashboardSection state={state} /> : null}
      {section === 'suppliers' ? <PartiesSection state={state} /> : null}
      {section === 'imports' ? <ImportsSection state={state} /> : null}
      {section === 'onboarding' ? <OnboardingSection state={state} /> : null}
      {section === 'rfqs' ? <PurchasingSection state={state} /> : null}
      {section === 'quotes' ? <PurchasingSection state={state} /> : null}
      {section === 'purchase-orders' ? <PurchasingSection state={state} /> : null}
      {section === 'catalog' ? <CatalogSection state={state} /> : null}
      {section === 'contracts' ? <ContractsSection state={state} /> : null}
      {section === 'documents' ? <DocumentsSection state={state} /> : null}
      {section === 'performance' ? <PerformanceSection state={state} /> : null}
      {section === 'risk' ? <RiskSection state={state} /> : null}
      {section === 'corrective-actions' ? <CorrectiveActionsSection state={state} /> : null}
      {section === 'supplier-portal' ? <SupplierPortalSection state={state} /> : null}
      {section === 'reports' ? <ReportsSection state={state} /> : null}
      {section === 'settings' ? <SettingsSection state={state} /> : null}
    </WorkspaceShell>
  )
}
