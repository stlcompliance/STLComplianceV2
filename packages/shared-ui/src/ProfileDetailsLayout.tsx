import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'

export type DetailTone = 'good' | 'warn' | 'bad' | 'info' | 'neutral'

export interface DetailBadgeConfig {
  label: string
  tone?: DetailTone
}

export interface DetailMetricConfig {
  label: string
  value: string | number
  hint: string
  icon?: ReactNode
  tone?: DetailTone
}

export interface DetailSnapshotFieldConfig {
  label: string
  value: string | number
  source: string
}

export interface DetailRailSectionConfig {
  title: string
  icon?: ReactNode
  content: ReactNode
}

export interface ProfileDetailsLayoutProps {
  testId?: string
  backLabel: string
  backTo: string
  breadcrumbs: string[]
  icon: ReactNode
  title: string
  subtitle: ReactNode
  badges: DetailBadgeConfig[]
  actions?: ReactNode
  metrics: DetailMetricConfig[]
  tabs: string[]
  snapshotTitle: string
  snapshotSubtitle: string
  snapshotFields: DetailSnapshotFieldConfig[]
  fieldSourceLabel?: string
  mainContent?: ReactNode
  decisionTitle: string
  decisionBadge: DetailBadgeConfig
  decisionIcon?: ReactNode
  decisionSummary: string
  decisionDetail: string
  allowedChecks: number
  blockedChecks: number
  railSections: DetailRailSectionConfig[]
}

function badgeClass(tone: DetailTone): string {
  if (tone === 'good') return 'border-emerald-400/30 bg-emerald-500/15 text-emerald-200'
  if (tone === 'warn') return 'border-amber-400/30 bg-amber-500/15 text-amber-200'
  if (tone === 'bad') return 'border-red-400/30 bg-red-500/15 text-red-200'
  if (tone === 'info') return 'border-sky-400/30 bg-sky-500/15 text-sky-200'
  return 'border-slate-500/30 bg-slate-500/10 text-slate-300'
}

function decisionClass(tone: DetailTone): string {
  if (tone === 'good') return 'border-emerald-500/30 bg-emerald-950/20'
  if (tone === 'bad') return 'border-red-500/30 bg-red-950/20'
  if (tone === 'warn') return 'border-amber-500/30 bg-amber-950/20'
  return 'border-sky-500/30 bg-sky-950/20'
}

function metricIconClass(tone: DetailTone): string {
  if (tone === 'good') return 'bg-emerald-500/15 text-emerald-300'
  if (tone === 'warn') return 'bg-amber-500/15 text-amber-300'
  if (tone === 'bad') return 'bg-red-500/15 text-red-300'
  if (tone === 'info') return 'bg-sky-500/15 text-sky-300'
  return 'bg-slate-700/60 text-slate-300'
}

