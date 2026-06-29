import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { VendorCatalogApiPanel } from './VendorCatalogApiPanel'
import { syncVendorCatalogApi } from '../api/client'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
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

vi.mock('../api/client', async () => {
  const actual = await vi.importActual<typeof import('../api/client')>('../api/client')
  return {
    ...actual,
    syncVendorCatalogApi: vi.fn(),
  }
})

const mockSyncVendorCatalogApi = vi.mocked(syncVendorCatalogApi)

function renderPanel() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })
  const invalidateSpy = vi.spyOn(queryClient, 'invalidateQueries')

  render(
    <QueryClientProvider client={queryClient}>
      <VendorCatalogApiPanel
        accessToken="token"
        canManage
        parts={[
          {
            partId: 'part-1',
            partKey: 'filter-001',
            catalogId: null,
            catalogKey: null,
            displayName: 'Primary Filter',
            description: '',
            categoryKey: 'filters',
            unitOfMeasure: 'each',
            manufacturerName: 'Fleet OEM',
            manufacturerPartNumber: 'FLT-001',
            status: 'active',
            reorderPoint: null,
            reorderQuantity: null,
            manufacturerAliases: [],
            vendorLinks: [
              {
                linkId: 'link-1',
                partyId: 'vendor-1',
                partyKey: 'midwest-fleet',
                partyDisplayName: 'Midwest Fleet Parts & Service',
                vendorPartNumber: 'V-FLT-001',
                isPreferred: true,
                catalogUnitPrice: 12.5,
                catalogCurrencyCode: 'USD',
                catalogMinimumOrderQuantity: 5,
                catalogLeadTimeDays: 10,
                catalogQuantityAvailable: 20,
                catalogAvailabilityStatus: 'in_stock',
                createdAt: '2026-05-27T00:00:00Z',
              },
            ],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        vendors={[
          {
            partyId: 'vendor-1',
            displayName: 'Midwest Fleet Parts & Service',
            partyKey: 'midwest-fleet',
          },
        ]}
      />
    </QueryClientProvider>,
  )

  return { invalidateSpy }
}

describe('VendorCatalogApiPanel', () => {
  it('syncs a supplier source payload and refreshes the workspace caches', async () => {
    mockSyncVendorCatalogApi.mockResolvedValue({
      syncType: 'vendor_catalog_api',
      dryRun: true,
      success: true,
      itemsRead: 1,
      itemsAccepted: 1,
      itemsApplied: 0,
      issues: [],
    })

    const { invalidateSpy } = renderPanel()

    expect(screen.getByTestId('vendor-catalog-api-panel')).toBeInTheDocument()
    expect(screen.getByText('1 current link')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Supplier source JSON payload'), {
      target: {
        value: JSON.stringify(
          [
            {
              partKey: 'filter-001',
              vendorPartNumber: 'V-FLT-001',
              isPreferred: true,
              catalogUnitPrice: 13.25,
              catalogCurrencyCode: 'USD',
              catalogMinimumOrderQuantity: 5,
              catalogLeadTimeDays: 11,
              catalogQuantityAvailable: 18,
              catalogAvailabilityStatus: 'in_stock',
            },
          ],
          null,
          2,
        ),
      },
    })

    fireEvent.click(screen.getByRole('button', { name: 'Sync feed' }))

    await waitFor(() => {
      expect(mockSyncVendorCatalogApi).toHaveBeenCalledWith('token', {
        vendorPartyKey: 'midwest-fleet',
        dryRun: true,
        items: [
          {
            partKey: 'filter-001',
            vendorPartNumber: 'V-FLT-001',
            isPreferred: true,
            catalogUnitPrice: 13.25,
            catalogCurrencyCode: 'USD',
            catalogMinimumOrderQuantity: 5,
            catalogLeadTimeDays: 11,
            catalogQuantityAvailable: 18,
            catalogAvailabilityStatus: 'in_stock',
          },
        ],
      })
    })

    await waitFor(() => {
      expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['supplyarr-parts'] })
      expect(invalidateSpy).toHaveBeenCalledWith({ queryKey: ['supplyarr-vendors'] })
    })

    expect(await screen.findByText('Sync succeeded')).toBeInTheDocument()
    expect(screen.getByText(/1 rows read/)).toBeInTheDocument()
  })
})
