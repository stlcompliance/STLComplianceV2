import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
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
    </div>
  )
}
