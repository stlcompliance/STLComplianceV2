import { useQuery } from '@tanstack/react-query'
import { AlertTriangle, ArrowRight, Activity, ServerCog } from 'lucide-react'
import { Link } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'

export function PlatformAdminDashboardPage() {
  const dashboardQuery = useQuery({
    queryKey: ['platform-admin-dashboard'],
    queryFn: () => nexarr.getPlatformAdminDashboard(),
  })

  if (dashboardQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading dashboard…</p>
  }

  if (dashboardQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(dashboardQuery.error, 'Failed to load dashboard.')}
        onRetry={() => void dashboardQuery.refetch()}
        retryLabel="Retry dashboard"
      />
    )
  }

  const d = dashboardQuery.data!
  const blockerCount = d.expiredUnredeemedHandoffCount
  const watchCount = d.pendingHandoffCount
  const activeLaunchContextCount = d.activeLaunchableDestinationCount
  const totalLaunchContextCount = d.totalLaunchableDestinationCount
  const recentHealth =
    d.activeServiceTokenCount > 0 ? 'Platform services are live' : 'No active service tokens'
  return (
    <div className="space-y-6">
      <PlatformAdminPageHeader
        title="Platform dashboard"
        summary="NexArr platform administration view for tenant identity, product destination status, launch setup, service tokens, and platform audit posture."
        updatedAt={new Date(d.generatedAt).toLocaleString()}
        badge={recentHealth}
        actions={
          <>
            <Link
              to="/app/platform-admin/status"
              className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-surface-muted)]"
            >
              <Activity className="h-4 w-4" aria-hidden />
              System status
            </Link>
            <Link
              to="/app/platform-admin/orchestration"
              className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] hover:bg-[var(--color-bg-surface-muted)]"
            >
              <ServerCog className="h-4 w-4" aria-hidden />
              Worker health
            </Link>
          </>
        }
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <PlatformAdminKpiCard label="Tenants (active)" value={d.activeTenantCount} hint={`${d.tenantCount} total tenant records are in the registry.`} tone="good" />
        <PlatformAdminKpiCard label="Products (active)" value={d.activeProductCount} hint={`${d.productCount} total registered products are tracked in NexArr.`} tone="good" />
        <PlatformAdminKpiCard label="Active launch contexts" value={activeLaunchContextCount} hint={`${totalLaunchContextCount} tenant launch contexts are tracked across the suite.`} tone="info" />
        <PlatformAdminKpiCard label="Service clients" value={d.serviceClientCount} hint={`${d.activeServiceTokenCount} active service tokens are available for product launches and service calls.`} tone={d.activeServiceTokenCount > 0 ? 'good' : 'warn'} />
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1.6fr)_minmax(0,1fr)]">
        <PlatformAdminSection
          title="Operational picture"
          description="The current platform shape, represented as the data NexArr owns directly."
        >
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Launch setup</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{d.launchProfileCount} launch settings</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">Launch routing and callback handling are configured in NexArr.</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Launch activity</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{d.pendingHandoffCount} pending</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">{d.expiredUnredeemedHandoffCount} expired launch code{d.expiredUnredeemedHandoffCount === 1 ? '' : 's'} require attention.</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Audit activity</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{d.auditEventsLast24Hours} events in 24h</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">Recent platform actions and administrative changes.</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Support surface</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">Platform admin</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">System-wide platform data, not product-owned workflow truth.</p>
            </div>
          </div>
        </PlatformAdminSection>

        <PlatformAdminSection
          title="Attention"
          description="Items that are blocked, stale, or likely to need a human decision."
        >
          {blockerCount > 0 ? (
            <div className="rounded-xl border border-[var(--color-destructive-border)] bg-[var(--color-destructive-bg)] p-4">
              <div className="flex items-start gap-3">
                <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-[var(--color-destructive-text)]" aria-hidden />
                <div>
                  <p className="font-semibold text-[var(--color-destructive-text)]">Expired unused launch codes</p>
                  <p className="mt-1 text-sm text-[var(--color-destructive-text)]">
                    {blockerCount} expired code{blockerCount === 1 ? '' : 's'} remain in the platform launch store. Review and clean up stale launches.
                  </p>
                </div>
              </div>
            </div>
          ) : (
            <div className="rounded-xl border border-[var(--color-success-border)] bg-[var(--color-success-bg)] p-4 text-sm text-[var(--color-success-text)]">
              No blocked product launches are currently waiting on NexArr intervention.
            </div>
          )}

          <div className="mt-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
            <p className="text-sm font-semibold text-[var(--color-text-primary)]">Next actions</p>
            <ul className="mt-2 space-y-2 text-sm text-[var(--color-text-muted)]">
              <li className="flex items-start gap-2">
                <ArrowRight className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
                Check system status and launch diagnostics when a product is not launching cleanly.
              </li>
              <li className="flex items-start gap-2">
                <ArrowRight className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
                Use worker health when service tokens, outbox runs, or cleanup jobs need review.
              </li>
            </ul>
          </div>
        </PlatformAdminSection>
      </div>

      <PlatformAdminSection
        title="Suite totals"
        description="Cross-checks for the platform registry and evidence history."
      >
        <ul className="list-disc space-y-2 pl-5 text-sm text-[var(--color-text-secondary)]">
          <li>
            {d.tenantCount} tenants, {d.productCount} products, and {totalLaunchContextCount} tenant launch contexts are tracked.
          </li>
          <li>
            {watchCount} pending product launches are open right now.
          </li>
        </ul>
      </PlatformAdminSection>

      <PlatformAdminScopeNote>
        Dashboard scope: NexArr covers platform login, tenant identity, product destination status, launch setup, service tokens, and platform admin audit history. Product execution and records remain in the owning products.
      </PlatformAdminScopeNote>
    </div>
  )
}
