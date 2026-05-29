import { StaffArrApiError } from '../../api/client'
import { ReadinessPanel } from '../../components/ReadinessPanel'
import { ReadinessRollupSupervisorPanel } from '../../components/ReadinessRollupSupervisorPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function ReadinessSection({ state }: Props) {
  const s = state
  return (
    <>
      {s.canViewReadinessRollupSummaries ? (
        <ReadinessRollupSupervisorPanel
          teamRollups={s.teamReadinessRollupsQuery.data ?? []}
          siteRollups={s.siteReadinessRollupsQuery.data ?? []}
          siteFilterOrgUnitId={s.readinessRollupSiteFilterId}
          onSiteFilterChange={s.setReadinessRollupSiteFilterId}
          memberReadinessFilter={s.readinessRollupMemberFilter}
          onMemberReadinessFilterChange={s.setReadinessRollupMemberFilter}
          selectedRollup={s.selectedReadinessRollup}
          onSelectRollup={s.setSelectedReadinessRollup}
          rollupMembers={s.readinessRollupMembersQuery.data ?? null}
          rollupMembersLoading={s.readinessRollupMembersQuery.isLoading}
          rollupMembersErrorMessage={
            s.readinessRollupMembersQuery.error instanceof StaffArrApiError
              ? s.readinessRollupMembersQuery.error.body || s.readinessRollupMembersQuery.error.message
              : null
          }
          onSelectPerson={s.setSelectedPersonId}
          isLoading={s.teamReadinessRollupsQuery.isLoading || s.siteReadinessRollupsQuery.isLoading}
          errorMessage={
            s.teamReadinessRollupsQuery.error instanceof StaffArrApiError
              ? s.teamReadinessRollupsQuery.error.body || s.teamReadinessRollupsQuery.error.message
              : s.siteReadinessRollupsQuery.error instanceof StaffArrApiError
                ? s.siteReadinessRollupsQuery.error.body || s.siteReadinessRollupsQuery.error.message
                : null
          }
        />
      ) : null}

      {s.selectedPerson ? (
        <ReadinessPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          readiness={s.personReadinessQuery.data ?? null}
          isLoading={s.personReadinessQuery.isLoading}
          canOverride={s.canOverridePersonReadiness}
          isSubmittingOverride={
            s.grantReadinessOverrideMutation.isPending || s.clearReadinessOverrideMutation.isPending
          }
          overrideErrorMessage={
            s.readinessOverrideMutationError instanceof StaffArrApiError
              ? s.readinessOverrideMutationError.body || s.readinessOverrideMutationError.message
              : null
          }
          onGrantOverride={async (payload) => {
            await s.grantReadinessOverrideMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              ...payload,
            })
          }}
          onClearOverride={async () => {
            await s.clearReadinessOverrideMutation.mutateAsync(s.selectedPerson!.personId)
          }}
        />
      ) : (
        <p className="text-sm text-slate-400">Select a person on the People page to view readiness.</p>
      )}
    </>
  )
}
