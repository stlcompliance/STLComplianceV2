import { StaffArrApiError } from '../../api/client'
import { CreatePersonPanel } from '../../components/CreatePersonPanel'
import { PersonProfileEditorPanel } from '../../components/PersonProfileEditorPanel'
import { PersonLookupPanel } from '../../components/PersonLookupPanel'
import { PersonTimelinePanel } from '../../components/PersonTimelinePanel'
import { PersonTrainarrTrainingHistoryPanel } from '../../components/PersonTrainarrTrainingHistoryPanel'
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
          {s.peopleQuery.isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading people…</p>
          ) : s.people.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No people have been added yet for this tenant.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-700">
              {s.people.map((person) => (
                <li key={person.personId} className="flex items-center justify-between py-3">
                  <button type="button" onClick={() => s.setSelectedPersonId(person.personId)} className="text-left">
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

      <CreatePersonPanel
        orgUnits={s.orgUnits}
        peopleOptions={s.people.map((person) => ({
          personId: person.personId,
          displayName: person.displayName,
        }))}
        canManage={s.canManagePeopleProfiles}
        isSubmitting={s.createPersonMutation.isPending}
        errorMessage={
          s.createPersonMutation.error instanceof StaffArrApiError
            ? s.createPersonMutation.error.body || s.createPersonMutation.error.message
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
            s.personProfileMutationError instanceof StaffArrApiError
              ? s.personProfileMutationError.body || s.personProfileMutationError.message
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
          selectedNote={s.noteDetailQuery.data ?? null}
          isLoading={s.personNotesQuery.isLoading}
          isLoadingDetail={s.noteDetailQuery.isLoading}
          canManage={s.canManagePersonNotes}
          isSubmitting={s.createNoteMutation.isPending}
          errorMessage={
            s.noteMutationError instanceof StaffArrApiError
              ? s.noteMutationError.body || s.noteMutationError.message
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
          selectedDocument={s.documentDetailQuery.data ?? null}
          isLoading={s.personDocumentsQuery.isLoading}
          isLoadingDetail={s.documentDetailQuery.isLoading}
          canManage={s.canManagePersonDocuments}
          isSubmitting={s.uploadDocumentMutation.isPending}
          errorMessage={
            s.documentMutationError instanceof StaffArrApiError
              ? s.documentMutationError.body || s.documentMutationError.message
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

      {s.selectedPerson ? (
        <PersonLookupPanel
          personId={s.selectedPerson.personId}
          personDisplayName={s.selectedPerson.displayName}
          lookup={s.personLookupQuery.data ?? null}
          isLoading={s.personLookupQuery.isLoading}
        />
      ) : null}

      {s.selectedPerson ? (
        <PersonHistorySummaryPanel
          personDisplayName={s.selectedPerson.displayName}
          summary={s.personHistorySummaryQuery.data ?? null}
          isLoading={s.personHistorySummaryQuery.isLoading}
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
        />
      ) : null}
    </>
  )
}
