import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useState } from 'react'

import { submitFieldCompanionFieldDvir, validateFieldCompanionFieldTask } from '../api/client'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import type { FieldInboxTaskItem } from '../api/types'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

interface FieldTaskDvirPanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  onSubmitComplete?: () => void
}

type DvirPhase = 'pre_trip' | 'post_trip'

export function FieldTaskDvirPanel({ accessToken, task, onSubmitComplete }: FieldTaskDvirPanelProps) {
  const queryClient = useQueryClient()
  const [phase, setPhase] = useState<DvirPhase>('pre_trip')
  const [result, setResult] = useState<'pass' | 'fail' | 'conditional'>('pass')
  const [odometer, setOdometer] = useState('')
  const [defectNotes, setDefectNotes] = useState('')
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const submitMutation = useMutation({
    mutationFn: async () => {
      const validation = await validateFieldCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'dvir',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          resolveDeniedReason(validation, 'DVIR cannot be submitted for this trip right now.'),
        )
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'dvir',
        phase: 'syncing',
      })

      return submitFieldCompanionFieldDvir(accessToken, {
        taskKey: task.taskKey,
        phase,
        result,
        odometerReading: odometer ? Number(odometer) : null,
        defectNotes: defectNotes.trim() || null,
        vehicleRefKey: null,
      })
    },
    onSuccess: (response) => {
      const message = `Submitted ${response.phase.replace('_', '-')} DVIR (${response.result}) to RoutArr.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'dvir',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-field-inbox', accessToken] })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = FieldCompanionPlainReason(error, 'DVIR submission failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'dvir',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const requiresDefectNotes = result === 'fail' || result === 'conditional'

  return (
    <div
      className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="fieldcompanion-field-dvir-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Submit DVIR</h4>
      <p className="mt-1 text-xs text-slate-400">
        Pre- and post-trip inspections submit to RoutArr via the fieldcompanion API.
      </p>

      <div className="mt-3 flex flex-wrap gap-2">
        {(['pre_trip', 'post_trip'] as const).map((option) => (
          <button
            key={option}
            type="button"
            className={`min-h-11 rounded-full px-3 py-1.5 text-xs font-medium ${
              phase === option
                ? 'bg-teal-600 text-white'
                : 'border border-slate-700 text-slate-300'
            }`}
            data-testid={`fieldcompanion-dvir-phase-${option}`}
            onClick={() => setPhase(option)}
          >
            {option === 'pre_trip' ? 'Pre-trip' : 'Post-trip'}
          </button>
        ))}
      </div>

      <div className="mt-3 flex flex-wrap gap-2">
        <label className="sr-only" htmlFor="fieldcompanion-dvir-result">
          Field DVIR result
        </label>
        <select
          id="fieldcompanion-dvir-result"
          className="min-h-12 rounded-md border border-slate-700 bg-slate-900 px-3 py-3 text-sm text-slate-100"
          value={result}
          onChange={(event) => setResult(event.target.value as 'pass' | 'fail' | 'conditional')}
          data-testid="fieldcompanion-dvir-result"
        >
          <option value="pass">Pass</option>
          <option value="conditional">Conditional</option>
          <option value="fail">Fail</option>
        </select>
        <label className="sr-only" htmlFor="fieldcompanion-dvir-odometer">
          Field DVIR odometer
        </label>
        <input
          id="fieldcompanion-dvir-odometer"
          type="number"
          className="min-h-12 w-28 rounded-md border border-slate-700 bg-slate-900 px-3 py-3 text-sm text-slate-100"
          placeholder="Odometer"
          value={odometer}
          onChange={(event) => setOdometer(event.target.value)}
          data-testid="fieldcompanion-dvir-odometer"
        />
      </div>

      {requiresDefectNotes && (
        <label className="mt-3 block text-sm text-slate-200" htmlFor="fieldcompanion-dvir-defect-notes">
          Defect notes (required for fail/conditional)
          <textarea
            id="fieldcompanion-dvir-defect-notes"
            className="mt-1 min-h-12 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-3 text-sm text-slate-100"
            placeholder="Defect notes (required for fail/conditional)"
            rows={2}
            value={defectNotes}
            onChange={(event) => setDefectNotes(event.target.value)}
            data-testid="fieldcompanion-dvir-defect-notes"
          />
        </label>
      )}

      <button
        type="button"
        className="mt-3 min-h-12 rounded-md bg-teal-600 px-4 py-3 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        disabled={submitMutation.isPending || (requiresDefectNotes && !defectNotes.trim())}
        data-testid="fieldcompanion-dvir-submit"
        onClick={() => {
          setSuccessMessage(null)
          submitMutation.mutate()
        }}
      >
        {submitMutation.isPending ? 'Submitting…' : 'Submit DVIR'}
      </button>

      {submitMutation.isError && (
        <div aria-live="polite" className="mt-2">
          <ApiErrorCallout
            testId="fieldcompanion-dvir-error"
            title="DVIR submission failed"
            message={getErrorMessage(submitMutation.error, 'DVIR submission failed.')}
          />
        </div>
      )}

      {successMessage && (
        <p
          className="mt-2 text-sm text-emerald-300"
          data-testid="fieldcompanion-dvir-success"
          aria-live="polite"
        >
          {successMessage}
        </p>
      )}
    </div>
  )
}
