import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { SupplierOnboardingPanel } from '../../components/SupplierOnboardingPanel'
import { SupplyReadinessCheckPanel } from '../../components/SupplyReadinessCheckPanel'
import { VendorRestrictionsPanel } from '../../components/VendorRestrictionsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

function uniqueParties(state: SupplyArrWorkspaceState) {
  const parties = [...state.vendors, ...(state.suppliersQuery.data ?? []), ...(state.dealersQuery.data ?? [])]
  const seen = new Set<string>()
  return parties.filter((party) => {
    if (seen.has(party.partyId)) {
      return false
    }
    seen.add(party.partyId)
    return true
  })
}

export function OnboardingSection({ state: s }: Props) {
  const parties = uniqueParties(s)

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <SupplierOnboardingPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        canReview={s.canApprovePr}
        onboardableParties={parties}
      />
      <SupplyReadinessCheckPanel
        accessToken={s.accessToken}
        canRead={s.canReadSupplyReadiness}
        parts={s.partsQuery.data ?? []}
        vendors={parties}
      />
      <VendorRestrictionsPanel
        accessToken={s.accessToken}
        canManage={s.canManage}
        restrictableParties={parties}
      />
      <ApprovalRemindersPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2">
        <h2 className="text-lg font-semibold text-slate-50">Qualification flow</h2>
        <p className="mt-1 text-sm text-slate-400">
          SupplyArr tracks supplier onboarding packets, document posture, approval state, and
          procurement holds. Compliance evidence remains stored in RecordArr and interpreted by
          Compliance Core.
        </p>
      </section>
    </div>
  )
}
