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
import type { StaffArrWorkspaceState } from '../useStaffArrWorkspaceState'

type Props = { state: StaffArrWorkspaceState }

export function PeopleSection({ state }: Props) {
  const s = state
  return (
    <>
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
            <div className="flex justify-between gap-4">
              <dt className="text-slate-500">Person ID</dt>
              <dd className="text-right font-mono text-xs text-slate-300">{s.me.personId}</dd>
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
                    const anchorId =
                      s.activeDirectoryPersonId ?? s.selectedPerson?.personId ?? s.filteredPeople[0]!.personId
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
                    s.setSelectedPersonId(s.activeDirectoryPersonId ?? s.filteredPeople[0]!.personId)
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
          ) : (
            <ul className="mt-4 divide-y divide-slate-700">
              {s.filteredPeople.map((person) => {
                const isSelected = s.effectivePersonId === person.personId
                const isActive =
                  Boolean(s.peopleDirectoryQuery.trim()) && s.activeDirectoryPersonId === person.personId
                const buttonClass = isSelected
                  ? 'w-full rounded-md px-1 py-1 text-left text-sky-200'
                  : isActive
                    ? 'w-full rounded-md px-1 py-1 text-left text-slate-100 ring-1 ring-sky-500/70'
                    : 'w-full rounded-md px-1 py-1 text-left'

                return (
                <li key={person.personId} className="flex items-center justify-between py-3">
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
              <dd className="text-right font-mono text-xs text-slate-300">{s.profile.managerPersonId ?? 'None'}</dd>
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

      {s.profile ? (
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

      {s.selectedPerson ? (
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

      {s.selectedPerson ? (
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

      {s.effectivePersonId && (s.selectedPerson ?? s.personProfileQuery.data) ? (
        <WorkforceOnboardingJourneyPanel
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

      {s.selectedPerson ? (
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

      {s.selectedPerson ? (
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

      {s.selectedPerson ? (
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

      {s.selectedPerson ? (
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

      {s.selectedPerson ? (
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
