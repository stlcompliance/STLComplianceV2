import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskInspectionPanel } from './FieldTaskInspectionPanel'
import type { FieldInboxTaskItem } from '../api/types'

const inspectionTask: FieldInboxTaskItem = {
  taskKey: 'maintainarr:inspection:22222222-2222-2222-2222-222222222222',
  productKey: 'maintainarr',
  taskType: 'inspection',
  title: 'Daily walkaround',
  subtitle: 'PMP-100 · Pump 1',
  status: 'in_progress',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/inspections/22222222-2222-2222-2222-222222222222',
}

const inspectionDetail = {
  taskKey: inspectionTask.taskKey,
  productKey: 'maintainarr',
  inspectionRunId: '22222222-2222-2222-2222-222222222222',
  assetTag: 'PMP-100',
  assetName: 'Pump 1',
  templateName: 'Daily walkaround',
  status: 'in_progress',
  result: null,
  checklistItems: [
    {
      checklistItemId: '33333333-3333-3333-3333-333333333333',
      itemKey: 'visual_leaks',
      prompt: 'Check for visible leaks',
      itemType: 'pass_fail',
      isRequired: true,
      sortOrder: 1,
    },
  ],
  answers: [],
}

vi.mock('../api/client', () => ({
  validateCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: inspectionTask.taskKey,
    productKey: 'maintainarr',
    title: inspectionTask.title,
    blockedReason: null,
  })),
  getCompanionFieldInspectionDetail: vi.fn(async () => inspectionDetail),
  submitCompanionFieldInspectionAnswers: vi.fn(async () => ({
    taskKey: inspectionTask.taskKey,
    productKey: 'maintainarr',
    inspectionRunId: inspectionDetail.inspectionRunId,
    status: 'in_progress',
    answerCount: 1,
    requiredItemCount: 1,
    answers: [
      {
        checklistItemId: '33333333-3333-3333-3333-333333333333',
        itemKey: 'visual_leaks',
        passFailValue: 'pass',
        numericValue: null,
        textValue: null,
        answeredAt: '2026-05-28T12:00:00.000Z',
      },
    ],
  })),
  completeCompanionFieldInspection: vi.fn(async () => ({
    taskKey: inspectionTask.taskKey,
    productKey: 'maintainarr',
    inspectionRunId: inspectionDetail.inspectionRunId,
    status: 'completed',
    result: 'passed',
    completedAt: '2026-05-28T12:05:00.000Z',
  })),
}))

describe('FieldTaskInspectionPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('renders inspection checklist for maintainarr inspection tasks', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldTaskInspectionPanel accessToken="test-token" task={inspectionTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('companion-field-inspection-panel')).toBeInTheDocument()
    expect(await screen.findByText('Check for visible leaks')).toBeInTheDocument()
    expect(await screen.findByTestId('companion-inspection-save')).toBeInTheDocument()
  })

  it('saves answers and completes inspection through companion API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitCompanionFieldInspectionAnswers, completeCompanionFieldInspection } =
      await import('../api/client')

    render(
      <QueryClientProvider client={client}>
        <FieldTaskInspectionPanel accessToken="test-token" task={inspectionTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('companion-inspection-pass-fail-visual_leaks')
    fireEvent.change(screen.getByTestId('companion-inspection-pass-fail-visual_leaks'), {
      target: { value: 'pass' },
    })
    fireEvent.click(screen.getByTestId('companion-inspection-complete'))

    await waitFor(() => {
      expect(submitCompanionFieldInspectionAnswers).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({
          taskKey: inspectionTask.taskKey,
          answers: [
            expect.objectContaining({
              checklistItemId: '33333333-3333-3333-3333-333333333333',
              passFailValue: 'pass',
            }),
          ],
        }),
      )
      expect(completeCompanionFieldInspection).toHaveBeenCalledWith('test-token', {
        taskKey: inspectionTask.taskKey,
      })
    })

    expect(await screen.findByTestId('companion-inspection-success')).toBeInTheDocument()
  })
})
