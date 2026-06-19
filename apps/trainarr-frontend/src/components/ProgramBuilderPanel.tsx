import { useMutation } from '@tanstack/react-query'
import { buildSemanticKey } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import { generateTrainingProgramDraft } from '../api/client'
import type {
  TrainingDefinitionResponse,
  TrainingProgramDraftResponse,
  TrainingProgramDetailResponse,
  TrainingProgramSummaryResponse,
  TrainingProgramVersionSummaryResponse,
} from '../api/types'

interface ProgramBuilderPanelProps {
  accessToken: string
  mode: 'drawer' | 'details' | 'create'
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
  accessToken,
  mode,
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
  type ProgramColumnKey = 'name' | 'definitions' | 'status' | 'publishedVersions'
  const storageKey = 'trainarr.programs.drawer.columns.v1'
  const allColumns: Array<{ key: ProgramColumnKey; label: string }> = [
    { key: 'name', label: 'Program name' },
    { key: 'definitions', label: 'Definitions' },
    { key: 'status', label: 'Status' },
    { key: 'publishedVersions', label: 'Published versions' },
  ]
  const [showProgramKeyPolicy, setShowProgramKeyPolicy] = useState(false)
  const [selectedColumns, setSelectedColumns] = useState<ProgramColumnKey[]>(['name', 'definitions', 'status', 'publishedVersions'])
  const [draftPrompt, setDraftPrompt] = useState('')
  const [draftSuggestion, setDraftSuggestion] = useState<TrainingProgramDraftResponse | null>(null)
  void programKey
  const isEditing = Boolean(selectedProgramId)
  const editStatus = selectedProgramDetail?.status ?? 'draft'
  const existingProgramKeys = programs.map((program) => program.programKey)
  const programKeySource = programName.trim()
  const generatedProgramKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'program',
        title: programKeySource,
        existingKeys: existingProgramKeys,
        maxLength: 128,
      }),
    [existingProgramKeys, programKeySource],
  )

  useEffect(() => {
    if (!isEditing) {
      onProgramKeyChange(generatedProgramKey)
    }
  }, [generatedProgramKey, isEditing, onProgramKeyChange])
  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(storageKey)
      if (!raw) return
      const parsed = JSON.parse(raw) as ProgramColumnKey[]
      const valid = parsed.filter((column) => allColumns.some((candidate) => candidate.key === column)).slice(0, 5)
      if (valid.length > 0) setSelectedColumns(valid)
    } catch {}
  }, [])
  useEffect(() => {
    window.localStorage.setItem(storageKey, JSON.stringify(selectedColumns))
  }, [selectedColumns])

  const generateDraftMutation = useMutation({
    mutationFn: async () => {
      const prompt = draftPrompt.trim()
      if (!prompt) {
        throw new Error('Enter a training scenario before generating a draft.')
      }
      return generateTrainingProgramDraft(accessToken, { prompt })
    },
    onSuccess: (result) => {
      setDraftSuggestion(result)
    },
  })

  const applyDraftSuggestion = () => {
    if (!draftSuggestion) {
      return
    }

    onProgramNameChange(draftSuggestion.name)
    onProgramDescriptionChange(draftSuggestion.description)

    const targetIds = new Set(draftSuggestion.trainingDefinitionIds)
    const selectedIds = new Set(selectedDefinitionIds)
    for (const definitionId of selectedDefinitionIds) {
      if (!targetIds.has(definitionId)) {
        onToggleDefinition(definitionId)
      }
    }
    for (const definitionId of draftSuggestion.trainingDefinitionIds) {
      if (!selectedIds.has(definitionId)) {
        onToggleDefinition(definitionId)
      }
    }

    if (draftSuggestion.trainingDefinitionIds.length > 0) {
      onSelectDefinitionForCitations(draftSuggestion.trainingDefinitionIds[0])
    }
  }

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
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Create training definitions before building a program.</p>
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
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Course catalog & builder</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Create courses and learning paths, edit drafts, publish versioned snapshots, and start new revisions.
      </p>

      {mode === 'create' ? (
        <>
          <section className="mt-4 rounded-xl border border-violet-700/40 bg-violet-950/20 p-4">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h3 className="text-sm font-semibold uppercase tracking-wide text-violet-100">AI-assisted draft</h3>
                <p className="mt-1 text-sm text-violet-50/80">
                  Describe the audience, role, or compliance need. The assistant suggests a draft name, description,
                  and active definitions to seed the course.
                </p>
              </div>
              <button
                type="button"
                className="rounded bg-violet-500 px-3 py-1.5 text-sm font-medium text-[var(--color-text-primary)] hover:bg-violet-400 disabled:opacity-50"
                disabled={generateDraftMutation.isPending || !draftPrompt.trim()}
                onClick={() => generateDraftMutation.mutate()}
              >
                {generateDraftMutation.isPending ? 'Generating…' : 'Generate draft'}
              </button>
            </div>
            <label htmlFor="program-builder-ai-prompt" className="mt-4 block text-xs text-violet-100/80">
              Draft prompt
              <textarea
                id="program-builder-ai-prompt"
                className="mt-1 w-full rounded border border-violet-700/40 bg-slate-950 px-2 py-2 text-sm text-slate-100"
                rows={3}
                value={draftPrompt}
                onChange={(e) => setDraftPrompt(e.target.value)}
                placeholder="Example: Create an onboarding program for hazmat drivers with forklift and annual compliance refreshers."
              />
            </label>
            {draftSuggestion ? (
              <div className="mt-4 rounded-lg border border-violet-700/40 bg-slate-950/60 p-4 text-sm text-slate-200">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-xs uppercase tracking-wide text-violet-200/70">Suggested draft</p>
                    <h4 className="mt-1 text-lg font-semibold text-white">{draftSuggestion.name}</h4>
                    <p className="mt-1 text-sm text-slate-300">{draftSuggestion.summary}</p>
                  </div>
                  <button
                    type="button"
                    className="rounded border border-violet-500 px-3 py-1.5 text-xs font-medium text-violet-100 hover:bg-violet-950/50"
                    onClick={applyDraftSuggestion}
                  >
                    Apply draft
                  </button>
                </div>
                <p className="mt-3 whitespace-pre-line text-sm text-slate-300">{draftSuggestion.description}</p>
                <div className="mt-4 grid gap-4 md:grid-cols-2">
                  <div className="rounded border border-slate-700 bg-slate-900/70 p-3">
                    <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Prompt</p>
                    <p className="mt-1 text-sm text-slate-200">{draftSuggestion.prompt}</p>
                  </div>
                  <div className="rounded border border-slate-700 bg-slate-900/70 p-3">
                    <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Selected definitions</p>
                    <ul className="mt-2 space-y-1 text-sm text-slate-200">
                      {draftSuggestion.matchedDefinitions.map((definition) => (
                        <li key={definition.trainingDefinitionId} className="rounded border border-slate-700 bg-slate-950/50 px-3 py-2">
                          <p className="font-medium text-white">{definition.name}</p>
                          <p className="mt-1 text-xs text-slate-400">{definition.matchReason}</p>
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              </div>
            ) : null}
          </section>
          <div className="mt-4 grid gap-3 sm:grid-cols-2">
            <div className="space-y-1">
              <p className="text-xs text-slate-400">Reference is auto-generated from course name.</p>
              {!showProgramKeyPolicy ? (
                <button
                  type="button"
                  className="text-xs text-[var(--color-text-muted)] underline-offset-2 hover:text-slate-300 hover:underline"
                  onClick={() => setShowProgramKeyPolicy(true)}
                  disabled={isCreating}
                >
                  Key policy
                </button>
              ) : null}
            </div>
            <label htmlFor="program-builder-name" className="block text-xs text-slate-400">
              Course name
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
            disabled={isCreating || selectedDefinitionIds.length === 0 || !programName.trim()}
            onClick={onCreateProgram}
          >
            {isCreating ? 'Creating…' : 'Create program'}
          </button>
        </>
      ) : mode === 'details' ? (
        <div className="mt-4 space-y-3">
          <p className="text-sm text-slate-300">
            Editing <span className="font-medium text-white">{selectedProgramDetail?.name ?? '…'}</span> (
            {editStatus})
          </p>
          <label htmlFor="program-builder-edit-name" className="block text-xs text-slate-400">
            Course name
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
              <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Published versions</h3>
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
      ) : null}

      {programs.length > 0 && (
        <div className="mt-6 border-t border-slate-700 pt-4">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Programs</h3>
          <div className="mt-2 rounded-md border border-slate-700 p-2">
            <p className="text-xs text-slate-400">Visible columns (max 5)</p>
            <div className="mt-2 flex flex-wrap gap-3">
              {allColumns.map((column) => (
                <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                  <input
                    type="checkbox"
                    checked={selectedColumns.includes(column.key)}
                    onChange={() => {
                      if (selectedColumns.includes(column.key)) {
                        const next = selectedColumns.filter((item) => item !== column.key)
                        if (next.length > 0) setSelectedColumns(next)
                      } else if (selectedColumns.length < 5) {
                        setSelectedColumns([...selectedColumns, column.key])
                      }
                    }}
                  />
                  {column.label}
                </label>
              ))}
            </div>
          </div>
          <div className="mt-3 overflow-x-auto rounded-md border border-slate-700">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-950/70">
                <tr>
                  {selectedColumns.map((column) => (
                    <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                      {allColumns.find((item) => item.key === column)?.label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
            {programs.map((program) => (
                <tr
                  key={program.programId}
                  className={`border-t border-slate-800 cursor-pointer ${selectedProgramId === program.programId ? 'bg-violet-950/30' : ''}`}
                  onClick={() => onSelectProgram(program.programId)}
                >
                  {selectedColumns.map((column) => (
                    <td key={`${program.programId}-${column}`} className="px-3 py-2 text-slate-200">
                      {column === 'name' ? program.name : null}
                      {column === 'definitions' ? String(program.definitionCount) : null}
                      {column === 'status' ? program.status : null}
                      {column === 'publishedVersions' ? String(program.publishedVersionCount) : null}
                    </td>
                  ))}
                </tr>
            ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  )
}
