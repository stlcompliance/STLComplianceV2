import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'

import { RecallsSection } from './RecallsSection'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

function buildState(): MaintainArrWorkspaceState {
  return {
    accessToken: 'token-123',
  } as unknown as MaintainArrWorkspaceState
}

function mockRecallFetches() {
  return vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = String(input)

    if (url === '/api/v1/recalls/dashboard') {
      return new Response(JSON.stringify({
        generatedAt: '2026-06-10T12:00:00Z',
        verifiedOpenRecallCount: 1,
        potentialMatchCount: 3,
        parkItWarningCount: 1,
        parkOutsideWarningCount: 0,
        workOrdersCreatedCount: 4,
        completedVerifiedThisMonthCount: 2,
        overdueReviewCount: 1,
        assetsNeverCheckedCount: 0,
        attentionItems: [],
      }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }

    if (url === '/api/v1/recalls/providers') {
      return new Response(JSON.stringify([
        {
          providerKey: 'nhtsa',
          displayName: 'NHTSA',
          description: 'Government recall data',
          sourceOfTruth: 'NHTSA API',
          status: 'active',
          supportsVehicleSearch: true,
          supportsCampaignSearch: true,
          supportsManualCampaigns: false,
          lastCheckedAt: '2026-06-10T11:00:00Z',
          lastSuccessfulAt: '2026-06-10T11:30:00Z',
          lastError: null,
        },
      ]), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }

    if (url === '/api/v1/recalls/providers/health') {
      return new Response(JSON.stringify([
        {
          providerKey: 'nhtsa',
          status: 'healthy',
          message: 'Provider reachable',
          checkedAt: '2026-06-10T11:30:00Z',
          latencyMs: 120,
        },
      ]), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }

    if (url === '/api/v1/recalls/campaigns?limit=50') {
      return new Response(JSON.stringify([
        {
          campaignId: 'campaign-1',
          sourceProvider: 'nhtsa',
          sourceType: 'api',
          sourceProviderRecordId: 'NHTSA-001',
          nhtsaCampaignNumber: '24V-123',
          nhtsaActionNumber: null,
          manufacturerCampaignNumber: null,
          campaignTitle: 'Air bag inflator replacement',
          manufacturer: 'Ford',
          component: 'Air bags',
          reportReceivedDate: '2026-06-01T00:00:00Z',
          campaignStartDate: '2026-06-01T00:00:00Z',
          campaignEndDate: null,
          campaignStatus: 'open',
          potentialUnitsAffected: 12,
          summary: 'Air bag recall',
          consequence: 'Deployment may fail.',
          remedy: 'Replace inflator module.',
          notes: '',
          parkIt: true,
          parkOutside: false,
          overTheAirUpdate: false,
          recallType: 'safety',
          sourceUrl: null,
          fetchedAt: '2026-06-10T12:00:00Z',
          applicability: [],
          assetCaseCount: 5,
          openCaseCount: 2,
          verifiedOpenCaseCount: 1,
          createdAt: '2026-06-01T00:00:00Z',
          updatedAt: '2026-06-10T12:00:00Z',
        },
        {
          campaignId: 'campaign-2',
          sourceProvider: 'nhtsa',
          sourceType: 'api',
          sourceProviderRecordId: 'NHTSA-002',
          nhtsaCampaignNumber: '25V-456',
          nhtsaActionNumber: null,
          manufacturerCampaignNumber: null,
          campaignTitle: 'Brake hose replacement',
          manufacturer: 'GM',
          component: 'Brakes',
          reportReceivedDate: '2026-05-20T00:00:00Z',
          campaignStartDate: '2026-05-20T00:00:00Z',
          campaignEndDate: null,
          campaignStatus: 'open',
          potentialUnitsAffected: 8,
          summary: 'Brake hose recall',
          consequence: 'Loss of braking performance.',
          remedy: 'Replace brake hose assembly.',
          notes: '',
          parkIt: false,
          parkOutside: true,
          overTheAirUpdate: false,
          recallType: 'safety',
          sourceUrl: null,
          fetchedAt: '2026-06-09T12:00:00Z',
          applicability: [],
          assetCaseCount: 0,
          openCaseCount: 0,
          verifiedOpenCaseCount: 0,
          createdAt: '2026-05-20T00:00:00Z',
          updatedAt: '2026-06-09T12:00:00Z',
        },
        {
          campaignId: 'campaign-3',
          sourceProvider: 'nhtsa',
          sourceType: 'api',
          sourceProviderRecordId: 'NHTSA-003',
          nhtsaCampaignNumber: '24V-789',
          nhtsaActionNumber: null,
          manufacturerCampaignNumber: null,
          campaignTitle: 'Steering column inspection',
          manufacturer: 'Volvo',
          component: 'Steering',
          reportReceivedDate: '2026-05-10T00:00:00Z',
          campaignStartDate: '2026-05-10T00:00:00Z',
          campaignEndDate: null,
          campaignStatus: 'open',
          potentialUnitsAffected: 4,
          summary: 'Steering inspection campaign',
          consequence: 'Steering play may increase.',
          remedy: 'Inspect and tighten steering column.',
          notes: '',
          parkIt: false,
          parkOutside: false,
          overTheAirUpdate: false,
          recallType: 'safety',
          sourceUrl: null,
          fetchedAt: '2026-06-08T12:00:00Z',
          applicability: [],
          assetCaseCount: 2,
          openCaseCount: 0,
          verifiedOpenCaseCount: 0,
          createdAt: '2026-05-10T00:00:00Z',
          updatedAt: '2026-06-08T12:00:00Z',
        },
      ]), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })
    }

    throw new Error(`Unexpected fetch: ${url}`)
  })
}

describe('RecallsSection', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('shows campaign coverage and the highest-risk campaigns', async () => {
    mockRecallFetches()
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/recalls']}>
          <RecallsSection state={buildState()} />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Campaign coverage')).toBeInTheDocument()
    expect(await screen.findByText(/67% covered/i)).toBeInTheDocument()
    expect(screen.getByText(/2 of 3 tracked campaigns have asset cases/i)).toBeInTheDocument()
    expect(screen.getByText('Coverage watchlist')).toBeInTheDocument()
    expect(screen.getAllByText('24V-123').length).toBeGreaterThan(0)
  })
})
