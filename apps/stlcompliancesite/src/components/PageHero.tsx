import type { ReactNode } from 'react'

type PageHeroProps = {
  eyebrow?: string
  title: string
  subtitle: string
  children?: ReactNode
}

export function PageHero({ eyebrow, title, subtitle, children }: PageHeroProps) {
  return (
    <section className="border-b border-slate-700/50 bg-slate-900/40">
      <div className="mx-auto max-w-6xl px-4 py-14 sm:px-6 sm:py-16">
        {eyebrow && (
          <p className="text-sm font-semibold uppercase tracking-wide text-teal-300">{eyebrow}</p>
        )}
        <h1 className="mt-2 text-3xl font-bold tracking-tight text-white sm:text-4xl">{title}</h1>
        <p className="mt-4 max-w-3xl text-lg text-slate-300">{subtitle}</p>
        {children && <div className="mt-8 flex flex-wrap gap-3">{children}</div>}
      </div>
    </section>
  )
}
