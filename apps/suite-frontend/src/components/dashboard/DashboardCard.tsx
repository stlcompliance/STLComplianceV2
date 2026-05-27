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
      className={`rounded-lg border border-slate-200 bg-white p-4 ${className}`.trim()}
    >
      <h4 className="text-sm font-semibold text-stl-navy">{title}</h4>
      <div className="mt-3">{children}</div>
    </section>
  )
}
