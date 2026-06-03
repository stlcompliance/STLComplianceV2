import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ComplianceWaiversPanel } from './ComplianceWaiversPanel'

const mockListComplianceWaivers = vi.fn()
const mockApproveComplianceWaiver = vi.fn()
const mockRevokeComplianceWaiver = vi.fn()
const mockCreateComplianceWaiver = vi.fn()
const mockRenewComplianceWaiver = vi.fn()

vi.mock('../api/client', () => ({
  listComplianceWaivers: (...args: unknown[]) => mockListComplianceWaivers(...args),
  approveComplianceWaiver: (...args: unknown[]) => mockApproveComplianceWaiver(...args),
  revokeComplianceWaiver: (...args: unknown[]) => mockRevokeComplianceWaiver(...args),
  createComplianceWaiver: (...args: unknown[]) => mockCreateComplianceWaiver(...args),
  renewComplianceWaiver: (...args: unknown[]) => mockRenewComplianceWaiver(...args),
}))

vi.mock('@stl/shared-ui', () => ({
  buildSemanticKey: () => 'waiver-test-key',
  GeneratedKeyField: () => <div data-testid="generated-key-field" />,
}))

describe('ComplianceWaiversPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('offers renewal for approved waivers and calls the renew endpoint', async () => {
    mockListComplianceWaivers.mockResolvedValueOnce([
      {
        waiverId: 'waiver-1',
        waiverKey: 'waiver-test-key',
        rulePackId: 'pack-1',
        packKey: 'pack-key',
        ruleKey: null,
        gateKey: null,
        subjectScopeKey: 'tenant',
        reasonCode: 'temporary_ops_override',
        explanation: 'Need a temporary extension.',
        status: 'approved',
        effectiveAt: '2026-06-01T00:00:00Z',
        expiresAt: '2026-06-30T00:00:00Z',
        createdByUserId: null,
        approvedByUserId: null,
        approvedAt: null,
        revokedByUserId: null,
        revokedAt: null,
        createdAt: '2026-06-01T00:00:00Z',
        updatedAt: '2026-06-01T00:00:00Z',
      },
    ])

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ComplianceWaiversPanel accessToken="token" rulePacks={[]} canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByRole('button', { name: 'Renew 30 days' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Renew 30 days' }))

    await waitFor(() => {
      expect(mockRenewComplianceWaiver).toHaveBeenCalledTimes(1)
    })
    expect(mockRenewComplianceWaiver).toHaveBeenCalledWith(
      'token',
      'waiver-1',
      expect.objectContaining({
        notes: 'Renewed from Compliance Core UI.',
      }),
    )
  })

  it('creates a time-bound waiver request with an expiry window', async () => {
    mockListComplianceWaivers.mockResolvedValueOnce([])
    mockCreateComplianceWaiver.mockResolvedValueOnce({
      waiverId: 'waiver-2',
      waiverKey: 'waiver-test-key',
      rulePackId: 'pack-1',
      packKey: 'pack-key',
      ruleKey: null,
      gateKey: null,
      subjectScopeKey: 'tenant',
      reasonCode: 'temporary_ops_override',
      explanation: 'Need a short-term exception.',
      status: 'pending',
      effectiveAt: '2026-06-01T00:00:00Z',
      expiresAt: '2026-07-01T00:00:00Z',
      createdByUserId: null,
      approvedByUserId: null,
      approvedAt: null,
      revokedByUserId: null,
      revokedAt: null,
      createdAt: '2026-06-01T00:00:00Z',
      updatedAt: '2026-06-01T00:00:00Z',
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ComplianceWaiversPanel
          accessToken="token"
          rulePacks={[
            {
              rulePackId: 'pack-1',
              packKey: 'pack-key',
              label: 'Pack Key',
              domainKey: 'safety',
              ownerKey: 'platform',
              status: 'published',
              effectiveAt: '2026-06-01T00:00:00Z',
              expiresAt: null,
              versionNumber: 1,
            } as never,
          ]}
          canManage={true}
        />
      </QueryClientProvider>,
    )

    await screen.findByTestId('compliance-waivers-panel')
    fireEvent.change(screen.getByLabelText('Rule pack'), {
      target: { value: 'pack-1' },
    })
    fireEvent.change(screen.getByLabelText('Explanation'), {
      target: { value: 'Need a short-term exception.' },
    })
    fireEvent.change(screen.getByLabelText('Expires at'), {
      target: { value: '2026-07-01T00:00' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Request waiver' }))

    await waitFor(() => {
      expect(mockCreateComplianceWaiver).toHaveBeenCalledTimes(1)
    })
    expect(mockCreateComplianceWaiver).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        waiverKey: 'waiver-test-key',
        rulePackId: 'pack-1',
        subjectScopeKey: 'tenant',
        reasonCode: 'temporary_ops_override',
        explanation: 'Need a short-term exception.',
        expiresAt: new Date('2026-07-01T00:00:00').toISOString(),
      }),
    )
  })
})
