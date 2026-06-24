import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { RoutArrTenantSettingsPanel } from './RoutArrTenantSettingsPanel'

vi.mock('../api/client', () => ({
  getEditableRoutArrTenantSettings: vi.fn(),
  getRoutArrTenantSettingAuditHistory: vi.fn(),
  getRoutArrTenantSettingsOptions: vi.fn(),
  previewRoutArrTenantSettings: vi.fn(),
  resetRoutArrTenantSettingGroup: vi.fn(),
  updateRoutArrTenantSettingGroup: vi.fn(),
  validateRoutArrTenantSettingGroup: vi.fn(),
}))

import * as client from '../api/client'

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <RoutArrTenantSettingsPanel accessToken="token" canManage />
    </QueryClientProvider>,
  )
}

describe('RoutArrTenantSettingsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
  })

  it('asks before resetting a settings section', async () => {
    vi.mocked(client.getEditableRoutArrTenantSettings).mockResolvedValue({
      tenantId: 'tenant-1',
      version: 1,
      effectiveAt: new Date().toISOString(),
      groups: [
        {
          groupKey: 'dispatch',
          label: 'Dispatch',
          description: 'Dispatch defaults',
          lastUpdatedAt: new Date().toISOString(),
          lastUpdatedByPersonId: 'person-1',
          fields: [
            {
              settingKey: 'defaultDispatchWindow',
              label: 'Dispatch window',
              helpText: 'Default dispatch window',
              valueKind: 'text',
              options: [],
              value: 'day',
              platformDefaultValue: 'day',
              effectiveSource: 'tenantDefault',
            },
          ],
        },
      ],
      overrides: [],
    })
    vi.mocked(client.getRoutArrTenantSettingsOptions).mockResolvedValue({
      groups: [],
      scopeTypes: [],
      permissions: [],
    })
    vi.mocked(client.getRoutArrTenantSettingAuditHistory).mockResolvedValue({ items: [] } as never)
    vi.mocked(client.previewRoutArrTenantSettings).mockResolvedValue({
      tenantId: 'tenant-1',
      effectiveAt: new Date().toISOString(),
      groups: [],
      version: 1,
      overrides: [],
    })
    vi.mocked(client.resetRoutArrTenantSettingGroup).mockResolvedValue({} as never)

    renderPanel()

    expect(await screen.findByText('RoutArr settings')).toBeTruthy()
    fireEvent.click(screen.getByTestId('routarr-settings-reset-section'))

    expect(await screen.findByRole('alertdialog')).toBeTruthy()
    expect(screen.getByText(/Reset Dispatch\?/i)).toBeTruthy()

    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: /reset/i }))

    await waitFor(() => {
      expect(client.resetRoutArrTenantSettingGroup).toHaveBeenCalledWith('token', 'dispatch', {
        expectedVersion: 1,
      })
    })
  })
})
