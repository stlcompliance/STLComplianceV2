import { useMemo, useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useSearchParams } from 'react-router-dom'
import {
  Activity,
  BadgeDollarSign,
  CheckCircle2,
  ClipboardList,
  FileText,
  GitBranch,
  MapPinned,
  PackageCheck,
  Plus,
  RefreshCw,
  Send,
  Truck,
  Users,
  Warehouse,
} from 'lucide-react'
import {
  ApiErrorCallout,
  ControlledSelect,
  ReferencePicker,
  ReferenceProviderClient,
  getErrorMessage,
  type CrossProductReference,
  type PickerOption,
} from '@stl/shared-ui'

import {
  createDocumentPacket,
  createDriverCapacitySnapshot,
  createFinancePacketContribution,
  createFreightClaim,
  createFreightRating,
  createPlanningScenario,
  createTender,
  createTransportationDemand,
  createVisibilityEvent,
  createYardEvent,
  listCollaborationSubmissions,
  listDocumentPackets,
  listDriverCapacitySnapshots,
  listFinancePacketContributions,
  listFreightClaims,
  listFreightRatings,
  listPlanningScenarios,
  listTenders,
  listTransportationDemands,
  listVisibilityEvents,
  listYardEvents,
  updateTenderStatus,
  updateTransportationDemandStatus,
} from '../api/client'
import type {
  CarrierTenderResponse,
  DocumentPacketResponse,
  DriverCapacitySnapshotResponse,
  FinancePacketContributionResponse,
  FreightClaimResponse,
  FreightRatingResponse,
  PlanningScenarioResponse,
  TransportationDemandResponse,
  VisibilityEventResponse,
  YardEventResponse,
} from '../api/types'

type Props = {
  accessToken: string
}

type TabKey =
  | 'queue'
  | 'planner'
  | 'consolidation'
  | 'tenders'
  | 'rating'
  | 'visibility'
  | 'capacity'
  | 'yard'
  | 'collaboration'
  | 'claims'
  | 'appointments'
  | 'finance'

const tabs: Array<{ key: TabKey; label: string; icon: typeof ClipboardList }> = [
  { key: 'queue', label: 'Demand queue', icon: ClipboardList },
  { key: 'planner', label: 'Planner', icon: GitBranch },
  { key: 'consolidation', label: 'Consolidation', icon: PackageCheck },
  { key: 'tenders', label: 'Tenders', icon: Send },
  { key: 'rating', label: 'Rating', icon: BadgeDollarSign },
  { key: 'visibility', label: 'Visibility', icon: Activity },
  { key: 'capacity', label: 'HOS/capacity', icon: Truck },
  { key: 'yard', label: 'Yard/trailer', icon: Warehouse },
  { key: 'collaboration', label: 'Collaboration', icon: Users },
  { key: 'claims', label: 'Claims', icon: FileText },
  { key: 'appointments', label: 'Appointments', icon: MapPinned },
  { key: 'finance', label: 'Finance packet', icon: CheckCircle2 },
]

const statusOptions = [
  'draft',
  'ready_for_planning',
  'planning',
  'planned',
  'assigned',
  'tender_required',
  'tendered',
  'accepted',
  'dispatched',
  'in_transit',
  'delivered',
  'closed',
  'canceled',
  'blocked',
]

const transportModeOptions: PickerOption[] = [
  { value: 'private_fleet', label: 'Private fleet' },
  { value: 'dedicated_carrier', label: 'Dedicated carrier' },
  { value: 'truckload', label: 'Truckload' },
  { value: 'ltl', label: 'Less-than-truckload (LTL)' },
  { value: 'parcel', label: 'Parcel' },
  { value: 'intermodal', label: 'Intermodal' },
  { value: 'rail', label: 'Rail' },
  { value: 'drayage', label: 'Drayage' },
  { value: 'ocean', label: 'Ocean' },
  { value: 'air', label: 'Air' },
  { value: 'courier', label: 'Courier' },
  { value: 'shuttle', label: 'Shuttle' },
  { value: 'internal_transfer', label: 'Internal transfer' },
]

const serviceLevelOptions: PickerOption[] = [
  { value: 'standard', label: 'Standard' },
  { value: 'expedited', label: 'Expedited' },
  { value: 'economy', label: 'Economy' },
  { value: 'guaranteed', label: 'Guaranteed' },
  { value: 'same_day', label: 'Same day' },
  { value: 'next_day', label: 'Next day' },
  { value: 'hotshot', label: 'Hotshot' },
]

const equipmentRequirementOptions: PickerOption[] = [
  { value: 'dry_van', label: 'Dry van trailer' },
  { value: 'reefer', label: 'Reefer trailer' },
  { value: 'flatbed', label: 'Flatbed trailer' },
  { value: 'step_deck', label: 'Step deck trailer' },
  { value: 'lowboy', label: 'Lowboy trailer' },
  { value: 'tanker', label: 'Tanker trailer' },
  { value: 'chassis', label: 'Container chassis' },
  { value: 'box_truck', label: 'Box truck' },
  { value: 'cargo_van', label: 'Cargo van' },
  { value: 'none_required', label: 'No specific equipment' },
]

function compactStatus(status: string) {
  return status.replaceAll('_', ' ')
}

function optionLabel(options: PickerOption[], value: string) {
  return options.find((option) => option.value === value)?.label ?? compactStatus(value)
}

function formatTimestamp(value: string | null | undefined) {
  if (!value) return '—'
  try {
    return new Date(value).toLocaleString()
  } catch {
    return value
  }
}

function splitRefs(value: string) {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

function serializeReferenceSnapshot(value: CrossProductReference | null): string | null {
  return value ? JSON.stringify(value) : null
}

function formatReferenceSnapshot(value: string | null | undefined): string {
  if (!value) return 'unassigned'
  try {
    const parsed = JSON.parse(value) as Partial<CrossProductReference>
    if (typeof parsed.displayLabelSnapshot === 'string' && parsed.displayLabelSnapshot.trim()) {
      return [
        parsed.displayLabelSnapshot,
        parsed.secondaryLabelSnapshot,
        parsed.statusSnapshot,
      ]
        .filter(Boolean)
        .join(' / ')
    }
  } catch {
    return value
  }

  return value
}

function parseNumber(value: string): number | null {
  if (!value.trim()) return null
  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : null
}

function SmallStat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-500">{label}</p>
      <p className="mt-1 text-base font-semibold text-slate-100">{value}</p>
    </div>
  )
}

function EmptyState({ label }: { label: string }) {
  return <p className="rounded border border-dashed border-slate-700 px-3 py-4 text-sm text-slate-500">{label}</p>
}

function DataError({
  error,
  label,
  onRetry,
}: {
  error: unknown
  label: string
  onRetry: () => void
}) {
  return (
    <ApiErrorCallout
      className="mt-3"
      message={getErrorMessage(error, label)}
      onRetry={onRetry}
      retryLabel="Retry"
    />
  )
}

function SelectedDemandSummary({ demand }: { demand: TransportationDemandResponse | null }) {
  if (!demand) {
    return (
      <section className="rounded border border-slate-700 bg-slate-900 p-4">
        <h3 className="text-sm font-semibold text-slate-100">Selected demand</h3>
        <p className="mt-2 text-sm text-slate-500">No transportation demand selected.</p>
      </section>
    )
  }

  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-semibold text-slate-100">{demand.demandNumber}</h3>
          <p className="mt-1 text-sm text-slate-300">{demand.title}</p>
          <p className="mt-1 text-xs text-slate-500">
            {formatReferenceSnapshot(demand.originLocationRef)} to {formatReferenceSnapshot(demand.destinationLocationRef)}
          </p>
        </div>
        <span className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">
          {compactStatus(demand.status)}
        </span>
      </div>
      <div className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2 lg:grid-cols-4">
        <p>Planning: {compactStatus(demand.planningStatus)}</p>
        <p>Tender: {compactStatus(demand.tenderStatus)}</p>
        <p>Rating: {compactStatus(demand.ratingStatus)}</p>
        <p>Visibility: {compactStatus(demand.visibilityStatus)}</p>
      </div>
      <div className="mt-3 flex flex-wrap gap-2 text-xs text-slate-400">
        <span className="rounded bg-slate-950 px-2 py-1">{demand.sourceProduct}</span>
        <span className="rounded bg-slate-950 px-2 py-1">{demand.freshnessState}</span>
        <span className="rounded bg-slate-950 px-2 py-1">{optionLabel(transportModeOptions, demand.transportMode)}</span>
        <span className="rounded bg-slate-950 px-2 py-1">{optionLabel(serviceLevelOptions, demand.serviceLevel)}</span>
        <span className="rounded bg-slate-950 px-2 py-1">{optionLabel(equipmentRequirementOptions, demand.equipmentRequirement)}</span>
      </div>
    </section>
  )
}

