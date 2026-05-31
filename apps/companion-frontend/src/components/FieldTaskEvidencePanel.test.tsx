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
  validateCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: evidenceTask.taskKey,
    productKey: 'trainarr',
    title: evidenceTask.title,
    blockedReason: null,
  })),
  submitCompanionFieldEvidence: vi.fn(async () => ({
    taskKey: evidenceTask.taskKey,
    evidenceTypeKey: 'photo',
    sizeBytes: 1234,
  })),
}))

vi.mock('../lib/evidenceCapture', () => ({
  defaultContentType: () => 'image/png',
  defaultFileName: () => 'photo.png',
  fileToBase64: vi.fn(async () => 'ZmFrZQ=='),
}))

describe('FieldTaskEvidencePanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('shows upload failures in shared callout', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitCompanionFieldEvidence } = await import('../api/client')
    vi.mocked(submitCompanionFieldEvidence).mockRejectedValueOnce(new Error('evidence failed'))

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskEvidencePanel accessToken="token" task={evidenceTask} />
      </QueryClientProvider>,
    )

    const file = new File(['fake'], 'photo.png', { type: 'image/png' })
    const input = screen.getByTestId('companion-evidence-file-input') as HTMLInputElement
    fireEvent.change(input, { target: { files: [file] } })
    fireEvent.click(screen.getByTestId('companion-evidence-submit'))

    expect(await screen.findByText('evidence failed')).toBeInTheDocument()
    expect(screen.getByTestId('companion-evidence-error')).toBeInTheDocument()
  })
})
