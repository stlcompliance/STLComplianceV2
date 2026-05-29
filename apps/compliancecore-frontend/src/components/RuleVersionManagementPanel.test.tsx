import { fireEvent, render, screen, cleanup, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { RuleVersionManagementPanel } from './RuleVersionManagementPanel'

vi.mock('../api/client', () => ({
  listRuleVersions: vi.fn().mockResolvedValue({
    items: [
      {
        rulePackId: 'pack-v2',
        packKey: 'driver_qualification',
        programKey: 'fmcsa_safety',
        programLabel: 'FMCSA Safety Compliance',
        versionNumber: 2,
        status: 'review',
        isActive: true,
        createdAt: '2026-05-28T00:00:00Z',
        updatedAt: '2026-05-28T00:00:00Z',
      },
      {
        rulePackId: 'pack-v1',
        packKey: 'driver_qualification',
        programKey: 'fmcsa_safety',
        programLabel: 'FMCSA Safety Compliance',
        versionNumber: 1,
        status: 'archived',
        isActive: true,
        createdAt: '2026-05-27T00:00:00Z',
        updatedAt: '2026-05-28T00:00:00Z',
      },
    ],
  }),
  publishRuleVersion: vi.fn().mockResolvedValue({
    rulePackId: 'pack-v2',
    packKey: 'driver_qualification',
    programKey: 'fmcsa_safety',
    programLabel: 'FMCSA Safety Compliance',
    versionNumber: 2,
    status: 'published',
    isActive: true,
    createdAt: '2026-05-28T00:00:00Z',
    updatedAt: '2026-05-28T00:00:00Z',
  }),
  rollbackRuleVersion: vi.fn(),
}))

function renderPanel(canManage = true) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <RuleVersionManagementPanel accessToken="token" canRead canManage={canManage} />
    </QueryClientProvider>,
  )
}

describe('RuleVersionManagementPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders grouped versions and publish action', async () => {
    renderPanel()

    expect(await screen.findByText(/Rule version publication/)).toBeInTheDocument()
    expect(await screen.findByText('driver_qualification')).toBeInTheDocument()
    expect(await screen.findByText(/v2/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Publish' })).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Roll back' })).not.toBeInTheDocument()
  })

  it('hides operator actions for read-only users', async () => {
    renderPanel(false)

    expect(await screen.findByText('driver_qualification')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Publish' })).not.toBeInTheDocument()
  })

  it('shows rollback for published versions beyond v1', async () => {
    const { listRuleVersions } = await import('../api/client')
    vi.mocked(listRuleVersions).mockResolvedValueOnce({
      items: [
        {
          rulePackId: 'pack-v2',
          packKey: 'driver_qualification',
          programKey: 'fmcsa_safety',
          programLabel: 'FMCSA Safety Compliance',
          versionNumber: 2,
          status: 'published',
          isActive: true,
          createdAt: '2026-05-28T00:00:00Z',
          updatedAt: '2026-05-28T00:00:00Z',
        },
      ],
    })

    renderPanel()
    expect(await screen.findByRole('button', { name: 'Roll back' })).toBeInTheDocument()
  })

  it('calls publish mutation when publish is clicked', async () => {
    const { publishRuleVersion } = await import('../api/client')

    renderPanel()
    fireEvent.click(await screen.findByRole('button', { name: 'Publish' }))
    await waitFor(() => {
      expect(publishRuleVersion).toHaveBeenCalledWith('token', 'pack-v2')
    })
  })
})
