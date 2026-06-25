import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { isActiveTenantStatus } from '../../lib/tenantStatus'
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
  const [validationSummary, setValidationSummary] = useState<{
    validationStatus: string
    errorCode: string | null
    errorMessage: string | null
    validatedAt: string
    readyUrl: string | null
    latencyMs: number | null
  } | null>(null)

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
      setValidationSummary(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-profiles', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-effective', tenantId] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const validateMutation = useMutation({
    mutationFn: () =>
      nexarr.validateDataPlaneProfile({
        tenantId: tenantId.trim(),
        productKey,
        deploymentMode,
        dataEndpointUrl: dataEndpointUrl.trim() || null,
        notes: notes.trim() || null,
      }),
    onSuccess: (result) => {
      setErrorMessage(null)
      setValidationSummary({
        validationStatus: result.validationStatus,
        errorCode: result.errorCode,
        errorMessage: result.errorMessage,
        validatedAt: result.validatedAt,
        readyUrl: result.readyUrl,
        latencyMs: result.latencyMs,
      })
      setTrustStatus(result.profile.trustStatus)
      setDataEndpointUrl(result.profile.dataEndpointUrl ?? '')
      setNotes(result.profile.notes ?? '')
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-profiles', tenantId] })
      void queryClient.invalidateQueries({ queryKey: ['platform-data-plane-effective', tenantId] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const deleteMutation = useMutation({
    mutationFn: (key: string) => nexarr.deleteDataPlaneProfile(tenantId, key),
    onSuccess: () => {
      setErrorMessage(null)
      setValidationSummary(null)
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

  useEffect(() => {
    setOverridesPage(1)
  }, [tenantId])

  useEffect(() => {
    setValidationSummary(null)
  }, [tenantId, productKey])

  return (
    <section
      data-testid="hybrid-data-plane-panel"
      className="space-y-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Hybrid data-plane metadata</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          NexArr remains the hosted control plane while product data may live on Render, customer
          infrastructure, or a hybrid split. Customer-hosted endpoints stay untrusted until the
          owning service validates them.
        </p>
      </header>

      <div className="max-w-xl">
        <StaticSearchPicker
          label="Data-plane tenant"
          id="data-plane-tenant"
          value={tenantId}
          onChange={setTenantId}
          options={tenantOptions}
          placeholder="Search tenants"
          testId="data-plane-tenant"
        />
      </div>

      {tenantId ? (
        <EffectiveDeploymentCard
          isLoading={effectiveQuery.isLoading}
          isError={effectiveQuery.isError}
          error={effectiveQuery.error}
          profiles={effectiveProfiles}
          onRetry={() => void effectiveQuery.refetch()}
        />
      ) : null}

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Configure product data plane</h3>
        <div className="mt-3 grid gap-3 sm:grid-cols-2">
          <StaticSearchPicker
            label="Product data plane"
            id="data-plane-product"
            value={productKey}
            onChange={setProductKey}
            options={productOptions}
            placeholder="Search products"
            testId="data-plane-product"
          />
          <label htmlFor="data-plane-deployment-mode" className="block text-sm text-[var(--color-text-secondary)]">
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
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            >
              {DEPLOYMENT_MODES.map((mode) => (
                <option key={mode.value} value={mode.value}>
                  {mode.label}
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="data-plane-endpoint" className="block text-sm text-[var(--color-text-secondary)] sm:col-span-2">
            Customer data endpoint URL {endpointRequired ? '(required)' : '(hosted default)'}
            <input
              id="data-plane-endpoint"
              value={dataEndpointUrl}
              onChange={(event) => setDataEndpointUrl(event.target.value)}
              placeholder={endpointRequired ? 'https://customer.example/api' : 'Leave blank for hosted mode'}
              disabled={!endpointRequired}
              data-testid="data-plane-endpoint"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)] disabled:opacity-50"
            />
          </label>
          <label htmlFor="data-plane-trust-status" className="block text-sm text-[var(--color-text-secondary)]">
            Endpoint trust status
            <select
              id="data-plane-trust-status"
              value={trustStatus}
              onChange={(event) => setTrustStatus(event.target.value)}
              data-testid="data-plane-trust-status"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
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
          <label htmlFor="data-plane-notes" className="block text-sm text-[var(--color-text-secondary)] sm:col-span-2">
            Data-plane notes (optional)
            <input
              id="data-plane-notes"
              value={notes}
              onChange={(event) => setNotes(event.target.value)}
              data-testid="data-plane-notes"
              className="mt-1 w-full rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
        </div>
        <button
          type="button"
          onClick={() => upsertMutation.mutate()}
          disabled={!tenantId || !productKey || upsertMutation.isPending}
          data-testid="data-plane-save"
          className="mt-3 rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
        >
          {upsertMutation.isPending ? 'Saving…' : 'Save data-plane profile'}
        </button>
        <button
          type="button"
          onClick={() => validateMutation.mutate()}
          disabled={!tenantId || !productKey || validateMutation.isPending}
          data-testid="data-plane-validate"
          className="mt-3 ml-3 rounded-md border border-[var(--color-accent-border)] px-4 py-2 text-sm font-medium text-[var(--color-accent)] hover:bg-[var(--color-accent-subtle)] disabled:opacity-50"
        >
          {validateMutation.isPending ? 'Validating…' : 'Validate and save'}
        </button>
      </div>

      {errorMessage ? (
        <p className="text-sm text-[var(--color-danger-text)]" data-testid="data-plane-error">
          {errorMessage}
        </p>
      ) : null}

      {validationSummary ? (
        <div className="rounded-lg border border-[var(--color-success-border)] bg-[var(--color-success-bg)] p-4 text-sm text-[var(--color-success-text)]" data-testid="data-plane-validation-result">
          <p className="font-medium">Validation {validationSummary.validationStatus.toLowerCase()}</p>
          <div className="mt-2 grid gap-1 text-[var(--color-text-secondary)] sm:grid-cols-2">
            <span>
              Ready URL: {validationSummary.readyUrl ?? 'Hosted mode / not required'}
            </span>
            <span>
              Latency: {validationSummary.latencyMs != null ? `${Math.round(validationSummary.latencyMs)} ms` : 'n/a'}
            </span>
            <span>Validated at: {new Date(validationSummary.validatedAt).toLocaleString()}</span>
            <span>
              Error:{' '}
              {validationSummary.errorCode
                ? `${validationSummary.errorCode}${validationSummary.errorMessage ? ` - ${validationSummary.errorMessage}` : ''}`
                : 'none'}
            </span>
          </div>
        </div>
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
