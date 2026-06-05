import { createProductHandoff } from '@stl/shared-ui'
import { ArrowRightLeft, ExternalLink } from 'lucide-react'
import { useState } from 'react'

const apiBase = import.meta.env.VITE_SUPPLYARR_API_BASE ?? ''

type Props = {
  accessToken: string
  title: string
  description: string
  metrics: ReadonlyArray<{ label: string; value: string | number }>
}

export function LoadArrHandoffPanel({ accessToken, title, description, metrics }: Props) {
  const [isLaunching, setIsLaunching] = useState(false)
  const [launchError, setLaunchError] = useState<string | null>(null)

  async function handleOpenLoadArr() {
    if (isLaunching) {
      return
    }

    setLaunchError(null)
    setIsLaunching(true)

    try {
      const handoff = await createProductHandoff(
        apiBase,
        accessToken,
        'loadarr',
        window.location.href,
      )
      window.location.assign(handoff.launchUrl)
    } catch (error) {
      setLaunchError(
        error instanceof Error ? error.message : 'Failed to launch LoadArr through the suite handoff.',
      )
      setIsLaunching(false)
    }
  }

  return (
    <section className="rounded-xl border border-cyan-900/70 bg-cyan-950/30 p-5">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="max-w-2xl">
          <div className="flex items-center gap-2 text-cyan-200">
            <ArrowRightLeft className="h-4 w-4" aria-hidden="true" />
            <p className="text-xs font-semibold uppercase tracking-[0.18em]">
              LoadArr owner handoff
            </p>
          </div>
          <h3 className="mt-2 text-lg font-semibold text-white">{title}</h3>
          <p className="mt-2 text-sm text-cyan-50/85">{description}</p>
        </div>
        <button
          type="button"
          onClick={() => void handleOpenLoadArr()}
          disabled={isLaunching}
          className="inline-flex items-center gap-2 rounded-md border border-cyan-400/60 bg-cyan-500/15 px-3 py-2 text-sm font-medium text-cyan-100 transition hover:bg-cyan-500/25 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <ExternalLink className="h-4 w-4" aria-hidden="true" />
          {isLaunching ? 'Opening LoadArr…' : 'Open in LoadArr'}
        </button>
      </div>

      <dl className="mt-4 grid gap-3 sm:grid-cols-3">
        {metrics.map((metric) => (
          <div
            key={metric.label}
            className="rounded-lg border border-cyan-900/60 bg-slate-950/45 px-3 py-3"
          >
            <dt className="text-xs uppercase tracking-wide text-cyan-200/80">{metric.label}</dt>
            <dd className="mt-1 text-lg font-semibold text-white">{metric.value}</dd>
          </div>
        ))}
      </dl>

      {launchError ? <p className="mt-3 text-sm text-rose-300">{launchError}</p> : null}
    </section>
  )
}
