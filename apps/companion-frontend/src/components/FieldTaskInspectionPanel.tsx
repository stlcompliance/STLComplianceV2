import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  completeCompanionFieldInspection,
  getCompanionFieldInspectionDetail,
  submitCompanionFieldInspectionAnswers,
  validateCompanionFieldTask,
} from '../api/client'
import type { FieldInboxTaskItem } from '../api/types'
import { companionPlainReason } from '../lib/companionPlainReason'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import {
  buildInspectionAnswerInputs,
  draftsFromInspectionAnswers,
  requiredInspectionItemsAnswered,
  type InspectionAnswerDraft,
} from '../lib/fieldInspection'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

interface FieldTaskInspectionPanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  onSubmitComplete?: () => void
}

export function FieldTaskInspectionPanel({
  accessToken,
  task,
  onSubmitComplete,
}: FieldTaskInspectionPanelProps) {
  const queryClient = useQueryClient()
  const [drafts, setDrafts] = useState<Record<string, InspectionAnswerDraft>>({})
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const detailQuery = useQuery({
    queryKey: ['companion-field-inspection', accessToken, task.taskKey],
    queryFn: async () => {
      const validation = await validateCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'inspection',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          resolveDeniedReason(
            validation,
            'Inspection capture is not allowed for this task right now.',
          ),
        )
      }

      return getCompanionFieldInspectionDetail(accessToken, task.taskKey)
    },
    enabled: Boolean(accessToken),
  })

  const initializedDrafts = useMemo(() => {
    if (!detailQuery.data) {
      return drafts
    }

    if (Object.keys(drafts).length > 0) {
      return drafts
    }

    return draftsFromInspectionAnswers(detailQuery.data.answers)
  }, [detailQuery.data, drafts])

  const saveMutation = useMutation({
    mutationFn: async () => {
      const detail = detailQuery.data
      if (!detail) {
        throw new Error('Inspection detail is not loaded yet.')
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'syncing',
      })

      return submitCompanionFieldInspectionAnswers(accessToken, {
        taskKey: task.taskKey,
        answers: buildInspectionAnswerInputs(
          initializedDrafts,
          detail.checklistItems.map((item) => item.checklistItemId),
        ),
      })
    },
    onSuccess: (response) => {
      const message = `Saved ${response.answerCount} inspection answer(s) to MaintainArr.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['companion-field-inspection', accessToken, task.taskKey],
      })
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'Inspection answer submission failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const completeMutation = useMutation({
    mutationFn: async () => {
      const detail = detailQuery.data
      if (!detail) {
        throw new Error('Inspection detail is not loaded yet.')
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'syncing',
      })

      await submitCompanionFieldInspectionAnswers(accessToken, {
        taskKey: task.taskKey,
        answers: buildInspectionAnswerInputs(
          initializedDrafts,
          detail.checklistItems.map((item) => item.checklistItemId),
        ),
      })

      return completeCompanionFieldInspection(accessToken, { taskKey: task.taskKey })
    },
    onSuccess: (response) => {
      const message = `Inspection completed (${response.result}) in MaintainArr.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      onSubmitComplete?.()
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'Inspection completion failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'inspection',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const detail = detailQuery.data
  const canComplete =
    detail &&
    requiredInspectionItemsAnswered(detail.checklistItems, initializedDrafts) &&
    detail.status === 'in_progress'

  function updateDraft(checklistItemId: string, patch: Partial<InspectionAnswerDraft>): void {
    setDrafts((current) => {
      const base = Object.keys(current).length > 0 ? current : initializedDrafts
      const existing = base[checklistItemId] ?? {
        passFailValue: '',
        numericValue: '',
        textValue: '',
      }
      return {
        ...base,
        [checklistItemId]: { ...existing, ...patch },
      }
    })
  }

  if (detailQuery.isLoading) {
    return (
      <div
        className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-400"
        data-testid="companion-field-inspection-panel"
      >
        Loading inspection checklist…
      </div>
    )
  }

  if (detailQuery.isError) {
    return (
      <div
        className="mt-4 rounded-lg border border-rose-900/50 bg-rose-950/20 p-4 text-sm text-rose-300"
        data-testid="companion-field-inspection-panel"
      >
        {detailQuery.error instanceof Error
          ? detailQuery.error.message
          : 'Failed to load inspection checklist.'}
      </div>
    )
  }

  if (!detail) {
    return null
  }

  return (
    <div
      className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="companion-field-inspection-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Complete inspection</h4>
      <p className="mt-1 text-xs text-slate-400">
        {detail.templateName} · {detail.assetTag} · {detail.assetName}
      </p>

      <ul className="mt-4 space-y-3">
        {detail.checklistItems.map((item) => {
          const draft = initializedDrafts[item.checklistItemId] ?? {
            passFailValue: '',
            numericValue: '',
            textValue: '',
          }

          return (
            <li
              key={item.checklistItemId}
              className="rounded-md border border-slate-800 bg-slate-900/70 p-3"
              data-testid={`companion-inspection-item-${item.itemKey}`}
            >
              <p className="text-sm text-slate-100">
                {item.prompt}
                {item.isRequired && <span className="text-rose-300"> *</span>}
              </p>

              {item.itemType === 'pass_fail' && (
                <select
                  className="mt-2 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                  value={draft.passFailValue}
                  onChange={(event) =>
                    updateDraft(item.checklistItemId, { passFailValue: event.target.value })
                  }
                  data-testid={`companion-inspection-pass-fail-${item.itemKey}`}
                >
                  <option value="">Select…</option>
                  <option value="pass">Pass</option>
                  <option value="fail">Fail</option>
                  <option value="na">N/A</option>
                </select>
              )}

              {item.itemType === 'numeric' && (
                <input
                  type="number"
                  className="mt-2 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                  value={draft.numericValue}
                  onChange={(event) =>
                    updateDraft(item.checklistItemId, { numericValue: event.target.value })
                  }
                  data-testid={`companion-inspection-numeric-${item.itemKey}`}
                />
              )}

              {item.itemType === 'text' && (
                <textarea
                  className="mt-2 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
                  rows={2}
                  value={draft.textValue}
                  onChange={(event) =>
                    updateDraft(item.checklistItemId, { textValue: event.target.value })
                  }
                  data-testid={`companion-inspection-text-${item.itemKey}`}
                />
              )}
            </li>
          )
        })}
      </ul>

      <div className="mt-4 flex flex-wrap gap-2">
        <button
          type="button"
          className="rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500 disabled:opacity-50"
          disabled={saveMutation.isPending || completeMutation.isPending}
          data-testid="companion-inspection-save"
          onClick={() => {
            setSuccessMessage(null)
            saveMutation.mutate()
          }}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save answers'}
        </button>
        <button
          type="button"
          className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
          disabled={!canComplete || saveMutation.isPending || completeMutation.isPending}
          data-testid="companion-inspection-complete"
          onClick={() => {
            setSuccessMessage(null)
            completeMutation.mutate()
          }}
        >
          {completeMutation.isPending ? 'Completing…' : 'Complete inspection'}
        </button>
      </div>

      {(saveMutation.isError || completeMutation.isError) && (
        <p className="mt-2 text-sm text-rose-400" data-testid="companion-inspection-error">
          {(saveMutation.error ?? completeMutation.error) instanceof Error
            ? (saveMutation.error ?? completeMutation.error)?.message
            : 'Inspection submission failed.'}
        </p>
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="companion-inspection-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
