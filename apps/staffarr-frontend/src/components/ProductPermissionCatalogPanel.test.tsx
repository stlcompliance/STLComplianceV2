import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ProductPermissionCatalogPanel } from './ProductPermissionCatalogPanel'

const catalog = [
  {
    permissionTemplateId: 'perm-1',
    productKey: 'maintainarr',
    permissionKey: 'maintainarr.work_orders.close',
    label: 'Close work orders',
    description: 'Allows closing completed work orders.',
    scope: 'site',
    sensitivity: 'standard',
    status: 'active',
    lastSyncedAt: new Date().toISOString(),
  },
]

describe('ProductPermissionCatalogPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders catalog entries and filter controls', () => {
    const onFilterChange = vi.fn()

    render(
      <ProductPermissionCatalogPanel
        productKeyFilter="maintainarr"
        catalog={catalog}
        isLoading={false}
        isError={false}
        readErrorMessage={null}
        onRetryRead={vi.fn()}
        onProductKeyFilterChange={onFilterChange}
      />,
    )

    expect(screen.getByText('Product permission catalog')).toBeTruthy()
    expect(screen.getByText('Close work orders')).toBeTruthy()
    expect(screen.getByText(/maintainarr.work_orders.close/i)).toBeTruthy()
    expect(screen.getByText(/Last synced/i)).toBeTruthy()

    fireEvent.change(screen.getByLabelText('Filter by product key'), {
      target: { value: 'routarr' },
    })
    expect(onFilterChange).toHaveBeenCalledWith('routarr')
  })

  it('renders retryable error callout', () => {
    const onRetryRead = vi.fn()
    render(
      <ProductPermissionCatalogPanel
        productKeyFilter=""
        catalog={[]}
        isLoading={false}
        isError
        readErrorMessage="catalog read failed"
        onRetryRead={onRetryRead}
        onProductKeyFilterChange={vi.fn()}
      />,
    )

    expect(screen.getByText('Product permission catalog unavailable')).toBeTruthy()
    expect(screen.getByText('catalog read failed')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry catalog' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })
})
