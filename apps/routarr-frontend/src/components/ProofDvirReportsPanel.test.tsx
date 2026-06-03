import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, afterEach } from 'vitest'
import { cleanup, fireEvent } from '@testing-library/react'

import { ProofDvirReportsPanel } from './ProofDvirReportsPanel'

vi.mock('../api/client', () => ({
  getProofDvirReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    scope: 'daily',
    windowStart: new Date().toISOString(),
    windowEnd: new Date().toISOString(),
    totalProofCount: 1,
    totalDvirCount: 1,
    tripWithProofOrDvirCount: 1,
    preTripDvirCount: 1,
    postTripDvirCount: 0,
    failOrConditionalDvirCount: 0,
    proofTypeCounts: [{ key: 'pickup', count: 1 }],
    dvirPhaseCounts: [{ key: 'pre_trip', count: 1 }],
    dvirResultCounts: [{ key: 'pass', count: 1 }],
    trips: [
      {
        tripId: 'trip-1',
        tripNumber: 'TR-PDV',
        title: 'Proof report haul',
        dispatchStatus: 'dispatched',
        assignedDriverPersonId: 'driver-1',
        vehicleRefKey: 'VEH-1',
        proofCount: 1,
        hasPreTripDvir: true,
        hasPostTripDvir: false,
        failOrConditionalDvirCount: 0,
      },
    ],
    recentProofs: [
      {
        proofId: 'proof-1',
        tripId: 'trip-1',
        tripNumber: 'TR-PDV',
        proofType: 'pickup',
        capturedByPersonId: 'driver-1',
        vehicleRefKey: 'VEH-1',
        referenceKey: 'BOL-1',
        reviewStatus: 'pending_review',
        capturedAt: new Date().toISOString(),
      },
    ],
    recentDvirInspections: [
      {
        dvirId: 'dvir-1',
        tripId: 'trip-1',
        tripNumber: 'TR-PDV',
        phase: 'pre_trip',
        result: 'pass',
        vehicleRefKey: 'VEH-1',
        submittedByPersonId: 'driver-1',
        submittedAt: new Date().toISOString(),
      },
    ],
  }),
  getProofDvirReportTripDetail: vi.fn(),
  getProofDvirReportProofDetail: vi.fn(),
  getProofDvirReportDvirDetail: vi.fn(),
  exportProofDvirReportSummaryCsv: vi.fn(),
  rejectProofDvirReportProof: vi.fn(),
  correctProofDvirReportProof: vi.fn(),
}))

describe('ProofDvirReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders proof/DVIR report summary rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProofDvirReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('proof-dvir-reports-panel')).toBeTruthy()
    expect(await screen.findByText(/TR-PDV — Proof report haul/i)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeTruthy()
  })

  it('returns null when user cannot read reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ProofDvirReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container.firstChild).toBeNull()
  })

  it('shows export failure callout', async () => {
    const { exportProofDvirReportSummaryCsv } = await import('../api/client')
    vi.mocked(exportProofDvirReportSummaryCsv).mockRejectedValueOnce(new Error('export down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProofDvirReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('proof-dvir-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
  })

  it('supports proof rejection and correction actions from proof detail', async () => {
    const { getProofDvirReportProofDetail, rejectProofDvirReportProof, correctProofDvirReportProof } = await import('../api/client')
    vi.mocked(getProofDvirReportProofDetail).mockResolvedValue({
      proofId: 'proof-1',
      tripId: 'trip-1',
      tripNumber: 'TR-PDV',
      tripTitle: 'Proof report haul',
      proofType: 'pickup',
      capturedByPersonId: 'driver-1',
      vehicleRefKey: 'VEH-1',
      referenceKey: 'BOL-1',
      notes: 'Original note',
      reviewStatus: 'pending_review',
      reviewedByPersonId: null,
      reviewedAt: null,
      reviewNotes: '',
      capturedAt: '2026-06-03T12:00:00Z',
      createdAt: '2026-06-03T12:00:00Z',
    })

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProofDvirReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('proof-dvir-reports-panel')
    fireEvent.click(await screen.findByRole('button', { name: /TR-PDV — pickup/i }))
    expect(await screen.findByText('Proof detail')).toBeInTheDocument()

    fireEvent.change(screen.getByPlaceholderText('Why is this being rejected or corrected?'), {
      target: { value: 'Needs updated receiver signature' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Reject proof/i }))

    await screen.findByText('Proof detail')
    await new Promise((resolve) => setTimeout(resolve, 0))
    expect(vi.mocked(rejectProofDvirReportProof)).toHaveBeenCalledWith('token', 'trip-1', 'proof-1', {
      reason: 'Needs updated receiver signature',
    })

    fireEvent.change(screen.getByPlaceholderText('Optional corrected reference'), {
      target: { value: 'BOL-2' },
    })
    fireEvent.change(screen.getByPlaceholderText('Optional updated notes'), {
      target: { value: 'Updated receiver signature captured.' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Mark corrected/i }))

    await new Promise((resolve) => setTimeout(resolve, 0))
    expect(vi.mocked(correctProofDvirReportProof)).toHaveBeenCalledWith('token', 'trip-1', 'proof-1', {
      reason: 'Proof corrected',
      referenceKey: 'BOL-2',
      notes: 'Updated receiver signature captured.',
    })
  })
})
