import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskReceivingPanel } from './FieldTaskReceivingPanel'
import type { FieldInboxTaskItem } from '../api/types'
import * as client from '../api/client'

const receivingTask: FieldInboxTaskItem = {
  taskKey: 'supplyarr:receiving:eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  productKey: 'supplyarr',
  taskType: 'receiving',
  title: 'RCPT-1001',
  subtitle: 'PO-5001',
  status: 'draft',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/receiving/eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
}

const receivingDetail = {
  taskKey: receivingTask.taskKey,
  productKey: 'supplyarr',
  receivingReceiptId: 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  receiptKey: 'RCPT-1001',
  status: 'draft',
  purchaseOrderKey: 'PO-5001',
  binKey: 'BIN-A1',
  binName: 'Main bin',
  locationName: 'Warehouse 1',
  notes: 'Dock delivery',
  lines: [
    {
      lineId: 'ffffffff-ffff-ffff-ffff-ffffffffffff',
      lineNumber: 1,
      partKey: 'FLT-001',
      partDisplayName: 'Oil filter',
      quantityExpected: 4,
      quantityReceived: 0,
      quantityOrdered: 4,
      quantityRemainingOnOrder: 4,
      openExceptionCount: 0,
    },
  ],
}

vi.mock('../api/client', () => ({
  validateFieldCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: receivingTask.taskKey,
    productKey: 'supplyarr',
    title: receivingTask.title,
    blockedReason: null,
  })),
  getFieldCompanionFieldReceivingDetail: vi.fn(async () => receivingDetail),
  updateFieldCompanionFieldReceivingLine: vi.fn(async () => ({
    taskKey: receivingTask.taskKey,
    productKey: 'supplyarr',
    receivingReceiptId: receivingDetail.receivingReceiptId,
    lineId: receivingDetail.lines[0]!.lineId,
    quantityReceived: 4,
    status: 'draft',
    updatedAt: '2026-05-28T12:00:00.000Z',
  })),
  postFieldCompanionFieldReceiving: vi.fn(async () => ({
    taskKey: receivingTask.taskKey,
    productKey: 'supplyarr',
    receivingReceiptId: receivingDetail.receivingReceiptId,
    status: 'posted',
    postedAt: '2026-05-28T12:05:00.000Z',
  })),
}))

describe('FieldTaskReceivingPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('renders receiving detail for supplyarr receiving tasks', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldTaskReceivingPanel accessToken="test-token" task={receivingTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('fieldcompanion-field-receiving-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('fieldcompanion-receiving-line-1')).toBeInTheDocument()
    expect(await screen.findByTestId('fieldcompanion-receiving-save-line-1')).toBeInTheDocument()
  })

  it('updates line quantity and posts receiving through fieldcompanion API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { updateFieldCompanionFieldReceivingLine, postFieldCompanionFieldReceiving } =
      await import('../api/client')

    render(
      <QueryClientProvider client={client}>
        <FieldTaskReceivingPanel accessToken="test-token" task={receivingTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-receiving-line-qty-1')
    fireEvent.change(screen.getByTestId('fieldcompanion-receiving-line-qty-1'), {
      target: { value: '4' },
    })
    fireEvent.click(screen.getByTestId('fieldcompanion-receiving-save-line-1'))

    await waitFor(() => {
      expect(updateFieldCompanionFieldReceivingLine).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({
          taskKey: receivingTask.taskKey,
          lineId: receivingDetail.lines[0]!.lineId,
          quantityReceived: 4,
        }),
      )
    })

    fireEvent.click(screen.getByTestId('fieldcompanion-receiving-post'))

    await waitFor(() => {
      expect(postFieldCompanionFieldReceiving).toHaveBeenCalledWith('test-token', {
        taskKey: receivingTask.taskKey,
      })
    })

    expect(await screen.findByTestId('fieldcompanion-receiving-success')).toBeInTheDocument()
  })

  it('renders retryable error callout when receiving detail fails', async () => {
    vi.mocked(client.getFieldCompanionFieldReceivingDetail).mockRejectedValueOnce(
      new Error('receiving detail unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskReceivingPanel accessToken="test-token" task={receivingTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('receiving detail unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry receiving detail' })).toBeInTheDocument()
  })

  it('renders mutation failure in shared callout', async () => {
    vi.mocked(client.updateFieldCompanionFieldReceivingLine).mockRejectedValueOnce(
      new Error('receiving line failed'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskReceivingPanel accessToken="test-token" task={receivingTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-receiving-line-qty-1')
    fireEvent.click(screen.getByTestId('fieldcompanion-receiving-save-line-1'))

    expect(await screen.findByText('receiving line failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-receiving-error')).toBeInTheDocument()
  })
})
