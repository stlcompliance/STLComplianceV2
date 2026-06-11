import { useEffect, useRef, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  CircleHelp,
  Lock,
  PencilLine,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  Users,
  X,
} from 'lucide-react'
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
  PermissionCatalogPermissionGroupResponse,
  PermissionCatalogPermissionResponse,
  PermissionCatalogResponse,
  SetStaffPersonRoleItemRequest,
  SetStaffRoleScopeItemRequest,
  StaffRoleAssignedPersonResponse,
  StaffRoleDetailResponse,
  StaffRoleSummaryResponse,
} from '../../api/types'

type RoleScopeType = SetStaffRoleScopeItemRequest['scopeType']
type RoleDirectoryFilter = 'standard' | 'custom'
type EditorTabKey = 'permissions' | 'scope' | 'assignments' | 'audit'

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

const EDITOR_TABS: Array<{ key: EditorTabKey; label: string }> = [
  { key: 'permissions', label: 'Permissions' },
  { key: 'scope', label: 'Scope' },
  { key: 'assignments', label: 'Assignments' },
  { key: 'audit', label: 'Audit' },
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

function classNames(...parts: Array<string | false | null | undefined>) {
  return parts.filter(Boolean).join(' ')
}

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

function permissionCountForModule(
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
  const selected = permissionCountForModule(module, permissions)
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

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Not available'
  }

  return new Date(value).toLocaleString()
}

function roleTypeLabel(roleType: string): string {
  return roleType.replace(/_/g, ' ')
}

function assignmentScopeNeedsReference(scopeType: RoleScopeType) {
  return scopeType !== 'tenant'
    && scopeType !== 'assigned_assets'
    && scopeType !== 'own_records'
    && scopeType !== 'direct_reports'
}

