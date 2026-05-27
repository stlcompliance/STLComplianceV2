import type { ReactNode } from 'react'

export function DashboardCard({
  title,
  children,
  className = '',
}: {
  title: string
  children: ReactNode
  className?: string
}) {
  return (
    <section
      className={`rounded-lg border border-slate-700 bg-slate-900/60 p-4 ${className}`.trim()}
    >
      <h4 className="text-sm font-semibold text-slate-100">{title}</h4>
      <div className="mt-3">{children}</div>
    </section>
  )
}
