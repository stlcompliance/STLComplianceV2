import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReturnsPanel } from './ReturnsPanel'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      placeholder?: string
      testId?: string
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
        <div data-testid={`${testId ?? 'picker'}-options`}>
          {options.map((option) => (
            <span key={option.value}>{option.label}</span>
          ))}
        </div>
      </label>
    ),
  }
})

const baseProps = {
  returns: [
    {
      returnId: 'ret-1',
      returnKey: 'ret-001',
      status: 'draft',
      sourceType: 'purchase_order_line',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Supply',
      purchaseOrderId: 'po-1',
      purchaseOrderKey: 'po-001',
      purchaseRequestId: 'pr-1',
      purchaseRequestKey: 'pr-001',
      inventoryBinId: 'bin-1',
      inventoryBinKey: 'main-bin',
      inventoryBinName: 'Main Bin',
      inventoryLocationId: 'loc-1',
      inventoryLocationKey: 'wh-1',
      inventoryLocationName: 'Warehouse',
      rmaNumber: 'RMA-100',
      notes: '',
      createdByUserId: 'user-1',
      postedByUserId: null,
      postedAt: null,
      cancelledByUserId: null,
      cancelledAt: null,
      cancellationReason: '',
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          partId: 'part-1',
          partKey: 'filter-01',
          partDisplayName: 'Oil Filter',
          purchaseOrderLineId: 'pol-1',
          purchaseOrderLineNumber: 1,
          quantity: 2,
          notes: '',
          createdAt: '2026-05-27T00:00:00Z',
          updatedAt: '2026-05-27T00:00:00Z',
        },
      ],
      createdAt: '2026-05-27T00:00:00Z',
      updatedAt: '2026-05-27T00:00:00Z',
    },
  ],
  vendors: [],
  parts: [],
  issuedPurchaseOrders: [],
  inventoryBins: [],
  canManage: true,
  isLoading: false,
  returnKey: '',
  selectedReturnId: 'ret-1',
  selectedVendorPartyId: '',
  selectedInventoryBinId: '',
  selectedReturnPoLineId: '',
  selectedReturnPartId: '',
  returnQuantity: '',
  rmaNumber: '',
  returnNotes: '',
  cancelReason: '',
  statusFilter: '',
  returnSource: 'stock' as const,
  onReturnKeyChange: () => {},
  onSelectedReturnIdChange: () => {},
  onSelectedVendorPartyIdChange: () => {},
  onSelectedInventoryBinIdChange: () => {},
  onSelectedReturnPoLineIdChange: () => {},
  onSelectedReturnPartIdChange: () => {},
  onReturnQuantityChange: () => {},
  onRmaNumberChange: () => {},
  onReturnNotesChange: () => {},
  onCancelReasonChange: () => {},
  onStatusFilterChange: () => {},
  onReturnSourceChange: () => {},
  onCreate: () => {},
  onPost: () => {},
  onCancel: () => {},
  isCreating: false,
  isPosting: false,
  isCancelling: false,
}

