import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      id,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      id: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
    }) => (
      <label htmlFor={id} className="block text-sm text-[var(--color-text-secondary)]">
        {label}
        <select
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          data-testid={testId}
          className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
        >
          <option value="">{placeholder ?? `Select ${label.toLowerCase()}…`}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

import * as nexarr from '../../api/nexarrClient'
import { ProductCatalogAdminPanel } from './ProductCatalogAdminPanel'

vi.mock('../../api/nexarrClient', () => ({
  listProducts: vi.fn(),
  getProduct: vi.fn(),
  createProduct: vi.fn(),
  updateProduct: vi.fn(),
  enableProduct: vi.fn(),
  disableProduct: vi.fn(),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ProductCatalogAdminPanel />
    </QueryClientProvider>,
  )
}

describe('ProductCatalogAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable callout when products query fails', async () => {
    vi.mocked(nexarr.listProducts).mockRejectedValueOnce(new Error('products unavailable'))

    renderPanel()

    expect(await screen.findByText('products unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry products' })).toBeTruthy()
  })

  it('can disable and re-enable the selected product', async () => {
    vi.mocked(nexarr.listProducts).mockResolvedValue({
      items: [
        {
          productKey: 'nexarr',
          displayName: 'NexArr',
          sortOrder: 10,
          isActive: true,
          productCategory: 'platform',
          productOwner: 'STL Compliance',
          productStatus: 'available',
          canonicalCallbackPath: '/auth/nexarr/callback',
          apiBaseUrl: 'https://api.example.com/nexarr',
          healthUrl: 'https://health.example.com/nexarr',
          serviceAudience: 'stl:nexarr:api',
          marketingUrl: 'https://example.com/nexarr',
          documentationUrl: 'https://docs.example.com/nexarr',
          supportUrl: 'https://support.example.com',
          environmentKey: 'local',
          entitlementDependencyRules: 'tenant-product-entitlement-required',
        },
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          sortOrder: 20,
          isActive: false,
          productCategory: 'platform',
          productOwner: 'STL Compliance',
          productStatus: 'disabled',
          canonicalCallbackPath: '/auth/nexarr/callback',
          apiBaseUrl: 'https://api.example.com/staffarr',
          healthUrl: 'https://health.example.com/staffarr',
          serviceAudience: 'stl:staffarr:api',
          marketingUrl: 'https://example.com/staffarr',
          documentationUrl: 'https://docs.example.com/staffarr',
          supportUrl: 'https://support.example.com',
          environmentKey: 'local',
          entitlementDependencyRules: 'tenant-product-entitlement-required',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getProduct).mockImplementation(async (productKey) => ({
      productKey,
      displayName: productKey === 'nexarr' ? 'NexArr' : 'StaffArr',
      sortOrder: productKey === 'nexarr' ? 10 : 20,
      isActive: productKey !== 'staffarr',
      productCategory: 'platform',
      productOwner: 'STL Compliance',
      productStatus: productKey === 'nexarr' ? 'available' : 'disabled',
      canonicalCallbackPath: '/auth/nexarr/callback',
      apiBaseUrl: `https://api.example.com/${productKey}`,
      healthUrl: `https://health.example.com/${productKey}`,
      serviceAudience: `stl:${productKey}:api`,
      marketingUrl: `https://example.com/${productKey}`,
      documentationUrl: `https://docs.example.com/${productKey}`,
      supportUrl: 'https://support.example.com',
      environmentKey: 'local',
      entitlementDependencyRules: 'tenant-product-entitlement-required',
    }))
    vi.mocked(nexarr.updateProduct).mockResolvedValue({
      productKey: 'nexarr',
      displayName: 'NexArr',
      sortOrder: 10,
      isActive: true,
      productCategory: 'platform',
      productOwner: 'STL Compliance',
      productStatus: 'available',
      canonicalCallbackPath: '/auth/nexarr/callback',
      apiBaseUrl: 'https://api.example.com/nexarr',
      healthUrl: 'https://health.example.com/nexarr',
      serviceAudience: 'stl:nexarr:api',
      marketingUrl: 'https://example.com/nexarr',
      documentationUrl: 'https://docs.example.com/nexarr',
      supportUrl: 'https://support.example.com',
      environmentKey: 'local',
      entitlementDependencyRules: 'tenant-product-entitlement-required',
    })
    vi.mocked(nexarr.disableProduct).mockResolvedValue({
      productKey: 'nexarr',
      displayName: 'NexArr',
      sortOrder: 10,
      isActive: false,
      productCategory: 'platform',
      productOwner: 'STL Compliance',
      productStatus: 'disabled',
      canonicalCallbackPath: '/auth/nexarr/callback',
      apiBaseUrl: 'https://api.example.com/nexarr',
      healthUrl: 'https://health.example.com/nexarr',
      serviceAudience: 'stl:nexarr:api',
      marketingUrl: 'https://example.com/nexarr',
      documentationUrl: 'https://docs.example.com/nexarr',
      supportUrl: 'https://support.example.com',
      environmentKey: 'local',
      entitlementDependencyRules: 'tenant-product-entitlement-required',
    })
    vi.mocked(nexarr.enableProduct).mockResolvedValue({
      productKey: 'staffarr',
      displayName: 'StaffArr',
      sortOrder: 20,
      isActive: true,
      productCategory: 'platform',
      productOwner: 'STL Compliance',
      productStatus: 'available',
      canonicalCallbackPath: '/auth/nexarr/callback',
      apiBaseUrl: 'https://api.example.com/staffarr',
      healthUrl: 'https://health.example.com/staffarr',
      serviceAudience: 'stl:staffarr:api',
      marketingUrl: 'https://example.com/staffarr',
      documentationUrl: 'https://docs.example.com/staffarr',
      supportUrl: 'https://support.example.com',
      environmentKey: 'local',
      entitlementDependencyRules: 'tenant-product-entitlement-required',
    })

    renderPanel()

    await screen.findByRole('option', { name: 'NexArr (nexarr)' })
    fireEvent.change(screen.getByLabelText('Product to edit'), { target: { value: 'nexarr' } })
    expect(await screen.findByText('Service audience')).toBeTruthy()
    expect(screen.getByText('stl:nexarr:api')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Disable product' }))
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Disable product' }))

    await waitFor(() => {
      expect(nexarr.disableProduct).toHaveBeenCalledWith('nexarr')
    })

    fireEvent.change(screen.getByLabelText('Product to edit'), { target: { value: 'staffarr' } })
    expect(await screen.findByText('stl:staffarr:api')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Enable product' }))
    fireEvent.click(within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Enable product' }))

    await waitFor(() => {
      expect(nexarr.enableProduct).toHaveBeenCalledWith('staffarr')
    })
  })
})
