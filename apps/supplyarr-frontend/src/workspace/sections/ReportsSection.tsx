import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
import { PurchasingReportsPanel } from '../../components/PurchasingReportsPanel'
import { VendorReportsPanel } from '../../components/VendorReportsPanel'
import {
  canExportComplianceReports,
  canExportPartsInventoryReports,
  canExportPurchasingReports,
  canExportVendorReports,
  canReadComplianceReports,
  canReadPartsInventoryReports,
  canReadPurchasingReports,
  canReadVendorReports,
} from '../../auth/sessionStorage'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReportsSection({ state: s }: Props) {
  const roleKey = s.me.tenantRoleKey
  const isPlatformAdmin = s.me.isPlatformAdmin

  const vendorCanRead = canReadVendorReports(roleKey, isPlatformAdmin)
  const vendorCanExport = canExportVendorReports(roleKey, isPlatformAdmin)
  const complianceCanRead = canReadComplianceReports(roleKey, isPlatformAdmin)
  const complianceCanExport = canExportComplianceReports(roleKey, isPlatformAdmin)
  const purchasingCanRead = canReadPurchasingReports(roleKey, isPlatformAdmin)
  const purchasingCanExport = canExportPurchasingReports(roleKey, isPlatformAdmin)
  const inventoryCanRead = canReadPartsInventoryReports(roleKey, isPlatformAdmin)
  const inventoryCanExport = canExportPartsInventoryReports(roleKey, isPlatformAdmin)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <VendorReportsPanel accessToken={s.accessToken} canRead={vendorCanRead} canExport={vendorCanExport} />
      <ComplianceReportsPanel
        accessToken={s.accessToken}
        canRead={complianceCanRead}
        canExport={complianceCanExport}
      />
      <PurchasingReportsPanel
        accessToken={s.accessToken}
        canRead={purchasingCanRead}
        canExport={purchasingCanExport}
      />
      <PartsInventoryReportsPanel
        accessToken={s.accessToken}
        canRead={inventoryCanRead}
        canExport={inventoryCanExport}
      />
    </div>
  )
}
