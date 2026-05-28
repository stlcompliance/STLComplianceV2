import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { M12AnalyticsWorkerSettingsPanel } from './M12AnalyticsWorkerSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getM12AnalyticsWorkerSettings: vi.fn(),
    upsertM12AnalyticsWorkerSettings: vi.fn(),
  }
})

describe('M12AnalyticsWorkerSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders worker settings form for managers', async () => {
    vi.mocked(client.getM12AnalyticsWorkerSettings).mockResolvedValue({
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
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <M12AnalyticsWorkerSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByTestId('compliancecore-m12-worker-save')).toBeTruthy()
    })
    expect(screen.getByTestId('compliancecore-m12-worker-enabled')).toBeTruthy()
    expect(screen.getByTestId('compliancecore-m12-worker-forecast')).toBeTruthy()
  })

  it('returns null when user cannot manage settings', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <M12AnalyticsWorkerSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )
    expect(container.firstChild).toBeNull()
  })
})
