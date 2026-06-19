import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { ProductCatalogAdminPanel } from '../../components/platform-admin/ProductCatalogAdminPanel'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'

export function ProductOverviewPage() {
  const [manifestProductKey, setManifestProductKey] = useState('')
  const [manifestTenantId, setManifestTenantId] = useState('')

  const overviewQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  const manifestsQuery = useQuery({
    queryKey: ['platform-admin-product-manifests', manifestProductKey, manifestTenantId],
    queryFn: () =>
      nexarr.getPlatformAdminProductManifests({
        productKey: manifestProductKey || undefined,
        tenantId: manifestTenantId || undefined,
        page: 1,
        pageSize: 20,
      }),
  })

  const serviceClientsQuery = useQuery({
    queryKey: ['platform-admin-service-clients'],
    queryFn: () => nexarr.listServiceClients(1, 100),
  })

  const launchAttemptsQuery = useQuery({
    queryKey: ['platform-admin-product-launch-attempts', manifestProductKey],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchAttempts({
        productKey: manifestProductKey || undefined,
        page: 1,
        pageSize: 10,
      }),
    enabled: Boolean(manifestProductKey.trim()),
  })

  if (overviewQuery.isLoading || manifestsQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading products…</p>
  }

  if (overviewQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(overviewQuery.error, 'Failed to load products.')}
        onRetry={() => void overviewQuery.refetch()}
        retryLabel="Retry products"
      />
    )
  }

  const products = overviewQuery.data!

  const manifests = manifestsQuery.data?.items ?? []
  const activeProducts = products.filter((product) => product.isActive).length
  const launchProfileActive = products.filter((product) => product.launchProfileActive).length

  return (
    <div className="space-y-6">
      <PlatformAdminPageHeader
        title="Product overview"
        summary="NexArr product registry, launch profile metadata, callback allowlists, and service client reachability."
        badge="Registry record"
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <PlatformAdminKpiCard
          label="Registered products"
          value={products.length}
          hint={`${activeProducts} products are active in the current registry snapshot.`}
          tone="good"
        />
        <PlatformAdminKpiCard
          label="Launch profiles"
          value={launchProfileActive}
          hint="Products with an active launch profile are ready to hand off from NexArr."
          tone={launchProfileActive === products.length ? 'good' : 'warn'}
        />
        <PlatformAdminKpiCard
          label="Manifest rows"
          value={manifests.length}
          hint="Filtered product manifests currently in scope for review."
          tone="info"
        />
        <PlatformAdminKpiCard
          label="Service client scope"
          value={serviceClientsQuery.data?.items.length ?? '—'}
          hint="Service clients that can act on behalf of products or tenants."
          tone="neutral"
        />
      </div>

      <PlatformAdminSection
        title="Registry posture"
        description="What NexArr knows about each product launch surface and entitlement state."
      >
        <p className="text-sm text-[var(--color-text-secondary)]">
          This page is a registry and launch-control view, not a product execution surface. Product data shown here is a snapshot owned by NexArr.
        </p>
      </PlatformAdminSection>

      <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
            <tr>
              <th className="px-3 py-2">Product</th>
              <th className="px-3 py-2">Active</th>
              <th className="px-3 py-2">Entitlements</th>
              <th className="px-3 py-2">Launch profile</th>
              <th className="px-3 py-2">Base URL</th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.productKey} className="border-b border-[var(--color-border-subtle)]">
                <td className="px-3 py-2">
                  <span className="font-medium text-stl-navy">{product.displayName}</span>
                  <span className="block text-xs text-[var(--color-text-muted)]">{product.productKey}</span>
                </td>
                <td className="px-3 py-2">{product.isActive ? 'Yes' : 'No'}</td>
                <td className="px-3 py-2">{product.activeEntitlementCount}</td>
                <td className="px-3 py-2">
                  {product.launchProfileActive
                    ? 'Active'
                    : product.hasLaunchProfile
                      ? 'Inactive'
                      : 'Missing'}
                </td>
                <td className="px-3 py-2 font-mono text-xs text-[var(--color-text-muted)]">
                  {product.baseUrl ?? '—'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-stl-navy">Product manifest explorer</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              Inspect launch profile, callback allowlist, and data-plane metadata known to NexArr.
            </p>
          </div>
        </div>

        <div className="mt-3 grid gap-3 md:grid-cols-2">
          <label className="block text-sm text-[var(--color-text-secondary)]">
            Filter by product key
            <input
              className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
              value={manifestProductKey}
              onChange={(event) => setManifestProductKey(event.target.value)}
              placeholder="staffarr"
            />
          </label>
          <label className="block text-sm text-[var(--color-text-secondary)]">
            Filter by tenant ID
            <input
              className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
              value={manifestTenantId}
              onChange={(event) => setManifestTenantId(event.target.value)}
              placeholder="tenant GUID"
            />
          </label>
        </div>

        {manifestsQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(manifestsQuery.error, 'Failed to load product manifests.')}
            onRetry={() => void manifestsQuery.refetch()}
            retryLabel="Retry manifests"
          />
        ) : manifests.length ? (
          <div className="mt-4 space-y-3">
            {manifests.map((manifest) => (
              <div key={`${manifest.productKey}-${manifest.launchProfileModifiedAt ?? 'none'}`} className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h4 className="text-sm font-semibold text-stl-navy">{manifest.displayName}</h4>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      {manifest.productKey} · {manifest.productOwner} · {manifest.productStatus}
                    </p>
                  </div>
                  <p className="text-xs text-[var(--color-text-muted)]">
                    Launch profile {manifest.launchProfileModifiedAt ? `updated ${new Date(manifest.launchProfileModifiedAt).toLocaleString()}` : 'not modified'}
                  </p>
                </div>

                <dl className="mt-3 grid gap-2 md:grid-cols-2">
                  <DetailRow label="Launch URL" value={manifest.launchUrl ?? '—'} mono />
                  <DetailRow label="Launch base URL" value={manifest.launchBaseUrl ?? '—'} mono />
                  <DetailRow label="Launch path" value={manifest.launchPath ?? '—'} mono />
                  <DetailRow label="Callback path" value={manifest.canonicalCallbackPath} mono />
                  <DetailRow label="API base URL" value={manifest.apiBaseUrl} mono />
                  <DetailRow label="Health URL" value={manifest.healthUrl} mono />
                  <DetailRow label="Service audience" value={manifest.serviceAudience} mono />
                  <DetailRow label="Environment" value={manifest.environmentKey} />
                  <DetailRow label="Dependency rules" value={manifest.entitlementDependencyRules} />
                  <DetailRow label="Product dependency metadata" value={manifest.productDependencyMetadata || '—'} />
                </dl>

                <div className="mt-3 grid gap-3 md:grid-cols-2">
                  <DetailList
                    label={`Callback allowlist (${manifest.callbackAllowlist.length})`}
                    items={manifest.callbackAllowlist.map((entry) => `${entry.urlPattern} [${entry.patternType}]${entry.tenantId ? ` · tenant ${entry.tenantId}` : ''}`)}
                  />
                  <DetailList
                    label={`Data plane profiles (${manifest.dataPlaneProfiles.length})`}
                    items={manifest.dataPlaneProfiles.map((profile) => `${profile.tenantId} · ${profile.deploymentMode} · ${profile.trustStatus}${profile.dataEndpointUrl ? ` · ${profile.dataEndpointUrl}` : ''}`)}
                  />
                </div>

                <div className="mt-3">
                  <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                    Service clients
                  </h5>
                  {serviceClientsQuery.isLoading ? (
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">Loading service clients…</p>
                  ) : serviceClientsQuery.isError ? (
                    <p className="mt-2 text-xs text-rose-600">Failed to load service clients.</p>
                  ) : (() => {
                    const productClients = (serviceClientsQuery.data?.items ?? []).filter(
                      (client) =>
                        client.sourceProductKey === manifest.productKey ||
                        client.allowedProductKeys.includes(manifest.productKey),
                    )

                    return productClients.length ? (
                      <ul className="mt-2 space-y-1 text-xs text-[var(--color-text-secondary)]">
                        {productClients.map((client) => (
                          <li
                            key={client.serviceClientId}
                            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2"
                          >
                            <div className="font-medium text-stl-navy">{client.displayName}</div>
                            <p className="mt-1 text-[var(--color-text-muted)]">{client.clientKey}</p>
                            <p className="mt-1 text-[var(--color-text-muted)]">
                              {client.isActive ? 'Active' : 'Inactive'} · last used{' '}
                              {client.lastUsedAt ? new Date(client.lastUsedAt).toLocaleString() : 'never'}
                            </p>
                          </li>
                        ))}
                      </ul>
                    ) : (
                      <p className="mt-2 text-xs text-[var(--color-text-muted)]">No service clients found for this product.</p>
                    )
                  })()}
                </div>

                <div className="mt-3">
                  <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                    Launch activity
                  </h5>
                  {!manifestProductKey.trim() ? (
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">
                      Enter a product key filter above to inspect recent launch attempts.
                    </p>
                  ) : launchAttemptsQuery.isLoading ? (
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">Loading launch activity…</p>
                  ) : launchAttemptsQuery.isError ? (
                    <p className="mt-2 text-xs text-rose-600">Failed to load launch activity.</p>
                  ) : launchAttemptsQuery.data?.items.length ? (
                    <ul className="mt-2 space-y-1 text-xs text-[var(--color-text-secondary)]">
                      {launchAttemptsQuery.data.items.map((attempt) => (
                        <li key={attempt.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2">
                          <div className="font-medium text-stl-navy">
                            {attempt.tenantDisplayName ?? attempt.tenantSlug ?? 'Unknown tenant'}
                          </div>
                          <p className="mt-1 text-[var(--color-text-muted)]">
                            {attempt.action} · {attempt.result}
                            {attempt.actorDisplayName ? ` · ${attempt.actorDisplayName}` : ''}
                          </p>
                          <p className="mt-1 text-[var(--color-text-muted)]">
                            {new Date(attempt.occurredAt).toLocaleString()}
                            {attempt.reasonCode ? ` · ${attempt.reasonCode}` : ''}
                          </p>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p className="mt-2 text-xs text-[var(--color-text-muted)]">No launch activity found for this product filter.</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">No product manifests found for the current filters.</p>
        )}
      </section>

      <PlatformAdminScopeNote>
        Detail scope: NexArr owns product launch profiles, callback allowlists, service client reachability, and registry snapshots. Individual product workflows remain in their own product shells.
      </PlatformAdminScopeNote>

      <ProductCatalogAdminPanel />
    </div>
  )
}

function DetailRow({
  label,
  value,
  mono = false,
}: {
  label: string
  value: string
  mono?: boolean
}) {
  return (
    <div className="grid grid-cols-[8rem_1fr] gap-3">
      <dt className="font-medium text-[var(--color-text-muted)]">{label}</dt>
      <dd className={mono ? 'font-mono text-xs break-all text-[var(--color-text-secondary)]' : 'text-[var(--color-text-secondary)]'}>{value || '—'}</dd>
    </div>
  )
}

function DetailList({ label, items }: { label: string; items: string[] }) {
  return (
    <div>
      <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">{label}</h5>
      {items.length ? (
        <ul className="mt-2 space-y-1 text-xs text-[var(--color-text-secondary)]">
          {items.map((item) => (
            <li key={item} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-3 py-2 font-mono break-all">
              {item}
            </li>
          ))}
        </ul>
      ) : (
        <p className="mt-2 text-xs text-[var(--color-text-muted)]">None</p>
      )}
    </div>
  )
}
