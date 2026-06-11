import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { getErrorMessage } from '@stl/shared-ui'
import {
  archiveStaffRole,
  cloneStaffRole,
  createStaffRole,
  getOrgUnits,
  getPeople,
  getPermissionCatalogs,
  getSessionBootstrap,
  getStaffPersonRoles,
  getStaffRole,
  listLocations,
  listStaffRoles,
  refreshPermissionCatalogs,
  setStaffPersonRoles,
  setStaffRolePermissions,
  setStaffRoleScopes,
  updateStaffRole,
} from '../../api/client'
import { loadSession } from '../../auth/sessionStorage'
import type {
  InternalLocationResponse,
  OrgUnitResponse,
  PermissionCatalogModuleResponse,
  PermissionCatalogPermissionResponse,
  PermissionCatalogResponse,
  SetStaffPersonRoleItemRequest,
  SetStaffRoleScopeItemRequest,
  StaffRoleAssignedPersonResponse,
  StaffRoleDetailResponse,
} from '../../api/types'

type RoleScopeType = SetStaffRoleScopeItemRequest['scopeType']

type RoleDraft = {
  name: string
  description: string
  roleType: 'tenant_role' | 'product_template'
  permissions: Record<string, 'allow' | 'deny'>
  scopes: SetStaffRoleScopeItemRequest[]
  recordSetText: string
}

type AssignmentDraft = {
  personId: string
  assignmentScopeType: RoleScopeType
  assignmentScopeRefId: string
  startsAt: string
  endsAt: string
}

const PRODUCT_ORDER = [
  'staffarr',
  'maintainarr',
  'trainarr',
  'routarr',
  'supplyarr',
  'loadarr',
  'compliancecore',
  'recordarr',
  'reportarr',
  'assurarr',
  'ordarr',
  'customarr',
  'fieldcompanion',
] as const

const FLAG_SCOPE_OPTIONS: Array<{ type: RoleScopeType; label: string; help: string }> = [
  { type: 'tenant', label: 'Entire tenant', help: 'Grant access across the tenant.' },
  { type: 'assigned_assets', label: 'Assigned assets only', help: 'Scope access to assets assigned to the person.' },
  { type: 'own_records', label: 'Own records only', help: 'Limit access to records owned by the person.' },
  { type: 'direct_reports', label: 'Direct reports only', help: 'Limit access to the person’s direct reports.' },
]

const emptyDraft = (): RoleDraft => ({
  name: '',
  description: '',
  roleType: 'tenant_role',
  permissions: {},
  scopes: [{ scopeType: 'tenant', scopeRefId: null, scopeRefSnapshot: 'Entire tenant' }],
  recordSetText: '',
})

const emptyAssignmentDraft = (): AssignmentDraft => ({
  personId: '',
  assignmentScopeType: 'tenant',
  assignmentScopeRefId: '',
  startsAt: '',
  endsAt: '',
})

function sortCatalogs(catalogs: PermissionCatalogResponse[]): PermissionCatalogResponse[] {
  return [...catalogs].sort((left, right) => {
    const leftIndex = PRODUCT_ORDER.indexOf(left.productKey as (typeof PRODUCT_ORDER)[number])
    const rightIndex = PRODUCT_ORDER.indexOf(right.productKey as (typeof PRODUCT_ORDER)[number])
    const normalizedLeft = leftIndex === -1 ? Number.MAX_SAFE_INTEGER : leftIndex
    const normalizedRight = rightIndex === -1 ? Number.MAX_SAFE_INTEGER : rightIndex
    if (normalizedLeft !== normalizedRight) {
      return normalizedLeft - normalizedRight
    }
    return left.productName.localeCompare(right.productName)
  })
}

function toRoleDraft(role: StaffRoleDetailResponse): RoleDraft {
  const permissions = Object.fromEntries(
    role.permissions.map((permission) => [permission.permissionKey, permission.effect]),
  ) as Record<string, 'allow' | 'deny'>
  const scopes = role.scopes
    .filter((scope) => scope.scopeType !== 'record_set')
    .map((scope) => ({
      scopeType: scope.scopeType,
      scopeRefId: scope.scopeRefId,
      scopeRefSnapshot: scope.scopeRefSnapshot,
    }))
  const recordSetText = role.scopes
    .filter((scope) => scope.scopeType === 'record_set')
    .map((scope) => scope.scopeRefSnapshot ?? scope.scopeRefId ?? '')
    .filter(Boolean)
    .join('\n')

  return {
    name: role.name,
    description: role.description ?? '',
    roleType: role.roleType === 'product_template' ? 'product_template' : 'tenant_role',
    permissions,
    scopes: scopes.length > 0 ? scopes : [{ scopeType: 'tenant', scopeRefId: null, scopeRefSnapshot: 'Entire tenant' }],
    recordSetText,
  }
}

function toDateTimeLocalValue(value: string | null): string {
  if (!value) {
    return ''
  }
  return value.slice(0, 16)
}

function fromDateTimeLocalValue(value: string): string | null {
  return value.trim() ? new Date(value).toISOString() : null
}

function hasScope(
  scopes: SetStaffRoleScopeItemRequest[],
  scopeType: RoleScopeType,
  scopeRefId: string | null,
): boolean {
  return scopes.some(
    (scope) =>
      scope.scopeType === scopeType &&
      (scope.scopeRefId ?? null) === (scopeRefId ?? null),
  )
}

function permissionKeyCountForModule(
  module: PermissionCatalogModuleResponse,
  permissions: Record<string, 'allow' | 'deny'>,
): number {
  return module.permissionGroups
    .flatMap((group) => group.permissions)
    .filter((permission) => permissions[permission.key])
    .length
}

