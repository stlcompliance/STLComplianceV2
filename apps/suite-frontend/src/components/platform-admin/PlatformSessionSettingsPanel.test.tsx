import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { PlatformSessionSettingsPanel } from './PlatformSessionSettingsPanel'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformSessionSettings: vi.fn(),
  upsertPlatformSessionSettings: vi.fn(),
}))

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <PlatformSessionSettingsPanel />
    </QueryClientProvider>,
  )
}

describe('PlatformSessionSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders settings and saves updates', async () => {
    vi.mocked(nexarr.getPlatformSessionSettings).mockResolvedValue({
      accessTokenMinutes: 45,
      refreshTokenDays: 14,
      rememberedRefreshTokenDays: 60,
      requirePlatformAdminMfa: false,
      passwordMinLength: 14,
      requirePasswordComplexity: true,
      updatedAt: '2026-06-02T12:00:00Z',
    })
    vi.mocked(nexarr.upsertPlatformSessionSettings).mockResolvedValue({
      accessTokenMinutes: 90,
      refreshTokenDays: 21,
      rememberedRefreshTokenDays: 120,
      requirePlatformAdminMfa: true,
      passwordMinLength: 16,
      requirePasswordComplexity: false,
      updatedAt: '2026-06-02T12:05:00Z',
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('platform-session-access-minutes')).toHaveValue(45)
    })

    fireEvent.change(screen.getByTestId('platform-session-access-minutes'), {
      target: { value: '90' },
    })
    fireEvent.change(screen.getByTestId('platform-session-refresh-days'), {
      target: { value: '21' },
    })
    fireEvent.change(screen.getByTestId('platform-session-remembered-days'), {
      target: { value: '120' },
    })
    fireEvent.change(screen.getByTestId('platform-session-password-min-length'), {
      target: { value: '16' },
    })
    fireEvent.click(screen.getByTestId('platform-session-require-admin-mfa'))
    fireEvent.click(screen.getByTestId('platform-session-require-password-complexity'))
    fireEvent.click(screen.getByTestId('platform-session-save'))

    await waitFor(() => {
      expect(nexarr.upsertPlatformSessionSettings).toHaveBeenCalledWith({
        accessTokenMinutes: 90,
        refreshTokenDays: 21,
        rememberedRefreshTokenDays: 120,
        requirePlatformAdminMfa: true,
        passwordMinLength: 16,
        requirePasswordComplexity: false,
      })
    })
    expect(await screen.findByText('Saved.')).toBeInTheDocument()
  })

  it('renders an API error callout when settings fail to load', async () => {
    vi.mocked(nexarr.getPlatformSessionSettings).mockRejectedValueOnce(new Error('settings unavailable'))

    renderPanel()

    expect(await screen.findByText('settings unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry settings' })).toBeInTheDocument()
  })
})
