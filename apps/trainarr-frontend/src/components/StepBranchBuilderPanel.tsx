import { buildSemanticKey } from '@stl/shared-ui'
import { type FormEvent, useEffect, useMemo, useState } from 'react'
import type {
  TrainingDefinitionResponse,
  TrainingDefinitionStepBranchResponse,
  TrainingDefinitionStepResponse,
  TrainingStepBranchCatalogItemResponse,
} from '../api/types'

interface StepBranchBuilderPanelProps {
  definitions: TrainingDefinitionResponse[]
  selectedDefinitionId: string | null
  steps: TrainingDefinitionStepResponse[]
  selectedStepId: string | null
  catalog: TrainingStepBranchCatalogItemResponse[]
  branches: TrainingDefinitionStepBranchResponse[]
  isLoading: boolean
  canManage: boolean
  isSubmitting: boolean
  onSelectDefinition: (definitionId: string) => void
  onSelectStep: (stepId: string) => void
  onCreateBranch: (request: {
    branchKey: string
    branchType: string
    label: string
    configJson: string
    sortOrder: number
  }) => Promise<void>
  onDeleteBranch: (branchId: string) => Promise<void>
}

function defaultConfigForType(
  catalog: TrainingStepBranchCatalogItemResponse[],
  branchType: string,
): string {
  return catalog.find((item) => item.branchType === branchType)?.defaultConfigJson ?? '{}'
}

