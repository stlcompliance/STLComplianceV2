import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import type { JourneySeedResultResponse } from '../../api/types'
import { useToast } from '../../feedback'

export function PlatformJourneySeedsPage() {
  const { pushToast } = useToast()
  const [resultsByProduct, setResultsByProduct] = useState<Record<string, JourneySeedResultResponse>>({})

  const targetsQuery = useQuery({
    queryKey: ['platform-admin-journey-seed-targets'],
    queryFn: () => nexarr.getPlatformJourneySeedTargets(),
  })

  const seedMutation = useMutation({
    mutationFn: (productKey: string) => nexarr.seedPlatformJourney(productKey),
    onSuccess: (result) => {
      setResultsByProduct((current) => ({
        ...current,
        [result.productKey]: result,
      }))
      pushToast({
        message: `${result.displayName} journey seed ${result.succeeded ? 'completed' : 'returned an error'}.`,
        variant: result.succeeded ? 'success' : 'info',
      })
    },
    onError: (error: Error) => {
      pushToast({ message: error.message, variant: 'error' })
    },
  })

  const targets = targetsQuery.data ?? []

  return (
    <div className="space-y-6">
      <header>
        <h4 className="text-lg font-semibold text-stl-navy">Journey seeds</h4>
        <p className="mt-1 text-sm text-slate-600">
          Trigger the non-tenant load-test business seeds from NexArr so product-specific setup stays in one place.
        </p>
      </header>

      {targetsQuery.isError ? (
        <ApiErrorCallout
          message={getErrorMessage(targetsQuery.error, 'Failed to load journey seed targets.')}
          onRetry={() => void targetsQuery.refetch()}
          retryLabel="Retry targets"
        />
      ) : null}

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        {targetsQuery.isLoading ? (
          <p className="text-sm text-slate-500">Loading journey seed targets…</p>
        ) : targets.length === 0 ? (
          <p className="text-sm text-slate-500">No journey seed targets are configured.</p>
        ) : (
          <div className="grid gap-4 lg:grid-cols-2">
            {targets.map((target) => {
              const result = resultsByProduct[target.productKey]
              const isRunning = seedMutation.isPending && seedMutation.variables === target.productKey
              return (
                <article
                  key={target.productKey}
                  className="rounded-lg border border-slate-200 bg-slate-50 p-4"
                  data-testid={`journey-seed-target-${target.productKey}`}
                >
                  <div className="flex flex-wrap items-start gap-3">
                    <div>
                      <h5 className="font-semibold text-stl-navy">{target.displayName}</h5>
                      <p className="mt-1 text-sm text-slate-600">{target.description}</p>
                    </div>
                    <span
                      className={[
                        'ml-auto rounded-full px-2 py-0.5 text-xs font-medium',
                        target.isConfigured
                          ? 'bg-emerald-100 text-emerald-800'
                          : 'bg-rose-100 text-rose-700',
                      ].join(' ')}
                    >
                      {target.isConfigured ? 'Configured' : 'Missing URL'}
                    </span>
                  </div>

                  <dl className="mt-4 space-y-1 text-sm text-slate-700">
                    <div className="flex flex-wrap justify-between gap-3">
                      <dt>Seed path</dt>
                      <dd className="font-mono text-xs text-slate-500">{target.seedPath}</dd>
                    </div>
                    <div className="flex flex-wrap justify-between gap-3">
                      <dt>Base URL</dt>
                      <dd className="font-mono text-xs text-slate-500">{target.baseUrl ?? '—'}</dd>
                    </div>
                  </dl>

                  <div className="mt-4 flex flex-wrap items-center gap-3">
                    <button
                      type="button"
                      disabled={!target.isConfigured || isRunning || seedMutation.isPending}
                      onClick={() => seedMutation.mutate(target.productKey)}
                      className="rounded-md bg-stl-navy px-3 py-1.5 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
                    >
                      {isRunning ? 'Seeding…' : 'Seed journey'}
                    </button>
                    <span className="text-xs text-slate-500">
                      {result
                        ? `${result.succeeded ? 'Last run succeeded' : 'Last run returned an error'} at ${new Date(result.requestedAt).toLocaleString()}`
                        : 'No run yet'}
                    </span>
                  </div>

                  {result ? (
                    <div
                      className={[
                        'mt-4 rounded-md border p-3 text-xs',
                        result.succeeded
                          ? 'border-emerald-200 bg-emerald-50 text-emerald-950'
                          : 'border-amber-200 bg-amber-50 text-amber-950',
                      ].join(' ')}
                    >
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="font-medium">Upstream response</span>
                        <span className="ml-auto font-mono">HTTP {result.statusCode}</span>
                      </div>
                      {result.responseBody ? (
                        <pre className="mt-2 max-h-56 overflow-auto whitespace-pre-wrap break-words font-mono text-[11px] leading-5">
                          {result.responseBody}
                        </pre>
                      ) : (
                        <p className="mt-2 text-xs text-slate-500">No response body was returned.</p>
                      )}
                    </div>
                  ) : null}
                </article>
              )
            })}
          </div>
        )}
      </section>
    </div>
  )
}
