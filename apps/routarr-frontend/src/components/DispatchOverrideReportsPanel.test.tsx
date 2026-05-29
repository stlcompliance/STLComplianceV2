import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { DispatchOverrideReportsPanel } from './DispatchOverrideReportsPanel'

vi.mock('../api/client', () => ({
  getDispatchOverrideReportSummary: vi.fn().mockResolvedValue({
    generatedAt: '2026-05-29T12:00:00Z',
    scope: 'daily',
    windowStart: '2026-05-29T00:00:00Z',
    windowEnd: '2026-05-30T00:00:00Z',
    totalOverrideCount: 1,
    driverAssignmentOverrideCount: 1,
    vehicleAssignmentOverrideCount: 0,
    overrideKindCounts: [{ key: 'availability', count: 1 }],
    recentOverrides: [
      {
        auditEventId: 'audit-1',
        actorUserId: 'user-1',
        action: 'trip.assign_driver',
        targetType: 'trip',
        targetId: 'trip-1',
        result: 'person-1 (override:availability)',
        overrideKinds: ['availability'],
        occurredAt: '2026-05-29T11:00:00Z',
      },
    ],
  }),
  exportDispatchOverrideReportSummaryCsv: vi.fn(),
}))

describe('DispatchOverrideReportsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders override audit metrics and table rows', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <DispatchOverrideReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('dispatch-override-reports-panel')).toBeInTheDocument()
    expect(
      screen.getByRole('heading', { name: 'Dispatch gate override audit' }),
    ).toBeInTheDocument()
    expect(await screen.findByText('Overrides in scope')).toBeInTheDocument()
    expect(await screen.findByText('availability: 1')).toBeInTheDocument()
    expect(await screen.findByText('person-1 (override:availability)')).toBeInTheDocument()
  })

  it('returns null when user cannot read reports', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <DispatchOverrideReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
