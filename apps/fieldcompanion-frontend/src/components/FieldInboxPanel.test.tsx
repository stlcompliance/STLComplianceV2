import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { FieldInboxPanel } from './FieldInboxPanel'
import type { AggregatedFieldInboxResponse, FieldInboxTaskItem } from '../api/types'

const workOrderTask: FieldInboxTaskItem = {
  taskKey: 'maintainarr:work-order:1',
  productKey: 'maintainarr',
  taskType: 'work_order',
  title: 'Replace belt',
  subtitle: 'PMP-100',
  status: 'open',
  priority: 'high',
  dueAt: '2026-05-27T14:00:00.000Z',
  sortAt: '2026-05-27T09:30:00.000Z',
  deepLinkPath: '/work-orders/1',
}

const tripTask: FieldInboxTaskItem = {
  taskKey: 'routarr:trip:1',
  productKey: 'routarr',
  taskType: 'trip',
  title: 'North route',
  subtitle: 'TR-100',
  status: 'assigned',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-25T08:00:00.000Z',
  deepLinkPath: '/trips/1',
}

const blockedTask: FieldInboxTaskItem = {
  taskKey: 'trainarr:assignment:blocked',
  productKey: 'trainarr',
  taskType: 'training_assignment',
  title: 'Blocked certification',
  subtitle: 'Hazmat annual',
  status: 'open',
  priority: 'high',
  dueAt: '2026-05-27T18:00:00.000Z',
  sortAt: '2026-05-27T07:00:00.000Z',
  deepLinkPath: '/assignments/blocked',
  blockedReason: 'Awaiting supervisor approval',
}

const inbox: AggregatedFieldInboxResponse = {
  summary: {
    totalCount: 3,
    blockedCount: 1,
    countByProduct: { maintainarr: 1, routarr: 1, trainarr: 1 },
  },
  items: [workOrderTask, tripTask, blockedTask],
  sources: [
    {
      productKey: 'maintainarr',
      available: true,
      fetched: true,
      errorCode: null,
      errorMessage: null,
      items: [workOrderTask],
    },
    {
      productKey: 'routarr',
      available: true,
      fetched: true,
      errorCode: null,
      errorMessage: null,
      items: [tripTask],
    },
    {
      productKey: 'trainarr',
      available: true,
      fetched: true,
      errorCode: null,
      errorMessage: null,
      items: [blockedTask],
    },
  ],
}

describe('FieldInboxPanel', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-05-27T10:00:00.000Z'))
  })

  afterEach(() => {
    cleanup()
    vi.useRealTimers()
    vi.restoreAllMocks()
  })

  it('renders tasks and filters by product', () => {
    const onFilter = vi.fn()
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <FieldInboxPanel
          inbox={inbox}
          productFilter=""
          onProductFilterChange={onFilter}
          accessToken="test-token"
        />
      </QueryClientProvider>,
    )

    expect(screen.getByText('Replace belt')).toBeInTheDocument()
    expect(screen.getByText('North route')).toBeInTheDocument()
    expect(screen.getByText('Blocked certification')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /MaintainArr \(1\)/ }))
    expect(onFilter).toHaveBeenCalledWith('maintainarr')
  })

  it('surfaces inbox urgency groups and freshness labels', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <FieldInboxPanel
          inbox={inbox}
          productFilter=""
          onProductFilterChange={() => undefined}
          accessToken="test-token"
        />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('fieldcompanion-inbox-urgency-banner')).toHaveTextContent(
      '2 urgent tasks need attention.',
    )
    expect(screen.getByRole('heading', { name: 'Blocked work' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Due soon' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Stale work' })).toBeInTheDocument()
    expect(screen.getAllByTestId('fieldcompanion-field-inbox-task')[0]).toHaveTextContent('Blocked certification')
    expect(screen.getByText('Due in 4h 0m')).toBeInTheDocument()
    expect(screen.getByText('Updated 2d 2h ago')).toBeInTheDocument()
  })

  it('prefers API deepLinkUrl when provided', () => {
    const trainarrTask: FieldInboxTaskItem = {
      taskKey: 'trainarr:assignment:2',
      productKey: 'trainarr',
      taskType: 'training_assignment',
      title: 'Hazmat annual',
      subtitle: 'manual',
      status: 'in_progress',
      priority: null,
      dueAt: null,
      sortAt: '2026-05-27T09:50:00.000Z',
      deepLinkPath: '/assignments/00000000-0000-0000-0000-000000000002/evidence',
      deepLinkUrl:
        'https://trainarr.example/assignments/00000000-0000-0000-0000-000000000002/evidence',
    }

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldInboxPanel
          inbox={{
            summary: { totalCount: 1, blockedCount: 0, countByProduct: { trainarr: 1 } },
            items: [trainarrTask],
            sources: [
              {
                productKey: 'trainarr',
                available: true,
                fetched: true,
                errorCode: null,
                errorMessage: null,
                items: [trainarrTask],
              },
            ],
          }}
          productFilter=""
          onProductFilterChange={() => undefined}
          accessToken="test-token"
        />
      </QueryClientProvider>,
    )

    const link = screen.getByRole('link', { name: /Open in TrainArr/i })
    expect(link).toHaveAttribute(
      'href',
      'https://trainarr.example/assignments/00000000-0000-0000-0000-000000000002/evidence',
    )
  })

  it('shows plain blocked reasons and per-product inbox load failures', () => {
    const blockedTrip: FieldInboxTaskItem = {
      ...tripTask,
      blockedReason: 'Pre-trip DVIR required',
    }

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <FieldInboxPanel
          inbox={{
            ...inbox,
            summary: { ...inbox.summary, blockedCount: 1 },
            items: [blockedTrip],
            sources: [
              {
                productKey: 'routarr',
                available: true,
                fetched: false,
                errorCode: 'upstream_unreachable',
                errorMessage: null,
                items: [],
              },
            ],
          }}
          productFilter=""
          onProductFilterChange={() => undefined}
          accessToken="test-token"
        />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('fieldcompanion-inbox-source-errors')).toHaveTextContent('RoutArr')
    expect(screen.getByTestId('fieldcompanion-task-blocked-reason')).toHaveTextContent('Pre-trip DVIR')
  })
})
