import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { TransportationDemandsPanel } from './TransportationDemandsPanel'

vi.mock('@stl/shared-ui', () => ({
  ApiErrorCallout: ({ message }: { message: string }) => <div>{message}</div>,
  getErrorMessage: (error: unknown, fallback: string) =>
    error instanceof Error ? error.message : fallback,
  ReferenceProviderClient: vi.fn(),
  ReferencePicker: ({ placeholder }: { placeholder?: string }) => (
    <div data-testid="reference-picker">{placeholder ?? 'Reference picker'}</div>
  ),
}))

vi.mock('../api/client', () => ({
  createDocumentPacket: vi.fn(),
  createDriverCapacitySnapshot: vi.fn(),
  createFinancePacketContribution: vi.fn(),
  createFreightClaim: vi.fn(),
  createFreightRating: vi.fn(),
  createPlanningScenario: vi.fn(),
  createTender: vi.fn(),
  createTransportationDemand: vi.fn(),
  createVisibilityEvent: vi.fn(),
  createYardEvent: vi.fn(),
  listCollaborationSubmissions: vi.fn(),
  listDocumentPackets: vi.fn(),
  listDriverCapacitySnapshots: vi.fn(),
  listFinancePacketContributions: vi.fn(),
  listFreightClaims: vi.fn(),
  listFreightRatings: vi.fn(),
  listPlanningScenarios: vi.fn(),
  listTenders: vi.fn(),
  listTransportationDemands: vi.fn(),
  listVisibilityEvents: vi.fn(),
  listYardEvents: vi.fn(),
  updateTenderStatus: vi.fn(),
  updateTransportationDemandStatus: vi.fn(),
}))

import * as client from '../api/client'

const baseDemand = {
  transportationDemandId: 'demand-1',
  demandNumber: 'TD-1001',
  title: 'Move ORD-1001',
  description: '',
  status: 'ready_for_planning',
  sourceProduct: 'ordarr',
  sourceObjectType: 'order',
  sourceObjectId: 'ORD-1001',
  sourceObjectNumber: 'ORD-1001',
  originLocationRef: 'North DC',
  destinationLocationRef: 'South DC',
  requestedPickupStartAt: null,
  requestedPickupEndAt: null,
  requestedDeliveryStartAt: null,
  requestedDeliveryEndAt: null,
  promisedPickupStartAt: null,
  promisedPickupEndAt: null,
  promisedDeliveryStartAt: null,
  promisedDeliveryEndAt: null,
  scheduledPickupStartAt: null,
  scheduledPickupEndAt: null,
  scheduledDeliveryStartAt: null,
  scheduledDeliveryEndAt: null,
  transportMode: 'truckload',
  serviceLevel: 'expedited',
  equipmentRequirement: 'reefer',
  handlingRequirements: ['temperature_control'],
  customerRefs: ['customarr:customer:alpha'],
  orderRefs: ['ordarr:order:ORD-1001'],
  vendorRefs: ['supplyarr:carrier:carrier-a'],
  requirementRefs: ['compliancecore:requirement:temperature'],
  planningStatus: 'scenario_created',
  tenderStatus: 'tendered',
  ratingStatus: 'estimated',
  visibilityStatus: 'in_transit',
  freshnessState: 'current',
  tripId: null,
  routeId: null,
  dispatchPlanId: null,
  createdByUserId: 'user-1',
  createdAt: '2026-06-17T12:00:00Z',
  updatedAt: '2026-06-17T12:00:00Z',
  canceledAt: null,
  cancelReason: null,
  lines: [
    {
      demandLineId: 'line-1',
      lineNumber: 1,
      sourceProduct: 'loadarr',
      sourceObjectRef: 'loadarr:load:ORD-1001',
      descriptionSnapshot: 'Palletized freight',
      quantitySnapshot: 12,
      unitOfMeasure: 'pallet',
      weightSnapshot: null,
      volumeSnapshot: null,
      palletCountSnapshot: null,
      handlingRequirementSnapshot: 'temperature_control',
    },
  ],
  requirements: [
    {
      requirementId: 'req-1',
      requirementType: 'temperature_check',
      sourceProduct: 'compliancecore',
      sourceRequirementRef: 'temperature_check',
      required: true,
      status: 'satisfied',
      evidenceRefs: [],
    },
  ],
  sourceRefs: [
    {
      sourceRefId: 'source-1',
      sourceProduct: 'ordarr',
      sourceObjectType: 'order',
      sourceObjectId: 'ORD-1001',
      sourceObjectNumber: 'ORD-1001',
      displayNameSnapshot: 'Order ORD-1001',
      statusSnapshot: 'released',
      snapshotAt: '2026-06-17T12:00:00Z',
      freshnessState: 'current',
    },
  ],
}

