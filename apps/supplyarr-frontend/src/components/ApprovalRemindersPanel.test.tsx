import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ApprovalRemindersPanel } from './ApprovalRemindersPanel'

vi.mock('../api/client', () => ({
  getApprovalRemindersDashboard: vi.fn().mockResolvedValue({
    overdueCount: 1,
    pendingCount: 1,
    items: [
      {
        reminderStateId: '00000000-0000-0000-0000-000000000001',
        subjectType: 'purchase_request',
        subjectId: '00000000-0000-0000-0000-000000000002',
        documentKey: 'PR-001',
        title: 'Test PR',
        documentStatus: 'submitted',
        supplierId: null,
        pendingSince: '2026-01-01T00:00:00Z',
        lastReminderSentAt: null,
        reminderCount: 0,
        hoursPending: 48,
        isOverdue: true,
      },
    ],
  }),
}))

describe('ApprovalRemindersPanel', () => {
  it('renders dashboard when user can read', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ApprovalRemindersPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('approval-reminders-panel')).toBeInTheDocument()
    expect(await screen.findByText(/PR-001 · Test PR/i)).toBeInTheDocument()
  })

  it('returns null when user cannot read', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <ApprovalRemindersPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )
    expect(container.firstChild).toBeNull()
  })
})
