import type { TrainingDefinitionResponse, TrainingProgramSummaryResponse } from '../api/types'

interface ProgramBuilderPanelProps {
  programs: TrainingProgramSummaryResponse[]
  definitions: TrainingDefinitionResponse[]
  selectedDefinitionIds: string[]
  selectedProgramId: string | null
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
  isCreating: boolean
  canManage: boolean
}

export function ProgramBuilderPanel({
  programs,
  definitions,
  selectedDefinitionIds,
  selectedProgramId,
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
  isCreating,
  canManage,
}: ProgramBuilderPanelProps) {
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
                    {program.definitionCount} definition(s) · {program.status}
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
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Program builder</h2>
      <p className="mt-1 text-xs text-slate-500">Link training definitions into a reusable program.</p>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label className="block text-xs text-slate-400">
          Program key
          <input
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
            value={programKey}
            onChange={(e) => onProgramKeyChange(e.target.value)}
            placeholder="annual_onboarding"
          />
        </label>
        <label className="block text-xs text-slate-400">
          Program name
          <input
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
            value={programName}
            onChange={(e) => onProgramNameChange(e.target.value)}
            placeholder="Annual onboarding"
          />
        </label>
      </div>

      <label className="mt-3 block text-xs text-slate-400">
        Description
        <textarea
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          rows={2}
          value={programDescription}
          onChange={(e) => onProgramDescriptionChange(e.target.value)}
          placeholder="Required training bundle for new operational staff."
        />
      </label>

      <fieldset className="mt-3">
        <legend className="text-xs text-slate-400">Training definitions</legend>
        {definitions.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">Create training definitions before building a program.</p>
        ) : (
          <ul className="mt-2 space-y-1">
            {definitions.map((definition) => (
              <li key={definition.trainingDefinitionId} className="flex flex-wrap items-center gap-2">
                <label className="flex items-center gap-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={selectedDefinitionIds.includes(definition.trainingDefinitionId)}
                    onChange={() => onToggleDefinition(definition.trainingDefinitionId)}
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

      <button
        type="button"
        className="mt-4 rounded bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
        disabled={isCreating || selectedDefinitionIds.length === 0}
        onClick={onCreateProgram}
      >
        {isCreating ? 'Creating…' : 'Create program'}
      </button>

      {programs.length > 0 && (
        <div className="mt-6 border-t border-slate-700 pt-4">
          <h3 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Existing programs</h3>
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
                    {program.programKey} · {program.definitionCount} definition(s) · {program.status}
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
