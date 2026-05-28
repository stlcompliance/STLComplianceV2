import { ChevronDown } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useLocation, useNavigate } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { useProductLaunch } from '../hooks/useProductLaunch'
import { getProductIcon } from '../lib/productIcons'
import {
  hasProductEntitlement,
  isInSuiteProduct,
} from '../lib/permissions'
import { normalizeProductKey } from '../navigation/suiteNavigation'

const PRODUCT_DESCRIPTIONS: Record<string, string> = {
  nexarr: 'Suite dashboard and control plane',
  staffarr: 'People, org, and readiness',
  trainarr: 'Training and qualifications',
  maintainarr: 'Assets and maintenance',
  routarr: 'Routes and dispatch',
  supplyarr: 'Procurement and inventory',
  compliancecore: 'Rules, vocabulary, and references',
  companion: 'Field inbox and mobile tasks',
}

function resolveProductDescription(productKey: string): string | undefined {
  return PRODUCT_DESCRIPTIONS[normalizeProductKey(productKey)]
}

function resolveCurrentProductKey(pathname: string): string {
  if (pathname.startsWith('/app/platform-admin')) {
    return 'nexarr'
  }
  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (match) {
    return normalizeProductKey(match[1])
  }
  return 'nexarr'
}

export function ProductSwitcher() {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const menuId = useId()
  const location = useLocation()
  const navigate = useNavigate()
  const { me } = useAuth()
  const launch = useProductLaunch()
  const currentProductKey = resolveCurrentProductKey(location.pathname)
  const CurrentIcon = getProductIcon(currentProductKey)

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: me !== undefined,
  })

  const entitlements = me?.entitlements ?? []
  const entitledProducts = (navigationQuery.data?.products ?? []).filter((product) =>
    hasProductEntitlement(entitlements, product.productKey),
  )
  const currentProduct =
    entitledProducts.find(
      (product) => normalizeProductKey(product.productKey) === currentProductKey,
    ) ?? entitledProducts[0]

  useEffect(() => {
    if (!open) {
      return
    }

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleEscape)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [open])

  function handleSelect(productKey: string) {
    const normalized = normalizeProductKey(productKey)
    setOpen(false)

    if (normalized === currentProductKey) {
      return
    }

    if (isInSuiteProduct(normalized)) {
      navigate('/app')
      return
    }

    void launch.mutate(normalized)
  }

  if (navigationQuery.isLoading) {
    return <span className="text-xs text-slate-400">Loading products…</span>
  }

  if (entitledProducts.length === 0) {
    return <span className="text-xs text-slate-500">No entitled products</span>
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        disabled={launch.isPending}
        onClick={() => setOpen((value) => !value)}
        className="inline-flex max-w-[14rem] items-center gap-2 rounded-md border border-white/20 bg-white/5 px-3 py-1.5 text-left text-sm text-white hover:bg-white/10 disabled:opacity-50"
      >
        <CurrentIcon className="h-4 w-4 shrink-0 text-stl-teal" aria-hidden />
        <span className="min-w-0 truncate font-medium">
          {currentProductKey === 'nexarr'
            ? 'Suite'
            : (currentProduct?.displayName ?? 'Suite')}
        </span>
        <ChevronDown
          className={['h-4 w-4 shrink-0 text-slate-300 transition-transform', open ? 'rotate-180' : ''].join(
            ' ',
          )}
          aria-hidden
        />
      </button>

      {open ? (
        <ul
          id={menuId}
          role="menu"
          aria-label="Switch product"
          className="absolute right-0 z-50 mt-2 w-72 overflow-hidden rounded-md border border-slate-600 bg-[#0a101c] py-1 shadow-xl"
        >
          {entitledProducts.map((product) => {
            const Icon = getProductIcon(product.productKey)
            const isCurrent = normalizeProductKey(product.productKey) === currentProductKey

            return (
              <li key={product.productKey} role="none">
                <button
                  type="button"
                  role="menuitem"
                  aria-current={isCurrent ? 'true' : undefined}
                  disabled={launch.isPending}
                  onClick={() => handleSelect(product.productKey)}
                  className={[
                    'flex w-full items-start gap-3 px-3 py-2 text-left text-sm transition-colors disabled:opacity-50',
                    isCurrent
                      ? 'bg-slate-800/80 text-white'
                      : 'text-slate-200 hover:bg-slate-800/50 hover:text-white',
                  ].join(' ')}
                >
                  <Icon className="mt-0.5 h-4 w-4 shrink-0 text-stl-teal" aria-hidden />
                  <span className="min-w-0">
                    <span className="block font-medium">{product.displayName}</span>
                    {resolveProductDescription(product.productKey) ? (
                      <span className="mt-0.5 block text-xs text-slate-400">
                        {resolveProductDescription(product.productKey)}
                      </span>
                    ) : null}
                  </span>
                </button>
              </li>
            )
          })}
        </ul>
      ) : null}

      {launch.isError ? (
        <p className="absolute right-0 mt-1 w-72 text-xs text-red-300" role="alert">
          Launch failed: {(launch.error as Error).message}
        </p>
      ) : null}
    </div>
  )
}
