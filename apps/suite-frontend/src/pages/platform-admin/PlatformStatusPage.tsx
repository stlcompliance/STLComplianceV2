import { useQuery } from '@tanstack/react-query'
import { HeartPulse, Activity, ServerCog } from 'lucide-react'
import { Link } from 'react-router-dom'
import {
  ApiErrorCallout,
  formatProductDisplayName,
  formatStatusLabel,
  getErrorMessage,
} from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import type { ProductDetailResponse, ProductHealthProbeResult } from '../../api/types'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import { healthBadgeClass } from '../../components/platform-admin/worker-health/utils'

export function PlatformStatusPage() {
  const healthQuery = useQuery({
    queryKey: ['platform-health'],
    queryFn: () => nexarr.getPlatformHealth(),
    refetchInterval: 30_000,
  })

  const registryQuery = useQuery({
    queryKey: ['platform-products-registry-health'],
    queryFn: () => nexarr.listProducts(1, 100),
    refetchInterval: 60_000,
  })

  if (healthQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading system status…</p>
  }

  if (healthQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(healthQuery.error, 'Failed to load system status.')}
        onRetry={() => void healthQuery.refetch()}
        retryLabel="Retry system status"
      />
    )
  }

  const status = healthQuery.data!
  const products = [...status.products].sort((a, b) => a.productKey.localeCompare(b.productKey))
  const productNameByKey = new Map(registryQuery.data?.items.map((product) => [product.productKey, product.displayName]) ?? [])
  const deploymentEvidence = buildDeploymentEvidence(products)
  const registryProducts = registryQuery.data?.items ?? []
  const registrySummary = buildRegistrySummary(registryProducts)
  const unhealthyProducts = products.filter((product) => product.status !== 'Healthy').length
  const driftTone = deploymentEvidence.hasDrift ? 'warn' : 'good'

  return (
    <div className="space-y-6">
      <PlatformAdminPageHeader
        title="System status"
        summary="Live platform health from NexArr control-plane probes. Use this view to spot deployment skew, missing configuration, and service readiness issues."
        updatedAt={new Date(status.timestampUtc).toLocaleString()}
        badge={formatStatusLabel(status.status)}
        actions={
          <>
            <Link
              to="/app/platform-admin/launch"
              className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm font-medium text-stl-navy hover:bg-[var(--color-bg-surface-muted)]"
            >
              <Activity className="h-4 w-4" aria-hidden />
              Launch diagnostics
            </Link>
            <Link
              to="/app/platform-admin/orchestration"
              className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm font-medium text-stl-navy hover:bg-[var(--color-bg-surface-muted)]"
            >
              <ServerCog className="h-4 w-4" aria-hidden />
              Worker health
            </Link>
          </>
        }
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <PlatformAdminKpiCard
          label="Overall health"
          value={formatStatusLabel(status.status)}
          hint={`Generated ${new Date(status.timestampUtc).toLocaleString()}.`}
          tone={status.status === 'Healthy' ? 'good' : status.status === 'Degraded' ? 'warn' : 'bad'}
        />
        <PlatformAdminKpiCard
          label="Products probed"
          value={products.length}
          hint={`${unhealthyProducts} product probe${unhealthyProducts === 1 ? '' : 's'} are not healthy.`}
          tone={unhealthyProducts === 0 ? 'good' : 'warn'}
        />
        <PlatformAdminKpiCard
          label="Deployment versions"
          value={deploymentEvidence.distinctVersionCount}
          hint={`${deploymentEvidence.productsWithEvidence} product${deploymentEvidence.productsWithEvidence === 1 ? '' : 's'} reported version evidence.`}
          tone={driftTone}
        />
        <PlatformAdminKpiCard
          label="Registry gaps"
          value={registrySummary.missingConfiguration.length}
          hint={`${registrySummary.totalProducts} products are listed in the NexArr registry.`}
          tone={registrySummary.missingConfiguration.length === 0 ? 'good' : 'warn'}
        />
      </div>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5" data-testid="platform-status-registry-summary">
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-[var(--color-bg-control-hover)]">
            <HeartPulse className="h-5 w-5 text-stl-teal" aria-hidden />
          </div>
          <div>
            <h5 className="font-semibold text-stl-navy">Overall platform health</h5>
            <p className="text-xs text-[var(--color-text-muted)]">
              Generated {new Date(status.timestampUtc).toLocaleString()}
            </p>
          </div>
          <span
            className={[
              'ml-auto rounded-full px-2 py-0.5 text-xs font-medium',
              healthBadgeClass(status.status),
            ].join(' ')}
            data-testid="platform-status-overall"
          >
            {formatStatusLabel(status.status)}
          </span>
        </div>

        <div className="mt-4 flex flex-wrap gap-3">
          <Link
            to="/app/platform-admin/launch"
            className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-sm font-medium hover:bg-[var(--color-bg-surface-muted)]"
          >
            <Activity className="h-4 w-4" aria-hidden />
            Launch diagnostics
          </Link>
          <Link
            to="/app/platform-admin/orchestration"
            className="inline-flex items-center gap-2 rounded-md border border-[var(--color-border-default)] px-3 py-1.5 text-sm font-medium hover:bg-[var(--color-bg-surface-muted)]"
          >
            <ServerCog className="h-4 w-4" aria-hidden />
            Worker health
          </Link>
        </div>
      </section>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <h5 className="font-semibold text-stl-navy">Product probes</h5>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Each probe checks the product&apos;s `/health/ready` endpoint from NexArr.
        </p>

        <div className="mt-4 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {products.map((product) => (
            <article
              key={product.productKey}
              className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4"
              data-testid={`platform-status-product-${product.productKey}`}
            >
              <div className="flex items-start gap-3">
                <div>
                  <h6 className="font-semibold text-stl-navy">{formatProductDisplayName(product.productKey)}</h6>
                  <p className="text-xs text-[var(--color-text-muted)]">
                    {product.readyUrl ? product.readyUrl : 'Not configured'}
                  </p>
                </div>
                <span
                  className={[
                    'ml-auto rounded-full px-2 py-0.5 text-xs font-medium',
                    healthBadgeClass(product.status),
                  ].join(' ')}
                >
                  {formatStatusLabel(product.status)}
                </span>
              </div>

              <dl className="mt-3 space-y-1 text-sm text-[var(--color-text-secondary)]">
                <div className="flex justify-between gap-3">
                  <dt>Latency</dt>
                  <dd>{product.latencyMs != null ? `${Math.round(product.latencyMs)} ms` : '—'}</dd>
                </div>
                <div className="flex justify-between gap-3">
                  <dt>Error</dt>
                  <dd className="text-right">{product.errorCode ?? '—'}</dd>
                </div>
                {product.errorMessage ? (
                  <p className="pt-1 text-xs text-[var(--color-text-muted)]">{product.errorMessage}</p>
                ) : null}
              </dl>

              {product.detail ? (
                <div
                  className="mt-4 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-xs text-[var(--color-text-muted)]"
                  data-testid={`platform-status-product-evidence-${product.productKey}`}
                >
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="font-medium text-[var(--color-text-secondary)]">Deployment evidence</span>
                    <span className="ml-auto font-mono text-[var(--color-text-muted)]">v{product.detail.version}</span>
                  </div>
                  <p className="mt-1 text-[var(--color-text-muted)]">
                    Observed {Object.keys(product.detail.checks ?? {}).length} health check
                    {Object.keys(product.detail.checks ?? {}).length === 1 ? '' : 's'} at{' '}
                    {new Date(product.detail.timestampUtc).toLocaleString()}.
                  </p>
                  {Object.entries(product.detail.checks ?? {}).length ? (
                    <ul className="mt-2 space-y-1">
                      {Object.entries(product.detail.checks ?? {}).map(([name, value]) => (
                        <li key={name} className="flex justify-between gap-3">
                          <span className="font-medium text-[var(--color-text-secondary)]">{name}</span>
                          <span className="text-right text-[var(--color-text-muted)]">{formatCheckValue(value)}</span>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </div>
              ) : null}
            </article>
          ))}
        </div>
      </section>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="flex flex-wrap items-center gap-3">
          <div>
            <h5 className="font-semibold text-stl-navy">Deployment drift evidence</h5>
            <p className="text-sm text-[var(--color-text-muted)]">
              NexArr compares observed probe versions to surface likely stale deployments and version skew.
            </p>
          </div>
          <span
            className={[
              'ml-auto rounded-full px-2 py-0.5 text-xs font-medium',
              deploymentEvidence.hasDrift
                ? 'bg-amber-100 text-amber-800'
                : 'bg-emerald-100 text-emerald-800',
            ].join(' ')}
            data-testid="platform-status-drift-state"
          >
            {deploymentEvidence.hasDrift ? 'Drift detected' : 'No drift detected'}
          </span>
        </div>

        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <RegistryMetric label="Observed versions" value={deploymentEvidence.distinctVersionCount.toString()} />
          <RegistryMetric label="Products with evidence" value={deploymentEvidence.productsWithEvidence.toString()} />
          <RegistryMetric label="Products missing version" value={deploymentEvidence.productsWithoutVersion.toString()} />
        </div>

        {deploymentEvidence.hasDrift ? (
          <div className="mt-4 rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">
            <p className="font-medium">Potential deployment skew</p>
            <ul className="mt-2 list-disc space-y-1 pl-5">
              {deploymentEvidence.versionGroups.map((group) => (
                <li key={group.version}>
                  {group.version}: {group.products.map((productKey) => productNameByKey.get(productKey) ?? formatProductDisplayName(productKey)).join(', ')}
                </li>
              ))}
            </ul>
          </div>
        ) : (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">All probed products reported the same deployed version.</p>
        )}
      </section>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="flex flex-wrap items-center gap-3">
          <div>
            <h5 className="font-semibold text-stl-navy">Product registry health summary</h5>
            <p className="text-sm text-[var(--color-text-muted)]">
              Derived from NexArr&apos;s canonical product registry, launch metadata, and health URLs.
            </p>
          </div>
          <span
            className="ml-auto rounded-full bg-[var(--color-bg-control-hover)] px-2 py-0.5 text-xs font-medium text-[var(--color-text-secondary)]"
            data-testid="platform-status-registry-count"
          >
            {registrySummary.totalProducts} products
          </span>
        </div>

        {registryQuery.isLoading ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading product registry…</p>
        ) : registryQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(registryQuery.error, 'Failed to load product registry.')}
            onRetry={() => void registryQuery.refetch()}
            retryLabel="Retry registry"
          />
        ) : (
          <div className="mt-4 space-y-4">
            <div className="grid gap-3 md:grid-cols-4">
              <RegistryMetric label="Active products" value={registrySummary.activeProducts.toString()} />
              <RegistryMetric label="Inactive products" value={registrySummary.inactiveProducts.toString()} />
              <RegistryMetric label="Missing API URL" value={registrySummary.missingApiUrl.toString()} />
              <RegistryMetric label="Missing health URL" value={registrySummary.missingHealthUrl.toString()} />
            </div>

            {registrySummary.missingConfiguration.length ? (
              <div className="rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900">
                <p className="font-medium">Registry configuration gaps</p>
                <ul className="mt-2 list-disc space-y-1 pl-5">
                  {registrySummary.missingConfiguration.map((item) => (
                    <li key={item.productKey}>
                      {item.displayName} is missing {item.missingFields.join(', ')}.
                    </li>
                  ))}
                </ul>
              </div>
            ) : (
              <p className="text-sm text-[var(--color-text-muted)]">All registered products have launch and health URLs configured.</p>
            )}

            <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)]">
              <table className="min-w-full text-left text-sm">
                <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
                  <tr>
                    <th className="px-3 py-2">Product</th>
                    <th className="px-3 py-2">Status</th>
                    <th className="px-3 py-2">API URL</th>
                    <th className="px-3 py-2">Health URL</th>
                  </tr>
                </thead>
                <tbody>
                  {registryProducts.map((product) => (
                    <tr key={product.productKey} className="border-b border-[var(--color-border-subtle)] last:border-b-0">
                      <td className="px-3 py-2">
                        <div className="font-medium text-stl-navy">{product.displayName}</div>
                        <div className="text-xs text-[var(--color-text-muted)]">{product.environmentKey}</div>
                      </td>
                      <td className="px-3 py-2">
                        <div className="text-sm text-[var(--color-text-secondary)]">{formatStatusLabel(product.productStatus)}</div>
                        <div className="text-xs text-[var(--color-text-muted)]">{product.isActive ? 'Active' : 'Inactive'}</div>
                      </td>
                      <td className="px-3 py-2 text-xs text-[var(--color-text-muted)]">
                        {product.apiBaseUrl || '—'}
                      </td>
                      <td className="px-3 py-2 text-xs text-[var(--color-text-muted)]">
                        {product.healthUrl || '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </section>

      <PlatformAdminScopeNote>
        Dashboard scope: NexArr covers platform health probes, product registry metadata, launch configuration, and deployment drift evidence. Product repair happens in the relevant product or deployment target.
      </PlatformAdminScopeNote>
    </div>
  )
}

function RegistryMetric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3">
      <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
      <div className="mt-1 text-2xl font-semibold text-stl-navy">{value}</div>
    </div>
  )
}

