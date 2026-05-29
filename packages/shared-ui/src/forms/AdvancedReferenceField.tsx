import { useState } from 'react'

export type AdvancedReferenceFieldProps = {
  value: string
  onChange: (value: string) => void
  label?: string
  id?: string
  placeholder?: string
  followUpId?: string
  disabled?: boolean
  testId?: string
}

export function AdvancedReferenceField({
  value,
  onChange,
  label = 'Reference ID',
  id,
  placeholder = 'Paste GUID or key…',
  followUpId,
  disabled = false,
  testId,
}: AdvancedReferenceFieldProps) {
  const [open, setOpen] = useState(false)
  const inputId = id ?? (testId ? `${testId}-input` : 'advanced-reference-input')

  return (
    <div data-testid={testId}>
      <button
        type="button"
        className="text-xs text-slate-400 underline hover:text-slate-200"
        onClick={() => setOpen((current) => !current)}
        data-testid={testId ? `${testId}-toggle` : 'advanced-reference-toggle'}
      >
        {open ? 'Hide advanced reference' : 'Advanced reference'}
      </button>
      {open ? (
        <label htmlFor={inputId} className="mt-2 block text-sm text-slate-400">
          {label}
          <input
            id={inputId}
            type="text"
            value={value}
            onChange={(event) => onChange(event.target.value)}
            placeholder={placeholder}
            disabled={disabled}
            data-testid={testId ? `${testId}-input` : 'advanced-reference-input'}
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm text-slate-100"
          />
          {followUpId ? (
            <span className="mt-1 block text-xs text-slate-600" data-follow-up-id={followUpId}>
              Picker API pending ({followUpId}).
            </span>
          ) : null}
        </label>
      ) : null}
    </div>
  )
}
