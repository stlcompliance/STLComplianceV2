import { buildSemanticKey, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { type FormEvent, useEffect, useMemo, useState } from 'react'
import type { TrainingDefinitionResponse, TrainingDefinitionStepResponse } from '../api/types'

interface StepBuilderPanelProps {
  definitions: TrainingDefinitionResponse[]
  selectedDefinitionId: string | null
  steps: TrainingDefinitionStepResponse[]
  isLoading: boolean
  canManage: boolean
  isSubmitting: boolean
  onSelectDefinition: (definitionId: string) => void
  onCreateStep: (request: {
    stepKey: string
    name: string
    description: string
    stepType: 'content' | 'quiz' | 'practical'
    configJson: string
    sortOrder: number
  }) => Promise<void>
  onDeleteStep: (stepId: string) => Promise<void>
}

const DEFAULT_QUIZ_CONFIG = JSON.stringify(
  {
    passingScorePercent: 80,
    questions: [
      {
        questionKey: 'q1',
        prompt: 'Select the correct safety response.',
        options: ['Evacuate immediately', 'Ignore the alarm', 'Disable sensors'],
        correctOptionIndex: 0,
      },
    ],
  },
  null,
  2,
)

const DEFAULT_PRACTICAL_CONFIG = JSON.stringify(
  {
    skillTaskName: 'Demonstrate the required procedure under evaluator observation.',
    passCriteria: 'Complete the task safely, in the correct order, without critical errors.',
    observationPrompts: ['Setup', 'Execution', 'Shutdown'],
    requiresEvaluatorSignoff: true,
    requireTraineeAcknowledgement: true,
    requireFailureComments: true,
    requireRetestOnFailure: true,
  },
  null,
  2,
)

const DEFAULT_CONTENT_CONFIG = JSON.stringify(
  {
    title: 'Lesson overview',
    body: 'Review the assigned training material.',
    mediaUrl: '',
    externalUrl: '',
    requireAcknowledgement: true,
  },
  null,
  2,
)

function defaultConfigForType(stepType: 'content' | 'quiz' | 'practical'): string {
  if (stepType === 'quiz') {
    return DEFAULT_QUIZ_CONFIG
  }
  if (stepType === 'practical') {
    return DEFAULT_PRACTICAL_CONFIG
  }
  return DEFAULT_CONTENT_CONFIG
}

export function StepBuilderPanel({
  definitions,
  selectedDefinitionId,
  steps,
  isLoading,
  canManage,
  isSubmitting,
  onSelectDefinition,
  onCreateStep,
  onDeleteStep,
}: StepBuilderPanelProps) {
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [stepType, setStepType] = useState<'content' | 'quiz' | 'practical'>('content')
  const [configJson, setConfigJson] = useState(DEFAULT_CONTENT_CONFIG)
  const [sortOrder, setSortOrder] = useState('0')
  const definitionOptions = useMemo<PickerOption[]>(
    () => definitions.map((definition) => ({ value: definition.trainingDefinitionId, label: definition.name })),
    [definitions],
  )
  const selectedDefinitionOption = useMemo<PickerOption | undefined>(
    () =>
      definitionOptions.find((option) => option.value === selectedDefinitionId) ??
      (selectedDefinitionId ? { value: selectedDefinitionId, label: selectedDefinitionId } : undefined),
    [definitionOptions, selectedDefinitionId],
  )

  const generatedStepKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'train',
        kind: 'step',
        title: name,
        existingKeys: steps.map((step) => step.stepKey),
        maxLength: 64,
      }),
    [name, steps],
  )

  useEffect(() => {
    setSortOrder(String(steps.length))
  }, [steps.length, selectedDefinitionId])

  const handleStepTypeChange = (value: 'content' | 'quiz' | 'practical') => {
    setStepType(value)
    setConfigJson(defaultConfigForType(value))
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!selectedDefinitionId) {
      return
    }

    await onCreateStep({
      stepKey: generatedStepKey.trim(),
      name: name.trim(),
      description: description.trim(),
      stepType,
      configJson,
      sortOrder: Number.parseInt(sortOrder, 10) || 0,
    })

    setName('')
    setDescription('')
    setStepType('content')
    setConfigJson(DEFAULT_CONTENT_CONFIG)
    setSortOrder(String(steps.length + 1))
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="step-builder-panel">
      <header>
        <h2 className="text-sm font-medium text-slate-300">Step builder</h2>
        <p className="mt-1 text-xs text-slate-500">
          docs/14 quiz/test/practical steps — attach content, quiz, and practical steps to a training definition.
        </p>
      </header>

      <div className="mt-4 block text-sm text-slate-300">
        <StaticSearchPicker
          id="step-builder-definition"
          label="Training definition"
          value={selectedDefinitionId ?? ''}
          onChange={onSelectDefinition}
          options={definitionOptions}
          selectedOption={selectedDefinitionOption}
          placeholder="Search training definitions…"
          testId="step-builder-definition"
        />
      </div>

      {selectedDefinitionId ? (
        <>
          {isLoading ? (
            <p className="mt-4 text-sm text-slate-400">Loading steps…</p>
          ) : steps.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No steps defined yet.</p>
          ) : (
            <ul className="mt-4 space-y-2">
              {steps.map((step) => (
                <li
                  key={step.stepId}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-2 text-sm"
                >
                  <div>
                    <p className="text-slate-100">
                      {step.sortOrder}. {step.name}{' '}
                      <span className="text-xs uppercase tracking-wide text-sky-300">{step.stepType}</span>
                    </p>
                    <p className="text-xs text-slate-500">{step.description}</p>
                  </div>
                  {canManage ? (
                    <button
                      type="button"
                      className="text-xs text-rose-300 hover:text-rose-200"
                      onClick={() => onDeleteStep(step.stepId)}
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
              <div className="md:col-span-2 text-xs text-slate-400">Step reference is generated automatically from the step name.</div>
              <label htmlFor="step-builder-sort-order" className="block text-sm text-slate-300">
                Sort order
                <input
                  id="step-builder-sort-order"
                  type="number"
                  value={sortOrder}
                  onChange={(event) => setSortOrder(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                />
              </label>
              <label htmlFor="step-builder-name" className="block text-sm text-slate-300 md:col-span-2">
                Name
                <input
                  id="step-builder-name"
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  required
                  minLength={2}
                />
              </label>
              <label htmlFor="step-builder-description" className="block text-sm text-slate-300 md:col-span-2">
                Description
                <textarea
                  id="step-builder-description"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  required
                  minLength={2}
                  rows={2}
                />
              </label>
              <label htmlFor="step-builder-type" className="block text-sm text-slate-300">
                Step type
                <select
                  id="step-builder-type"
                  value={stepType}
                  onChange={(event) =>
                    handleStepTypeChange(event.target.value as 'content' | 'quiz' | 'practical')
                  }
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                >
                  <option value="content">Content</option>
                  <option value="quiz">Quiz / test</option>
                  <option value="practical">Practical evaluation</option>
                </select>
              </label>
              <label htmlFor="step-builder-config-json" className="block text-sm text-slate-300 md:col-span-2">
                Config JSON
                <textarea
                  id="step-builder-config-json"
                  value={configJson}
                  onChange={(event) => setConfigJson(event.target.value)}
                  className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
                  required
                  rows={8}
                />
              </label>
              <div className="md:col-span-2">
                <button
                  type="submit"
                  disabled={isSubmitting || !generatedStepKey}
                  className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                >
                  {isSubmitting ? 'Adding…' : 'Add step'}
                </button>
              </div>
            </form>
          ) : null}
        </>
      ) : (
        <p className="mt-4 text-sm text-slate-400">Select a training definition to manage steps.</p>
      )}
    </section>
  )
}
