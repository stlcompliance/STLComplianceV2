import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      testId,
      placeholder,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      testId?: string
      placeholder?: string
    }) => (
      <label>
        <span>{label}</span>
        <input
          aria-label={label}
          data-testid={testId}
          placeholder={placeholder}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <div>
          {options.map((option) => (
            <button key={option.value} type="button" onClick={() => onChange(option.value)}>
              {option.label}
            </button>
          ))}
        </div>
      </label>
    ),
  }
})

import * as nexarr from '../../api/nexarrClient'
import { ToastProvider } from '../../feedback'
import { CallbackAllowlistPage } from './CallbackAllowlistPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminProductOverview: vi.fn(),
  getPlatformAdminTenantOverview: vi.fn(),
  listCallbackAllowlist: vi.fn(),
  createCallbackAllowlistEntry: vi.fn(),
  deleteCallbackAllowlistEntry: vi.fn(),
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <ToastProvider>
        <CallbackAllowlistPage />
      </ToastProvider>
    </QueryClientProvider>,
  )
}

describe('CallbackAllowlistPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('lists, creates, and deletes callback allowlist entries', async () => {
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'nexarr',
        displayName: 'NexArr',
        isActive: true,
        activeEntitlementCount: 1,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'http://localhost:5101',
      },
    ])
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId: 'tenant-a',
          slug: 'alpha',
          displayName: 'Alpha Corp',
          status: 'Active',
          activeEntitlementCount: 1,
          membershipCount: 2,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 200,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listCallbackAllowlist).mockResolvedValue([
      {
        entryId: 'entry-1',
        productKey: 'nexarr',
        tenantId: null,
        urlPattern: 'https://app.example.com',
        patternType: 'origin',
        isActive: true,
        createdAt: '2026-05-02T00:00:00Z',
      },
    ])
    vi.mocked(nexarr.createCallbackAllowlistEntry).mockResolvedValue({
      entryId: 'entry-2',
      productKey: 'nexarr',
      tenantId: 'tenant-a',
      urlPattern: 'https://app.example.com/callback',
      patternType: 'prefix',
      isActive: true,
      createdAt: '2026-05-03T00:00:00Z',
    })
    vi.mocked(nexarr.deleteCallbackAllowlistEntry).mockResolvedValue(undefined)

    renderPage()

    await waitFor(() => {
      expect(screen.getByText('https://app.example.com')).toBeInTheDocument()
    })

    expect(screen.getByRole('button', { name: /NexArr \(nexarr\)/ })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Alpha Corp \(alpha\)/ })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Alpha Corp \(alpha\)/ }))
    fireEvent.change(screen.getByLabelText('URL pattern'), {
      target: { value: 'https://app.example.com/callback' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Add allowlist entry' }))

    await waitFor(() => {
      expect(nexarr.createCallbackAllowlistEntry).toHaveBeenCalledWith({
        productKey: 'nexarr',
        tenantId: 'tenant-a',
        urlPattern: 'https://app.example.com/callback',
        patternType: 'origin',
      })
    })

    fireEvent.click(screen.getAllByRole('button', { name: 'Delete' })[0])
    fireEvent.click(screen.getByRole('button', { name: 'Delete allowlist entry' }))

    await waitFor(() => {
      expect(nexarr.deleteCallbackAllowlistEntry).toHaveBeenCalledWith('entry-1')
    })
  })
})
