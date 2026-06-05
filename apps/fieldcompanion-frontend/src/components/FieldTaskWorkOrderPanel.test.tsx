import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskWorkOrderPanel } from './FieldTaskWorkOrderPanel'
import type { FieldInboxTaskItem } from '../api/types'
import * as client from '../api/client'

const workOrderTask: FieldInboxTaskItem = {
  taskKey: 'maintainarr:work-order:dddddddd-dddd-dddd-dddd-dddddddddddd',
  productKey: 'maintainarr',
  taskType: 'work_order',
  title: 'Replace pump seal',
  subtitle: 'PMP-100 · Pump 1',
  status: 'open',
  priority: 'high',
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/work-orders/dddddddd-dddd-dddd-dddd-dddddddddddd',
}

const workOrderDetail = {
  taskKey: workOrderTask.taskKey,
  productKey: 'maintainarr',
  workOrderId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  workOrderNumber: 'WO-1001',
  assetTag: 'PMP-100',
  assetName: 'Pump 1',
  title: 'Replace pump seal',
  description: 'Seal leaking at coupling.',
  priority: 'high',
  status: 'open',
  tasks: [
    {
      taskLineId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      title: 'Remove old seal',
      description: '',
      sortOrder: 1,
      status: 'open',
      completedAt: null,
    },
  ],
  laborEntries: [],
}

vi.mock('../api/client', () => ({
  validateFieldCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: workOrderTask.taskKey,
    productKey: 'maintainarr',
    title: workOrderTask.title,
    blockedReason: null,
  })),
  getFieldCompanionFieldWorkOrderDetail: vi.fn(async () => workOrderDetail),
  updateFieldCompanionFieldWorkOrderStatus: vi.fn(async () => ({
    taskKey: workOrderTask.taskKey,
    productKey: 'maintainarr',
    workOrderId: workOrderDetail.workOrderId,
    status: 'in_progress',
    updatedAt: '2026-05-28T12:00:00.000Z',
  })),
  logFieldCompanionFieldWorkOrderLabor: vi.fn(async () => ({
    taskKey: workOrderTask.taskKey,
    productKey: 'maintainarr',
    workOrderId: workOrderDetail.workOrderId,
    laborEntryId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    hoursWorked: 1.5,
    laborTypeKey: 'regular',
    status: 'in_progress',
    loggedAt: '2026-05-28T12:00:00.000Z',
  })),
}))

describe('FieldTaskWorkOrderPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('renders work order detail for maintainarr work order tasks', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldTaskWorkOrderPanel accessToken="test-token" task={workOrderTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('fieldcompanion-field-work-order-panel')).toBeInTheDocument()
    expect(await screen.findByText('Remove old seal')).toBeInTheDocument()
    expect(await screen.findByTestId('fieldcompanion-work-order-log-labor')).toBeInTheDocument()
  })

  it('logs labor and updates status through fieldcompanion API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { logFieldCompanionFieldWorkOrderLabor, updateFieldCompanionFieldWorkOrderStatus } =
      await import('../api/client')

    render(
      <QueryClientProvider client={client}>
        <FieldTaskWorkOrderPanel accessToken="test-token" task={workOrderTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-work-order-labor-hours')
    fireEvent.change(screen.getByTestId('fieldcompanion-work-order-labor-hours'), {
      target: { value: '1.5' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-work-order-log-labor'))

    await waitFor(() => {
      expect(logFieldCompanionFieldWorkOrderLabor).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({
          taskKey: workOrderTask.taskKey,
          hoursWorked: 1.5,
          laborTypeKey: 'regular',
        }),
      )
    })

    fireEvent.click(screen.getByTestId('fieldcompanion-work-order-update-status'))

    await waitFor(() => {
      expect(updateFieldCompanionFieldWorkOrderStatus).toHaveBeenCalledWith('test-token', {
        taskKey: workOrderTask.taskKey,
        status: 'in_progress',
      })
    })

    expect(await screen.findByTestId('fieldcompanion-work-order-success')).toBeInTheDocument()
  })

  it('renders retryable error callout when work order detail fails', async () => {
    vi.mocked(client.getFieldCompanionFieldWorkOrderDetail).mockRejectedValueOnce(
      new Error('work order detail unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskWorkOrderPanel accessToken="test-token" task={workOrderTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('work order detail unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry work order detail' })).toBeInTheDocument()
  })

  it('renders mutation failure in shared callout', async () => {
    vi.mocked(client.logFieldCompanionFieldWorkOrderLabor).mockRejectedValueOnce(new Error('labor failed'))
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskWorkOrderPanel accessToken="test-token" task={workOrderTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-work-order-labor-hours')
    fireEvent.click(screen.getByTestId('fieldcompanion-work-order-log-labor'))

    expect(await screen.findByText('labor failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-work-order-error')).toBeInTheDocument()
  })
})
