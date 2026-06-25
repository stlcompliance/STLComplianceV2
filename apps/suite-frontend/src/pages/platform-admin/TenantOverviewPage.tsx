import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import {
  ApiErrorCallout,
  StaticSearchPicker,
  formatProductDisplayName,
  formatRoleDisplayName,
  formatStatusLabel,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { TenantCatalogAdminPanel } from '../../components/platform-admin/TenantCatalogAdminPanel'
import {
  PlatformAdminKpiCard,
  PlatformAdminPageHeader,
  PlatformAdminScopeNote,
  PlatformAdminSection,
} from '../../components/platform-admin/PlatformAdminPageChrome'
import type { PlatformAuditEventTimelineItem } from '../../api/types'

function tenantStatusLabel(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') {
    return 'Enabled'
  }
  if (normalized === 'suspended') {
    return 'Suspended'
  }
  return status
}

export function TenantOverviewPage() {
  const queryClient = useQueryClient()
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [selectedProductKey, setSelectedProductKey] = useState('')

  const overviewQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const productsQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.listProducts(1, 100),
  })

  const tenants = overviewQuery.data?.items ?? []
  const selectedTenant = useMemo(
    () => tenants.find((tenant) => tenant.tenantId === selectedTenantId) ?? tenants[0] ?? null,
    [selectedTenantId, tenants],
  )

  const tenantDetailQuery = useQuery({
    queryKey: ['platform-admin-tenant-detail', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenant(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const tenantMembersQuery = useQuery({
    queryKey: ['platform-admin-tenant-members', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenantMembers(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const tenantAvailabilityQuery = useQuery({
    queryKey: ['platform-admin-tenant-availability', selectedTenant?.tenantId],
    queryFn: () => nexarr.listTenantAvailabilityRecords(selectedTenant!.tenantId),
    enabled: Boolean(selectedTenant),
  })

  const orderedTenantAvailability = useMemo(
    () =>
      [...(tenantAvailabilityQuery.data?.items ?? [])].sort((left, right) => {
        const leftActive = left.status.toLowerCase() === 'active'
        const rightActive = right.status.toLowerCase() === 'active'

        if (leftActive !== rightActive) {
          return leftActive ? -1 : 1
        }

        return left.productDisplayName.localeCompare(right.productDisplayName)
      }),
    [tenantAvailabilityQuery.data?.items],
  )

  const productOptions = useMemo<PickerOption[]>(
    () =>
      (productsQuery.data?.items ?? []).map((product) => ({
        value: product.productKey,
        label: product.displayName,
        inactive: !product.isActive,
      })),
    [productsQuery.data?.items],
  )

  const grantAvailabilityMutation = useMutation({
    mutationFn: () => nexarr.grantTenantAvailability(selectedTenant!.tenantId, selectedProductKey),
    onSuccess: () => {
      setSelectedProductKey('')
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-availability', selectedTenant?.tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
  })

  const revokeAvailabilityMutation = useMutation({
    mutationFn: (productKey: string) =>
      nexarr.revokeTenantAvailability(selectedTenant!.tenantId, productKey),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-availability', selectedTenant?.tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
  })

  const serviceClientsQuery = useQuery({
    queryKey: ['platform-admin-service-clients'],
    queryFn: () => nexarr.listServiceClients(1, 100),
    enabled: Boolean(selectedTenant),
  })

  const tenantServiceClients = useMemo(
    () =>
      (serviceClientsQuery.data?.items ?? []).filter((client) =>
        client.allowedTenantIds.includes(selectedTenant?.tenantId ?? ''),
      ),
    [selectedTenant?.tenantId, serviceClientsQuery.data?.items],
  )

  const launchHistoryQuery = useQuery({
    queryKey: ['platform-admin-tenant-launch-history', selectedTenant?.tenantId],
    queryFn: () =>
      nexarr.getPlatformAdminLaunchAttempts({
        tenantId: selectedTenant!.tenantId,
        page: 1,
        pageSize: 10,
      }),
    enabled: Boolean(selectedTenant),
  })

  const auditQuery = useQuery({
    queryKey: ['platform-admin-tenant-audit', selectedTenant?.tenantId],
    queryFn: () => nexarr.getTenantAuditEvents(selectedTenant!.tenantId, { page: 1, pageSize: 10 }),
    enabled: Boolean(selectedTenant),
  })

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading tenants…</p>
  }

  if (overviewQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(overviewQuery.error, 'Failed to load tenants.')}
        onRetry={() => void overviewQuery.refetch()}
        retryLabel="Retry tenants"
      />
    )
  }

  return (
    <div className="space-y-6">
      <PlatformAdminPageHeader
        title="Tenant overview"
        summary="Selected tenant record, membership state, availability posture, launch attempts, and audit history for NexArr platform administration."
        badge={selectedTenant ? `${tenantStatusLabel(selectedTenant.status)} tenant` : 'Tenant records'}
        updatedAt={selectedTenant ? new Date(selectedTenant.createdAt).toLocaleString() : undefined}
      />

      {selectedTenant ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          <PlatformAdminKpiCard
            label="Members"
            value={selectedTenant.membershipCount}
            hint="Tenant membership records are managed in NexArr."
            tone="info"
          />
          <PlatformAdminKpiCard
            label="Launch availability"
            value={selectedTenant.activeEntitlementCount}
            hint="Launch availability records are tracked through NexArr."
            tone="good"
          />
          <PlatformAdminKpiCard
            label="Launch attempts"
            value={launchHistoryQuery.data?.items.length ?? '—'}
            hint="Recent launch handoff results for this tenant."
            tone="warn"
          />
          <PlatformAdminKpiCard
            label="Service clients"
            value={tenantServiceClients.length || '—'}
            hint="Tenant-scoped service clients that can call NexArr."
            tone={tenantServiceClients.length > 0 ? 'good' : 'neutral'}
          />
        </div>
      ) : null}

      <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
            <tr>
              <th className="px-3 py-2">Tenant</th>
              <th className="px-3 py-2">Status</th>
              <th className="px-3 py-2">Launch availability</th>
              <th className="px-3 py-2">Members</th>
              <th className="px-3 py-2">Created</th>
            </tr>
          </thead>
          <tbody>
            {tenants.map((tenant) => {
              const isSelected = selectedTenant?.tenantId === tenant.tenantId
              return (
                <tr
                  key={tenant.tenantId}
                className={`cursor-pointer border-b border-[var(--color-border-subtle)] ${isSelected ? 'bg-[var(--color-warning-bg)]' : ''}`}
                  onClick={() => setSelectedTenantId(tenant.tenantId)}
                >
                  <td className="px-3 py-2">
                    <span className="font-medium text-[var(--color-text-primary)]">{tenant.displayName}</span>
                  </td>
                  <td className="px-3 py-2">{formatStatusLabel(tenant.status)}</td>
                  <td className="px-3 py-2">{tenant.activeEntitlementCount}</td>
                  <td className="px-3 py-2">{tenant.membershipCount}</td>
                  <td className="px-3 py-2 text-[var(--color-text-muted)]">
                    {new Date(tenant.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant detail</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant ? `Full record for ${selectedTenant.displayName}` : 'Select a tenant to inspect the full record.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : tenantDetailQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading tenant detail…</p>
        ) : tenantDetailQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(tenantDetailQuery.error, 'Failed to load tenant detail.')}
            onRetry={() => void tenantDetailQuery.refetch()}
            retryLabel="Retry tenant detail"
          />
        ) : tenantDetailQuery.data ? (
          <dl className="mt-3 grid gap-2 md:grid-cols-2">
            <DetailRow label="Tenant ID" value={tenantDetailQuery.data.tenantId} mono />
            <DetailRow label="Slug" value={tenantDetailQuery.data.slug} mono />
            <DetailRow label="Display name" value={tenantDetailQuery.data.displayName} />
            <DetailRow label="Status" value={formatStatusLabel(tenantDetailQuery.data.status)} />
            <DetailRow label="Subscription tier" value={tenantDetailQuery.data.subscriptionTier} />
            <DetailRow label="Trial tenant" value={tenantDetailQuery.data.isTrial ? 'Enabled' : 'Disabled'} />
            <DetailRow label="Internal tenant" value={tenantDetailQuery.data.isInternalTenant ? 'Enabled' : 'Disabled'} />
            <DetailRow
              label="Billing customer ID"
              value={tenantDetailQuery.data.billingCustomerId ?? '—'}
              mono
            />
            <DetailRow
              label="Billing subscription ID"
              value={tenantDetailQuery.data.billingSubscriptionId ?? '—'}
              mono
            />
            <DetailRow
              label="Billing grace days"
              value={tenantDetailQuery.data.billingGraceDays?.toString() ?? '—'}
            />
            <DetailRow label="Created" value={new Date(tenantDetailQuery.data.createdAt).toLocaleString()} />
            <DetailRow label="Modified" value={new Date(tenantDetailQuery.data.modifiedAt).toLocaleString()} />
          </dl>
        ) : null}
      </section>

      {selectedTenant ? (
        <PlatformAdminSection
          title="Decision summary"
          description="Readiness and availability posture for the selected tenant record."
        >
          <div className="grid gap-4 md:grid-cols-2">
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Current state</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{tenantStatusLabel(selectedTenant.status)}</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                {selectedTenant.activeEntitlementCount > 0
                  ? 'Tenant has active launch availability and can open permitted surfaces.'
                  : 'Tenant has no active launch availability records and should be reviewed before launch.'}
              </p>
            </div>
            <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--color-text-muted)]">Source of truth</p>
              <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">NexArr tenant record</p>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Tenant identity, membership, availability, and launch snapshots are owned here and surfaced to other products as references.
              </p>
            </div>
          </div>
        </PlatformAdminSection>
      ) : null}

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant members and launch availability</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant
                ? `Live tenant members and launch availability for ${selectedTenant.displayName}`
                : 'Select a tenant to inspect members and launch availability.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : (
          <div className="mt-4 space-y-4">
            <div className="grid gap-4 lg:grid-cols-2">
              <div>
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Members</h4>
                {tenantMembersQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading tenant members…</p>
                ) : tenantMembersQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(tenantMembersQuery.error, 'Failed to load tenant members.')}
                    onRetry={() => void tenantMembersQuery.refetch()}
                    retryLabel="Retry members"
                  />
                ) : tenantMembersQuery.data?.members.length ? (
                  <ul className="mt-2 space-y-2">
                    {tenantMembersQuery.data.members.map((member) => (
                      <li key={member.membershipId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm">
                        <div className="font-medium text-[var(--color-text-primary)]">{member.displayName}</div>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{member.email}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {formatRoleDisplayName(member.roleKey)} · {member.isActive ? 'Enabled' : 'Disabled'}
                        </p>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No tenant members found.</p>
                )}
              </div>

              <div>
                <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Launch availability</h4>
                {tenantAvailabilityQuery.isLoading ? (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading launch availability…</p>
                ) : tenantAvailabilityQuery.isError ? (
                  <ApiErrorCallout
                    message={getErrorMessage(tenantAvailabilityQuery.error, 'Failed to load launch availability.')}
                    onRetry={() => void tenantAvailabilityQuery.refetch()}
                    retryLabel="Retry launch availability"
                  />
                ) : tenantAvailabilityQuery.data?.items.length ? (
                  <ul className="mt-2 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
                    {orderedTenantAvailability.map((availabilityRecord) => {
                      const isActive = availabilityRecord.status.toLowerCase() === 'active'

                      return (
                        <li
                          key={availabilityRecord.entitlementId}
                          className={[
                            'rounded-xl border p-4 text-sm shadow-sm transition',
                            isActive
                              ? 'border-cyan-500/40 bg-[var(--color-bg-surface-muted)]'
                              : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] opacity-80',
                          ].join(' ')}
                        >
                          <div className="flex items-start justify-between gap-3">
                            <div className="min-w-0">
                              <div className="font-medium text-[var(--color-text-primary)]">{availabilityRecord.productDisplayName}</div>
                            </div>
                            <span
                              className={[
                                'inline-flex rounded-full border px-2.5 py-0.5 text-xs font-medium capitalize',
                                isActive
                                  ? 'border-[var(--color-success-border)] bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                                  : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-secondary)]',
                              ].join(' ')}
                            >
                              {formatStatusLabel(availabilityRecord.status)}
                            </span>
                          </div>

                          <label className="mt-3 flex items-start gap-2 text-xs text-[var(--color-text-muted)]">
                            <input
                              type="checkbox"
                              checked={isActive}
                              readOnly
                              aria-label={`${availabilityRecord.productDisplayName} availability enabled`}
                              className="mt-0.5 h-4 w-4 rounded border-[var(--color-border-strong)] accent-cyan-500"
                            />
                            <span>
                              activated {new Date(availabilityRecord.grantedAt).toLocaleString()}
                              {availabilityRecord.revokedAt
                                ? ` · deactivated ${new Date(availabilityRecord.revokedAt).toLocaleString()}`
                                : ''}
                            </span>
                          </label>

                          {isActive ? (
                            <button
                              type="button"
                              className="mt-3 inline-flex rounded-md border border-[var(--color-destructive-border)] px-2.5 py-1 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                              onClick={() => revokeAvailabilityMutation.mutate(availabilityRecord.productKey)}
                              disabled={revokeAvailabilityMutation.isPending}
                              data-testid={`tenant-availability-revoke-${availabilityRecord.productKey}`}
                            >
                              Deactivate availability
                            </button>
                          ) : null}
                        </li>
                      )
                    })}
                  </ul>
                ) : (
                  <p className="mt-2 text-sm text-[var(--color-text-muted)]">No tenant launch availability records found.</p>
                )}
              </div>
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Activate availability</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Activate a launch availability record for the selected tenant.
              </p>

              <div className="mt-3 grid gap-3 md:grid-cols-[minmax(0,1fr)_auto] md:items-end">
                <StaticSearchPicker
                  label="Product to activate"
                  id="tenant-availability-product"
                  value={selectedProductKey}
                  onChange={setSelectedProductKey}
                  options={productOptions}
                  placeholder="Search products"
                  testId="tenant-availability-product"
                />

                  <button
                    type="button"
                    className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
                    onClick={() => grantAvailabilityMutation.mutate()}
                    disabled={!selectedProductKey || grantAvailabilityMutation.isPending}
                    data-testid="tenant-availability-grant"
                  >
                  {grantAvailabilityMutation.isPending ? 'Activating…' : 'Activate availability'}
                  </button>
              </div>
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Service clients</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Service clients that can call NexArr on behalf of this tenant.
              </p>

              {!selectedTenant ? null : serviceClientsQuery.isLoading ? (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading tenant service clients…</p>
              ) : serviceClientsQuery.isError ? (
                <ApiErrorCallout
                  message={getErrorMessage(serviceClientsQuery.error, 'Failed to load tenant service clients.')}
                  onRetry={() => void serviceClientsQuery.refetch()}
                  retryLabel="Retry service clients"
                />
              ) : tenantServiceClients.length ? (
                <ul className="mt-3 space-y-2">
                  {tenantServiceClients.map((client) => (
                    <li key={client.serviceClientId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
                      <div className="font-medium text-[var(--color-text-primary)]">{client.displayName}</div>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">{client.clientKey}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {client.isActive ? 'Enabled' : 'Disabled'} · last used{' '}
                        {client.lastUsedAt ? new Date(client.lastUsedAt).toLocaleString() : 'never'}
                      </p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {client.failedAuthenticationAttempts} failed auth attempts · allowed products{' '}
                        {client.allowedProductKeys.length}
                      </p>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">No tenant service clients found.</p>
              )}
            </div>

            <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
              <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Launch history</h4>
              <p className="mt-1 text-sm text-[var(--color-text-muted)]">
                Recent tenant launch attempts and failure reasons.
              </p>

              {launchHistoryQuery.isLoading ? (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading launch history…</p>
              ) : launchHistoryQuery.isError ? (
                <ApiErrorCallout
                  message={getErrorMessage(launchHistoryQuery.error, 'Failed to load launch history.')}
                  onRetry={() => void launchHistoryQuery.refetch()}
                  retryLabel="Retry launch history"
                />
              ) : launchHistoryQuery.data?.items.length ? (
                <ul className="mt-3 space-y-2">
                  {launchHistoryQuery.data.items.map((attempt) => (
                    <li key={attempt.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-sm">
                      <div className="font-medium text-[var(--color-text-primary)]">
                        {formatProductDisplayName(attempt.productDisplayName ?? attempt.productKey ?? 'Unknown product')}
                      </div>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {attempt.action} · {attempt.result}
                        {attempt.actorDisplayName ? ` · ${attempt.actorDisplayName}` : ''}
                      </p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {new Date(attempt.occurredAt).toLocaleString()}
                        {attempt.reasonCode ? ` · ${attempt.reasonCode}` : ''}
                      </p>
                      {attempt.remediationHint ? (
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{attempt.remediationHint}</p>
                      ) : null}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-3 text-sm text-[var(--color-text-muted)]">No launch history found for this tenant.</p>
              )}
            </div>
          </div>
        )}
      </section>

      <section className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h3 className="text-base font-semibold text-[var(--color-text-primary)]">Tenant audit history</h3>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenant ? `Events for ${selectedTenant.displayName} (${selectedTenant.slug})` : 'Select a tenant to inspect audit events.'}
            </p>
          </div>
        </div>

        {!selectedTenant ? null : auditQuery.isLoading ? (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading audit history…</p>
        ) : auditQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(auditQuery.error, 'Failed to load tenant audit history.')}
            onRetry={() => void auditQuery.refetch()}
            retryLabel="Retry audit"
          />
        ) : auditQuery.data?.items.length ? (
          <AuditEventList items={auditQuery.data.items} />
        ) : (
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">No tenant audit events found.</p>
        )}
      </section>

      <PlatformAdminScopeNote>
        Detail scope: NexArr manages the tenant record, membership, availability, product launch handoff, and platform audit history. Product-local permissions and execution remain in the target products.
      </PlatformAdminScopeNote>

      <TenantCatalogAdminPanel />
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

function AuditEventList({ items }: { items: PlatformAuditEventTimelineItem[] }) {
  return (
    <ul className="mt-3 space-y-2">
      {items.map((item) => (
        <li key={item.auditEventId} className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-3 text-sm">
          <div className="font-medium text-[var(--color-text-primary)]">{item.action}</div>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            {item.result}
            {item.targetType ? ` · ${item.targetType}` : ''}
            {item.actorUserId ? ` · actor ${item.actorUserId}` : ''}
          </p>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{new Date(item.occurredAt).toLocaleString()}</p>
        </li>
      ))}
    </ul>
  )
}
