import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { AuditDeliveryOrchestrationPanel } from './AuditDeliveryOrchestrationPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getAuditDeliveryOrchestrationStatus: vi.fn(),
    triggerScheduledRuleEvaluation: vi.fn(),
    triggerM12AnalyticsBatch: vi.fn(),
  }
})

describe('AuditDeliveryOrchestrationPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders orchestration status and trigger controls for admins', async () => {
    vi.mocked(client.getAuditDeliveryOrchestrationStatus).mockResolvedValue({
      workerSettings: {
        isEnabled: true,
        defaultScopeKey: 'tenant',
        intervalHours: 24,
        riskScoringEnabled: true,
        missingEvidenceEnabled: true,
        controlEffectivenessEnabled: true,
        readinessForecastEnabled: true,
        auditDeliveryEnabled: false,
        lastBatchRunAt: null,
        lastRiskScoringRunAt: null,
        lastMissingEvidenceRunAt: null,
        lastControlEffectivenessRunAt: null,
        lastReadinessForecastRunAt: null,
        lastAuditDeliveryRunAt: null,
        updatedAt: '2026-05-28T12:00:00Z',
      },
      scheduledEvaluation: {
        pendingPacksCount: 2,
        lastRun: {
          runId: '11111111-1111-1111-1111-111111111101',
          startedAt: '2026-05-28T10:00:00Z',
          completedAt: '2026-05-28T10:01:00Z',
          status: 'completed',
          packsDueCount: 2,
          evaluatedCount: 2,
          skippedCount: 0,
          allowCount: 2,
          warnCount: 0,
          blockCount: 0,
        },
      },
      m12Batch: {
        workerEnabled: true,
        batchDue: true,
        pendingSteps: {
          tenantId: '22222222-2222-2222-2222-222222222201',
          defaultScopeKey: 'tenant',
          intervalHours: 24,
          riskScoringDue: true,
          missingEvidenceDue: false,
          controlEffectivenessDue: false,
          readinessForecastDue: false,
          auditDeliveryDue: false,
        },
        lastRun: null,
      },
      auditPackages: {
        pendingJobsCount: 1,
        recentJobs: [
          {
            jobId: '33333333-3333-3333-3333-333333333301',
            status: 'pending',
            format: 'zip',
            createdAt: '2026-05-28T11:00:00Z',
            completedAt: null,
            packageId: null,
            errorMessage: null,
          },
        ],
      },
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <AuditDeliveryOrchestrationPanel accessToken="token" canRead canTrigger />
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByTestId('compliancecore-orchestration-trigger-scheduled-eval')).toBeTruthy()
    })
    expect(screen.getByTestId('compliancecore-orchestration-trigger-m12-batch')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-orchestration-scheduled-eval')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-orchestration-audit-jobs')).toBeTruthy()
  })

  it('returns null when user cannot read orchestration', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <AuditDeliveryOrchestrationPanel accessToken="token" canRead={false} canTrigger={false} />
      </QueryClientProvider>,
    )
    expect(container.firstChild).toBeNull()
  })

  it('shows retry callout when orchestration load fails', async () => {
    vi.mocked(client.getAuditDeliveryOrchestrationStatus).mockRejectedValue(new Error('status down'))
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <AuditDeliveryOrchestrationPanel accessToken="token" canRead canTrigger />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Orchestration status unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry status' })).toBeInTheDocument()
  })
})
