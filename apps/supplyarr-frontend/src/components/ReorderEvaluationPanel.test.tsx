import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { ReorderEvaluationPanel } from './ReorderEvaluationPanel'

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
  suggestions: [
    {
      partId: 'part-1',
      partKey: 'filter-01',
      displayName: 'Oil Filter',
      unitOfMeasure: 'each',
      reorderPoint: 10,
      reorderQuantity: 24,
      quantityOnHand: 3,
      quantityReserved: 0,
      quantityAvailable: 3,
      suggestedOrderQuantity: 24,
      preferredSupplierId: 'vendor-1',
      preferredSupplierKey: 'acme',
      preferredSupplierDisplayName: 'Acme Supply',
      preferredVendorPartyId: 'vendor-1',
      preferredVendorPartyKey: 'acme',
      preferredVendorDisplayName: 'Acme Supply',
      hasOpenPurchaseRequest: false,
      skipReason: null,
    },
  ],
  parts: [
    {
      partId: 'part-1',
      partKey: 'filter-01',
      displayName: 'Oil Filter',
    },
  ],
  canManagePolicy: true,
  canCreatePurchaseRequest: true,
  isLoading: false,
  selectedPartId: 'part-1',
  reorderPoint: '10',
  reorderQuantity: '24',
  selectedSuggestionPartIds: [],
  prRequestKey: 'reorder-pr-001',
  prTitle: 'Restock filters',
  prNotes: '',
  onSelectedPartIdChange: () => {},
  onReorderPointChange: () => {},
  onReorderQuantityChange: () => {},
  onSelectedSuggestionPartIdsChange: () => {},
  onPrRequestKeyChange: () => {},
  onPrTitleChange: () => {},
  onPrNotesChange: () => {},
  onSavePolicy: () => {},
  onRefreshEvaluation: () => {},
  onCreatePurchaseRequest: () => {},
  isSavingPolicy: false,
  isCreatingPurchaseRequest: false,
}

describe('ReorderEvaluationPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders reorder suggestions with suggested quantity', () => {
    render(<ReorderEvaluationPanel {...baseProps} />)
    expect(screen.getByText('Reorder evaluation')).toBeInTheDocument()
    expect(screen.getByText('filter-01')).toBeInTheDocument()
    expect(screen.getByText('24')).toBeInTheDocument()
    expect(screen.getByText('Acme Supply')).toBeInTheDocument()
    expect(screen.getByText('Preferred supplier')).toBeInTheDocument()
  })

  it('renders a searchable reorder policy part picker', () => {
    const onSelectedPartIdChange = vi.fn()

    render(
      <ReorderEvaluationPanel
        {...baseProps}
        onSelectedPartIdChange={onSelectedPartIdChange}
        selectedPartId=""
      />,
    )

    expect(screen.getByTestId('reorder-policy-part-picker-options')).toHaveTextContent(
      'filter-01 · Oil Filter',
    )

    fireEvent.change(screen.getByTestId('reorder-policy-part-picker'), {
      target: { value: 'part-1' },
    })

    expect(onSelectedPartIdChange).toHaveBeenCalledWith('part-1')
  })
})
