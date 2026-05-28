import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { RecertificationSettingsPanel } from './RecertificationSettingsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getRecertificationSettings: vi.fn(),
    upsertRecertificationSettings: vi.fn(),
    getRecertificationAssignmentRuns: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <RecertificationSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('RecertificationSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and recent runs', async () => {
    vi.mocked(client.getRecertificationSettings).mockResolvedValue({
      isEnabled: true,
      leadDays: 45,
      updatedAt: '2026-05-28T00:00:00Z',
    })
    vi.mocked(client.getRecertificationAssignmentRuns).mockResolvedValue({
      items: [
        {
          runId: 'run-1',
          qualificationIssueId: '11111111-1111-1111-1111-111111111111',
          trainingAssignmentId: '22222222-2222-2222-2222-222222222222',
          outcome: 'assigned',
          skipReason: null,
          processedAt: '2026-05-28T10:00:00Z',
        },
      ],
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('recertification-enabled')).toBeChecked()
    })
    expect(screen.getByTestId('recertification-lead-days')).toHaveValue(45)
    expect(screen.getByTestId('recertification-runs-list')).toBeInTheDocument()
    expect(screen.getByText('assigned')).toBeInTheDocument()
  })
})
