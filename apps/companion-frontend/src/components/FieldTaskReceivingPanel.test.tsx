import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskReceivingPanel } from './FieldTaskReceivingPanel'
import type { FieldInboxTaskItem } from '../api/types'

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
  validateCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: receivingTask.taskKey,
    productKey: 'supplyarr',
    title: receivingTask.title,
    blockedReason: null,
  })),
  getCompanionFieldReceivingDetail: vi.fn(async () => receivingDetail),
  updateCompanionFieldReceivingLine: vi.fn(async () => ({
    taskKey: receivingTask.taskKey,
    productKey: 'supplyarr',
    receivingReceiptId: receivingDetail.receivingReceiptId,
    lineId: receivingDetail.lines[0]!.lineId,
    quantityReceived: 4,
    status: 'draft',
    updatedAt: '2026-05-28T12:00:00.000Z',
  })),
  postCompanionFieldReceiving: vi.fn(async () => ({
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

    expect(await screen.findByTestId('companion-field-receiving-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('companion-receiving-line-1')).toBeInTheDocument()
    expect(await screen.findByTestId('companion-receiving-save-line-1')).toBeInTheDocument()
  })

  it('updates line quantity and posts receiving through companion API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { updateCompanionFieldReceivingLine, postCompanionFieldReceiving } =
      await import('../api/client')

    render(
      <QueryClientProvider client={client}>
        <FieldTaskReceivingPanel accessToken="test-token" task={receivingTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('companion-receiving-line-qty-1')
    fireEvent.change(screen.getByTestId('companion-receiving-line-qty-1'), {
      target: { value: '4' },
    })
    fireEvent.click(screen.getByTestId('companion-receiving-save-line-1'))

    await waitFor(() => {
      expect(updateCompanionFieldReceivingLine).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({
          taskKey: receivingTask.taskKey,
          lineId: receivingDetail.lines[0]!.lineId,
          quantityReceived: 4,
        }),
      )
    })

    fireEvent.click(screen.getByTestId('companion-receiving-post'))

    await waitFor(() => {
      expect(postCompanionFieldReceiving).toHaveBeenCalledWith('test-token', {
        taskKey: receivingTask.taskKey,
      })
    })

    expect(await screen.findByTestId('companion-receiving-success')).toBeInTheDocument()
  })
})
