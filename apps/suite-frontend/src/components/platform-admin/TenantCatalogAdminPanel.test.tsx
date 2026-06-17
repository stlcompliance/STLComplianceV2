import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from './TenantCatalogAdminPanel'

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
      options: Array<{ value: string; label: string; inactive?: boolean }>
      placeholder?: string
      testId?: string
    }) => (
      <label htmlFor={id} className="block text-sm text-slate-700">
        {label}
        <select
          id={id}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          data-testid={testId}
          className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
        >
          <option value="">{placeholder ?? `Select ${label.toLowerCase()}…`}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
              {option.inactive ? ' (inactive)' : ''}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

vi.mock('../../api/nexarrClient', () => ({
  listTenants: vi.fn(),
  createTenant: vi.fn(),
  updateTenant: vi.fn(),
  updateTenantStatus: vi.fn(),
}))

function renderPanel() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <TenantCatalogAdminPanel />
    </QueryClientProvider>,
  )
}

describe('TenantCatalogAdminPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable callout when tenants query fails', async () => {
    vi.mocked(nexarr.listTenants).mockRejectedValueOnce(new Error('tenants unavailable'))

    renderPanel()

    expect(await screen.findByText('tenants unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry tenants' })).toBeTruthy()
  })

  it('loads tenant options into the searchable picker and binds the selected tenant', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.listTenants).mockResolvedValue({
      items: [
        {
          tenantId: 'tenant-1',
          slug: 'demo-stl',
          displayName: 'STL Demo Tenant',
          status: 'Active',
          subscriptionTier: 'enterprise',
          billingCustomerId: 'cus_001',
          billingSubscriptionId: 'sub_001',
          billingGraceDays: 14,
          isTrial: true,
          isInternalTenant: true,
          createdAt: '2026-01-01T00:00:00Z',
          modifiedAt: '2026-01-02T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPanel()

    expect(await screen.findByRole('option', { name: /STL Demo Tenant/ })).toBeTruthy()
    expect(screen.queryByRole('option', { name: /inactive/i })).toBeNull()

    await user.selectOptions(screen.getByTestId('tenant-catalog-selected-tenant'), 'tenant-1')

    expect(screen.getByLabelText('Updated tenant display name')).toHaveValue('STL Demo Tenant')
    expect(screen.getByLabelText('Tenant lifecycle status', { selector: '#tenant-catalog-edit-status' })).toHaveValue('active')
    expect(screen.getByLabelText('Subscription tier', { selector: '#tenant-catalog-edit-subscription-tier' })).toHaveValue('enterprise')
    expect(screen.getByLabelText('Billing customer ID', { selector: '#tenant-catalog-edit-billing-customer' })).toHaveValue('cus_001')
    expect(screen.getByLabelText('Billing subscription ID', { selector: '#tenant-catalog-edit-billing-subscription' })).toHaveValue('sub_001')
    expect(screen.getByLabelText('Billing grace days', { selector: '#tenant-catalog-edit-billing-grace-days' })).toHaveValue(14)
    expect(screen.getByLabelText('Trial tenant', { selector: '#tenant-catalog-edit-is-trial' })).toBeChecked()
    expect(screen.getByLabelText('Internal/free tenant', { selector: '#tenant-catalog-edit-is-internal' })).toBeChecked()
  })

  it('submits billing readiness fields when creating a tenant', async () => {
    const user = userEvent.setup()
    vi.mocked(nexarr.listTenants).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 100,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.createTenant).mockResolvedValue({
      tenantId: 'tenant-new',
      slug: 'new-tenant',
      displayName: 'New Tenant',
      status: 'trial',
      subscriptionTier: 'enterprise',
      billingCustomerId: 'cus_new',
      billingSubscriptionId: 'sub_new',
      billingGraceDays: 21,
      isTrial: true,
      isInternalTenant: true,
      createdAt: '2026-01-03T00:00:00Z',
      modifiedAt: '2026-01-03T00:00:00Z',
    })

    renderPanel()

    await user.type(await screen.findByLabelText('New tenant slug'), 'new-tenant')
    await user.type(screen.getByLabelText('New tenant display name'), 'New Tenant')
    await user.clear(screen.getByLabelText('Subscription tier', { selector: '#tenant-catalog-create-subscription-tier' }))
    await user.type(screen.getByLabelText('Subscription tier', { selector: '#tenant-catalog-create-subscription-tier' }), 'enterprise')
    await user.type(screen.getByLabelText('Billing customer ID', { selector: '#tenant-catalog-create-billing-customer' }), 'cus_new')
    await user.type(
      screen.getByLabelText('Billing subscription ID', { selector: '#tenant-catalog-create-billing-subscription' }),
      'sub_new',
    )
    await user.clear(screen.getByLabelText('Billing grace days', { selector: '#tenant-catalog-create-billing-grace-days' }))
    await user.type(screen.getByLabelText('Billing grace days', { selector: '#tenant-catalog-create-billing-grace-days' }), '21')
    await user.click(screen.getByLabelText('Trial tenant', { selector: '#tenant-catalog-create-is-trial' }))
    await user.click(screen.getByLabelText('Internal/free tenant', { selector: '#tenant-catalog-create-is-internal' }))
    await user.click(screen.getByRole('button', { name: 'Create tenant' }))

    expect(vi.mocked(nexarr.createTenant)).toHaveBeenCalledWith({
      slug: 'new-tenant',
      displayName: 'New Tenant',
      subscriptionTier: 'enterprise',
      billingCustomerId: 'cus_new',
      billingSubscriptionId: 'sub_new',
      billingGraceDays: 21,
      isTrial: true,
      isInternalTenant: true,
    })
  }, 10000)
})
