import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  getCompanionFieldReceivingDetail,
  postCompanionFieldReceiving,
  updateCompanionFieldReceivingLine,
  validateCompanionFieldTask,
} from '../api/client'
import type { FieldInboxTaskItem } from '../api/types'
import { companionPlainReason } from '../lib/companionPlainReason'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import {
  parseQuantityInput,
  receivingEditable,
  receivingPostActionLabel,
  receivingPostReady,
} from '../lib/fieldReceiving'
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
  const [lineQuantities, setLineQuantities] = useState<Record<string, string>>({})
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['companion-field-receiving', accessToken, task.taskKey],
    queryFn: async () => {
      const validation = await validateCompanionFieldTask(accessToken, {
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

      return getCompanionFieldReceivingDetail(accessToken, task.taskKey)
    },
    enabled: Boolean(accessToken),
  })

  const lineMutation = useMutation({
    mutationFn: async (input: { lineId: string; quantityReceived: number }) => {
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'syncing',
      })

      return updateCompanionFieldReceivingLine(accessToken, {
        taskKey: task.taskKey,
        lineId: input.lineId,
        quantityReceived: input.quantityReceived,
      })
    },
    onSuccess: (response) => {
      const message = `Updated received quantity to ${response.quantityReceived}.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['companion-field-receiving', accessToken, task.taskKey],
      })
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'Receiving line update failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const postMutation = useMutation({
    mutationFn: async () => {
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'syncing',
      })

      return postCompanionFieldReceiving(accessToken, {
        taskKey: task.taskKey,
      })
    },
    onSuccess: () => {
      const message = 'Receiving receipt posted.'
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'receiving',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['companion-field-receiving', accessToken, task.taskKey],
      })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'Receiving post failed.')
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
  const editable = detail ? receivingEditable(detail.status) : false
  const postLabel = detail ? receivingPostActionLabel(detail.status) : null

  const effectiveLineQuantities =
    detail?.lines.map((line) => {
      const localValue = lineQuantities[line.lineId]
      if (localValue !== undefined) {
        const parsed = parseQuantityInput(localValue)
        if (parsed !== null) {
          return parsed
        }
      }

      return line.quantityReceived
    }) ?? []

  const canPost = receivingPostReady(
    effectiveLineQuantities.map((quantityReceived) => ({ quantityReceived })),
  )

  if (detailQuery.isLoading) {
    return (
      <div
        className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-400"
        data-testid="companion-field-receiving-panel"
      >
        Loading receiving detail…
      </div>
    )
  }

  if (detailQuery.isError) {
    return (
      <div
        className="mt-4 rounded-lg border border-rose-900/50 bg-rose-950/20 p-4 text-sm text-rose-300"
        data-testid="companion-field-receiving-panel"
      >
        {detailQuery.error instanceof Error
          ? detailQuery.error.message
          : 'Failed to load receiving detail.'}
      </div>
    )
  }

  if (!detail) {
    return null
  }

  return (
    <div
      className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="companion-field-receiving-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Count and post receiving</h4>
      <p className="mt-1 text-xs text-slate-400">
        {detail.receiptKey} · PO {detail.purchaseOrderKey} · {detail.binName} ({detail.locationName})
      </p>
      {detail.notes && <p className="mt-2 text-sm text-slate-300">{detail.notes}</p>}

      {detail.lines.length > 0 && (
        <ul className="mt-4 space-y-3">
          {detail.lines.map((line) => {
            const quantityValue =
              lineQuantities[line.lineId] ?? String(line.quantityReceived)
            const quantityValid = parseQuantityInput(quantityValue) !== null

            return (
              <li
                key={line.lineId}
                className="rounded-md border border-slate-800 bg-slate-900/70 px-3 py-3"
                data-testid={`companion-receiving-line-${line.lineNumber}`}
              >
                <div className="text-sm text-slate-100">
                  {line.partKey} · {line.partDisplayName}
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  Expected {line.quantityExpected} · Ordered {line.quantityOrdered} · Remaining{' '}
                  {line.quantityRemainingOnOrder}
                  {line.openExceptionCount > 0
                    ? ` · ${line.openExceptionCount} open exception(s)`
                    : ''}
                </p>

                {editable && (
                  <label className="mt-2 block text-sm text-slate-200" htmlFor="fieldtaskreceiving-quantity-received">
          Quantity received
          <input id="fieldtaskreceiving-quantity-received"
                      type="number"
                      min="0"
                      step="1"
                      className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                      value={quantityValue}
                      onChange={(event) =>
                        setLineQuantities((current) => ({
                          ...current,
                          [line.lineId]: event.target.value,
                        }))
                      }
                      data-testid={`companion-receiving-line-qty-${line.lineNumber}`}
                    />
                  </label>
                )}

                {editable && (
                  <button
                    type="button"
                    className="mt-2 rounded-md border border-slate-600 px-3 py-1.5 text-xs font-medium text-slate-100 hover:border-teal-500 disabled:opacity-50"
                    disabled={!quantityValid || lineMutation.isPending || postMutation.isPending}
                    data-testid={`companion-receiving-save-line-${line.lineNumber}`}
                    onClick={() => {
                      const quantity = parseQuantityInput(quantityValue)
                      if (quantity === null) {
                        return
                      }

                      setSuccessMessage(null)
                      lineMutation.mutate({
                        lineId: line.lineId,
                        quantityReceived: quantity,
                      })
                    }}
                  >
                    {lineMutation.isPending ? 'Saving…' : 'Save count'}
                  </button>
                )}
              </li>
            )
          })}
        </ul>
      )}

      <div className="mt-4 flex flex-wrap gap-2">
        {editable && postLabel && (
          <button
            type="button"
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
            disabled={!canPost || postMutation.isPending || lineMutation.isPending}
            data-testid="companion-receiving-post"
            onClick={() => {
              setSuccessMessage(null)
              postMutation.mutate()
            }}
          >
            {postMutation.isPending ? 'Posting…' : postLabel}
          </button>
        )}
      </div>

      {(lineMutation.isError || postMutation.isError) && (
        <p className="mt-2 text-sm text-rose-400" data-testid="companion-receiving-error">
          {(lineMutation.error ?? postMutation.error) instanceof Error
            ? (lineMutation.error ?? postMutation.error)?.message
            : 'Receiving update failed.'}
        </p>
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="companion-receiving-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
