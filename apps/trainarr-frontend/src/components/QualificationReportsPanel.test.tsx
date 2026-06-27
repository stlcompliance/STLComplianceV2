import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { QualificationReportsPanel } from './QualificationReportsPanel'

vi.mock('@stl/shared-ui', () => {
  return {
    ApiErrorCallout: ({
      title,
      message,
      retryLabel,
      onRetry,
    }: {
      title: string
      message: string
      retryLabel?: string
      onRetry?: () => void
    }) => (
      <div>
        <h3>{title}</h3>
        <p>{message}</p>
        {retryLabel && onRetry ? <button type="button" onClick={onRetry}>{retryLabel}</button> : null}
      </div>
    ),
    ReferenceProviderClient: class ReferenceProviderClient {
      constructor(_options: unknown) {}
    },
    ReferenceSearchPicker: ({
      label,
      value,
      onChange,
      id,
      placeholder,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      id?: string
      placeholder?: string
    }) => (
      <label htmlFor={id ?? label}>
        {label}
        <input
          id={id ?? label}
          aria-label={label ?? placeholder ?? 'Reference search picker'}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
      </label>
    ),
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      id,
      placeholder,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      id?: string
      placeholder?: string
    }) => (
      <label htmlFor={id ?? label}>
        {label}
        <input
          id={id ?? label}
          aria-label={label ?? placeholder ?? 'Static search picker'}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
      </label>
    ),
    getErrorMessage: (error: unknown, fallback: string) => (error instanceof Error ? error.message : fallback),
  }
})

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getQualificationReportSummary: vi.fn(),
    exportQualificationReportSummaryCsv: vi.fn(),
    getPointInTimeQualificationReport: vi.fn(),
    listQualificationIssuesForReport: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <QualificationReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('QualificationReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getQualificationReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportQualificationReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    vi.mocked(client.listQualificationIssuesForReport).mockResolvedValue([
      {
        qualificationIssueId: 'issue-1',
        trainingAssignmentId: 'assignment-1',
        staffarrPersonId: 'person-1',
        qualificationKey: 'hazmat_endorsement',
        qualificationName: 'Hazmat Endorsement',
        status: 'issued',
        issuedAt: new Date().toISOString(),
        expiresAt: null,
        statusChangedAt: null,
        lifecycleReason: null,
      },
    ])
    vi.mocked(client.getPointInTimeQualificationReport).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      staffarrPersonId: 'person-1',
      actionTask: 'Drive hazmat route',
      qualificationKey: 'hazmat_endorsement',
      qualificationName: 'Hazmat Endorsement',
      asOfUtc: new Date().toISOString(),
      isQualified: true,
      statusOnDate: 'issued',
      qualificationMessage: 'Qualification was active.',
      sourceCertificate: null,
      programVersion: null,
      expirationState: {
        expiresAt: null,
        isExpired: false,
        daysUntilExpiration: null,
        message: 'No expiration date was recorded for the certificate.',
      },
      restrictions: [],
      evidence: [],
      signoffs: [],
      auditTrail: [],
    })

    renderPanel()

    expect(await screen.findByText('Qualification report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })

  it('runs a point-in-time report and renders the result', async () => {
    vi.mocked(client.getQualificationReportSummary).mockResolvedValue({
      totalQualifications: 1,
      issuedCount: 1,
      expiredCount: 0,
      suspendedCount: 0,
      revokedCount: 0,
      expiringWithin30Days: 0,
      recentQualifications: [],
    })
    vi.mocked(client.exportQualificationReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    vi.mocked(client.listQualificationIssuesForReport).mockResolvedValue([
      {
        qualificationIssueId: 'issue-1',
        trainingAssignmentId: 'assignment-1',
        staffarrPersonId: 'person-1',
        qualificationKey: 'hazmat_endorsement',
        qualificationName: 'Hazmat Endorsement',
        status: 'issued',
        issuedAt: new Date().toISOString(),
        expiresAt: null,
        statusChangedAt: null,
        lifecycleReason: null,
      },
    ])
    vi.mocked(client.getPointInTimeQualificationReport).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      staffarrPersonId: 'person-1',
      actionTask: 'Drive hazmat route',
      qualificationKey: 'hazmat_endorsement',
      qualificationName: 'Hazmat Endorsement',
      asOfUtc: new Date().toISOString(),
      isQualified: true,
      statusOnDate: 'issued',
      qualificationMessage: 'Qualification was active on the requested date.',
      sourceCertificate: {
        qualificationIssueId: 'issue-1',
        trainingAssignmentId: 'assignment-1',
        grantPublicationId: 'grant-1',
        issuedAt: new Date().toISOString(),
        expiresAt: null,
        statusOnDate: 'issued',
        lifecycleReason: null,
        lifecyclePublicationId: null,
      },
      programVersion: null,
      expirationState: {
        expiresAt: null,
        isExpired: false,
        daysUntilExpiration: null,
        message: 'No expiration date was recorded for the certificate.',
      },
      restrictions: ['Qualification was active on the requested date.'],
      evidence: [],
      signoffs: [],
      auditTrail: [],
    })

    renderPanel()

    fireEvent.change(screen.getByLabelText(/^Person$/i), { target: { value: 'person-1' } })
    fireEvent.change(screen.getByLabelText('Qualification key'), {
      target: { value: 'hazmat_endorsement' },
    })
    fireEvent.change(screen.getByLabelText('Action / task'), {
      target: { value: 'Drive hazmat route' },
    })
    await waitFor(() => expect(screen.getByRole('button', { name: 'Run report' })).toBeEnabled())
    fireEvent.click(screen.getByRole('button', { name: 'Run report' }))

    expect(await screen.findByText('Qualified')).toBeInTheDocument()
    expect(screen.getByText('Hazmat Endorsement')).toBeInTheDocument()
  })
})
