import { StaffArrApiError } from '../../api/client'
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
          canManage={s.canManageOrgUnits}
          isSubmitting={
            s.createAssignmentMutation.isPending ||
            s.updateAssignmentMutation.isPending ||
            s.updateAssignmentStatusMutation.isPending
          }
          errorMessage={
            s.assignmentMutationError instanceof StaffArrApiError
              ? s.assignmentMutationError.body || s.assignmentMutationError.message
              : null
          }
          onCreate={async (payload) => {
            await s.createAssignmentMutation.mutateAsync({ personId: s.selectedPerson!.personId, ...payload })
          }}
          onUpdate={async (assignmentId, payload) => {
            await s.updateAssignmentMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              assignmentId,
              ...payload,
            })
          }}
          onStatusChange={async (assignmentId, status) => {
            await s.updateAssignmentStatusMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              assignmentId,
              status,
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
          selectedSubordinate={s.selectedSubordinateDetail}
          canManage={s.canManageHierarchy}
          isSubmitting={s.updateManagerMutation.isPending}
          errorMessage={
            s.managerMutationError instanceof StaffArrApiError
              ? s.managerMutationError.body || s.managerMutationError.message
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
        orgUnits={s.orgUnits}
        canManage={s.canManageOrgUnits}
        isSubmitting={
          s.createOrgUnitMutation.isPending ||
          s.updateOrgUnitMutation.isPending ||
          s.updateOrgUnitStatusMutation.isPending
        }
        errorMessage={
          s.orgMutationError instanceof StaffArrApiError ? s.orgMutationError.body || s.orgMutationError.message : null
        }
        onCreate={async (payload) => {
          await s.createOrgUnitMutation.mutateAsync(payload)
        }}
        onUpdate={async (orgUnitId, payload) => {
          await s.updateOrgUnitMutation.mutateAsync({ orgUnitId, ...payload })
        }}
        onStatusChange={async (orgUnitId, status) => {
          await s.updateOrgUnitStatusMutation.mutateAsync({ orgUnitId, status })
        }}
      />
    </>
  )
}
