import { AuditHistoryPanel } from '../../components/AuditHistoryPanel'
import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { ErpExportsPanel } from '../../components/ErpExportsPanel'
import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
import { PurchasingReportsPanel } from '../../components/PurchasingReportsPanel'
import { VendorReportsPanel } from '../../components/VendorReportsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReportsSection({ state: s }: Props) {
  const showReportsWorkspace =
    s.canReadVendorReports ||
    s.canExportVendorReports ||
    s.canReadPartsInventoryReports ||
    s.canExportPartsInventoryReports ||
    s.canReadPurchasingReports ||
    s.canExportPurchasingReports ||
    s.canReadComplianceReports ||
    s.canExportComplianceReports ||
    s.canReadAuditHistory

  if (!showReportsWorkspace) {
    return null
  }

  return (
    <div className="grid gap-6" data-testid="supplyarr-reports-workspace">
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

      <ErpExportsPanel
        accessToken={s.accessToken}
        canExport={s.canExportPurchasingReports || s.canExportComplianceReports}
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
