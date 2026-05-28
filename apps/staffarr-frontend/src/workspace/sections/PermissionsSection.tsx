import { StaffArrApiError } from '../../api/client'
import { PermissionProjectionTimelinePanel } from '../../components/PermissionProjectionTimelinePanel'
import { RoleTemplateAssignmentPanel } from '../../components/RoleTemplateAssignmentPanel'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function PermissionsSection({ state }: Props) {
  const s = state
  if (!s.selectedPerson) {
    return <p className="text-sm text-slate-400">Select a person on the People page to manage permissions.</p>
  }

  return (
    <>
      <RoleTemplateAssignmentPanel
        personId={s.selectedPerson.personId}
        personDisplayName={s.selectedPerson.displayName}
        orgUnits={s.orgUnits}
        permissionTemplates={s.permissionTemplates}
        roleTemplates={s.roleTemplates}
        roleAssignments={s.roleAssignments}
        canManage={s.canManageHierarchy}
        isSubmitting={
          s.upsertPermissionTemplateMutation.isPending ||
          s.createRoleTemplateMutation.isPending ||
          s.updateRoleTemplateMutation.isPending ||
          s.createRoleAssignmentMutation.isPending ||
          s.updateRoleAssignmentStatusMutation.isPending
        }
        errorMessage={
          s.roleTemplateMutationError instanceof StaffArrApiError
            ? s.roleTemplateMutationError.body || s.roleTemplateMutationError.message
            : null
        }
        onUpsertPermissionTemplate={async (payload) => {
          await s.upsertPermissionTemplateMutation.mutateAsync(payload)
        }}
        onCreateRoleTemplate={async (payload) => {
          await s.createRoleTemplateMutation.mutateAsync(payload)
        }}
        onUpdateRoleTemplateStatus={async (roleTemplateId, status) => {
          const existing = s.roleTemplates.find((role) => role.roleTemplateId === roleTemplateId)
          if (!existing) return
          await s.updateRoleTemplateMutation.mutateAsync({
            roleTemplateId,
            name: existing.name,
            description: existing.description,
            status,
            permissions: existing.permissions.map((mapping) => ({
              permissionTemplateId: mapping.permissionTemplateId,
              scopeType: mapping.scopeType,
              scopeValue: mapping.scopeValue,
            })),
          })
        }}
        onCreateRoleAssignment={async (payload) => {
          await s.createRoleAssignmentMutation.mutateAsync({
            personId: s.selectedPerson!.personId,
            ...payload,
          })
        }}
        onUpdateRoleAssignmentStatus={async (assignmentId, status) => {
          await s.updateRoleAssignmentStatusMutation.mutateAsync({
            personId: s.selectedPerson!.personId,
            assignmentId,
            status,
          })
        }}
      />

      <PermissionProjectionTimelinePanel
        personDisplayName={s.selectedPerson.displayName}
        orgUnits={s.orgUnits}
        projection={s.effectivePermissions}
        timeline={s.permissionHistory}
      />
    </>
  )
}
