import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

import { AssetReservationPanel } from './AssetReservationPanel'
import { createAssetReservation, updateAssetReservation } from '../api/client'
import type { AssetReadinessResponse, AssetReservationResponse, AssetSearchResponse } from '../api/types'

const { readiness, reservation, assets } = vi.hoisted(() => ({
  readiness: {
    assetId: 'asset-1',
    assetTag: 'TRK-100',
    assetName: 'Truck 100',
    lifecycleStatus: 'in_service',
    readinessStatus: 'ready',
    readinessBasis: 'maintenance_clear',
    calculatedAt: '2026-06-01T00:00:00Z',
    blockers: [],
    signals: {
      openCriticalDefectCount: 0,
      openHighDefectCount: 0,
      activeWorkOrderCount: 0,
      pmDueCount: 0,
      pmOverdueCount: 0,
      failedInspectionCount: 0,
    },
  } as AssetReadinessResponse,
  assets: [
    {
      assetId: 'asset-1',
      assetTypeId: 'type-truck',
      typeKey: 'truck',
      typeName: 'Truck',
      classKey: 'vehicle',
      className: 'Vehicle',
      assetTag: 'TRK-100',
      name: 'Truck 100',
      description: 'Blocked current asset',
      lifecycleStatus: 'in_service',
      siteRef: 'site-1',
      staffarrSiteOrgUnitId: 'site-1',
      staffarrSiteNameSnapshot: 'North Yard',
      openDefectCount: 1,
      openWorkOrderCount: 2,
      readinessStatus: 'not_ready',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
    {
      assetId: 'asset-2',
      assetTypeId: 'type-truck',
      typeKey: 'truck',
      typeName: 'Truck',
      classKey: 'vehicle',
      className: 'Vehicle',
      assetTag: 'TRK-101',
      name: 'Truck 101',
      description: 'Ready same-type alternative',
      lifecycleStatus: 'in_service',
      siteRef: 'site-1',
      staffarrSiteOrgUnitId: 'site-1',
      staffarrSiteNameSnapshot: 'North Yard',
      openDefectCount: 0,
      openWorkOrderCount: 0,
      readinessStatus: 'ready',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
    {
      assetId: 'asset-3',
      assetTypeId: 'type-van',
      typeKey: 'van',
      typeName: 'Van',
      classKey: 'vehicle',
      className: 'Vehicle',
      assetTag: 'VAN-201',
      name: 'Van 201',
      description: 'Ready same-class alternative',
      lifecycleStatus: 'in_service',
      siteRef: 'site-2',
      staffarrSiteOrgUnitId: 'site-2',
      staffarrSiteNameSnapshot: 'South Yard',
      openDefectCount: 0,
      openWorkOrderCount: 0,
      readinessStatus: 'ready',
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    },
  ] as AssetSearchResponse[],
  reservation: {
    reservationId: 'reservation-1',
    assetId: 'asset-1',
    assetTag: 'TRK-100',
    assetName: 'Truck 100',
    reservationNumber: 'RV-20260601-0001',
    status: 'requested',
    purpose: 'Move equipment to job site',
    requestedStartAt: '2026-06-02T08:00:00Z',
    requestedEndAt: '2026-06-02T12:00:00Z',
    pickupLocationRef: 'site-1',
    pickupLocationNameSnapshot: 'North Yard',
    returnLocationRef: 'site-1',
    returnLocationNameSnapshot: 'North Yard',
    capacityNotes: 'Two passengers',
    equipmentNotes: 'Key and fuel card',
    operatorPersonId: 'person-1',
    operatorDisplayNameSnapshot: 'Alex Coordinator',
    driverPersonId: 'person-1',
    driverDisplayNameSnapshot: 'Alex Coordinator',
    requestedByPersonId: 'person-1',
    requestedByDisplayNameSnapshot: 'Alex Coordinator',
    notes: 'Handle with care',
    checkOutMeterReading: 128.4,
    returnMeterReading: 146.1,
    approvedAt: null,
    reservedAt: null,
    checkedOutAt: null,
    inUseAt: null,
    returnedAt: null,
    inspectedAt: null,
    closedAt: null,
    canceledAt: null,
    noShowAt: null,
    cancelReason: null,
    noShowReason: null,
    inspectionNotes: 'Interior inspection passed; no service defects found.',
    damageNotes: 'Minor scuff on rear bumper recorded at return.',
    chargeNotes: 'Chargeback waived after photo review.',
    readinessStatus: 'ready',
    readinessBasis: 'maintenance_clear',
    decisionStatus: 'watch',
    decisionSummary: 'Reservation is waiting for approval.',
    decisionDetail: 'No blockers were found, but the reservation still needs approval before it can be reserved.',
    conflictCount: 0,
    conflicts: [],
    qualificationChecks: [
      {
        role: 'operator',
        personId: 'person-1',
        personDisplayName: 'Alex Coordinator',
        qualificationKey: 'maintainarr.technician',
        outcome: 'allow',
        reasonCode: 'qualified',
        message: 'Operator is qualified.',
      },
    ],
    timeline: [
      {
        eventId: 'event-1',
        eventType: 'requested',
        fromStatus: '',
        toStatus: 'requested',
        message: 'Reservation RV-20260601-0001 was requested.',
        actorPersonId: 'person-1',
        actorDisplayNameSnapshot: 'Alex Coordinator',
        notes: 'Handle with care',
        meterReading: null,
        occurredAt: '2026-06-01T07:00:00Z',
        createdAt: '2026-06-01T07:00:00Z',
      },
    ],
    createdByUserId: 'user-1',
    createdAt: '2026-06-01T07:00:00Z',
    updatedAt: '2026-06-01T07:00:00Z',
  } as AssetReservationResponse,
}))

vi.mock('../api/client', () => ({
  getAssets: vi.fn().mockImplementation(async () => assets),
  getAssetReservations: vi.fn().mockResolvedValue([reservation]),
  getSites: vi.fn().mockResolvedValue([
    {
      key: 'site-1',
      id: 'site-1',
      label: 'North Yard',
      source: 'staffarr',
      sourceOfTruth: 'StaffArr',
      storedValue: 'site-1',
      displayValue: 'North Yard',
      isActive: true,
    },
  ]),
  getPeople: vi.fn().mockResolvedValue([
    {
      key: 'person-1',
      id: 'person-1',
      label: 'Alex Coordinator',
      source: 'staffarr',
      sourceOfTruth: 'StaffArr',
      storedValue: 'person-1',
      displayValue: 'Alex Coordinator',
      isActive: true,
    },
  ]),
  createAssetReservation: vi.fn().mockResolvedValue({
    ...reservation,
    reservationId: 'reservation-2',
    reservationNumber: 'RV-20260601-0002',
    purpose: 'Shop move',
  }),
  updateAssetReservation: vi.fn().mockResolvedValue({
    ...reservation,
    status: 'approved',
    decisionStatus: 'clear',
    decisionSummary: 'Reservation is approved and waiting for checkout.',
    decisionDetail: 'Approval has been captured. The reservation can move to the reserved state when the asset is ready.',
  }),
}))

function renderPanel(canRequest = true, canManage = true, currentReadiness: AssetReadinessResponse = readiness) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <AssetReservationPanel
        accessToken="token"
        assetId="asset-1"
        assetTag="TRK-100"
        assetName="Truck 100"
        readiness={currentReadiness}
        canRequest={canRequest}
        canManage={canManage}
      />
    </QueryClientProvider>,
  )
}

