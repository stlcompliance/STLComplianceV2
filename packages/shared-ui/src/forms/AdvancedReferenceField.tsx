import { useState } from 'react'
import { unavailableReferenceLabel } from '../displayLabels'

export type AdvancedReferenceFieldProps = {
  value: string
  onChange: (value: string) => void
  label?: string
  id?: string
  placeholder?: string
  followUpId?: string
  allowManualEntry?: boolean
  manualEntryDisabledMessage?: string
  disabled?: boolean
  testId?: string
}

export function AdvancedReferenceField({
  value,
  onChange,
  label = 'Selected reference',
  id,
  placeholder,
  followUpId,
  allowManualEntry = false,
  manualEntryDisabledMessage = 'Manual entry is disabled. Select from the owning product picker.',
  disabled = false,
  testId,
}: AdvancedReferenceFieldProps) {
  const [open, setOpen] = useState(false)
  const inputId = id ?? (testId ? `${testId}-input` : 'advanced-reference-input')
  const inputDisabled = disabled
  const displayedValue = allowManualEntry ? value : value ? unavailableReferenceLabel(value) : ''
  const resolvedPlaceholder =
    placeholder ??
    (allowManualEntry
      ? 'Type a verified reference…'
      : 'Selected from the owning product picker')

  return (
    <div data-testid={testId}>
      <button
        type="button"
        className="text-xs font-medium text-[var(--color-text-muted)] underline underline-offset-4 transition hover:text-[var(--color-text-primary)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
        onClick={() => setOpen((current) => !current)}
        data-testid={testId ? `${testId}-toggle` : 'advanced-reference-toggle'}
      >
        {open ? 'Hide reference policy' : 'Reference policy'}
      </button>
      {open ? (
        <label htmlFor={inputId} className="mt-2 block text-sm text-[var(--color-text-primary)]">
          {label}
          <input
            id={inputId}
            type="text"
            value={displayedValue}
            onChange={(event) => {
              if (!allowManualEntry || disabled) {
                return
              }
              onChange(event.target.value)
            }}
            placeholder={resolvedPlaceholder}
            disabled={inputDisabled}
            readOnly={!allowManualEntry}
            aria-readonly={!allowManualEntry}
            data-testid={testId ? `${testId}-input` : 'advanced-reference-input'}
            className="mt-1 w-full rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] outline-none transition hover:bg-[var(--color-bg-control-hover)] focus:border-[var(--color-accent-border)] focus:ring-2 focus:ring-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-70"
          />
          {!allowManualEntry ? (
            <span className="mt-1 block text-xs text-[var(--color-text-muted)]" data-testid={testId ? `${testId}-manual-disabled` : 'advanced-reference-manual-disabled'}>
              {manualEntryDisabledMessage}
            </span>
          ) : null}
          {followUpId ? (
            <span className="mt-1 block text-xs text-[var(--color-text-muted)]" data-follow-up-id={followUpId}>
              Picker API pending ({followUpId}).
            </span>
          ) : null}
        </label>
      ) : null}
    </div>
  )
}
