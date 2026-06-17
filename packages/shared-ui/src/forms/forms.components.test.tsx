import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { CheckboxMultiSelect } from './CheckboxMultiSelect'
import { GeneratedKeyField } from './GeneratedKeyField'
import { AdvancedReferenceField } from './AdvancedReferenceField'
import { StaticSearchPicker } from './StaticSearchPicker'

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
