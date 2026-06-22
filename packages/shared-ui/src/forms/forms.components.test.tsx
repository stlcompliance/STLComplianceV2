import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import type React from 'react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { CheckboxMultiSelect } from './CheckboxMultiSelect'
import { GeneratedKeyField } from './GeneratedKeyField'
import { AdvancedReferenceField } from './AdvancedReferenceField'
import { ReferencePicker } from './ReferencePicker'
import { ReferenceSearchPicker } from './ReferenceSearchPicker'
import { StaticSearchPicker } from './StaticSearchPicker'
import { QuickCreateDrawer } from './QuickCreateDrawer'

afterEach(() => {
  cleanup()
})

describe('GeneratedKeyField', () => {
  it('shows preview and confirmed key', () => {
    render(
      <GeneratedKeyField
        sourceLabel="Widget"
        generatedKey="widget"
        manualOverride=""
        onManualOverrideChange={() => {}}
      />,
    )
    expect(screen.getByTestId('generated-key-preview')).toHaveTextContent('widget')
  })

  it('prefers confirmed key from API', () => {
    render(
      <GeneratedKeyField
        sourceLabel="Widget"
        generatedKey="widget"
        confirmedKey="widget-v2"
        manualOverride=""
        onManualOverrideChange={() => {}}
      />,
    )
    expect(screen.getByTestId('generated-key-preview')).toHaveTextContent('widget-v2')
  })

  it('shows manual override only when advanced', () => {
    const onManualOverrideChange = vi.fn()
    render(
      <GeneratedKeyField
        sourceLabel="Widget"
        generatedKey="widget"
        manualOverride="custom"
        onManualOverrideChange={onManualOverrideChange}
        showAdvancedKey
        allowManualOverride
      />,
    )
    expect(screen.getByTestId('generated-key-manual-override')).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('generated-key-manual-override'), {
      target: { value: 'custom-key' },
    })
    expect(onManualOverrideChange).toHaveBeenCalledWith('custom-key')
  })

  it('shows policy message when advanced override is disabled', () => {
    render(
      <GeneratedKeyField
        sourceLabel="Widget"
        generatedKey="widget"
        manualOverride=""
        onManualOverrideChange={() => {}}
        showAdvancedKey
      />,
    )
    expect(screen.getByTestId('generated-key-manual-override-disabled')).toBeInTheDocument()
  })
})

describe('CheckboxMultiSelect', () => {
  it('toggles values', () => {
    const onChange = vi.fn()
    render(
      <CheckboxMultiSelect
        values={['a']}
        onChange={onChange}
        options={[
          { value: 'a', label: 'Alpha' },
          { value: 'b', label: 'Beta' },
        ]}
      />,
    )
    fireEvent.click(screen.getByLabelText('Beta'))
    expect(onChange).toHaveBeenCalledWith(['a', 'b'])
  })
})

describe('StaticSearchPicker', () => {
  it('selects option by value and shows inactive orphan', () => {
    const onChange = vi.fn()
    render(
      <StaticSearchPicker
        value="gone"
        onChange={onChange}
        options={[{ value: 'active', label: 'Active trip' }]}
        selectedOption={{ value: 'gone', label: 'Old trip', inactive: true }}
        testId="trip-picker"
      />,
    )
    expect(screen.getByTestId('trip-picker')).toBeInTheDocument()
    const input = screen.getByRole('searchbox')
    expect(input).toHaveValue('Old trip (inactive)')
  })

  it('uses a safe fallback when an orphaned selected value has no readable label', () => {
    render(
      <StaticSearchPicker
        value="4d0d9c6d-1e42-4c4f-8af2-24f020de1c5f"
        onChange={() => {}}
        options={[{ value: 'active', label: 'Active trip' }]}
        testId="safe-picker"
      />,
    )

    expect(screen.getByRole('searchbox')).toHaveValue('Unavailable record (inactive)')
  })
})

describe('AdvancedReferenceField', () => {
  it('blocks manual edits by default', () => {
    const onChange = vi.fn()
    render(<AdvancedReferenceField value="trip-1" onChange={onChange} testId="advanced-ref" />)

    fireEvent.click(screen.getByTestId('advanced-ref-toggle'))
    const input = screen.getByTestId('advanced-ref-input')
    fireEvent.change(input, { target: { value: 'trip-2' } })

    expect(onChange).not.toHaveBeenCalled()
    expect(screen.getByTestId('advanced-ref-input')).toHaveValue('Trip 1')
    expect(screen.getByTestId('advanced-ref-manual-disabled')).toBeInTheDocument()
  })

  it('allows manual edits only when explicitly enabled', () => {
    const onChange = vi.fn()
    render(
      <AdvancedReferenceField
        value=""
        onChange={onChange}
        allowManualEntry
        testId="advanced-ref-enabled"
      />,
    )

    fireEvent.click(screen.getByTestId('advanced-ref-enabled-toggle'))
    fireEvent.change(screen.getByTestId('advanced-ref-enabled-input'), {
      target: { value: 'trip-2' },
    })

    expect(onChange).toHaveBeenCalledWith('trip-2')
  })
})

