import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { formatDisplayLabel } from './displayLabels'

export type DetailTone =
  | 'neutral'
  | 'info'
  | 'success'
  | 'warning'
  | 'danger'
  | 'destructive'
  | 'pending'
  | 'inactive'
  | 'draft'
  | 'compliant'
  | 'non_compliant'
  | 'needs_review'
  | 'good'
  | 'warn'
  | 'bad'

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

export interface DetailTabConfig {
  key: string
  label: string
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
  tabs: Array<string | DetailTabConfig>
  activeTab?: string
  onTabChange?: (tabKey: string) => void
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

function normalizeDetailTone(tone: DetailTone): Exclude<DetailTone, 'good' | 'warn' | 'bad'> {
  if (tone === 'good') return 'success'
  if (tone === 'warn') return 'warning'
  if (tone === 'bad') return 'danger'
  return tone
}

export function DetailBadge({ label, tone = 'neutral' }: DetailBadgeConfig) {
  return (
    <span
      className="stl-tone-badge inline-flex items-center rounded-full border px-3 py-1 text-xs font-semibold"
      data-tone={normalizeDetailTone(tone)}
    >
      {formatDisplayLabel(label)}
    </span>
  )
}

export function DetailEmptyState({ text }: { text: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-700 bg-slate-950/40 p-4 text-sm text-slate-400">
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
  activeTab,
  onTabChange,
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
  const normalizedTabs = tabs.map((tab) =>
    typeof tab === 'string' ? { key: tab, label: tab } : tab,
  )

  return (
    <div className="space-y-6" data-testid={testId}>
      <section className="rounded-lg border border-slate-800 bg-slate-950/80 p-4 shadow-sm shadow-slate-950/30 sm:p-5">
        <div className="flex flex-wrap items-start justify-between gap-5">
          <div className="min-w-0">
            <nav className="mb-5 flex flex-wrap items-center gap-2 text-sm text-sky-200/80" aria-label="Breadcrumb">
              <Link
                to={backTo}
                className="inline-flex items-center gap-2 rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2 transition hover:border-sky-700 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400"
              >
                {backLabel}
              </Link>
              {breadcrumbs.map((crumb) => (
                <span key={crumb} className="inline-flex items-center gap-3">
                  <span className="text-[var(--color-text-muted)]">/</span>
                  <span className={crumb === title ? 'font-semibold text-white' : ''}>{crumb}</span>
                </span>
              ))}
            </nav>
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start">
              <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-lg border border-sky-700/50 bg-sky-500/15 text-sky-300 sm:h-16 sm:w-16">
                {icon}
              </div>
              <div className="min-w-0">
                <div className="mb-3 flex flex-wrap gap-2">
                  {badges.map((badge) => (
                    <DetailBadge key={`${badge.label}-${badge.tone ?? 'neutral'}`} {...badge} />
                  ))}
                </div>
                <h1 className="break-words text-2xl font-bold tracking-normal text-white sm:text-3xl">{title}</h1>
                <div className="mt-2 text-sm text-sky-100/75">{subtitle}</div>
              </div>
            </div>
          </div>
          {actions ? <div className="flex flex-wrap gap-2 sm:justify-end">{actions}</div> : null}
        </div>
      </section>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => {
          const tone = metric.tone ?? 'neutral'
          return (
            <section key={metric.label} className="min-h-28 rounded-lg border border-slate-800 bg-slate-950/70 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-sm text-sky-200/80">{metric.label}</p>
                  <p className="mt-3 text-3xl font-bold tracking-normal text-white">{metric.value}</p>
                </div>
                {metric.icon ? (
                  <div className="stl-tone-icon rounded-lg p-3" data-tone={normalizeDetailTone(tone)}>
                    {metric.icon}
                  </div>
                ) : null}
              </div>
              <p className="mt-2 text-xs text-slate-400">{metric.hint}</p>
            </section>
          )
        })}
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_28rem]">
        <section className="min-w-0 overflow-hidden rounded-lg border border-slate-800 bg-slate-950/70">
          <div className="flex overflow-x-auto border-b border-slate-800">
            {normalizedTabs.map((tab, index) => {
              const isActive = activeTab ? tab.key === activeTab : index === 0
              return (
              <button
                key={tab.key}
                type="button"
                role="tab"
                aria-selected={isActive}
                className={`shrink-0 px-5 py-4 text-sm font-semibold transition focus-visible:outline focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-sky-400 ${
                  isActive ? 'bg-slate-900 text-sky-300' : 'text-sky-200/75 hover:bg-slate-900/50'
                }`}
                onClick={() => onTabChange?.(tab.key)}
              >
                {tab.label}
              </button>
            )})}
          </div>
          <div className="space-y-6 p-4 sm:p-5">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <h2 className="text-xl font-bold text-white">{snapshotTitle}</h2>
                <p className="mt-1 text-sm text-sky-100/70">{snapshotSubtitle}</p>
              </div>
              <button
                type="button"
                className="inline-flex items-center gap-2 rounded-lg border border-slate-800 bg-slate-900 px-4 py-2 text-sm font-semibold text-sky-100 transition hover:border-sky-600 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400"
              >
                {fieldSourceLabel}
              </button>
            </div>
            <dl className="grid gap-3 md:grid-cols-2 2xl:grid-cols-3">
              {snapshotFields.map((field) => (
                <div key={field.label} className="min-h-[4.5rem] rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <dt className="text-xs font-semibold uppercase tracking-normal text-sky-200/55">{field.label}</dt>
                    <span className="max-w-[9rem] shrink-0 text-right text-[10px] leading-4 text-[var(--color-text-muted)]">{field.source}</span>
                  </div>
                  <dd className="mt-3 break-words text-sm font-semibold text-white">{field.value}</dd>
                </div>
              ))}
            </dl>
            {mainContent}
          </div>
        </section>

        <aside className="min-w-0 space-y-6">
          <section className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h2 className="text-lg font-bold text-white">{decisionTitle}</h2>
              <DetailBadge {...decisionBadge} />
            </div>
            <div className="stl-tone-surface rounded-lg border p-5" data-tone={normalizeDetailTone(activeTone)}>
              <div className="flex gap-3">
                {decisionIcon ? <div className="mt-1 shrink-0">{decisionIcon}</div> : null}
                <div>
                  <h3 className="font-bold text-white">{decisionSummary}</h3>
                  <p className="mt-2 text-sm leading-6 text-sky-100/80">{decisionDetail}</p>
                </div>
              </div>
            </div>
            <div className="mt-4 grid grid-cols-2 gap-3">
              <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Allowed checks</p>
                <p className="mt-2 text-xl font-bold text-white">{allowedChecks}</p>
              </div>
              <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Blocked checks</p>
                <p className="mt-2 text-xl font-bold text-white">{blockedChecks}</p>
              </div>
            </div>
          </section>

          {railSections.map((section) => (
            <section key={section.title} className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
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
