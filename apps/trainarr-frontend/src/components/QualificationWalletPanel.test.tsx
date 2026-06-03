import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import * as client from '../api/client'
import { QualificationWalletPanel } from './QualificationWalletPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getQualificationWalletCredential: vi.fn(),
    verifyQualificationWalletCredential: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <QualificationWalletPanel
        accessToken="token"
        canManage
        selectedIssueId="issue-1"
        issues={[
          {
            qualificationIssueId: 'issue-1',
            trainingAssignmentId: 'assignment-1',
            staffarrPersonId: 'person-1',
            qualificationKey: 'hazmat_endorsement',
            qualificationName: 'Hazmat Endorsement',
            status: 'issued',
            issuedAt: '2026-05-28T12:00:00Z',
            expiresAt: '2026-08-28T12:00:00Z',
            statusChangedAt: null,
            lifecycleReason: null,
          },
        ]}
      />
    </QueryClientProvider>,
  )
}

describe('QualificationWalletPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders a signed credential card and verifies smart badge tokens', async () => {
    vi.mocked(client.getQualificationWalletCredential).mockResolvedValue({
      qualificationIssueId: 'issue-1',
      staffarrPersonId: 'person-1',
      qualificationKey: 'hazmat_endorsement',
      qualificationName: 'Hazmat Endorsement',
      status: 'issued',
      issuedAt: '2026-05-28T12:00:00Z',
      expiresAt: '2026-08-28T12:00:00Z',
      generatedAt: '2026-05-29T12:00:00Z',
      credentialToken: 'signed-credential-token',
      verificationUrl: 'http://localhost/api/v1/qualifications/wallet/verify',
      displayLabel: 'Hazmat Endorsement credential',
    })
    vi.mocked(client.verifyQualificationWalletCredential).mockResolvedValue({
      verifiedAt: '2026-05-29T12:30:00Z',
      isValid: true,
      message: 'Credential is valid for Hazmat Endorsement.',
      credential: {
        qualificationIssueId: 'issue-1',
        staffarrPersonId: 'person-1',
        qualificationKey: 'hazmat_endorsement',
        qualificationName: 'Hazmat Endorsement',
        status: 'issued',
        issuedAt: '2026-05-28T12:00:00Z',
        expiresAt: '2026-08-28T12:00:00Z',
        generatedAt: '2026-05-29T12:00:00Z',
        credentialToken: 'signed-credential-token',
        verificationUrl: 'http://localhost/api/v1/qualifications/wallet/verify',
        displayLabel: 'Hazmat Endorsement credential',
      },
      report: {
        generatedAt: '2026-05-29T12:30:00Z',
        staffarrPersonId: 'person-1',
        actionTask: 'Drive hazmat route',
        qualificationKey: 'hazmat_endorsement',
        qualificationName: 'Hazmat Endorsement',
        asOfUtc: '2026-05-29T12:30:00Z',
        isQualified: true,
        statusOnDate: 'issued',
        qualificationMessage: 'Qualification was active.',
        sourceCertificate: {
          qualificationIssueId: 'issue-1',
          trainingAssignmentId: 'assignment-1',
          grantPublicationId: 'grant-1',
          issuedAt: '2026-05-28T12:00:00Z',
          expiresAt: '2026-08-28T12:00:00Z',
          statusOnDate: 'issued',
          lifecycleReason: null,
          lifecyclePublicationId: null,
        },
        programVersion: null,
        expirationState: {
          expiresAt: '2026-08-28T12:00:00Z',
          isExpired: false,
          daysUntilExpiration: 91,
          message: 'Qualification expires on 2026-08-28 12:00:00Z (91 day(s) remaining).',
        },
        restrictions: ['Qualification was active on the requested date.'],
        evidence: [],
        signoffs: [],
        auditTrail: [],
      },
    })

    renderPanel()

    expect(await screen.findByText('Hazmat Endorsement credential')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Download JSON' })).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Credential token'), {
      target: { value: 'signed-credential-token' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Verify credential' }))

    expect(await screen.findByText('Valid badge')).toBeInTheDocument()
    expect(screen.getByText('Credential is valid for Hazmat Endorsement.')).toBeInTheDocument()
    expect(screen.getByText('Qualification was active on the requested date.')).toBeInTheDocument()
  })
})
