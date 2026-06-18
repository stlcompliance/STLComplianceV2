import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SettingsSection } from './SettingsSection'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

vi.mock('../../components/RoutArrTenantSettingsPanel', () => ({
  RoutArrTenantSettingsPanel: () => <div data-testid="routarr-tenant-settings-panel" />,
}))

function buildState(roleKey: string, isPlatformAdmin = false): RoutArrWorkspaceState {
  return {
    session: { accessToken: 'token' },
    roleKey,
    isPlatformAdmin,
  } as RoutArrWorkspaceState
}

describe('SettingsSection', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders the first-class RoutArr tenant settings panel for routarr_admin', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState('routarr_admin')} />
      </QueryClientProvider>,
    )

    expect(screen.getByTestId('routarr-settings-admin-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('routarr-tenant-settings-panel')).toBeInTheDocument()
  })

  it('renders nothing for unauthorized tenant members', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    const { container } = render(
      <QueryClientProvider client={queryClient}>
        <SettingsSection state={buildState('tenant_member')} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
