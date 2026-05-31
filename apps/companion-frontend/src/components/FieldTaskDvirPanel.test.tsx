import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, cleanup } from '@testing-library/react'
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'
import { FieldTaskDvirPanel } from './FieldTaskDvirPanel'
import type { FieldInboxTaskItem } from '../api/types'

const tripTask: FieldInboxTaskItem = {
  taskKey: 'routarr:trip:11111111-1111-1111-1111-111111111111',
  productKey: 'routarr',
  taskType: 'trip',
  title: 'North route',
  subtitle: 'TR-100',
  status: 'assigned',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/trips/11111111-1111-1111-1111-111111111111',
  blockedReason: 'Pre-trip DVIR required',
}

vi.mock('../api/client', () => ({
  validateCompanionFieldTask: vi.fn(async () => ({
    allowed: true,
    reasonCode: null,
    reasonMessage: null,
    taskKey: tripTask.taskKey,
    productKey: 'routarr',
    title: tripTask.title,
    blockedReason: tripTask.blockedReason ?? null,
  })),
  submitCompanionFieldDvir: vi.fn(async () => ({
    taskKey: tripTask.taskKey,
    productKey: 'routarr',
    dvirId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
    tripId: '11111111-1111-1111-1111-111111111111',
    phase: 'pre_trip',
    result: 'pass',
    odometerReading: 1000,
    defectNotes: '',
    submittedAt: '2026-05-28T12:00:00.000Z',
  })),
}))

describe('FieldTaskDvirPanel', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('renders DVIR form for routarr trip tasks', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldTaskDvirPanel accessToken="test-token" task={tripTask} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('companion-field-dvir-panel')).toBeInTheDocument()
    expect(screen.getByTestId('companion-dvir-phase-pre_trip')).toBeInTheDocument()
    expect(screen.getByTestId('companion-dvir-submit')).toBeInTheDocument()
  })

  it('submits pre-trip DVIR through companion API', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitCompanionFieldDvir } = await import('../api/client')

    render(
      <QueryClientProvider client={client}>
        <FieldTaskDvirPanel accessToken="test-token" task={tripTask} />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByTestId('companion-dvir-odometer'), { target: { value: '1000' } })
    fireEvent.click(screen.getByTestId('companion-dvir-submit'))

    await waitFor(() => {
      expect(submitCompanionFieldDvir).toHaveBeenCalledWith(
        'test-token',
        expect.objectContaining({
          taskKey: tripTask.taskKey,
          phase: 'pre_trip',
          result: 'pass',
          odometerReading: 1000,
        }),
      )
    })

    expect(await screen.findByTestId('companion-dvir-success')).toBeInTheDocument()
  })

  it('shows submission failure in shared callout', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { submitCompanionFieldDvir } = await import('../api/client')
    vi.mocked(submitCompanionFieldDvir).mockRejectedValueOnce(new Error('dvir submit failed'))

    render(
      <QueryClientProvider client={client}>
        <FieldTaskDvirPanel accessToken="test-token" task={tripTask} />
      </QueryClientProvider>,
    )

    fireEvent.click(screen.getByTestId('companion-dvir-submit'))

    expect(await screen.findByText('dvir submit failed')).toBeInTheDocument()
    expect(screen.getByTestId('companion-dvir-error')).toBeInTheDocument()
  })
})
