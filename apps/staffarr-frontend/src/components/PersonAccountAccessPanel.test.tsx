import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { PersonAccountAccessPanel } from './PersonAccountAccessPanel'
import type {
  PersonAccountAccessActionResponse,
  PersonAccountAccessSummaryResponse,
} from '../api/types'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  getPersonAccountAccess: vi.fn(),
  provisionPersonAccount: vi.fn(),
  updatePersonLoginEmail: vi.fn(),
  requestPersonPasswordReset: vi.fn(),
  resetPersonMfa: vi.fn(),
  disablePersonLogin: vi.fn(),
  enablePersonLogin: vi.fn(),
}))

const summary: PersonAccountAccessSummaryResponse = {
  personId: 'person-1',
  workEmail: 'worker@example.com',
  hasPlatformIdentity: false,
  hasPlatformLogin: false,
  accountState: 'no_platform_login',
  loginEmail: null,
  loginEmailMatchesWorkEmail: false,
  isEnabled: false,
  isMfaEnabled: false,
  requiresPasswordChange: false,
  launchEligible: false,
  tenantRoleSummary: 'hr_admin',
  lastLoginAt: null,
  lastProductLaunchAt: null,
  integrationAvailable: true,
  notice: null,
}

const provisionResult: PersonAccountAccessActionResponse = {
  summary,
  message: 'Platform login was provisioned for this person.',
}

const enabledSummary: PersonAccountAccessSummaryResponse = {
  ...summary,
  hasPlatformIdentity: true,
  hasPlatformLogin: true,
  accountState: 'login_enabled',
  loginEmail: 'worker@example.com',
  loginEmailMatchesWorkEmail: true,
  isEnabled: true,
  isMfaEnabled: true,
  requiresPasswordChange: false,
  launchEligible: true,
  lastLoginAt: '2026-06-25T09:15:00.000Z',
  lastProductLaunchAt: '2026-06-25T09:20:00.000Z',
}

const disabledSummary: PersonAccountAccessSummaryResponse = {
  ...summary,
  hasPlatformIdentity: true,
  hasPlatformLogin: true,
  accountState: 'login_disabled',
  loginEmail: 'worker@example.com',
  loginEmailMatchesWorkEmail: true,
  isEnabled: false,
  isMfaEnabled: false,
  requiresPasswordChange: false,
  launchEligible: false,
  lastLoginAt: '2026-06-24T11:00:00.000Z',
  lastProductLaunchAt: '2026-06-24T11:30:00.000Z',
}

function renderPanel(canManage: boolean) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <PersonAccountAccessPanel
        accessToken="access-token"
        personId="person-1"
        displayName="Worker Example"
        workEmail="worker@example.com"
        canManage={canManage}
      />
    </QueryClientProvider>,
  )
}

describe('PersonAccountAccessPanel', () => {
  beforeEach(() => {
    vi.mocked(client.getPersonAccountAccess).mockResolvedValue(summary)
    vi.mocked(client.provisionPersonAccount).mockResolvedValue(provisionResult)
    vi.mocked(client.updatePersonLoginEmail).mockResolvedValue(provisionResult)
    vi.mocked(client.requestPersonPasswordReset).mockResolvedValue(provisionResult)
    vi.mocked(client.resetPersonMfa).mockResolvedValue(provisionResult)
    vi.mocked(client.disablePersonLogin).mockResolvedValue(provisionResult)
    vi.mocked(client.enablePersonLogin).mockResolvedValue(provisionResult)
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders read-only account details when delegated actions are unavailable', async () => {
    renderPanel(false)

    await waitFor(() => {
      expect(screen.getByText('No platform login')).toBeTruthy()
    })

    expect(screen.getByText('Read only')).toBeTruthy()
    expect(screen.queryByLabelText(/Login email/i)).toBeNull()
    expect(screen.queryByRole('button', { name: /Provision login/i })).toBeNull()
  })

  it('provisions login access from the person profile panel', async () => {
    renderPanel(true)

    await waitFor(() => {
      expect(screen.getByText('No platform login')).toBeTruthy()
    })

    fireEvent.change(screen.getByLabelText('Login email', { selector: 'input[type="email"]' }), {
      target: { value: 'new.worker@example.com' },
    })
    fireEvent.change(screen.getByLabelText(/Temporary sign-in password/i), {
      target: { value: 'TempPass123!' },
    })
    fireEvent.click(screen.getByRole('button', { name: /Provision login/i }))

    await waitFor(() => {
      expect(client.provisionPersonAccount).toHaveBeenCalledWith('access-token', 'person-1', {
        loginEmail: 'new.worker@example.com',
        temporaryPassword: 'TempPass123!',
        syncWorkEmail: false,
      })
    })

    expect(screen.getByText('Platform login was provisioned for this person.')).toBeTruthy()
  })

  it('confirms and disables an enabled login', async () => {
    vi.mocked(client.getPersonAccountAccess).mockResolvedValueOnce(enabledSummary)
    vi.mocked(client.disablePersonLogin).mockResolvedValueOnce({
      summary: disabledSummary,
      message: 'Platform login was disabled for this person.',
    })

    renderPanel(true)

    await waitFor(() => {
      expect(screen.getByText('Login enabled')).toBeTruthy()
    })

    fireEvent.click(screen.getByRole('button', { name: /Disable login/i }))

    const dialog = await screen.findByRole('alertdialog')
    expect(within(dialog).getByText('Disable login for Worker Example?')).toBeTruthy()

    fireEvent.click(within(dialog).getByRole('button', { name: 'Disable login' }))

    await waitFor(() => {
      expect(client.disablePersonLogin).toHaveBeenCalledWith('access-token', 'person-1', { reason: null })
    })

    expect(screen.getByText('Platform login was disabled for this person.')).toBeTruthy()
  })

  it('re-enables a disabled login without confirmation', async () => {
    vi.mocked(client.getPersonAccountAccess).mockResolvedValueOnce(disabledSummary)
    vi.mocked(client.enablePersonLogin).mockResolvedValueOnce({
      summary: enabledSummary,
      message: 'Platform login was re-enabled for this person.',
    })

    renderPanel(true)

    await waitFor(() => {
      expect(screen.getByText('Login disabled')).toBeTruthy()
    })

    fireEvent.click(screen.getByRole('button', { name: /Re-enable login/i }))

    await waitFor(() => {
      expect(client.enablePersonLogin).toHaveBeenCalledWith('access-token', 'person-1', { reason: null })
    })

    expect(screen.getByText('Platform login was re-enabled for this person.')).toBeTruthy()
  })
})
