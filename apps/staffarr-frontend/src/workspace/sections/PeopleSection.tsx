import { getErrorMessage } from '@stl/shared-ui'
import { CreatePersonPanel } from '../../components/CreatePersonPanel'
import { PersonProfileEditorPanel } from '../../components/PersonProfileEditorPanel'
import { PersonLookupPanel } from '../../components/PersonLookupPanel'
import { PersonTimelinePanel } from '../../components/PersonTimelinePanel'
import { PersonTrainarrTrainingHistoryPanel } from '../../components/PersonTrainarrTrainingHistoryPanel'
import { WorkforceOnboardingJourneyPanel } from '../../components/WorkforceOnboardingJourneyPanel'
import { PersonOffboardingPanel } from '../../components/PersonOffboardingPanel'
import { PersonHistorySummaryPanel } from '../../components/PersonHistorySummaryPanel'
import { PersonnelNotesPanel } from '../../components/PersonnelNotesPanel'
import { PersonnelDocumentsPanel } from '../../components/PersonnelDocumentsPanel'
import { Link, useLocation } from 'react-router-dom'
import { useEffect, useMemo, useState } from 'react'
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }
type PeopleViewMode = 'drawer' | 'details' | 'create'
type DrawerColumnKey = 'name' | 'email' | 'jobTitle' | 'orgUnit' | 'status' | 'manager'

const PEOPLE_DRAWER_COLUMN_STORAGE_KEY = 'staffarr.people.drawer.columns.v1'

const ALL_DRAWER_COLUMNS: Array<{ key: DrawerColumnKey; label: string }> = [
  { key: 'name', label: 'Name' },
  { key: 'email', label: 'Email' },
  { key: 'jobTitle', label: 'Job title' },
  { key: 'orgUnit', label: 'Org unit' },
  { key: 'status', label: 'Status' },
  { key: 'manager', label: 'Manager' },
]

const DEFAULT_DRAWER_COLUMNS: DrawerColumnKey[] = ['name', 'email', 'jobTitle', 'orgUnit', 'status']

