import type { ReactNode } from 'react'
import { useHintsPreference } from '@stl/shared-ui'

type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

function adminTone(tone: Tone) {
  if (tone === 'good') return 'success'
  if (tone === 'warn') return 'warning'
  if (tone === 'bad') return 'danger'
  if (tone === 'info') return 'info'
  return 'neutral'
}

export function PlatformAdminPageHeader({
  title,
  summary,
  updatedAt,
  badge,
  actions,
}: {
  title: string
  summary: string
  updatedAt?: string | null
  badge?: string
  actions?: ReactNode
}) {
  const { showHints } = useHintsPreference()
  return (
    <header className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">
              NexArr platform admin
            </p>
            {badge ? (
              <span className="stl-tone-badge rounded-full border px-2.5 py-0.5 text-xs font-medium" data-tone="info">
                {badge}
              </span>
            ) : null}
          </div>
          <h1 className="mt-2 text-2xl font-semibold text-stl-navy">{title}</h1>
          {showHints ? <p className="mt-1 max-w-3xl text-sm text-[var(--color-text-muted)]">{summary}</p> : null}
          {showHints && updatedAt ? (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">Last updated {updatedAt}</p>
          ) : null}
        </div>
        {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
      </div>
    </header>
  )
}

export function PlatformAdminKpiCard({
  label,
  value,
  hint,
  tone = 'neutral',
}: {
  label: string
  value: string | number
  hint: string
  tone?: Tone
}) {
  const { showHints } = useHintsPreference()
  return (
    <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">
            {label}
          </p>
          <p className="mt-2 text-3xl font-semibold text-stl-navy">{value}</p>
        </div>
        <span
          className="stl-tone-badge rounded-full border px-2.5 py-0.5 text-xs font-medium"
          data-tone={adminTone(tone)}
        >
          {tone === 'good' ? 'Healthy' : tone === 'warn' ? 'Watch' : tone === 'bad' ? 'Blocked' : tone === 'info' ? 'Info' : 'Neutral'}
        </span>
      </div>
      {showHints ? <p className="mt-2 text-xs text-[var(--color-text-muted)]">{hint}</p> : null}
    </section>
  )
}

export function PlatformAdminSection({
  title,
  description,
  children,
}: {
  title: string
  description?: string
  children: ReactNode
}) {
  const { showHints } = useHintsPreference()
  return (
    <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-sm">
      <div>
        <h2 className="text-lg font-semibold text-stl-navy">{title}</h2>
        {description && showHints ? <p className="mt-1 text-sm text-[var(--color-text-muted)]">{description}</p> : null}
      </div>
      <div className="mt-4">{children}</div>
    </section>
  )
}

export function PlatformAdminScopeNote({
  children,
}: {
  children: ReactNode
}) {
  const { showHints } = useHintsPreference()
  if (!showHints) {
    return null
  }

  return <p className="text-xs leading-6 text-[var(--color-text-muted)]">{children}</p>
}
