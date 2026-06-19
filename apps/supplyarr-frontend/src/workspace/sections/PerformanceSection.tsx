import { PartsInventoryReportsPanel } from '../../components/PartsInventoryReportsPanel'
import { PurchasingReportsPanel } from '../../components/PurchasingReportsPanel'
import { VendorReportsPanel } from '../../components/VendorReportsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function PerformanceSection({ state: s }: Props) {
  const canReadVendorReports = s.canReadParties
  const canReadPurchasingReports = s.canReadParties || s.canReadSupplyReadiness
  const canReadInventoryReports = s.canReadSupplyReadiness

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <VendorReportsPanel
        accessToken={s.accessToken}
        canRead={canReadVendorReports}
        canExport={false}
      />
      <PurchasingReportsPanel
        accessToken={s.accessToken}
        canRead={canReadPurchasingReports}
        canExport={false}
      />
      <PartsInventoryReportsPanel
        accessToken={s.accessToken}
        canRead={canReadInventoryReports}
        canExport={false}
      />

      <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-white">Performance lens</h2>
        <p className="mt-1 text-sm text-slate-400">
          Supplier performance is evaluated through vendor scorecards, purchasing trend reports,
          and inventory coverage signals. This surface stays read-only and does not alter source
          records.
        </p>
      </section>
    </div>
  )
}
