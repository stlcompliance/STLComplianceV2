import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getAuditDeliveryOrchestrationStatus,
  triggerM12AnalyticsBatch,
  triggerScheduledRuleEvaluation,
} from '../api/client'

interface AuditDeliveryOrchestrationPanelProps {
  accessToken: string
  canRead: boolean
  canTrigger: boolean
}

function formatWhen(value: string | null | undefined) {
  if (!value) {
    return 'Never'
  }
  return new Date(value).toLocaleString()
}

export function AuditDeliveryOrchestrationPanel({
  accessToken,
  canRead,
  canTrigger,
}: AuditDeliveryOrchestrationPanelProps) {
  const queryClient = useQueryClient()

  const statusQuery = useQuery({
    queryKey: ['compliancecore-audit-delivery-orchestration', accessToken],
    queryFn: () => getAuditDeliveryOrchestrationStatus(accessToken),
    enabled: canRead,
    refetchInterval: 30_000,
  })

  const scheduledMutation = useMutation({
    mutationFn: () => triggerScheduledRuleEvaluation(accessToken),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['compliancecore-audit-delivery-orchestration', accessToken],
      })
    },
  })

  const m12Mutation = useMutation({
    mutationFn: () => triggerM12AnalyticsBatch(accessToken),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['compliancecore-audit-delivery-orchestration', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['compliancecore-m12-analytics-worker-settings', accessToken],
      })
    },
  })

  if (!canRead) {
    return null
  }

  const status = statusQuery.data
  const scheduled = status?.scheduledEvaluation
  const m12 = status?.m12Batch
  const auditPackages = status?.auditPackages
  const worker = status?.workerSettings

  return (
    <section
      data-testid="compliancecore-audit-delivery-orchestration-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Audit delivery orchestration</h2>
        <p className="mt-1 text-sm text-slate-400">
          Scheduled rule pack evaluation (W47), M12 analytics batches (W231), and audit package
          generation jobs in one operational view.
        </p>
      </header>

      {statusQuery.isLoading ? (
        <p className="text-sm text-[var(--color-text-muted)]">Loading orchestration status…</p>
      ) : null}
      {statusQuery.isError ? (
        <ApiErrorCallout
          title="Orchestration status unavailable"
          message={getErrorMessage(statusQuery.error, 'Failed to load orchestration status.')}
          retryLabel="Retry status"
          onRetry={() => {
            void statusQuery.refetch()
          }}
        />
      ) : null}

      {status ? (
        <div className="space-y-4">
          <div
            data-testid="compliancecore-orchestration-scheduled-eval"
            className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
          >
            <h3 className="text-sm font-medium text-slate-200">Scheduled rule evaluation</h3>
            <p className="mt-2 text-sm text-slate-400">
              Pending packs:{' '}
              <span className="font-mono text-slate-200">{scheduled?.pendingPacksCount ?? 0}</span>
            </p>
            {scheduled?.lastRun ? (
              <p className="mt-1 text-sm text-slate-400" data-testid="compliancecore-orchestration-scheduled-last-run">
                Last run {formatWhen(scheduled.lastRun.completedAt ?? scheduled.lastRun.startedAt)} ·{' '}
                {scheduled.lastRun.status} · {scheduled.lastRun.evaluatedCount} evaluated
              </p>
            ) : (
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">No scheduled evaluation runs yet.</p>
            )}
            {canTrigger ? (
              <button
                type="button"
                onClick={() => scheduledMutation.mutate()}
                disabled={scheduledMutation.isPending}
                data-testid="compliancecore-orchestration-trigger-scheduled-eval"
                className="mt-3 rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              >
                {scheduledMutation.isPending ? 'Running…' : 'Run scheduled evaluation now'}
              </button>
            ) : null}
          </div>

          <div
            data-testid="compliancecore-orchestration-m12-batch"
            className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
          >
            <h3 className="text-sm font-medium text-slate-200">M12 analytics batch</h3>
            <p className="mt-2 text-sm text-slate-400">
              Worker:{' '}
              <span className="font-mono text-slate-200">
                {m12?.workerEnabled ? 'enabled' : 'disabled'}
              </span>
              {m12?.batchDue ? (
                <span className="ml-2 rounded-md bg-amber-900/50 px-2 py-0.5 text-xs text-amber-200">
                  steps due
                </span>
              ) : null}
            </p>
            {m12?.pendingSteps ? (
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Due: risk {m12.pendingSteps.riskScoringDue ? 'yes' : 'no'} · missing evidence{' '}
                {m12.pendingSteps.missingEvidenceDue ? 'yes' : 'no'} · control{' '}
                {m12.pendingSteps.controlEffectivenessDue ? 'yes' : 'no'} · forecast{' '}
                {m12.pendingSteps.readinessForecastDue ? 'yes' : 'no'} · audit ZIP{' '}
                {m12.pendingSteps.auditDeliveryDue ? 'yes' : 'no'}
              </p>
            ) : null}
            {m12?.lastRun ? (
              <p className="mt-1 text-sm text-slate-400" data-testid="compliancecore-orchestration-m12-last-run">
                Last batch {formatWhen(m12.lastRun.completedAt ?? m12.lastRun.startedAt)} ·{' '}
                {m12.lastRun.status}
                {m12.lastRun.auditDeliveryQueued && m12.lastRun.auditPackageJobId
                  ? ` · audit job ${m12.lastRun.auditPackageJobId}`
                  : ''}
              </p>
            ) : (
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">No M12 batch runs yet.</p>
            )}
            <p className="mt-1 text-sm text-[var(--color-text-muted)]">
              Last audit delivery hook: {formatWhen(worker?.lastAuditDeliveryRunAt)}
            </p>
            {canTrigger ? (
              <button
                type="button"
                onClick={() => m12Mutation.mutate()}
                disabled={m12Mutation.isPending || !m12?.workerEnabled}
                data-testid="compliancecore-orchestration-trigger-m12-batch"
                className="mt-3 rounded-md bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-500 disabled:opacity-50"
              >
                {m12Mutation.isPending ? 'Running…' : 'Run M12 batch now'}
              </button>
            ) : null}
          </div>

          <div
            data-testid="compliancecore-orchestration-audit-jobs"
            className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
          >
            <h3 className="text-sm font-medium text-slate-200">Audit package jobs</h3>
            <p className="mt-2 text-sm text-slate-400">
              Pending/processing:{' '}
              <span className="font-mono text-slate-200">
                {auditPackages?.pendingJobsCount ?? 0}
              </span>
            </p>
            {auditPackages && auditPackages.recentJobs.length > 0 ? (
              <ul className="mt-3 divide-y divide-slate-800 text-sm text-slate-300">
                {auditPackages.recentJobs.map((job) => (
                  <li key={job.jobId} className="py-2">
                    <span className="font-mono text-sky-300">{job.jobId.slice(0, 8)}…</span> ·{' '}
                    {job.status} · {job.format} · {formatWhen(job.createdAt)}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-2 text-sm text-[var(--color-text-muted)]">No audit package jobs yet.</p>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