describe('ReturnsPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('shows draft return with RMA and PR/PO linkage', () => {
    render(<ReturnsPanel {...baseProps} />)
    expect(screen.getByText('ret-001')).toBeInTheDocument()
    expect(screen.getAllByText(/RMA RMA-100/).length).toBeGreaterThan(0)
    expect(screen.getByText(/PO po-001/)).toBeInTheDocument()
    expect(screen.getByText(/PR pr-001/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Post return' })).toBeInTheDocument()
  })

  it('uses searchable pickers for stock-based return creation inputs', () => {
    const onSelectedVendorPartyIdChange = vi.fn()
    const onSelectedInventoryBinIdChange = vi.fn()
    const onSelectedReturnPartIdChange = vi.fn()

    render(
      <ReturnsPanel
        {...baseProps}
        onSelectedVendorPartyIdChange={onSelectedVendorPartyIdChange}
        onSelectedInventoryBinIdChange={onSelectedInventoryBinIdChange}
        onSelectedReturnPartIdChange={onSelectedReturnPartIdChange}
        returnSource="stock"
        vendors={[
          {
            partyId: 'vendor-1',
            partyKey: 'acme',
            displayName: 'Acme Supply',
            partyType: 'vendor',
            legalName: '',
            taxIdentifier: null,
            approvalStatus: 'approved',
            status: 'active',
            notes: '',
            contacts: [],
            createdAt: '',
            updatedAt: '',
          },
        ]}
        parts={[
          {
            partId: 'part-1',
            partKey: 'filter-01',
            displayName: 'Oil Filter',
            description: '',
            categoryKey: '',
            unitOfMeasure: 'each',
            manufacturerName: '',
            manufacturerPartNumber: '',
            status: 'active',
            catalogId: null,
            catalogKey: null,
            reorderPoint: null,
            reorderQuantity: null,
            manufacturerAliases: [],
            vendorLinks: [],
            createdAt: '',
            updatedAt: '',
          },
        ]}
        inventoryBins={[
          { binId: 'bin-1', binKey: 'main-bin', name: 'Main Bin', label: 'WH-1 / main-bin — Main Bin' },
        ]}
        issuedPurchaseOrders={[
          {
            purchaseOrderId: 'po-1',
            orderKey: 'po-001',
            title: 'Replacement filters',
            notes: '',
            status: 'issued',
            purchaseRequestId: 'pr-1',
            purchaseRequestKey: 'pr-001',
            vendorPartyId: 'vendor-1',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme Supply',
            createdByUserId: 'user-1',
            approvedAt: '2026-05-27T00:00:00Z',
            approvedByUserId: 'user-1',
            issuedAt: '2026-05-27T00:00:00Z',
            issuedByUserId: 'user-1',
            cancelledAt: null,
            cancelledByUserId: null,
            cancellationReason: '',
            lines: [
              {
                lineId: 'pol-1',
                lineNumber: 1,
                purchaseRequestLineId: 'prl-1',
                partId: 'part-1',
                partKey: 'filter-01',
                partDisplayName: 'Oil Filter',
                quantityOrdered: 4,
                quantityReceived: 2,
                quantityRemaining: 2,
                unitOfMeasure: 'each',
                notes: '',
                createdAt: '2026-05-27T00:00:00Z',
                updatedAt: '2026-05-27T00:00:00Z',
              },
            ],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByTestId('vendor-return-bin-picker-options')).toHaveTextContent(
      'WH-1 / main-bin — Main Bin',
    )
    expect(screen.getByTestId('vendor-return-vendor-picker-options')).toHaveTextContent(
      'Acme Supply (acme)',
    )
    expect(screen.getByTestId('vendor-return-part-picker-options')).toHaveTextContent(
      'Oil Filter (filter-01)',
    )

    fireEvent.change(screen.getByTestId('vendor-return-vendor-picker'), {
      target: { value: 'vendor-1' },
    })
    fireEvent.change(screen.getByTestId('vendor-return-part-picker'), {
      target: { value: 'part-1' },
    })
    fireEvent.change(screen.getByTestId('vendor-return-bin-picker'), {
      target: { value: 'bin-1' },
    })

    expect(onSelectedVendorPartyIdChange).toHaveBeenCalledWith('vendor-1')
    expect(onSelectedReturnPartIdChange).toHaveBeenCalledWith('part-1')
    expect(onSelectedInventoryBinIdChange).toHaveBeenCalledWith('bin-1')
  })

  it('uses a searchable PO-line picker for PO-line return creation', () => {
    const onSelectedReturnPoLineIdChange = vi.fn()

    render(
      <ReturnsPanel
        {...baseProps}
        onSelectedReturnPoLineIdChange={onSelectedReturnPoLineIdChange}
        returnSource="purchase_order_line"
        issuedPurchaseOrders={[
          {
            purchaseOrderId: 'po-1',
            orderKey: 'po-001',
            title: 'Replacement filters',
            notes: '',
            status: 'issued',
            purchaseRequestId: 'pr-1',
            purchaseRequestKey: 'pr-001',
            vendorPartyId: 'vendor-1',
            vendorPartyKey: 'acme',
            vendorDisplayName: 'Acme Supply',
            createdByUserId: 'user-1',
            approvedAt: '2026-05-27T00:00:00Z',
            approvedByUserId: 'user-1',
            issuedAt: '2026-05-27T00:00:00Z',
            issuedByUserId: 'user-1',
            cancelledAt: null,
            cancelledByUserId: null,
            cancellationReason: '',
            lines: [
              {
                lineId: 'pol-1',
                lineNumber: 1,
                purchaseRequestLineId: 'prl-1',
                partId: 'part-1',
                partKey: 'filter-01',
                partDisplayName: 'Oil Filter',
                quantityOrdered: 4,
                quantityReceived: 2,
                quantityRemaining: 2,
                unitOfMeasure: 'each',
                notes: '',
                createdAt: '2026-05-27T00:00:00Z',
                updatedAt: '2026-05-27T00:00:00Z',
              },
            ],
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T00:00:00Z',
          },
        ]}
      />,
    )

    expect(screen.getByTestId('vendor-return-po-line-picker-options')).toHaveTextContent(
      'po-001 · line 1 · filter-01 (2 received)',
    )

    fireEvent.change(screen.getByTestId('vendor-return-po-line-picker'), {
      target: { value: 'pol-1' },
    })

    expect(onSelectedReturnPoLineIdChange).toHaveBeenCalledWith('pol-1')
  })
})
