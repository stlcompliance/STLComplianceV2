import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { CatalogSection } from './CatalogSection'

vi.mock('../../components/PartCatalogPanel', () => ({
  PartCatalogPanel: () => <div data-testid="supplyarr-part-catalog-panel" />,
}))

vi.mock('../../components/PartSubstitutionsPanel', () => ({
  PartSubstitutionsPanel: () => <div data-testid="supplyarr-part-substitutions-panel" />,
}))

vi.mock('../../components/VendorCatalogApiPanel', () => ({
  VendorCatalogApiPanel: () => <div data-testid="supplyarr-vendor-catalog-api-panel" />,
}))

const state = {
  accessToken: 'token',
  canManageCatalog: true,
  canReadPartSubstitutions: true,
  catalogsQuery: { data: [], isLoading: false },
  partsQuery: { data: [], isLoading: false },
  suppliersQuery: { data: [] },
  vendorsQuery: { data: [] },
  createCatalogMutation: { isPending: false, mutate: vi.fn() },
  createPartMutation: { isPending: false, mutate: vi.fn() },
  createPartSourceMutation: { isPending: false, mutate: vi.fn() },
  linkVendorMutation: { isPending: false, mutate: vi.fn() },
  substitutionPartId: '',
  vendors: [],
} as never

describe('CatalogSection', () => {
  it('renders the vendor catalog api panel with the catalog workspace', () => {
    render(<CatalogSection state={state} />)

    expect(screen.getByTestId('supplyarr-part-catalog-panel')).toBeInTheDocument()
    expect(screen.getByTestId('supplyarr-part-substitutions-panel')).toBeInTheDocument()
    expect(screen.getByTestId('supplyarr-vendor-catalog-api-panel')).toBeInTheDocument()
  })
})