export function StepBranchBuilderPanel({
  definitions,
  selectedDefinitionId,
  steps,
  selectedStepId,
  catalog,
  branches,
  isLoading,
  canManage,
  isSubmitting,
  onSelectDefinition,
  onSelectStep,
  onCreateBranch,
  onDeleteBranch,
}: StepBranchBuilderPanelProps) {
  const [label, setLabel] = useState('')
  const [branchType, setBranchType] = useState('quiz_failed_remediation')
  const [configJson, setConfigJson] = useState('{}')
  const [sortOrder, setSortOrder] = useState('0')

  const selectedStep = steps.find((step) => step.stepId === selectedStepId) ?? null
  const generatedBranchKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'branch',
        title: label,
        existingKeys: branches.map((branch) => branch.branchKey),
        maxLength: 64,
      }),
    [branches, label],
  )

  useEffect(() => {
    setSortOrder(String(branches.length))
  }, [branches.length, selectedStepId])

  useEffect(() => {
    if (catalog.length > 0) {
      const initialType = catalog[0]?.branchType ?? 'quiz_failed_remediation'
      setBranchType(initialType)
      setConfigJson(defaultConfigForType(catalog, initialType))
    }
  }, [catalog])

  const handleBranchTypeChange = (value: string) => {
    setBranchType(value)
    setConfigJson(defaultConfigForType(catalog, value))
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!selectedDefinitionId || !selectedStepId) {
      return
    }

    await onCreateBranch({
      branchKey: generatedBranchKey.trim(),
      branchType,
      label: label.trim(),
      configJson,
      sortOrder: Number.parseInt(sortOrder, 10) || 0,
    })

    setLabel('')
    setSortOrder(String(branches.length + 1))
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="step-branch-builder-panel"
    >
      <header>
        <h2 className="text-sm font-medium text-slate-300">Conditional branching</h2>
        <p className="mt-1 text-xs text-slate-500">
          docs/2.5 — quiz-failed remediation triggers and step visibility rules per training step.
        </p>
      </header>

      <label htmlFor="branch-builder-definition" className="mt-4 block text-sm text-slate-300">
        Training definition
        <select
          id="branch-builder-definition"
          value={selectedDefinitionId ?? ''}
          onChange={(event) => onSelectDefinition(event.target.value)}
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
        >
          <option value="">Select definition…</option>
          {definitions.map((definition) => (
            <option key={definition.trainingDefinitionId} value={definition.trainingDefinitionId}>
              {definition.name}
            </option>
          ))}
        </select>
      </label>

      {selectedDefinitionId ? (
        <>
          <label htmlFor="branch-builder-step" className="mt-4 block text-sm text-slate-300">
            Training step
            <select
              id="branch-builder-step"
              value={selectedStepId ?? ''}
              onChange={(event) => onSelectStep(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">Select step…</option>
              {steps.map((step) => (
                <option key={step.stepId} value={step.stepId}>
                  {step.sortOrder}. {step.name} ({step.stepType})
                </option>
              ))}
            </select>
          </label>

          {selectedStepId ? (
            <>
              {selectedStep ? (
                <p className="mt-2 text-xs text-slate-500">
                  Branch rules attach to <span className="text-slate-300">{selectedStep.name}</span> (
                  {selectedStep.stepType}).
                </p>
              ) : null}

              {isLoading ? (
                <p className="mt-4 text-sm text-slate-400">Loading branches…</p>
              ) : branches.length === 0 ? (
                <p className="mt-4 text-sm text-slate-400">No branch rules on this step yet.</p>
              ) : (
                <ul className="mt-4 space-y-2">
                  {branches.map((branch) => (
                    <li
                      key={branch.branchId}
                      className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-2 text-sm"
                    >
                      <div>
                        <p className="text-slate-100">
                          {branch.label}{' '}
                          <span className="text-xs uppercase tracking-wide text-amber-300">
                            {branch.branchType}
                          </span>
                        </p>
                        <p className="font-mono text-xs text-slate-500">{branch.configJson}</p>
                      </div>
                      {canManage ? (
                        <button
                          type="button"
                          className="text-xs text-rose-300 hover:text-rose-200"
                          onClick={() => onDeleteBranch(branch.branchId)}
                        >
                          Delete
                        </button>
                      ) : null}
                    </li>
                  ))}
                </ul>
              )}

              {canManage ? (
                <form className="mt-6 grid gap-4 md:grid-cols-2" onSubmit={handleSubmit}>
                  <div className="md:col-span-2 text-xs text-slate-400">Branch reference is generated automatically from label.</div>
                  <label htmlFor="branch-builder-sort-order" className="block text-sm text-slate-300">
                    Sort order
                    <input
                      id="branch-builder-sort-order"
                      type="number"
                      value={sortOrder}
                      onChange={(event) => setSortOrder(event.target.value)}
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    />
                  </label>
                  <label htmlFor="branch-builder-type" className="block text-sm text-slate-300">
                    Branch type
                    <select
                      id="branch-builder-type"
                      value={branchType}
                      onChange={(event) => handleBranchTypeChange(event.target.value)}
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    >
                      {catalog.map((item) => (
                        <option key={item.branchType} value={item.branchType}>
                          {item.label}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label htmlFor="branch-builder-label" className="block text-sm text-slate-300 md:col-span-2">
                    Label
                    <input
                      id="branch-builder-label"
                      value={label}
                      onChange={(event) => setLabel(event.target.value)}
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                      required
                      minLength={2}
                    />
                  </label>
                  <label htmlFor="branch-builder-config-json" className="block text-sm text-slate-300 md:col-span-2">
                    Config JSON
                    <textarea
                      id="branch-builder-config-json"
                      value={configJson}
                      onChange={(event) => setConfigJson(event.target.value)}
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
                      required
                      rows={6}
                    />
                  </label>
                  <div className="md:col-span-2">
                    <button
                      type="submit"
                      disabled={isSubmitting || !generatedBranchKey}
                      className="rounded-md bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-500 disabled:opacity-50"
                    >
                      {isSubmitting ? 'Adding…' : 'Add branch rule'}
                    </button>
                  </div>
                </form>
              ) : null}
            </>
          ) : (
            <p className="mt-4 text-sm text-slate-400">Select a step to configure branching.</p>
          )}
        </>
      ) : (
        <p className="mt-4 text-sm text-slate-400">Select a training definition to manage branches.</p>
      )}
    </section>
  )
}