describe('ReferencePicker', () => {
  it('searches by owner label and selects a canonical snapshot without showing raw IDs', async () => {
    const onChange = vi.fn()
    const client = {
      searchReferences: vi.fn(async () => ({
        results: [
          {
            ownerProductKey: 'customarr',
            referenceType: 'customer',
            referenceId: 'cust-123',
            displayLabel: 'Acme Manufacturing',
            secondaryLabel: 'CUST-001',
            status: 'prospect',
          },
        ],
      })),
      getQuickCreateSchema: vi.fn(async () => ({
        ownerProductKey: 'customarr',
        referenceType: 'customer',
        allowed: false,
        managedByLabel: 'CustomArr',
        fields: [],
      })),
      quickCreate: vi.fn(),
    }

    renderWithQueryClient(
      <ReferencePicker
        client={client}
        ownerProductKey="customarr"
        referenceType="customer"
        value={null}
        onChange={onChange}
        minQueryLength={1}
        debounceMs={0}
        testId="customer-ref"
      />,
    )

    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'ac' } })

    expect(await screen.findByText('Acme Manufacturing')).toBeInTheDocument()
    expect(screen.queryByText('cust-123')).not.toBeInTheDocument()

    fireEvent.click(screen.getByText('Acme Manufacturing'))

    expect(onChange).toHaveBeenCalledWith({
      ownerProductKey: 'customarr',
      referenceType: 'customer',
      referenceId: 'cust-123',
      displayLabelSnapshot: 'Acme Manufacturing',
      secondaryLabelSnapshot: 'CUST-001',
      statusSnapshot: 'prospect',
      ownerVersion: undefined,
      createdVia: 'selected',
    })
  })

  it('shows owner-disabled quick create capability', async () => {
    const client = {
      searchReferences: vi.fn(async () => ({ results: [] })),
      getQuickCreateSchema: vi.fn(async () => ({
        ownerProductKey: 'staffarr',
        referenceType: 'person',
        allowed: false,
        managedByLabel: 'StaffArr',
        disabledReason: 'Person quick create is disabled.',
        fields: [],
      })),
      quickCreate: vi.fn(),
    }

    renderWithQueryClient(
      <ReferencePicker
        client={client}
        ownerProductKey="staffarr"
        referenceType="person"
        value={null}
        onChange={() => {}}
        testId="person-ref"
      />,
    )

    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'pe' } })
    fireEvent.click(await screen.findByRole('option', { name: /Quick create/ }))

    expect(await screen.findByText('Person quick create is disabled.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create' })).toBeDisabled()
  })

  it('keeps the drawer open when the owner returns duplicate candidates', async () => {
    const client = {
      searchReferences: vi.fn(async () => ({ results: [] })),
      getQuickCreateSchema: vi.fn(async () => ({
        ownerProductKey: 'supplyarr',
        referenceType: 'supplier',
        allowed: true,
        managedByLabel: 'SupplyArr',
        fields: [
          {
            key: 'displayName',
            label: 'Display name',
            fieldType: 'text',
            required: true,
          },
        ],
      })),
      quickCreate: vi.fn(async () => ({
        reference: null,
        duplicateCandidates: [
          {
            referenceId: 'supplier-1',
            displayLabel: 'Acme Supply',
            secondaryLabel: 'acme',
            status: 'pending',
            matchReason: 'matching display name',
            confidence: 0.9,
          },
        ],
        created: false,
        message: 'Possible duplicate.',
      })),
    }

    renderWithQueryClient(
      <ReferencePicker
        client={client}
        ownerProductKey="supplyarr"
        referenceType="supplier"
        value={null}
        onChange={() => {}}
        testId="supplier-ref"
      />,
    )

    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'ac' } })
    fireEvent.click(await screen.findByRole('option', { name: /Quick create/ }))
    fireEvent.change(await screen.findByLabelText(/Display name/), {
      target: { value: 'Acme Supply' },
    })
    fireEvent.click(screen.getByRole('button', { name: 'Create' }))

    await waitFor(() => expect(client.quickCreate).toHaveBeenCalled())
    expect(await screen.findByText('Possible duplicates')).toBeInTheDocument()
    expect(screen.getByText('Acme Supply')).toBeInTheDocument()
  })

  it('opens quick create from the reference search dropdown', async () => {
    const client = {
      searchReferences: vi.fn(async () => ({ results: [] })),
      getSummary: vi.fn(async () => ({
        ownerProductKey: 'staffarr',
        referenceType: 'person',
        referenceId: 'person-1',
        displayLabel: 'Pat Jones',
      })),
      getQuickCreateSchema: vi.fn(async () => ({
        ownerProductKey: 'staffarr',
        referenceType: 'person',
        allowed: false,
        managedByLabel: 'StaffArr',
        disabledReason: 'Person quick create is disabled.',
        fields: [],
      })),
      quickCreate: vi.fn(),
    }

    renderWithQueryClient(
      <ReferenceSearchPicker
        client={client}
        referenceType="person"
        value=""
        onChange={() => {}}
        testId="person-search-ref"
      />,
    )

    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'pa' } })
    fireEvent.click(await screen.findByRole('option', { name: /Quick create/ }))

    expect(await screen.findByText('Person quick create is disabled.')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Create' })).toBeDisabled()
  })
})

describe('QuickCreateDrawer', () => {
  it('shows a safe fallback when quick create fails', async () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const onCreate = vi.fn().mockRejectedValue(new Error('database exploded'))

    render(
      <QuickCreateDrawer
        open
        schema={{
          ownerProductKey: 'staffarr',
          referenceType: 'person',
          allowed: true,
          managedByLabel: 'StaffArr',
          fields: [
            {
              key: 'displayName',
              label: 'Display name',
              fieldType: 'text',
              required: true,
            },
          ],
        }}
        onClose={() => undefined}
        onCreate={onCreate}
      />,
    )

    fireEvent.change(screen.getByLabelText('Display name *'), { target: { value: 'Alex' } })
    fireEvent.click(screen.getByRole('button', { name: 'Create' }))

    expect(await screen.findByText('Quick create is temporarily unavailable. Please try again.')).toBeInTheDocument()
    expect(consoleError).toHaveBeenCalled()

    consoleError.mockRestore()
  })
})

function renderWithQueryClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>)
}
