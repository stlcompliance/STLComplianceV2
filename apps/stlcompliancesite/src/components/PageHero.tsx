import type { ReactNode } from 'react'
import { BrandLogoFrame } from './BrandLogoFrame'

type PageHeroProps = {
  eyebrow?: string
  title: string
  subtitle: string
  brandImageSrc?: string
  brandImageAlt?: string
  backgroundImageSrc?: string
  children?: ReactNode
}

export function PageHero({
  eyebrow,
  title,
  subtitle,
  brandImageSrc,
  brandImageAlt = '',
  backgroundImageSrc,
  children,
}: PageHeroProps) {
  return (
    <section className="relative overflow-hidden border-b border-slate-700/50 bg-slate-900/40">
      {backgroundImageSrc ? (
        <img
          src={backgroundImageSrc}
          alt=""
          className="absolute inset-y-0 right-0 h-full w-full object-cover opacity-20"
          aria-hidden
        />
      ) : null}
      <div className="absolute inset-0 bg-[var(--color-overlay-scrim)]" />
      <div className="relative mx-auto max-w-6xl px-4 py-14 sm:px-6 sm:py-16">
        {brandImageSrc ? (
          <BrandLogoFrame
            src={brandImageSrc}
            alt={brandImageAlt}
            size="lg"
            className="mb-6"
          />
        ) : null}
        {eyebrow && <p className="text-sm font-semibold uppercase text-teal-300">{eyebrow}</p>}
        <h1 className="mt-2 max-w-4xl text-3xl font-bold text-white sm:text-4xl">{title}</h1>
        <p className="mt-4 max-w-3xl text-lg text-slate-300">{subtitle}</p>
        {children && <div className="mt-8 flex flex-wrap gap-3">{children}</div>}
      </div>
    </section>
  )
}
