import { useState } from 'react'
import { ArrowRight, ClipboardCopy, TriangleAlert } from 'lucide-react'

import type { FieldCompanionOperationalFallbackSnapshot } from '../lib/degradedOperation'

interface PanelAction {
  label: string
  href?: string
  onClick?: () => void
  testId?: string
}

interface DegradedOperationPanelProps {
  snapshot: FieldCompanionOperationalFallbackSnapshot
  actions: PanelAction[]
}

export function DegradedOperationPanel({ snapshot, actions }: DegradedOperationPanelProps) {
  const [copyStatus, setCopyStatus] = useState<string | null>(null)

  if (!snapshot.isVisible) {
    return null
  }

  const toneKey = snapshot.tone === 'error' ? 'danger' : snapshot.tone === 'warning' ? 'warning' : 'info'

  const copySummary = async () => {
    setCopyStatus(null)
    try {
      if (!navigator.clipboard?.writeText) {
        throw new Error('Clipboard unavailable')
      }

      await navigator.clipboard.writeText(snapshot.diagnosticSummary)
      setCopyStatus('Support summary copied to clipboard.')
    } catch {
      setCopyStatus('Copy failed. Select and copy the support summary manually.')
    }
  }

  return (
    <section
      className="stl-tone-surface rounded-2xl border p-5 shadow-lg shadow-black/20"
      data-tone={toneKey}
      data-testid="fieldcompanion-degraded-operation-panel"
      aria-live="polite"
    >
      <div className="flex items-start gap-3">
        <TriangleAlert className="mt-0.5 h-5 w-5 shrink-0 text-current" aria-hidden />
        <div className="min-w-0">
          <p className="text-xs font-semibold uppercase tracking-wide text-current/70">
            Recovery guidance
          </p>
          <h2 className="mt-1 text-xl font-semibold text-current">{snapshot.title}</h2>
          <p className="mt-2 text-sm text-current/90">{snapshot.summary}</p>
        </div>
      </div>

      {snapshot.issueLabels.length > 0 ? (
        <div className="mt-4 flex flex-wrap gap-2">
          {snapshot.issueLabels.map((issue) => (
            <span
              key={issue}
              className="rounded-full border border-current/20 px-2.5 py-1 text-xs font-medium text-current/90"
            >
              {issue}
            </span>
          ))}
        </div>
      ) : null}

      {snapshot.recommendedSteps.length > 0 ? (
        <ol className="mt-4 space-y-2 text-sm text-current/90">
          {snapshot.recommendedSteps.map((step, index) => (
            <li
              key={step}
              className="flex gap-3 rounded-xl border border-current/15 bg-black/5 px-3 py-2"
            >
              <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full border border-current/20 text-xs font-semibold text-current/70">
                {index + 1}
              </span>
              <span>{step}</span>
            </li>
          ))}
        </ol>
      ) : null}

      <div className="mt-4 flex flex-wrap gap-2">
        {actions.map((action) =>
          action.href ? (
            <a
              key={action.label}
              href={action.href}
              className="inline-flex min-h-11 items-center gap-2 rounded-lg bg-[var(--color-bg-control)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
              data-testid={action.testId}
            >
              {action.label}
              <ArrowRight className="h-4 w-4" aria-hidden />
            </a>
          ) : (
            <button
              key={action.label}
              type="button"
              className="inline-flex min-h-11 items-center gap-2 rounded-lg bg-[var(--color-bg-control)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
              onClick={action.onClick}
              data-testid={action.testId}
            >
              {action.label}
            </button>
          ),
        )}
        <button
          type="button"
          className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-[var(--color-border-subtle)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
          onClick={() => {
            void copySummary()
          }}
          data-testid="fieldcompanion-degraded-operation-copy"
        >
          <ClipboardCopy className="h-4 w-4" aria-hidden />
          Copy support summary
        </button>
      </div>

      {copyStatus ? (
        <p
          className="mt-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-xs text-[var(--color-text-primary)]"
          data-testid="fieldcompanion-degraded-operation-copy-status"
        >
          {copyStatus}
        </p>
      ) : null}

      <details className="mt-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm">
        <summary className="cursor-pointer font-medium text-[var(--color-text-primary)]">Support summary</summary>
        <pre className="mt-3 whitespace-pre-wrap break-words text-xs text-current/85">
          {snapshot.diagnosticSummary}
        </pre>
      </details>
    </section>
  )
}
