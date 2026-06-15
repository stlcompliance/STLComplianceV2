import type { ReactNode } from 'react'

type Tone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

function toneClass(tone: Tone) {
  if (tone === 'good') return 'border-emerald-200 bg-emerald-50 text-emerald-700'
  if (tone === 'warn') return 'border-amber-200 bg-amber-50 text-amber-700'
  if (tone === 'bad') return 'border-rose-200 bg-rose-50 text-rose-700'
  if (tone === 'info') return 'border-sky-200 bg-sky-50 text-sky-700'
  return 'border-slate-200 bg-slate-50 text-slate-600'
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
  return (
    <header className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
              NexArr platform admin
            </p>
            {badge ? (
              <span className="rounded-full border border-sky-200 bg-sky-50 px-2.5 py-0.5 text-xs font-medium text-sky-700">
                {badge}
              </span>
            ) : null}
          </div>
          <h1 className="mt-2 text-2xl font-semibold text-stl-navy">{title}</h1>
          <p className="mt-1 max-w-3xl text-sm text-slate-600">{summary}</p>
          {updatedAt ? (
            <p className="mt-2 text-xs text-slate-500">Last updated {updatedAt}</p>
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
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">
            {label}
          </p>
          <p className="mt-2 text-3xl font-semibold text-stl-navy">{value}</p>
        </div>
        <span className={`rounded-full border px-2.5 py-0.5 text-xs font-medium ${toneClass(tone)}`}>
          {tone === 'good' ? 'Healthy' : tone === 'warn' ? 'Watch' : tone === 'bad' ? 'Blocked' : tone === 'info' ? 'Info' : 'Neutral'}
        </span>
      </div>
      <p className="mt-2 text-xs text-slate-500">{hint}</p>
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
  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
      <div>
        <h2 className="text-lg font-semibold text-stl-navy">{title}</h2>
        {description ? <p className="mt-1 text-sm text-slate-600">{description}</p> : null}
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
  return (
    <p className="text-xs leading-6 text-slate-500">{children}</p>
  )
}
