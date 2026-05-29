import { type FormEvent, useMemo, useState } from 'react'
import type {
  OrgUnitResponse,
  PermissionTemplateSummaryResponse,
  PersonRoleAssignmentResponse,
  RoleTemplatePermissionInput,
  RoleTemplateResponse,
} from '../api/types'

interface RoleTemplateAssignmentPanelProps {
  personId: string
  personDisplayName: string
  orgUnits: OrgUnitResponse[]
  permissionTemplates: PermissionTemplateSummaryResponse[]
  roleTemplates: RoleTemplateResponse[]
  roleAssignments: PersonRoleAssignmentResponse[]
  canManage: boolean
  isSubmitting: boolean
  errorMessage: string | null
  onUpsertPermissionTemplate: (request: {
    permissionKey: string
    name: string
    description: string | null
  }) => Promise<void>
  onCreateRoleTemplate: (request: {
    roleKey: string
    name: string
    description: string | null
    permissions: RoleTemplatePermissionInput[]
  }) => Promise<void>
  onUpdateRoleTemplateStatus: (roleTemplateId: string, status: 'active' | 'inactive') => Promise<void>
  onCreateRoleAssignment: (request: {
    roleTemplateId: string
    scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
    scopeValue: string | null
  }) => Promise<void>
  onUpdateRoleAssignmentStatus: (assignmentId: string, status: 'active' | 'inactive') => Promise<void>
}

const SCOPE_TYPES = ['tenant', 'site', 'department', 'team', 'position'] as const

export function formatRoleTemplateMutationError(errorMessage: string | null): string | null {
  if (!errorMessage) {
    return null
  }

  const normalized = errorMessage.toLowerCase()
  if (normalized.includes('"status":403') || normalized.includes('forbidden')) {
    return `Forbidden: ${errorMessage}`
  }

  if (normalized.includes('"status":409') || normalized.includes('conflict')) {
    return `Conflict: ${errorMessage}`
  }

  if (normalized.includes('"status":400') || normalized.includes('validation')) {
    return `Validation: ${errorMessage}`
  }

  return errorMessage
}

function scopeLabel(scopeType: string, scopeValue: string | null, orgUnits: OrgUnitResponse[]): string {
  if (scopeType === 'tenant') {
    return 'Tenant-wide'
  }

  return orgUnits.find((unit) => unit.orgUnitId === scopeValue)?.name ?? scopeValue ?? 'Unknown scope'
}

function unitsForScope(orgUnits: OrgUnitResponse[], scopeType: string): OrgUnitResponse[] {
  if (scopeType === 'tenant') {
    return []
  }

  return orgUnits
    .filter((unit) => unit.status === 'active' && unit.unitType.toLowerCase() === scopeType)
    .sort((left, right) => left.name.localeCompare(right.name))
}

