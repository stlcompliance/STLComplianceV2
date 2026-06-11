import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskReceivingPanel } from './FieldTaskReceivingPanel'
import type { FieldInboxTaskItem } from '../api/types'
import * as client from '../api/client'

const loadArrReceivingTask: FieldInboxTaskItem = {
  taskKey: 'loadarr:receiving:11111111-1111-1111-1111-111111111111',
  productKey: 'loadarr',
  taskType: 'receiving',
  title: 'RCV-24018',
  subtitle: 'PO-10492',
  status: 'open',
  priority: 'high',
  dueAt: null,
  sortAt: '2026-06-02T20:10:00.000Z',
  deepLinkPath:
    '/work/receiving/recv-24018?taskKey=loadarr:receiving:11111111-1111-1111-1111-111111111111',
}

const loadArrReceivingDetail = {
  taskKey: loadArrReceivingTask.taskKey,
  productKey: 'loadarr',
  receivingReceiptId: 'recv-24018',
  receiptKey: 'RCV-24018',
  status: 'open',
  purchaseOrderKey: 'PO-10492',
  binKey: 'loc-dock-01',
  binName: 'Receiving Dock 1',
  locationName: 'STL North Yard',
  notes: 'Midwest Fleet Supply',
  lines: [
    {
      lineId: 'line-24018-1',
      lineNumber: 1,
      partKey: 'SUP-VALVE-KIT-A',
      partDisplayName: 'Valve repair kit A',
      quantityExpected: 38,
      quantityReceived: 38,
      quantityOrdered: 38,
      quantityRemainingOnOrder: 0,
      openExceptionCount: 0,
    },
  ],
}

vi.mock('../api/client', () => ({
  validateFieldCompanionFieldTask: vi.fn(async (_accessToken: string, input: { taskKey: string; productKey?: string | null }) => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: input.taskKey,
    productKey: input.productKey ?? 'loadarr',
    title: loadArrReceivingTask.title,
    blockedReason: null,
  })),
  getFieldCompanionFieldReceivingDetail: vi.fn(async () => loadArrReceivingDetail),
  postFieldCompanionFieldReceiving: vi.fn(async () => ({
    taskKey: loadArrReceivingTask.taskKey,
    productKey: 'loadarr',
    receivingReceiptId: loadArrReceivingDetail.receivingReceiptId,
    status: 'completed',
    postedAt: '2026-06-02T20:15:00.000Z',
  })),
}))

describe('FieldTaskReceivingPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('renders loadarr receiving detail without inline line editing', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskReceivingPanel accessToken="test-token" task={loadArrReceivingTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('fieldcompanion-field-receiving-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('fieldcompanion-receiving-line-1')).toBeInTheDocument()
    expect(
      await screen.findByText(
        'Line quantity edits stay in LoadArr. Use the owner-side session when counts need to change.',
      ),
    ).toBeInTheDocument()
    expect(screen.queryByTestId('fieldcompanion-receiving-save-line-1')).not.toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-receiving-post')).toHaveTextContent(
      'Complete receiving',
    )
  })

  it('posts loadarr receiving through the fieldcompanion API', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { postFieldCompanionFieldReceiving } = await import('../api/client')

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskReceivingPanel accessToken="test-token" task={loadArrReceivingTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-receiving-post')
    fireEvent.click(screen.getByTestId('fieldcompanion-receiving-post'))

    await waitFor(() => {
      expect(postFieldCompanionFieldReceiving).toHaveBeenCalledWith('test-token', {
        taskKey: loadArrReceivingTask.taskKey,
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
        <FieldTaskReceivingPanel accessToken="test-token" task={loadArrReceivingTask} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('receiving detail unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry receiving detail' })).toBeInTheDocument()
  })

  it('renders completion failure in shared callout', async () => {
    vi.mocked(client.postFieldCompanionFieldReceiving).mockRejectedValueOnce(
      new Error('receiving completion failed'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <FieldTaskReceivingPanel accessToken="test-token" task={loadArrReceivingTask} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('fieldcompanion-receiving-post')
    fireEvent.click(screen.getByTestId('fieldcompanion-receiving-post'))

    expect(await screen.findByText('receiving completion failed')).toBeInTheDocument()
    expect(screen.getByTestId('fieldcompanion-receiving-error')).toBeInTheDocument()
  })
})
