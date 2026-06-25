import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { LaunchPadPage } from './LaunchPadPage'
import type { MeResponse, NavigationItem } from '../api/types'
import * as nexarr from '../api/nexarrClient'
import { HintsPreferenceProvider } from '@stl/shared-ui'

const mutateMock = vi.fn()

vi.mock('../hooks/useProductLaunch', () => ({
  useProductLaunch: () => ({
    isPending: false,
    isError: false,
    error: null,
    mutate: mutateMock,
  }),
}))

vi.mock('../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/nexarrClient')>()
  return {
    ...actual,
    sendAiAssistantMessage: vi.fn(),
  }
})

const baseMe: MeResponse = {
  userId: 'user-1',
  email: 'alex@example.com',
  displayName: 'Alex Operator',
  isPlatformAdmin: false,
  requiresPasswordChange: false,
  tenantId: 'tenant-a',
  tenantSlug: 'alpha',
  tenantDisplayName: 'Alpha Corp',
  entitlements: [],
}

const navigationProducts: NavigationItem[] = [
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    routePath: '/app/staffarr',
    sortOrder: 1,
    surfaces: [
      {
        surfaceKey: 'launch',
        label: 'Launch',
        relativePath: 'launch',
        iconKey: 'launch',
        sortOrder: 0,
        isEnabled: true,
        permissionHint: null,
      },
      {
        surfaceKey: 'overview',
        label: 'Overview',
        relativePath: '',
        iconKey: 'dashboard',
        sortOrder: 0,
        isEnabled: true,
        permissionHint: null,
      },
      {
        surfaceKey: 'roles',
        label: 'Roles',
        relativePath: 'roles',
        iconKey: 'auth',
        sortOrder: 10,
        isEnabled: true,
        permissionHint: null,
      },
    ],
  },
]

describe('LaunchPadPage', () => {
  afterEach(() => {
    cleanup()
    mutateMock.mockReset()
    vi.clearAllMocks()
  })

  it('launches available products and surfaces an AI deep link', async () => {
    vi.mocked(nexarr.sendAiAssistantMessage).mockResolvedValueOnce({
      sessionId: 'session-1',
      messageId: 'message-1',
      outcome: 'success',
      answer: 'Open StaffArr Roles to review the user assignment.',
      requiredReviewReasons: [],
    })

    render(
      <MemoryRouter>
        <LaunchPadPage me={baseMe} navigationProducts={navigationProducts} />
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: /Launch StaffArr/i }))
    expect(mutateMock).toHaveBeenCalledWith('staffarr')

    fireEvent.change(screen.getByLabelText('What do you need to do?'), {
      target: { value: 'I need to review a user assignment.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Ask NexArr' }))

    await waitFor(() => {
      expect(nexarr.sendAiAssistantMessage).toHaveBeenCalled()
    })

    expect(await screen.findByRole('link', { name: /Open StaffArr roles/i })).toHaveAttribute(
      'href',
      'http://localhost:5175/roles',
    )
  })

  it('hides optional launchpad guidance when hints are disabled', () => {
    render(
      <HintsPreferenceProvider showHints={false} setShowHints={() => undefined}>
        <MemoryRouter>
          <LaunchPadPage me={baseMe} navigationProducts={navigationProducts} />
        </MemoryRouter>
      </HintsPreferenceProvider>,
    )

    expect(
      screen.getByText('Select a product to launch. NexArr keeps login, tenant, and launch control centralized.'),
    ).toBeInTheDocument()
    expect(
      screen.queryByText(/ask the helper and it will point you to the relevant page or section/i),
    ).toBeNull()
  })

  it('shows a safe fallback when assistant guidance fails', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    vi.mocked(nexarr.sendAiAssistantMessage).mockRejectedValueOnce(new Error('unexpected boom'))

    render(
      <MemoryRouter>
        <LaunchPadPage me={baseMe} navigationProducts={navigationProducts} />
      </MemoryRouter>,
    )

    fireEvent.change(screen.getByLabelText('What do you need to do?'), {
      target: { value: 'I need help.' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Ask NexArr' }))

    expect(
      await screen.findByText('AI assistance is temporarily unavailable. Please try again.'),
    ).toBeInTheDocument()
    expect(consoleError).toHaveBeenCalled()

    consoleError.mockRestore()
  })
})
