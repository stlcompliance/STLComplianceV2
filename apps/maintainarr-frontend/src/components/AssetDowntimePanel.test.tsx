import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AssetDowntimePanel } from './AssetDowntimePanel'

vi.mock('../api/client', () => ({
  getFleetAvailability: vi.fn().mockResolvedValue({
    periodStart: '2026-04-29T00:00:00Z',
    periodEnd: '2026-05-29T00:00:00Z',
    assetCount: 2,
    totalHours: 1440,
    downtimeHours: 24,
    availabilityPercent: 98.3,
    plannedDowntimeHours: 8,
    unplannedDowntimeHours: 16,
    activeDowntimeEventCount: 1,
    computedAt: '2026-05-29T00:00:00Z',
    isMaterialized: true,
  }),
  listDowntimeEvents: vi.fn().mockResolvedValue([
    {
      eventId: 'event-1',
      assetId: 'asset-1',
      assetTag: 'TRK-001',
      assetName: 'Truck 1',
      source: 'automatic_status',
      reason: 'restricted_use',
      isPlanned: false,
      startedAt: '2026-05-28T12:00:00Z',
      endedAt: null,
      statusTrigger: 'readiness:not_ready',
      workOrderId: null,
      defectId: null,
      notes: null,
      isActive: true,
      createdAt: '2026-05-28T12:00:00Z',
      updatedAt: '2026-05-28T12:00:00Z',
    },
  ]),
  createManualDowntimeEvent: vi.fn(),
  closeDowntimeEvent: vi.fn(),
}))

const assets = [
  {
    id: 'asset-1',
    assetTag: 'TRK-001',
    name: 'Truck 1',
    description: '',
    lifecycleStatus: 'active',
    siteRef: null,
    assetTypeId: 'type-1',
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z',
  },
]

function renderPanel(canManage = true, initialEntry = '/downtime') {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <QueryClientProvider client={queryClient}>
        <AssetDowntimePanel
          accessToken="token"
          canRead
          canManage={canManage}
          assets={assets}
        />
      </QueryClientProvider>
    </MemoryRouter>,
  )
}

describe('AssetDowntimePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders fleet availability and downtime events', async () => {
    renderPanel()
    expect(await screen.findByTestId('maintainarr-downtime-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('maintainarr-fleet-availability-summary')).toBeInTheDocument()
    expect(await screen.findByText('98.3%')).toBeInTheDocument()
    expect(await screen.findByText('restricted_use')).toBeInTheDocument()
  })

  it('hides manual form when user cannot manage downtime', async () => {
    renderPanel(false)
    expect(await screen.findByTestId('maintainarr-downtime-panel')).toBeInTheDocument()
    expect(screen.queryByTestId('maintainarr-manual-downtime-form')).not.toBeInTheDocument()
  })

  it('shows deep link banner and highlights linked event', async () => {
    renderPanel(
      true,
      '/downtime?assetId=asset-1&workOrderId=wo-1&eventId=event-1',
    )
    expect(await screen.findByTestId('maintainarr-downtime-deep-link-banner')).toBeInTheDocument()
    expect(await screen.findByTestId('maintainarr-downtime-event-event-1')).toHaveAttribute(
      'data-highlighted',
      'true',
    )
  })
})
