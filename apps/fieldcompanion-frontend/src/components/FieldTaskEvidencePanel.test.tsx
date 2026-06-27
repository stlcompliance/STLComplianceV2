import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { FieldTaskEvidencePanel } from './FieldTaskEvidencePanel'
import type { FieldInboxTaskItem } from '../api/types'

const evidenceTask: FieldInboxTaskItem = {
  taskKey: 'trainarr:assignment:22222222-2222-2222-2222-222222222222',
  productKey: 'trainarr',
  taskType: 'assignment',
  title: 'Evidence upload',
  subtitle: 'Assignment',
  status: 'assigned',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/assignments/22222222-2222-2222-2222-222222222222',
  blockedReason: null,
}

vi.mock('../api/client', () => ({
  validateFieldCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: evidenceTask.taskKey,
    productKey: 'trainarr',
    title: evidenceTask.title,
    blockedReason: null,
  })),
  submitFieldCompanionFieldEvidence: vi.fn(async () => ({
    taskKey: evidenceTask.taskKey,
    evidenceTypeKey: 'photo',
    sizeBytes: 1234,
  })),
}))

vi.mock('../lib/evidenceCapture', () => ({
  defaultContentType: () => 'image/png',
  defaultFileName: () => 'photo.png',
  canvasToFile: vi.fn(async () => new File(['signature'], 'field-signature.png', { type: 'image/png' })),
  fileToBase64: vi.fn(async (file: File) => file.name),
}))

vi.mock('../lib/evidenceOptimization', () => ({
  formatFieldCompanionEvidenceBytes: (bytes: number) => `${bytes} B`,
  prepareFieldCompanionEvidenceAttachment: vi.fn(async (file: File, captureKind: string) => {
    if (captureKind === 'photo') {
      return {
        originalFile: file,
        uploadFile: new File(['optimized'], 'photo.jpg', { type: 'image/jpeg' }),
        previewDataUrl: 'data:image/jpeg;base64,thumb',
        originalSizeBytes: file.size,
        uploadSizeBytes: 512,
        wasOptimized: true,
        preservesOriginal: false,
        summary: 'Photo optimized from 1.2 KB to 512 B.',
        storageSummary: 'Thumbnail generated for review.',
        networkSummary: 'Saved 712 B on upload.',
      }
    }

    return {
      originalFile: file,
      uploadFile: file,
      previewDataUrl: null,
      originalSizeBytes: file.size,
      uploadSizeBytes: file.size,
      wasOptimized: false,
      preservesOriginal: true,
      summary: 'Attachment will upload unchanged.',
      storageSummary: 'Original file retained.',
      networkSummary: 'No optimization was applied.',
    }
  }),
}))

describe('FieldTaskEvidencePanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('shows upload failures in shared callout', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitFieldCompanionFieldEvidence } = await import('../api/client')
    vi.mocked(submitFieldCompanionFieldEvidence).mockRejectedValueOnce(new Error('evidence failed'))

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskEvidencePanel accessToken="token" task={evidenceTask} signerDisplayName="Alex Worker" />
      </QueryClientProvider>,
    )

    const file = new File(['fake'], 'photo.png', { type: 'image/png' })
    const input = screen.getByTestId('fieldcompanion-evidence-file-input') as HTMLInputElement
    fireEvent.change(input, { target: { files: [file] } })
    expect(await screen.findByTestId('fieldcompanion-evidence-attachment-preview')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-evidence-attachment-preview')).toHaveAttribute(
      'aria-live',
      'polite',
    )
    expect(screen.getByTestId('fieldcompanion-evidence-attachment-summary')).toHaveTextContent(
      'Photo optimized from 1.2 KB to 512 B.',
    )
    fireEvent.click(screen.getByTestId('fieldcompanion-evidence-submit'))

    expect(await screen.findByText('evidence failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-evidence-error')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-evidence-error').parentElement).toHaveAttribute(
      'aria-live',
      'polite',
    )
    expect(vi.mocked(submitFieldCompanionFieldEvidence)).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        captureKind: 'photo',
        fileName: 'photo.jpg',
      }),
    )
  })

  it('captures a drawn signature and submits it as signature evidence', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitFieldCompanionFieldEvidence } = await import('../api/client')
    vi.mocked(submitFieldCompanionFieldEvidence).mockResolvedValueOnce({
      taskKey: evidenceTask.taskKey,
      productKey: 'trainarr',
      evidenceId: '00000000-0000-0000-0000-000000000001',
      evidenceTypeKey: 'signature',
      fileName: 'field-signature.png',
      contentType: 'image/png',
      sizeBytes: 4321,
      notes: null,
      createdAt: '2026-06-23T18:00:00Z',
    })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskEvidencePanel accessToken="token" task={evidenceTask} signerDisplayName="Alex Worker" />
      </QueryClientProvider>,
    )

    const canvasContext = {
      beginPath: vi.fn(),
      moveTo: vi.fn(),
      lineTo: vi.fn(),
      stroke: vi.fn(),
      fillRect: vi.fn(),
      fillStyle: '',
      lineWidth: 0,
      lineCap: '',
      lineJoin: '',
      strokeStyle: '',
    }
    vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue(canvasContext as never)
    vi.spyOn(HTMLCanvasElement.prototype, 'getBoundingClientRect').mockReturnValue({
      x: 0,
      y: 0,
      top: 0,
      left: 0,
      bottom: 224,
      right: 672,
      width: 672,
      height: 224,
      toJSON: () => undefined,
    } as DOMRect)

    fireEvent.click(screen.getByTestId('fieldcompanion-evidence-kind-signature'))
    const canvas = await screen.findByTestId('fieldcompanion-signature-canvas')
    expect(screen.getByTestId('fieldcompanion-signature-summary')).toHaveTextContent('Alex Worker')

    fireEvent.pointerDown(canvas, { clientX: 20, clientY: 20, pointerId: 1 })
    fireEvent.pointerMove(canvas, { clientX: 120, clientY: 80, pointerId: 1 })
    fireEvent.pointerUp(canvas, { clientX: 120, clientY: 80, pointerId: 1 })

    fireEvent.click(screen.getByTestId('fieldcompanion-signature-confirmed'))
    fireEvent.click(screen.getByTestId('fieldcompanion-evidence-submit'))

    expect(await screen.findByTestId('fieldcompanion-evidence-success')).toHaveTextContent('signature evidence')
    expect(vi.mocked(submitFieldCompanionFieldEvidence)).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        captureKind: 'signature',
        fileName: 'field-signature.png',
        contentType: 'image/png',
      }),
    )
  })
})
