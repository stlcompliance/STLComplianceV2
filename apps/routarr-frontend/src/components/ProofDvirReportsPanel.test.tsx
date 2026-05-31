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
})