export function PeopleSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const [selectedColumns, setSelectedColumns] = useState<DrawerColumnKey[]>(DEFAULT_DRAWER_COLUMNS)
  const managerDisplayName = s.profile?.managerPersonId
    ? s.people.find((person) => person.personId === s.profile!.managerPersonId)?.displayName ?? 'Assigned'
    : 'None'
  const selectedPersonId = s.selectedPerson?.personId ?? null
  const activeFilteredPersonId = (() => {
    if (!s.peopleDirectoryQuery.trim() || s.filteredPeople.length === 0) {
      return null
    }
    if (s.activeDirectoryPersonId && s.filteredPeople.some((person) => person.personId === s.activeDirectoryPersonId)) {
      return s.activeDirectoryPersonId
    }
    if (selectedPersonId && s.filteredPeople.some((person) => person.personId === selectedPersonId)) {
      return selectedPersonId
    }
    return s.filteredPeople[0]!.personId
  })()
  const mode: PeopleViewMode = location.pathname.startsWith('/people/create')
    ? 'create'
    : location.pathname.startsWith('/people/details')
      ? 'details'
      : 'drawer'

  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(PEOPLE_DRAWER_COLUMN_STORAGE_KEY)
      if (!raw) return
      const parsed = JSON.parse(raw) as DrawerColumnKey[]
      const valid = parsed.filter((column) => ALL_DRAWER_COLUMNS.some((candidate) => candidate.key === column))
      if (valid.length > 0) {
        setSelectedColumns(valid.slice(0, 5))
      }
    } catch {
      // Ignore malformed persisted state.
    }
  }, [])

  useEffect(() => {
    window.localStorage.setItem(PEOPLE_DRAWER_COLUMN_STORAGE_KEY, JSON.stringify(selectedColumns))
  }, [selectedColumns])

  const visibleColumns = useMemo(() => {
    const picked = selectedColumns
      .filter((column) => ALL_DRAWER_COLUMNS.some((candidate) => candidate.key === column))
      .slice(0, 5)
    return picked.length > 0 ? picked : DEFAULT_DRAWER_COLUMNS
  }, [selectedColumns])

  const toggleColumn = (column: DrawerColumnKey) => {
    setSelectedColumns((previous) => {
      if (previous.includes(column)) {
        const next = previous.filter((item) => item !== column)
        return next.length > 0 ? next : previous
      }
      if (previous.length >= 5) {
        return previous
      }
      return [...previous, column]
    })
  }

  const managerNameByPersonId = useMemo(() => {
    return new Map(s.people.map((person) => [person.personId, person.displayName]))
  }, [s.people])

  const cellValue = (person: (typeof s.filteredPeople)[number], column: DrawerColumnKey): string => {
    switch (column) {
      case 'name':
        return person.displayName
      case 'email':
        return person.primaryEmail
      case 'jobTitle':
        return person.jobTitle ?? 'Unspecified'
      case 'orgUnit':
        return person.primaryOrgUnitName ?? 'Unassigned'
      case 'status':
        return person.employmentStatus
      case 'manager':
        return person.managerPersonId ? managerNameByPersonId.get(person.managerPersonId) ?? 'Assigned' : 'None'
      default:
        return ''
    }
  }

  return (
    <>
      {mode === 'create' ? (
        <div className="rounded-xl border border-teal-700/50 bg-teal-950/20 p-4 text-sm text-teal-100">
          <p>Create people in a guided flow using friendly business fields only.</p>
          <ol className="mt-2 list-decimal space-y-1 pl-5">
            <li>Step 1: Add identity fields so this person can be recognized in staffing workflows.</li>
            <li>Step 2: Set organization placement to route assignments and approvals correctly.</li>
            <li>Step 3: Confirm role and status so readiness and training logic stays accurate.</li>
          </ol>
        </div>
      ) : null}
      {mode === 'details' ? (
        <div className="rounded-xl border border-sky-700/50 bg-sky-950/20 p-4 text-sm text-sky-100">
          People details view centers on the selected person and their profile context.
        </div>
      ) : null}
      <section className="mt-8 grid gap-6 lg:grid-cols-3">
        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-sm font-medium text-slate-300">Session context</h2>
          <dl className="mt-4 grid gap-3 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Signed in</dt>
              <dd className="text-right text-white">{s.me.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Role</dt>
              <dd className="text-right text-sky-300">{s.me.tenantRoleKey || 'tenant_member'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-slate-200">{s.me.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Job title</dt>
              <dd className="text-right text-slate-200">{s.me.jobTitle ?? 'Unspecified'}</dd>
            </div>
          </dl>
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-6 lg:col-span-2">
          <h2 className="text-sm font-medium text-slate-300">People directory</h2>
          <div className="mt-3 space-y-2">
            <label className="block text-xs font-medium uppercase tracking-wide text-slate-400" htmlFor="workspace-directory-filter">
              Quick filter
            </label>
            <div className="flex items-center gap-2">
              <input
                id="workspace-directory-filter"
                type="search"
                aria-label="People quick filter"
                data-testid="workspace-people-directory-filter"
                value={s.peopleDirectoryQuery}
                onChange={(event) => s.setPeopleDirectoryQuery(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === 'Escape' && s.peopleDirectoryQuery) {
                    event.preventDefault()
                    s.setPeopleDirectoryQuery('')
                    return
                  }
                  if (
                    (event.key === 'ArrowDown' || event.key === 'ArrowUp') &&
                    s.peopleDirectoryQuery.trim() &&
                    s.filteredPeople.length > 0
                  ) {
                    event.preventDefault()
                    const anchorId = activeFilteredPersonId ?? s.filteredPeople[0]!.personId
                    const currentIndex = s.filteredPeople.findIndex((person) => person.personId === anchorId)
                    const startIndex = currentIndex >= 0 ? currentIndex : 0
                    const nextIndex =
                      event.key === 'ArrowDown'
                        ? (startIndex + 1) % s.filteredPeople.length
                        : (startIndex - 1 + s.filteredPeople.length) % s.filteredPeople.length
                    s.setActiveDirectoryPersonId(s.filteredPeople[nextIndex]!.personId)
                    return
                  }
                  if (event.key === 'Enter' && s.peopleDirectoryQuery.trim() && s.filteredPeople.length > 0) {
                    event.preventDefault()
                    s.setSelectedPersonId(activeFilteredPersonId ?? s.filteredPeople[0]!.personId)
                  }
                }}
                placeholder="Search by name, email, title, org unit, or status"
                className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white placeholder:text-slate-500 focus:border-sky-500 focus:outline-none"
              />
              {s.peopleDirectoryQuery ? (
                <button
                  type="button"
                  onClick={() => s.setPeopleDirectoryQuery('')}
                  className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-300 hover:border-slate-500 hover:text-white"
                >
                  Clear
                </button>
              ) : null}
            </div>
            {!s.peopleQuery.isLoading && s.people.length > 0 ? (
              <p className="text-xs text-slate-500" aria-live="polite">
                Showing {s.filteredPeople.length} of {s.people.length} people
              </p>
            ) : null}
            {!s.peopleQuery.isLoading && s.peopleDirectoryQuery.trim() && s.filteredPeople.length > 0 ? (
              <p className="text-xs text-slate-500">Use ↑/↓ to move through results, then press Enter to select.</p>
            ) : null}
            {s.selectedPersonHiddenByFilter ? (
              <div className="rounded-md border border-amber-700/60 bg-amber-950/20 p-2 text-xs text-amber-200">
                The selected person is hidden by the current filter.
                <button
                  type="button"
                  onClick={() => s.setPeopleDirectoryQuery('')}
                  className="ml-2 underline decoration-amber-400/70 underline-offset-2 hover:text-amber-100"
                >
                  Clear filter to show selection
                </button>
              </div>
            ) : null}
          </div>
          {s.peopleQuery.isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading people…</p>
          ) : s.people.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No people have been added yet for this tenant.</p>
          ) : s.filteredPeople.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400" aria-live="polite">
              No people match the current filter. Try a different name, email, or status.
            </p>
          ) : mode === 'drawer' ? (
            <div className="mt-4 space-y-3">
              <div className="rounded-md border border-slate-700 p-2">
                <p className="text-xs text-slate-400">Visible columns (max 5)</p>
                <div className="mt-2 flex flex-wrap gap-3">
                  {ALL_DRAWER_COLUMNS.map((column) => (
                    <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                      <input
                        type="checkbox"
                        checked={visibleColumns.includes(column.key)}
                        onChange={() => toggleColumn(column.key)}
                      />
                      {column.label}
                    </label>
                  ))}
                </div>
              </div>
              <div className="overflow-x-auto rounded-md border border-slate-700">
                <table className="min-w-full text-left text-sm">
                  <thead className="bg-slate-950/70">
                    <tr>
                      {visibleColumns.map((column) => (
                        <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                          {ALL_DRAWER_COLUMNS.find((item) => item.key === column)?.label}
                        </th>
                      ))}
                      <th className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {s.filteredPeople.map((person) => (
                      <tr key={person.personId} className="border-t border-slate-800">
                        {visibleColumns.map((column) => (
                          <td key={`${person.personId}-${column}`} className="px-3 py-2 text-slate-200">
                            {cellValue(person, column)}
                          </td>
                        ))}
                        <td className="px-3 py-2">
                          <div className="flex items-center gap-2 text-xs">
                            <Link
                              to="/people/details"
                              onClick={() => s.setSelectedPersonId(person.personId)}
                              className="text-sky-300 hover:text-sky-200 hover:underline"
                            >
                              View
                            </Link>
                            <Link
                              to="/people/create"
                              onClick={() => s.setSelectedPersonId(person.personId)}
                              className="text-emerald-300 hover:text-emerald-200 hover:underline"
                            >
                              Edit
                            </Link>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <ul className="mt-4 divide-y divide-slate-700">
              {s.filteredPeople.map((person) => {
                const isSelected = s.effectivePersonId === person.personId
                const isActive =
                  Boolean(s.peopleDirectoryQuery.trim()) && activeFilteredPersonId === person.personId
                const buttonClass = isSelected
                  ? 'w-full rounded-md px-1 py-1 text-left text-sky-200'
                  : isActive
                    ? 'w-full rounded-md px-1 py-1 text-left text-slate-100 ring-1 ring-sky-500/70'
                    : 'w-full rounded-md px-1 py-1 text-left'

                return (
                  <li key={person.personId} className="flex items-center justify-between gap-4 py-3">
                    <button
                      type="button"
                      onMouseEnter={() => s.setActiveDirectoryPersonId(person.personId)}
                      onClick={() => {
                        s.setActiveDirectoryPersonId(person.personId)
                        s.setSelectedPersonId(person.personId)
                      }}
                      className={buttonClass}
                    >
                      <p className="text-sm text-white">{person.displayName}</p>
                      <p className="text-xs text-slate-400">
                        {person.jobTitle ?? 'No title'} · {person.primaryEmail}
                      </p>
                    </button>
                    <span className="text-xs uppercase tracking-wide text-slate-500">{person.employmentStatus}</span>
                  </li>
                )
              })}
            </ul>
          )}
        </div>
      </section>

      {mode === 'create' ? (
        <CreatePersonPanel
          orgUnits={s.orgUnits}
          peopleOptions={s.people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          canManage={s.canManagePeopleProfiles}
          isSubmitting={s.createPersonMutation.isPending}
          errorMessage={
            s.createPersonMutation.error
              ? getErrorMessage(s.createPersonMutation.error, 'Failed to create person profile.')
              : null
          }
          onCreate={async (request) => {
            await s.createPersonMutation.mutateAsync(request)
          }}
        />
      ) : null}

      {mode === 'details' ? (
      <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Selected profile</h2>
        {s.personProfileQuery.isLoading ? (
          <p className="mt-4 text-sm text-slate-400">Loading selected profile…</p>
        ) : !s.profile ? (
          <p className="mt-4 text-sm text-slate-400">No profile selected.</p>
        ) : (
          <dl className="mt-4 grid gap-3 text-sm md:grid-cols-2">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Name</dt>
              <dd className="text-right text-white">{s.profile.displayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Email</dt>
              <dd className="text-right text-white">{s.profile.primaryEmail}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Org unit</dt>
              <dd className="text-right text-white">{s.profile.primaryOrgUnitName ?? 'Unassigned'}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Manager</dt>
              <dd className="text-right text-white">{managerDisplayName}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Created</dt>
              <dd className="text-right text-slate-200">{new Date(s.profile.createdAt).toLocaleDateString()}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Updated</dt>
              <dd className="text-right text-slate-200">{new Date(s.profile.updatedAt).toLocaleDateString()}</dd>
            </div>
          </dl>
        )}
      </section>
      ) : null}

      {s.profile && mode === 'details' ? (
        <PersonProfileEditorPanel
          profile={s.profile}
          orgUnits={s.orgUnits}
          peopleOptions={s.people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          canManage={s.canManagePeopleProfiles}
          isSubmitting={s.updatePersonMutation.isPending || s.updateEmploymentStatusMutation.isPending}
          errorMessage={
            s.personProfileMutationError
              ? getErrorMessage(s.personProfileMutationError, 'Failed to update person profile.')
              : null
          }
          onUpdate={async (request) => {
            await s.updatePersonMutation.mutateAsync({
              personId: s.profile!.personId,
              ...request,
            })
          }}
          onEmploymentStatusChange={async (request) => {
            await s.updateEmploymentStatusMutation.mutateAsync({
              personId: s.profile!.personId,
              ...request,
            })
          }}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonnelNotesPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          notes={s.personNotes}
          selectedNoteId={s.selectedNoteId}
          selectedNote={s.noteDetailQuery.data ?? null}
          isLoading={s.personNotesQuery.isLoading}
          isError={s.personNotesQuery.isError}
          readErrorMessage={
            s.personNotesQuery.isError
              ? getErrorMessage(
                  s.personNotesQuery.error,
                  'Failed to load personnel notes.',
                )
              : null
          }
          onRetryRead={() => void s.personNotesQuery.refetch()}
          isLoadingDetail={s.noteDetailQuery.isLoading}
          isDetailError={s.noteDetailQuery.isError}
          detailErrorMessage={
            s.noteDetailQuery.isError
              ? getErrorMessage(
                  s.noteDetailQuery.error,
                  'Failed to load note detail.',
                )
              : null
          }
          onRetryDetail={() => void s.noteDetailQuery.refetch()}
          canManage={s.canManagePersonNotes}
          isSubmitting={s.createNoteMutation.isPending}
          actionErrorMessage={
            s.noteMutationError
              ? getErrorMessage(s.noteMutationError, 'Failed to save personnel note.')
              : null
          }
          onSelectNote={s.setSelectedNoteId}
          onCreateNote={async (payload) => {
            await s.createNoteMutation.mutateAsync(payload)
          }}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonnelDocumentsPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          accessToken={s.accessToken}
          documents={s.personDocuments}
          selectedDocumentId={s.selectedDocumentId}
          selectedDocument={s.documentDetailQuery.data ?? null}
          isLoading={s.personDocumentsQuery.isLoading}
          isError={s.personDocumentsQuery.isError}
          readErrorMessage={
            s.personDocumentsQuery.isError
              ? getErrorMessage(
                  s.personDocumentsQuery.error,
                  'Failed to load personnel documents.',
                )
              : null
          }
          onRetryRead={() => void s.personDocumentsQuery.refetch()}
          isLoadingDetail={s.documentDetailQuery.isLoading}
          isDetailError={s.documentDetailQuery.isError}
          detailErrorMessage={
            s.documentDetailQuery.isError
              ? getErrorMessage(
                  s.documentDetailQuery.error,
                  'Failed to load document detail.',
                )
              : null
          }
          onRetryDetail={() => void s.documentDetailQuery.refetch()}
          canManage={s.canManagePersonDocuments}
          isSubmitting={s.uploadDocumentMutation.isPending}
          actionErrorMessage={
            s.documentMutationError
              ? getErrorMessage(s.documentMutationError, 'Failed to upload personnel document.')
              : null
          }
          onSelectDocument={s.setSelectedDocumentId}
          onUploadDocument={async (payload) => {
            await s.uploadDocumentMutation.mutateAsync(payload)
          }}
          contentUrlFor={(documentId) =>
            s.personnelDocumentContentUrl(s.selectedPerson!.personId, documentId)
          }
        />
      ) : null}

      {mode === 'details' && s.effectivePersonId && (s.selectedPerson ?? s.personProfileQuery.data) ? (
        <WorkforceOnboardingJourneyPanel
          accessToken={s.accessToken}
          personDisplayName={
            s.selectedPerson?.displayName ?? s.personProfileQuery.data!.displayName
          }
          journey={s.workforceOnboardingJourneyQuery.data ?? null}
          isLoading={s.workforceOnboardingJourneyQuery.isLoading}
          isError={s.workforceOnboardingJourneyQuery.isError}
          readErrorMessage={
            s.workforceOnboardingJourneyQuery.isError
              ? getErrorMessage(
                  s.workforceOnboardingJourneyQuery.error,
                  'Failed to load workforce onboarding journey.',
                )
              : null
          }
          onRetryRead={() => void s.workforceOnboardingJourneyQuery.refetch()}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonOffboardingPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          peopleOptions={s.people.map((person) => ({
            personId: person.personId,
            displayName: person.displayName,
          }))}
          offboarding={s.personOffboardingQuery.data ?? null}
          isLoading={s.personOffboardingQuery.isLoading}
          isError={s.personOffboardingQuery.isError}
          readErrorMessage={
            s.personOffboardingQuery.isError
              ? getErrorMessage(
                  s.personOffboardingQuery.error,
                  'Failed to load offboarding workflow state.',
                )
              : null
          }
          onRetryRead={() => void s.personOffboardingQuery.refetch()}
          canManage={s.canManagePeopleProfiles}
          isSubmitting={s.startOffboardingMutation.isPending || s.executeOffboardingMutation.isPending}
          actionErrorMessage={
            s.offboardingMutationError
              ? getErrorMessage(s.offboardingMutationError, 'Failed to update offboarding workflow.')
              : null
          }
          onStart={async (request) => {
            await s.startOffboardingMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              ...request,
            })
          }}
          onExecute={async (request) => {
            const offboarding = s.personOffboardingQuery.data
            if (!offboarding) {
              return
            }

            await s.executeOffboardingMutation.mutateAsync({
              personId: s.selectedPerson!.personId,
              offboardingId: offboarding.offboardingId,
              ...request,
            })
          }}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonLookupPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          lookup={s.personLookupQuery.data ?? null}
          isLoading={s.personLookupQuery.isLoading}
          isError={s.personLookupQuery.isError}
          readErrorMessage={
            s.personLookupQuery.isError
              ? getErrorMessage(
                  s.personLookupQuery.error,
                  'Failed to load person identity and placement details.',
                )
              : null
          }
          onRetryRead={() => void s.personLookupQuery.refetch()}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonHistorySummaryPanel
          personDisplayName={s.selectedPerson.displayName}
          summary={s.personHistorySummaryQuery.data ?? null}
          isLoading={s.personHistorySummaryQuery.isLoading}
          isError={s.personHistorySummaryQuery.isError}
          readErrorMessage={
            s.personHistorySummaryQuery.isError
              ? getErrorMessage(
                  s.personHistorySummaryQuery.error,
                  'Failed to load personnel history summary.',
                )
              : null
          }
          onRetryRead={() => void s.personHistorySummaryQuery.refetch()}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonTimelinePanel
          personDisplayName={s.selectedPerson.displayName}
          entries={s.personTimelineEntries}
          totalCount={s.personTimelineTotalCount}
          page={s.personTimelinePage}
          pageSize={s.personTimelinePageSize}
          hasNextPage={s.personTimelineHasNextPage}
          categoryFilter={s.personTimelineCategoryFilter}
          isLoading={s.personTimelineQuery.isLoading}
          isError={s.personTimelineQuery.isError}
          readErrorMessage={
            s.personTimelineQuery.isError
              ? getErrorMessage(
                  s.personTimelineQuery.error,
                  'Failed to load person timeline events.',
                )
              : null
          }
          onRetryRead={() => void s.personTimelineQuery.refetch()}
          onCategoryFilterChange={s.setPersonTimelineCategoryFilter}
          onPageChange={s.setPersonTimelinePage}
          onPageSizeChange={s.setPersonTimelinePageSize}
        />
      ) : null}

      {s.selectedPerson && mode === 'details' ? (
        <PersonTrainarrTrainingHistoryPanel
          personDisplayName={s.selectedPerson.displayName}
          history={s.trainarrTrainingHistoryQuery.data ?? null}
          isLoading={s.trainarrTrainingHistoryQuery.isLoading}
          isError={s.trainarrTrainingHistoryQuery.isError}
          readErrorMessage={
            s.trainarrTrainingHistoryQuery.isError
              ? getErrorMessage(
                  s.trainarrTrainingHistoryQuery.error,
                  'Failed to load TrainArr training history.',
                )
              : null
          }
          onRetryRead={() => void s.trainarrTrainingHistoryQuery.refetch()}
        />
      ) : null}
    </>
  )
}
