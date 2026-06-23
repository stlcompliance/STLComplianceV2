import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { IntegrationBrandMark } from '../nexarr/IntegrationBrandMark'

function pretty(value: string): string {
  return value
    .replace(/[_-]/g, ' ')
    .replace(/\b\w/g, (match) => match.toUpperCase())
}

function statusTone(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'connected' || normalized === 'succeeded') {
    return 'success'
  }
  if (normalized === 'configured' || normalized === 'queued' || normalized === 'running') {
    return 'info'
  }
  if (normalized === 'degraded' || normalized === 'needs_review' || normalized === 'source_unavailable') {
    return 'needs_review'
  }
  if (normalized === 'failed' || normalized === 'dead_letter') {
    return 'danger'
  }
  if (normalized === 'disabled' || normalized === 'not_configured') {
    return 'inactive'
  }
  return 'neutral'
}

function formatDate(value: string | null | undefined): string {
  return value ? new Date(value).toLocaleString() : 'None'
}

export function PlatformIntegrationsPanel() {
  const [tenantId, setTenantId] = useState('')
  const [providerKey, setProviderKey] = useState('')
  const [page, setPage] = useState(1)

  const catalogQuery = useQuery({
    queryKey: ['tenant-integration-catalog'],
    queryFn: () => nexarr.getTenantIntegrationCatalog(),
  })

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 250),
  })

  const integrationsQuery = useQuery({
    queryKey: ['platform-integrations', tenantId, providerKey, page],
    queryFn: () =>
      nexarr.listPlatformTenantIntegrations({
        tenantId: tenantId || undefined,
        providerKey: providerKey || undefined,
        page,
        pageSize: 50,
      }),
  })

  const providers = catalogQuery.data?.providers ?? []
  const tenants = tenantsQuery.data?.items ?? []
  const rows = integrationsQuery.data?.items ?? []
  const configuredTenantCount = useMemo(
    () => new Set(rows.map((row) => row.tenantId)).size,
    [rows],
  )
  const writebackCount = rows.filter((row) => row.writebacksEnabled).length
  const issueCount = rows.filter((row) =>
    ['degraded', 'needs_review'].includes(row.status),
  ).length

  if (catalogQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(catalogQuery.error, 'Failed to load integration catalog.')}
        onRetry={() => void catalogQuery.refetch()}
        retryLabel="Retry catalog"
      />
    )
  }

  if (integrationsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(integrationsQuery.error, 'Failed to load platform integrations.')}
        onRetry={() => void integrationsQuery.refetch()}
        retryLabel="Retry integrations"
      />
    )
  }

  return (
    <section data-testid="platform-integrations-panel" className="space-y-4">
      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Tenant integrations</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Cross-tenant NexArr connection status, credential posture, sync health, and writeback state.
        </p>
      </header>

      <div className="grid gap-3 sm:grid-cols-4">
        <Metric label="Catalog providers" value={providers.length} />
        <Metric label="Visible connections" value={integrationsQuery.data?.totalCount ?? rows.length} />
        <Metric label="Tenant count" value={configuredTenantCount} />
        <Metric label="Attention" value={issueCount + writebackCount} tone={issueCount + writebackCount ? 'amber' : 'slate'} />
      </div>

      <div className="flex flex-col gap-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 sm:flex-row">
        <label className="block text-sm text-[var(--color-text-secondary)] sm:w-80">
          Tenant
          <select
            value={tenantId}
            onChange={(event) => {
              setTenantId(event.target.value)
              setPage(1)
            }}
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          >
            <option value="">All tenants</option>
            {tenants.map((tenant) => (
              <option key={tenant.tenantId} value={tenant.tenantId}>
                {tenant.displayName} ({tenant.slug})
              </option>
            ))}
          </select>
        </label>
        <label className="block text-sm text-[var(--color-text-secondary)] sm:w-80">
          Provider
          <select
            value={providerKey}
            onChange={(event) => {
              setProviderKey(event.target.value)
              setPage(1)
            }}
            className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          >
            <option value="">All providers</option>
            {providers.map((provider) => (
              <option key={provider.providerKey} value={provider.providerKey}>
                {provider.displayName}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-[var(--color-border-subtle)] text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
            <tr>
              <th className="px-4 py-3 font-medium">Tenant</th>
              <th className="px-4 py-3 font-medium">Provider</th>
              <th className="px-4 py-3 font-medium">Status</th>
              <th className="px-4 py-3 font-medium">Credential</th>
              <th className="px-4 py-3 font-medium">Health</th>
              <th className="px-4 py-3 font-medium">Latest sync</th>
              <th className="px-4 py-3 font-medium">Writebacks</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--color-border-subtle)]">
            {integrationsQuery.isLoading ? (
              <tr>
                <td colSpan={7} className="px-4 py-6 text-[var(--color-text-muted)]">
                  Loading integrations...
                </td>
              </tr>
            ) : null}
            {!integrationsQuery.isLoading && rows.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-6 text-[var(--color-text-muted)]">
                  No tenant integrations match the current filters.
                </td>
              </tr>
            ) : null}
            {rows.map((row) => (
              <tr key={row.connectionId}>
                <td className="px-4 py-3">
                  <p className="font-medium text-[var(--color-text-primary)]">{row.tenantDisplayName}</p>
                  <p className="font-mono text-xs text-[var(--color-text-muted)]">{row.tenantSlug}</p>
                </td>
                <td className="px-4 py-3">
                  <div className="flex min-w-0 items-start gap-3">
                    <IntegrationBrandMark brand={row.brand} label={row.providerDisplayName} size="sm" />
                    <div className="min-w-0">
                      <p className="truncate text-[var(--color-text-primary)]">{row.providerDisplayName}</p>
                      <p className="text-xs text-[var(--color-text-muted)]">{row.category}</p>
                    </div>
                  </div>
                </td>
                <td className="px-4 py-3">
                  <span
                    className="stl-tone-badge rounded-full border px-2 py-0.5 text-xs font-medium"
                    data-tone={statusTone(row.status)}
                  >
                    {pretty(row.status)}
                  </span>
                </td>
                <td className="px-4 py-3 text-[var(--color-text-secondary)]">
                  {row.credential ? (
                    <span>
                      {row.credential.redactedLabel}
                      <span className="block text-xs text-[var(--color-text-muted)]">
                        Key {row.credential.encryptionKeyId}
                      </span>
                    </span>
                  ) : (
                    <span className="text-[var(--color-warning-text)]">Missing</span>
                  )}
                </td>
                <td className="px-4 py-3 text-[var(--color-text-secondary)]">
                  {row.health ? (
                    <span>
                      {pretty(row.health.status)}
                      <span className="block text-xs text-[var(--color-text-muted)]">
                        {row.health.latencyMs != null ? `${Math.round(row.health.latencyMs)} ms` : 'No latency'}
                      </span>
                    </span>
                  ) : (
                    'Untested'
                  )}
                </td>
                <td className="px-4 py-3 text-[var(--color-text-secondary)]">
                  {row.latestSyncRun ? (
                    <span>
                      {pretty(row.latestSyncRun.status)}
                      <span className="block text-xs text-[var(--color-text-muted)]">
                        {formatDate(row.latestSyncRun.startedAt)}
                      </span>
                    </span>
                  ) : (
                    'No sync'
                  )}
                </td>
                <td className="px-4 py-3">
                  <span
                    className="stl-tone-badge rounded-full border px-2 py-0.5 text-xs font-medium"
                    data-tone={row.writebacksEnabled ? 'warning' : 'inactive'}
                  >
                    {row.writebacksEnabled ? 'On' : 'Off'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between text-sm text-[var(--color-text-muted)]">
        <span>
          Page {integrationsQuery.data?.page ?? page} of{' '}
          {Math.max(1, Math.ceil((integrationsQuery.data?.totalCount ?? 0) / 50))}
        </span>
        <div className="flex gap-2">
          <button
            type="button"
            disabled={page <= 1}
            onClick={() => setPage((current) => Math.max(1, current - 1))}
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
          >
            Previous
          </button>
          <button
            type="button"
            disabled={!integrationsQuery.data?.hasNextPage}
            onClick={() => setPage((current) => current + 1)}
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-1.5 text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
    </section>
  )
}

function Metric({
  label,
  value,
  tone = 'slate',
}: {
  label: string
  value: string | number
  tone?: 'slate' | 'amber'
}) {
  const valueClass = tone === 'amber' ? 'text-[var(--color-warning-text)]' : 'text-[var(--color-text-primary)]'
  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className={`mt-2 text-2xl font-semibold ${valueClass}`}>{value}</p>
    </div>
  )
}