function moduleSelectionState(
  module: PermissionCatalogModuleResponse,
  permissions: Record<string, 'allow' | 'deny'>,
): 'Full' | 'Some' | 'None' {
  const all = module.permissionGroups.flatMap((group) => group.permissions).length
  const selected = permissionKeyCountForModule(module, permissions)
  if (selected === 0) {
    return 'None'
  }
  if (selected === all) {
    return 'Full'
  }
  return 'Some'
}

function scopeSummary(role: StaffRoleDetailResponse | null, draft: RoleDraft, isEditMode: boolean): string[] {
  if (!isEditMode && role) {
    return role.scopes.map((scope) => scope.scopeRefSnapshot ?? scope.scopeRefId ?? scope.scopeType)
  }

  const labels = draft.scopes.map((scope) => scope.scopeRefSnapshot ?? scope.scopeRefId ?? scope.scopeType)
  const recordSets = draft.recordSetText
    .split(/[\n,]/)
    .map((entry) => entry.trim())
    .filter(Boolean)
  return [...labels, ...recordSets]
}

export function RolesPage() {
  const session = loadSession()
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const location = useLocation()
  const { roleId } = useParams<{ roleId: string }>()
  const isNew = location.pathname.endsWith('/roles/new')
  const isEditMode = isNew || location.pathname.endsWith('/edit')

  const [draft, setDraft] = useState<RoleDraft>(emptyDraft)
  const [selectedProductKey, setSelectedProductKey] = useState<string>('')
  const [assignmentDraft, setAssignmentDraft] = useState<AssignmentDraft>(emptyAssignmentDraft)

  const sessionQuery = useQuery({
    queryKey: ['staffarr-session-bootstrap', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const rolesQuery = useQuery({
    queryKey: ['staffarr-v1-roles', session?.accessToken],
    queryFn: () => listStaffRoles(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const roleDetailQuery = useQuery({
    queryKey: ['staffarr-v1-role', session?.accessToken, roleId],
    queryFn: () => getStaffRole(session!.accessToken, roleId!),
    enabled: Boolean(session?.accessToken && roleId && !isNew),
  })

  const catalogsQuery = useQuery({
    queryKey: ['staffarr-v1-permission-catalogs', session?.accessToken],
    queryFn: () => getPermissionCatalogs(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const peopleQuery = useQuery({
    queryKey: ['staffarr-role-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-role-org-units', session?.accessToken],
    queryFn: () => getOrgUnits(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const locationsQuery = useQuery({
    queryKey: ['staffarr-role-locations', session?.accessToken],
    queryFn: () => listLocations(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  useEffect(() => {
    if (isNew) {
      setDraft(emptyDraft())
      return
    }

    if (roleDetailQuery.data) {
      setDraft(toRoleDraft(roleDetailQuery.data))
    }
  }, [isNew, roleDetailQuery.data])

  useEffect(() => {
    const roleProductKey = roleDetailQuery.data?.permissions[0]?.productKey
    const firstCatalogProductKey = sortCatalogs(catalogsQuery.data ?? [])[0]?.productKey ?? ''
    if (roleProductKey) {
      setSelectedProductKey(roleProductKey)
      return
    }
    if (!selectedProductKey && firstCatalogProductKey) {
      setSelectedProductKey(firstCatalogProductKey)
    }
  }, [catalogsQuery.data, roleDetailQuery.data, selectedProductKey])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const accessToken = session!.accessToken
      const rolePayload = {
        name: draft.name.trim(),
        description: draft.description.trim() || null,
      }

      let nextRoleId = roleId ?? ''
      if (isNew) {
        const created = await createStaffRole(accessToken, {
          ...rolePayload,
          roleType: draft.roleType,
        })
        nextRoleId = created.roleId
      } else {
        await updateStaffRole(accessToken, roleId!, rolePayload)
        nextRoleId = roleId!
      }

      await setStaffRolePermissions(accessToken, nextRoleId, {
        permissions: Object.entries(draft.permissions).map(([permissionKey, effect]) => ({
          productKey: permissionKey.split('.')[0],
          permissionKey,
          effect,
        })),
      })

      const recordSetScopes = draft.recordSetText
        .split(/[\n,]/)
        .map((entry) => entry.trim())
        .filter(Boolean)
        .map((entry) => ({
          scopeType: 'record_set' as const,
          scopeRefId: entry,
          scopeRefSnapshot: entry,
        }))

      await setStaffRoleScopes(accessToken, nextRoleId, {
        scopes: [...draft.scopes, ...recordSetScopes],
      })

      return nextRoleId
    },
    onSuccess: async (nextRoleId) => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-roles'] })
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-role'] })
      navigate(`/roles/${nextRoleId}`)
    },
  })

  const archiveMutation = useMutation({
    mutationFn: async () => {
      const reason = window.prompt('Archive reason', 'Replaced by a newer role')
      return archiveStaffRole(session!.accessToken, roleId!, { reason })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-roles'] })
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-role'] })
    },
  })

  const cloneMutation = useMutation({
    mutationFn: async () => {
      const proposedName = window.prompt('Clone name', `${draft.name} Copy`)
      if (!proposedName?.trim()) {
        throw new Error('Clone cancelled.')
      }

      return cloneStaffRole(session!.accessToken, roleId!, {
        name: proposedName.trim(),
        description: draft.description.trim() || null,
        roleType: draft.roleType,
      })
    },
    onSuccess: async (clonedRole) => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-roles'] })
      navigate(`/roles/${clonedRole.roleId}/edit`)
    },
  })

  const refreshCatalogMutation = useMutation({
    mutationFn: () => refreshPermissionCatalogs(session!.accessToken),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-permission-catalogs'] })
    },
  })

  const assignPersonMutation = useMutation({
    mutationFn: async () => {
      if (!roleId) {
        throw new Error('Save the role before assigning people.')
      }
      if (!assignmentDraft.personId) {
        throw new Error('Choose a person to assign.')
      }

      const existing = await getStaffPersonRoles(session!.accessToken, assignmentDraft.personId)
      const nextRoles: SetStaffPersonRoleItemRequest[] = existing
        .filter((assignment) => assignment.roleId !== roleId)
        .map((assignment) => ({
          roleId: assignment.roleId,
          assignmentScopeType: assignment.assignmentScopeType,
          assignmentScopeRefId: assignment.assignmentScopeRefId,
          startsAt: assignment.startsAt,
          endsAt: assignment.endsAt,
        }))

      nextRoles.push({
        roleId,
        assignmentScopeType: assignmentDraft.assignmentScopeType,
        assignmentScopeRefId:
          assignmentDraft.assignmentScopeType === 'tenant'
            || assignmentDraft.assignmentScopeType === 'assigned_assets'
            || assignmentDraft.assignmentScopeType === 'own_records'
            || assignmentDraft.assignmentScopeType === 'direct_reports'
            ? null
            : assignmentDraft.assignmentScopeRefId.trim() || null,
        startsAt: fromDateTimeLocalValue(assignmentDraft.startsAt),
        endsAt: fromDateTimeLocalValue(assignmentDraft.endsAt),
      })

      await setStaffPersonRoles(session!.accessToken, assignmentDraft.personId, { roles: nextRoles })
    },
    onSuccess: async () => {
      setAssignmentDraft(emptyAssignmentDraft())
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-role'] })
    },
  })

  const removeAssignmentMutation = useMutation({
    mutationFn: async (assignment: StaffRoleAssignedPersonResponse) => {
      const existing = await getStaffPersonRoles(session!.accessToken, assignment.personId)
      const nextRoles = existing
        .filter((personRole) => personRole.personRoleId !== assignment.personRoleId)
        .map((personRole) => ({
          roleId: personRole.roleId,
          assignmentScopeType: personRole.assignmentScopeType,
          assignmentScopeRefId: personRole.assignmentScopeRefId,
          startsAt: personRole.startsAt,
          endsAt: personRole.endsAt,
        }))
      await setStaffPersonRoles(session!.accessToken, assignment.personId, { roles: nextRoles })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-role'] })
    },
  })

  if (!session?.accessToken) {
    return <p className="px-4 py-6 text-sm text-slate-300">Sign in to manage roles.</p>
  }

  const catalogs = sortCatalogs(catalogsQuery.data ?? [])
  const activeCatalog =
    catalogs.find((catalog) => catalog.productKey === selectedProductKey) ?? catalogs[0] ?? null
  const role = roleDetailQuery.data ?? null
  const apiError = [
    rolesQuery.error,
    roleDetailQuery.error,
    catalogsQuery.error,
    peopleQuery.error,
    orgUnitsQuery.error,
    locationsQuery.error,
    saveMutation.error,
    archiveMutation.error,
    cloneMutation.error,
    refreshCatalogMutation.error,
    assignPersonMutation.error,
    removeAssignmentMutation.error,
  ].find(Boolean)
  const errorMessage = apiError ? getErrorMessage(apiError, 'Failed to load role editor.') : null

  const people = [...(peopleQuery.data ?? [])].sort((left, right) => left.displayName.localeCompare(right.displayName))
  const orgUnits = orgUnitsQuery.data ?? []
  const locations = locationsQuery.data ?? []
  const siteUnits = orgUnits.filter((unit) => unit.unitType === 'site')
  const departmentUnits = orgUnits.filter((unit) => unit.unitType === 'department')
  const teamUnits = orgUnits.filter((unit) => unit.unitType === 'team')
  const positionUnits = orgUnits.filter((unit) => unit.unitType === 'position')

  const isBusy =
    rolesQuery.isLoading ||
    sessionQuery.isLoading ||
    catalogsQuery.isLoading ||
    saveMutation.isPending ||
    archiveMutation.isPending ||
    cloneMutation.isPending ||
    refreshCatalogMutation.isPending ||
    assignPersonMutation.isPending ||
    removeAssignmentMutation.isPending

  const selectedCount = activeCatalog
    ? activeCatalog.modules
        .flatMap((module) => module.permissionGroups)
        .flatMap((group) => group.permissions)
        .filter((permission) => draft.permissions[permission.key])
        .length
    : 0

  const selectedRoleId = roleId ?? ''

  function updateModulePermissions(module: PermissionCatalogModuleResponse, nextState: 'Full' | 'None') {
    const nextPermissions = { ...draft.permissions }
    module.permissionGroups.flatMap((group) => group.permissions).forEach((permission) => {
      if (nextState === 'Full') {
        nextPermissions[permission.key] = nextPermissions[permission.key] ?? 'allow'
      } else {
        delete nextPermissions[permission.key]
      }
    })
    setDraft((current) => ({ ...current, permissions: nextPermissions }))
  }

  function togglePermission(permission: PermissionCatalogPermissionResponse, checked: boolean) {
    setDraft((current) => {
      const permissions = { ...current.permissions }
      if (checked) {
        permissions[permission.key] = permissions[permission.key] ?? 'allow'
      } else {
        delete permissions[permission.key]
      }
      return { ...current, permissions }
    })
  }

  function setPermissionEffect(permissionKey: string, effect: 'allow' | 'deny') {
    setDraft((current) => ({
      ...current,
      permissions: {
        ...current.permissions,
        [permissionKey]: effect,
      },
    }))
  }

  function toggleScope(scopeType: RoleScopeType, scopeRefId: string | null, scopeRefSnapshot: string | null) {
    setDraft((current) => {
      const exists = hasScope(current.scopes, scopeType, scopeRefId)
      if (exists) {
        return {
          ...current,
          scopes: current.scopes.filter(
            (scope) =>
              !(scope.scopeType === scopeType && (scope.scopeRefId ?? null) === (scopeRefId ?? null)),
          ),
        }
      }

      return {
        ...current,
        scopes: [...current.scopes, { scopeType, scopeRefId, scopeRefSnapshot }],
      }
    })
  }

  function assignmentScopeNeedsReference(scopeType: RoleScopeType) {
    return scopeType !== 'tenant'
      && scopeType !== 'assigned_assets'
      && scopeType !== 'own_records'
      && scopeType !== 'direct_reports'
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <header className="mb-6 flex flex-col gap-4 rounded-3xl border border-slate-800 bg-gradient-to-br from-slate-950 via-slate-900 to-slate-950 p-6 shadow-2xl shadow-slate-950/40 md:flex-row md:items-end md:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.3em] text-cyan-300">StaffArr authority</p>
          <h1 className="mt-2 text-3xl font-semibold text-white">Role editor</h1>
          <p className="mt-2 max-w-3xl text-sm text-slate-400">
            Create tenant roles, assign cross-product permissions from live catalogs, scope access with StaffArr org and location data, and assign roles to people.
          </p>
        </div>
        <div className="flex flex-wrap gap-3">
          <button
            type="button"
            onClick={() => refreshCatalogMutation.mutate()}
            className="rounded-full border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-cyan-400 hover:text-white"
          >
            Refresh catalogs
          </button>
          <Link
            to="/roles/new"
            className="rounded-full bg-cyan-400 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-cyan-300"
          >
            New role
          </Link>
        </div>
      </header>

      {errorMessage ? (
        <div className="mb-4 rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
          {errorMessage}
        </div>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[320px_minmax(0,1fr)]">
        <aside className="rounded-3xl border border-slate-800 bg-slate-950/80 p-4 shadow-xl shadow-slate-950/30">
          <div className="mb-4 flex items-center justify-between">
            <div>
              <h2 className="text-lg font-semibold text-white">Roles</h2>
              <p className="text-sm text-slate-400">System templates are read-only and can be cloned.</p>
            </div>
            <span className="rounded-full bg-slate-800 px-2.5 py-1 text-xs text-slate-300">
              {(rolesQuery.data ?? []).length}
            </span>
          </div>

          <div className="space-y-3">
            {(rolesQuery.data ?? []).map((roleItem) => {
              const isActive = selectedRoleId === roleItem.roleId
              return (
                <Link
                  key={roleItem.roleId}
                  to={`/roles/${roleItem.roleId}`}
                  className={`block rounded-2xl border px-4 py-3 transition ${
                    isActive
                      ? 'border-cyan-400 bg-cyan-400/10 text-white'
                      : 'border-slate-800 bg-slate-900/80 text-slate-200 hover:border-slate-700'
                  }`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-medium">{roleItem.name}</p>
                      <p className="mt-1 text-xs uppercase tracking-[0.2em] text-slate-400">
                        {roleItem.roleType.replace('_', ' ')}
                      </p>
                    </div>
                    <span
                      className={`rounded-full px-2 py-1 text-[11px] font-semibold ${
                        roleItem.isArchived
                          ? 'bg-slate-700 text-slate-300'
                          : roleItem.isSystem
                            ? 'bg-amber-400/20 text-amber-200'
                            : 'bg-emerald-400/15 text-emerald-200'
                      }`}
                    >
                      {roleItem.isArchived ? 'Archived' : roleItem.isSystem ? 'System' : 'Tenant'}
                    </span>
                  </div>
                  <p className="mt-3 text-sm text-slate-400">{roleItem.description ?? 'No description yet.'}</p>
                  <div className="mt-3 flex flex-wrap gap-2 text-xs text-slate-300">
                    <span className="rounded-full bg-slate-800 px-2 py-1">{roleItem.permissionCount} permissions</span>
                    <span className="rounded-full bg-slate-800 px-2 py-1">{roleItem.scopeCount} scopes</span>
                    <span className="rounded-full bg-slate-800 px-2 py-1">{roleItem.assignedPersonCount} people</span>
                  </div>
                </Link>
              )
            })}

            {!rolesQuery.isLoading && (rolesQuery.data ?? []).length === 0 ? (
              <p className="rounded-2xl border border-dashed border-slate-800 px-4 py-6 text-sm text-slate-400">
                No roles are available yet.
              </p>
            ) : null}
          </div>
        </aside>

        <section className="space-y-6">
          {!roleId && !isNew ? (
            <div className="rounded-3xl border border-dashed border-slate-700 bg-slate-950/70 px-6 py-10 text-center text-slate-300">
              Choose a role from the list or create a new one to start editing.
            </div>
          ) : (
            <>
              <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-xl shadow-slate-950/30">
                <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-300">
                      {isNew ? 'New role' : isEditMode ? 'Edit role' : 'Role detail'}
                    </p>
                    <h2 className="mt-2 text-2xl font-semibold text-white">
                      {draft.name.trim() || role?.name || 'Untitled role'}
                    </h2>
                    <p className="mt-2 max-w-3xl text-sm text-slate-400">
                      {draft.description.trim() || role?.description || 'Describe when this role should be used.'}
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-3">
                    {!isNew && !isEditMode ? (
                      <Link
                        to={`/roles/${roleId}/edit`}
                        className="rounded-full border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-cyan-400 hover:text-white"
                      >
                        Edit
                      </Link>
                    ) : null}
                    {!isNew ? (
                      <button
                        type="button"
                        onClick={() => cloneMutation.mutate()}
                        className="rounded-full border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-cyan-400 hover:text-white"
                      >
                        Clone
                      </button>
                    ) : null}
                    {!isNew && !role?.isSystem && !role?.isArchived ? (
                      <button
                        type="button"
                        onClick={() => archiveMutation.mutate()}
                        className="rounded-full border border-rose-500/50 px-4 py-2 text-sm font-medium text-rose-200 transition hover:border-rose-400 hover:text-white"
                      >
                        Archive
                      </button>
                    ) : null}
                    {isEditMode ? (
                      <button
                        type="button"
                        onClick={() => saveMutation.mutate()}
                        className="rounded-full bg-cyan-400 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-cyan-300"
                      >
                        Save
                      </button>
                    ) : null}
                    <a
                      href="#role-audit"
                      className="rounded-full border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-cyan-400 hover:text-white"
                    >
                      Audit history
                    </a>
                  </div>
                </div>
              </div>

              <div className="grid gap-6 lg:grid-cols-2">
                <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                  <h3 className="text-lg font-semibold text-white">Basic details</h3>
                  <div className="mt-4 grid gap-4">
                    <label className="grid gap-2 text-sm text-slate-300">
                      <span>Role name</span>
                      <input
                        value={draft.name}
                        disabled={!isEditMode}
                        onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))}
                        className="rounded-2xl border border-slate-700 bg-slate-900 px-4 py-3 text-white outline-none transition focus:border-cyan-400 disabled:opacity-70"
                      />
                    </label>
                    <label className="grid gap-2 text-sm text-slate-300">
                      <span>Description</span>
                      <textarea
                        value={draft.description}
                        disabled={!isEditMode}
                        onChange={(event) => setDraft((current) => ({ ...current, description: event.target.value }))}
                        rows={4}
                        className="rounded-2xl border border-slate-700 bg-slate-900 px-4 py-3 text-white outline-none transition focus:border-cyan-400 disabled:opacity-70"
                      />
                    </label>
                    <label className="grid gap-2 text-sm text-slate-300">
                      <span>Role type</span>
                      <select
                        value={draft.roleType}
                        disabled={!isEditMode || !isNew}
                        onChange={(event) =>
                          setDraft((current) => ({
                            ...current,
                            roleType: event.target.value as RoleDraft['roleType'],
                          }))
                        }
                        className="rounded-2xl border border-slate-700 bg-slate-900 px-4 py-3 text-white outline-none transition focus:border-cyan-400 disabled:opacity-70"
                      >
                        <option value="tenant_role">Tenant role</option>
                        <option value="product_template">Product template</option>
                      </select>
                    </label>
                    {role ? (
                      <div className="flex flex-wrap gap-2 text-xs text-slate-300">
                        <span className="rounded-full bg-slate-800 px-2.5 py-1">
                          {role.isSystem ? 'System template' : 'Editable tenant role'}
                        </span>
                        <span className="rounded-full bg-slate-800 px-2.5 py-1">
                          {role.isArchived ? 'Archived' : 'Active'}
                        </span>
                        <span className="rounded-full bg-slate-800 px-2.5 py-1">
                          Updated {new Date(role.updatedAt).toLocaleString()}
                        </span>
                      </div>
                    ) : null}
                  </div>
                </section>

                <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                  <h3 className="text-lg font-semibold text-white">Product access</h3>
                  <p className="mt-2 text-sm text-slate-400">
                    Products shown here come from the active permission catalog and are filtered by current tenant entitlement.
                  </p>
                  <div className="mt-4 flex flex-wrap gap-2">
                    {catalogs.map((catalog) => (
                      <button
                        key={catalog.productKey}
                        type="button"
                        onClick={() => setSelectedProductKey(catalog.productKey)}
                        className={`rounded-full border px-3 py-2 text-sm transition ${
                          activeCatalog?.productKey === catalog.productKey
                            ? 'border-cyan-400 bg-cyan-400/10 text-white'
                            : 'border-slate-700 bg-slate-900 text-slate-300 hover:border-slate-500'
                        }`}
                      >
                        {catalog.productName}
                      </button>
                    ))}
                  </div>
                  <div className="mt-4 rounded-2xl border border-slate-800 bg-slate-900/80 p-4">
                    <p className="text-sm text-slate-300">
                      {activeCatalog
                        ? `${activeCatalog.productName} exposes ${selectedCount} selected permissions in this role.`
                        : 'Refresh catalogs to load product permission groups.'}
                    </p>
                    {activeCatalog ? (
                      <p className="mt-1 text-xs uppercase tracking-[0.2em] text-slate-500">
                        Catalog version {activeCatalog.version}
                      </p>
                    ) : null}
                  </div>
                </section>
              </div>

              <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <h3 className="text-lg font-semibold text-white">Module permissions</h3>
                    <p className="mt-2 text-sm text-slate-400">
                      Use Full, Some, and None at the module level, then adjust individual permissions when a module needs a custom mix.
                    </p>
                  </div>
                  {activeCatalog ? (
                    <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-300">
                      {activeCatalog.modules.length} modules
                    </span>
                  ) : null}
                </div>

                {!activeCatalog ? (
                  <div className="mt-4 rounded-2xl border border-dashed border-slate-700 px-4 py-8 text-sm text-slate-400">
                    No product catalog is available yet.
                  </div>
                ) : (
                  <div className="mt-6 space-y-5">
                    {activeCatalog.modules.map((module) => {
                      const moduleState = moduleSelectionState(module, draft.permissions)
                      return (
                        <article key={module.key} className="rounded-3xl border border-slate-800 bg-slate-900/70 p-5">
                          <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                            <div>
                              <h4 className="text-base font-semibold text-white">{module.label}</h4>
                              <p className="mt-1 text-sm text-slate-400">
                                {module.description ?? 'Module permissions grouped from the active product catalog.'}
                              </p>
                            </div>
                            <div className="flex flex-wrap gap-2">
                              {(['Full', 'Some', 'None'] as const).map((state) => (
                                <button
                                  key={state}
                                  type="button"
                                  disabled={!isEditMode || state === 'Some'}
                                  onClick={() =>
                                    state === 'Full' || state === 'None'
                                      ? updateModulePermissions(module, state)
                                      : undefined
                                  }
                                  className={`rounded-full border px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.18em] ${
                                    moduleState === state
                                      ? 'border-cyan-400 bg-cyan-400/10 text-white'
                                      : 'border-slate-700 text-slate-300'
                                  } ${state === 'Some' ? 'cursor-default' : 'transition hover:border-slate-500'} disabled:opacity-80`}
                                >
                                  {state}
                                </button>
                              ))}
                            </div>
                          </div>

                          <div className="mt-5 grid gap-4 xl:grid-cols-2">
                            {module.permissionGroups.map((group) => (
                              <div key={group.key} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
                                <div className="mb-3 flex items-center justify-between gap-3">
                                  <div>
                                    <p className="font-medium text-white">{group.label}</p>
                                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">
                                      {permissionKeyCountForModule({ ...module, permissionGroups: [group] }, draft.permissions)} selected
                                    </p>
                                  </div>
                                </div>
                                <div className="space-y-3">
                                  {group.permissions.map((permission) => {
                                    const selected = Boolean(draft.permissions[permission.key])
                                    const missingDependencies = permission.dependsOn.filter(
                                      (dependency) => !draft.permissions[dependency],
                                    )
                                    const conflicts = permission.conflictsWith.filter(
                                      (conflict) => Boolean(draft.permissions[conflict]),
                                    )
                                    return (
                                      <div key={permission.key} className="rounded-2xl border border-slate-800 bg-slate-900/80 p-3">
                                        <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                                          <label className="flex gap-3">
                                            <input
                                              type="checkbox"
                                              checked={selected}
                                              disabled={!isEditMode}
                                              onChange={(event) => togglePermission(permission, event.target.checked)}
                                              className="mt-1 size-4 rounded border-slate-600 bg-slate-950"
                                            />
                                            <div>
                                              <div className="flex flex-wrap items-center gap-2">
                                                <span className="font-medium text-white">{permission.label}</span>
                                                <span
                                                  className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${
                                                    permission.riskLevel === 'critical'
                                                      ? 'bg-rose-500/20 text-rose-200'
                                                      : permission.riskLevel === 'high'
                                                        ? 'bg-amber-500/20 text-amber-200'
                                                        : permission.riskLevel === 'medium'
                                                          ? 'bg-sky-500/20 text-sky-200'
                                                          : 'bg-emerald-500/20 text-emerald-200'
                                                  }`}
                                                >
                                                  {permission.riskLevel}
                                                </span>
                                                {permission.requiresScope ? (
                                                  <span className="rounded-full bg-slate-800 px-2 py-0.5 text-[11px] text-slate-300">
                                                    needs scope
                                                  </span>
                                                ) : null}
                                              </div>
                                              <p className="mt-1 text-sm text-slate-400">
                                                {permission.description ?? permission.key}
                                              </p>
                                              <p className="mt-1 text-[11px] uppercase tracking-[0.18em] text-slate-500">
                                                {permission.key}
                                              </p>
                                            </div>
                                          </label>
                                          {selected ? (
                                            <select
                                              value={draft.permissions[permission.key]}
                                              disabled={!isEditMode}
                                              onChange={(event) =>
                                                setPermissionEffect(
                                                  permission.key,
                                                  event.target.value as 'allow' | 'deny',
                                                )
                                              }
                                              className="rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white outline-none transition focus:border-cyan-400"
                                            >
                                              <option value="allow">Allow</option>
                                              <option value="deny">Deny</option>
                                            </select>
                                          ) : null}
                                        </div>

                                        {missingDependencies.length > 0 ? (
                                          <p className="mt-3 text-xs text-amber-200">
                                            Depends on: {missingDependencies.join(', ')}
                                          </p>
                                        ) : null}
                                        {conflicts.length > 0 ? (
                                          <p className="mt-1 text-xs text-rose-200">
                                            Conflicts with: {conflicts.join(', ')}
                                          </p>
                                        ) : null}
                                      </div>
                                    )
                                  })}
                                </div>
                              </div>
                            ))}
                          </div>
                        </article>
                      )
                    })}
                  </div>
                )}
              </section>

              <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                <h3 className="text-lg font-semibold text-white">Scope and record sets</h3>
                <p className="mt-2 text-sm text-slate-400">
                  StaffArr owns sites, org units, and internal locations. Use these selectors to constrain where the role applies.
                </p>

                <div className="mt-6 space-y-6">
                  <div>
                    <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Scope flags</h4>
                    <div className="mt-3 grid gap-3 md:grid-cols-2">
                      {FLAG_SCOPE_OPTIONS.map((option) => (
                        <label key={option.type} className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                          <div className="flex items-start gap-3">
                            <input
                              type="checkbox"
                              checked={hasScope(draft.scopes, option.type, null)}
                              disabled={!isEditMode}
                              onChange={() => toggleScope(option.type, null, option.label)}
                              className="mt-1 size-4 rounded border-slate-600 bg-slate-950"
                            />
                            <div>
                              <p className="font-medium text-white">{option.label}</p>
                              <p className="mt-1 text-sm text-slate-400">{option.help}</p>
                            </div>
                          </div>
                        </label>
                      ))}
                    </div>
                  </div>

                  <ScopeSelectionGrid
                    title="Sites"
                    items={siteUnits}
                    scopeType="site"
                    scopes={draft.scopes}
                    disabled={!isEditMode}
                    onToggle={toggleScope}
                  />
                  <ScopeSelectionGrid
                    title="Departments"
                    items={departmentUnits}
                    scopeType="department"
                    scopes={draft.scopes}
                    disabled={!isEditMode}
                    onToggle={toggleScope}
                  />
                  <ScopeSelectionGrid
                    title="Teams"
                    items={teamUnits}
                    scopeType="team"
                    scopes={draft.scopes}
                    disabled={!isEditMode}
                    onToggle={toggleScope}
                  />
                  <ScopeSelectionGrid
                    title="Positions"
                    items={positionUnits}
                    scopeType="position"
                    scopes={draft.scopes}
                    disabled={!isEditMode}
                    onToggle={toggleScope}
                  />
                  <LocationSelectionGrid
                    title="Locations"
                    items={locations}
                    scopes={draft.scopes}
                    disabled={!isEditMode}
                    onToggle={toggleScope}
                  />

                  <label className="grid gap-2 text-sm text-slate-300">
                    <span className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Record sets</span>
                    <textarea
                      value={draft.recordSetText}
                      disabled={!isEditMode}
                      onChange={(event) => setDraft((current) => ({ ...current, recordSetText: event.target.value }))}
                      rows={4}
                      placeholder="One record set id per line"
                      className="rounded-2xl border border-slate-700 bg-slate-900 px-4 py-3 text-white outline-none transition focus:border-cyan-400 disabled:opacity-70"
                    />
                  </label>

                  <div>
                    <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Applied scope summary</h4>
                    <div className="mt-3 flex flex-wrap gap-2">
                      {scopeSummary(role, draft, isEditMode).map((label) => (
                        <span key={label} className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-200">
                          {label}
                        </span>
                      ))}
                    </div>
                  </div>
                </div>
              </section>

              <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                <h3 className="text-lg font-semibold text-white">Assigned people</h3>
                <p className="mt-2 text-sm text-slate-400">
                  Assign this role to people after the role has been created. Assignments are stored per person so products can evaluate scope at runtime.
                </p>

                {!roleId ? (
                  <div className="mt-4 rounded-2xl border border-dashed border-slate-700 px-4 py-6 text-sm text-slate-400">
                    Save the role before assigning people.
                  </div>
                ) : (
                  <>
                    <div className="mt-5 grid gap-4 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
                      <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                        <h4 className="font-medium text-white">Current assignments</h4>
                        <div className="mt-3 space-y-3">
                          {(role?.assignedPeople ?? []).map((assignment) => (
                            <div key={assignment.personRoleId} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-3">
                              <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                                <div>
                                  <p className="font-medium text-white">{assignment.displayName}</p>
                                  <p className="mt-1 text-sm text-slate-400">
                                    {assignment.assignmentScopeType.replace('_', ' ')}
                                    {assignment.assignmentScopeRefId ? ` • ${assignment.assignmentScopeRefId}` : ''}
                                  </p>
                                  <p className="mt-1 text-xs text-slate-500">
                                    {assignment.startsAt ? `Starts ${new Date(assignment.startsAt).toLocaleString()}` : 'Starts immediately'}
                                    {assignment.endsAt ? ` • Ends ${new Date(assignment.endsAt).toLocaleString()}` : ''}
                                  </p>
                                </div>
                                {isEditMode ? (
                                  <button
                                    type="button"
                                    onClick={() => removeAssignmentMutation.mutate(assignment)}
                                    className="rounded-full border border-rose-500/50 px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.2em] text-rose-200 transition hover:border-rose-400 hover:text-white"
                                  >
                                    Remove
                                  </button>
                                ) : null}
                              </div>
                            </div>
                          ))}
                          {(role?.assignedPeople ?? []).length === 0 ? (
                            <p className="rounded-2xl border border-dashed border-slate-800 px-4 py-6 text-sm text-slate-400">
                              No people are assigned to this role yet.
                            </p>
                          ) : null}
                        </div>
                      </div>

                      <div className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                        <h4 className="font-medium text-white">Add assignment</h4>
                        <div className="mt-3 grid gap-3">
                          <label className="grid gap-2 text-sm text-slate-300">
                            <span>Person</span>
                            <select
                              value={assignmentDraft.personId}
                              disabled={!isEditMode}
                              onChange={(event) =>
                                setAssignmentDraft((current) => ({ ...current, personId: event.target.value }))
                              }
                              className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-white outline-none transition focus:border-cyan-400"
                            >
                              <option value="">Select a person</option>
                              {people.map((person) => (
                                <option key={person.personId} value={person.personId}>
                                  {person.displayName}
                                </option>
                              ))}
                            </select>
                          </label>
                          <label className="grid gap-2 text-sm text-slate-300">
                            <span>Assignment scope</span>
                            <select
                              value={assignmentDraft.assignmentScopeType}
                              disabled={!isEditMode}
                              onChange={(event) =>
                                setAssignmentDraft((current) => ({
                                  ...current,
                                  assignmentScopeType: event.target.value as RoleScopeType,
                                  assignmentScopeRefId: '',
                                }))
                              }
                              className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-white outline-none transition focus:border-cyan-400"
                            >
                              <option value="tenant">Entire tenant</option>
                              <option value="site">Site</option>
                              <option value="department">Department</option>
                              <option value="location">Location</option>
                              <option value="team">Team</option>
                              <option value="position">Position</option>
                              <option value="record_set">Record set</option>
                              <option value="assigned_assets">Assigned assets only</option>
                              <option value="own_records">Own records only</option>
                              <option value="direct_reports">Direct reports only</option>
                            </select>
                          </label>
                          {assignmentScopeNeedsReference(assignmentDraft.assignmentScopeType) ? (
                            <label className="grid gap-2 text-sm text-slate-300">
                              <span>Scope reference id</span>
                              <input
                                value={assignmentDraft.assignmentScopeRefId}
                                disabled={!isEditMode}
                                onChange={(event) =>
                                  setAssignmentDraft((current) => ({
                                    ...current,
                                    assignmentScopeRefId: event.target.value,
                                  }))
                                }
                                className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-white outline-none transition focus:border-cyan-400"
                              />
                            </label>
                          ) : null}
                          <label className="grid gap-2 text-sm text-slate-300">
                            <span>Starts at</span>
                            <input
                              type="datetime-local"
                              value={assignmentDraft.startsAt}
                              disabled={!isEditMode}
                              onChange={(event) =>
                                setAssignmentDraft((current) => ({ ...current, startsAt: event.target.value }))
                              }
                              className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-white outline-none transition focus:border-cyan-400"
                            />
                          </label>
                          <label className="grid gap-2 text-sm text-slate-300">
                            <span>Ends at</span>
                            <input
                              type="datetime-local"
                              value={assignmentDraft.endsAt}
                              disabled={!isEditMode}
                              onChange={(event) =>
                                setAssignmentDraft((current) => ({ ...current, endsAt: event.target.value }))
                              }
                              className="rounded-2xl border border-slate-700 bg-slate-950 px-4 py-3 text-white outline-none transition focus:border-cyan-400"
                            />
                          </label>
                          <button
                            type="button"
                            disabled={!isEditMode}
                            onClick={() => assignPersonMutation.mutate()}
                            className="rounded-full bg-cyan-400 px-4 py-2 text-sm font-semibold text-slate-950 transition hover:bg-cyan-300 disabled:opacity-60"
                          >
                            Assign role
                          </button>
                        </div>
                      </div>
                    </div>
                  </>
                )}
              </section>

              <section id="role-audit" className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6">
                <h3 className="text-lg font-semibold text-white">Audit history</h3>
                <p className="mt-2 text-sm text-slate-400">
                  Every role create, update, archive, clone, permission change, scope change, and person assignment is recorded server-side.
                </p>
                <div className="mt-4 space-y-3">
                  {(role?.auditHistory ?? []).map((entry) => (
                    <div key={entry.id} className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                      <div className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
                        <div>
                          <p className="font-medium text-white">{entry.action}</p>
                          <p className="mt-1 text-sm text-slate-400">{entry.reason ?? 'No reason provided.'}</p>
                        </div>
                        <span className="text-xs uppercase tracking-[0.2em] text-slate-500">
                          {new Date(entry.createdAt).toLocaleString()}
                        </span>
                      </div>
                    </div>
                  ))}
                  {role && role.auditHistory.length === 0 ? (
                    <p className="rounded-2xl border border-dashed border-slate-800 px-4 py-6 text-sm text-slate-400">
                      No audit history has been recorded for this role yet.
                    </p>
                  ) : null}
                </div>
              </section>
            </>
          )}
        </section>
      </div>

      {isBusy ? <p className="mt-4 text-sm text-slate-400">Working…</p> : null}
    </div>
  )
}

function ScopeSelectionGrid({
  title,
  items,
  scopeType,
  scopes,
  disabled,
  onToggle,
}: {
  title: string
  items: OrgUnitResponse[]
  scopeType: RoleScopeType
  scopes: SetStaffRoleScopeItemRequest[]
  disabled: boolean
  onToggle: (scopeType: RoleScopeType, scopeRefId: string | null, scopeRefSnapshot: string | null) => void
}) {
  return (
    <div>
      <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">{title}</h4>
      <div className="mt-3 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {items.map((item) => (
          <label key={item.orgUnitId} className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                checked={hasScope(scopes, scopeType, item.orgUnitId)}
                disabled={disabled}
                onChange={() => onToggle(scopeType, item.orgUnitId, item.name)}
                className="mt-1 size-4 rounded border-slate-600 bg-slate-950"
              />
              <div>
                <p className="font-medium text-white">{item.name}</p>
                <p className="mt-1 text-xs uppercase tracking-[0.18em] text-slate-500">{item.unitType}</p>
              </div>
            </div>
          </label>
        ))}
      </div>
    </div>
  )
}

function LocationSelectionGrid({
  title,
  items,
  scopes,
  disabled,
  onToggle,
}: {
  title: string
  items: InternalLocationResponse[]
  scopes: SetStaffRoleScopeItemRequest[]
  disabled: boolean
  onToggle: (scopeType: RoleScopeType, scopeRefId: string | null, scopeRefSnapshot: string | null) => void
}) {
  return (
    <div>
      <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">{title}</h4>
      <div className="mt-3 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {items.map((item) => (
          <label key={item.locationId} className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
            <div className="flex items-start gap-3">
              <input
                type="checkbox"
                checked={hasScope(scopes, 'location', item.locationId)}
                disabled={disabled}
                onChange={() => onToggle('location', item.locationId, item.name)}
                className="mt-1 size-4 rounded border-slate-600 bg-slate-950"
              />
              <div>
                <p className="font-medium text-white">{item.name}</p>
                <p className="mt-1 text-xs uppercase tracking-[0.18em] text-slate-500">{item.locationType}</p>
              </div>
            </div>
          </label>
        ))}
      </div>
    </div>
  )
}
