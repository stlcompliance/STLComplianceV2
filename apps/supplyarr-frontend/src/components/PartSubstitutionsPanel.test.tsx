import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { cleanup, render, screen } from '@testing-library/react'

import { afterEach, describe, expect, it, vi } from 'vitest'

import { PartSubstitutionsPanel } from './PartSubstitutionsPanel'

const mockGetSubstitutions = vi.fn()

vi.mock('../api/client', () => ({
  getSubstitutions: (...args: unknown[]) => mockGetSubstitutions(...args),
}))

const parts = [
  {
    partId: 'part-1',
    partKey: 'P-1001',
    catalogId: null,
    catalogKey: null,
    displayName: 'Brake Pads',
    description: '',
    categoryKey: 'parts',
    unitOfMeasure: 'each',
    manufacturerName: 'Acme',
    manufacturerPartNumber: 'BP-1001',
    status: 'active',
    reorderPoint: null,
    reorderQuantity: null,
    manufacturerAliases: [],
    vendorLinks: [],
    createdAt: '2026-06-01T00:00:00Z',
    updatedAt: '2026-06-01T00:00:00Z',
  },
]

const substitutions = [
  {
    partId: 'part-1',
    partKey: 'P-1001',
    partDisplayName: 'Brake Pads',
    aliasId: 'alias-1',
    aliasKey: 'alias-1',
    manufacturerName: 'Acme',
    manufacturerPartNumber: 'BP-1001-A',
    createdAt: '2026-06-02T14:00:00Z',
  },
]

describe('PartSubstitutionsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders substitutions and a part filter', async () => {
    mockGetSubstitutions.mockResolvedValue(substitutions)

    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <PartSubstitutionsPanel
          accessToken="token"
          parts={parts}
          canRead={true}
          selectedPartId=""
          onSelectedPartIdChange={() => undefined}
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Part substitutions')).toBeTruthy()
    expect(screen.getByLabelText('Part filter')).toBeTruthy()
    expect(await screen.findByText('P-1001 · Brake Pads')).toBeTruthy()
    expect(screen.getByText('alias')).toBeTruthy()
  })

  it('hides the panel when the user cannot read parts', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={client}>
        <PartSubstitutionsPanel
          accessToken="token"
          parts={parts}
          canRead={false}
          selectedPartId=""
          onSelectedPartIdChange={() => undefined}
        />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
    expect(mockGetSubstitutions).not.toHaveBeenCalled()
  })
})