export function TransportationDemandsPanel({ accessToken }: Props) {
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const [activeTab, setActiveTab] = useState<TabKey>('queue')
  const [statusFilter, setStatusFilter] = useState('')
  const [selectedDemandId, setSelectedDemandId] = useState(searchParams.get('demand') ?? '')
  const [title, setTitle] = useState('')
  const [originLocationReference, setOriginLocationReference] = useState<CrossProductReference | null>(null)
  const [destinationLocationReference, setDestinationLocationReference] = useState<CrossProductReference | null>(null)
  const [sourceObjectNumber, setSourceObjectNumber] = useState('')
  const [transportMode, setTransportMode] = useState('truckload')
  const [serviceLevel, setServiceLevel] = useState('standard')
  const [equipmentRequirement, setEquipmentRequirement] = useState('dry_van')
  const [customerReference, setCustomerReference] = useState<CrossProductReference | null>(null)
  const [orderRefs, setOrderRefs] = useState('')
  const [newDemandStatus, setNewDemandStatus] = useState('ready_for_planning')
  const [carrierReference, setCarrierReference] = useState<CrossProductReference | null>(null)
  const [tenderStatus, setTenderStatus] = useState('accepted')
  const [buyRateEstimate, setBuyRateEstimate] = useState('')
  const [sellRateEstimate, setSellRateEstimate] = useState('')
  const [actualFreightCost, setActualFreightCost] = useState('')
  const [visibilityStatus, setVisibilityStatus] = useState('in_transit')
  const [visibilitySummary, setVisibilitySummary] = useState('')
  const [capacityPersonId, setCapacityPersonId] = useState('')
  const [hosRemainingMinutes, setHosRemainingMinutes] = useState('')
  const [yardEventType, setYardEventType] = useState('gate_in')
  const [trailerAssetRef, setTrailerAssetRef] = useState('')
  const [claimReason, setClaimReason] = useState('')
  const [documentPacketType, setDocumentPacketType] = useState('dispatch_packet')
  const [financeTargetProduct, setFinanceTargetProduct] = useState('ordarr')

  const demandsQuery = useQuery({
    queryKey: ['routarr-transportation-demands', accessToken, statusFilter],
    queryFn: () => listTransportationDemands(accessToken, { status: statusFilter || undefined }),
  })

  const selectedDemand = useMemo(() => {
    const demands = demandsQuery.data ?? []
    if (!selectedDemandId && demands.length > 0) return demands[0]
    return demands.find((demand) => demand.transportationDemandId === selectedDemandId) ?? null
  }, [demandsQuery.data, selectedDemandId])

  const selectedDemandFilter = selectedDemand?.transportationDemandId
  const staffReferenceClient = useMemo(
    () =>
      new ReferenceProviderClient({
        baseUrl: import.meta.env.VITE_STAFFARR_API_BASE ?? import.meta.env.VITE_ROUTARR_API_BASE ?? '',
        getHeaders: () => ({ Authorization: `Bearer ${accessToken}` }),
      }),
    [accessToken],
  )
  const customReferenceClient = useMemo(
    () =>
      new ReferenceProviderClient({
        baseUrl: import.meta.env.VITE_CUSTOMARR_API_BASE ?? import.meta.env.VITE_ROUTARR_API_BASE ?? '',
        getHeaders: () => ({ Authorization: `Bearer ${accessToken}` }),
      }),
    [accessToken],
  )
  const supplyReferenceClient = useMemo(
    () =>
      new ReferenceProviderClient({
        baseUrl: import.meta.env.VITE_SUPPLYARR_API_BASE ?? import.meta.env.VITE_ROUTARR_API_BASE ?? '',
        getHeaders: () => ({ Authorization: `Bearer ${accessToken}` }),
      }),
    [accessToken],
  )

  const tendersQuery = useQuery({
    queryKey: ['routarr-tenders', accessToken, selectedDemandFilter],
    queryFn: () => listTenders(accessToken, { transportationDemandId: selectedDemandFilter }),
    enabled: Boolean(accessToken),
  })

  const ratingsQuery = useQuery({
    queryKey: ['routarr-freight-ratings', accessToken, selectedDemandFilter],
    queryFn: () => listFreightRatings(accessToken, { transportationDemandId: selectedDemandFilter }),
    enabled: Boolean(accessToken),
  })

  const visibilityQuery = useQuery({
    queryKey: ['routarr-visibility-events', accessToken, selectedDemandFilter],
    queryFn: () => listVisibilityEvents(accessToken, { transportationDemandId: selectedDemandFilter }),
    enabled: Boolean(accessToken),
  })

  const planningQuery = useQuery({
    queryKey: ['routarr-planning-scenarios', accessToken],
    queryFn: () => listPlanningScenarios(accessToken),
  })

  const capacityQuery = useQuery({
    queryKey: ['routarr-driver-capacity', accessToken, capacityPersonId],
    queryFn: () => listDriverCapacitySnapshots(accessToken, capacityPersonId || undefined),
  })

  const yardQuery = useQuery({
    queryKey: ['routarr-yard-events', accessToken, selectedDemandFilter],
    queryFn: () => listYardEvents(accessToken, { transportationDemandId: selectedDemandFilter }),
  })

  const collaborationQuery = useQuery({
    queryKey: ['routarr-collaboration-submissions', accessToken],
    queryFn: () => listCollaborationSubmissions(accessToken),
  })

  const claimsQuery = useQuery({
    queryKey: ['routarr-freight-claims', accessToken, selectedDemandFilter],
    queryFn: () => listFreightClaims(accessToken, { transportationDemandId: selectedDemandFilter }),
  })

  const documentsQuery = useQuery({
    queryKey: ['routarr-document-packets', accessToken, selectedDemandFilter],
    queryFn: () => listDocumentPackets(accessToken, selectedDemandFilter),
  })

  const financeQuery = useQuery({
    queryKey: ['routarr-finance-contributions', accessToken],
    queryFn: () => listFinancePacketContributions(accessToken),
  })

  const createDemandMutation = useMutation({
    mutationFn: () =>
      createTransportationDemand(accessToken, {
        title,
        status: 'ready_for_planning',
        sourceProduct: 'routarr',
        sourceObjectType: 'manual',
        sourceObjectId: sourceObjectNumber || null,
        sourceObjectNumber: sourceObjectNumber || null,
        originLocationRef: serializeReferenceSnapshot(originLocationReference) ?? '',
        destinationLocationRef: serializeReferenceSnapshot(destinationLocationReference) ?? '',
        transportMode,
        serviceLevel,
        equipmentRequirement,
        customerRefs: serializeReferenceSnapshot(customerReference) ? [serializeReferenceSnapshot(customerReference)!] : [],
        orderRefs: splitRefs(orderRefs),
        lines: [
          {
            sourceProduct: 'routarr',
            sourceObjectRef: sourceObjectNumber || null,
            descriptionSnapshot: title,
            quantitySnapshot: 1,
            unitOfMeasure: 'move',
          },
        ],
        sourceRefs: sourceObjectNumber
          ? [
              {
                sourceProduct: 'routarr',
                sourceObjectType: 'manual',
                sourceObjectId: sourceObjectNumber,
                sourceObjectNumber,
                displayNameSnapshot: title,
                statusSnapshot: 'created',
                freshnessState: 'current',
              },
            ]
          : null,
      }),
    onSuccess: async (created) => {
      setSelectedDemandId(created.transportationDemandId)
      setTitle('')
      setOriginLocationReference(null)
      setDestinationLocationReference(null)
      setSourceObjectNumber('')
      setCustomerReference(null)
      setOrderRefs('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const updateDemandStatusMutation = useMutation({
    mutationFn: () =>
      updateTransportationDemandStatus(accessToken, selectedDemand!.transportationDemandId, {
        status: newDemandStatus,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const createPlanMutation = useMutation({
    mutationFn: () =>
      createPlanningScenario(accessToken, {
        demandRefs: [selectedDemand!.transportationDemandId],
        objective: 'service_cost_balance',
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-planning-scenarios'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const createTenderMutation = useMutation({
    mutationFn: () =>
      createTender(accessToken, {
        transportationDemandId: selectedDemand!.transportationDemandId,
        routingGuideSequence: (tendersQuery.data ?? []).length + 1,
        carrierSupplierRef: serializeReferenceSnapshot(carrierReference) ?? '',
        carrierSnapshotJson: serializeReferenceSnapshot(carrierReference),
        tenderMethod: 'portal',
      }),
    onSuccess: async () => {
      setCarrierReference(null)
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenders'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const updateTenderMutation = useMutation({
    mutationFn: (tender: CarrierTenderResponse) =>
      updateTenderStatus(accessToken, tender.tenderId, { status: tenderStatus }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-tenders'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const createRatingMutation = useMutation({
    mutationFn: () =>
      createFreightRating(accessToken, {
        transportationDemandId: selectedDemand!.transportationDemandId,
        tripId: selectedDemand!.tripId,
        buyRateEstimate: parseNumber(buyRateEstimate),
        sellRateEstimate: parseNumber(sellRateEstimate),
        plannedFreightCost: parseNumber(buyRateEstimate),
        actualFreightCost: parseNumber(actualFreightCost),
        currencyCode: 'USD',
        rateSourceSnapshot: 'RoutArr operational estimate',
        allocationSnapshotJson: JSON.stringify({ demandRef: selectedDemand!.demandNumber }),
      }),
    onSuccess: async () => {
      setBuyRateEstimate('')
      setSellRateEstimate('')
      setActualFreightCost('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-freight-ratings'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const createVisibilityMutation = useMutation({
    mutationFn: () =>
      createVisibilityEvent(accessToken, {
        transportationDemandId: selectedDemand!.transportationDemandId,
        tripId: selectedDemand!.tripId,
        eventType: visibilityStatus,
        normalizedStatus: visibilityStatus,
        source: 'manual_control_tower',
        freshnessState: 'current',
        reviewStatus: 'accepted',
        summary: visibilitySummary || compactStatus(visibilityStatus),
      }),
    onSuccess: async () => {
      setVisibilitySummary('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-visibility-events'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-transportation-demands'] })
    },
  })

  const createCapacityMutation = useMutation({
    mutationFn: () =>
      createDriverCapacitySnapshot(accessToken, {
        personId: capacityPersonId,
        source: 'manual_dispatch',
        hosRemainingMinutes: parseNumber(hosRemainingMinutes),
        feasibilityStatus: parseNumber(hosRemainingMinutes) === 0 ? 'blocked' : 'feasible',
        blockerSummary: parseNumber(hosRemainingMinutes) === 0 ? 'No HOS remaining' : null,
        freshnessState: 'current',
      }),
    onSuccess: async () => {
      setHosRemainingMinutes('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-driver-capacity'] })
    },
  })

  const createYardMutation = useMutation({
    mutationFn: () =>
      createYardEvent(accessToken, {
        transportationDemandId: selectedDemand?.transportationDemandId,
        tripId: selectedDemand?.tripId,
        eventType: yardEventType,
        trailerAssetRef,
        source: 'yard_console',
        dispatchImpact: yardEventType === 'gate_in' ? 'arrival confirmed' : 'dispatch status updated',
      }),
    onSuccess: async () => {
      setTrailerAssetRef('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-yard-events'] })
    },
  })

  const createClaimMutation = useMutation({
    mutationFn: () =>
      createFreightClaim(accessToken, {
        transportationDemandId: selectedDemand?.transportationDemandId,
        tripId: selectedDemand?.tripId,
        claimAgainstPartyType: 'carrier',
        claimReason,
        currencyCode: 'USD',
      }),
    onSuccess: async () => {
      setClaimReason('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-freight-claims'] })
    },
  })

  const createDocumentPacketMutation = useMutation({
    mutationFn: () =>
      createDocumentPacket(accessToken, {
        transportationDemandId: selectedDemand?.transportationDemandId,
        tripId: selectedDemand?.tripId,
        packetType: documentPacketType,
        requiredDocumentTypes: ['bill_of_lading', 'proof_of_delivery'],
        sourceFactsJson: JSON.stringify({ demandNumber: selectedDemand?.demandNumber }),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-document-packets'] })
    },
  })

  const createFinanceMutation = useMutation({
    mutationFn: () =>
      createFinancePacketContribution(accessToken, {
        transportationDemandId: selectedDemand?.transportationDemandId,
        tripId: selectedDemand?.tripId,
        freightRatingId: ratingsQuery.data?.[0]?.freightRatingId,
        contributionType: 'freight_operational_snapshot',
        targetProduct: financeTargetProduct,
        operationalSummary: `${selectedDemand?.demandNumber ?? 'Demand'} finance packet contribution`,
        costSnapshotJson: JSON.stringify({
          ratingId: ratingsQuery.data?.[0]?.freightRatingId ?? null,
          plannedFreightCost: ratingsQuery.data?.[0]?.plannedFreightCost ?? null,
          actualFreightCost: ratingsQuery.data?.[0]?.actualFreightCost ?? null,
        }),
        documentPacketRefs: documentsQuery.data?.map((packet) => packet.documentPacketRequestId) ?? [],
        claimRefs: claimsQuery.data?.map((claim) => claim.freightClaimId) ?? [],
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-finance-contributions'] })
    },
  })

  const demands = demandsQuery.data ?? []
  const selectedDemandTenders = tendersQuery.data ?? []
  const selectedDemandRatings = ratingsQuery.data ?? []
  const selectedDemandVisibility = visibilityQuery.data ?? []
  const selectedDemandYardEvents = yardQuery.data ?? []
  const selectedDemandClaims = claimsQuery.data ?? []
  const selectedDemandDocuments = documentsQuery.data ?? []
  const financeContributions = financeQuery.data ?? []
  const collaborationSubmissions = collaborationQuery.data ?? []
  const planningScenarios = planningQuery.data ?? []
  const capacitySnapshots = capacityQuery.data ?? []

  const blockers = useMemo(() => {
    if (!selectedDemand) return []
    const items: string[] = []
    if (!selectedDemand.originLocationRef) items.push('missing origin')
    if (!selectedDemand.destinationLocationRef) items.push('missing destination')
    if (selectedDemand.requirements.some((requirement) => requirement.required && requirement.status !== 'satisfied')) {
      items.push('open requirements')
    }
    if (selectedDemand.tenderStatus === 'tender_required') items.push('tender required')
    if (selectedDemand.freshnessState !== 'current') items.push('stale source snapshot')
    return items
  }, [selectedDemand])

  return (
    <section className="space-y-5" data-testid="transportation-demands-panel">
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-xl font-semibold text-slate-50">Transportation demands</h2>
          <p className="mt-1 text-sm text-slate-400">TMS planning runtime</p>
        </div>
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded bg-slate-800 px-3 py-2 text-sm text-slate-100 hover:bg-slate-700"
          onClick={() => {
            void Promise.all([
              demandsQuery.refetch(),
              tendersQuery.refetch(),
              ratingsQuery.refetch(),
              visibilityQuery.refetch(),
              planningQuery.refetch(),
              capacityQuery.refetch(),
              yardQuery.refetch(),
              collaborationQuery.refetch(),
              claimsQuery.refetch(),
              documentsQuery.refetch(),
              financeQuery.refetch(),
            ])
          }}
        >
          <RefreshCw className="h-4 w-4" aria-hidden="true" />
          Refresh
        </button>
      </header>

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        <SmallStat label="Open" value={demands.filter((demand) => !['closed', 'canceled'].includes(demand.status)).length} />
        <SmallStat label="Blocked" value={demands.filter((demand) => demand.status === 'blocked').length} />
        <SmallStat label="Tendered" value={demands.filter((demand) => demand.tenderStatus === 'tendered').length} />
        <SmallStat label="Visibility review" value={selectedDemandVisibility.filter((event) => event.reviewStatus !== 'accepted').length} />
        <SmallStat label="Finance ready" value={financeContributions.filter((item) => item.status === 'ready').length} />
      </div>

      {demandsQuery.isError ? (
        <DataError
          error={demandsQuery.error}
          label="Failed to load transportation demands."
          onRetry={() => void demandsQuery.refetch()}
        />
      ) : null}

      <div className="grid gap-4 lg:grid-cols-[minmax(280px,360px)_1fr]">
        <aside className="space-y-4">
          <section className="rounded border border-slate-700 bg-slate-900 p-4">
            <div className="flex items-center justify-between gap-3">
              <h3 className="text-sm font-semibold text-slate-100">Demand queue</h3>
              <select
                className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-xs text-slate-100"
                value={statusFilter}
                onChange={(event) => setStatusFilter(event.target.value)}
                aria-label="Demand status filter"
              >
                <option value="">All statuses</option>
                {statusOptions.map((status) => (
                  <option key={status} value={status}>
                    {compactStatus(status)}
                  </option>
                ))}
              </select>
            </div>
            <div className="mt-3 space-y-2">
              {demandsQuery.isLoading ? <p className="text-sm text-slate-500">Loading demands...</p> : null}
              {!demandsQuery.isLoading && demands.length === 0 ? <EmptyState label="No transportation demands found." /> : null}
              {demands.map((demand) => {
                const active = demand.transportationDemandId === selectedDemand?.transportationDemandId
                return (
                  <button
                    key={demand.transportationDemandId}
                    type="button"
                    className={`w-full rounded border px-3 py-2 text-left transition ${
                      active
                        ? 'border-sky-500 bg-sky-950/50 text-slate-50'
                        : 'border-slate-700 bg-slate-950 text-slate-300 hover:border-slate-500'
                    }`}
                    onClick={() => setSelectedDemandId(demand.transportationDemandId)}
                  >
                    <span className="block text-sm font-semibold">{demand.demandNumber}</span>
                    <span className="mt-1 block text-xs text-slate-500">{demand.title}</span>
                    <span className="mt-1 block text-xs text-slate-400">
                      {compactStatus(demand.status)} · {demand.freshnessState}
                    </span>
                  </button>
                )
              })}
            </div>
          </section>

          <section className="rounded border border-slate-700 bg-slate-900 p-4">
            <h3 className="text-sm font-semibold text-slate-100">Create demand</h3>
            <div className="mt-3 grid gap-2">
              <label className="block text-sm font-medium text-slate-300">
                Demand title
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  placeholder="Move customer freight"
                />
              </label>
              <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-1">
                <ReferencePicker
                  client={staffReferenceClient}
                  ownerProductKey="staffarr"
                  referenceType="location"
                  value={originLocationReference}
                  onChange={setOriginLocationReference}
                  label="Origin - StaffArr location"
                  placeholder="Search StaffArr locations"
                  allowQuickCreate={false}
                  required
                />
                <ReferencePicker
                  client={staffReferenceClient}
                  ownerProductKey="staffarr"
                  referenceType="location"
                  value={destinationLocationReference}
                  onChange={setDestinationLocationReference}
                  label="Destination - StaffArr location"
                  placeholder="Search StaffArr locations"
                  allowQuickCreate={false}
                  required
                />
              </div>
              <label className="block text-sm font-medium text-slate-300">
                Source number
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  value={sourceObjectNumber}
                  onChange={(event) => setSourceObjectNumber(event.target.value)}
                  placeholder="Order, load, or request number"
                />
              </label>
              <div className="grid gap-2 sm:grid-cols-3 lg:grid-cols-1">
                <ControlledSelect
                  value={transportMode}
                  onChange={setTransportMode}
                  options={transportModeOptions}
                  label="Transportation mode"
                  emptyLabel="Select mode"
                />
                <ControlledSelect
                  value={serviceLevel}
                  onChange={setServiceLevel}
                  options={serviceLevelOptions}
                  label="Service level"
                  emptyLabel="Select service level"
                />
                <ControlledSelect
                  value={equipmentRequirement}
                  onChange={setEquipmentRequirement}
                  options={equipmentRequirementOptions}
                  label="Equipment requirement"
                  emptyLabel="Select equipment"
                />
              </div>
              <ReferencePicker
                client={customReferenceClient}
                ownerProductKey="customarr"
                referenceType="customer"
                value={customerReference}
                onChange={setCustomerReference}
                label="Customer - CustomArr"
                placeholder="Search CustomArr customers"
              />
              <label className="block text-sm font-medium text-slate-300">
                OrdArr order refs
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                  value={orderRefs}
                  onChange={(event) => setOrderRefs(event.target.value)}
                  placeholder="ORD-1001, ORD-1002"
                />
              </label>
              <button
                type="button"
                className="inline-flex items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
                disabled={!title.trim() || !originLocationReference || !destinationLocationReference || createDemandMutation.isPending}
                onClick={() => createDemandMutation.mutate()}
              >
                <Plus className="h-4 w-4" aria-hidden="true" />
                Create demand
              </button>
              {createDemandMutation.isError ? (
                <p className="text-sm text-rose-300">{getErrorMessage(createDemandMutation.error, 'Demand creation failed.')}</p>
              ) : null}
            </div>
          </section>
        </aside>

        <main className="space-y-4">
          <SelectedDemandSummary demand={selectedDemand} />

          <div className="flex gap-2 overflow-x-auto rounded border border-slate-700 bg-slate-900 p-2">
            {tabs.map((tab) => {
              const Icon = tab.icon
              return (
                <button
                  key={tab.key}
                  type="button"
                  className={`inline-flex shrink-0 items-center gap-2 rounded px-3 py-2 text-sm ${
                    activeTab === tab.key
                      ? 'bg-sky-700 text-white'
                      : 'bg-slate-950 text-slate-300 hover:bg-slate-800'
                  }`}
                  onClick={() => setActiveTab(tab.key)}
                >
                  <Icon className="h-4 w-4" aria-hidden="true" />
                  {tab.label}
                </button>
              )
            })}
          </div>

          {activeTab === 'queue' ? (
            <DemandDetailTab
              demand={selectedDemand}
              blockers={blockers}
              newDemandStatus={newDemandStatus}
              setNewDemandStatus={setNewDemandStatus}
              isUpdating={updateDemandStatusMutation.isPending}
              updateError={updateDemandStatusMutation.error}
              onUpdate={() => updateDemandStatusMutation.mutate()}
            />
          ) : null}

          {activeTab === 'planner' ? (
            <PlanningTab
              selectedDemand={selectedDemand}
              scenarios={planningScenarios}
              isLoading={planningQuery.isLoading}
              error={planningQuery.error}
              onRetry={() => void planningQuery.refetch()}
              isCreating={createPlanMutation.isPending}
              createError={createPlanMutation.error}
              onCreate={() => createPlanMutation.mutate()}
            />
          ) : null}

          {activeTab === 'consolidation' ? <ConsolidationTab demands={demands} selectedDemand={selectedDemand} /> : null}

          {activeTab === 'tenders' ? (
            <TenderTab
              selectedDemand={selectedDemand}
              tenders={selectedDemandTenders}
              isLoading={tendersQuery.isLoading}
              error={tendersQuery.error}
              onRetry={() => void tendersQuery.refetch()}
              carrierReference={carrierReference}
              setCarrierReference={setCarrierReference}
              supplyReferenceClient={supplyReferenceClient}
              tenderStatus={tenderStatus}
              setTenderStatus={setTenderStatus}
              isCreating={createTenderMutation.isPending}
              createError={createTenderMutation.error}
              onCreate={() => createTenderMutation.mutate()}
              isUpdating={updateTenderMutation.isPending}
              updateError={updateTenderMutation.error}
              onUpdate={(tender) => updateTenderMutation.mutate(tender)}
            />
          ) : null}

          {activeTab === 'rating' ? (
            <RatingTab
              selectedDemand={selectedDemand}
              ratings={selectedDemandRatings}
              isLoading={ratingsQuery.isLoading}
              error={ratingsQuery.error}
              onRetry={() => void ratingsQuery.refetch()}
              buyRateEstimate={buyRateEstimate}
              setBuyRateEstimate={setBuyRateEstimate}
              sellRateEstimate={sellRateEstimate}
              setSellRateEstimate={setSellRateEstimate}
              actualFreightCost={actualFreightCost}
              setActualFreightCost={setActualFreightCost}
              isCreating={createRatingMutation.isPending}
              createError={createRatingMutation.error}
              onCreate={() => createRatingMutation.mutate()}
            />
          ) : null}

          {activeTab === 'visibility' ? (
            <VisibilityTab
              selectedDemand={selectedDemand}
              events={selectedDemandVisibility}
              isLoading={visibilityQuery.isLoading}
              error={visibilityQuery.error}
              onRetry={() => void visibilityQuery.refetch()}
              visibilityStatus={visibilityStatus}
              setVisibilityStatus={setVisibilityStatus}
              visibilitySummary={visibilitySummary}
              setVisibilitySummary={setVisibilitySummary}
              isCreating={createVisibilityMutation.isPending}
              createError={createVisibilityMutation.error}
              onCreate={() => createVisibilityMutation.mutate()}
            />
          ) : null}

          {activeTab === 'capacity' ? (
            <CapacityTab
              snapshots={capacitySnapshots}
              isLoading={capacityQuery.isLoading}
              error={capacityQuery.error}
              onRetry={() => void capacityQuery.refetch()}
              capacityPersonId={capacityPersonId}
              setCapacityPersonId={setCapacityPersonId}
              hosRemainingMinutes={hosRemainingMinutes}
              setHosRemainingMinutes={setHosRemainingMinutes}
              isCreating={createCapacityMutation.isPending}
              createError={createCapacityMutation.error}
              onCreate={() => createCapacityMutation.mutate()}
            />
          ) : null}

          {activeTab === 'yard' ? (
            <YardTab
              selectedDemand={selectedDemand}
              events={selectedDemandYardEvents}
              isLoading={yardQuery.isLoading}
              error={yardQuery.error}
              onRetry={() => void yardQuery.refetch()}
              yardEventType={yardEventType}
              setYardEventType={setYardEventType}
              trailerAssetRef={trailerAssetRef}
              setTrailerAssetRef={setTrailerAssetRef}
              isCreating={createYardMutation.isPending}
              createError={createYardMutation.error}
              onCreate={() => createYardMutation.mutate()}
            />
          ) : null}

          {activeTab === 'collaboration' ? (
            <CollaborationTab
              submissions={collaborationSubmissions}
              isLoading={collaborationQuery.isLoading}
              error={collaborationQuery.error}
              onRetry={() => void collaborationQuery.refetch()}
            />
          ) : null}

          {activeTab === 'claims' ? (
            <ClaimsTab
              selectedDemand={selectedDemand}
              claims={selectedDemandClaims}
              isLoading={claimsQuery.isLoading}
              error={claimsQuery.error}
              onRetry={() => void claimsQuery.refetch()}
              claimReason={claimReason}
              setClaimReason={setClaimReason}
              isCreating={createClaimMutation.isPending}
              createError={createClaimMutation.error}
              onCreate={() => createClaimMutation.mutate()}
            />
          ) : null}

          {activeTab === 'appointments' ? (
            <AppointmentsTab selectedDemand={selectedDemand} yardEvents={selectedDemandYardEvents} visibilityEvents={selectedDemandVisibility} />
          ) : null}

          {activeTab === 'finance' ? (
            <FinanceTab
              selectedDemand={selectedDemand}
              documents={selectedDemandDocuments}
              ratings={selectedDemandRatings}
              claims={selectedDemandClaims}
              contributions={financeContributions}
              documentsLoading={documentsQuery.isLoading}
              financeLoading={financeQuery.isLoading}
              error={documentsQuery.error ?? financeQuery.error}
              onRetry={() => {
                void documentsQuery.refetch()
                void financeQuery.refetch()
              }}
              documentPacketType={documentPacketType}
              setDocumentPacketType={setDocumentPacketType}
              financeTargetProduct={financeTargetProduct}
              setFinanceTargetProduct={setFinanceTargetProduct}
              isCreatingDocument={createDocumentPacketMutation.isPending}
              createDocumentError={createDocumentPacketMutation.error}
              onCreateDocument={() => createDocumentPacketMutation.mutate()}
              isCreatingFinance={createFinanceMutation.isPending}
              createFinanceError={createFinanceMutation.error}
              onCreateFinance={() => createFinanceMutation.mutate()}
            />
          ) : null}
        </main>
      </div>
    </section>
  )
}

function DemandDetailTab({
  demand,
  blockers,
  newDemandStatus,
  setNewDemandStatus,
  isUpdating,
  updateError,
  onUpdate,
}: {
  demand: TransportationDemandResponse | null
  blockers: string[]
  newDemandStatus: string
  setNewDemandStatus: (value: string) => void
  isUpdating: boolean
  updateError: unknown
  onUpdate: () => void
}) {
  if (!demand) return <EmptyState label="Select a transportation demand." />

  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_260px]">
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Readiness</h3>
          {blockers.length === 0 ? (
            <p className="mt-2 rounded border border-emerald-800 bg-emerald-950 px-3 py-2 text-sm text-emerald-100">
              No active blockers.
            </p>
          ) : (
            <ul className="mt-2 space-y-2">
              {blockers.map((blocker) => (
                <li key={blocker} className="rounded border border-amber-800 bg-amber-950 px-3 py-2 text-sm text-amber-100">
                  {blocker}
                </li>
              ))}
            </ul>
          )}
          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <DetailList title="Source refs" items={demand.sourceRefs.map((ref) => `${ref.sourceProduct} ${ref.sourceObjectNumber ?? ref.sourceObjectId} · ${ref.freshnessState}`)} />
            <DetailList title="Requirements" items={demand.requirements.map((requirement) => `${requirement.requirementType} · ${requirement.status}`)} />
            <DetailList title="Orders" items={demand.orderRefs} />
            <DetailList title="Handling" items={demand.handlingRequirements} />
          </div>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Lifecycle</h3>
          <select
            className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            value={newDemandStatus}
            onChange={(event) => setNewDemandStatus(event.target.value)}
            aria-label="New demand status"
          >
            {statusOptions.map((status) => (
              <option key={status} value={status}>
                {compactStatus(status)}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
            disabled={isUpdating}
            onClick={onUpdate}
          >
            <CheckCircle2 className="h-4 w-4" aria-hidden="true" />
            Update status
          </button>
          {updateError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(updateError, 'Status update failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function DetailList({ title, items }: { title: string; items: string[] }) {
  return (
    <div className="rounded border border-slate-700 bg-slate-950 p-3">
      <h4 className="text-xs font-semibold text-slate-300">{title}</h4>
      {items.length === 0 ? (
        <p className="mt-2 text-xs text-slate-500">None</p>
      ) : (
        <ul className="mt-2 space-y-1 text-xs text-slate-400">
          {items.map((item) => (
            <li key={item}>{item}</li>
          ))}
        </ul>
      )}
    </div>
  )
}

function PlanningTab({
  selectedDemand,
  scenarios,
  isLoading,
  error,
  onRetry,
  isCreating,
  createError,
  onCreate,
}: {
  selectedDemand: TransportationDemandResponse | null
  scenarios: PlanningScenarioResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  const related = selectedDemand
    ? scenarios.filter((scenario) => scenario.demandRefsJson.includes(selectedDemand.transportationDemandId))
    : scenarios

  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h3 className="text-sm font-semibold text-slate-100">Planning scenarios</h3>
        <button
          type="button"
          className="inline-flex items-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={!selectedDemand || isCreating}
          onClick={onCreate}
        >
          <GitBranch className="h-4 w-4" aria-hidden="true" />
          Run planner
        </button>
      </div>
      {error ? <DataError error={error} label="Failed to load planning scenarios." onRetry={onRetry} /> : null}
      {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Planning failed.')}</p> : null}
      {isLoading ? <p className="mt-3 text-sm text-slate-500">Loading scenarios...</p> : null}
      {!isLoading && related.length === 0 ? <EmptyState label="No planning scenarios found." /> : null}
      <div className="mt-3 grid gap-3">
        {related.map((scenario) => (
          <article key={scenario.planningScenarioId} className="rounded border border-slate-700 bg-slate-950 p-3">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h4 className="text-sm font-semibold text-slate-100">{scenario.scenarioNumber}</h4>
                <p className="mt-1 text-xs text-slate-500">{scenario.objective}</p>
              </div>
              <span className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">{compactStatus(scenario.status)}</span>
            </div>
            <div className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-3">
              <p>Cost: {scenario.costEstimate ?? '—'}</p>
              <p>Risk: {scenario.serviceRiskEstimate ?? '—'}</p>
              <p>Evaluated: {formatTimestamp(scenario.evaluatedAt)}</p>
            </div>
            {scenario.suggestions.length > 0 ? (
              <ul className="mt-3 space-y-2">
                {scenario.suggestions.map((suggestion) => (
                  <li key={suggestion.suggestionId} className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300">
                    {suggestion.summary}
                  </li>
                ))}
              </ul>
            ) : null}
          </article>
        ))}
      </div>
    </section>
  )
}

function ConsolidationTab({
  demands,
  selectedDemand,
}: {
  demands: TransportationDemandResponse[]
  selectedDemand: TransportationDemandResponse | null
}) {
  const suggestions = selectedDemand
    ? demands.filter(
        (demand) =>
          demand.transportationDemandId !== selectedDemand.transportationDemandId &&
          demand.originLocationRef === selectedDemand.originLocationRef &&
          demand.destinationLocationRef === selectedDemand.destinationLocationRef &&
          !['closed', 'canceled'].includes(demand.status),
      )
    : []

  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <h3 className="text-sm font-semibold text-slate-100">Consolidation</h3>
      {!selectedDemand ? <EmptyState label="Select a transportation demand." /> : null}
      {selectedDemand && suggestions.length === 0 ? <EmptyState label="No lane-compatible demands found." /> : null}
      <div className="mt-3 grid gap-3">
        {suggestions.map((demand) => (
          <article key={demand.transportationDemandId} className="rounded border border-slate-700 bg-slate-950 p-3">
            <h4 className="text-sm font-semibold text-slate-100">{demand.demandNumber}</h4>
            <p className="mt-1 text-sm text-slate-400">{demand.title}</p>
            <p className="mt-1 text-xs text-slate-500">
              {compactStatus(demand.status)} · {demand.transportMode} · {demand.serviceLevel}
            </p>
          </article>
        ))}
      </div>
    </section>
  )
}

function TenderTab({
  selectedDemand,
  tenders,
  isLoading,
  error,
  onRetry,
  carrierReference,
  setCarrierReference,
  supplyReferenceClient,
  tenderStatus,
  setTenderStatus,
  isCreating,
  createError,
  onCreate,
  isUpdating,
  updateError,
  onUpdate,
}: {
  selectedDemand: TransportationDemandResponse | null
  tenders: CarrierTenderResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  carrierReference: CrossProductReference | null
  setCarrierReference: (value: CrossProductReference | null) => void
  supplyReferenceClient: ReferenceProviderClient
  tenderStatus: string
  setTenderStatus: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
  isUpdating: boolean
  updateError: unknown
  onUpdate: (tender: CarrierTenderResponse) => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_260px]">
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Tender board</h3>
          {error ? <DataError error={error} label="Failed to load tenders." onRetry={onRetry} /> : null}
          {isLoading ? <p className="mt-3 text-sm text-slate-500">Loading tenders...</p> : null}
          {!isLoading && tenders.length === 0 ? <EmptyState label="No tenders found." /> : null}
          <div className="mt-3 grid gap-3">
            {tenders.map((tender) => (
              <article key={tender.tenderId} className="rounded border border-slate-700 bg-slate-950 p-3">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h4 className="text-sm font-semibold text-slate-100">{tender.tenderNumber}</h4>
                    <p className="mt-1 text-xs text-slate-500">{formatReferenceSnapshot(tender.carrierSupplierRef)}</p>
                  </div>
                  <span className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">{compactStatus(tender.status)}</span>
                </div>
                <p className="mt-2 text-xs text-slate-400">
                  Sequence {tender.routingGuideSequence} · {tender.tenderMethod} · expires {formatTimestamp(tender.expiresAt)}
                </p>
                <button
                  type="button"
                  className="mt-3 inline-flex items-center gap-2 rounded bg-slate-800 px-3 py-1.5 text-xs text-slate-100 disabled:opacity-50"
                  disabled={isUpdating}
                  onClick={() => onUpdate(tender)}
                >
                  <CheckCircle2 className="h-3.5 w-3.5" aria-hidden="true" />
                  Mark {compactStatus(tenderStatus)}
                </button>
              </article>
            ))}
          </div>
        </div>
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Tender action</h3>
          <div className="mt-3">
          <ReferencePicker
            client={supplyReferenceClient}
            ownerProductKey="supplyarr"
            referenceType="carrier"
            value={carrierReference}
            onChange={setCarrierReference}
            placeholder="Search SupplyArr carriers"
          />
          </div>
          <select
            className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            value={tenderStatus}
            onChange={(event) => setTenderStatus(event.target.value)}
            aria-label="Tender status"
          >
            {['accepted', 'rejected', 'expired', 'countered', 'withdrawn', 'fallback_required'].map((status) => (
              <option key={status} value={status}>
                {compactStatus(status)}
              </option>
            ))}
          </select>
          <button
            type="button"
            className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
            disabled={!selectedDemand || !carrierReference || isCreating}
            onClick={onCreate}
          >
            <Send className="h-4 w-4" aria-hidden="true" />
            Create tender
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Tender creation failed.')}</p> : null}
          {updateError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(updateError, 'Tender update failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function RatingTab({
  selectedDemand,
  ratings,
  isLoading,
  error,
  onRetry,
  buyRateEstimate,
  setBuyRateEstimate,
  sellRateEstimate,
  setSellRateEstimate,
  actualFreightCost,
  setActualFreightCost,
  isCreating,
  createError,
  onCreate,
}: {
  selectedDemand: TransportationDemandResponse | null
  ratings: FreightRatingResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  buyRateEstimate: string
  setBuyRateEstimate: (value: string) => void
  sellRateEstimate: string
  setSellRateEstimate: (value: string) => void
  actualFreightCost: string
  setActualFreightCost: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <RecordList
          title="Freight ratings"
          isLoading={isLoading}
          emptyLabel="No freight ratings found."
          records={ratings}
          render={(rating) => (
            <article key={rating.freightRatingId} className="rounded border border-slate-700 bg-slate-950 p-3">
              <h4 className="text-sm font-semibold text-slate-100">{rating.ratingNumber}</h4>
              <p className="mt-1 text-xs text-slate-500">
                {compactStatus(rating.status)} · {rating.currencyCode} · audit {rating.auditStatus}
              </p>
              <div className="mt-2 grid gap-2 text-xs text-slate-400 sm:grid-cols-3">
                <p>Buy {rating.buyRateEstimate ?? '—'}</p>
                <p>Sell {rating.sellRateEstimate ?? '—'}</p>
                <p>Variance {rating.varianceAmount ?? '—'}</p>
              </div>
            </article>
          )}
          error={error}
          onRetry={onRetry}
        />
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Rating snapshot</h3>
          <input className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={buyRateEstimate} onChange={(event) => setBuyRateEstimate(event.target.value)} placeholder="Buy estimate" aria-label="Buy estimate" />
          <input className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={sellRateEstimate} onChange={(event) => setSellRateEstimate(event.target.value)} placeholder="Sell estimate" aria-label="Sell estimate" />
          <input className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={actualFreightCost} onChange={(event) => setActualFreightCost(event.target.value)} placeholder="Actual cost" aria-label="Actual cost" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!selectedDemand || isCreating} onClick={onCreate}>
            <BadgeDollarSign className="h-4 w-4" aria-hidden="true" />
            Add rating
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Rating creation failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function VisibilityTab({
  selectedDemand,
  events,
  isLoading,
  error,
  onRetry,
  visibilityStatus,
  setVisibilityStatus,
  visibilitySummary,
  setVisibilitySummary,
  isCreating,
  createError,
  onCreate,
}: {
  selectedDemand: TransportationDemandResponse | null
  events: VisibilityEventResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  visibilityStatus: string
  setVisibilityStatus: (value: string) => void
  visibilitySummary: string
  setVisibilitySummary: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <RecordList
          title="Control tower events"
          isLoading={isLoading}
          emptyLabel="No visibility events found."
          records={events}
          render={(event) => (
            <article key={event.visibilityEventId} className="rounded border border-slate-700 bg-slate-950 p-3">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <h4 className="text-sm font-semibold text-slate-100">{compactStatus(event.normalizedStatus)}</h4>
                <span className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200">{event.freshnessState}</span>
              </div>
              <p className="mt-1 text-sm text-slate-400">{event.summary}</p>
              <p className="mt-1 text-xs text-slate-500">
                {event.source} · received {formatTimestamp(event.receivedAt)} · review {event.reviewStatus}
              </p>
            </article>
          )}
          error={error}
          onRetry={onRetry}
        />
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Visibility event</h3>
          <select className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={visibilityStatus} onChange={(event) => setVisibilityStatus(event.target.value)} aria-label="Visibility status">
            {['appointment_scheduled', 'gate_in', 'loaded', 'departed', 'in_transit', 'arrived', 'delivered', 'exception'].map((status) => (
              <option key={status} value={status}>{compactStatus(status)}</option>
            ))}
          </select>
          <input className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={visibilitySummary} onChange={(event) => setVisibilitySummary(event.target.value)} placeholder="Summary" aria-label="Visibility summary" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!selectedDemand || isCreating} onClick={onCreate}>
            <Activity className="h-4 w-4" aria-hidden="true" />
            Add event
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Visibility event creation failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function CapacityTab({
  snapshots,
  isLoading,
  error,
  onRetry,
  capacityPersonId,
  setCapacityPersonId,
  hosRemainingMinutes,
  setHosRemainingMinutes,
  isCreating,
  createError,
  onCreate,
}: {
  snapshots: DriverCapacitySnapshotResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  capacityPersonId: string
  setCapacityPersonId: (value: string) => void
  hosRemainingMinutes: string
  setHosRemainingMinutes: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <RecordList
          title="Driver capacity snapshots"
          isLoading={isLoading}
          emptyLabel="No capacity snapshots found."
          records={snapshots}
          render={(snapshot) => (
            <article key={snapshot.driverCapacitySnapshotId} className="rounded border border-slate-700 bg-slate-950 p-3">
              <h4 className="text-sm font-semibold text-slate-100">{snapshot.personId}</h4>
              <p className="mt-1 text-xs text-slate-500">
                {snapshot.feasibilityStatus} · HOS {snapshot.hosRemainingMinutes ?? '—'} min · {snapshot.freshnessState}
              </p>
              {snapshot.blockerSummary ? <p className="mt-2 text-sm text-amber-200">{snapshot.blockerSummary}</p> : null}
            </article>
          )}
          error={error}
          onRetry={onRetry}
        />
        <div>
          <h3 className="text-sm font-semibold text-slate-100">HOS snapshot</h3>
          <input className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={capacityPersonId} onChange={(event) => setCapacityPersonId(event.target.value)} placeholder="StaffArr person ref" aria-label="StaffArr person ref" />
          <input className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={hosRemainingMinutes} onChange={(event) => setHosRemainingMinutes(event.target.value)} placeholder="HOS remaining minutes" aria-label="HOS remaining minutes" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!capacityPersonId.trim() || isCreating} onClick={onCreate}>
            <Truck className="h-4 w-4" aria-hidden="true" />
            Add snapshot
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Capacity snapshot creation failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function YardTab({
  selectedDemand,
  events,
  isLoading,
  error,
  onRetry,
  yardEventType,
  setYardEventType,
  trailerAssetRef,
  setTrailerAssetRef,
  isCreating,
  createError,
  onCreate,
}: {
  selectedDemand: TransportationDemandResponse | null
  events: YardEventResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  yardEventType: string
  setYardEventType: (value: string) => void
  trailerAssetRef: string
  setTrailerAssetRef: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <RecordList
          title="Yard events"
          isLoading={isLoading}
          emptyLabel="No yard events found."
          records={events}
          render={(event) => (
            <article key={event.yardEventId} className="rounded border border-slate-700 bg-slate-950 p-3">
              <h4 className="text-sm font-semibold text-slate-100">{compactStatus(event.eventType)}</h4>
              <p className="mt-1 text-xs text-slate-500">
                Trailer {event.trailerAssetRef || '—'} · {event.loadedEmptyStatus} · {formatTimestamp(event.occurredAt)}
              </p>
              <p className="mt-2 text-sm text-slate-400">{event.dispatchImpact}</p>
            </article>
          )}
          error={error}
          onRetry={onRetry}
        />
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Gate/drop-hook</h3>
          <select className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={yardEventType} onChange={(event) => setYardEventType(event.target.value)} aria-label="Yard event type">
            {['gate_in', 'gate_out', 'trailer_dropped', 'trailer_hooked', 'dwell_started', 'detention_risk'].map((eventType) => (
              <option key={eventType} value={eventType}>{compactStatus(eventType)}</option>
            ))}
          </select>
          <input className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={trailerAssetRef} onChange={(event) => setTrailerAssetRef(event.target.value)} placeholder="MaintainArr trailer ref" aria-label="MaintainArr trailer ref" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!selectedDemand || isCreating} onClick={onCreate}>
            <Warehouse className="h-4 w-4" aria-hidden="true" />
            Add yard event
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Yard event creation failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function CollaborationTab({
  submissions,
  isLoading,
  error,
  onRetry,
}: {
  submissions: Array<{
    submissionId: string
    externalActorType: string
    externalActorRef: string
    actionType: string
    status: string
    submittedDataSummary: string
    submittedAt: string
  }>
  isLoading: boolean
  error: unknown
  onRetry: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <RecordList
        title="Portal submissions"
        isLoading={isLoading}
        emptyLabel="No collaboration submissions found."
        records={submissions}
        render={(submission) => (
          <article key={submission.submissionId} className="rounded border border-slate-700 bg-slate-950 p-3">
            <h4 className="text-sm font-semibold text-slate-100">{submission.actionType}</h4>
            <p className="mt-1 text-xs text-slate-500">
              {submission.externalActorType} {submission.externalActorRef} · {compactStatus(submission.status)}
            </p>
            <p className="mt-2 text-sm text-slate-400">{submission.submittedDataSummary}</p>
          </article>
        )}
        error={error}
        onRetry={onRetry}
      />
    </section>
  )
}

function ClaimsTab({
  selectedDemand,
  claims,
  isLoading,
  error,
  onRetry,
  claimReason,
  setClaimReason,
  isCreating,
  createError,
  onCreate,
}: {
  selectedDemand: TransportationDemandResponse | null
  claims: FreightClaimResponse[]
  isLoading: boolean
  error: unknown
  onRetry: () => void
  claimReason: string
  setClaimReason: (value: string) => void
  isCreating: boolean
  createError: unknown
  onCreate: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <RecordList
          title="Freight claims"
          isLoading={isLoading}
          emptyLabel="No freight claims found."
          records={claims}
          render={(claim) => (
            <article key={claim.freightClaimId} className="rounded border border-slate-700 bg-slate-950 p-3">
              <h4 className="text-sm font-semibold text-slate-100">{claim.claimNumber}</h4>
              <p className="mt-1 text-xs text-slate-500">
                {compactStatus(claim.status)} · {claim.claimAgainstPartyType} · {claim.currencyCode}
              </p>
              <p className="mt-2 text-sm text-slate-400">{claim.claimReason}</p>
            </article>
          )}
          error={error}
          onRetry={onRetry}
        />
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Claim request</h3>
          <input className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={claimReason} onChange={(event) => setClaimReason(event.target.value)} placeholder="Claim reason" aria-label="Claim reason" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!selectedDemand || !claimReason.trim() || isCreating} onClick={onCreate}>
            <FileText className="h-4 w-4" aria-hidden="true" />
            Request claim
          </button>
          {createError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createError, 'Claim creation failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function AppointmentsTab({
  selectedDemand,
  yardEvents,
  visibilityEvents,
}: {
  selectedDemand: TransportationDemandResponse | null
  yardEvents: YardEventResponse[]
  visibilityEvents: VisibilityEventResponse[]
}) {
  if (!selectedDemand) return <EmptyState label="Select a transportation demand." />

  const relevantEvents = [...visibilityEvents, ...yardEvents]
    .filter((event) => JSON.stringify(event).includes('gate') || JSON.stringify(event).includes('appointment'))
    .slice(0, 8)

  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      <h3 className="text-sm font-semibold text-slate-100">Appointment windows</h3>
      <div className="mt-3 grid gap-3 sm:grid-cols-2">
        <SmallStat label="Requested pickup" value={`${formatTimestamp(selectedDemand.requestedPickupStartAt)} to ${formatTimestamp(selectedDemand.requestedPickupEndAt)}`} />
        <SmallStat label="Requested delivery" value={`${formatTimestamp(selectedDemand.requestedDeliveryStartAt)} to ${formatTimestamp(selectedDemand.requestedDeliveryEndAt)}`} />
        <SmallStat label="Scheduled pickup" value={`${formatTimestamp(selectedDemand.scheduledPickupStartAt)} to ${formatTimestamp(selectedDemand.scheduledPickupEndAt)}`} />
        <SmallStat label="Scheduled delivery" value={`${formatTimestamp(selectedDemand.scheduledDeliveryStartAt)} to ${formatTimestamp(selectedDemand.scheduledDeliveryEndAt)}`} />
      </div>
      <h4 className="mt-4 text-xs font-semibold text-slate-300">SLA and detention signals</h4>
      {relevantEvents.length === 0 ? <EmptyState label="No appointment or gate signals found." /> : null}
      <div className="mt-3 grid gap-3">
        {relevantEvents.map((event, index) => (
          <article key={`${'visibilityEventId' in event ? event.visibilityEventId : event.yardEventId}-${index}`} className="rounded border border-slate-700 bg-slate-950 p-3">
            <h5 className="text-sm font-semibold text-slate-100">
              {compactStatus('normalizedStatus' in event ? event.normalizedStatus : event.eventType)}
            </h5>
            <p className="mt-1 text-xs text-slate-500">
              {formatTimestamp('receivedAt' in event ? event.receivedAt : event.occurredAt)}
            </p>
          </article>
        ))}
      </div>
    </section>
  )
}

function FinanceTab({
  selectedDemand,
  documents,
  ratings,
  claims,
  contributions,
  documentsLoading,
  financeLoading,
  error,
  onRetry,
  documentPacketType,
  setDocumentPacketType,
  financeTargetProduct,
  setFinanceTargetProduct,
  isCreatingDocument,
  createDocumentError,
  onCreateDocument,
  isCreatingFinance,
  createFinanceError,
  onCreateFinance,
}: {
  selectedDemand: TransportationDemandResponse | null
  documents: DocumentPacketResponse[]
  ratings: FreightRatingResponse[]
  claims: FreightClaimResponse[]
  contributions: FinancePacketContributionResponse[]
  documentsLoading: boolean
  financeLoading: boolean
  error: unknown
  onRetry: () => void
  documentPacketType: string
  setDocumentPacketType: (value: string) => void
  financeTargetProduct: string
  setFinanceTargetProduct: (value: string) => void
  isCreatingDocument: boolean
  createDocumentError: unknown
  onCreateDocument: () => void
  isCreatingFinance: boolean
  createFinanceError: unknown
  onCreateFinance: () => void
}) {
  return (
    <section className="rounded border border-slate-700 bg-slate-900 p-4">
      {error ? <DataError error={error} label="Failed to load finance readiness." onRetry={onRetry} /> : null}
      <div className="grid gap-4 lg:grid-cols-[1fr_280px]">
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-3">
            <SmallStat label="Ratings" value={ratings.length} />
            <SmallStat label="Documents" value={documents.length} />
            <SmallStat label="Claims" value={claims.length} />
          </div>
          <RecordList
            title="Finance packet contributions"
            isLoading={financeLoading}
            emptyLabel="No finance packet contributions found."
            records={contributions}
            render={(contribution) => (
              <article key={contribution.financePacketContributionId} className="rounded border border-slate-700 bg-slate-950 p-3">
                <h4 className="text-sm font-semibold text-slate-100">{contribution.contributionNumber}</h4>
                <p className="mt-1 text-xs text-slate-500">
                  {contribution.targetProduct} · {compactStatus(contribution.status)} · {contribution.contributionType}
                </p>
                <p className="mt-2 text-sm text-slate-400">{contribution.operationalSummary}</p>
              </article>
            )}
          />
          <RecordList
            title="Document packets"
            isLoading={documentsLoading}
            emptyLabel="No document packets found."
            records={documents}
            render={(packet) => (
              <article key={packet.documentPacketRequestId} className="rounded border border-slate-700 bg-slate-950 p-3">
                <h4 className="text-sm font-semibold text-slate-100">{packet.packetType}</h4>
                <p className="mt-1 text-xs text-slate-500">
                  {compactStatus(packet.status)} · {packet.requiredDocumentTypes.join(', ') || 'no required docs'}
                </p>
              </article>
            )}
          />
        </div>
        <div>
          <h3 className="text-sm font-semibold text-slate-100">Packet readiness</h3>
          <input className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={documentPacketType} onChange={(event) => setDocumentPacketType(event.target.value)} placeholder="Packet type" aria-label="Packet type" />
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-slate-800 px-3 py-2 text-sm font-medium text-slate-100 disabled:opacity-50" disabled={!selectedDemand || isCreatingDocument} onClick={onCreateDocument}>
            <FileText className="h-4 w-4" aria-hidden="true" />
            Request documents
          </button>
          <select className="mt-3 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100" value={financeTargetProduct} onChange={(event) => setFinanceTargetProduct(event.target.value)} aria-label="Finance target product">
            <option value="ordarr">OrdArr</option>
            <option value="supplyarr">SupplyArr</option>
          </select>
          <button type="button" className="mt-2 inline-flex w-full items-center justify-center gap-2 rounded bg-sky-700 px-3 py-2 text-sm font-medium text-white disabled:opacity-50" disabled={!selectedDemand || isCreatingFinance} onClick={onCreateFinance}>
            <CheckCircle2 className="h-4 w-4" aria-hidden="true" />
            Create contribution
          </button>
          {createDocumentError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createDocumentError, 'Document request failed.')}</p> : null}
          {createFinanceError ? <p className="mt-2 text-sm text-rose-300">{getErrorMessage(createFinanceError, 'Finance contribution failed.')}</p> : null}
        </div>
      </div>
    </section>
  )
}

function RecordList<T>({
  title,
  records,
  render,
  isLoading,
  emptyLabel,
  error,
  onRetry,
}: {
  title: string
  records: T[]
  render: (record: T) => ReactNode
  isLoading: boolean
  emptyLabel: string
  error?: unknown
  onRetry?: () => void
}) {
  return (
    <div>
      <h3 className="text-sm font-semibold text-slate-100">{title}</h3>
      {error && onRetry ? <DataError error={error} label={`Failed to load ${title.toLowerCase()}.`} onRetry={onRetry} /> : null}
      {isLoading ? <p className="mt-3 text-sm text-slate-500">Loading...</p> : null}
      {!isLoading && records.length === 0 ? <EmptyState label={emptyLabel} /> : null}
      <div className="mt-3 grid gap-3">{records.map(render)}</div>
    </div>
  )
}
