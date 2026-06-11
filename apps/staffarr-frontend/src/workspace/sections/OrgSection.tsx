import { getErrorMessage } from '@stl/shared-ui'
import { ManagerHierarchyPanel } from '../../components/ManagerHierarchyPanel'
import { OrgHierarchyManager } from '../../components/OrgHierarchyManager'
import { PersonOrgAssignmentsManager } from '../../components/PersonOrgAssignmentsManager'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function OrgSection({ state }: Props) {
  const s = state
  return (
    <>
      {s.selectedPerson ? (
        <PersonOrgAssignmentsManager
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          orgUnits={s.orgUnits}
          assignments={s.assignments}
          isLoading={s.assignmentQuery.isLoading || s.orgUnitsQuery.isLoading}
          isError={s.assignmentQuery.isError || s.orgUnitsQuery.isError}
          readErrorMessage={
            s.assignmentQuery.isError
              ? getErrorMessage(
                  s.assignmentQuery.error,
                  'Failed to load person org assignments.',
                )
              : s.orgUnitsQuery.isError
                ? getErrorMessage(
                    s.orgUnitsQuery.error,
                    'Failed to load org unit options.',
                  )
                : null
          }
          onRetryRead={() => {
            void s.assignmentQuery.refetch()
            void s.orgUnitsQuery.refetch()
          }}
          canManage={s.canManageOrgUnits}
          isSubmitting={
            s.createAssignmentMutation.isPending ||
            s.updateAssignmentMutation.isPending ||
            s.updateAssignmentStatusMutation.isPending
          }
          actionErrorMessage={
            s.assignmentMutationError
              ? getErrorMessage(s.assignmentMutationError, 'Failed to save org assignments.')
              : null
          }
          onCreate={async (request) => {
            await s.createAssignmentMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              request,
            })
          }}
          onUpdate={async (assignmentId, request) => {
            await s.updateAssignmentMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              assignmentId,
              request,
            })
          }}
          onStatusChange={async (assignmentId, request) => {
            await s.updateAssignmentStatusMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              assignmentId,
              request,
            })
          }}
        />
      ) : (
        <p className="text-sm text-slate-400">Select a person on the People page to manage org assignments.</p>
      )}

      {s.selectedPerson ? (
        <ManagerHierarchyPanel
          selectedPersonId={s.selectedPerson.personId}
          selectedPersonDisplayName={s.selectedPerson.displayName}
          people={s.people}
          managerChain={s.managerChain}
          subordinates={s.subordinates}
          selectedSubordinateId={s.selectedSubordinateId}
          selectedSubordinate={s.selectedSubordinateDetail}
          isLoading={s.managerChainQuery.isLoading || s.subordinatesQuery.isLoading}
          isError={s.managerChainQuery.isError || s.subordinatesQuery.isError}
          readErrorMessage={
            s.managerChainQuery.isError
              ? getErrorMessage(
                  s.managerChainQuery.error,
                  'Failed to load manager chain.',
                )
              : s.subordinatesQuery.isError
                ? getErrorMessage(
                    s.subordinatesQuery.error,
                    'Failed to load subordinate hierarchy.',
                  )
                : null
          }
          onRetryRead={() => {
            void s.managerChainQuery.refetch()
            void s.subordinatesQuery.refetch()
          }}
          isLoadingSubordinateDetail={s.subordinateDetailQuery.isLoading}
          isSubordinateDetailError={s.subordinateDetailQuery.isError}
          subordinateDetailErrorMessage={
            s.subordinateDetailQuery.isError
              ? getErrorMessage(
                  s.subordinateDetailQuery.error,
                  'Failed to load subordinate detail.',
                )
              : null
          }
          onRetrySubordinateDetail={() => void s.subordinateDetailQuery.refetch()}
          canManage={s.canManageHierarchy}
          isSubmitting={s.updateManagerMutation.isPending}
          actionErrorMessage={
            s.managerMutationError
              ? getErrorMessage(s.managerMutationError, 'Failed to update manager hierarchy.')
              : null
          }
          onSelectSubordinate={(subordinatePersonId) => s.setSelectedSubordinateId(subordinatePersonId)}
          onUpdateManager={async (managerPersonId) => {
            await s.updateManagerMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              managerPersonId,
            })
          }}
        />
      ) : null}

      <OrgHierarchyManager
        orgUnits={s.orgUnitsAdminQuery.data ?? s.orgUnits}
        peopleOptions={s.people.map((person) => ({
          personId: person.personId,
          displayName: person.displayName,
        }))}
        isLoading={s.orgUnitsAdminQuery.isLoading}
        isError={s.orgUnitsAdminQuery.isError}
        readErrorMessage={
          s.orgUnitsAdminQuery.isError
            ? getErrorMessage(
                s.orgUnitsAdminQuery.error,
                'Failed to load org hierarchy data.',
              )
            : null
        }
        onRetryRead={() => void s.orgUnitsAdminQuery.refetch()}
        canManage={s.canManageOrgUnits}
        isSubmitting={
          s.createOrgUnitMutation.isPending ||
          s.updateOrgUnitMutation.isPending ||
          s.updateOrgUnitStatusMutation.isPending ||
          s.restoreOrgUnitMutation.isPending
        }
        actionErrorMessage={
          s.orgMutationError ? getErrorMessage(s.orgMutationError, 'Failed to update org hierarchy.') : null
        }
        onCreate={async (payload) => {
          await s.createOrgUnitMutation.mutateAsync(payload)
        }}
        onUpdate={async (orgUnitId, request) => {
          await s.updateOrgUnitMutation.mutateAsync({ orgUnitId, request })
        }}
        onStatusChange={async (orgUnitId, status) => {
          await s.updateOrgUnitStatusMutation.mutateAsync({ orgUnitId, status })
        }}
        onRestore={async (orgUnitId) => {
          await s.restoreOrgUnitMutation.mutateAsync({ orgUnitId, status: 'active' })
        }}
      />
    </>
  )
}
