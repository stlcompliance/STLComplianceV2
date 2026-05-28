import { useEffect } from 'react'

import type { SubmissionToast } from '../lib/submissionState'

interface SubmissionActivityBannerProps {
  toasts: SubmissionToast[]
  onDismiss: (id: string) => void
}

export function SubmissionActivityBanner({ toasts, onDismiss }: SubmissionActivityBannerProps) {
  const latest = toasts[0]

  useEffect(() => {
    if (!latest) {
      return
    }

    const timer = window.setTimeout(() => onDismiss(latest.id), 6000)
    return () => window.clearTimeout(timer)
  }, [latest, onDismiss])

  if (!latest) {
    return null
  }

  const toneClasses =
    latest.tone === 'success'
      ? 'border-emerald-500/50 bg-emerald-950/40 text-emerald-100'
      : latest.tone === 'error'
        ? 'border-rose-500/50 bg-rose-950/40 text-rose-100'
        : 'border-slate-600 bg-slate-900/90 text-slate-100'

  return (
    <div
      className={`flex items-start justify-between gap-3 rounded-xl border px-4 py-3 text-sm shadow-lg ${toneClasses}`}
      data-testid="companion-submission-toast"
      role="status"
    >
      <p>{latest.message}</p>
      <button
        type="button"
        className="shrink-0 text-xs font-medium uppercase tracking-wide opacity-80 hover:opacity-100"
        onClick={() => onDismiss(latest.id)}
      >
        Dismiss
      </button>
    </div>
  )
}
