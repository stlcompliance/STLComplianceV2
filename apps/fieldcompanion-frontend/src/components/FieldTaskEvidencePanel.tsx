import { useEffect, useRef, useState, type PointerEvent as ReactPointerEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { submitFieldCompanionFieldEvidence, validateFieldCompanionFieldTask } from '../api/client'
import { resolveDeniedReason } from '../lib/FieldCompanionDeniedReasonCatalog'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import type { FieldInboxTaskItem } from '../api/types'
import {
  canvasToFile,
  defaultContentType,
  defaultFileName,
  type EvidenceCaptureKind,
  fileToBase64,
} from '../lib/evidenceCapture'
import {
  formatFieldCompanionEvidenceBytes,
  prepareFieldCompanionEvidenceAttachment,
  type FieldCompanionEvidenceAttachmentSnapshot,
} from '../lib/evidenceOptimization'
import { pushSubmissionToast, setLocalSubmission } from '../lib/submissionState'

function getThemeColor(variableName: string): string {
  if (typeof document === 'undefined') {
    return ''
  }

  const rootColor = window.getComputedStyle(document.documentElement).getPropertyValue(variableName).trim()
  if (rootColor) {
    return rootColor
  }

  return window.getComputedStyle(document.body).backgroundColor
}

interface FieldTaskEvidencePanelProps {
  accessToken: string
  task: FieldInboxTaskItem
  signerDisplayName?: string
  onUploadComplete?: () => void
}

export function FieldTaskEvidencePanel({
  accessToken,
  task,
  signerDisplayName,
  onUploadComplete,
}: FieldTaskEvidencePanelProps) {
  const queryClient = useQueryClient()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const signatureCanvasRef = useRef<HTMLCanvasElement>(null)
  const signatureDrawingRef = useRef(false)
  const signatureLastPointRef = useRef<{ x: number; y: number } | null>(null)
  const [captureKind, setCaptureKind] = useState<EvidenceCaptureKind>('photo')
  const [notes, setNotes] = useState('')
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [attachmentSnapshot, setAttachmentSnapshot] = useState<FieldCompanionEvidenceAttachmentSnapshot | null>(null)
  const [attachmentError, setAttachmentError] = useState<string | null>(null)
  const [isPreparingAttachment, setIsPreparingAttachment] = useState(false)
  const [signatureConfirmed, setSignatureConfirmed] = useState(false)
  const [signatureHasInk, setSignatureHasInk] = useState(false)
  const [signatureError, setSignatureError] = useState<string | null>(null)

  useEffect(() => {
    setAttachmentSnapshot(null)
    setAttachmentError(null)
    setIsPreparingAttachment(false)
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }

    if (captureKind !== 'signature') {
      return
    }

    const canvas = signatureCanvasRef.current
    if (!canvas) {
      return
    }

    const context = canvas.getContext('2d')
    if (!context) {
      return
    }

    const { width, height } = canvas
    context.fillStyle = getThemeColor('--color-bg-surface')
    context.fillRect(0, 0, width, height)
    context.lineWidth = 2.5
    context.lineCap = 'round'
    context.lineJoin = 'round'
    context.strokeStyle = getThemeColor('--color-text-primary')
    signatureDrawingRef.current = false
    signatureLastPointRef.current = null
    setSignatureHasInk(false)
    setSignatureConfirmed(false)
    setSignatureError(null)
  }, [captureKind])

  const handleEvidenceFileSelection = async (file: File | null) => {
    setSuccessMessage(null)
    setAttachmentError(null)
    setAttachmentSnapshot(null)

    if (!file) {
      return
    }

    setIsPreparingAttachment(true)
    try {
      const snapshot = await prepareFieldCompanionEvidenceAttachment(file, captureKind)
      setAttachmentSnapshot(snapshot)
    } catch (error) {
      const message = FieldCompanionPlainReason(error, 'Unable to prepare the attachment.')
      setAttachmentError(message)
      setAttachmentSnapshot({
        originalFile: file,
        uploadFile: file,
        previewDataUrl: null,
        originalSizeBytes: file.size,
        uploadSizeBytes: file.size,
        wasOptimized: false,
        preservesOriginal: true,
        summary: 'Attachment will upload unchanged.',
        storageSummary: `Original file retained at ${formatFieldCompanionEvidenceBytes(file.size)}.`,
        networkSummary: 'No optimization was applied.',
      })
    } finally {
      setIsPreparingAttachment(false)
    }
  }

  const submitMutation = useMutation({
    mutationFn: async (file: File) => {
      const validation = await validateFieldCompanionFieldTask(accessToken, {
        taskKey: task.taskKey,
        submissionKind: 'evidence',
        productKey: task.productKey,
      })
      if (!validation.allowed) {
        throw new Error(
          resolveDeniedReason(
            validation,
            'Evidence cannot be uploaded for this task right now.',
          ),
        )
      }

      setLocalSubmission({
        taskKey: task.taskKey,
        kind: 'evidence',
        phase: 'uploading',
      })
      const contentBase64 = await fileToBase64(file)
      return submitFieldCompanionFieldEvidence(accessToken, {
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
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-field-inbox', accessToken] })
      onUploadComplete?.()
    },
    onError: (error) => {
      const message = FieldCompanionPlainReason(error, 'Evidence upload failed.')
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

  const signatureInstructions =
    signerDisplayName != null
      ? `Signer on record: ${signerDisplayName}. Draw the signature that represents this session before submitting.`
      : 'Draw the signature that represents this session before submitting.'

  const signatureSummary = signerDisplayName != null
    ? `Current signer: ${signerDisplayName}`
    : 'Current signer: authenticated session'

  const resetSignatureCanvas = () => {
    const canvas = signatureCanvasRef.current
    if (!canvas) {
      return
    }

    const context = canvas.getContext('2d')
    if (!context) {
      return
    }

    context.fillStyle = getThemeColor('--color-bg-surface')
    context.fillRect(0, 0, canvas.width, canvas.height)
    signatureDrawingRef.current = false
    signatureLastPointRef.current = null
    setSignatureHasInk(false)
    setSignatureConfirmed(false)
    setSignatureError(null)
  }

  const getCanvasPoint = (event: ReactPointerEvent<HTMLCanvasElement>) => {
    const canvas = signatureCanvasRef.current
    if (!canvas) {
      return null
    }

    const rect = canvas.getBoundingClientRect()
    const scaleX = canvas.width / (rect.width || canvas.width)
    const scaleY = canvas.height / (rect.height || canvas.height)
    return {
      x: (event.clientX - rect.left) * scaleX,
      y: (event.clientY - rect.top) * scaleY,
    }
  }

  const drawSignatureSegment = (from: { x: number; y: number }, to: { x: number; y: number }) => {
    const canvas = signatureCanvasRef.current
    const context = canvas?.getContext('2d')
    if (!canvas || !context) {
      return
    }

    context.beginPath()
    context.moveTo(from.x, from.y)
    context.lineTo(to.x, to.y)
    context.stroke()
  }

  const exportSignatureFile = async (): Promise<File | null> => {
    const canvas = signatureCanvasRef.current
    if (!canvas || !signatureHasInk || !signatureConfirmed) {
      return null
    }

    return canvasToFile(canvas, defaultFileName('signature'), defaultContentType('signature'))
  }

  return (
    <div
      className="mt-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
      data-testid="fieldcompanion-field-evidence-panel"
    >
      <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Capture evidence</h4>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Uploads to TrainArr assignment storage via the fieldcompanion API (same path as product
        evidence upload).
      </p>

      <div className="mt-3 flex flex-wrap gap-2">
        {(['photo', 'document', 'signature'] as const).map((kind) => (
          <button
            key={kind}
            type="button"
            className={`rounded-full px-3 py-1 text-xs font-medium capitalize ${
              captureKind === kind
                ? 'bg-[var(--color-accent)] text-[var(--color-on-accent)]'
                : 'border border-[var(--color-border-subtle)] text-[var(--color-text-secondary)]'
            }`}
            data-testid={`fieldcompanion-evidence-kind-${kind}`}
            onClick={() => setCaptureKind(kind)}
          >
            {kind}
          </button>
        ))}
      </div>

      {captureKind === 'signature' ? (
        <div className="mt-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">
          <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Signature review</p>
          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{signatureInstructions}</p>
          <div className="mt-3 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-2 shadow-inner">
            <canvas
              ref={signatureCanvasRef}
              className="block h-56 w-full touch-none rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]"
              height={224}
              width={672}
              data-testid="fieldcompanion-signature-canvas"
              aria-label="Draw signature"
              onPointerDown={(event) => {
                const canvas = signatureCanvasRef.current
                if (!canvas) {
                  return
                }
                canvas.setPointerCapture?.(event.pointerId)
                const point = getCanvasPoint(event)
                if (!point) {
                  return
                }
                signatureDrawingRef.current = true
                signatureLastPointRef.current = point
              }}
              onPointerMove={(event) => {
                if (!signatureDrawingRef.current) {
                  return
                }
                const point = getCanvasPoint(event)
                const lastPoint = signatureLastPointRef.current
                if (!point || !lastPoint) {
                  return
                }
                drawSignatureSegment(lastPoint, point)
                signatureLastPointRef.current = point
                setSignatureHasInk(true)
              }}
              onPointerUp={(event) => {
                const canvas = signatureCanvasRef.current
                canvas?.releasePointerCapture?.(event.pointerId)
                signatureDrawingRef.current = false
                signatureLastPointRef.current = null
              }}
              onPointerLeave={() => {
                signatureDrawingRef.current = false
                signatureLastPointRef.current = null
              }}
            />
          </div>
          <div className="mt-3 flex flex-wrap items-center gap-3">
            <button
              type="button"
              className="rounded-md border border-[var(--color-border-strong)] px-3 py-1.5 text-sm text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]"
              data-testid="fieldcompanion-signature-clear"
              onClick={() => resetSignatureCanvas()}
            >
              Clear signature
            </button>
            <label className="flex items-center gap-2 text-xs text-[var(--color-text-secondary)]" htmlFor="fieldcompanion-signature-confirmed">
              <input
                id="fieldcompanion-signature-confirmed"
                type="checkbox"
                checked={signatureConfirmed}
                onChange={(event) => {
                  setSignatureConfirmed(event.target.checked)
                  setSignatureError(null)
                }}
                data-testid="fieldcompanion-signature-confirmed"
              />
              I confirm this signature reflects my intent for this record.
            </label>
          </div>

          <details className="mt-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-xs text-[var(--color-text-secondary)]">
            <summary className="cursor-pointer text-[var(--color-text-primary)]">Use an uploaded signature image instead</summary>
            <label className="mt-3 block text-xs text-[var(--color-text-secondary)]" htmlFor="fieldcompanion-evidence-file-input">
              <span className="font-medium">Signature image file</span>
              <input
                id="fieldcompanion-evidence-file-input"
                ref={fileInputRef}
                className="mt-1 block w-full text-sm text-[var(--color-text-secondary)]"
                type="file"
                accept={acceptByKind.signature}
                data-testid="fieldcompanion-evidence-file-input"
                onChange={(event) => {
                  void handleEvidenceFileSelection(event.target.files?.[0] ?? null)
                }}
              />
            </label>
          </details>

          <p className="mt-2 text-xs text-[var(--color-text-muted)]" data-testid="fieldcompanion-signature-summary">
            {signatureSummary}
          </p>
          {signatureError && (
            <p className="mt-2 rounded-lg border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] px-3 py-2 text-xs text-[var(--tone-warning-text)]">
              {signatureError}
            </p>
          )}
        </div>
      ) : (
        <label className="mt-3 block text-xs text-[var(--color-text-secondary)]" htmlFor="fieldcompanion-evidence-file-input">
          <span className="font-medium">Evidence file</span>
          <input
            id="fieldcompanion-evidence-file-input"
            ref={fileInputRef}
            className="mt-1 block w-full text-sm text-[var(--color-text-secondary)]"
            type="file"
            accept={acceptByKind[captureKind]}
            capture={captureKind === 'photo' ? 'environment' : undefined}
            data-testid="fieldcompanion-evidence-file-input"
            onChange={(event) => {
              void handleEvidenceFileSelection(event.target.files?.[0] ?? null)
            }}
          />
        </label>
      )}

      {attachmentSnapshot ? (
        <div
          className="mt-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3"
          data-testid="fieldcompanion-evidence-attachment-preview"
          aria-live="polite"
        >
          <div className="flex flex-col gap-3 sm:flex-row sm:items-start">
            {attachmentSnapshot.previewDataUrl ? (
              <img
                alt="Evidence attachment preview"
                className="h-24 w-24 rounded-md border border-[var(--color-border-subtle)] object-cover"
                src={attachmentSnapshot.previewDataUrl}
              />
            ) : (
              <div className="flex h-24 w-24 items-center justify-center rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] text-center text-[10px] text-[var(--color-text-muted)]">
                Preview unavailable
              </div>
            )}
            <div className="min-w-0 flex-1">
              <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Attachment review</p>
              <p
                className="mt-1 text-sm text-[var(--color-text-primary)]"
                data-testid="fieldcompanion-evidence-attachment-summary"
              >
                {attachmentSnapshot.summary}
              </p>
              <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                Original: {formatFieldCompanionEvidenceBytes(attachmentSnapshot.originalSizeBytes)} · Upload:{' '}
                {formatFieldCompanionEvidenceBytes(attachmentSnapshot.uploadSizeBytes)}
              </p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{attachmentSnapshot.storageSummary}</p>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">{attachmentSnapshot.networkSummary}</p>
            </div>
          </div>
        </div>
      ) : null}

      {attachmentError ? (
        <p
          className="mt-2 rounded-lg border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] px-3 py-2 text-xs text-[var(--tone-warning-text)]"
          data-testid="fieldcompanion-evidence-attachment-error"
          aria-live="polite"
        >
          {attachmentError}
        </p>
      ) : null}

      <label className="mt-3 block text-xs text-[var(--color-text-secondary)]" htmlFor="fieldcompanion-evidence-notes">
        <span className="font-medium">Notes (optional)</span>
        <textarea
          id="fieldcompanion-evidence-notes"
          className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          rows={2}
          value={notes}
          onChange={(event) => setNotes(event.target.value)}
        />
      </label>

      <button
        type="button"
        className="mt-3 min-h-12 rounded-md bg-[var(--color-accent)] px-4 py-3 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
        disabled={submitMutation.isPending || isPreparingAttachment}
        data-testid="fieldcompanion-evidence-submit"
        onClick={async () => {
          const fallbackFile = fileInputRef.current?.files?.[0]
          let file = attachmentSnapshot?.uploadFile ?? fallbackFile

          if (captureKind === 'signature') {
            if (!signatureConfirmed) {
              setSignatureError('Confirm the signature intent before submitting.')
              return
            }

            const drawnSignatureFile = signatureHasInk ? await exportSignatureFile() : null
            file = drawnSignatureFile ?? attachmentSnapshot?.uploadFile ?? fallbackFile
            if (!file) {
              setSignatureError('Capture a signature before submitting.')
              return
            }
          }

          if (!file) {
            return
          }

          setSuccessMessage(null)
          setSignatureError(null)
          submitMutation.mutate(file)
        }}
      >
        {submitMutation.isPending
          ? 'Uploading…'
          : captureKind === 'signature'
            ? 'Submit signature'
            : 'Upload evidence'}
      </button>

      {submitMutation.isError && (
        <div aria-live="polite" className="mt-2">
          <ApiErrorCallout
            testId="fieldcompanion-evidence-error"
            title="Evidence upload failed"
            message={getErrorMessage(submitMutation.error, 'Evidence upload failed.')}
          />
        </div>
      )}

      {successMessage && (
        <p className="mt-2 text-sm text-[var(--tone-success-text)]" data-testid="fieldcompanion-evidence-success" aria-live="polite">
          {successMessage}
        </p>
      )}
    </div>
  )
}
