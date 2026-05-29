import { EntitlementReconciliationSettingsPanel } from '../../components/platform-admin/EntitlementReconciliationSettingsPanel'
import { EntitlementAdminPanel } from '../../components/platform-admin/EntitlementAdminPanel'

export function EntitlementReconciliationPage() {
  return (
    <div className="space-y-6">
      <EntitlementAdminPanel />
      <EntitlementReconciliationSettingsPanel />
    </div>
  )
}
