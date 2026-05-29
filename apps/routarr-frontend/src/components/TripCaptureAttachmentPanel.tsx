import { useRef, useState } from 'react'

import {
  readFileAsDataUrl,
  uploadDriverPortalCaptureAttachment,
} from '../api/client'
import type { TripCaptureAttachmentResponse } from '../api/types'

type Props = {
  accessToken: string
  tripId: string
  subjectType: 'proof' | 'dvir'
  subjectId: string
  subjectLabel: string
  attachments: TripCaptureAttachmentResponse[]
  onUploaded: () => void
}

function SignaturePad({
  disabled,
  onCapture,
}: {
  disabled: boolean
  onCapture: (dataUrl: string) => void
}) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null)
  const drawingRef = useRef(false)

  const getPoint = (event: React.PointerEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current
    if (!canvas) {
      return { x: 0, y: 0 }
    }
    const rect = canvas.getBoundingClientRect()
    return {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top,
    }
  }

  const startDraw = (event: React.PointerEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !ctx) {
      return
    }
    drawingRef.current = true
    const point = getPoint(event)
    ctx.strokeStyle = '#e2e8f0'
    ctx.lineWidth = 2
    ctx.lineCap = 'round'
    ctx.beginPath()
    ctx.moveTo(point.x, point.y)
  }

  const draw = (event: React.PointerEvent<HTMLCanvasElement>) => {
    if (!drawingRef.current) {
      return
    }
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !ctx) {
      return
    }
    const point = getPoint(event)
    ctx.lineTo(point.x, point.y)
    ctx.stroke()
  }

  const endDraw = () => {
    drawingRef.current = false
  }

  const clear = () => {
    const canvas = canvasRef.current
    const ctx = canvas?.getContext('2d')
    if (!canvas || !ctx) {
      return
    }
    ctx.clearRect(0, 0, canvas.width, canvas.height)
  }

  const save = () => {
    const canvas = canvasRef.current
    if (!canvas) {
      return
    }
    onCapture(canvas.toDataURL('image/png'))
  }

  return (
    <div className="mt-2" data-testid="signature-pad">
      <canvas
        ref={canvasRef}
        width={280}
        height={96}
        className="rounded border border-slate-600 bg-slate-950 touch-none"
        onPointerDown={startDraw}
        onPointerMove={draw}
        onPointerUp={endDraw}
        onPointerLeave={endDraw}
      />
      <div className="mt-1 flex gap-2">
        <button
          type="button"
          className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
          disabled={disabled}
          onClick={clear}
        >
          Clear
        </button>
        <button
          type="button"
          className="rounded bg-indigo-700 px-2 py-1 text-xs text-white disabled:opacity-50"
          disabled={disabled}
          onClick={save}
        >
          Save signature
        </button>
      </div>
    </div>
  )
}

export function TripCaptureAttachmentPanel({
  accessToken,
  tripId,
  subjectType,
  subjectId,
  subjectLabel,
  attachments,
  onUploaded,
}: Props) {
  const [pending, setPending] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const upload = async (
    attachmentKind: 'photo' | 'document' | 'signature',
    fileName: string,
    contentType: string,
    contentBase64: string,
  ) => {
    setPending(true)
    setError(null)
    try {
      await uploadDriverPortalCaptureAttachment(accessToken, tripId, subjectType, subjectId, {
        attachmentKind,
        fileName,
        contentType,
        contentBase64,
      })
      onUploaded()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed')
    } finally {
      setPending(false)
    }
  }

  const onFileSelected = async (
    attachmentKind: 'photo' | 'document',
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) {
      return
    }
    const dataUrl = await readFileAsDataUrl(file)
    await upload(attachmentKind, file.name, file.type || 'application/octet-stream', dataUrl)
  }

  return (
    <div
      className="mt-2 rounded border border-slate-700 bg-slate-950/30 p-2"
      data-testid={`capture-attachments-${subjectType}-${subjectId}`}
    >
      <p className="text-xs font-medium text-slate-300">{subjectLabel} attachments</p>
      {attachments.length === 0 ? (
        <p className="mt-1 text-xs text-slate-500">No attachments yet.</p>
      ) : (
        <ul className="mt-1 space-y-1">
          {attachments.map((attachment) => (
            <li key={attachment.attachmentId} className="text-xs text-slate-400">
              {attachment.attachmentKind}: {attachment.fileName} ({attachment.sizeBytes} bytes)
            </li>
          ))}
        </ul>
      )}

      <div className="mt-2 flex flex-wrap gap-2">
        <label className="cursor-pointer rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">
          Photo
          <input
            type="file"
            accept="image/*"
            capture="environment"
            className="hidden"
            disabled={pending}
            onChange={(event) => void onFileSelected('photo', event)}
          />
        </label>
        <label className="cursor-pointer rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">
          Document
          <input
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,.webp,.heic,image/*,application/pdf"
            className="hidden"
            disabled={pending}
            onChange={(event) => void onFileSelected('document', event)}
          />
        </label>
      </div>

      <SignaturePad
        disabled={pending}
        onCapture={(dataUrl) =>
          void upload('signature', 'signature.png', 'image/png', dataUrl)
        }
      />

      {error ? (
        <p className="mt-2 text-xs text-red-400" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  )
}