export function RoleTemplateAssignmentPanel({
  personId,
  personDisplayName,
  orgUnits,
  permissionTemplates,
  roleTemplates,
  roleAssignments,
  canManage,
  isSubmitting,
  errorMessage,
  onUpsertPermissionTemplate,
  onCreateRoleTemplate,
  onUpdateRoleTemplateStatus,
  onCreateRoleAssignment,
  onUpdateRoleAssignmentStatus,
}: RoleTemplateAssignmentPanelProps) {
  const normalizedError = formatRoleTemplateMutationError(errorMessage)

  const [permissionKey, setPermissionKey] = useState('staffarr.people.read')
  const [permissionName, setPermissionName] = useState('People read')
  const [permissionDescription, setPermissionDescription] = useState('')

  const [roleKey, setRoleKey] = useState('staffarr.viewer')
  const [roleName, setRoleName] = useState('StaffArr Viewer')
  const [roleDescription, setRoleDescription] = useState('')
  const [roleScopeType, setRoleScopeType] = useState<(typeof SCOPE_TYPES)[number]>('tenant')
  const [roleScopeValue, setRoleScopeValue] = useState('')
  const [selectedPermissionIds, setSelectedPermissionIds] = useState<string[]>([])

  const [assignmentRoleTemplateId, setAssignmentRoleTemplateId] = useState('')
  const [assignmentScopeType, setAssignmentScopeType] = useState<(typeof SCOPE_TYPES)[number]>('tenant')
  const [assignmentScopeValue, setAssignmentScopeValue] = useState('')

  const roleScopeUnits = useMemo(() => unitsForScope(orgUnits, roleScopeType), [orgUnits, roleScopeType])
  const assignmentScopeUnits = useMemo(() => unitsForScope(orgUnits, assignmentScopeType), [orgUnits, assignmentScopeType])

  const handlePermissionSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onUpsertPermissionTemplate({
      permissionKey,
      name: permissionName,
      description: permissionDescription || null,
    })
  }

  const handleRoleTemplateSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCreateRoleTemplate({
      roleKey,
      name: roleName,
      description: roleDescription || null,
      permissions: selectedPermissionIds.map((permissionTemplateId) => ({
        permissionTemplateId,
        scopeType: roleScopeType,
        scopeValue: roleScopeType === 'tenant' ? null : roleScopeValue || null,
      })),
    })
  }

  const handleRoleAssignmentSubmit = async (event: FormEvent) => {
    event.preventDefault()
    await onCreateRoleAssignment({
      roleTemplateId: assignmentRoleTemplateId,
      scopeType: assignmentScopeType,
      scopeValue: assignmentScopeType === 'tenant' ? null : assignmentScopeValue || null,
    })
  }

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex items-center justify-between">
        <h2 className="text-sm font-medium text-slate-300">Role and permission templates</h2>
        <span className={`text-xs ${canManage ? 'text-emerald-300' : 'text-slate-500'}`}>
          {canManage ? 'Write enabled' : 'Read only'}
        </span>
      </div>
      <p className="mt-2 text-xs text-slate-500">
        Template and assignment foundations for {personDisplayName} ({personId})
      </p>
      {normalizedError ? <p className="mt-3 text-sm text-red-300">{normalizedError}</p> : null}

      <div className="mt-5 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Permission templates</h3>
          {permissionTemplates.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No permission templates configured yet.</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-700 text-sm">
              {permissionTemplates.map((permission) => (
                <li key={permission.permissionTemplateId} className="py-2">
                  <p className="text-white">{permission.name}</p>
                  <p className="text-xs text-slate-400">{permission.permissionKey}</p>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-300">Role templates</h3>
          {roleTemplates.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No role templates created yet.</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-700 text-sm">
              {roleTemplates.map((role) => (
                <li key={role.roleTemplateId} className="py-2">
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-white">{role.name}</p>
                      <p className="text-xs text-slate-400">
                        {role.roleKey} · {role.permissions.length} permission mappings
                      </p>
                    </div>
                    {canManage ? (
                      <button
                        type="button"
                        className="rounded bg-amber-700 px-2 py-1 text-xs text-white disabled:opacity-50"
                        onClick={() =>
                          onUpdateRoleTemplateStatus(role.roleTemplateId, role.status === 'active' ? 'inactive' : 'active')
                        }
                        disabled={isSubmitting}
                      >
                        {role.status === 'active' ? 'Deactivate' : 'Activate'}
                      </button>
                    ) : null}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-medium text-slate-300">Person role assignments</h3>
        {roleAssignments.length === 0 ? (
          <p className="mt-3 text-sm text-slate-400">No role assignments for this person.</p>
        ) : (
          <ul className="mt-3 divide-y divide-slate-700 text-sm">
            {roleAssignments.map((assignment) => (
              <li key={assignment.assignmentId} className="flex items-center justify-between py-2">
                <div>
                  <p className="text-white">{assignment.roleName}</p>
                  <p className="text-xs text-slate-400">
                    {assignment.roleKey} · {scopeLabel(assignment.scopeType, assignment.scopeValue, orgUnits)}
                  </p>
                </div>
                {canManage ? (
                  <button
                    type="button"
                    className="rounded bg-amber-700 px-2 py-1 text-xs text-white disabled:opacity-50"
                    onClick={() =>
                      onUpdateRoleAssignmentStatus(
                        assignment.assignmentId,
                        assignment.status === 'active' ? 'inactive' : 'active',
                      )
                    }
                    disabled={isSubmitting}
                  >
                    {assignment.status === 'active' ? 'Deactivate' : 'Activate'}
                  </button>
                ) : (
                  <span className="text-xs uppercase tracking-wide text-slate-500">{assignment.status}</span>
                )}
              </li>
            ))}
          </ul>
        )}
      </div>

      {canManage ? (
        <div className="mt-6 grid gap-6 xl:grid-cols-3">
          <form className="space-y-3" onSubmit={handlePermissionSubmit}>
            <h3 className="text-sm font-medium text-slate-300">Upsert permission template</h3>
            <label htmlFor="permission-template-key" className="block text-sm text-slate-300">
              Permission key
              <input
                id="permission-template-key"
                value={permissionKey}
                onChange={(event) => setPermissionKey(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="permission-template-name" className="block text-sm text-slate-300">
              Permission name
              <input
                id="permission-template-name"
                value={permissionName}
                onChange={(event) => setPermissionName(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="permission-template-description" className="block text-sm text-slate-300">
              Description (optional)
              <input
                id="permission-template-description"
                value={permissionDescription}
                onChange={(event) => setPermissionDescription(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              />
            </label>
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Saving…' : 'Save permission template'}
            </button>
          </form>

          <form className="space-y-3" onSubmit={handleRoleTemplateSubmit}>
            <h3 className="text-sm font-medium text-slate-300">Create role template</h3>
            <label htmlFor="role-template-key" className="block text-sm text-slate-300">
              Role key
              <input
                id="role-template-key"
                value={roleKey}
                onChange={(event) => setRoleKey(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="role-template-name" className="block text-sm text-slate-300">
              Role template name
              <input
                id="role-template-name"
                value={roleName}
                onChange={(event) => setRoleName(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              />
            </label>
            <label htmlFor="role-template-description" className="block text-sm text-slate-300">
              Description (optional)
              <input
                id="role-template-description"
                value={roleDescription}
                onChange={(event) => setRoleDescription(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              />
            </label>
            <label htmlFor="role-template-scope-type" className="block text-sm text-slate-300">
              Permission scope type
              <select
                id="role-template-scope-type"
                value={roleScopeType}
                onChange={(event) => setRoleScopeType(event.target.value as (typeof SCOPE_TYPES)[number])}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              >
                {SCOPE_TYPES.map((scopeType) => (
                  <option key={scopeType} value={scopeType}>
                    {scopeType}
                  </option>
                ))}
              </select>
            </label>
            {roleScopeType === 'tenant' ? null : (
              <label htmlFor="role-template-scope-value" className="block text-sm text-slate-300">
                Permission scope unit
                <select
                  id="role-template-scope-value"
                  value={roleScopeValue}
                  onChange={(event) => setRoleScopeValue(event.target.value)}
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  required
                >
                  <option value="">Select scope unit</option>
                  {roleScopeUnits.map((unit) => (
                    <option key={unit.orgUnitId} value={unit.orgUnitId}>
                      {unit.name}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <fieldset className="max-h-32 overflow-y-auto rounded border border-slate-700 p-2">
              <legend className="px-1 text-xs text-slate-400">Permission mappings</legend>
              {permissionTemplates.length === 0 ? (
                <p className="text-xs text-slate-500">Add permission templates first.</p>
              ) : (
                permissionTemplates.map((permission) => {
                  const checkboxId = `role-template-permission-${permission.permissionTemplateId}`
                  return (
                    <label key={permission.permissionTemplateId} htmlFor={checkboxId} className="flex items-center gap-2 text-xs text-slate-200">
                      <input
                        id={checkboxId}
                        type="checkbox"
                        data-testid={checkboxId}
                        checked={selectedPermissionIds.includes(permission.permissionTemplateId)}
                        onChange={(event) => {
                          setSelectedPermissionIds((current) =>
                            event.target.checked
                              ? [...current, permission.permissionTemplateId]
                              : current.filter((id) => id !== permission.permissionTemplateId),
                          )
                        }}
                      />
                      <span>{permission.permissionKey}</span>
                    </label>
                  )
                })
              )}
            </fieldset>
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting || selectedPermissionIds.length === 0}
            >
              {isSubmitting ? 'Saving…' : 'Create role template'}
            </button>
          </form>

          <form className="space-y-3" onSubmit={handleRoleAssignmentSubmit}>
            <h3 className="text-sm font-medium text-slate-300">Assign role to person</h3>
            <label htmlFor="role-assignment-template" className="block text-sm text-slate-300">
              Role template
              <select
                id="role-assignment-template"
                value={assignmentRoleTemplateId}
                onChange={(event) => setAssignmentRoleTemplateId(event.target.value)}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                required
              >
                <option value="">Select role template</option>
                {roleTemplates
                  .filter((role) => role.status === 'active')
                  .map((role) => (
                    <option key={role.roleTemplateId} value={role.roleTemplateId}>
                      {role.name}
                    </option>
                  ))}
              </select>
            </label>
            <label htmlFor="role-assignment-scope-type" className="block text-sm text-slate-300">
              Assignment scope type
              <select
                id="role-assignment-scope-type"
                value={assignmentScopeType}
                onChange={(event) => setAssignmentScopeType(event.target.value as (typeof SCOPE_TYPES)[number])}
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
              >
                {SCOPE_TYPES.map((scopeType) => (
                  <option key={scopeType} value={scopeType}>
                    {scopeType}
                  </option>
                ))}
              </select>
            </label>
            {assignmentScopeType === 'tenant' ? null : (
              <label htmlFor="role-assignment-scope-value" className="block text-sm text-slate-300">
                Assignment scope unit
                <select
                  id="role-assignment-scope-value"
                  value={assignmentScopeValue}
                  onChange={(event) => setAssignmentScopeValue(event.target.value)}
                  className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  required
                >
                  <option value="">Select scope unit</option>
                  {assignmentScopeUnits.map((unit) => (
                    <option key={unit.orgUnitId} value={unit.orgUnitId}>
                      {unit.name}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={isSubmitting || !assignmentRoleTemplateId}
            >
              {isSubmitting ? 'Saving…' : 'Create role assignment'}
            </button>
          </form>
        </div>
      ) : (
        <p className="mt-4 text-xs text-slate-500">
          Your role does not include role template or permission assignment write permission.
        </p>
      )}
    </section>
  )
}