function buildRegistrySummary(products: ProductDetailResponse[]) {
  const missingConfiguration = products
    .map((product) => {
      const missingFields = [
        !product.apiBaseUrl?.trim() ? 'API URL' : null,
        !product.healthUrl?.trim() ? 'health URL' : null,
      ].filter(Boolean) as string[]
      return missingFields.length ? { productKey: product.productKey, displayName: product.displayName, missingFields } : null
    })
    .filter((item): item is NonNullable<typeof item> => item !== null)

  return {
    totalProducts: products.length,
    activeProducts: products.filter((product) => product.isActive).length,
    inactiveProducts: products.filter((product) => !product.isActive).length,
    missingApiUrl: products.filter((product) => !product.apiBaseUrl?.trim()).length,
    missingHealthUrl: products.filter((product) => !product.healthUrl?.trim()).length,
    missingConfiguration,
  }
}

function buildDeploymentEvidence(products: ProductHealthProbeResult[]) {
  const withVersion = products
    .map((product) => ({
      productKey: product.productKey,
      version: product.detail?.version?.trim() ?? '',
    }))
    .filter((product) => product.version.length > 0)

  const versionGroups = Array.from(
    withVersion.reduce((groups, product) => {
      const group = groups.get(product.version) ?? { version: product.version, products: [] as string[] }
      group.products.push(product.productKey)
      groups.set(product.version, group)
      return groups
    }, new Map<string, { version: string; products: string[] }>()),
  ).sort((a, b) => a[0].localeCompare(b[0])).map(([, group]) => group)

  return {
    distinctVersionCount: versionGroups.length,
    productsWithEvidence: withVersion.length,
    productsWithoutVersion: products.length - withVersion.length,
    versionGroups,
    hasDrift: versionGroups.length > 1 || products.length !== withVersion.length,
  }
}

function formatCheckValue(value: unknown) {
  if (value == null) {
    return '—'
  }
  if (typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean') {
    return String(value)
  }

  try {
    return JSON.stringify(value)
  } catch {
    return String(value)
  }
}
