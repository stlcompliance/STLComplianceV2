import { render, screen, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { PartCatalogPanel } from './PartCatalogPanel'

describe('PartCatalogPanel', () => {
  it('renders part catalog list', () => {
    render(
      <PartCatalogPanel
        catalogs={[
          {
            catalogId: '22222222-2222-2222-2222-222222222222',
            catalogKey: 'oem-filters',
            name: 'OEM Filters',
            description: 'Filter catalog',
            status: 'active',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        parts={[
          {
            partId: '33333333-3333-3333-3333-333333333333',
            partKey: 'filter-001',
            catalogId: '22222222-2222-2222-2222-222222222222',
            catalogKey: 'oem-filters',
            displayName: 'Primary Oil Filter',
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
                linkId: '44444444-4444-4444-4444-444444444444',
                partyId: '11111111-1111-1111-1111-111111111111',
                partyKey: 'acme-parts',
                partyDisplayName: 'Acme Parts Co.',
                vendorPartNumber: 'V-FLT-001',
                isPreferred: true,
                catalogUnitPrice: null,
                catalogCurrencyCode: null,
                catalogMinimumOrderQuantity: null,
                catalogLeadTimeDays: null,
                catalogQuantityAvailable: null,
                catalogAvailabilityStatus: null,
                createdAt: '2026-05-27T00:00:00Z',
              },
            ],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
        canManage={false}
        isLoading={false}
        catalogKey=""
        catalogName=""
        catalogDescription=""
        partKey=""
        partName=""
        partCategory=""
        partUom=""
        partManufacturer=""
        partMfgNumber=""
        selectedCatalogId=""
        vendorPartNumber=""
        selectedPartId=""
        selectedVendorId=""
        vendors={[]}
        onCatalogKeyChange={() => {}}
        onCatalogNameChange={() => {}}
        onCatalogDescriptionChange={() => {}}
        onPartKeyChange={() => {}}
        onPartNameChange={() => {}}
        onPartCategoryChange={() => {}}
        onPartUomChange={() => {}}
        onPartManufacturerChange={() => {}}
        onPartMfgNumberChange={() => {}}
        onSelectedCatalogIdChange={() => {}}
        onVendorPartNumberChange={() => {}}
        onSelectedPartIdChange={() => {}}
        onSelectedVendorIdChange={() => {}}
        onCreateCatalog={() => {}}
        onCreatePart={() => {}}
        onLinkVendor={() => {}}
        isCreatingCatalog={false}
        isCreatingPart={false}
        isLinkingVendor={false}
      />,
    )

    expect(screen.getByText('Primary Oil Filter')).toBeInTheDocument()
    expect(screen.getByText('OEM Filters')).toBeInTheDocument()
    expect(screen.getByText(/Acme Parts Co/)).toBeInTheDocument()
  })

  it('shows generated key preview when managing catalog', () => {
    render(
      <PartCatalogPanel
        catalogs={[]}
        parts={[]}
        canManage={true}
        isLoading={false}
        catalogKey=""
        catalogName="OEM Filters"
        catalogDescription=""
        partKey=""
        partName=""
        partCategory="general"
        partUom="each"
        partManufacturer=""
        partMfgNumber=""
        selectedCatalogId=""
        vendorPartNumber=""
        selectedPartId=""
        selectedVendorId=""
        vendors={[]}
        onCatalogKeyChange={() => {}}
        onCatalogNameChange={() => {}}
        onCatalogDescriptionChange={() => {}}
        onPartKeyChange={() => {}}
        onPartNameChange={() => {}}
        onPartCategoryChange={() => {}}
        onPartUomChange={() => {}}
        onPartManufacturerChange={() => {}}
        onPartMfgNumberChange={() => {}}
        onSelectedCatalogIdChange={() => {}}
        onVendorPartNumberChange={() => {}}
        onSelectedPartIdChange={() => {}}
        onSelectedVendorIdChange={() => {}}
        onCreateCatalog={() => {}}
        onCreatePart={() => {}}
        onLinkVendor={() => {}}
        isCreatingCatalog={false}
        isCreatingPart={false}
        isLinkingVendor={false}
      />,
    )

    const catalogSection = screen.getByRole('heading', { name: 'Add catalog' }).parentElement!
    expect(within(catalogSection).getByTestId('generated-key-preview')).toHaveTextContent('oem-filters')
  })
})
