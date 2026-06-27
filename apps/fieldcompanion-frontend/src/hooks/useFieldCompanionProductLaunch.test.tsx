import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useState } from 'react'

vi.mock('../api/client', () => ({
  createHandoff: vi.fn(),
  getLaunchContext: vi.fn(),
}))

vi.mock('../lib/FieldCompanionDeniedReasonCatalog', () => ({
  resolveDeniedReason: vi.fn(() => 'Launch denied by policy.'),
}))

vi.mock('../lib/productLaunch', () => ({
  buildFieldCompanionProductCallbackUrl: vi.fn(() => '/fieldcompanion/callback'),
}))

const originalLocation = window.location

const client = await import('../api/client')
const { resolveDeniedReason } = await import('../lib/FieldCompanionDeniedReasonCatalog')
const { buildFieldCompanionProductCallbackUrl } = await import('../lib/productLaunch')
const { useFieldCompanionProductLaunch } = await import('./useFieldCompanionProductLaunch')

function Harness({ productKey }: { productKey: string }) {
  const launch = useFieldCompanionProductLaunch({
    accessToken: 'access-token',
    suiteHomeUrl: '/suite',
    productLaunchUrls: {
      maintainarr: '/launch/maintainarr',
    },
  })
  const [status, setStatus] = useState('idle')
  const [error, setError] = useState<string | null>(null)

  return (
    <div>
      <button
        type="button"
        onClick={() => {
          setStatus('pending')
          setError(null)
          void launch
            .mutateAsync(productKey)
            .then((result) => {
              setStatus(JSON.stringify(result))
            })
            .catch((launchError: unknown) => {
              setStatus('error')
              setError(launchError instanceof Error ? launchError.message : String(launchError))
            })
        }}
      >
        Launch
      </button>
      <div data-testid="launch-status">{status}</div>
      {error ? <div data-testid="launch-error">{error}</div> : null}
    </div>
  )
}

describe('useFieldCompanionProductLaunch', () => {
  beforeEach(() => {
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: {
        ...originalLocation,
        assign: vi.fn(),
      },
    })
  })

  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
    Object.defineProperty(window, 'location', {
      configurable: true,
      value: originalLocation,
    })
  })

  function renderHarness(productKey: string) {
    const queryClient = new QueryClient({
      defaultOptions: {
        mutations: { retry: false },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <Harness productKey={productKey} />
      </QueryClientProvider>,
    )
  }

  it('returns the current product without redirecting through NexArr', async () => {
    renderHarness('fieldcompanion')

    fireEvent.click(screen.getByRole('button', { name: 'Launch' }))

    await waitFor(() => {
      expect(screen.getByTestId('launch-status')).toHaveTextContent('"mode":"current"')
    })

    expect(client.getLaunchContext).not.toHaveBeenCalled()
    expect(client.createHandoff).not.toHaveBeenCalled()
    expect(window.location.assign).not.toHaveBeenCalled()
  })

  it('requests a scoped handoff when another product is launched', async () => {
    vi.mocked(client.getLaunchContext).mockResolvedValue({
      canLaunch: true,
      denialReasonCode: null,
    } as never)
    vi.mocked(client.createHandoff).mockResolvedValue({
      launchUrl: 'https://example.test/maintainarr/launch?handoff=abc',
    } as never)

    renderHarness('maintainarr')

    fireEvent.click(screen.getByRole('button', { name: 'Launch' }))

    await waitFor(() => {
      expect(screen.getByTestId('launch-status')).toHaveTextContent('"mode":"handoff"')
    })

    expect(client.getLaunchContext).toHaveBeenCalledWith('access-token', 'maintainarr')
    expect(buildFieldCompanionProductCallbackUrl).toHaveBeenCalledWith(
      'maintainarr',
      '/suite',
      { maintainarr: '/launch/maintainarr' },
    )
    expect(client.createHandoff).toHaveBeenCalledWith(
      'access-token',
      'maintainarr',
      '/fieldcompanion/callback',
    )
    expect(window.location.assign).toHaveBeenCalledWith(
      'https://example.test/maintainarr/launch?handoff=abc',
    )
  })

  it('surfaces a denial message when the target product is unavailable', async () => {
    vi.mocked(client.getLaunchContext).mockResolvedValue({
      canLaunch: false,
      denialReasonCode: 'tenant_suspended',
    } as never)

    renderHarness('maintainarr')

    fireEvent.click(screen.getByRole('button', { name: 'Launch' }))

    await waitFor(() => {
      expect(screen.getByTestId('launch-error')).toHaveTextContent('Launch denied by policy.')
    })

    expect(resolveDeniedReason).toHaveBeenCalledWith(
      { reasonCode: 'tenant_suspended' },
      'Product launch is not permitted.',
    )
    expect(client.createHandoff).not.toHaveBeenCalled()
    expect(window.location.assign).not.toHaveBeenCalled()
  })
})
