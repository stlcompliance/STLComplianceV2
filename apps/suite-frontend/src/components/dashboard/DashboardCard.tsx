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
      className={`rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 ${className}`.trim()}
    >
      <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">{title}</h4>
      <div className="mt-3">{children}</div>
    </section>
  )
}
