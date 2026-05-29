import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import { submitCompanionFieldDvir, validateCompanionFieldTask } from '../api/client'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import { companionPlainReason } from '../lib/companionPlainReason'
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
      const validation = await validateCompanionFieldTask(accessToken, {
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

      return submitCompanionFieldDvir(accessToken, {
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
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'DVIR submission failed.')
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
      data-testid="companion-field-dvir-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Submit DVIR</h4>
      <p className="mt-1 text-xs text-slate-400">
        Pre- and post-trip inspections submit to RoutArr via the companion API.
      </p>

      <div className="mt-3 flex flex-wrap gap-2">
        {(['pre_trip', 'post_trip'] as const).map((option) => (
          <button
            key={option}
            type="button"
            className={`rounded-full px-3 py-1 text-xs font-medium ${
              phase === option
                ? 'bg-teal-600 text-white'
                : 'border border-slate-700 text-slate-300'
            }`}
            data-testid={`companion-dvir-phase-${option}`}
            onClick={() => setPhase(option)}
          >
            {option === 'pre_trip' ? 'Pre-trip' : 'Post-trip'}
          </button>
        ))}
      </div>

      <div className="mt-3 flex flex-wrap gap-2">
        <select
          className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          value={result}
          onChange={(event) => setResult(event.target.value as 'pass' | 'fail' | 'conditional')}
          data-testid="companion-dvir-result"
        >
          <option value="pass">Pass</option>
          <option value="conditional">Conditional</option>
          <option value="fail">Fail</option>
        </select>
        <input
          type="number"
          className="w-28 rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          placeholder="Odometer"
          value={odometer}
          onChange={(event) => setOdometer(event.target.value)}
          data-testid="companion-dvir-odometer"
        />
      </div>

      {requiresDefectNotes && (
        <textarea
          className="mt-3 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          placeholder="Defect notes (required for fail/conditional)"
          rows={2}
          value={defectNotes}
          onChange={(event) => setDefectNotes(event.target.value)}
          data-testid="companion-dvir-defect-notes"
        />
      )}

      <button
        type="button"
        className="mt-3 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        disabled={submitMutation.isPending || (requiresDefectNotes && !defectNotes.trim())}
        data-testid="companion-dvir-submit"
        onClick={() => {
          setSuccessMessage(null)
          submitMutation.mutate()
        }}
      >
        {submitMutation.isPending ? 'Submitting…' : 'Submit DVIR'}
      </button>

      {submitMutation.isError && (
        <p className="mt-2 text-sm text-rose-400" data-testid="companion-dvir-error">
          {submitMutation.error instanceof Error
            ? submitMutation.error.message
            : 'DVIR submission failed.'}
        </p>
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="companion-dvir-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
