import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, buildSemanticKey, GeneratedKeyField, getErrorMessage } from '@stl/shared-ui'
import { type FormEvent, useEffect, useMemo, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'

export function ProductCatalogAdminPanel() {
  const queryClient = useQueryClient()
  const [selectedProductKey, setSelectedProductKey] = useState('')
  const [productKey, setProductKey] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [sortOrder, setSortOrder] = useState('100')
  const [isActive, setIsActive] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [showProductKeyPolicy, setShowProductKeyPolicy] = useState(false)

  const productsQuery = useQuery({
    queryKey: ['platform-products-admin'],
    queryFn: () => nexarr.listProducts(1, 100),
  })

  const products = productsQuery.data?.items ?? []
  const selectedProduct = products.find((product) => product.productKey === selectedProductKey) ?? null
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

  const handleCreate = (event: FormEvent) => {
    event.preventDefault()
    createMutation.mutate()
  }

  return (
    <section
      data-testid="product-catalog-admin-panel"
      className="mt-6 space-y-6 rounded-xl border border-slate-200 bg-white p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-stl-navy">Product catalog administration</h2>
        <p className="mt-1 text-sm text-slate-600">
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
        <div className="space-y-1 text-sm text-slate-700">
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
              className="text-xs text-slate-500 underline-offset-2 hover:text-slate-700 hover:underline"
              onClick={() => setShowProductKeyPolicy(true)}
              disabled={createMutation.isPending}
            >
              Key policy
            </button>
          ) : null}
        </div>
        <label htmlFor="product-catalog-create-display-name" className="block text-sm text-slate-700 md:col-span-2">
          New product display name
          <input
            id="product-catalog-create-display-name"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            required
          />
        </label>
        <label htmlFor="product-catalog-create-sort-order" className="block text-sm text-slate-700">
          Catalog sort order
          <input
            id="product-catalog-create-sort-order"
            type="number"
            value={sortOrder}
            onChange={(event) => setSortOrder(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          />
        </label>
        <label htmlFor="product-catalog-create-active" className="flex items-center gap-2 text-sm text-slate-700 md:col-span-4">
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
            className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
          >
            {createMutation.isPending ? 'Creating…' : 'Create product'}
          </button>
        </div>
      </form>

      <div className="grid gap-3 md:grid-cols-2">
        <label htmlFor="product-catalog-selected-product" className="block text-sm text-slate-700">
          Product to edit
          <select
            id="product-catalog-selected-product"
            value={selectedProductKey}
            onChange={(event) => {
              const key = event.target.value
              setSelectedProductKey(key)
              const product = products.find((item) => item.productKey === key)
              setDisplayName(product?.displayName ?? '')
              setSortOrder(String(product?.sortOrder ?? 100))
              setIsActive(product?.isActive ?? true)
            }}
            className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          >
            <option value="">Select product…</option>
            {products.map((product) => (
              <option key={product.productKey} value={product.productKey}>
                {product.displayName} ({product.productKey})
              </option>
            ))}
          </select>
        </label>
        {selectedProduct ? (
          <div className="flex items-end">
            <button
              type="button"
              disabled={updateMutation.isPending}
              onClick={() => updateMutation.mutate()}
              className="rounded-md border border-slate-300 px-4 py-2 text-sm hover:bg-slate-50 disabled:opacity-50"
            >
              Save product changes
            </button>
          </div>
        ) : null}
      </div>
    </section>
  )
}
