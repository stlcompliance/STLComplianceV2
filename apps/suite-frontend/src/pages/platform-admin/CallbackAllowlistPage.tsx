import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'
import * as nexarr from '../../api/nexarrClient'
import { ConfirmDialog, useToast } from '../../feedback'
import { isActiveTenantStatus } from '../../lib/tenantStatus'

type PendingDelete = {
  entryId: string
  urlPattern: string
}

export function CallbackAllowlistPage() {
  const queryClient = useQueryClient()
  const { pushToast } = useToast()
  const [selectedProductKey, setSelectedProductKey] = useState('')
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [urlPattern, setUrlPattern] = useState('')
  const [patternType, setPatternType] = useState('origin')
  const [pendingDelete, setPendingDelete] = useState<PendingDelete | null>(null)

  const productsQuery = useQuery({
    queryKey: ['platform-admin-callback-allowlist-products'],
    queryFn: () => nexarr.getPlatformAdminProductOverview(),
  })

  const tenantsQuery = useQuery({
    queryKey: ['platform-admin-callback-allowlist-tenants'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 200),
  })

  const productOptions = useMemo<PickerOption[]>(
    () =>
      (productsQuery.data ?? []).map((product) => ({
        value: product.productKey,
        label: product.displayName,
        inactive: !product.isActive,
      })),
    [productsQuery.data],
  )

  const tenantOptions = useMemo<PickerOption[]>(
    () =>
      (tenantsQuery.data?.items ?? []).map((tenant) => ({
        value: tenant.tenantId,
        label: tenant.displayName,
        inactive: !isActiveTenantStatus(tenant.status),
      })),
    [tenantsQuery.data?.items],
  )

  useEffect(() => {
    if (!selectedProductKey && productOptions.length > 0) {
      setSelectedProductKey(productOptions[0].value)
    }
  }, [productOptions, selectedProductKey])

  const allowlistQuery = useQuery({
    queryKey: ['platform-admin-callback-allowlist', selectedProductKey, selectedTenantId],
    queryFn: () => nexarr.listCallbackAllowlist(selectedProductKey, selectedTenantId || null),
    enabled: Boolean(selectedProductKey),
  })

  const createMutation = useMutation({
    mutationFn: () =>
      nexarr.createCallbackAllowlistEntry({
        productKey: selectedProductKey,
        tenantId: selectedTenantId || null,
        urlPattern: urlPattern.trim(),
        patternType,
      }),
    onSuccess: async () => {
      setUrlPattern('')
      setPatternType('origin')
      pushToast({ message: 'Callback allowlist entry created.', variant: 'success' })
      await queryClient.invalidateQueries({
        queryKey: ['platform-admin-callback-allowlist', selectedProductKey, selectedTenantId],
      })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const deleteMutation = useMutation({
    mutationFn: (entryId: string) => nexarr.deleteCallbackAllowlistEntry(entryId),
    onSuccess: async () => {
      setPendingDelete(null)
      pushToast({ message: 'Callback allowlist entry removed.', variant: 'success' })
      await queryClient.invalidateQueries({
        queryKey: ['platform-admin-callback-allowlist', selectedProductKey, selectedTenantId],
      })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const allowlist = allowlistQuery.data ?? []

  return (
    <div className="space-y-6">
      <ConfirmDialog
        open={pendingDelete !== null}
        title="Delete callback allowlist entry?"
        description={
          pendingDelete
            ? `This will remove ${pendingDelete.urlPattern} from the selected product allowlist.`
            : ''
        }
        confirmLabel="Delete allowlist entry"
        danger
        loading={deleteMutation.isPending}
        onCancel={() => {
          if (!deleteMutation.isPending) {
            setPendingDelete(null)
          }
        }}
        onConfirm={() => {
          if (pendingDelete) {
            deleteMutation.mutate(pendingDelete.entryId)
          }
        }}
      />

      <header>
        <h4 className="text-lg font-semibold text-[var(--color-text-primary)]">Callback allowlist</h4>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Manage which product callback URLs NexArr will accept during launch handoff validation.
        </p>
      </header>

      {(productsQuery.isError || tenantsQuery.isError) ? (
        <ApiErrorCallout
          message={
            productsQuery.isError
              ? getErrorMessage(productsQuery.error, 'Failed to load products.')
              : getErrorMessage(tenantsQuery.error, 'Failed to load tenants.')
          }
          onRetry={() => {
            if (productsQuery.isError) {
              void productsQuery.refetch()
            }
            if (tenantsQuery.isError) {
              void tenantsQuery.refetch()
            }
          }}
          retryLabel="Retry options"
        />
      ) : null}

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="grid gap-4 lg:grid-cols-3">
          <StaticSearchPicker
            label="Product"
            id="callback-allowlist-product"
            value={selectedProductKey}
            onChange={setSelectedProductKey}
            options={productOptions}
            placeholder="Search products"
            testId="callback-allowlist-product"
          />
          <StaticSearchPicker
            label="Tenant scope"
            id="callback-allowlist-tenant"
            value={selectedTenantId}
            onChange={setSelectedTenantId}
            options={tenantOptions}
            placeholder="Search tenants"
            testId="callback-allowlist-tenant"
          />
          <label className="block text-sm text-[var(--color-text-secondary)]">
            Pattern type
            <select
              value={patternType}
              onChange={(event) => setPatternType(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          >
              <option value="origin">Origin</option>
              <option value="prefix">Prefix</option>
            </select>
          </label>
        </div>

        <div className="mt-4 grid gap-3 md:grid-cols-[1fr_auto]">
          <label className="block text-sm text-[var(--color-text-secondary)]">
            URL pattern
            <input
              value={urlPattern}
              onChange={(event) => setUrlPattern(event.target.value)}
              placeholder={
                patternType === 'origin'
                  ? 'https://app.example.com'
                  : 'https://app.example.com/callback'
              }
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            />
          </label>
          <div className="flex items-end">
            <button
              type="button"
              disabled={!selectedProductKey || !urlPattern.trim() || createMutation.isPending}
              onClick={() => createMutation.mutate()}
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
            >
              {createMutation.isPending ? 'Saving…' : 'Add allowlist entry'}
            </button>
          </div>
        </div>
      </section>

      <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h5 className="font-semibold text-[var(--color-text-primary)]">Current entries</h5>
            <p className="text-sm text-[var(--color-text-muted)]">
              {selectedTenantId
                ? 'Showing global and tenant-specific entries for the selected product.'
                : 'Showing global and tenant-specific entries for the selected product.'}
            </p>
          </div>
          {allowlistQuery.isFetching ? (
            <span className="text-xs text-[var(--color-text-muted)]">Refreshing…</span>
          ) : null}
        </div>

        {allowlistQuery.isLoading ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading allowlist…</p>
        ) : allowlistQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(allowlistQuery.error, 'Failed to load allowlist.')}
            onRetry={() => void allowlistQuery.refetch()}
            retryLabel="Retry allowlist"
          />
        ) : allowlist.length === 0 ? (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">No callback allowlist entries found.</p>
        ) : (
          <ul className="mt-4 divide-y divide-[var(--color-border-subtle)] rounded-lg border border-[var(--color-border-subtle)]">
            {allowlist.map((entry) => (
              <li
                key={entry.entryId}
                className="flex flex-wrap items-center justify-between gap-3 px-4 py-3 text-sm"
              >
                <div>
                  <p className="font-medium text-[var(--color-text-primary)]">{entry.urlPattern}</p>
                  <p className="text-xs text-[var(--color-text-muted)]">
                    {entry.patternType} · {entry.tenantId ? `tenant ${entry.tenantId}` : 'global'} ·{' '}
                    {entry.isActive ? 'active' : 'inactive'}
                  </p>
                </div>
                <button
                  type="button"
                  disabled={deleteMutation.isPending}
                  onClick={() => setPendingDelete({ entryId: entry.entryId, urlPattern: entry.urlPattern })}
                  className="rounded-md border border-[var(--color-destructive-border)] px-3 py-1.5 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                >
                  Delete
                </button>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