function setupMocks() {
  vi.mocked(client.listTransportationDemands).mockResolvedValue([baseDemand])
  vi.mocked(client.listTenders).mockResolvedValue([
    {
      tenderId: 'tender-1',
      transportationDemandId: 'demand-1',
      tenderNumber: 'TND-1001',
      status: 'accepted',
      routingGuideSequence: 1,
      carrierSupplierRef: 'supplyarr:carrier:carrier-a',
      carrierSnapshotJson: '{}',
      tenderMethod: 'portal',
      expiresAt: null,
      sentAt: null,
      respondedAt: '2026-06-17T12:30:00Z',
      declineReason: null,
      counterSummary: null,
      proposedAlternative: null,
      createdAt: '2026-06-17T12:00:00Z',
      updatedAt: '2026-06-17T12:30:00Z',
    },
  ])
  vi.mocked(client.listFreightRatings).mockResolvedValue([
    {
      freightRatingId: 'rating-1',
      transportationDemandId: 'demand-1',
      tripId: null,
      ratingNumber: 'FRT-1001',
      status: 'estimated',
      buyRateEstimate: 900,
      sellRateEstimate: 1250,
      plannedFreightCost: 900,
      actualFreightCost: null,
      currencyCode: 'USD',
      rateSourceSnapshot: 'spot',
      fuelSurcharge: null,
      accessorialTotal: 0,
      varianceAmount: null,
      varianceReason: null,
      allocationSnapshotJson: '{}',
      auditStatus: 'not_reviewed',
      createdAt: '2026-06-17T12:00:00Z',
      updatedAt: '2026-06-17T12:00:00Z',
    },
  ])
  vi.mocked(client.listVisibilityEvents).mockResolvedValue([
    {
      visibilityEventId: 'vis-1',
      transportationDemandId: 'demand-1',
      tripId: null,
      stopId: null,
      eventType: 'in_transit',
      source: 'telematics',
      sourceOccurredAt: '2026-06-17T12:10:00Z',
      receivedAt: '2026-06-17T12:11:00Z',
      normalizedStatus: 'in_transit',
      latitude: null,
      longitude: null,
      eta: null,
      etaConfidence: 'unknown',
      freshnessState: 'current',
      reviewStatus: 'accepted',
      rawExternalRef: 'tele-1',
      summary: 'Departed North DC',
      updatedTrackingState: true,
    },
  ])
  vi.mocked(client.listPlanningScenarios).mockResolvedValue([
    {
      planningScenarioId: 'scenario-1',
      scenarioNumber: 'TPL-1001',
      status: 'suggestions_ready',
      objective: 'service_cost_balance',
      demandRefsJson: '["demand-1"]',
      routeRefsJson: '[]',
      tripRefsJson: '[]',
      hardBlockersJson: '[]',
      warningsJson: '[]',
      serviceRiskEstimate: 0.25,
      costEstimate: 250,
      createdAt: '2026-06-17T12:00:00Z',
      evaluatedAt: '2026-06-17T12:00:00Z',
      suggestions: [],
    },
  ])
  vi.mocked(client.listDriverCapacitySnapshots).mockResolvedValue([])
  vi.mocked(client.listYardEvents).mockResolvedValue([])
  vi.mocked(client.listCollaborationSubmissions).mockResolvedValue([])
  vi.mocked(client.listFreightClaims).mockResolvedValue([])
  vi.mocked(client.listDocumentPackets).mockResolvedValue([
    {
      documentPacketRequestId: 'packet-1',
      transportationDemandId: 'demand-1',
      tripId: null,
      packetType: 'dispatch_packet',
      status: 'requested',
      requiredDocumentTypes: ['bill_of_lading'],
      sourceFactsJson: '{}',
      recordPackageRef: null,
      createdAt: '2026-06-17T12:00:00Z',
      updatedAt: '2026-06-17T12:00:00Z',
    },
  ])
  vi.mocked(client.listFinancePacketContributions).mockResolvedValue([
    {
      financePacketContributionId: 'finance-1',
      contributionNumber: 'FPC-1001',
      transportationDemandId: 'demand-1',
      tripId: null,
      freightRatingId: 'rating-1',
      contributionType: 'freight_operational_snapshot',
      targetProduct: 'ordarr',
      status: 'ready',
      operationalSummary: 'Ready for closeout',
      costSnapshotJson: '{}',
      accessorialRefs: [],
      proofRefs: [],
      documentPacketRefs: ['packet-1'],
      claimRefs: [],
      createdAt: '2026-06-17T12:00:00Z',
      updatedAt: '2026-06-17T12:00:00Z',
      sentAt: null,
      acceptedAt: null,
    },
  ])
}

function renderPanel() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })

  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/transportation-demands?demand=demand-1']}>
        <TransportationDemandsPanel accessToken="token" />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('TransportationDemandsPanel', () => {
  it('renders the TMS demand queue and selected demand refs', async () => {
    setupMocks()
    renderPanel()

    expect(await screen.findAllByText('TD-1001')).toHaveLength(2)
    expect(screen.getAllByText('Move ORD-1001')[0]).toBeInTheDocument()
    expect(screen.getByText(/Planning: scenario created/i)).toBeInTheDocument()
    expect(screen.getByText(/Tender: tendered/i)).toBeInTheDocument()
    expect(screen.getByText('ordarr')).toBeInTheDocument()
  })

  it('shows tender and finance readiness panels from the grouped TMS tabs', async () => {
    setupMocks()
    renderPanel()

    await screen.findAllByText('TD-1001')

    fireEvent.click(screen.getByRole('button', { name: /Tenders/i }))
    expect(await screen.findByText('TND-1001')).toBeInTheDocument()
    expect(screen.getByText('supplyarr:carrier:carrier-a')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Finance packet/i }))
    expect(await screen.findByText('FPC-1001')).toBeInTheDocument()
    expect(screen.getByText('dispatch_packet')).toBeInTheDocument()
  })
})
