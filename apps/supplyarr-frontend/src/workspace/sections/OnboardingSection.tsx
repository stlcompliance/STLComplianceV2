import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { SupplierOnboardingPanel } from '../../components/SupplierOnboardingPanel'
import { SupplyReadinessCheckPanel } from '../../components/SupplyReadinessCheckPanel'
import { SupplierRestrictionsPanel } from '../../components/SupplierRestrictionsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function uniqueSuppliers(state: SupplyArrWorkspaceState) {
  return state.supplierDirectory
}

export function OnboardingSection({ state: s }: Props) {
  const suppliers = uniqueSuppliers(s)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplierOnboardingPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        canReview={s.canApprovePr}
        onboardableSuppliers={suppliers}
      />
      <SupplyReadinessCheckPanel
        accessToken={s.accessToken}
        canRead={s.canReadSupplyReadiness}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
      />
      <SupplierRestrictionsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        restrictableSuppliers={suppliers}
      />
      <ApprovalRemindersPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-slate-50">Qualification flow</h2>
        <p className="mt-1 text-sm text-slate-400">
          Track supplier onboarding packets, document posture, approval state, and procurement holds.
          Compliance evidence is handled separately and interpreted by Compliance Core.
        </p>
      </section>
    </div>
  )
}
