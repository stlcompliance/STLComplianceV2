import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  StaticSearchPicker,
  formatStatusLabel,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import { type FormEvent, useMemo, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import { isActiveTenantStatus } from '../../lib/tenantStatus'

function normalizeTenantStatus(status: string | null | undefined): string {
  switch ((status ?? '').trim().toLowerCase()) {
    case 'active':
      return 'active'
    case 'trial':
      return 'trial'
    case 'suspended':
    case 'inactive':
      return 'suspended'
    case 'archived':
      return 'archived'
    default:
      return 'active'
  }
}

function parseBillingGraceDays(value: string): number | null {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }

  const parsed = Number(trimmed)
  return Number.isFinite(parsed) ? parsed : null
}

export function TenantCatalogAdminPanel() {
  const queryClient = useQueryClient()
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [slug, setSlug] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [status, setStatus] = useState('active')
  const [subscriptionTier, setSubscriptionTier] = useState('standard')
  const [billingCustomerId, setBillingCustomerId] = useState('')
  const [billingSubscriptionId, setBillingSubscriptionId] = useState('')
  const [billingGraceDays, setBillingGraceDays] = useState('')
  const [isTrial, setIsTrial] = useState(false)
  const [isInternalTenant, setIsInternalTenant] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const tenantsQuery = useQuery({
    queryKey: ['platform-tenants-admin'],
    queryFn: () => nexarr.listTenants(1, 100),
  })

  const tenants = tenantsQuery.data?.items ?? []
  const selectedTenant = tenants.find((tenant) => tenant.tenantId === selectedTenantId) ?? null
  const tenantOptions = useMemo<PickerOption[]>(
    () =>
      tenants.map((tenant) => ({
        value: tenant.tenantId,
        label: tenant.displayName,
        inactive: !isActiveTenantStatus(tenant.status),
      })),
    [tenants],
  )

  const createMutation = useMutation({
    mutationFn: () =>
      nexarr.createTenant({
        slug: slug.trim(),
        displayName: displayName.trim(),
        subscriptionTier: subscriptionTier.trim() || 'standard',
        billingCustomerId: billingCustomerId.trim() || null,
        billingSubscriptionId: billingSubscriptionId.trim() || null,
        billingGraceDays: parseBillingGraceDays(billingGraceDays),
        isTrial,
        isInternalTenant,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      setSlug('')
      setDisplayName('')
      setSubscriptionTier('standard')
      setBillingCustomerId('')
      setBillingSubscriptionId('')
      setBillingGraceDays('')
      setIsTrial(false)
      setIsInternalTenant(false)
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const updateMutation = useMutation({
    mutationFn: () =>
      nexarr.updateTenant(selectedTenantId, {
        displayName: editDisplayName.trim(),
        subscriptionTier: subscriptionTier.trim() || 'standard',
        billingCustomerId: billingCustomerId.trim() || null,
        billingSubscriptionId: billingSubscriptionId.trim() || null,
        billingGraceDays: parseBillingGraceDays(billingGraceDays),
        isTrial,
        isInternalTenant,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const statusMutation = useMutation({
    mutationFn: () => nexarr.updateTenantStatus(selectedTenantId, { status }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-tenants-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const handleCreate = (event: FormEvent) => {
    event.preventDefault()
    createMutation.mutate()
  }

  return (
    <section
      data-testid="tenant-catalog-admin-panel"
      className="mt-6 space-y-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Tenant administration</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Create and update tenants via NexArr <code className="text-xs">/api/tenants</code>.
        </p>
      </header>

      {tenantsQuery.isError ? (
        <ApiErrorCallout
          message={getErrorMessage(tenantsQuery.error, 'Failed to load tenants.')}
          onRetry={() => void tenantsQuery.refetch()}
          retryLabel="Retry tenants"
        />
      ) : null}

      {errorMessage ? (
        <ApiErrorCallout message={errorMessage} />
      ) : null}

      <form className="grid gap-3 md:grid-cols-3" onSubmit={handleCreate}>
        <label htmlFor="tenant-catalog-create-slug" className="block text-sm text-[var(--color-text-secondary)]">
          New tenant slug
          <input
            id="tenant-catalog-create-slug"
            value={slug}
            onChange={(event) => setSlug(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            required
          />
        </label>
        <label htmlFor="tenant-catalog-create-display-name" className="block text-sm text-[var(--color-text-secondary)] md:col-span-2">
          New tenant display name
          <input
            id="tenant-catalog-create-display-name"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            required
          />
        </label>
        <label htmlFor="tenant-catalog-create-subscription-tier" className="block text-sm text-[var(--color-text-secondary)]">
          Subscription tier
          <input
            id="tenant-catalog-create-subscription-tier"
            value={subscriptionTier}
            onChange={(event) => setSubscriptionTier(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            placeholder="standard"
          />
        </label>
        <label htmlFor="tenant-catalog-create-billing-customer" className="block text-sm text-[var(--color-text-secondary)]">
          Billing customer ID
          <input
            id="tenant-catalog-create-billing-customer"
            value={billingCustomerId}
            onChange={(event) => setBillingCustomerId(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            placeholder="cus_123"
          />
        </label>
        <label htmlFor="tenant-catalog-create-billing-subscription" className="block text-sm text-[var(--color-text-secondary)]">
          Billing subscription ID
          <input
            id="tenant-catalog-create-billing-subscription"
            value={billingSubscriptionId}
            onChange={(event) => setBillingSubscriptionId(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            placeholder="sub_123"
          />
        </label>
        <label htmlFor="tenant-catalog-create-billing-grace-days" className="block text-sm text-[var(--color-text-secondary)]">
          Billing grace days
          <input
            id="tenant-catalog-create-billing-grace-days"
            type="number"
            min={0}
            value={billingGraceDays}
            onChange={(event) => setBillingGraceDays(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
            placeholder="7"
          />
        </label>
        <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-catalog-create-is-trial"
            type="checkbox"
            checked={isTrial}
            onChange={(event) => setIsTrial(event.target.checked)}
          />
          Trial tenant
        </label>
        <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-catalog-create-is-internal"
            type="checkbox"
            checked={isInternalTenant}
            onChange={(event) => setIsInternalTenant(event.target.checked)}
          />
          Internal/free tenant
        </label>
        <div className="md:col-span-3">
          <button
            type="submit"
            disabled={createMutation.isPending}
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
          >
            {createMutation.isPending ? 'Creating…' : 'Create tenant'}
          </button>
        </div>
      </form>

      <div className="grid gap-3 md:grid-cols-2">
        <StaticSearchPicker
          label="Tenant to edit"
          id="tenant-catalog-selected-tenant"
          value={selectedTenantId}
          onChange={(tenantId) => {
            setSelectedTenantId(tenantId)
            const tenant = tenants.find((item) => item.tenantId === tenantId)
            setEditDisplayName(tenant?.displayName ?? '')
            setStatus(normalizeTenantStatus(tenant?.status))
            setSubscriptionTier(tenant?.subscriptionTier ?? 'standard')
            setBillingCustomerId(tenant?.billingCustomerId ?? '')
            setBillingSubscriptionId(tenant?.billingSubscriptionId ?? '')
            setBillingGraceDays(tenant?.billingGraceDays?.toString() ?? '')
            setIsTrial(tenant?.isTrial ?? false)
            setIsInternalTenant(tenant?.isInternalTenant ?? false)
          }}
          options={tenantOptions}
          placeholder="Search tenants"
          testId="tenant-catalog-selected-tenant"
        />
        {selectedTenant ? (
          <>
            <label htmlFor="tenant-catalog-edit-display-name" className="block text-sm text-[var(--color-text-secondary)]">
              Updated tenant display name
              <input
                id="tenant-catalog-edit-display-name"
                value={editDisplayName}
                onChange={(event) => setEditDisplayName(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
              />
            </label>
            <label htmlFor="tenant-catalog-edit-status" className="block text-sm text-[var(--color-text-secondary)]">
              Tenant lifecycle status
              <select
                id="tenant-catalog-edit-status"
                value={status}
                onChange={(event) => setStatus(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
              >
                <option value="active">{formatStatusLabel('active')}</option>
                <option value="trial">{formatStatusLabel('trial')}</option>
                <option value="inactive">{formatStatusLabel('inactive')}</option>
                <option value="suspended">{formatStatusLabel('suspended')}</option>
                <option value="archived">{formatStatusLabel('archived')}</option>
              </select>
            </label>
            <label htmlFor="tenant-catalog-edit-subscription-tier" className="block text-sm text-[var(--color-text-secondary)]">
              Subscription tier
              <input
                id="tenant-catalog-edit-subscription-tier"
                value={subscriptionTier}
                onChange={(event) => setSubscriptionTier(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                placeholder="standard"
              />
            </label>
            <label htmlFor="tenant-catalog-edit-billing-customer" className="block text-sm text-[var(--color-text-secondary)]">
              Billing customer ID
              <input
                id="tenant-catalog-edit-billing-customer"
                value={billingCustomerId}
                onChange={(event) => setBillingCustomerId(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                placeholder="cus_123"
              />
            </label>
            <label htmlFor="tenant-catalog-edit-billing-subscription" className="block text-sm text-[var(--color-text-secondary)]">
              Billing subscription ID
              <input
                id="tenant-catalog-edit-billing-subscription"
                value={billingSubscriptionId}
                onChange={(event) => setBillingSubscriptionId(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                placeholder="sub_123"
              />
            </label>
            <label htmlFor="tenant-catalog-edit-billing-grace-days" className="block text-sm text-[var(--color-text-secondary)]">
              Billing grace days
              <input
                id="tenant-catalog-edit-billing-grace-days"
                type="number"
                min={0}
                value={billingGraceDays}
                onChange={(event) => setBillingGraceDays(event.target.value)}
                className="mt-1 w-full rounded-md border border-[var(--color-border-default)] px-3 py-2 text-sm"
                placeholder="7"
              />
            </label>
            <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
              <input
                id="tenant-catalog-edit-is-trial"
                type="checkbox"
                checked={isTrial}
                onChange={(event) => setIsTrial(event.target.checked)}
              />
              Trial tenant
            </label>
            <label className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
              <input
                id="tenant-catalog-edit-is-internal"
                type="checkbox"
                checked={isInternalTenant}
                onChange={(event) => setIsInternalTenant(event.target.checked)}
              />
              Internal/free tenant
            </label>
            <div className="flex flex-wrap gap-2 md:col-span-2">
              <button
                type="button"
                disabled={updateMutation.isPending}
                onClick={() => updateMutation.mutate()}
                className="rounded-md border border-[var(--color-border-default)] px-4 py-2 text-sm hover:bg-[var(--color-bg-surface-muted)] disabled:opacity-50"
              >
                Save display name
              </button>
              <button
                type="button"
                disabled={statusMutation.isPending}
                onClick={() => statusMutation.mutate()}
                className="rounded-md border border-[var(--color-border-default)] px-4 py-2 text-sm hover:bg-[var(--color-bg-surface-muted)] disabled:opacity-50"
              >
                Update status
              </button>
            </div>
          </>
        ) : null}
      </div>

      {selectedTenant ? (
        <div className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Billing readiness</h3>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            Billing details are shown here for admin review and future subscription-driven entitlement flows.
          </p>
          <dl className="mt-3 grid gap-3 md:grid-cols-2">
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Subscription tier</dt>
              <dd className="text-[var(--color-text-secondary)]">{selectedTenant.subscriptionTier || '—'}</dd>
            </div>
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Trial tenant</dt>
              <dd className="text-[var(--color-text-secondary)]">{selectedTenant.isTrial ? 'Yes' : 'No'}</dd>
            </div>
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Internal tenant</dt>
              <dd className="text-[var(--color-text-secondary)]">{selectedTenant.isInternalTenant ? 'Yes' : 'No'}</dd>
            </div>
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Billing customer ID</dt>
              <dd className="font-mono text-xs break-all text-[var(--color-text-secondary)]">{selectedTenant.billingCustomerId ?? '—'}</dd>
            </div>
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Billing subscription ID</dt>
              <dd className="font-mono text-xs break-all text-[var(--color-text-secondary)]">{selectedTenant.billingSubscriptionId ?? '—'}</dd>
            </div>
            <div className="grid grid-cols-[10rem_1fr] gap-3">
              <dt className="font-medium text-[var(--color-text-muted)]">Grace days</dt>
              <dd className="text-[var(--color-text-secondary)]">{selectedTenant.billingGraceDays ?? '—'}</dd>
            </div>
          </dl>
        </div>
      ) : null}
    </section>
  )
}
