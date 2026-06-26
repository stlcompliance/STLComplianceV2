import type { ReactNode } from 'react'
import { useHintsPreference } from '@stl/shared-ui'

export function PreferenceSection({
  title,
  subtitle,
  actions,
  children,
}: {
  title: string
  subtitle?: string
  actions?: ReactNode
  children: ReactNode
}) {
  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-[var(--shadow-surface)]">
      <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{title}</h2>
          {subtitle ? <p className="mt-1 text-sm text-[var(--color-text-muted)]">{subtitle}</p> : null}
        </div>
        {actions ? <div className="flex flex-wrap items-center gap-2">{actions}</div> : null}
      </div>
      <div className="space-y-4">{children}</div>
    </section>
  )
}

export function PreferenceField({
  label,
  description,
  error,
  children,
}: {
  label: string
  description?: string
  error?: string | null
  children: ReactNode
}) {
  const { showHints } = useHintsPreference()
  return (
    <div className="space-y-1.5">
      <div className="flex items-baseline justify-between gap-4">
        <label className="text-sm font-medium text-[var(--color-text-primary)]">{label}</label>
      </div>
      {description && showHints ? <p className="text-sm text-[var(--color-text-muted)]">{description}</p> : null}
      {children}
      {error ? <p className="text-sm text-[var(--color-destructive-text)]">{error}</p> : null}
    </div>
  )
}

export function PreferenceToggle({
  checked,
  onChange,
  disabled,
  label,
}: {
  checked: boolean
  onChange: (checked: boolean) => void
  disabled?: boolean
  label: string
}) {
  return (
    <label className="inline-flex items-center gap-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]">
      <input
        type="checkbox"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
        disabled={disabled}
        className="h-4 w-4 rounded border-[var(--color-border-strong)] text-[var(--color-accent)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
      />
      <span>{label}</span>
    </label>
  )
}

export function PreferenceSelect({
  value,
  onChange,
  options,
  disabled,
  'aria-label': ariaLabel,
}: {
  value: string
  onChange: (value: string) => void
  options: readonly { value: string; label: string }[]
  disabled?: boolean
  'aria-label'?: string
}) {
  return (
    <select
      aria-label={ariaLabel}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      disabled={disabled}
      className="min-h-10 w-full rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 text-sm text-[var(--color-text-primary)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
    >
      {options.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </select>
  )
}

export function PreferenceNumberInput({
  value,
  onChange,
  min,
  max,
  disabled,
}: {
  value: number
  onChange: (value: number) => void
  min?: number
  max?: number
  disabled?: boolean
}) {
  return (
    <input
      type="number"
      value={value}
      min={min}
      max={max}
      onChange={(event) => onChange(Number(event.target.value))}
      disabled={disabled}
      className="min-h-10 w-full rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 text-sm text-[var(--color-text-primary)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
    />
  )
}

export function PreferenceResetButton({
  onClick,
  children,
}: {
  onClick: () => void
  children: ReactNode
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="inline-flex min-h-10 items-center rounded-lg border border-[var(--color-border-strong)] px-3 text-sm text-[var(--color-text-secondary)] transition hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
    >
      {children}
    </button>
  )
}
