import type { ReactNode } from 'react'
import { useHintsPreference } from './HintsPreferenceContext'
import { useRegisterPrintableSurface } from './print/PrintRuntime'
import type { PrintableSurfaceRegistration } from './print/types'

export function PageHeader({
  title,
  subtitle,
  eyebrow = 'STL Compliance',
  actions,
  printRegistration,
}: {
  title: string
  subtitle?: string
  eyebrow?: string
  actions?: ReactNode
  printRegistration?: Partial<PrintableSurfaceRegistration> | false
}) {
  const { showHints } = useHintsPreference()
  useRegisterPrintableSurface(
    printRegistration === false
      ? false
      : {
          title,
          sourceDisplayRef: title,
          documentStatus: 'working_copy',
          ...printRegistration,
        },
  )

  return (
    <header
      className="mb-6 flex flex-col gap-4 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm shadow-slate-950/10 sm:flex-row sm:items-end sm:justify-between"
      data-print-section="page-header"
    >
      <div className="min-w-0">
        <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-accent)]">{eyebrow}</p>
        <h1 className="mt-1 break-words text-2xl font-semibold tracking-normal text-[var(--color-text-primary)]">
          {title}
        </h1>
        {subtitle && showHints ? (
          <p className="mt-1 max-w-3xl text-sm leading-6 text-[var(--color-text-muted)]">{subtitle}</p>
        ) : null}
      </div>
      {actions ? (
        <div className="flex shrink-0 flex-wrap items-center gap-2" data-print-hide>
          {actions}
        </div>
      ) : null}
    </header>
  )
}
