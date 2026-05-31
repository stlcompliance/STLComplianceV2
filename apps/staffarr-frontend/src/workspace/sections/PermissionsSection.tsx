import { getErrorMessage } from '@stl/shared-ui'
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
        isLoading={
          s.permissionTemplatesQuery.isLoading
          || s.roleTemplatesQuery.isLoading
          || s.roleAssignmentsQuery.isLoading
        }
        isError={
          s.permissionTemplatesQuery.isError
          || s.roleTemplatesQuery.isError
          || s.roleAssignmentsQuery.isError
        }
        readErrorMessage={
          s.permissionTemplatesQuery.isError
            ? getErrorMessage(
                s.permissionTemplatesQuery.error,
                'Failed to load permission templates.',
              )
            : s.roleTemplatesQuery.isError
              ? getErrorMessage(
                  s.roleTemplatesQuery.error,
                  'Failed to load role templates.',
                )
              : s.roleAssignmentsQuery.isError
                ? getErrorMessage(
                    s.roleAssignmentsQuery.error,
                    'Failed to load role assignments.',
                  )
                : null
        }
        onRetryRead={() => {
          void s.permissionTemplatesQuery.refetch()
          void s.roleTemplatesQuery.refetch()
          void s.roleAssignmentsQuery.refetch()
        }}
        canManage={s.canManageHierarchy}
        isSubmitting={
          s.upsertPermissionTemplateMutation.isPending ||
          s.createRoleTemplateMutation.isPending ||
          s.updateRoleTemplateMutation.isPending ||
          s.createRoleAssignmentMutation.isPending ||
          s.updateRoleAssignmentStatusMutation.isPending
        }
        actionErrorMessage={
          s.roleTemplateMutationError
            ? getErrorMessage(s.roleTemplateMutationError, 'Failed to update role templates or assignments.')
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
        isLoading={s.effectivePermissionsQuery.isLoading || s.permissionHistoryQuery.isLoading}
        isError={s.effectivePermissionsQuery.isError || s.permissionHistoryQuery.isError}
        readErrorMessage={
          s.effectivePermissionsQuery.isError
            ? getErrorMessage(
                s.effectivePermissionsQuery.error,
                'Failed to load effective permission projection.',
              )
            : s.permissionHistoryQuery.isError
              ? getErrorMessage(
                  s.permissionHistoryQuery.error,
                  'Failed to load permission history timeline.',
                )
              : null
        }
        onRetryRead={() => {
          void s.effectivePermissionsQuery.refetch()
          void s.permissionHistoryQuery.refetch()
        }}
      />
    </>
  )
}
