import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  StaticSearchPicker,
  buildSemanticKey,
  GeneratedKeyField,
  formatProductDisplayName,
  formatStatusLabel,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'
import { type FormEvent, useEffect, useMemo, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import { ConfirmDialog } from '../../feedback'

export function ProductCatalogAdminPanel() {
  const queryClient = useQueryClient()
  const [selectedProductKey, setSelectedProductKey] = useState('')
  const [productKey, setProductKey] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [sortOrder, setSortOrder] = useState('100')
  const [isActive, setIsActive] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [showProductKeyPolicy, setShowProductKeyPolicy] = useState(false)
  const [pendingToggle, setPendingToggle] = useState<'enable' | 'disable' | null>(null)

  const productsQuery = useQuery({
    queryKey: ['platform-products-admin'],
    queryFn: () => nexarr.listProducts(1, 100),
  })

  const products = productsQuery.data?.items ?? []
  const selectedProduct = products.find((product) => product.productKey === selectedProductKey) ?? null
  const selectedProductDetailQuery = useQuery({
    queryKey: ['platform-products-admin-detail', selectedProductKey],
    queryFn: () => nexarr.getProduct(selectedProductKey),
    enabled: Boolean(selectedProductKey),
  })
  const productOptions = useMemo<PickerOption[]>(
    () =>
      products.map((product) => ({
        value: product.productKey,
        label: `${product.displayName} (${product.productKey})`,
        inactive: !product.isActive,
      })),
    [products],
  )
  const generatedProductKey = useMemo(
    () =>
      buildSemanticKey({
        domain: 'product',
        kind: 'catalog',
        title: displayName.trim(),
        existingKeys: products.map((product) => product.productKey),
        maxLength: 128,
      }),
    [displayName, products],
  )

  useEffect(() => {
    setProductKey(generatedProductKey)
  }, [generatedProductKey])

  const createMutation = useMutation({
    mutationFn: () =>
      nexarr.createProduct({
        productKey: productKey.trim(),
        displayName: displayName.trim(),
        sortOrder: Number.parseInt(sortOrder, 10) || 100,
        isActive,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      setProductKey('')
      setDisplayName('')
      void queryClient.invalidateQueries({ queryKey: ['platform-products-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const updateMutation = useMutation({
    mutationFn: () =>
      nexarr.updateProduct(selectedProductKey, {
        displayName: displayName.trim(),
        sortOrder: Number.parseInt(sortOrder, 10) || 100,
        isActive,
      }),
    onSuccess: () => {
      setErrorMessage(null)
      void queryClient.invalidateQueries({ queryKey: ['platform-products-admin'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const enableMutation = useMutation({
    mutationFn: () => nexarr.enableProduct(selectedProductKey),
    onSuccess: async () => {
      setErrorMessage(null)
      setPendingToggle(null)
      await queryClient.invalidateQueries({ queryKey: ['platform-products-admin'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const disableMutation = useMutation({
    mutationFn: () => nexarr.disableProduct(selectedProductKey),
    onSuccess: async () => {
      setErrorMessage(null)
      setPendingToggle(null)
      await queryClient.invalidateQueries({ queryKey: ['platform-products-admin'] })
      await queryClient.invalidateQueries({ queryKey: ['platform-admin-product-overview'] })
    },
    onError: (error: Error) => setErrorMessage(error.message),
  })

  const handleCreate = (event: FormEvent) => {
    event.preventDefault()
    createMutation.mutate()
  }

  return (
    <section
      data-testid="product-catalog-admin-panel"
      className="mt-6 space-y-6 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5"
    >
      <ConfirmDialog
        open={pendingToggle !== null}
        title={pendingToggle === 'disable' ? 'Disable product?' : 'Enable product?'}
        description={
          selectedProduct
            ? pendingToggle === 'disable'
              ? `${selectedProduct.displayName} will stop launching until it is re-enabled.`
              : `${selectedProduct.displayName} will be launchable again for entitled tenants.`
            : ''
        }
        confirmLabel={pendingToggle === 'disable' ? 'Disable product' : 'Enable product'}
        danger={pendingToggle === 'disable'}
        loading={disableMutation.isPending || enableMutation.isPending}
        onCancel={() => {
          if (!disableMutation.isPending && !enableMutation.isPending) {
            setPendingToggle(null)
          }
        }}
        onConfirm={() => {
          if (pendingToggle === 'disable') {
            disableMutation.mutate()
          } else if (pendingToggle === 'enable') {
            enableMutation.mutate()
          }
        }}
      />

      <header>
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Product catalog administration</h2>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Create and update catalog entries via NexArr <code className="text-xs">/api/products</code>.
        </p>
      </header>

      {productsQuery.isError ? (
        <ApiErrorCallout
          message={getErrorMessage(productsQuery.error, 'Failed to load products.')}
          onRetry={() => void productsQuery.refetch()}
          retryLabel="Retry products"
        />
      ) : null}

      {errorMessage ? (
        <ApiErrorCallout message={errorMessage} />
      ) : null}

      <form className="grid gap-3 md:grid-cols-4" onSubmit={handleCreate}>
        <div className="space-y-1 text-sm text-[var(--color-text-secondary)]">
          <GeneratedKeyField
            sourceLabel={displayName.trim()}
            generatedKey={generatedProductKey}
            confirmedKey={productKey}
            manualOverride=""
            onManualOverrideChange={() => {}}
            showAdvancedKey={showProductKeyPolicy}
            disabled={createMutation.isPending}
            label="New product key"
          />
          {!showProductKeyPolicy ? (
            <button
              type="button"
              className="text-xs text-[var(--color-text-muted)] underline-offset-2 hover:text-[var(--color-text-secondary)] hover:underline"
              onClick={() => setShowProductKeyPolicy(true)}
              disabled={createMutation.isPending}
            >
              Key policy
            </button>
          ) : null}
        </div>
        <label htmlFor="product-catalog-create-display-name" className="block text-sm text-[var(--color-text-secondary)] md:col-span-2">
          New product display name
          <input
            id="product-catalog-create-display-name"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            required
          />
        </label>
        <label htmlFor="product-catalog-create-sort-order" className="block text-sm text-[var(--color-text-secondary)]">
          Catalog sort order
          <input
            id="product-catalog-create-sort-order"
            type="number"
            value={sortOrder}
            onChange={(event) => setSortOrder(event.target.value)}
            className="mt-1 w-full rounded-md border border-[var(--color-border-default)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
          />
        </label>
        <label htmlFor="product-catalog-create-active" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)] md:col-span-4">
          <input
            id="product-catalog-create-active"
            type="checkbox"
            checked={isActive}
            onChange={(event) => setIsActive(event.target.checked)}
          />
          Active in catalog
        </label>
        <div className="md:col-span-4">
          <button
            type="submit"
            disabled={createMutation.isPending || !productKey.trim() || !displayName.trim()}
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
          >
            {createMutation.isPending ? 'Creating…' : 'Create product'}
          </button>
        </div>
      </form>

      <div className="grid gap-4 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="space-y-3">
          <StaticSearchPicker
            label="Product to edit"
            id="product-catalog-selected-product"
            value={selectedProductKey}
            onChange={(key) => {
              setSelectedProductKey(key)
              const product = products.find((item) => item.productKey === key)
              setDisplayName(product?.displayName ?? '')
              setSortOrder(String(product?.sortOrder ?? 100))
              setIsActive(product?.isActive ?? true)
            }}
            options={productOptions}
            placeholder="Search products"
            testId="product-catalog-selected-product"
          />
          {selectedProduct ? (
            <div className="flex flex-wrap items-end gap-2">
              <button
                type="button"
                disabled={updateMutation.isPending}
                onClick={() => updateMutation.mutate()}
                className="rounded-md border border-[var(--color-border-default)] px-4 py-2 text-sm hover:bg-[var(--color-bg-surface-muted)] disabled:opacity-50"
              >
                Save product changes
              </button>
              <button
                type="button"
                disabled={selectedProduct.isActive ? disableMutation.isPending : enableMutation.isPending}
                onClick={() => setPendingToggle(selectedProduct.isActive ? 'disable' : 'enable')}
                className={[
                  'rounded-md px-4 py-2 text-sm font-medium disabled:opacity-50',
                  selectedProduct.isActive
                    ? 'border border-[var(--color-destructive-border)] text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)]'
                    : 'border border-[var(--color-success-border)] text-[var(--color-success-text)] hover:bg-[var(--color-success-bg)]',
                ].join(' ')}
              >
                {selectedProduct.isActive ? 'Disable product' : 'Enable product'}
              </button>
            </div>
          ) : null}
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] p-4">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Product manifest</h3>
          {selectedProductDetailQuery.isLoading ? (
            <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading product manifest…</p>
          ) : selectedProductDetailQuery.isError ? (
            <ApiErrorCallout
              message={getErrorMessage(selectedProductDetailQuery.error, 'Failed to load product manifest.')}
              onRetry={() => void selectedProductDetailQuery.refetch()}
              retryLabel="Retry manifest"
            />
          ) : selectedProductDetailQuery.data ? (
            <dl className="mt-3 grid gap-2 text-sm">
              <DetailRow label="Owner" value={formatProductDisplayName(selectedProductDetailQuery.data.productOwner)} />
              <DetailRow label="Category" value={selectedProductDetailQuery.data.productCategory} />
              <DetailRow label="Status" value={formatStatusLabel(selectedProductDetailQuery.data.productStatus)} />
              <DetailRow label="Environment" value={selectedProductDetailQuery.data.environmentKey} />
              <DetailRow label="Callback path" value={selectedProductDetailQuery.data.canonicalCallbackPath} mono />
              <DetailRow label="API base URL" value={selectedProductDetailQuery.data.apiBaseUrl} mono />
              <DetailRow label="Health URL" value={selectedProductDetailQuery.data.healthUrl} mono />
              <DetailRow label="Service audience" value={selectedProductDetailQuery.data.serviceAudience} mono />
              <DetailRow label="Marketing URL" value={selectedProductDetailQuery.data.marketingUrl} mono />
              <DetailRow label="Documentation URL" value={selectedProductDetailQuery.data.documentationUrl} mono />
              <DetailRow label="Support URL" value={selectedProductDetailQuery.data.supportUrl} mono />
              <DetailRow label="Dependency rules" value={selectedProductDetailQuery.data.entitlementDependencyRules} />
            </dl>
          ) : (
            <p className="mt-2 text-sm text-[var(--color-text-muted)]">Select a product to inspect its manifest details.</p>
          )}
        </div>
      </div>
    </section>
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
    <div className="grid grid-cols-[9rem_1fr] gap-3">
      <dt className="font-medium text-[var(--color-text-muted)]">{label}</dt>
      <dd className={mono ? 'font-mono text-xs break-all text-[var(--color-text-secondary)]' : 'text-[var(--color-text-secondary)]'}>{value || '—'}</dd>
    </div>
  )
}
