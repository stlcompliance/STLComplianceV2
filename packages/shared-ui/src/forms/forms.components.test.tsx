import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { CheckboxMultiSelect } from './CheckboxMultiSelect'
import { GeneratedKeyField } from './GeneratedKeyField'
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
      />,
    )
    expect(screen.getByTestId('generated-key-manual-override')).toBeInTheDocument()
    fireEvent.change(screen.getByTestId('generated-key-manual-override'), {
      target: { value: 'custom-key' },
    })
    expect(onManualOverrideChange).toHaveBeenCalledWith('custom-key')
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
})
