import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ProductWorkspaceFrame } from './ProductWorkspaceFrame'
import { buildThemePreferenceStorageKey } from './theme'

describe('ProductWorkspaceFrame', () => {
  afterEach(() => {
    cleanup()
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    document.documentElement.classList.remove('dark', 'light')
    document.documentElement.style.colorScheme = ''
    vi.unstubAllGlobals()
  })

  it('redirects to NexArr login with callback context when no session is present', async () => {
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'http://localhost:5175/people?tab=roles',
      assign,
    })

    render(
      <ProductWorkspaceFrame
        productName="StaffArr"
        productKey="staffarr"
        suiteHomeUrl="http://localhost:5174/app"
        workspaceSession={null}
      >
        <p>Workspace content</p>
      </ProductWorkspaceFrame>,
    )

    expect(screen.getByText('Redirecting to sign in')).toBeInTheDocument()
    expect(screen.queryByText('Workspace content')).not.toBeInTheDocument()
    await waitFor(() => {
      expect(assign).toHaveBeenCalledWith(
        'http://localhost:5174/login?productKey=staffarr&callbackUrl=http%3A%2F%2Flocalhost%3A5175%2Fpeople%3Ftab%3Droles',
      )
    })
  })

  it('renders shell chrome when session bootstrap succeeds', () => {
    render(
      <MemoryRouter>
        <ProductWorkspaceFrame
          productName="StaffArr"
          productKey="staffarr"
          workspaceSubtitle="People, org, and readiness"
          suiteHomeUrl="/app"
          entitlements={['staffarr', 'trainarr']}
          onSignOut={() => undefined}
          workspaceSession={{
            userId: 'user-1',
            tenantId: 'tenant-1',
            userDisplayName: 'Demo Admin',
            tenantDisplayName: 'STL Demo Tenant',
            tenantSlug: 'demo-stl',
          }}
        >
          <p>Workspace content</p>
        </ProductWorkspaceFrame>
      </MemoryRouter>,
    )

    expect(screen.getByRole('img', { name: 'StaffArr logo' })).toBeInTheDocument()
    expect(screen.getByRole('img', { name: 'STL Compliance logo' })).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /demo admin/i }))
    expect(screen.getByRole('menu', { name: 'Account menu' })).toBeInTheDocument()
    expect(screen.getByRole('menuitem', { name: 'Preferences' })).toHaveAttribute(
      'href',
      '/app/staffarr/preferences',
    )
    expect(screen.getByRole('menuitem', { name: 'Sign out' })).toBeInTheDocument()
    expect(screen.getByText('Workspace content')).toBeInTheDocument()
  })

  it('stores theme changes per workspace user and tenant', () => {
    render(
      <MemoryRouter>
        <ProductWorkspaceFrame
          productName="StaffArr"
          productKey="staffarr"
          workspaceSession={{
            userId: 'user-1',
            tenantId: 'tenant-1',
            userDisplayName: 'Demo Admin',
            tenantDisplayName: 'STL Demo Tenant',
            tenantSlug: 'demo-stl',
          }}
        >
          <p>Workspace content</p>
        </ProductWorkspaceFrame>
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Switch to light mode' }))

    expect(document.documentElement.dataset.theme).toBe('light')
    expect(
      localStorage.getItem(
        buildThemePreferenceStorageKey({ userId: 'user-1', tenantId: 'tenant-1' }),
      ),
    ).toBe('light')
    expect(
      localStorage.getItem(
        buildThemePreferenceStorageKey({ userId: 'user-2', tenantId: 'tenant-1' }),
      ),
    ).toBeNull()
  })

  it('opens product AI help and sends route-aware requests through the configured API base', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          sessionId: 'session-1',
          messageId: 'message-1',
          outcome: 'success',
          answer: 'Review the readiness panel before committing.',
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )
    vi.stubGlobal('fetch', fetchMock)

    render(
      <MemoryRouter initialEntries={['/people']}>
        <ProductWorkspaceFrame
          productName="StaffArr"
          productKey="staffarr"
          workspaceSubtitle="People, org, and readiness"
          entitlements={['staffarr']}
          productLaunchUrls={{
            staffarr: 'https://app.stlcompliance.com/staffarr/launch',
          }}
          navItems={[{ label: 'Roles', to: '/roles' }]}
          aiAssistance={{ apiBase: 'https://staffarr.example', accessToken: 'token-1' }}
          workspaceSession={{
            userId: 'user-1',
            tenantId: 'tenant-1',
            userDisplayName: 'Demo Admin',
            tenantDisplayName: 'STL Demo Tenant',
            tenantSlug: 'demo-stl',
          }}
        >
          <p>Workspace content</p>
        </ProductWorkspaceFrame>
      </MemoryRouter>,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Show hints' }))
    expect(screen.getByText('staffarr · /people')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Hide hints' })).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Message'), {
      target: { value: '  what should I check next?  ' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Send' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(1))
    const [url, init] = fetchMock.mock.calls[0]
    expect(url).toBe('https://staffarr.example/api/v1/ai/assistant/messages')
    expect(init.headers.Authorization).toBe('Bearer token-1')
    const body = JSON.parse(init.body)
    expect(body).toMatchObject({
      productKey: 'staffarr',
      surface: 'product-shell',
      route: '/people',
      category: 'guidance',
      message: 'what should I check next?',
      pageContext: {
        productName: 'StaffArr',
        workspaceSubtitle: 'People, org, and readiness',
        tenant: 'STL Demo Tenant',
      },
    })
    expect(body.pageContext.navigationLinks).toContainEqual(
      expect.objectContaining({
        label: 'StaffArr roles',
        productKey: 'staffarr',
        route: '/roles',
        href: 'https://app.stlcompliance.com/staffarr/roles',
      }),
    )

    expect(await screen.findByText('Review the readiness panel before committing.')).toBeInTheDocument()
  })

  it('shows entitlement denial messaging', () => {
    render(
      <ProductWorkspaceFrame
        productName="TrainArr"
        productKey="trainarr"
        workspaceSession={null}
        bootstrapError="forbidden"
      >
        <p>Workspace content</p>
      </ProductWorkspaceFrame>,
    )

    expect(screen.getByText('Access denied')).toBeInTheDocument()
    expect(screen.getByText(/not entitled to TrainArr/i)).toBeInTheDocument()
  })

  it('redirects expired workspace sessions through NexArr login', async () => {
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'http://localhost:5176/assignments',
      assign,
    })

    render(
      <ProductWorkspaceFrame
        productName="TrainArr"
        productKey="trainarr"
        suiteHomeUrl="http://localhost:5174/app"
        workspaceSession={null}
        bootstrapError="expired"
      >
        <p>Workspace content</p>
      </ProductWorkspaceFrame>,
    )

    expect(screen.getByText('Redirecting to sign in')).toBeInTheDocument()
    await waitFor(() => {
      expect(assign).toHaveBeenCalledWith(
        'http://localhost:5174/login?productKey=trainarr&callbackUrl=http%3A%2F%2Flocalhost%3A5176%2Fassignments',
      )
    })
  })
})
