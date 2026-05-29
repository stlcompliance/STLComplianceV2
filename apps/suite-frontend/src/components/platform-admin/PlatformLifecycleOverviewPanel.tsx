import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import * as nexarr from '../../api/nexarrClient'

export function PlatformLifecycleOverviewPanel() {
  const overviewQuery = useQuery({
    queryKey: ['platform-lifecycle-overview'],
    queryFn: () => nexarr.getPlatformLifecycleOverview(),
  })

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading platform lifecycle overview…</p>
  }

  if (overviewQuery.isError) {
    return (
      <p className="text-sm text-red-700" role="alert">
        Failed to load overview: {(overviewQuery.error as Error).message}
      </p>
    )
  }

  const overview = overviewQuery.data!

  return (
    <div className="space-y-4" data-testid="platform-lifecycle-overview">
      <p className="text-xs text-slate-500">
        Generated {new Date(overview.generatedAt).toLocaleString()} · shared-worker and nexarr-worker
        jobs call NexArr internal batch APIs with dedicated service-token scopes.
      </p>

      <div className="grid gap-4 lg:grid-cols-3">
        {overview.workers.map((worker) => (
          <section
            key={worker.workerKey}
            className="rounded-lg border border-slate-200 bg-white p-4"
            data-testid={`lifecycle-worker-${worker.workerKey}`}
          >
            <div className="flex items-start justify-between gap-2">
              <div>
                <h4 className="font-semibold text-stl-navy">{worker.label}</h4>
                <p className="mt-1 text-xs text-slate-600">{worker.description}</p>
              </div>
              <span
                className={[
                  'rounded-full px-2 py-0.5 text-xs font-medium',
                  worker.isEnabled
                    ? 'bg-emerald-100 text-emerald-800'
                    : 'bg-slate-100 text-slate-600',
                ].join(' ')}
              >
                {worker.isEnabled ? 'Enabled' : 'Disabled'}
              </span>
            </div>

            <dl className="mt-3 space-y-1 text-sm text-slate-700">
              <div className="flex justify-between gap-2">
                <dt>Pending (sample)</dt>
                <dd className="font-medium tabular-nums">{worker.pendingCount}</dd>
              </div>
              {worker.latestRun ? (
                <>
                  <div className="flex justify-between gap-2">
                    <dt>Last run</dt>
                    <dd>{new Date(worker.latestRun.processedAt).toLocaleString()}</dd>
                  </div>
                  <div className="flex justify-between gap-2">
                    <dt>Outcome</dt>
                    <dd className="font-medium">{worker.latestRun.outcome}</dd>
                  </div>
                  <div className="flex justify-between gap-2">
                    <dt>{worker.latestRun.primaryCountLabel}</dt>
                    <dd className="font-medium tabular-nums">{worker.latestRun.primaryCount}</dd>
                  </div>
                </>
              ) : (
                <p className="text-xs text-slate-500">No batch runs recorded yet.</p>
              )}
            </dl>

            <p className="mt-3 font-mono text-[10px] text-slate-500">{worker.serviceTokenScope}</p>

            <Link
              to={worker.suiteAdminPath}
              className="mt-3 inline-block text-sm font-medium text-stl-teal hover:underline"
            >
              Open settings →
            </Link>
          </section>
        ))}
      </div>
    </div>
  )
}
