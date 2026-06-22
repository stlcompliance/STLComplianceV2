import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

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
            isTrackable: true,
            isStocked: true,
            requiresSerialLotTracking: false,
            reorderPoint: null,
            reorderQuantity: null,
            manufacturerAliases: [],
            sources: [
              {
                sourceId: '55555555-5555-5555-5555-555555555555',
                sourceType: 'rebuilt',
                label: 'Rebuild bench',
                notes: '',
                createdAt: '2026-05-27T00:00:00Z',
              },
            ],
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
        partIsTrackable={true}
        partIsStocked={true}
        selectedCatalogId=""
        selectedSourcePartId=""
        partSourceType="unknown"
        partSourceLabel=""
        partSourceNotes=""
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
        onPartIsTrackableChange={() => {}}
        onPartIsStockedChange={() => {}}
        onSelectedCatalogIdChange={() => {}}
        onSelectedSourcePartIdChange={() => {}}
        onPartSourceTypeChange={() => {}}
        onPartSourceLabelChange={() => {}}
        onPartSourceNotesChange={() => {}}
        onVendorPartNumberChange={() => {}}
        onSelectedPartIdChange={() => {}}
        onSelectedVendorIdChange={() => {}}
        onCreateCatalog={() => {}}
        onCreatePart={() => {}}
        onCreatePartSource={() => {}}
        onLinkVendor={() => {}}
        isCreatingCatalog={false}
        isCreatingPart={false}
        isCreatingPartSource={false}
        isLinkingVendor={false}
      />,
    )

    expect(screen.getByText('Primary Oil Filter')).toBeInTheDocument()
    expect(screen.getByText('OEM Filters')).toBeInTheDocument()
    expect(screen.getByText(/Acme Parts Co/)).toBeInTheDocument()
    expect(screen.getByText('Rebuild bench')).toBeInTheDocument()
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
        partIsTrackable={true}
        partIsStocked={true}
        selectedCatalogId=""
        selectedSourcePartId=""
        partSourceType="unknown"
        partSourceLabel=""
        partSourceNotes=""
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
        onPartIsTrackableChange={() => {}}
        onPartIsStockedChange={() => {}}
        onSelectedCatalogIdChange={() => {}}
        onSelectedSourcePartIdChange={() => {}}
        onPartSourceTypeChange={() => {}}
        onPartSourceLabelChange={() => {}}
        onPartSourceNotesChange={() => {}}
        onVendorPartNumberChange={() => {}}
        onSelectedPartIdChange={() => {}}
        onSelectedVendorIdChange={() => {}}
        onCreateCatalog={() => {}}
        onCreatePart={() => {}}
        onCreatePartSource={() => {}}
        onLinkVendor={() => {}}
        isCreatingCatalog={false}
        isCreatingPart={false}
        isCreatingPartSource={false}
        isLinkingVendor={false}
      />,
    )

    const catalogSection = screen.getByRole('heading', { name: 'Add catalog' }).parentElement!
    expect(within(catalogSection).getByTestId('generated-key-preview')).toHaveTextContent(
      'part.catalog.oemfilters',
    )
    expect(screen.getByTestId('part-catalog-picker')).toBeInTheDocument()
    expect(screen.getByTestId('part-source-part-picker')).toBeInTheDocument()
    expect(screen.getByTestId('vendor-link-part-picker')).toBeInTheDocument()
    expect(screen.getByTestId('vendor-link-vendor-picker')).toBeInTheDocument()
  })
})
