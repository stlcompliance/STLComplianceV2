import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import {
  createPersonOrgAssignment,
  createPersonRoleAssignment,
  createRoleTemplate,
  createOrgUnit,
  getEffectivePermissions,
  getManagerChain,
  getMe,
  getOrgUnits,
  getPermissionTemplates,
  getPermissionHistoryTimeline,
  getPersonOrgAssignments,
  getPersonRoleAssignments,
  getRoleTemplates,
  getSubordinateDetail,
  getSubordinates,
  getPeople,
  getPerson,
  StaffArrApiError,
  updatePersonManager,
  updatePersonOrgAssignment,
  updatePersonOrgAssignmentStatus,
  updatePersonRoleAssignmentStatus,
  updateRoleTemplate,
  upsertPermissionTemplate,
  updateOrgUnit,
  updateOrgUnitStatus,
} from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { ManagerHierarchyPanel } from '../components/ManagerHierarchyPanel'
import { canManageOrgHierarchy, OrgHierarchyManager } from '../components/OrgHierarchyManager'
import { PersonOrgAssignmentsManager } from '../components/PersonOrgAssignmentsManager'
import { PermissionProjectionTimelinePanel } from '../components/PermissionProjectionTimelinePanel'
import { RoleTemplateAssignmentPanel } from '../components/RoleTemplateAssignmentPanel'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const queryClient = useQueryClient()

  const meQuery = useQuery({
    queryKey: ['staffarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const peopleQuery = useQuery({
    queryKey: ['staffarr-people', session?.accessToken],
    queryFn: () => getPeople(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const orgUnitsQuery = useQuery({
    queryKey: ['staffarr-org-units', session?.accessToken],
    queryFn: () => getOrgUnits(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const [selectedPersonId, setSelectedPersonId] = useState<string | null>(null)
  const fallbackPersonId = peopleQuery.data?.[0]?.personId ?? meQuery.data?.personId ?? null
  useEffect(() => {
    if (!selectedPersonId && fallbackPersonId) {
      setSelectedPersonId(fallbackPersonId)
    }
  }, [fallbackPersonId, selectedPersonId])

  const effectivePersonId = selectedPersonId ?? fallbackPersonId
  const personProfileQuery = useQuery({
    queryKey: ['staffarr-person', session?.accessToken, effectivePersonId],
    queryFn: () => getPerson(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const assignmentQuery = useQuery({
    queryKey: ['staffarr-org-assignments', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonOrgAssignments(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const [selectedSubordinateId, setSelectedSubordinateId] = useState<string | null>(null)
  useEffect(() => {
    setSelectedSubordinateId(null)
  }, [effectivePersonId])
  const managerChainQuery = useQuery({
    queryKey: ['staffarr-manager-chain', session?.accessToken, effectivePersonId],
    queryFn: () => getManagerChain(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const subordinatesQuery = useQuery({
    queryKey: ['staffarr-subordinates', session?.accessToken, effectivePersonId],
    queryFn: () => getSubordinates(session!.accessToken, effectivePersonId!, true, 200),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const subordinateDetailQuery = useQuery({
    queryKey: ['staffarr-subordinate-detail', session?.accessToken, effectivePersonId, selectedSubordinateId],
    queryFn: () => getSubordinateDetail(session!.accessToken, effectivePersonId!, selectedSubordinateId!),
    enabled: Boolean(session?.accessToken && effectivePersonId && selectedSubordinateId),
  })
  const permissionTemplatesQuery = useQuery({
    queryKey: ['staffarr-permission-templates', session?.accessToken],
    queryFn: () => getPermissionTemplates(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const roleTemplatesQuery = useQuery({
    queryKey: ['staffarr-role-templates', session?.accessToken],
    queryFn: () => getRoleTemplates(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })
  const roleAssignmentsQuery = useQuery({
    queryKey: ['staffarr-role-assignments', session?.accessToken, effectivePersonId],
    queryFn: () => getPersonRoleAssignments(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const effectivePermissionsQuery = useQuery({
    queryKey: ['staffarr-effective-permissions', session?.accessToken, effectivePersonId],
    queryFn: () => getEffectivePermissions(session!.accessToken, effectivePersonId!),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })
  const permissionHistoryQuery = useQuery({
    queryKey: ['staffarr-permission-history', session?.accessToken, effectivePersonId],
    queryFn: () => getPermissionHistoryTimeline(session!.accessToken, effectivePersonId!, 100),
    enabled: Boolean(session?.accessToken && effectivePersonId),
  })

  const createOrgUnitMutation = useMutation({
    mutationFn: (payload: { unitType: string; name: string; parentOrgUnitId: string | null }) =>
      createOrgUnit(session!.accessToken, payload),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const updateOrgUnitMutation = useMutation({
    mutationFn: (payload: { orgUnitId: string; unitType: string; name: string; parentOrgUnitId: string | null }) =>
      updateOrgUnit(session!.accessToken, payload.orgUnitId, {
        unitType: payload.unitType,
        name: payload.name,
        parentOrgUnitId: payload.parentOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const updateOrgUnitStatusMutation = useMutation({
    mutationFn: (payload: { orgUnitId: string; status: 'active' | 'inactive' }) =>
      updateOrgUnitStatus(session!.accessToken, payload.orgUnitId, { status: payload.status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-units', session?.accessToken] })
    },
  })

  const createAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      siteOrgUnitId: string
      departmentOrgUnitId: string
      teamOrgUnitId: string
      positionOrgUnitId: string
    }) =>
      createPersonOrgAssignment(session!.accessToken, payload.personId, {
        siteOrgUnitId: payload.siteOrgUnitId,
        departmentOrgUnitId: payload.departmentOrgUnitId,
        teamOrgUnitId: payload.teamOrgUnitId,
        positionOrgUnitId: payload.positionOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })

  const updateAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      assignmentId: string
      siteOrgUnitId: string
      departmentOrgUnitId: string
      teamOrgUnitId: string
      positionOrgUnitId: string
    }) =>
      updatePersonOrgAssignment(session!.accessToken, payload.personId, payload.assignmentId, {
        siteOrgUnitId: payload.siteOrgUnitId,
        departmentOrgUnitId: payload.departmentOrgUnitId,
        teamOrgUnitId: payload.teamOrgUnitId,
        positionOrgUnitId: payload.positionOrgUnitId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })

  const updateAssignmentStatusMutation = useMutation({
    mutationFn: (payload: { personId: string; assignmentId: string; status: 'active' | 'inactive' }) =>
      updatePersonOrgAssignmentStatus(session!.accessToken, payload.personId, payload.assignmentId, { status: payload.status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['staffarr-org-assignments', session?.accessToken] })
    },
  })
  const updateManagerMutation = useMutation({
    mutationFn: (payload: { personId: string; managerPersonId: string | null }) =>
      updatePersonManager(session!.accessToken, payload.personId, {
        managerPersonId: payload.managerPersonId,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-people', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-person', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-manager-chain', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-subordinate-detail', session?.accessToken] }),
      ])
    },
  })
  const upsertPermissionTemplateMutation = useMutation({
    mutationFn: (payload: { permissionKey: string; name: string; description: string | null }) =>
      upsertPermissionTemplate(session!.accessToken, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
      ])
    },
  })
  const createRoleTemplateMutation = useMutation({
    mutationFn: (payload: {
      roleKey: string
      name: string
      description: string | null
      permissions: Array<{
        permissionTemplateId: string
        scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
        scopeValue: string | null
      }>
    }) => createRoleTemplate(session!.accessToken, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
      ])
    },
  })
  const updateRoleTemplateMutation = useMutation({
    mutationFn: (payload: {
      roleTemplateId: string
      name: string
      description: string | null
      status: 'active' | 'inactive'
      permissions: Array<{
        permissionTemplateId: string
        scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
        scopeValue: string | null
      }>
    }) =>
      updateRoleTemplate(session!.accessToken, payload.roleTemplateId, {
        name: payload.name,
        description: payload.description,
        status: payload.status,
        permissions: payload.permissions,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-templates', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
      ])
    },
  })
  const createRoleAssignmentMutation = useMutation({
    mutationFn: (payload: {
      personId: string
      roleTemplateId: string
      scopeType: 'tenant' | 'site' | 'department' | 'team' | 'position'
      scopeValue: string | null
    }) => createPersonRoleAssignment(session!.accessToken, payload.personId, payload),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
      ])
    },
  })
  const updateRoleAssignmentStatusMutation = useMutation({
    mutationFn: (payload: { personId: string; assignmentId: string; status: 'active' | 'inactive' }) =>
      updatePersonRoleAssignmentStatus(session!.accessToken, payload.personId, payload.assignmentId, payload.status),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['staffarr-role-assignments', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-effective-permissions', session?.accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['staffarr-permission-history', session?.accessToken] }),
      ])
    },
  })

  if (!session) {
    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">
          <h1 className="text-xl font-semibold text-white">StaffArr</h1>
          <p className="mt-4 text-sm text-slate-400">
            No active session. Launch from the suite to receive a handoff code.
          </p>
          <Link className="mt-6 inline-block text-sm text-sky-400 hover:underline" to="/launch">
            Open launch path
          </Link>
        </div>
      </main>
    )
  }

  if (meQuery.isLoading) {
    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <p className="text-slate-400">Loading your workspace…</p>
      </main>
    )
  }

  if (meQuery.isError || !meQuery.data) {
    if (meQuery.error instanceof StaffArrApiError && (meQuery.error.status === 401 || meQuery.error.status === 403)) {
      clearSession()
    }

    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">
          <h1 className="text-xl font-semibold text-white">StaffArr</h1>
          <p className="mt-4 text-sm text-red-300">
            {meQuery.error instanceof StaffArrApiError && meQuery.error.status === 403
              ? 'Your session is not entitled for StaffArr access.'
              : 'Could not load your StaffArr profile.'}
          </p>
          <p className="mt-2 text-xs text-slate-500">Relaunch StaffArr from the suite shell.</p>
        </div>
      </main>
    )
  }

  if (
    peopleQuery.isError ||
    orgUnitsQuery.isError ||
    assignmentQuery.isError ||
    managerChainQuery.isError ||
    subordinatesQuery.isError ||
    subordinateDetailQuery.isError
    || permissionTemplatesQuery.isError
    || roleTemplatesQuery.isError
    || roleAssignmentsQuery.isError
    || effectivePermissionsQuery.isError
    || permissionHistoryQuery.isError
  ) {
    const directoryError =
      peopleQuery.error ??
      orgUnitsQuery.error ??
      assignmentQuery.error ??
      managerChainQuery.error ??
      subordinatesQuery.error ??
      subordinateDetailQuery.error ??
      permissionTemplatesQuery.error ??
      roleTemplatesQuery.error ??
      roleAssignmentsQuery.error ??
      effectivePermissionsQuery.error ??
      permissionHistoryQuery.error
    if (directoryError instanceof StaffArrApiError && (directoryError.status === 401 || directoryError.status === 403)) {
      clearSession()
    }

    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">
          <h1 className="text-xl font-semibold text-white">StaffArr</h1>
          <p className="mt-4 text-sm text-red-300">Could not load people directory data.</p>
          <p className="mt-2 text-xs text-slate-500">Relaunch StaffArr from the suite shell.</p>
        </div>
      </main>
    )
  }

  const me = meQuery.data
  const people = peopleQuery.data ?? []
  const orgUnits = orgUnitsQuery.data ?? []
  const profile = personProfileQuery.data
  const selectedPerson = people.find((person) => person.personId === effectivePersonId) ?? null
  const assignments = assignmentQuery.data ?? []
  const managerChain = managerChainQuery.data ?? []
  const subordinates = subordinatesQuery.data ?? []
  const selectedSubordinateDetail = subordinateDetailQuery.data ?? null
  const permissionTemplates = permissionTemplatesQuery.data ?? []
  const roleTemplates = roleTemplatesQuery.data ?? []
  const roleAssignments = roleAssignmentsQuery.data ?? []
  const effectivePermissions = effectivePermissionsQuery.data ?? null
  const permissionHistory = permissionHistoryQuery.data ?? []
  const canManageOrgUnits = canManageOrgHierarchy(me.tenantRoleKey, me.isPlatformAdmin)
  const canManageHierarchy = canManageOrgUnits
  const orgMutationError =
    createOrgUnitMutation.error ?? updateOrgUnitMutation.error ?? updateOrgUnitStatusMutation.error ?? null
  const assignmentMutationError =
    createAssignmentMutation.error ?? updateAssignmentMutation.error ?? updateAssignmentStatusMutation.error ?? null
  const managerMutationError = updateManagerMutation.error
  const roleTemplateMutationError =
    upsertPermissionTemplateMutation.error ??
    createRoleTemplateMutation.error ??
    updateRoleTemplateMutation.error ??
    createRoleAssignmentMutation.error ??
    updateRoleAssignmentStatusMutation.error ??
    null

  return (
    <main className="mx-auto max-w-6xl p-8">
      <header className="border-b border-slate-700 pb-6">
        <p className="text-xs uppercase tracking-wide text-slate-500">STL Compliance</p>
        <h1 className="mt-1 text-3xl font-semibold text-white">StaffArr</h1>
        <p className="mt-2 text-slate-400">People directory and profile workspace</p>
      </header>

      <section className="mt-8 grid gap-6 lg:grid-cols-3">
        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Session context</h2>
          <dl className="mt-4 grid gap-3 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Signed in</dt>
              <dd className="text-right text-white">{me.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Role</dt>
              <dd className="text-right text-sky-300">{me.tenantRoleKey || 'tenant_member'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-slate-200">{me.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Job title</dt>
              <dd className="text-right text-slate-200">{me.jobTitle ?? 'Unspecified'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Person ID</dt>
              <dd className="text-right font-mono text-xs text-slate-300">{me.personId}</dd>
            </div>
          </dl>
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6 lg:col-span-2">
          <h2 className="text-sm font-medium text-slate-300">People directory</h2>
          {peopleQuery.isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading people…</p>
          ) : people.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No people have been added yet for this tenant.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-700">
              {people.map((person) => (
                <li key={person.personId} className="flex items-center justify-between py-3">
                  <button
                    type="button"
                    onClick={() => setSelectedPersonId(person.personId)}
                    className="text-left"
                  >
                    <p className="text-sm text-white">{person.displayName}</p>
                    <p className="text-xs text-slate-400">
                      {person.jobTitle ?? 'No title'} · {person.primaryEmail}
                    </p>
                  </button>
                  <span className="text-xs uppercase tracking-wide text-slate-500">{person.employmentStatus}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>

      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Selected profile</h2>
        {personProfileQuery.isLoading ? (
          <p className="mt-4 text-sm text-slate-400">Loading selected profile…</p>
        ) : !profile ? (
          <p className="mt-4 text-sm text-slate-400">No profile selected.</p>
        ) : (
          <dl className="mt-4 grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Name</dt>
              <dd className="text-right text-white">{profile.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Email</dt>
              <dd className="text-right text-white">{profile.primaryEmail}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-white">{profile.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Manager</dt>
              <dd className="text-right font-mono text-xs text-slate-300">{profile.managerPersonId ?? 'None'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Created</dt>
              <dd className="text-right text-slate-200">{new Date(profile.createdAt).toLocaleDateString()}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Updated</dt>
              <dd className="text-right text-slate-200">{new Date(profile.updatedAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        )}
      </section>

      {selectedPerson ? (
        <PersonOrgAssignmentsManager
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          assignments={assignments}
          canManage={canManageOrgUnits}
          isSubmitting={
            createAssignmentMutation.isPending || updateAssignmentMutation.isPending || updateAssignmentStatusMutation.isPending
          }
          errorMessage={
            assignmentMutationError instanceof StaffArrApiError
              ? assignmentMutationError.body || assignmentMutationError.message
              : null
          }
          onCreate={async (payload) => {
            await createAssignmentMutation.mutateAsync({ personId: selectedPerson.personId, ...payload })
          }}
          onUpdate={async (assignmentId, payload) => {
            await updateAssignmentMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              ...payload,
            })
          }}
          onStatusChange={async (assignmentId, status) => {
            await updateAssignmentStatusMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              status,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <ManagerHierarchyPanel
          selectedPersonId={selectedPerson.personId}
          selectedPersonDisplayName={selectedPerson.displayName}
          people={people}
          managerChain={managerChain}
          subordinates={subordinates}
          selectedSubordinate={selectedSubordinateDetail}
          canManage={canManageHierarchy}
          isSubmitting={updateManagerMutation.isPending}
          errorMessage={
            managerMutationError instanceof StaffArrApiError
              ? managerMutationError.body || managerMutationError.message
              : null
          }
          onSelectSubordinate={(subordinatePersonId) => setSelectedSubordinateId(subordinatePersonId)}
          onUpdateManager={async (managerPersonId) => {
            await updateManagerMutation.mutateAsync({
              personId: selectedPerson.personId,
              managerPersonId,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <RoleTemplateAssignmentPanel
          personId={selectedPerson.personId}
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          permissionTemplates={permissionTemplates}
          roleTemplates={roleTemplates}
          roleAssignments={roleAssignments}
          canManage={canManageHierarchy}
          isSubmitting={
            upsertPermissionTemplateMutation.isPending ||
            createRoleTemplateMutation.isPending ||
            updateRoleTemplateMutation.isPending ||
            createRoleAssignmentMutation.isPending ||
            updateRoleAssignmentStatusMutation.isPending
          }
          errorMessage={
            roleTemplateMutationError instanceof StaffArrApiError
              ? roleTemplateMutationError.body || roleTemplateMutationError.message
              : null
          }
          onUpsertPermissionTemplate={async (payload) => {
            await upsertPermissionTemplateMutation.mutateAsync(payload)
          }}
          onCreateRoleTemplate={async (payload) => {
            await createRoleTemplateMutation.mutateAsync(payload)
          }}
          onUpdateRoleTemplateStatus={async (roleTemplateId, status) => {
            const existing = roleTemplates.find((role) => role.roleTemplateId === roleTemplateId)
            if (!existing) {
              return
            }

            await updateRoleTemplateMutation.mutateAsync({
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
            await createRoleAssignmentMutation.mutateAsync({
              personId: selectedPerson.personId,
              ...payload,
            })
          }}
          onUpdateRoleAssignmentStatus={async (assignmentId, status) => {
            await updateRoleAssignmentStatusMutation.mutateAsync({
              personId: selectedPerson.personId,
              assignmentId,
              status,
            })
          }}
        />
      ) : null}

      {selectedPerson ? (
        <PermissionProjectionTimelinePanel
          personDisplayName={selectedPerson.displayName}
          orgUnits={orgUnits}
          projection={effectivePermissions}
          timeline={permissionHistory}
        />
      ) : null}

      <OrgHierarchyManager
        orgUnits={orgUnits}
        canManage={canManageOrgUnits}
        isSubmitting={
          createOrgUnitMutation.isPending || updateOrgUnitMutation.isPending || updateOrgUnitStatusMutation.isPending
        }
        errorMessage={orgMutationError instanceof StaffArrApiError ? orgMutationError.body || orgMutationError.message : null}
        onCreate={async (payload) => {
          await createOrgUnitMutation.mutateAsync(payload)
        }}
        onUpdate={async (orgUnitId, payload) => {
          await updateOrgUnitMutation.mutateAsync({ orgUnitId, ...payload })
        }}
        onStatusChange={async (orgUnitId, status) => {
          await updateOrgUnitStatusMutation.mutateAsync({ orgUnitId, status })
        }}
      />

      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Signed in</h2>
        <dl className="mt-4 grid gap-3 text-sm">
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Name</dt>
            <dd className="text-right text-white">{me.displayName}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Email</dt>
            <dd className="text-right text-white">{me.email}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Tenant</dt>
            <dd className="text-right font-mono text-xs text-slate-300">{session.tenantSlug}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Org units loaded</dt>
            <dd className="text-right text-white">{orgUnits.length}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">StaffArr entitlement</dt>
            <dd className="text-right text-emerald-400">
              {me.hasStaffArrEntitlement ? 'Active' : 'Missing'}
            </dd>
          </div>
        </dl>
      </section>
    </main>
  )
}