export function DetailBadge({ label, tone = 'neutral' }: DetailBadgeConfig) {
  return (
    <span className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold ${badgeClass(tone)}`}>
      {label}
    </span>
  )
}

export function DetailEmptyState({ text }: { text: string }) {
  return (
    <div className="rounded-xl border border-slate-800 bg-slate-950/40 p-4 text-sm text-slate-400">
      {text}
    </div>
  )
}

export function ProfileDetailsLayout({
  testId,
  backLabel,
  backTo,
  breadcrumbs,
  icon,
  title,
  subtitle,
  badges,
  actions,
  metrics,
  tabs,
  snapshotTitle,
  snapshotSubtitle,
  snapshotFields,
  fieldSourceLabel = 'Field sources',
  mainContent,
  decisionTitle,
  decisionBadge,
  decisionIcon,
  decisionSummary,
  decisionDetail,
  allowedChecks,
  blockedChecks,
  railSections,
}: ProfileDetailsLayoutProps) {
  const activeTone = decisionBadge.tone ?? 'neutral'

  return (
    <div className="space-y-6" data-testid={testId}>
      <section className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-2xl shadow-sky-950/20">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div className="min-w-0">
            <nav className="mb-5 flex flex-wrap items-center gap-3 text-sm text-sky-200/80">
              <Link
                to={backTo}
                className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-950/60 px-3 py-2 hover:border-sky-700"
              >
                {backLabel}
              </Link>
              {breadcrumbs.map((crumb) => (
                <span key={crumb} className="inline-flex items-center gap-3">
                  <span className="text-slate-600">/</span>
                  <span className={crumb === title ? 'font-semibold text-white' : ''}>{crumb}</span>
                </span>
              ))}
            </nav>
            <div className="flex items-start gap-4">
              <div className="flex h-[4.5rem] w-[4.5rem] shrink-0 items-center justify-center rounded-2xl border border-sky-700/50 bg-sky-500/15 text-sky-300">
                {icon}
              </div>
              <div className="min-w-0">
                <div className="mb-3 flex flex-wrap gap-2">
                  {badges.map((badge) => (
                    <DetailBadge key={`${badge.label}-${badge.tone ?? 'neutral'}`} {...badge} />
                  ))}
                </div>
                <h1 className="break-words text-3xl font-bold tracking-normal text-white">{title}</h1>
                <div className="mt-2 text-sm text-sky-100/75">{subtitle}</div>
              </div>
            </div>
          </div>
          {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
        </div>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => {
          const tone = metric.tone ?? 'neutral'
          return (
            <section key={metric.label} className="min-h-32 rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm text-sky-200/80">{metric.label}</p>
                  <p className="mt-3 text-3xl font-bold tracking-normal text-white">{metric.value}</p>
                </div>
                {metric.icon ? <div className={`rounded-xl p-3 ${metricIconClass(tone)}`}>{metric.icon}</div> : null}
              </div>
              <p className="mt-2 text-xs text-slate-400">{metric.hint}</p>
            </section>
          )
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_28rem]">
        <section className="overflow-hidden rounded-2xl border border-slate-800 bg-slate-950/70">
          <div className="flex overflow-x-auto border-b border-slate-800">
            {tabs.map((tab, index) => (
              <button
                key={tab}
                type="button"
                role="tab"
                aria-selected={index === 0}
                className={`shrink-0 px-5 py-4 text-sm font-semibold ${
                  index === 0 ? 'bg-slate-900 text-sky-300' : 'text-sky-200/75 hover:bg-slate-900/50'
                }`}
              >
                {tab}
              </button>
            ))}
          </div>
          <div className="space-y-6 p-6">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h2 className="text-xl font-bold text-white">{snapshotTitle}</h2>
                <p className="mt-1 text-sm text-sky-100/70">{snapshotSubtitle}</p>
              </div>
              <button
                type="button"
                className="inline-flex items-center gap-2 rounded-xl border border-slate-800 bg-slate-900 px-4 py-2 text-sm font-semibold text-sky-100"
              >
                {fieldSourceLabel}
              </button>
            </div>
            <dl className="grid gap-3 md:grid-cols-2 2xl:grid-cols-3">
              {snapshotFields.map((field) => (
                <div key={field.label} className="min-h-[4.5rem] rounded-xl border border-slate-800 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <dt className="text-xs font-semibold uppercase tracking-normal text-sky-200/55">{field.label}</dt>
                    <span className="shrink-0 text-[10px] text-slate-500">{field.source}</span>
                  </div>
                  <dd className="mt-3 break-words text-sm font-semibold text-white">{field.value}</dd>
                </div>
              ))}
            </dl>
            {mainContent}
          </div>
        </section>

        <aside className="space-y-6">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h2 className="text-lg font-bold text-white">{decisionTitle}</h2>
              <DetailBadge {...decisionBadge} />
            </div>
            <div className={`rounded-2xl border p-5 ${decisionClass(activeTone)}`}>
              <div className="flex gap-3">
                {decisionIcon ? <div className="mt-1 shrink-0">{decisionIcon}</div> : null}
                <div>
                  <h3 className="font-bold text-white">{decisionSummary}</h3>
                  <p className="mt-2 text-sm leading-6 text-sky-100/80">{decisionDetail}</p>
                </div>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3">
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Allowed checks</p>
                <p className="mt-2 text-xl font-bold text-white">{allowedChecks}</p>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Blocked checks</p>
                <p className="mt-2 text-xl font-bold text-white">{blockedChecks}</p>
              </div>
            </div>
          </section>

          {railSections.map((section) => (
            <section key={section.title} className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5">
              <div className="mb-4 flex items-center justify-between gap-3">
                <h2 className="text-lg font-bold text-white">{section.title}</h2>
                {section.icon ? <div className="text-sky-300">{section.icon}</div> : null}
              </div>
              {section.content}
            </section>
          ))}
        </aside>
      </div>
    </div>
  )
}
