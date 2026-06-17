import type { ReactNode } from 'react'

export function PageHeader({
  title,
  subtitle,
  eyebrow = 'STL Compliance',
  actions,
}: {
  title: string
  subtitle?: string
  eyebrow?: string
  actions?: ReactNode
}) {
  return (
    <header className="mb-6 flex flex-col gap-4 rounded-lg border border-slate-800 bg-slate-950/60 p-4 shadow-sm shadow-slate-950/20 sm:flex-row sm:items-end sm:justify-between">
      <div className="min-w-0">
        <p className="text-xs font-semibold uppercase tracking-wide text-sky-300">{eyebrow}</p>
        <h1 className="mt-1 break-words text-2xl font-semibold tracking-normal text-white">{title}</h1>
        {subtitle ? <p className="mt-1 max-w-3xl text-sm leading-6 text-slate-400">{subtitle}</p> : null}
      </div>
      {actions ? <div className="flex shrink-0 flex-wrap items-center gap-2">{actions}</div> : null}
    </header>
  )
}
