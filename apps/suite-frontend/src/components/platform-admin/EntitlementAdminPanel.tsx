import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { isActiveTenantStatus } from '../../lib/tenantStatus'

export function EntitlementAdminPanel() {
  const queryClient = useQueryClient()
  const [tenantId, setTenantId] = useState('')
  const [productKey, setProductKey] = useState('')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const productsQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  const entitlementsQuery = useQuery({
    queryKey: ['platform-entitlements', tenantId],
    queryFn: () => nexarr.listEntitlements(tenantId),
    enabled: Boolean(tenantId.trim()),
  })

  const grantMutation = useMutation({
    mutationFn: () =>
      nexarr.grantEntitlement({
        tenantId: tenantId.trim(),
        productKey,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      setProductKey('')
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlements', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const revokeMutation = useMutation({
    mutationFn: (entitlementId: string) => nexarr.revokeEntitlement(entitlementId),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlements', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-tenant-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const tenants = tenantsQuery.data?.items ?? []
  const products = (productsQuery.data ?? []).filter((product) => product.isActive)
  const entitlements = entitlementsQuery.data?.items ?? []
  const tenantOptions = useMemo<PickerOption[]>(
    () =>
      tenants.map((tenant) => ({
        value: tenant.tenantId,
        label: `${tenant.displayName} (${tenant.slug})`,
        inactive: !isActiveTenantStatus(tenant.status),
      })),
    [tenants],
  )
  const productOptions = useMemo<PickerOption[]>(
    () =>
      products.map((product) => ({
        value: product.productKey,
        label: product.displayName,
        inactive: !product.isActive,
      })),
    [products],
  )

  return (
    <section
      data-testid="entitlement-admin-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Entitlement administration</h2>
        <p className="mt-1 text-sm text-slate-400">
          Grant or revoke tenant product entitlements via NexArr{' '}
          <code className="text-xs">/api/entitlements</code>. Platform admins can entitle any tenant;
          tenant admins are limited to their active tenant on the API.
        </p>
      </header>

      <div className="grid gap-3 sm:grid-cols-2">
        <StaticSearchPicker
          label="Entitlement tenant"
          id="entitlement-admin-tenant"
          value={tenantId}
          onChange={setTenantId}
          options={tenantOptions}
          placeholder="Search tenants"
          testId="entitlement-admin-tenant"
        />

        <StaticSearchPicker
          label="Product to grant"
          id="entitlement-admin-product"
          value={productKey}
          onChange={setProductKey}
          options={productOptions}
          placeholder="Search products"
          testId="entitlement-admin-product"
        />
      </div>

      <button
        type="button"
        onClick={() => grantMutation.mutate()}
        disabled={!tenantId || !productKey || grantMutation.isPending}
        data-testid="entitlement-admin-grant"
        className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
      >
        {grantMutation.isPending ? 'Granting…' : 'Grant entitlement'}
      </button>

      {errorMessage ? (
        <p className="text-sm text-rose-400" data-testid="entitlement-admin-error">
          {errorMessage}
        </p>
      ) : null}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Current entitlements</h3>
        {!tenantId ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Select a tenant to view entitlements.</p>
        ) : entitlementsQuery.isLoading ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading entitlements…</p>
        ) : entitlements.length === 0 ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="entitlement-admin-empty">
            No entitlements recorded for this tenant.
          </p>
        ) : (
          <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="entitlement-admin-list">
            {entitlements.map((item) => (
              <li key={item.entitlementId} className="flex flex-wrap items-center justify-between gap-2 py-2">
                <div>
                  <span className="font-medium text-slate-100">{item.productDisplayName}</span>
                  <span className="ml-2 font-mono text-xs text-[var(--color-text-muted)]">{item.productKey}</span>
                  <p className="text-xs text-slate-400">
                    {item.status} · granted {new Date(item.grantedAt).toLocaleString()}
                  </p>
                </div>
                {item.status === 'Active' ? (
                  <button
                    type="button"
                    onClick={() => revokeMutation.mutate(item.entitlementId)}
                    disabled={revokeMutation.isPending}
                    data-testid={`entitlement-revoke-${item.entitlementId}`}
                    className="rounded-md bg-rose-700 px-3 py-1 text-xs font-medium text-white hover:bg-rose-600 disabled:opacity-50"
                  >
                    Revoke
                  </button>
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
