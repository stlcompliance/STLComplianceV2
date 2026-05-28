import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
import { PurchasingReportsPanel } from '../../components/PurchasingReportsPanel'
import { VendorReportsPanel } from '../../components/VendorReportsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReportsSection({ state: s }: Props) {
  return (
    <div className="grid gap-6">
      <VendorReportsPanel
        accessToken={s.accessToken}
        canRead={s.canReadVendorReports}
        canExport={s.canExportVendorReports}
      />
      <PartsInventoryReportsPanel
        accessToken={s.accessToken}
        canRead={s.canReadPartsInventoryReports}
        canExport={s.canExportPartsInventoryReports}
      />
      <PurchasingReportsPanel
        accessToken={s.accessToken}
        canRead={s.canReadPurchasingReports}
        canExport={s.canExportPurchasingReports}
      />
      <ComplianceReportsPanel
        accessToken={s.accessToken}
        canRead={s.canReadComplianceReports}
        canExport={s.canExportComplianceReports}
      />
      <AuditHistoryPanel accessToken={s.accessToken} canRead={s.canReadAuditHistory} />
    </div>
  )
}
