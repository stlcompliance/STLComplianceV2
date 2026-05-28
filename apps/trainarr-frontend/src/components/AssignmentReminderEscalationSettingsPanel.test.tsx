import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { AssignmentReminderEscalationSettingsPanel } from './AssignmentReminderEscalationSettingsPanel'

vi.mock('../api/client', () => ({
  getAssignmentDueReminderSettings: vi.fn(async () => ({
    isEnabled: true,
    dueSoonLeadDays: 7,
    reminderCooldownHours: 24,
    maxRemindersPerAssignment: 5,
    updatedAt: null,
  })),
  upsertAssignmentDueReminderSettings: vi.fn(),
  getPendingAssignmentDueReminders: vi.fn(async () => ({ asOfUtc: '', batchSize: 25, items: [] })),
  getAssignmentDueReminderRuns: vi.fn(async () => ({ items: [] })),
  getAssignmentEscalationSettings: vi.fn(async () => ({
    isEnabled: false,
    overdueEscalationAfterHours: 24,
    escalationCooldownHours: 48,
    maxEscalationsPerAssignment: 10,
    updatedAt: null,
  })),
  upsertAssignmentEscalationSettings: vi.fn(),
  getPendingAssignmentEscalations: vi.fn(async () => ({ asOfUtc: '', batchSize: 25, items: [] })),
  getAssignmentEscalationRuns: vi.fn(async () => ({ items: [] })),
  getAssignmentEscalationEvents: vi.fn(async () => ({ items: [] })),
}))

describe('AssignmentReminderEscalationSettingsPanel', () => {
  it('renders due reminder and escalation sections', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AssignmentReminderEscalationSettingsPanel accessToken="token" canManage />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('assignment-reminder-escalation-settings-panel')).toBeTruthy()
    expect(screen.getByTestId('due-reminder-settings-section')).toBeTruthy()
    expect(screen.getByTestId('assignment-escalation-settings-section')).toBeTruthy()
  })
})
