import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import { EffectiveDeploymentCard } from './data-plane/EffectiveDeploymentCard'
import { OverridesCard } from './data-plane/OverridesCard'

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
  const [overridesPage, setOverridesPage] = useState(1)
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
    queryKey: ['platform-data-plane-profiles', tenantId, overridesPage],
    queryFn: () => nexarr.listDataPlaneProfiles({ tenantId, page: overridesPage, pageSize: 25 }),
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
  const profilesPage = profilesQuery.data
  const effectiveProfiles = effectiveQuery.data ?? []
  const endpointRequired = deploymentMode !== 'hosted'

  useEffect(() => {
    setOverridesPage(1)
  }, [tenantId])

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

      <label htmlFor="data-plane-tenant" className="block max-w-xl text-sm text-slate-300">
        Data-plane tenant
        <select
          id="data-plane-tenant"
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
        <EffectiveDeploymentCard
          isLoading={effectiveQuery.isLoading}
          isError={effectiveQuery.isError}
          error={effectiveQuery.error}
          profiles={effectiveProfiles}
          onRetry={() => void effectiveQuery.refetch()}
        />
      ) : null}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Configure product data plane</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <label htmlFor="data-plane-product" className="block text-sm text-slate-300">
            Product data plane
            <select
              id="data-plane-product"
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
          <label htmlFor="data-plane-deployment-mode" className="block text-sm text-slate-300">
            Deployment mode
            <select
              id="data-plane-deployment-mode"
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
          <label htmlFor="data-plane-endpoint" className="block text-sm text-slate-300 sm:col-span-2">
            Customer data endpoint URL {endpointRequired ? '(required)' : '(hosted default)'}
            <input
              id="data-plane-endpoint"
              value={dataEndpointUrl}
              onChange={(event) => setDataEndpointUrl(event.target.value)}
              placeholder={endpointRequired ? 'https://customer.example/api' : 'Not used for hosted mode'}
              disabled={!endpointRequired}
              data-testid="data-plane-endpoint"
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 disabled:opacity-50"
            />
          </label>
          <label htmlFor="data-plane-trust-status" className="block text-sm text-slate-300">
            Endpoint trust status
            <select
              id="data-plane-trust-status"
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
          <label htmlFor="data-plane-notes" className="block text-sm text-slate-300 sm:col-span-2">
            Data-plane notes (optional)
            <input
              id="data-plane-notes"
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
        <OverridesCard
          isLoading={profilesQuery.isLoading}
          isError={profilesQuery.isError}
          error={profilesQuery.error}
          pagedProfiles={profilesPage}
          deletePending={deleteMutation.isPending}
          onDelete={(productKey) => deleteMutation.mutate(productKey)}
          page={overridesPage}
          onPreviousPage={() => setOverridesPage((value) => Math.max(1, value - 1))}
          onNextPage={() => {
            if (profilesPage?.hasNextPage) {
              setOverridesPage((value) => value + 1)
            }
          }}
          onRetry={() => void profilesQuery.refetch()}
        />
      ) : null}
    </section>
  )
}
