import type {
  TrainingDefinitionResponse,
  TrainingProgramDetailResponse,
  TrainingProgramSummaryResponse,
  TrainingProgramVersionSummaryResponse,
} from '../api/types'

interface ProgramBuilderPanelProps {
  programs: TrainingProgramSummaryResponse[]
  definitions: TrainingDefinitionResponse[]
  selectedDefinitionIds: string[]
  selectedProgramId: string | null
  selectedProgramDetail: TrainingProgramDetailResponse | null | undefined
  programVersions: TrainingProgramVersionSummaryResponse[]
  selectedDefinitionIdForCitations: string | null
  onSelectProgram: (programId: string) => void
  onSelectDefinitionForCitations: (definitionId: string) => void
  programKey: string
  programName: string
  programDescription: string
  onProgramKeyChange: (value: string) => void
  onProgramNameChange: (value: string) => void
  onProgramDescriptionChange: (value: string) => void
  onToggleDefinition: (definitionId: string) => void
  onCreateProgram: () => void
  onSaveProgram: () => void
  onPublishProgram: () => void
  onStartRevision: () => void
  isCreating: boolean
  isSaving: boolean
  isPublishing: boolean
  isStartingRevision: boolean
  canManage: boolean
}

export function ProgramBuilderPanel({
  programs,
  definitions,
  selectedDefinitionIds,
  selectedProgramId,
  selectedProgramDetail,
  programVersions,
  selectedDefinitionIdForCitations,
  onSelectProgram,
  onSelectDefinitionForCitations,
  programKey,
  programName,
  programDescription,
  onProgramKeyChange,
  onProgramNameChange,
  onProgramDescriptionChange,
  onToggleDefinition,
  onCreateProgram,
  onSaveProgram,
  onPublishProgram,
  onStartRevision,
  isCreating,
  isSaving,
  isPublishing,
  isStartingRevision,
  canManage,
}: ProgramBuilderPanelProps) {
  const isEditing = Boolean(selectedProgramId)
  const editStatus = selectedProgramDetail?.status ?? 'draft'
  const canPublish =
    isEditing &&
    editStatus === 'draft' &&
    selectedDefinitionIds.length > 0 &&
    programName.trim().length >= 3 &&
    programDescription.trim().length >= 8

  const definitionPicker = (
    <fieldset className="mt-3">
      <legend className="text-xs text-slate-400">Training definitions</legend>
      {definitions.length === 0 ? (
        <p className="mt-2 text-sm text-slate-500">Create training definitions before building a program.</p>
      ) : (
        <ul className="mt-2 space-y-1">
          {definitions.map((definition) => (
            <li key={definition.trainingDefinitionId} className="flex flex-wrap items-center gap-2">
              <label htmlFor={`program-builder-definition-${definition.trainingDefinitionId}`} className="flex items-center gap-2 text-sm text-slate-200">
                <input
                  id={`program-builder-definition-${definition.trainingDefinitionId}`}
                  type="checkbox"
                  data-testid={`program-builder-definition-${definition.trainingDefinitionId}`}
                  checked={selectedDefinitionIds.includes(definition.trainingDefinitionId)}
                  onChange={() => onToggleDefinition(definition.trainingDefinitionId)}
                  disabled={!canManage}
                />
                {definition.name}
              </label>
              <button
                type="button"
                className={`rounded border px-2 py-0.5 text-xs ${
                  selectedDefinitionIdForCitations === definition.trainingDefinitionId
                    ? 'border-violet-500 text-violet-200'
                    : 'border-slate-600 text-slate-400'
                }`}
                onClick={() => onSelectDefinitionForCitations(definition.trainingDefinitionId)}
              >
                Citations
              </button>
            </li>
          ))}
        </ul>
      )}
    </fieldset>
  )

  if (!canManage) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Training programs</h2>
        <p className="mt-3 text-sm text-slate-400">Program builder requires trainarr admin access.</p>
        {programs.length > 0 && (
          <ul className="mt-4 space-y-2">
            {programs.map((program) => (
              <li key={program.programId}>
                <button
                  type="button"
                  className={`w-full rounded-lg border p-3 text-left text-sm ${
                    selectedProgramId === program.programId
                      ? 'border-violet-500 bg-violet-950/30'
                      : 'border-slate-700 bg-slate-950/40'
                  }`}
                  onClick={() => onSelectProgram(program.programId)}
                >
                  <p className="font-medium text-slate-100">{program.name}</p>
                  <p className="mt-1 text-xs text-slate-400">
                    {program.definitionCount} definition(s) · {program.status} · v{program.publishedVersionCount}
                  </p>
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>
    )
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4" data-testid="program-builder-panel">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Program builder</h2>
      <p className="mt-1 text-xs text-slate-500">
        Create programs, edit drafts, publish versioned snapshots, and start new revisions.
      </p>

      {!isEditing ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2">
            <label htmlFor="program-builder-key" className="block text-xs text-slate-400">
              Program key
              <input
                id="program-builder-key"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                value={programKey}
                onChange={(e) => onProgramKeyChange(e.target.value)}
              />
            </label>
            <label htmlFor="program-builder-name" className="block text-xs text-slate-400">
              Program name
              <input
                id="program-builder-name"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                value={programName}
                onChange={(e) => onProgramNameChange(e.target.value)}
              />
            </label>
          </div>
          <label htmlFor="program-builder-description" className="mt-3 block text-xs text-slate-400">
            Description
            <textarea
              id="program-builder-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              rows={2}
              value={programDescription}
              onChange={(e) => onProgramDescriptionChange(e.target.value)}
            />
          </label>
          {definitionPicker}
          <button
            type="button"
            className="mt-4 rounded bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={isCreating || selectedDefinitionIds.length === 0}
            onClick={onCreateProgram}
          >
            {isCreating ? 'Creating…' : 'Create program'}
          </button>
        </>
      ) : (
        <div className="mt-4 space-y-3">
          <p className="text-sm text-slate-300">
            Editing <span className="font-medium text-white">{selectedProgramDetail?.name ?? '…'}</span> (
            {editStatus})
            {selectedProgramDetail?.programKey ? (
              <span className="ml-2 font-mono text-xs text-slate-500">{selectedProgramDetail.programKey}</span>
            ) : null}
          </p>
          <label htmlFor="program-builder-edit-name" className="block text-xs text-slate-400">
            Program name
            <input
              id="program-builder-edit-name"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={programName}
              onChange={(e) => onProgramNameChange(e.target.value)}
            />
          </label>
          <label htmlFor="program-builder-edit-description" className="block text-xs text-slate-400">
            Description
            <textarea
              id="program-builder-edit-description"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              rows={2}
              value={programDescription}
              onChange={(e) => onProgramDescriptionChange(e.target.value)}
            />
          </label>
          {definitionPicker}
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded border border-slate-600 px-3 py-1.5 text-sm text-slate-100 hover:bg-slate-800 disabled:opacity-50"
              disabled={isSaving || selectedDefinitionIds.length === 0}
              onClick={onSaveProgram}
            >
              {isSaving ? 'Saving…' : 'Save draft'}
            </button>
            <button
              type="button"
              className="rounded bg-emerald-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
              disabled={isPublishing || !canPublish}
              onClick={onPublishProgram}
            >
              {isPublishing ? 'Publishing…' : 'Publish version'}
            </button>
            {editStatus === 'published' ? (
              <button
                type="button"
                className="rounded border border-violet-600 px-3 py-1.5 text-sm text-violet-100 hover:bg-violet-950/40 disabled:opacity-50"
                disabled={isStartingRevision}
                onClick={onStartRevision}
              >
                {isStartingRevision ? 'Starting…' : 'Start new revision'}
              </button>
            ) : null}
          </div>
          {programVersions.length > 0 ? (
            <div className="border-t border-slate-700 pt-3">
              <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Published versions</h3>
              <ul className="mt-2 space-y-1 text-sm text-slate-300">
                {programVersions.map((v) => (
                  <li key={v.programVersionId}>
                    v{v.versionNumber} · {v.name} · {v.definitionCount} definition(s)
                    {v.publishedAt ? ` · ${new Date(v.publishedAt).toLocaleString()}` : ''}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
        </div>
      )}

      {programs.length > 0 && (
        <div className="mt-6 border-t border-slate-700 pt-4">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Programs</h3>
          <ul className="mt-2 space-y-2">
            {programs.map((program) => (
              <li key={program.programId}>
                <button
                  type="button"
                  className={`w-full rounded-lg border p-3 text-left text-sm ${
                    selectedProgramId === program.programId
                      ? 'border-violet-500 bg-violet-950/30'
                      : 'border-slate-700 bg-slate-950/40'
                  }`}
                  onClick={() => onSelectProgram(program.programId)}
                >
                  <p className="font-medium text-slate-100">{program.name}</p>
                  <p className="mt-1 text-xs text-slate-400">
                    {program.programKey} · {program.definitionCount} definition(s) · {program.status} ·{' '}
                    {program.publishedVersionCount} published version(s)
                  </p>
                </button>
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
