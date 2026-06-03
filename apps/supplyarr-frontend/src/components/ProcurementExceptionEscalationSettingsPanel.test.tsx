import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ProcurementExceptionEscalationSettingsPanel } from './ProcurementExceptionEscalationSettingsPanel'

vi.mock('../api/client', () => ({
  getProcurementExceptionEscalationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    escalationCooldownHours: 24,
    maxEscalationsPerException: 5,
    notifyOnProcurementExceptionSlaEscalation: true,
    autoCloseCompletedExceptionsEnabled: true,
    autoCloseCompletedExceptionsAfterHours: 48,
    updatedAt: null,
  }),
  getPendingProcurementExceptionEscalations: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    batchSize: 25,
    items: [],
  }),
  getPendingProcurementExceptionAutoCloses: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    batchSize: 25,
    items: [
      {
        procurementExceptionId: 'ex-close-1',
        exceptionKey: 'PEX-CLOSE-1',
        subjectType: 'purchase_request',
        subjectId: 'subject-1',
        subjectKey: 'pr-close-1',
        title: 'Auto-close candidate',
        status: 'resolved',
        resolvedAt: new Date().toISOString(),
        waivedAt: null,
        completedAt: new Date().toISOString(),
        hoursCompleted: 50,
        hoursUntilAutoClose: 0,
      },
    ],
  }),
  getProcurementExceptionEscalationRuns: vi.fn().mockResolvedValue({ items: [] }),
  getProcurementExceptionEscalationEvents: vi.fn().mockResolvedValue({
    items: [
      {
        eventId: 'event-1',
        procurementExceptionId: 'ex-1',
        exceptionKey: 'PEX-E2E-1',
        escalationLevel: 1,
        actionKind: 'sla_escalation',
        notificationDispatchId: null,
        createdAt: new Date().toISOString(),
      },
    ],
  }),
  upsertProcurementExceptionEscalationSettings: vi.fn(),
}))

describe('ProcurementExceptionEscalationSettingsPanel', () => {
  it('renders when user can manage settings', async () => {
    render(
      <QueryClientProvider client={new QueryClient()}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(
      await screen.findByTestId('procurement-exception-escalation-settings-panel'),
    ).toBeInTheDocument()
    expect(screen.getByText('Procurement exception SLA escalation')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-auto-close-enabled')).toBeInTheDocument()
  })

  it('renders escalation event rows with exception key test ids', async () => {
    render(
      <QueryClientProvider client={new QueryClient()}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    const eventRow = await screen.findByTestId('procurement-exception-escalation-event-PEX-E2E-1')
    expect(eventRow).toBeInTheDocument()
    expect(eventRow).toHaveTextContent('Level 1')
  })

  it('renders auto-close preview rows with exception key test ids', async () => {
    render(
      <QueryClientProvider client={new QueryClient()}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    const rows = await screen.findAllByTestId('procurement-exception-auto-close-pending-PEX-CLOSE-1')
    expect(rows.length).toBeGreaterThan(0)
    expect(rows[0]).toHaveTextContent('50h completed')
  })

  it('returns null when user cannot manage settings', () => {
    const { container } = render(
      <QueryClientProvider client={new QueryClient()}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retryable settings error callout when settings query fails', async () => {
    vi.mocked(client.getProcurementExceptionEscalationSettings).mockRejectedValueOnce(
      new Error('settings unavailable'),
    )
    render(
      <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
