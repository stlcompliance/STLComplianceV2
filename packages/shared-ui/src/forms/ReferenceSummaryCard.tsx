import { CheckCircle2, X } from 'lucide-react'

import type { CrossProductReference, ReferenceSummaryResponse } from './referenceTypes'

export type ReferenceSummaryCardProps = {
  reference: CrossProductReference | ReferenceSummaryResponse
  onClear?: () => void
  disabled?: boolean
  testId?: string
}

export function ReferenceSummaryCard({
  reference,
  onClear,
  disabled = false,
  testId,
}: ReferenceSummaryCardProps) {
  const isSnapshot = 'displayLabelSnapshot' in reference
  const displayLabel =
    isSnapshot
      ? reference.displayLabelSnapshot
      : reference.displayLabel
  const secondaryLabel =
    isSnapshot
      ? reference.secondaryLabelSnapshot
      : reference.secondaryLabel
  const status =
    isSnapshot ? reference.statusSnapshot : reference.status
  const ownerProductKey = reference.ownerProductKey
  const referenceType = reference.referenceType

  return (
    <div
      className="flex min-h-16 items-start justify-between gap-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-3"
      data-testid={testId}
    >
      <div className="flex min-w-0 gap-3">
        <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-success)]" aria-hidden />
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-[var(--color-text-primary)]">{displayLabel}</p>
          <p className="truncate text-xs text-[var(--color-text-muted)]">
            {[secondaryLabel, status].filter(Boolean).join(' / ') || `${ownerProductKey} ${referenceType}`}
          </p>
          <p className="mt-1 text-[11px] uppercase tracking-wide text-[var(--color-text-muted)]">
            Managed by {ownerProductKey}
          </p>
        </div>
      </div>
      {onClear ? (
        <button
          type="button"
          onClick={onClear}
          disabled={disabled}
          className="rounded-md p-1.5 text-[var(--color-text-muted)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] disabled:cursor-not-allowed disabled:opacity-50"
          aria-label="Clear reference"
        >
          <X className="h-4 w-4" aria-hidden />
        </button>
      ) : null}
    </div>
  )
}
