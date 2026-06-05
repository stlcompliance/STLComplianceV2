import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getFieldCompanionFieldWorkOrderDetail,
  logFieldCompanionFieldWorkOrderLabor,
  updateFieldCompanionFieldWorkOrderStatus,
  validateFieldCompanionFieldTask,
} from '../api/client'
import type { FieldInboxTaskItem } from '../api/types'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import {
  parseLaborHoursInput,
  workOrderEditable,
  workOrderStatusActionLabel,
  WORK_ORDER_LABOR_TYPES,
  nextWorkOrderStatusAction,
} from '../lib/fieldWorkOrder'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

interface FieldTaskWorkOrderPanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  onSubmitComplete?: () => void
}

export function FieldTaskWorkOrderPanel({
  accessToken,
  task,
  onSubmitComplete,
}: FieldTaskWorkOrderPanelProps) {
  const queryClient = useQueryClient()
  const [laborHours, setLaborHours] = useState('1')
  const [laborTypeKey, setLaborTypeKey] = useState<(typeof WORK_ORDER_LABOR_TYPES)[number]>('regular')
  const [laborNotes, setLaborNotes] = useState('')
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['fieldcompanion-field-work-order', accessToken, task.taskKey],
    queryFn: async () => {
      const validation = await validateFieldCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'work-order',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          resolveDeniedReason(
            validation,
            'Work order updates are not allowed for this task right now.',
          ),
        )
      }

      return getFieldCompanionFieldWorkOrderDetail(accessToken, task.taskKey)
    },
    enabled: Boolean(accessToken),
  })

  const statusMutation = useMutation({
    mutationFn: async (status: string) => {
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'syncing',
      })

      return updateFieldCompanionFieldWorkOrderStatus(accessToken, {
        taskKey: task.taskKey,
        status,
      })
    },
    onSuccess: (response) => {
      const message = `Work order status updated to ${response.status.replaceAll('_', ' ')}.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['fieldcompanion-field-work-order', accessToken, task.taskKey],
      })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = FieldCompanionPlainReason(error, 'Work order status update failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const laborMutation = useMutation({
    mutationFn: async () => {
      const hours = parseLaborHoursInput(laborHours)
      if (hours === null) {
        throw new Error('Enter a valid number of hours greater than zero.')
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'syncing',
      })

      return logFieldCompanionFieldWorkOrderLabor(accessToken, {
        taskKey: task.taskKey,
        hoursWorked: hours,
        laborTypeKey,
        notes: laborNotes.trim() || null,
        workOrderTaskLineId: null,
      })
    },
    onSuccess: (response) => {
      const message = `Logged ${response.hoursWorked} hour(s) of ${response.laborTypeKey} labor.`
      setSuccessMessage(message)
      setLaborNotes('')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['fieldcompanion-field-work-order', accessToken, task.taskKey],
      })
    },
    onError: (error) => {
      const message = FieldCompanionPlainReason(error, 'Work order labor logging failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'work-order',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const detail = detailQuery.data
  const statusAction = detail ? nextWorkOrderStatusAction(detail.status) : null
  const statusActionLabel = detail ? workOrderStatusActionLabel(detail.status) : null
  const editable = detail ? workOrderEditable(detail.status) : false
  const laborHoursValid = parseLaborHoursInput(laborHours) !== null

  if (detailQuery.isLoading) {
    return (
      <div
        className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-400"
        data-testid="fieldcompanion-field-work-order-panel"
      >
        Loading work order detail…
      </div>
    )
  }

  if (detailQuery.isError) {
    return (
      <div
        className="mt-4"
        data-testid="fieldcompanion-field-work-order-panel"
      >
        <ApiErrorCallout
          message={getErrorMessage(detailQuery.error, 'Failed to load work order detail.')}
          onRetry={() => void detailQuery.refetch()}
          retryLabel="Retry work order detail"
        />
      </div>
    )
  }

  if (!detail) {
    return null
  }

  return (
    <div
      className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="fieldcompanion-field-work-order-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Update work order</h4>
      <p className="mt-1 text-xs text-slate-400">
        {detail.workOrderNumber} · {detail.assetTag} · {detail.assetName}
      </p>
      {detail.description && (
        <p className="mt-2 text-sm text-slate-300">{detail.description}</p>
      )}

      {detail.tasks.length > 0 && (
        <div className="mt-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Tasks</p>
          <ul className="mt-2 space-y-2">
            {detail.tasks.map((taskLine) => (
              <li
                key={taskLine.taskLineId}
                className="rounded-md border border-slate-800 bg-slate-900/70 px-3 py-2 text-sm text-slate-200"
                data-testid={`fieldcompanion-work-order-task-${taskLine.sortOrder}`}
              >
                {taskLine.title}
                <span className="ml-2 text-xs uppercase text-slate-400">
                  {taskLine.status.replaceAll('_', ' ')}
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {detail.laborEntries.length > 0 && (
        <div className="mt-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">Labor logged</p>
          <ul className="mt-2 space-y-2">
            {detail.laborEntries.map((entry) => (
              <li
                key={entry.laborEntryId}
                className="rounded-md border border-slate-800 bg-slate-900/70 px-3 py-2 text-sm text-slate-200"
              >
                {entry.hoursWorked}h {entry.laborTypeKey}
                {entry.notes ? ` · ${entry.notes}` : ''}
              </li>
            ))}
          </ul>
        </div>
      )}

      {editable && (
        <div className="mt-4 space-y-3">
          <label className="block text-sm text-slate-200" htmlFor="fieldcompanion-work-order-labor-hours">
            Hours worked
            <input
              id="fieldcompanion-work-order-labor-hours"
              type="number"
              min="0.25"
              step="0.25"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              value={laborHours}
              onChange={(event) => setLaborHours(event.target.value)}
              data-testid="fieldcompanion-work-order-labor-hours"
            />
          </label>

          <label className="block text-sm text-slate-200" htmlFor="fieldcompanion-work-order-labor-type">
            Labor type
            <select
              id="fieldcompanion-work-order-labor-type"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              value={laborTypeKey}
              onChange={(event) =>
                setLaborTypeKey(event.target.value as (typeof WORK_ORDER_LABOR_TYPES)[number])
              }
              data-testid="fieldcompanion-work-order-labor-type"
            >
              {WORK_ORDER_LABOR_TYPES.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </label>

          <label className="block text-sm text-slate-200" htmlFor="fieldcompanion-work-order-labor-notes">
            Labor notes
            <textarea
              id="fieldcompanion-work-order-labor-notes"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
              rows={2}
              value={laborNotes}
              onChange={(event) => setLaborNotes(event.target.value)}
              data-testid="fieldcompanion-work-order-labor-notes"
            />
          </label>
        </div>
      )}

      <div className="mt-4 flex flex-wrap gap-2">
        {editable && (
          <button
            type="button"
            className="rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500 disabled:opacity-50"
            disabled={!laborHoursValid || laborMutation.isPending || statusMutation.isPending}
            data-testid="fieldcompanion-work-order-log-labor"
            onClick={() => {
              setSuccessMessage(null)
              laborMutation.mutate()
            }}
          >
            {laborMutation.isPending ? 'Logging…' : 'Log labor'}
          </button>
        )}

        {statusAction && statusActionLabel && (
          <button
            type="button"
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
            disabled={statusMutation.isPending || laborMutation.isPending}
            data-testid="fieldcompanion-work-order-update-status"
            onClick={() => {
              setSuccessMessage(null)
              statusMutation.mutate(statusAction)
            }}
          >
            {statusMutation.isPending ? 'Updating…' : statusActionLabel}
          </button>
        )}
      </div>

      {(statusMutation.isError || laborMutation.isError) && (
        <ApiErrorCallout
          className="mt-2"
          testId="fieldcompanion-work-order-error"
          title="Work order update failed"
          message={getErrorMessage(statusMutation.error ?? laborMutation.error, 'Work order update failed.')}
        />
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="fieldcompanion-work-order-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
