import { useMemo, useState } from 'react'
import type { TrainingAssignmentStepProgressResponse } from '../api/types'

interface AssignmentStepsPanelProps {
  steps: TrainingAssignmentStepProgressResponse[]
  isLoading: boolean
  canComplete: boolean
  canEvaluate: boolean
  isSubmitting: boolean
  onSubmitStep: (
    stepId: string,
    payload: {
      selectedOptionIndexes?: number[]
      practicalResult?: string
      notes?: string
      contentAcknowledged?: boolean
      practicalObservationNotes?: string
      safetyCriticalFailure?: boolean
      failureComments?: string
      traineeAcknowledged?: boolean
      retestRequired?: boolean
    },
  ) => Promise<void>
}

interface QuizQuestion {
  prompt: string
  options: string[]
}

interface ContentStepConfig {
  title?: string
  body?: string
  mediaUrl?: string
  externalUrl?: string
  requireAcknowledgement?: boolean
}

interface PracticalStepConfig {
  skillTaskName?: string
  passCriteria?: string
  observationPrompts?: string[]
  requiresEvaluatorSignoff?: boolean
  requireTraineeAcknowledgement?: boolean
  requireFailureComments?: boolean
  requireRetestOnFailure?: boolean
  evaluationRubric?: string
}

function parseQuizQuestions(configJson: string): QuizQuestion[] {
  try {
    const parsed = JSON.parse(configJson) as { questions?: Array<{ prompt?: string; options?: string[] }> }
    return (parsed.questions ?? []).map((question) => ({
      prompt: question.prompt ?? 'Question',
      options: question.options ?? [],
    }))
  } catch {
    return []
  }
}

function parseContentConfig(configJson: string): ContentStepConfig {
  try {
    const parsed = JSON.parse(configJson) as ContentStepConfig
    return parsed ?? {}
  } catch {
    return {}
  }
}

function parsePracticalConfig(configJson: string): PracticalStepConfig {
  try {
    const parsed = JSON.parse(configJson) as PracticalStepConfig
    return parsed ?? {}
  } catch {
    return {}
  }
}

