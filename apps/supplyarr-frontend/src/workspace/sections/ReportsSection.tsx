import { ComplianceReportsPanel } from '../../components/ComplianceReportsPanel'
import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
import { PurchasingReportsPanel } from '../../components/PurchasingReportsPanel'
import { SupplierReportsPanel } from '../../components/SupplierReportsPanel'
import {
  canExportComplianceReports,
  canExportPartsInventoryReports,
  canExportPurchasingReports,
  canExportSupplierReports,
  canReadComplianceReports,
  canReadPartsInventoryReports,
  canReadPurchasingReports,
  canReadSupplierReports,
} from '../../auth/sessionStorage'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReportsSection({ state: s }: Props) {
  const roleKey = s.me.tenantRoleKey
  const isPlatformAdmin = s.me.isPlatformAdmin

  const supplierCanRead = canReadSupplierReports(roleKey, isPlatformAdmin)
  const supplierCanExport = canExportSupplierReports(roleKey, isPlatformAdmin)
  const complianceCanRead = canReadComplianceReports(roleKey, isPlatformAdmin)
  const complianceCanExport = canExportComplianceReports(roleKey, isPlatformAdmin)
  const purchasingCanRead = canReadPurchasingReports(roleKey, isPlatformAdmin)
  const purchasingCanExport = canExportPurchasingReports(roleKey, isPlatformAdmin)
  const inventoryCanRead = canReadPartsInventoryReports(roleKey, isPlatformAdmin)
  const inventoryCanExport = canExportPartsInventoryReports(roleKey, isPlatformAdmin)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplierReportsPanel accessToken={s.accessToken} canRead={supplierCanRead} canExport={supplierCanExport} />
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
