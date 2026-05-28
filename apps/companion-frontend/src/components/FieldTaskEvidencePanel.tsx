import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useRef, useState } from 'react'

import { submitCompanionFieldEvidence, validateCompanionFieldTask } from '../api/client'
import { companionPlainReason } from '../lib/companionPlainReason'
import type { FieldInboxTaskItem } from '../api/types'
import {
  defaultContentType,
  defaultFileName,
  type EvidenceCaptureKind,
  fileToBase64,
} from '../lib/evidenceCapture'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

interface FieldTaskEvidencePanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  onUploadComplete?: () => void
}

export function FieldTaskEvidencePanel({
  accessToken,
  task,
  onUploadComplete,
}: FieldTaskEvidencePanelProps) {
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [captureKind, setCaptureKind] = useState<EvidenceCaptureKind>('photo')
  const [notes, setNotes] = useState('')
  const [successMessage, setSuccessMessage] = useState<string | null>(null)

  const submitMutation = useMutation({
    mutationFn: async (file: File) => {
      const validation = await validateCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'evidence',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          validation.reasonMessage ?? 'Evidence cannot be uploaded for this task right now.',
        )
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'evidence',
        phase: 'uploading',
      })
      const contentBase64 = await fileToBase64(file)
      return submitCompanionFieldEvidence(accessToken, {
        taskKey: task.taskKey,
        captureKind,
        fileName: file.name || defaultFileName(captureKind),
        contentType: file.type || defaultContentType(captureKind),
        contentBase64,
        notes: notes.trim() || null,
      })
    },
    onSuccess: (response) => {
      const message = `Uploaded ${response.evidenceTypeKey} evidence (${response.sizeBytes} bytes) to TrainArr.`
      setSuccessMessage(message)
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'evidence',
        phase: 'synced',
        message,
      })
      pushSubmissionToast({ tone: 'success', message })
      void queryClient.invalidateQueries({ queryKey: ['companion-field-inbox', accessToken] })
      onUploadComplete?.()
    },
    onError: (error) => {
      const message = companionPlainReason(error, 'Evidence upload failed.')
      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'evidence',
        phase: 'failed',
        message,
      })
      pushSubmissionToast({ tone: 'error', message })
    },
  })

  const acceptByKind: Record<EvidenceCaptureKind, string> = {
    photo: 'image/*',
    document: 'application/pdf,image/*,.doc,.docx',
    signature: 'image/png,image/jpeg',
  }

  return (
    <div
      className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4"
      data-testid="companion-field-evidence-panel"
    >
      <h4 className="text-sm font-semibold text-slate-100">Capture evidence</h4>
      <p className="mt-1 text-xs text-slate-400">
        Uploads to TrainArr assignment storage via the companion API (same path as product
        evidence upload).
      </p>

      <div className="mt-3 flex flex-wrap gap-2">
        {(['photo', 'document', 'signature'] as const).map((kind) => (
          <button
            key={kind}
            type="button"
            className={`rounded-full px-3 py-1 text-xs font-medium capitalize ${
              captureKind === kind
                ? 'bg-teal-600 text-white'
                : 'border border-slate-700 text-slate-300'
            }`}
            data-testid={`companion-evidence-kind-${kind}`}
            onClick={() => setCaptureKind(kind)}
          >
            {kind}
          </button>
        ))}
      </div>

      <label className="mt-3 block text-xs text-slate-300">
        <span className="font-medium">File</span>
        <input
          ref={fileInputRef}
          className="mt-1 block w-full text-sm text-slate-200"
          type="file"
          accept={acceptByKind[captureKind]}
          capture={captureKind === 'photo' ? 'environment' : undefined}
          data-testid="companion-evidence-file-input"
        />
      </label>

      <label className="mt-3 block text-xs text-slate-300">
        <span className="font-medium">Notes (optional)</span>
        <textarea
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-100"
          rows={2}
          value={notes}
          onChange={(event) => setNotes(event.target.value)}
        />
      </label>

      <button
        type="button"
        className="mt-3 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        disabled={submitMutation.isPending}
        data-testid="companion-evidence-submit"
        onClick={() => {
          const file = fileInputRef.current?.files?.[0]
          if (!file) {
            return
          }
          setSuccessMessage(null)
          submitMutation.mutate(file)
        }}
      >
        {submitMutation.isPending ? 'Uploading…' : 'Upload evidence'}
      </button>

      {submitMutation.isError && (
        <p className="mt-2 text-sm text-rose-400" data-testid="companion-evidence-error">
          {submitMutation.error instanceof Error
            ? submitMutation.error.message
            : 'Evidence upload failed.'}
        </p>
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-emerald-300" data-testid="companion-evidence-success">
          {successMessage}
        </p>
      )}
    </div>
  )
}
