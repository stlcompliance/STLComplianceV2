import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import {
  createOrgUnit,
  getMe,
  getOrgUnits,
  getPeople,
  getPerson,
  StaffArrApiError,
  updateOrgUnit,
  updateOrgUnitStatus,
} from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { canManageOrgHierarchy, OrgHierarchyManager } from '../components/OrgHierarchyManager'

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
  const selectedPersonId = peopleQuery.data?.[0]?.personId ?? meQuery.data?.personId
  const personProfileQuery = useQuery({
    queryKey: ['staffarr-person', session?.accessToken, selectedPersonId],
    queryFn: () => getPerson(session!.accessToken, selectedPersonId!),
    enabled: Boolean(session?.accessToken && selectedPersonId),
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

  if (peopleQuery.isError || orgUnitsQuery.isError) {
    const directoryError = peopleQuery.error ?? orgUnitsQuery.error
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
  const canManageOrgUnits = canManageOrgHierarchy(me.tenantRoleKey, me.isPlatformAdmin)
  const orgMutationError =
    createOrgUnitMutation.error ?? updateOrgUnitMutation.error ?? updateOrgUnitStatusMutation.error ?? null

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
                  <div>
                    <p className="text-sm text-white">{person.displayName}</p>
                    <p className="text-xs text-slate-400">
                      {person.jobTitle ?? 'No title'} · {person.primaryEmail}
                    </p>
                  </div>
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
