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
      <section className="rounded-lg border border-rose-300 bg-rose-50/90 p-4 text-sm text-rose-800">
        {errorMessage}
      </section>
    )
  }

  if (isLoading) {
    return (
      <section className="rounded-lg border border-slate-200 bg-white/90 p-4 text-sm text-slate-500">
        Loading print history…
      </section>
    )
  }

  if (items.length === 0) {
    return (
      <section className="rounded-lg border border-slate-200 bg-white/90 p-4 text-sm text-slate-500">
        No print or export history exists for this surface yet.
      </section>
    )
  }

  return (
    <section className="rounded-lg border border-slate-200 bg-white/95 p-4 text-slate-900">
      <h3 className="text-sm font-semibold uppercase tracking-wide text-slate-500">Print history</h3>
      <ul className="mt-3 space-y-3">
        {items.map((item) => (
          <li key={item.id} className="rounded-lg border border-slate-200 bg-slate-50/80 p-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm font-semibold text-slate-900">{item.sourceDisplayRef}</p>
              <p className="text-xs text-slate-500">
                {new Date(item.requestedAtUtc).toLocaleString()}
              </p>
            </div>
            <p className="mt-1 text-xs text-slate-600">
              {actionLabel(item.action)} · {documentStatusLabel(item.documentStatus)} · {item.templateKey}
            </p>
            {item.failureReason ? (
              <p className="mt-2 text-xs text-rose-700">Failure: {item.failureReason}</p>
            ) : null}
          </li>
        ))}
      </ul>
    </section>
  )
}
