import { getErrorMessage } from '@stl/shared-ui'
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
          rollupMembersReadErrorMessage={
            s.readinessRollupMembersQuery.isError
              ? getErrorMessage(
                  s.readinessRollupMembersQuery.error,
                  'Failed to load readiness rollup members.',
                )
              : null
          }
          onSelectPerson={s.setSelectedPersonId}
          isLoading={s.teamReadinessRollupsQuery.isLoading || s.siteReadinessRollupsQuery.isLoading}
          readErrorMessage={
            s.teamReadinessRollupsQuery.isError
              ? getErrorMessage(
                  s.teamReadinessRollupsQuery.error,
                  'Failed to load team readiness rollups.',
                )
              : s.siteReadinessRollupsQuery.isError
                ? getErrorMessage(
                    s.siteReadinessRollupsQuery.error,
                    'Failed to load site readiness rollups.',
                  )
                : null
          }
          onRetryRead={() => {
            void s.teamReadinessRollupsQuery.refetch()
            void s.siteReadinessRollupsQuery.refetch()
          }}
          onRetryRollupMembersRead={() => void s.readinessRollupMembersQuery.refetch()}
        />
      ) : null}

      {s.selectedPerson ? (
        <ReadinessPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          readiness={s.personReadinessQuery.data ?? null}
          isLoading={s.personReadinessQuery.isLoading}
          isError={s.personReadinessQuery.isError}
          readErrorMessage={
            s.personReadinessQuery.isError
              ? getErrorMessage(
                  s.personReadinessQuery.error,
                  'Failed to load readiness status for this person.',
                )
              : null
          }
          onRetryRead={() => void s.personReadinessQuery.refetch()}
          canOverride={s.canOverridePersonReadiness}
          isSubmittingOverride={
            s.grantReadinessOverrideMutation.isPending || s.clearReadinessOverrideMutation.isPending
          }
          overrideErrorMessage={
            s.readinessOverrideMutationError
              ? getErrorMessage(s.readinessOverrideMutationError, 'Failed to update readiness override.')
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