export function RolesPage() {
  const session = loadSession()
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const location = useLocation()
  const { roleId } = useParams<{ roleId: string }>()
  const isNew = location.pathname.endsWith('/roles/new')
  const isEditMode = isNew || location.pathname.endsWith('/edit')
  const isOverlayOpen = Boolean(roleId) || isNew

  const [draft, setDraft] = useState<RoleDraft>(emptyDraft)
  const [selectedProductKey, setSelectedProductKey] = useState<string>('')
  const [assignmentDraft, setAssignmentDraft] = useState<AssignmentDraft>(emptyAssignmentDraft)
  const [directoryFilter, setDirectoryFilter] = useState<RoleDirectoryFilter>('standard')
  const [searchText, setSearchText] = useState('')
  const [editorTab, setEditorTab] = useState<EditorTabKey>('permissions')
  const initialCatalogRefreshKeyRef = useRef('')

  const sessionQuery = useQuery({
    queryKey: ['staffarr-session-bootstrap', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    staleTime: 0,
    refetchOnMount: 'always',
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
    enabled: Boolean(session?.accessToken && sessionQuery.data),
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

  useEffect(() => {
    if (isOverlayOpen) {
      setEditorTab('permissions')
    }
  }, [isOverlayOpen, roleId, isNew])

  const entitledProductKeys = (sessionQuery.data?.entitlements ?? [])
    .map((entitlement) => entitlement.trim().toLowerCase())
    .filter((entitlement) => PRODUCT_ORDER.includes(entitlement as (typeof PRODUCT_ORDER)[number]))

  useEffect(() => {
    if (!session?.accessToken || entitledProductKeys.length === 0) {
      return
    }

    const refreshKey = [...entitledProductKeys].sort().join('|')
    if (!refreshKey || initialCatalogRefreshKeyRef.current === refreshKey) {
      return
    }

    initialCatalogRefreshKeyRef.current = refreshKey
    void (async () => {
      await Promise.all(
        entitledProductKeys.map((productKey) =>
          refreshPermissionCatalogs(session.accessToken, { productKey }),
        ),
      )
      await queryClient.invalidateQueries({ queryKey: ['staffarr-v1-permission-catalogs'] })
    })()
  }, [entitledProductKeys, queryClient, session?.accessToken])

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
      navigate(`/roles/${nextRoleId}/edit`)
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
      if (roleId) {
        navigate(`/roles/${roleId}`)
      }
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
    mutationFn: async () => {
      if (entitledProductKeys.length === 0) {
        return refreshPermissionCatalogs(session!.accessToken)
      }

      const results = await Promise.all(
        entitledProductKeys.map((productKey) =>
          refreshPermissionCatalogs(session!.accessToken, { productKey }),
        ),
      )
      return results[results.length - 1]
    },
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

  const catalogs = sortCatalogs(
    (catalogsQuery.data ?? []).filter(
      (catalog) =>
        entitledProductKeys.length === 0 ||
        entitledProductKeys.includes(catalog.productKey.toLowerCase()),
    ),
  )
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

  const allRoles = rolesQuery.data ?? []
  const standardRoleCount = allRoles.filter((roleItem) => roleItem.isSystem).length
  const customRoleCount = allRoles.filter((roleItem) => !roleItem.isSystem).length
  const filteredRoles = allRoles.filter((roleItem) => {
    const matchesFilter = directoryFilter === 'standard' ? roleItem.isSystem : !roleItem.isSystem
    if (!matchesFilter) {
      return false
    }

    const query = searchText.trim().toLowerCase()
    if (!query) {
      return true
    }

    return roleItem.name.toLowerCase().includes(query)
      || (roleItem.description ?? '').toLowerCase().includes(query)
      || roleTypeLabel(roleItem.roleType).toLowerCase().includes(query)
  })

  const people = [...(peopleQuery.data ?? [])].sort((left, right) => left.displayName.localeCompare(right.displayName))
  const orgUnits = orgUnitsQuery.data ?? []
  const locations = locationsQuery.data ?? []
  const siteUnits = orgUnits.filter((unit) => unit.unitType === 'site')
  const departmentUnits = orgUnits.filter((unit) => unit.unitType === 'department')
  const teamUnits = orgUnits.filter((unit) => unit.unitType === 'team')
  const positionUnits = orgUnits.filter((unit) => unit.unitType === 'position')
  const selectedCount = activeCatalog ? permissionCountForCatalog(activeCatalog, draft.permissions) : 0
  const roleScopeLabels = scopeSummary(role, draft, isEditMode)
  const selectedRoleId = roleId ?? ''
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
  const isEditableRole =
    isNew || (role != null && isEditMode && !role.isSystem && !role.isArchived)
  const editorTitle = isNew ? 'Add Role' : isEditMode ? 'Edit Role' : 'Role Detail'
  const editorDescription = draft.description.trim() || role?.description || 'Describe when this role should be used.'

  function closeEditor() {
    navigate('/roles')
  }

  function updateCatalogPermissions(nextState: 'Full' | 'None') {
    if (!activeCatalog) {
      return
    }

    const nextPermissions = { ...draft.permissions }
    activeCatalog.modules
      .flatMap((module) => module.permissionGroups)
      .flatMap((group) => group.permissions)
      .forEach((permission) => {
        if (nextState === 'Full') {
          nextPermissions[permission.key] = nextPermissions[permission.key] ?? 'allow'
        } else {
          delete nextPermissions[permission.key]
        }
      })

    setDraft((current) => ({ ...current, permissions: nextPermissions }))
  }

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

  return (
    <>
      <div className="mx-auto max-w-[1440px] px-4 py-6">
        <header className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <h1 className="text-4xl font-semibold tracking-tight text-white">Roles</h1>
            <p className="mt-2 max-w-3xl text-sm text-slate-400">
              Manage StaffArr authority with standard templates, tenant-specific roles, live product permission catalogs,
              and scoped assignments.
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <button
              type="button"
              onClick={() => refreshCatalogMutation.mutate()}
              className="inline-flex items-center gap-2 rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-2.5 text-sm font-medium text-slate-200 transition hover:border-cyan-400 hover:text-white"
            >
              <RefreshCw className="h-4 w-4" />
              Refresh catalogs
            </button>
            <button
              type="button"
              onClick={() => navigate('/roles/new')}
              className="inline-flex items-center gap-2 rounded-2xl bg-emerald-500 px-4 py-2.5 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400"
            >
              <Plus className="h-4 w-4" />
              Add Role
            </button>
          </div>
        </header>

        {errorMessage ? (
          <div className="mt-5 rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-3 text-sm text-rose-100">
            {errorMessage}
          </div>
        ) : null}

        <section className="mt-6 rounded-[28px] border border-slate-800 bg-[#171717] shadow-2xl shadow-black/35">
          <div className="flex flex-col gap-4 border-b border-slate-800 px-5 py-5 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex flex-wrap items-center gap-3">
              <DirectoryFilterButton
                active={directoryFilter === 'standard'}
                label={`Standard Roles (${standardRoleCount})`}
                onClick={() => setDirectoryFilter('standard')}
              />
              <DirectoryFilterButton
                active={directoryFilter === 'custom'}
                label={`Users with Custom Roles (${customRoleCount})`}
                onClick={() => setDirectoryFilter('custom')}
              />
            </div>
            <div className="flex items-center gap-3">
              <div className="relative w-full max-w-sm min-w-[220px]">
                <Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-500" />
                <input
                  value={searchText}
                  onChange={(event) => setSearchText(event.target.value)}
                  placeholder="Search"
                  className="w-full rounded-2xl border border-slate-700 bg-[#2f2f2f] py-2.5 pl-10 pr-4 text-sm text-white outline-none transition placeholder:text-slate-500 focus:border-emerald-400"
                />
              </div>
              <div className="hidden text-sm text-slate-300 lg:block">
                {filteredRoles.length} role{filteredRoles.length === 1 ? '' : 's'}
              </div>
            </div>
          </div>

          <div className="hidden border-b border-slate-800 px-5 py-3 text-xs font-semibold uppercase tracking-[0.18em] text-slate-500 lg:grid lg:grid-cols-[minmax(0,2.8fr)_110px_170px_170px_120px] lg:gap-4">
            <span>Name</span>
            <span>Users</span>
            <span>Created At</span>
            <span>Updated At</span>
            <span className="text-right">Action</span>
          </div>

          <div className="divide-y divide-slate-800">
            {filteredRoles.map((roleItem) => (
              <RoleDirectoryRow
                key={roleItem.roleId}
                active={selectedRoleId === roleItem.roleId}
                role={roleItem}
                onOpen={() => navigate(`/roles/${roleItem.roleId}`)}
                onEdit={() => navigate(`/roles/${roleItem.roleId}/edit`)}
              />
            ))}

            {!rolesQuery.isLoading && filteredRoles.length === 0 ? (
              <div className="px-5 py-14 text-center text-sm text-slate-400">
                No roles match the current filter.
              </div>
            ) : null}
          </div>
        </section>

        {isBusy ? <p className="mt-4 text-sm text-slate-400">Working…</p> : null}
      </div>

      {isOverlayOpen ? (
        <>
          <div className="fixed inset-0 z-40 bg-slate-950/78 backdrop-blur-sm" onClick={closeEditor} />
          <section className="fixed inset-x-4 bottom-4 top-4 z-50 mx-auto max-w-[1480px] overflow-hidden rounded-[30px] border border-slate-800 bg-[#1b1b1b] shadow-[0_40px_120px_rgba(0,0,0,0.55)]">
            <div className="flex items-center justify-between border-b border-slate-800 px-6 py-4">
              <div className="min-w-0">
                <p className="text-xs font-semibold uppercase tracking-[0.24em] text-emerald-300">StaffArr authority</p>
                <h2 className="mt-1 truncate text-2xl font-semibold text-white">{editorTitle}</h2>
              </div>
              <button
                type="button"
                onClick={closeEditor}
                className="rounded-full p-2 text-slate-400 transition hover:bg-slate-800 hover:text-white"
                aria-label="Close editor"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <div className="grid h-[calc(100%-81px)] xl:grid-cols-[minmax(0,1fr)_320px]">
              <div className="overflow-y-auto px-6 py-6">
                {!isNew && roleId && roleDetailQuery.isLoading ? (
                  <div className="rounded-3xl border border-dashed border-slate-700 bg-slate-950/50 px-6 py-12 text-center text-sm text-slate-400">
                    Loading role details…
                  </div>
                ) : (
                  <div className="space-y-6">
                    <section className="rounded-[26px] border border-slate-800 bg-[#202020] p-6">
                      <div className="flex flex-col gap-5 lg:flex-row lg:items-start lg:justify-between">
                        <div className="min-w-0">
                          <div className="flex flex-wrap items-center gap-2">
                            {!isNew ? (
                              <span className="rounded-full bg-slate-800 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-300">
                                {role?.isSystem ? 'Standard' : 'Custom'}
                              </span>
                            ) : null}
                            {!isNew && role?.isArchived ? (
                              <span className="rounded-full bg-slate-700 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-200">
                                Archived
                              </span>
                            ) : null}
                          </div>
                          <h3 className="mt-3 text-2xl font-semibold text-white">
                            {draft.name.trim() || role?.name || 'Untitled role'}
                          </h3>
                          <p className="mt-2 max-w-4xl text-sm text-slate-400">{editorDescription}</p>
                        </div>
                        <div className="flex flex-wrap gap-2">
                          {!isNew && !isEditMode ? (
                            <button
                              type="button"
                              onClick={() => navigate(`/roles/${roleId}/edit`)}
                              className="rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-emerald-400 hover:text-white"
                            >
                              Edit
                            </button>
                          ) : null}
                          {!isNew ? (
                            <button
                              type="button"
                              onClick={() => cloneMutation.mutate()}
                              className="rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-emerald-400 hover:text-white"
                            >
                              Clone
                            </button>
                          ) : null}
                          {!isNew && !role?.isSystem && !role?.isArchived ? (
                            <button
                              type="button"
                              onClick={() => archiveMutation.mutate()}
                              className="rounded-2xl border border-rose-500/40 bg-rose-500/10 px-4 py-2 text-sm font-medium text-rose-200 transition hover:border-rose-400 hover:text-white"
                            >
                              Archive
                            </button>
                          ) : null}
                        </div>
                      </div>

                      {role && (role.isSystem || role.isArchived) && isEditMode ? (
                        <div className="mt-5 rounded-2xl border border-amber-500/35 bg-amber-500/10 px-4 py-3 text-sm text-amber-100">
                          {role.isSystem
                            ? 'Standard roles are read-only. Clone this role to make tenant-specific changes.'
                            : 'Archived roles are read-only. View audit and assignments here, or clone the role to create a replacement.'}
                        </div>
                      ) : null}

                      <div className="mt-6 grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)_240px]">
                        <label className="grid gap-2 text-sm text-slate-300">
                          <span>
                            Name <span className="text-rose-300">*</span>
                          </span>
                          <input
                            value={draft.name}
                            disabled={!isEditableRole}
                            onChange={(event) => setDraft((current) => ({ ...current, name: event.target.value }))}
                            className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                          />
                        </label>
                        <label className="grid gap-2 text-sm text-slate-300">
                          <span>Description</span>
                          <textarea
                            value={draft.description}
                            disabled={!isEditableRole}
                            onChange={(event) => setDraft((current) => ({ ...current, description: event.target.value }))}
                            rows={4}
                            className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                          />
                        </label>
                        <label className="grid gap-2 text-sm text-slate-300">
                          <span>Role type</span>
                          <select
                            value={draft.roleType}
                            disabled={!isEditableRole || !isNew}
                            onChange={(event) =>
                              setDraft((current) => ({
                                ...current,
                                roleType: event.target.value as RoleDraft['roleType'],
                              }))
                            }
                            className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                          >
                            <option value="tenant_role">Tenant role</option>
                            <option value="product_template">Product template</option>
                          </select>
                        </label>
                      </div>
                    </section>

                    <div className="flex flex-wrap gap-2">
                      {EDITOR_TABS.map((tab) => (
                        <EditorTabButton
                          key={tab.key}
                          active={editorTab === tab.key}
                          label={tab.label}
                          onClick={() => setEditorTab(tab.key)}
                        />
                      ))}
                    </div>

                    {editorTab === 'permissions' ? (
                      <section className="rounded-[26px] border border-slate-800 bg-[#202020] p-6">
                        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
                          <div>
                            <h3 className="text-lg font-semibold text-white">Module Permissions</h3>
                            <p className="mt-1 text-sm text-slate-400">
                              Choose a product catalog, set module-level access, then fine-tune individual permissions.
                            </p>
                          </div>
                          <div className="flex flex-wrap items-center gap-3 text-sm">
                            <span className="text-slate-400">Select All:</span>
                            <button
                              type="button"
                              disabled={!isEditableRole || !activeCatalog}
                              onClick={() => updateCatalogPermissions('Full')}
                              className="font-semibold text-emerald-300 transition hover:text-emerald-200 disabled:opacity-50"
                            >
                              Full
                            </button>
                            <button
                              type="button"
                              disabled={!isEditableRole || !activeCatalog}
                              onClick={() => updateCatalogPermissions('None')}
                              className="font-semibold text-slate-300 transition hover:text-white disabled:opacity-50"
                            >
                              None
                            </button>
                          </div>
                        </div>

                        <div className="mt-5 flex flex-wrap gap-2">
                          {catalogs.map((catalog) => (
                            <button
                              key={catalog.productKey}
                              type="button"
                              onClick={() => setSelectedProductKey(catalog.productKey)}
                              className={classNames(
                                'rounded-2xl border px-3 py-2 text-sm transition',
                                activeCatalog?.productKey === catalog.productKey
                                  ? 'border-emerald-400 bg-emerald-500/10 text-white'
                                  : 'border-slate-700 bg-slate-900/80 text-slate-300 hover:border-slate-500',
                              )}
                            >
                              {catalog.productName}
                            </button>
                          ))}
                        </div>

                        <div className="mt-4 rounded-2xl border border-slate-800 bg-slate-950/55 px-4 py-3 text-sm text-slate-300">
                          {activeCatalog
                            ? `${activeCatalog.productName} exposes ${activeCatalog.modules.length} modules with ${selectedCount} selected permissions in this role.`
                            : 'Refresh catalogs to load product permission groups.'}
                        </div>

                        {!activeCatalog ? (
                          <div className="mt-5 rounded-2xl border border-dashed border-slate-700 px-4 py-10 text-sm text-slate-400">
                            No product catalog is available yet.
                          </div>
                        ) : (
                          <div className="mt-6 space-y-5">
                            {activeCatalog.modules.map((module) => {
                              const moduleState = moduleSelectionState(module, draft.permissions)
                              return (
                                <article key={module.key} className="rounded-3xl border border-slate-800 bg-slate-950/55 p-5">
                                  <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_280px] lg:items-start">
                                    <div>
                                      <div className="flex flex-wrap items-center gap-3">
                                        <h4 className="text-base font-semibold text-white">{module.label}</h4>
                                        <span className="rounded-full bg-slate-800 px-2.5 py-1 text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-300">
                                          {permissionCountForModule(module, draft.permissions)} selected
                                        </span>
                                      </div>
                                      <p className="mt-2 text-sm text-slate-400">
                                        {module.description ?? 'Module permissions grouped from the active product catalog.'}
                                      </p>
                                    </div>
                                    <div className="grid gap-2">
                                      <p className="text-sm font-medium text-slate-300">Access</p>
                                      <div className="flex flex-wrap gap-2">
                                        {(['Full', 'Some', 'None'] as const).map((state) => (
                                          <ModuleAccessButton
                                            key={state}
                                            active={moduleState === state}
                                            disabled={!isEditableRole || state === 'Some'}
                                            label={state}
                                            onClick={() =>
                                              state === 'Full' || state === 'None'
                                                ? updateModulePermissions(module, state)
                                                : undefined
                                            }
                                          />
                                        ))}
                                      </div>
                                    </div>
                                  </div>

                                  <div className="mt-5 space-y-4 border-t border-slate-800 pt-5">
                                    {module.permissionGroups.map((group) => (
                                      <PermissionGroupCard
                                        key={group.key}
                                        group={group}
                                        isEditableRole={isEditableRole}
                                        permissions={draft.permissions}
                                        onTogglePermission={togglePermission}
                                        onSetPermissionEffect={setPermissionEffect}
                                      />
                                    ))}
                                  </div>
                                </article>
                              )
                            })}
                          </div>
                        )}
                      </section>
                    ) : null}

                    {editorTab === 'scope' ? (
                      <section className="rounded-[26px] border border-slate-800 bg-[#202020] p-6">
                        <h3 className="text-lg font-semibold text-white">Scope and Record Sets</h3>
                        <p className="mt-2 text-sm text-slate-400">
                          StaffArr owns sites, org units, and internal locations. Use these selectors to constrain where the role applies.
                        </p>

                        <div className="mt-6 space-y-6">
                          <div>
                            <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Scope Flags</h4>
                            <div className="mt-3 grid gap-3 md:grid-cols-2">
                              {FLAG_SCOPE_OPTIONS.map((option) => (
                                <label key={option.type} className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
                                  <div className="flex items-start gap-3">
                                    <input
                                      type="checkbox"
                                      checked={hasScope(draft.scopes, option.type, null)}
                                      disabled={!isEditableRole}
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
                            disabled={!isEditableRole}
                            onToggle={toggleScope}
                          />
                          <ScopeSelectionGrid
                            title="Departments"
                            items={departmentUnits}
                            scopeType="department"
                            scopes={draft.scopes}
                            disabled={!isEditableRole}
                            onToggle={toggleScope}
                          />
                          <ScopeSelectionGrid
                            title="Teams"
                            items={teamUnits}
                            scopeType="team"
                            scopes={draft.scopes}
                            disabled={!isEditableRole}
                            onToggle={toggleScope}
                          />
                          <ScopeSelectionGrid
                            title="Positions"
                            items={positionUnits}
                            scopeType="position"
                            scopes={draft.scopes}
                            disabled={!isEditableRole}
                            onToggle={toggleScope}
                          />
                          <LocationSelectionGrid
                            title="Locations"
                            items={locations}
                            scopes={draft.scopes}
                            disabled={!isEditableRole}
                            onToggle={toggleScope}
                          />

                          <label className="grid gap-2 text-sm text-slate-300">
                            <span className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Record Sets</span>
                            <textarea
                              value={draft.recordSetText}
                              disabled={!isEditableRole}
                              onChange={(event) => setDraft((current) => ({ ...current, recordSetText: event.target.value }))}
                              rows={4}
                              placeholder="One record set id per line"
                              className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                            />
                          </label>

                          <div>
                            <h4 className="text-sm font-semibold uppercase tracking-[0.2em] text-slate-300">Applied Scope Summary</h4>
                            <div className="mt-3 flex flex-wrap gap-2">
                              {roleScopeLabels.map((label) => (
                                <span key={label} className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-200">
                                  {label}
                                </span>
                              ))}
                              {roleScopeLabels.length === 0 ? (
                                <span className="rounded-full bg-slate-800 px-3 py-1 text-xs text-slate-400">No scope selected</span>
                              ) : null}
                            </div>
                          </div>
                        </div>
                      </section>
                    ) : null}

                    {editorTab === 'assignments' ? (
                      <section className="rounded-[26px] border border-slate-800 bg-[#202020] p-6">
                        <h3 className="text-lg font-semibold text-white">Assigned People</h3>
                        <p className="mt-2 text-sm text-slate-400">
                          Assign this role to people after the role has been created. Assignments are stored per person so products can evaluate scope at runtime.
                        </p>

                        {!roleId ? (
                          <div className="mt-5 rounded-2xl border border-dashed border-slate-700 px-4 py-8 text-sm text-slate-400">
                            Save the role before assigning people.
                          </div>
                        ) : (
                          <div className="mt-5 grid gap-4 xl:grid-cols-[minmax(0,1.3fr)_minmax(320px,0.7fr)]">
                            <div className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
                              <h4 className="font-medium text-white">Current Assignments</h4>
                              <div className="mt-3 space-y-3">
                                {(role?.assignedPeople ?? []).map((assignment) => (
                                  <div key={assignment.personRoleId} className="rounded-2xl border border-slate-800 bg-slate-900/70 p-4">
                                    <div className="flex flex-col gap-3 md:flex-row md:items-start md:justify-between">
                                      <div>
                                        <p className="font-medium text-white">{assignment.displayName}</p>
                                        <p className="mt-1 text-sm text-slate-400">
                                          {assignment.assignmentScopeType.replace(/_/g, ' ')}
                                          {assignment.assignmentScopeRefId ? ` • ${assignment.assignmentScopeRefId}` : ''}
                                        </p>
                                        <p className="mt-1 text-xs text-slate-500">
                                          {assignment.startsAt ? `Starts ${formatDateTime(assignment.startsAt)}` : 'Starts immediately'}
                                          {assignment.endsAt ? ` • Ends ${formatDateTime(assignment.endsAt)}` : ''}
                                        </p>
                                      </div>
                                      {isEditableRole ? (
                                        <button
                                          type="button"
                                          onClick={() => removeAssignmentMutation.mutate(assignment)}
                                          className="rounded-2xl border border-rose-500/40 bg-rose-500/10 px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.18em] text-rose-200 transition hover:border-rose-400 hover:text-white"
                                        >
                                          Remove
                                        </button>
                                      ) : null}
                                    </div>
                                  </div>
                                ))}
                                {(role?.assignedPeople ?? []).length === 0 ? (
                                  <div className="rounded-2xl border border-dashed border-slate-800 px-4 py-8 text-sm text-slate-400">
                                    No people are assigned to this role yet.
                                  </div>
                                ) : null}
                              </div>
                            </div>

                            <div className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
                              <h4 className="font-medium text-white">Add Assignment</h4>
                              <div className="mt-4 grid gap-3">
                                <label className="grid gap-2 text-sm text-slate-300">
                                  <span>Person</span>
                                  <select
                                    value={assignmentDraft.personId}
                                    disabled={!isEditableRole}
                                    onChange={(event) =>
                                      setAssignmentDraft((current) => ({ ...current, personId: event.target.value }))
                                    }
                                    className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
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
                                    disabled={!isEditableRole}
                                    onChange={(event) =>
                                      setAssignmentDraft((current) => ({
                                        ...current,
                                        assignmentScopeType: event.target.value as RoleScopeType,
                                        assignmentScopeRefId: '',
                                      }))
                                    }
                                    className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
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
                                      disabled={!isEditableRole}
                                      onChange={(event) =>
                                        setAssignmentDraft((current) => ({
                                          ...current,
                                          assignmentScopeRefId: event.target.value,
                                        }))
                                      }
                                      className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                                    />
                                  </label>
                                ) : null}
                                <label className="grid gap-2 text-sm text-slate-300">
                                  <span>Starts at</span>
                                  <input
                                    type="datetime-local"
                                    value={assignmentDraft.startsAt}
                                    disabled={!isEditableRole}
                                    onChange={(event) =>
                                      setAssignmentDraft((current) => ({ ...current, startsAt: event.target.value }))
                                    }
                                    className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                                  />
                                </label>
                                <label className="grid gap-2 text-sm text-slate-300">
                                  <span>Ends at</span>
                                  <input
                                    type="datetime-local"
                                    value={assignmentDraft.endsAt}
                                    disabled={!isEditableRole}
                                    onChange={(event) =>
                                      setAssignmentDraft((current) => ({ ...current, endsAt: event.target.value }))
                                    }
                                    className="rounded-2xl border border-slate-700 bg-[#343434] px-4 py-3 text-white outline-none transition focus:border-emerald-400 disabled:opacity-70"
                                  />
                                </label>
                                <button
                                  type="button"
                                  disabled={!isEditableRole}
                                  onClick={() => assignPersonMutation.mutate()}
                                  className="rounded-2xl bg-emerald-500 px-4 py-3 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400 disabled:opacity-60"
                                >
                                  Assign role
                                </button>
                              </div>
                            </div>
                          </div>
                        )}
                      </section>
                    ) : null}

                    {editorTab === 'audit' ? (
                      <section className="rounded-[26px] border border-slate-800 bg-[#202020] p-6">
                        <h3 className="text-lg font-semibold text-white">Audit History</h3>
                        <p className="mt-2 text-sm text-slate-400">
                          Every role create, update, archive, clone, permission change, scope change, and person assignment is recorded server-side.
                        </p>
                        <div className="mt-5 space-y-3">
                          {(role?.auditHistory ?? []).map((entry) => (
                            <div key={entry.id} className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
                              <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                                <div>
                                  <p className="font-medium text-white">{entry.action}</p>
                                  <p className="mt-1 text-sm text-slate-400">{entry.reason ?? 'No reason provided.'}</p>
                                </div>
                                <span className="text-xs uppercase tracking-[0.18em] text-slate-500">
                                  {formatDateTime(entry.createdAt)}
                                </span>
                              </div>
                            </div>
                          ))}
                          {role && role.auditHistory.length === 0 ? (
                            <div className="rounded-2xl border border-dashed border-slate-800 px-4 py-8 text-sm text-slate-400">
                              No audit history has been recorded for this role yet.
                            </div>
                          ) : null}
                        </div>
                      </section>
                    ) : null}
                  </div>
                )}
              </div>

              <aside className="border-t border-slate-800 bg-[#171717] px-6 py-6 xl:border-l xl:border-t-0">
                <div className="space-y-4 xl:sticky xl:top-0">
                  <SummaryCard
                    icon={<ShieldCheck className="h-4 w-4 text-emerald-300" />}
                    title="Role Summary"
                  >
                    <SummaryRow label="Type" value={roleTypeLabel(draft.roleType)} />
                    <SummaryRow label="Selected permissions" value={String(Object.keys(draft.permissions).length)} />
                    <SummaryRow label="Applied scopes" value={String(roleScopeLabels.length)} />
                    <SummaryRow label="Assigned people" value={String(role?.assignedPeople?.length ?? 0)} />
                    {role ? <SummaryRow label="Updated" value={formatDateTime(role.updatedAt)} /> : null}
                  </SummaryCard>

                  <SummaryCard
                    icon={<Users className="h-4 w-4 text-cyan-300" />}
                    title="Products in Scope"
                  >
                    <div className="flex flex-wrap gap-2">
                      {catalogs.map((catalog) => (
                        <span
                          key={catalog.productKey}
                          className={classNames(
                            'rounded-full px-2.5 py-1 text-xs',
                            activeCatalog?.productKey === catalog.productKey
                              ? 'bg-emerald-500/15 text-emerald-100'
                              : 'bg-slate-800 text-slate-300',
                          )}
                        >
                          {catalog.productName}
                        </span>
                      ))}
                    </div>
                  </SummaryCard>

                  <SummaryCard
                    icon={<CircleHelp className="h-4 w-4 text-amber-300" />}
                    title="Guidance"
                  >
                    <p className="text-sm leading-6 text-slate-300">
                      Standard roles are templates. Tenant roles should be specific, scoping only the product access and org context the user actually needs.
                    </p>
                  </SummaryCard>

                  <div className="rounded-[24px] border border-slate-800 bg-[#202020] p-4">
                    <div className="flex flex-col gap-3">
                      {isEditableRole ? (
                        <button
                          type="button"
                          onClick={() => saveMutation.mutate()}
                          className="rounded-2xl bg-emerald-500 px-4 py-3 text-sm font-semibold text-slate-950 transition hover:bg-emerald-400"
                        >
                          Save role
                        </button>
                      ) : null}
                      {!isNew && !isEditMode ? (
                        <button
                          type="button"
                          onClick={() => navigate(`/roles/${roleId}/edit`)}
                          className="inline-flex items-center justify-center gap-2 rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-3 text-sm font-medium text-slate-200 transition hover:border-emerald-400 hover:text-white"
                        >
                          <PencilLine className="h-4 w-4" />
                          Open edit mode
                        </button>
                      ) : null}
                      {!isNew ? (
                        <button
                          type="button"
                          onClick={() => cloneMutation.mutate()}
                          className="rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-3 text-sm font-medium text-slate-200 transition hover:border-emerald-400 hover:text-white"
                        >
                          Clone role
                        </button>
                      ) : null}
                      <button
                        type="button"
                        onClick={closeEditor}
                        className="rounded-2xl border border-slate-700 bg-slate-900/80 px-4 py-3 text-sm font-medium text-slate-200 transition hover:border-slate-500 hover:text-white"
                      >
                        Close
                      </button>
                    </div>
                  </div>

                  {role?.isSystem ? (
                    <div className="rounded-[24px] border border-slate-800 bg-slate-900/70 p-4">
                      <div className="flex items-start gap-3">
                        <Lock className="mt-0.5 h-4 w-4 shrink-0 text-amber-300" />
                        <p className="text-sm text-slate-300">
                          This is a standard system role. Clone it to create a tenant-specific version before changing permissions or scope.
                        </p>
                      </div>
                    </div>
                  ) : null}
                </div>
              </aside>
            </div>
          </section>
        </>
      ) : null}
    </>
  )
}

function permissionCountForCatalog(
  catalog: PermissionCatalogResponse,
  permissions: Record<string, 'allow' | 'deny'>,
): number {
  return catalog.modules
    .flatMap((module) => module.permissionGroups)
    .flatMap((group) => group.permissions)
    .filter((permission) => permissions[permission.key])
    .length
}

function DirectoryFilterButton({
  active,
  label,
  onClick,
}: {
  active: boolean
  label: string
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={classNames(
        'rounded-full px-4 py-2 text-sm font-medium transition',
        active
          ? 'bg-emerald-500 text-slate-950'
          : 'text-emerald-300 hover:bg-emerald-500/10 hover:text-emerald-200',
      )}
    >
      {label}
    </button>
  )
}

function RoleDirectoryRow({
  active,
  role,
  onOpen,
  onEdit,
}: {
  active: boolean
  role: StaffRoleSummaryResponse
  onOpen: () => void
  onEdit: () => void
}) {
  return (
    <div
      className={classNames(
        'grid gap-4 px-5 py-4 transition lg:grid-cols-[minmax(0,2.8fr)_110px_170px_170px_120px] lg:items-center',
        active && 'bg-emerald-500/6',
      )}
    >
      <button type="button" onClick={onOpen} className="min-w-0 text-left">
        <div className="flex items-start gap-3">
          <div className="mt-0.5 shrink-0">
            {role.isSystem ? (
              <Lock className="h-4 w-4 text-slate-400" />
            ) : (
              <div className="h-4 w-4 rounded-[4px] border border-slate-600 bg-slate-900" />
            )}
          </div>
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <p className="font-semibold text-white">{role.name}</p>
              {role.isSystem ? (
                <span className="rounded-full bg-slate-700 px-2 py-0.5 text-[11px] font-medium text-slate-300">
                  Default
                </span>
              ) : null}
            </div>
            <p className="mt-1 text-sm text-slate-400">
              {role.description ?? 'No description yet.'}
            </p>
          </div>
        </div>
      </button>

      <div className="text-sm text-emerald-300">
        {role.assignedPersonCount > 0 ? `${role.assignedPersonCount} Users` : '—'}
      </div>

      <div className="text-sm text-slate-300">{formatDateTime(role.createdAt)}</div>
      <div className="text-sm text-slate-300">{formatDateTime(role.updatedAt)}</div>

      <div className="flex items-center justify-end gap-2">
        <button
          type="button"
          onClick={onOpen}
          className="rounded-xl border border-slate-700 px-3 py-1.5 text-xs font-medium text-slate-300 transition hover:border-slate-500 hover:text-white"
        >
          View
        </button>
        <button
          type="button"
          onClick={onEdit}
          className="rounded-xl border border-emerald-500/40 bg-emerald-500/10 px-3 py-1.5 text-xs font-medium text-emerald-200 transition hover:border-emerald-400 hover:text-white"
        >
          Edit
        </button>
      </div>
    </div>
  )
}

function EditorTabButton({
  active,
  label,
  onClick,
}: {
  active: boolean
  label: string
  onClick: () => void
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={classNames(
        'rounded-2xl border px-4 py-2 text-sm font-medium transition',
        active
          ? 'border-emerald-400 bg-emerald-500/10 text-white'
          : 'border-slate-700 bg-slate-900/80 text-slate-300 hover:border-slate-500 hover:text-white',
      )}
    >
      {label}
    </button>
  )
}

function ModuleAccessButton({
  active,
  disabled,
  label,
  onClick,
}: {
  active: boolean
  disabled: boolean
  label: 'Full' | 'Some' | 'None'
  onClick?: () => void
}) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      className={classNames(
        'inline-flex items-center gap-2 rounded-full border px-3 py-1.5 text-sm transition disabled:cursor-default disabled:opacity-65',
        active
          ? 'border-emerald-400 bg-emerald-500/10 text-white'
          : 'border-slate-700 text-slate-300 hover:border-slate-500 hover:text-white',
      )}
    >
      <span
        className={classNames(
          'h-3.5 w-3.5 rounded-full border',
          active ? 'border-emerald-300 bg-emerald-400' : 'border-slate-500 bg-transparent',
        )}
      />
      {label}
    </button>
  )
}

function PermissionGroupCard({
  group,
  isEditableRole,
  permissions,
  onTogglePermission,
  onSetPermissionEffect,
}: {
  group: PermissionCatalogPermissionGroupResponse
  isEditableRole: boolean
  permissions: Record<string, 'allow' | 'deny'>
  onTogglePermission: (permission: PermissionCatalogPermissionResponse, checked: boolean) => void
  onSetPermissionEffect: (permissionKey: string, effect: 'allow' | 'deny') => void
}) {
  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/55 p-4">
      <p className="text-sm font-semibold uppercase tracking-[0.18em] text-slate-400">{group.label}</p>
      <div className="mt-4 space-y-3">
        {group.permissions.map((permission) => {
          const selected = Boolean(permissions[permission.key])
          const missingDependencies = permission.dependsOn.filter((dependency) => !permissions[dependency])
          const conflicts = permission.conflictsWith.filter((conflict) => Boolean(permissions[conflict]))
          return (
            <div key={permission.key} className="rounded-2xl border border-slate-800 bg-[#232323] p-4">
              <div className="flex flex-col gap-3 lg:flex-row lg:items-start lg:justify-between">
                <label className="flex min-w-0 items-start gap-3">
                  <input
                    type="checkbox"
                    checked={selected}
                    disabled={!isEditableRole}
                    onChange={(event) => onTogglePermission(permission, event.target.checked)}
                    className="mt-1 size-4 rounded border-slate-600 bg-slate-950"
                  />
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium text-white">{permission.label}</span>
                      <RiskPill riskLevel={permission.riskLevel} />
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
                    value={permissions[permission.key]}
                    disabled={!isEditableRole}
                    onChange={(event) =>
                      onSetPermissionEffect(
                        permission.key,
                        event.target.value as 'allow' | 'deny',
                      )
                    }
                    className="rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white outline-none transition focus:border-emerald-400"
                  >
                    <option value="allow">Allow</option>
                    <option value="deny">Deny</option>
                  </select>
                ) : null}
              </div>

              {missingDependencies.length > 0 ? (
                <p className="mt-3 text-xs text-amber-200">Depends on: {missingDependencies.join(', ')}</p>
              ) : null}
              {conflicts.length > 0 ? (
                <p className="mt-1 text-xs text-rose-200">Conflicts with: {conflicts.join(', ')}</p>
              ) : null}
            </div>
          )
        })}
      </div>
    </div>
  )
}

