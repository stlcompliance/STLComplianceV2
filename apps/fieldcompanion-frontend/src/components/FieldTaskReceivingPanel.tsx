import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getFieldCompanionFieldReceivingDetail,
  postFieldCompanionFieldReceiving,
  validateFieldCompanionFieldTask,
} from '../api/client'
import type { FieldInboxTaskItem } from '../api/types'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import { receivingPostActionLabel, receivingPostReady } from '../lib/fieldReceiving'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

interface FieldTaskReceivingPanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  onSubmitComplete?: () => void
}

export function FieldTaskReceivingPanel({
  accessToken,
  task,
  onSubmitComplete,
}: FieldTaskReceivingPanelProps) {
  const queryClient = useQueryClient()
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['fieldcompanion-field-receiving', accessToken, task.taskKey],
    queryFn: async () => {
      const validation = await validateFieldCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'receiving',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          resolveDeniedReason(
            validation,
            'Receiving updates are not allowed for this task right now.',
          ),
        )
      }

      return getFieldCompanionFieldReceivingDetail(accessToken, task.taskKey)
    },
    enabled: Boolean(accessToken),
  })

  const postMutation = useMutation({
    mutationFn: async () => {
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'syncing',
      })

      return postFieldCompanionFieldReceiving(accessToken, {
        taskKey: task.taskKey,
      })
    },
    onSuccess: () => {
      const message = 'Receiving session completed.'
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['fieldcompanion-field-receiving', accessToken, task.taskKey],
      })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = FieldCompanionPlainReason(error, 'Receiving completion failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const detail = detailQuery.data
  const postLabel = detail
    ? receivingPostActionLabel(detail.status, detail.productKey, task.blockedReason)
    : null
  const canPost = receivingPostReady(detail?.lines ?? [])

  if (detailQuery.isLoading) {
    return (
      <div
        className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-400"
        data-testid="fieldcompanion-field-receiving-panel"
      >
        Loading receiving detail…
      </div>
    )
  }

  if (detailQuery.isError) {
    return (
      <div
        className="mt-4"
        data-testid="fieldcompanion-field-receiving-panel"
      >
        <ApiErrorCallout
          message={getErrorMessage(detailQuery.error, 'Failed to load receiving detail.')}
          onRetry={() => void detailQuery.refetch()}
          retryLabel="Retry receiving detail"
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
      data-testid="fieldcompanion-field-receiving-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Review and complete receiving</h4>
      <p className="mt-1 text-xs text-slate-400">
        {detail.receiptKey} · {detail.purchaseOrderKey} · {detail.binName} ({detail.locationName})
      </p>
      {detail.notes && <p className="mt-2 text-sm text-slate-300">{detail.notes}</p>}
      {detail.productKey === 'loadarr' && (
        <p className="mt-2 text-xs text-slate-400">
          Line quantity edits stay in LoadArr. Use the owner-side session when counts need to change.
        </p>
      )}

      {detail.lines.length > 0 && (
        <ul className="mt-4 space-y-3">
          {detail.lines.map((line) => (
            <li
              key={line.lineId}
              className="rounded-md border border-slate-800 bg-slate-900/70 px-3 py-3"
              data-testid={`fieldcompanion-receiving-line-${line.lineNumber}`}
            >
              <div className="text-sm text-slate-100">
                {line.partKey} · {line.partDisplayName}
              </div>
              <p className="mt-1 text-xs text-slate-400">
                Expected {line.quantityExpected} · Received {line.quantityReceived} · Ordered{' '}
                {line.quantityOrdered} · Remaining {line.quantityRemainingOnOrder}
                {line.openExceptionCount > 0
                  ? ` · ${line.openExceptionCount} open exception(s)`
                  : ''}
              </p>
            </li>
          ))}
        </ul>
      )}

      <div className="mt-4 flex flex-wrap gap-2">
        {postLabel && (
          <button
            type="button"
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
            disabled={!canPost || postMutation.isPending}
            data-testid="fieldcompanion-receiving-post"
            onClick={() => {
              setSuccessMessage(null)
              postMutation.mutate()
            }}
          >
            {postMutation.isPending ? 'Completing…' : postLabel}
          </button>
        )}
      </div>

      {postMutation.isError && (
        <ApiErrorCallout
          className="mt-2"
          testId="fieldcompanion-receiving-error"
          title="Receiving completion failed"
          message={getErrorMessage(postMutation.error, 'Receiving completion failed.')}
        />
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="fieldcompanion-receiving-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
