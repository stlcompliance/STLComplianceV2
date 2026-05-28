import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ApprovalReminderSettingsPanel } from './ApprovalReminderSettingsPanel'

vi.mock('../api/client', () => ({
  getApprovalReminderSettings: vi.fn().mockResolvedValue({
    isEnabled: false,
    prReminderAfterHours: 24,
    poReminderAfterHours: 24,
    reminderCooldownHours: 24,
    maxRemindersPerSubject: 10,
    notifyOnPrApprovalReminder: true,
    notifyOnPoApprovalReminder: true,
    updatedAt: null,
  }),
  getPendingApprovalReminders: vi.fn().mockResolvedValue({ asOfUtc: '', batchSize: 25, items: [] }),
  getApprovalReminderRuns: vi.fn().mockResolvedValue({ items: [] }),
  upsertApprovalReminderSettings: vi.fn(),
}))

describe('ApprovalReminderSettingsPanel', () => {
  it('renders when user can manage', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ApprovalReminderSettingsPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )
    expect(screen.getByTestId('approval-reminder-settings-panel')).toBeInTheDocument()
    expect(screen.getByText('Approval reminder worker')).toBeInTheDocument()
  })

  it('returns null when user cannot manage', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <ApprovalReminderSettingsPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )
    expect(container.firstChild).toBeNull()
  })
})