export function AssignmentStepsPanel({
  steps,
  isLoading,
  canComplete,
  canEvaluate,
  isSubmitting,
  onSubmitStep,
}: AssignmentStepsPanelProps) {
  const [quizSelections, setQuizSelections] = useState<Record<string, number[]>>({})
  const [contentAcknowledgedByStep, setContentAcknowledgedByStep] = useState<Record<string, boolean>>({})
  const [practicalResults, setPracticalResults] = useState<Record<string, string>>({})
  const [practicalObservationNotesByStep, setPracticalObservationNotesByStep] = useState<Record<string, string>>({})
  const [practicalFailureCommentsByStep, setPracticalFailureCommentsByStep] = useState<Record<string, string>>({})
  const [practicalSafetyCriticalByStep, setPracticalSafetyCriticalByStep] = useState<Record<string, boolean>>({})
  const [practicalTraineeAcknowledgedByStep, setPracticalTraineeAcknowledgedByStep] = useState<Record<string, boolean>>({})
  const [practicalRetestRequiredByStep, setPracticalRetestRequiredByStep] = useState<Record<string, boolean>>({})
  const [notesByStep, setNotesByStep] = useState<Record<string, string>>({})

  const pendingSteps = useMemo(
    () =>
      steps.filter(
        (step) => step.isVisible && (step.status === 'pending' || step.status === 'failed'),
      ),
    [steps],
  )

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading training steps…</p>
  }

  if (steps.length === 0) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Training steps</h2>
        <p className="mt-2 text-sm text-slate-400">No structured steps are configured for this training definition.</p>
      </section>
    )
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6" data-testid="assignment-steps-panel">
      <h2 className="text-sm font-medium text-slate-300">Training steps</h2>
      <p className="mt-1 text-xs text-slate-500">Complete content, quiz, and practical steps before signoff.</p>

      <ul className="mt-4 space-y-4">
        {steps.map((step) => {
          const quizQuestions = step.stepType === 'quiz' ? parseQuizQuestions(step.configJson) : []
          const contentConfig = step.stepType === 'content' ? parseContentConfig(step.configJson) : null
          const practicalConfig = step.stepType === 'practical' ? parsePracticalConfig(step.configJson) : null
          const practicalObservationPrompts = practicalConfig?.observationPrompts ?? []
          const contentRequiresAcknowledgement = Boolean(contentConfig?.requireAcknowledgement)
          const contentAcknowledged = contentAcknowledgedByStep[step.stepId] ?? false
          const submitDisabled =
            isSubmitting ||
            (step.stepType === 'content' && contentRequiresAcknowledgement && !contentAcknowledged)
          const canSubmit =
            step.isVisible
            && step.status !== 'completed'
            && ((step.stepType === 'practical' && canEvaluate) || (step.stepType !== 'practical' && canComplete))

          return (
            <li key={step.progressId} className="rounded-lg border border-slate-800 bg-slate-950/40 p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="text-sm text-white">
                    {step.sortOrder}. {step.name}{' '}
                    <span className="text-xs uppercase tracking-wide text-sky-300">{step.stepType}</span>
                  </p>
                  <p className="mt-1 text-xs text-slate-500">{step.description}</p>
                </div>
                <span className="text-xs uppercase tracking-wide text-slate-400">{step.status}</span>
              </div>

              {step.quizScorePercent !== null ? (
                <p className="mt-2 text-xs text-slate-400">Quiz score: {step.quizScorePercent}%</p>
              ) : null}

              {!step.isVisible ? (
                <p className="mt-2 text-xs text-amber-300/90">
                  This step is not available yet. Complete prerequisite steps or meet branch conditions first.
                </p>
              ) : null}

              {canSubmit ? (
                <div className="mt-3 space-y-3">
                  {step.stepType === 'content' ? (
                    <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/30 p-3">
                      <div className="space-y-1">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Lesson</p>
                        <p className="text-sm text-slate-100">{contentConfig?.title ?? step.name}</p>
                        {contentConfig?.body ? (
                          <p className="whitespace-pre-wrap text-sm text-slate-300">{contentConfig.body}</p>
                        ) : (
                          <p className="text-sm text-slate-400">{step.description}</p>
                        )}
                        {contentConfig?.mediaUrl ? (
                          <a
                            href={contentConfig.mediaUrl}
                            target="_blank"
                            rel="noreferrer"
                            className="inline-flex text-xs text-violet-300 hover:text-violet-200"
                          >
                            Open lesson media
                          </a>
                        ) : null}
                        {contentConfig?.externalUrl ? (
                          <a
                            href={contentConfig.externalUrl}
                            target="_blank"
                            rel="noreferrer"
                            className="inline-flex text-xs text-sky-300 hover:text-sky-200"
                          >
                            Open reference link
                          </a>
                        ) : null}
                      </div>

                      <label className="flex items-center gap-2 text-sm text-slate-300">
                        <input
                          type="checkbox"
                          checked={contentAcknowledgedByStep[step.stepId] ?? false}
                          onChange={(event) =>
                            setContentAcknowledgedByStep((previous) => ({
                              ...previous,
                              [step.stepId]: event.target.checked,
                            }))
                          }
                        />
                        I have reviewed and acknowledge this lesson
                        {contentRequiresAcknowledgement ? ' (required)' : ''}
                      </label>
                    </div>
                  ) : null}

                  {step.stepType === 'quiz'
                    ? quizQuestions.map((question, index) => (
                        <fieldset key={`${step.stepId}-${index}`} className="text-sm text-slate-300">
                          <legend>{question.prompt}</legend>
                          <div className="mt-2 space-y-1">
                            {question.options.map((option, optionIndex) => {
                              const optionId = `${step.stepId}-question-${index}-option-${optionIndex}`
                              return (
                                <label key={option} htmlFor={optionId} className="flex items-center gap-2">
                                  <input
                                    id={optionId}
                                    type="radio"
                                    name={`${step.stepId}-${index}`}
                                    checked={quizSelections[step.stepId]?.[index] === optionIndex}
                                    onChange={() => {
                                      const current = [...(quizSelections[step.stepId] ?? [])]
                                      current[index] = optionIndex
                                      setQuizSelections((previous) => ({
                                        ...previous,
                                        [step.stepId]: current,
                                      }))
                                    }}
                                  />
                                  {option}
                                </label>
                              )
                            })}
                          </div>
                        </fieldset>
                      ))
                    : null}

                  {step.stepType === 'practical' ? (
                    <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/30 p-3">
                      <div className="space-y-1">
                        <p className="text-xs uppercase tracking-wide text-slate-500">Practical evaluation</p>
                        <p className="text-sm text-slate-100">{practicalConfig?.skillTaskName ?? step.name}</p>
                        <p className="text-xs text-slate-400">
                          {practicalConfig?.passCriteria ?? practicalConfig?.evaluationRubric ?? step.description}
                        </p>
                        {practicalObservationPrompts.length > 0 ? (
                          <ul className="mt-2 list-disc space-y-1 pl-5 text-xs text-slate-400">
                            {practicalObservationPrompts.map((prompt) => (
                              <li key={prompt}>{prompt}</li>
                            ))}
                          </ul>
                        ) : null}
                      </div>

                      <label htmlFor={`${step.stepId}-practical-result`} className="block text-sm text-slate-300">
                        Practical result
                        <select
                          id={`${step.stepId}-practical-result`}
                          value={practicalResults[step.stepId] ?? 'pass'}
                          onChange={(event) => {
                            const next = event.target.value
                            setPracticalResults((previous) => ({
                              ...previous,
                              [step.stepId]: next,
                            }))
                            if (next === 'pass') {
                              setPracticalRetestRequiredByStep((previous) => ({
                                ...previous,
                                [step.stepId]: false,
                              }))
                            }
                          }}
                          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                        >
                          <option value="pass">Pass</option>
                          <option value="fail">Fail</option>
                        </select>
                      </label>

                      <label htmlFor={`${step.stepId}-practical-observation-notes`} className="block text-sm text-slate-300">
                        Observation notes
                        <textarea
                          id={`${step.stepId}-practical-observation-notes`}
                          value={practicalObservationNotesByStep[step.stepId] ?? ''}
                          onChange={(event) =>
                            setPracticalObservationNotesByStep((previous) => ({
                              ...previous,
                              [step.stepId]: event.target.value,
                            }))
                          }
                          rows={3}
                          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                        />
                      </label>

                      <label htmlFor={`${step.stepId}-practical-failure-comments`} className="block text-sm text-slate-300">
                        Failure comments
                        <textarea
                          id={`${step.stepId}-practical-failure-comments`}
                          value={practicalFailureCommentsByStep[step.stepId] ?? ''}
                          onChange={(event) =>
                            setPracticalFailureCommentsByStep((previous) => ({
                              ...previous,
                              [step.stepId]: event.target.value,
                            }))
                          }
                          rows={2}
                          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                          placeholder="Required if the trainee fails the practical evaluation."
                        />
                      </label>

                      <div className="grid gap-2 text-sm text-slate-300 md:grid-cols-3">
                        <label className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            checked={practicalSafetyCriticalByStep[step.stepId] ?? false}
                            onChange={(event) =>
                              setPracticalSafetyCriticalByStep((previous) => ({
                                ...previous,
                                [step.stepId]: event.target.checked,
                              }))
                            }
                          />
                          Safety-critical failure
                        </label>
                        <label className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            checked={practicalTraineeAcknowledgedByStep[step.stepId] ?? false}
                            onChange={(event) =>
                              setPracticalTraineeAcknowledgedByStep((previous) => ({
                                ...previous,
                                [step.stepId]: event.target.checked,
                              }))
                            }
                          />
                          Trainee acknowledged
                        </label>
                        <label className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            checked={practicalRetestRequiredByStep[step.stepId] ?? false}
                            onChange={(event) =>
                              setPracticalRetestRequiredByStep((previous) => ({
                                ...previous,
                                [step.stepId]: event.target.checked,
                              }))
                            }
                          />
                          Retest required
                        </label>
                      </div>
                    </div>
                  ) : null}

                  <label htmlFor={`${step.stepId}-step-notes`} className="block text-sm text-slate-300">
                    Notes
                    <input
                      id={`${step.stepId}-step-notes`}
                      value={notesByStep[step.stepId] ?? ''}
                      onChange={(event) =>
                        setNotesByStep((previous) => ({
                          ...previous,
                          [step.stepId]: event.target.value,
                        }))
                      }
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                    />
                  </label>

                  <button
                    type="button"
                    disabled={submitDisabled}
                    className="rounded-md bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
                    onClick={() =>
                      onSubmitStep(step.stepId, {
                        selectedOptionIndexes: quizSelections[step.stepId],
                        practicalResult: practicalResults[step.stepId] ?? 'pass',
                        contentAcknowledged: step.stepType === 'content' ? contentAcknowledged : undefined,
                        practicalObservationNotes: practicalObservationNotesByStep[step.stepId] || undefined,
                        safetyCriticalFailure: practicalSafetyCriticalByStep[step.stepId] ?? undefined,
                        failureComments: practicalFailureCommentsByStep[step.stepId] || undefined,
                        traineeAcknowledged: practicalTraineeAcknowledgedByStep[step.stepId] ?? undefined,
                        retestRequired: practicalRetestRequiredByStep[step.stepId] ?? undefined,
                        notes: notesByStep[step.stepId] || undefined,
                      })
                    }
                  >
                    Submit step
                  </button>
                </div>
              ) : null}
            </li>
          )
        })}
      </ul>

      {pendingSteps.length === 0 ? (
        <p className="mt-4 text-sm text-emerald-300">All training steps are complete.</p>
      ) : null}
    </section>
  )
}
