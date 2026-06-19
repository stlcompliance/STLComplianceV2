export type GeneratedKeyFieldProps = {
  sourceLabel: string
  generatedKey: string
  confirmedKey?: string | null
  manualOverride: string
  onManualOverrideChange: (value: string) => void
  showAdvancedKey?: boolean
  allowManualOverride?: boolean
  manualOverrideDisabledMessage?: string
  collisionWarning?: string | null
  disabled?: boolean
  label?: string
}

export function GeneratedKeyField({
  sourceLabel,
  generatedKey,
  confirmedKey,
  manualOverride,
  onManualOverrideChange,
  showAdvancedKey = false,
  allowManualOverride = false,
  manualOverrideDisabledMessage = 'Manual key overrides are disabled. The system generates and maintains this key.',
  collisionWarning,
  disabled = false,
  label = 'Key',
}: GeneratedKeyFieldProps) {
  const displayKey = confirmedKey ?? generatedKey
  const previewId = 'generated-key-preview-output'
  const manualOverrideId = 'generated-key-manual-override-input'

  return (
    <div className="space-y-2" data-testid="generated-key-field">
      <label htmlFor={previewId} className="block text-sm text-slate-300">
        {label}
        <output
          id={previewId}
          className="mt-1 block w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm text-slate-200"
          data-testid="generated-key-preview"
        >
          {displayKey || '—'}
        </output>
      </label>
      {!displayKey && sourceLabel.trim() ? (
        <p className="text-xs text-[var(--color-text-muted)]">Enter a display name to preview the key.</p>
      ) : null}
      {collisionWarning ? (
        <p className="text-xs text-amber-400" data-testid="generated-key-collision-warning">
          {collisionWarning}
        </p>
      ) : null}
      {showAdvancedKey && allowManualOverride ? (
        <label htmlFor={manualOverrideId} className="block text-sm text-slate-400">
          Manual key override
          <input
            id={manualOverrideId}
            type="text"
            value={manualOverride}
            onChange={(event) => onManualOverrideChange(event.target.value)}
            disabled={disabled}
            data-testid="generated-key-manual-override"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-sm text-slate-100"
          />
        </label>
      ) : null}
      {showAdvancedKey && !allowManualOverride ? (
        <p className="text-xs text-[var(--color-text-muted)]" data-testid="generated-key-manual-override-disabled">
          {manualOverrideDisabledMessage}
        </p>
      ) : null}
    </div>
  )
}
