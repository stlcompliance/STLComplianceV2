import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import * as nexarr from '../../api/nexarrClient'

const DEPLOYMENT_MODES = [
  { value: 'hosted', label: 'Hosted (Render)' },
  { value: 'customer_hosted', label: 'Customer-hosted' },
  { value: 'hybrid', label: 'Hybrid' },
] as const

const TRUST_STATUSES = [
  { value: 'trusted', label: 'Trusted' },
  { value: 'untrusted', label: 'Untrusted' },
  { value: 'pending_validation', label: 'Pending validation' },
] as const

export function HybridDataPlanePanel() {
  const queryClient = useQueryClient()
  const [tenantId, setTenantId] = useState('')
  const [productKey, setProductKey] = useState('')
  const [deploymentMode, setDeploymentMode] = useState('hosted')
  const [dataEndpointUrl, setDataEndpointUrl] = useState('')
  const [trustStatus, setTrustStatus] = useState('trusted')
  const [notes, setNotes] = useState('')
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-tenant-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
  })

  const productsQuery = useQuery({
    queryKey: ['platform-admin-product-overview'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  const profilesQuery = useQuery({
    queryKey: ['platform-data-plane-profiles', tenantId],
    queryFn: () => nexarr.listDataPlaneProfiles({ tenantId, pageSize: 100 }),
    enabled: Boolean(tenantId.trim()),
  })

  const effectiveQuery = useQuery({
    queryKey: ['platform-data-plane-effective', tenantId],
    queryFn: () => nexarr.listEffectiveDataPlaneProfiles(tenantId),
    enabled: Boolean(tenantId.trim()),
  })

  const upsertMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertDataPlaneProfile({
        tenantId: tenantId.trim(),
        productKey,
        deploymentMode,
        dataEndpointUrl: dataEndpointUrl.trim() || null,
        trustStatus,
        notes: notes.trim() || null,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-profiles', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-effective', tenantId] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const deleteMutation = useMutation({
    mutationFn: (key: string) => nexarr.deleteDataPlaneProfile(tenantId, key),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-profiles', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-effective', tenantId] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const tenants = tenantsQuery.data?.items ?? []
  const products = (productsQuery.data ?? []).filter((product) => product.isActive)
  const profiles = profilesQuery.data?.items ?? []
  const effectiveProfiles = effectiveQuery.data ?? []
  const endpointRequired = deploymentMode !== 'hosted'

  return (
    <section
      data-testid="hybrid-data-plane-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Hybrid data-plane metadata</h2>
        <p className="mt-1 text-sm text-slate-400">
          NexArr remains the hosted control plane while product data may live on Render, customer
          infrastructure, or a hybrid split. Customer-hosted endpoints stay untrusted until the
          owning service validates them.
        </p>
      </header>

      <label className="block max-w-xl text-sm text-slate-300">
        Tenant
        <select
          value={tenantId}
          onChange={(event) => setTenantId(event.target.value)}
          data-testid="data-plane-tenant"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
        >
          <option value="">Select tenant…</option>
          {tenants.map((tenant) => (
            <option key={tenant.tenantId} value={tenant.tenantId}>
              {tenant.displayName} ({tenant.slug})
            </option>
          ))}
        </select>
      </label>

      {tenantId ? (
        <div
          className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
          data-testid="data-plane-effective-section"
        >
          <h3 className="text-sm font-medium text-slate-200">Effective deployment map</h3>
          {effectiveQuery.isLoading ? (
            <p className="mt-2 text-sm text-slate-500">Loading effective profiles…</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-800 text-sm">
              {effectiveProfiles.map((profile) => (
                <li key={profile.productKey} className="py-2">
                  <span className="font-medium text-slate-100">{profile.productDisplayName}</span>
                  <span className="ml-2 font-mono text-xs text-teal-300">{profile.deploymentMode}</span>
                  <span className="ml-2 text-xs text-slate-500">{profile.trustStatus}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : null}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Configure product data plane</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <label className="block text-sm text-slate-300">
            Product
            <select
              value={productKey}
              onChange={(event) => setProductKey(event.target.value)}
              data-testid="data-plane-product"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="">Select product…</option>
              {products.map((product) => (
                <option key={product.productKey} value={product.productKey}>
                  {product.displayName}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300">
            Deployment mode
            <select
              value={deploymentMode}
              onChange={(event) => {
                setDeploymentMode(event.target.value)
                if (event.target.value === 'customer_hosted') {
                  setTrustStatus('untrusted')
                }
              }}
              data-testid="data-plane-deployment-mode"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {DEPLOYMENT_MODES.map((mode) => (
                <option key={mode.value} value={mode.value}>
                  {mode.label}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2">
            Data endpoint URL {endpointRequired ? '(required)' : '(hosted default)'}
            <input
              value={dataEndpointUrl}
              onChange={(event) => setDataEndpointUrl(event.target.value)}
              placeholder={endpointRequired ? 'https://customer.example/api' : 'Not used for hosted mode'}
              disabled={!endpointRequired}
              data-testid="data-plane-endpoint"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 disabled:opacity-50"
            />
          </label>
          <label className="block text-sm text-slate-300">
            Trust status
            <select
              value={trustStatus}
              onChange={(event) => setTrustStatus(event.target.value)}
              data-testid="data-plane-trust-status"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              {TRUST_STATUSES.map((status) => (
                <option
                  key={status.value}
                  value={status.value}
                  disabled={
                    deploymentMode === 'customer_hosted' && status.value === 'trusted'
                  }
                >
                  {status.label}
                </option>
              ))}
            </select>
          </label>
          <label className="block text-sm text-slate-300 sm:col-span-2">
            Notes (optional)
            <input
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
              data-testid="data-plane-notes"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
        </div>
        <button
          type="button"
          onClick={() => upsertMutation.mutate()}
          disabled={!tenantId || !productKey || upsertMutation.isPending}
          data-testid="data-plane-save"
          className="mt-3 rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
        >
          {upsertMutation.isPending ? 'Saving…' : 'Save data-plane profile'}
        </button>
      </div>

      {errorMessage ? (
        <p className="text-sm text-rose-400" data-testid="data-plane-error">
          {errorMessage}
        </p>
      ) : null}

      {tenantId ? (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Stored overrides</h3>
          {profilesQuery.isLoading ? (
            <p className="mt-2 text-sm text-slate-500">Loading overrides…</p>
          ) : profiles.length === 0 ? (
            <p className="mt-2 text-sm text-slate-500" data-testid="data-plane-overrides-empty">
              No overrides — all products default to hosted/trusted.
            </p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="data-plane-overrides-list">
              {profiles.map((profile) => (
                <li key={profile.profileId} className="flex flex-wrap items-center justify-between gap-2 py-2">
                  <div>
                    <span className="font-medium text-slate-100">{profile.productDisplayName}</span>
                    <p className="text-xs text-slate-400">
                      {profile.deploymentMode} · {profile.trustStatus}
                      {profile.dataEndpointUrl ? ` · ${profile.dataEndpointUrl}` : ''}
                    </p>
                  </div>
                  <button
                    type="button"
                    onClick={() => deleteMutation.mutate(profile.productKey)}
                    disabled={deleteMutation.isPending}
                    data-testid={`data-plane-reset-${profile.productKey}`}
                    className="rounded-md bg-slate-700 px-3 py-1 text-xs font-medium text-white hover:bg-slate-600 disabled:opacity-50"
                  >
                    Reset to hosted default
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : null}
    </section>
  )
}
