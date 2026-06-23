import type { PrintHistoryItem } from './types'

type Props = {
  items: PrintHistoryItem[]
  isLoading?: boolean
  errorMessage?: string | null
}

function actionLabel(value: PrintHistoryItem['action']): string {
  return value.replace(/_/g, ' ')
}

function documentStatusLabel(value: PrintHistoryItem['documentStatus']): string {
  return value.replace(/_/g, ' ')
}

export function PrintHistoryPanel({ items, isLoading = false, errorMessage = null }: Props) {
  if (errorMessage) {
    return (
      <section className="rounded-lg border border-[var(--color-destructive-border)] bg-[var(--color-destructive-bg)] p-4 text-sm text-[var(--color-destructive-text)]">
        {errorMessage}
      </section>
    )
  }

  if (isLoading) {
    return (
      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm text-[var(--color-text-muted)]">
        Loading print history…
      </section>
    )
  }

  if (items.length === 0) {
    return (
      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm text-[var(--color-text-muted)]">
        No print or export history exists for this surface yet.
      </section>
    )
  }

  return (
    <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-[var(--color-text-primary)]">
      <h3 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Print history</h3>
      <ul className="mt-3 space-y-3">
        {items.map((item) => (
          <li key={item.id} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] p-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm font-semibold text-[var(--color-text-primary)]">{item.sourceDisplayRef}</p>
              <p className="text-xs text-[var(--color-text-muted)]">
                {new Date(item.requestedAtUtc).toLocaleString()}
              </p>
            </div>
            <p className="mt-1 text-xs text-[var(--color-text-secondary)]">
              {actionLabel(item.action)} · {documentStatusLabel(item.documentStatus)} · {item.templateKey}
            </p>
            {item.failureReason ? (
              <p className="mt-2 text-xs text-[var(--color-destructive-text)]">Failure: {item.failureReason}</p>
            ) : null}
          </li>
        ))}
      </ul>
    </section>
  )
}