function RiskPill({ riskLevel }: { riskLevel: string }) {
  return (
    <span
      className={classNames(
        'rounded-full px-2 py-0.5 text-[11px] font-semibold',
        riskLevel === 'critical'
          ? 'bg-rose-500/20 text-rose-200'
          : riskLevel === 'high'
            ? 'bg-amber-500/20 text-amber-200'
            : riskLevel === 'medium'
              ? 'bg-sky-500/20 text-sky-200'
              : 'bg-emerald-500/20 text-emerald-200',
      )}
    >
      {riskLevel}
    </span>
  )
}

function SummaryCard({
  icon,
  title,
  children,
}: {
  icon: ReactNode
  title: string
  children: ReactNode
}) {
  return (
    <section className="rounded-[24px] border border-slate-800 bg-[#202020] p-4">
      <div className="mb-3 flex items-center gap-2">
        {icon}
        <h3 className="text-sm font-semibold text-white">{title}</h3>
      </div>
      <div className="space-y-3">{children}</div>
    </section>
  )
}

function SummaryRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-3 text-sm">
      <span className="text-slate-400">{label}</span>
      <span className="text-right text-slate-200">{value}</span>
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
          <label key={item.orgUnitId} className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
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
          <label key={item.locationId} className="rounded-2xl border border-slate-800 bg-slate-950/55 p-4">
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