describe('AssetReservationPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders the reservation list and selected reservation details', async () => {
    renderPanel()

    expect(await screen.findByTestId('asset-reservation-panel')).toBeInTheDocument()
    expect(await screen.findByTestId('asset-reservation-list')).toBeInTheDocument()
    expect(await screen.findByTestId('asset-reservation-detail')).toBeInTheDocument()
    expect(await screen.findByTestId('asset-reservation-usage')).toBeInTheDocument()
    expect(await screen.findByTestId('asset-reservation-post-use')).toBeInTheDocument()
    const detail = screen.getByTestId('asset-reservation-detail')
    expect(screen.getByText('RV-20260601-0001')).toBeInTheDocument()
    expect(screen.getByText('Reservation is waiting for approval.')).toBeInTheDocument()
    expect(screen.getByText('Move equipment to job site')).toBeInTheDocument()
    expect(within(detail).getByText('Capacity notes')).toBeInTheDocument()
    expect(within(detail).getByText('Two passengers')).toBeInTheDocument()
    expect(within(detail).getByText('Handoff items')).toBeInTheDocument()
    expect(within(detail).getByText('Key and fuel card')).toBeInTheDocument()
    expect(within(detail).getByText('Usage meter delta')).toBeInTheDocument()
    expect(within(detail).getByText('17.7')).toBeInTheDocument()
    expect(within(detail).getByText('Damage notes')).toBeInTheDocument()
    expect(within(detail).getByText('Minor scuff on rear bumper recorded at return.')).toBeInTheDocument()
    expect(within(detail).getByText('Inspection notes')).toBeInTheDocument()
    expect(within(detail).getByText('Interior inspection passed; no service defects found.')).toBeInTheDocument()
    const postUse = screen.getByTestId('asset-reservation-post-use')
    expect(within(postUse).getByText('Charge notes')).toBeInTheDocument()
    expect(within(postUse).getByText('Chargeback waived after photo review.')).toBeInTheDocument()
  })

  it('allows workers to request a reservation', async () => {
    renderPanel(true, false)

    await waitFor(() => {
      expect(screen.getByTestId('reservation-pickup-location').querySelectorAll('option').length).toBeGreaterThan(1)
      expect(screen.getByTestId('reservation-operator').querySelectorAll('option').length).toBeGreaterThan(1)
    })

    fireEvent.change(
      screen.getByPlaceholderText('Haul equipment to a job site, loan to a technician, etc.'),
      { target: { value: 'Shop move' } },
    )
    fireEvent.change(screen.getByLabelText('Requested start'), { target: { value: '2026-06-02T08:00' } })
    fireEvent.change(screen.getByLabelText('Requested end'), { target: { value: '2026-06-02T12:00' } })
    fireEvent.change(screen.getByTestId('reservation-pickup-location'), { target: { value: 'site-1' } })
    fireEvent.change(screen.getByTestId('reservation-operator'), { target: { value: 'person-1' } })

    await waitFor(() => {
      expect(screen.getByPlaceholderText('Haul equipment to a job site, loan to a technician, etc.')).toHaveValue(
        'Shop move',
      )
      expect(screen.getByLabelText('Requested start')).toHaveValue('2026-06-02T08:00')
      expect(screen.getByLabelText('Requested end')).toHaveValue('2026-06-02T12:00')
      expect(screen.getByTestId('reservation-pickup-location')).toHaveValue('site-1')
      expect(screen.getByTestId('reservation-operator')).toHaveValue('person-1')
    })

    await waitFor(() =>
      expect(screen.getByRole('button', { name: 'Request reservation' })).not.toBeDisabled(),
    )
    fireEvent.click(screen.getByRole('button', { name: 'Request reservation' }))

    await waitFor(() =>
      expect(createAssetReservation).toHaveBeenCalledWith(
        'token',
        'asset-1',
        expect.objectContaining({
          purpose: 'Shop move',
          pickupLocationRef: 'site-1',
          operatorPersonId: 'person-1',
        }),
      ),
    )
  })

  it('suggests ready alternatives when the current asset is blocked', async () => {
    const blockedReadiness = {
      ...readiness,
      readinessStatus: 'not_ready',
      readinessBasis: 'maintenance_blockers',
      blockers: [
        {
          blockerType: 'active_work_order',
          message: 'An active work order keeps this asset out of service.',
          sourceEntityType: 'work_order',
          sourceEntityId: 'wo-blocked-1',
          relatedEntityId: null,
        },
      ],
      signals: {
        ...readiness.signals,
        activeWorkOrderCount: 1,
      },
    } as AssetReadinessResponse

    renderPanel(true, true, blockedReadiness)

    expect(await screen.findByTestId('asset-reservation-alternatives')).toBeInTheDocument()
    expect(screen.getByText('Suggested alternatives')).toBeInTheDocument()
    expect(screen.getByText('Truck 101')).toBeInTheDocument()
    expect(screen.getByText('Van 201')).toBeInTheDocument()
    expect(screen.getByTestId('asset-reservation-alternatives')).toHaveTextContent(/same type/i)
    expect(screen.getByTestId('asset-reservation-alternatives')).toHaveTextContent(/same class/i)
  })

  it('exposes actions for the selected reservation', async () => {
    renderPanel(true, true)

    fireEvent.click(await screen.findByRole('button', { name: 'Approve' }))

    await waitFor(() =>
      expect(updateAssetReservation).toHaveBeenCalledWith(
        'token',
        'reservation-1',
        'approve',
        expect.objectContaining({
          occurredAt: expect.any(String),
        }),
      ),
    )
  })
})
