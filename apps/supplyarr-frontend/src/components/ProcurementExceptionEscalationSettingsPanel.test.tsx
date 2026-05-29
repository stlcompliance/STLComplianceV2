import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ProcurementExceptionEscalationSettingsPanel } from './ProcurementExceptionEscalationSettingsPanel'

vi.mock('../api/client', () => ({
  getProcurementExceptionEscalationSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    escalationCooldownHours: 24,
    maxEscalationsPerException: 5,
    notifyOnProcurementExceptionSlaEscalation: true,
    updatedAt: null,
  }),
  getPendingProcurementExceptionEscalations: vi.fn().mockResolvedValue({
    asOfUtc: new Date().toISOString(),
    batchSize: 25,
    items: [],
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

  it('returns null when user cannot manage settings', () => {
    const { container } = render(
      <QueryClientProvider client={new QueryClient()}>
        <ProcurementExceptionEscalationSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
