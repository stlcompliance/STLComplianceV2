import { LayoutDashboard } from 'lucide-react'
import { useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { ProductSwitcher } from './ProductSwitcher'
import { getProductNavIcon } from '../navigation/navIcons'
import { normalizeProductKey } from '../navigation/suiteNavigation'

function resolveTitle(pathname: string): { title: string; subtitle: string } {
  if (pathname.startsWith('/app/platform-admin')) {
    return { title: 'Platform administration', subtitle: 'NexArr control plane' }
  }

  if (pathname === '/app' || pathname === '/app/') {
    return { title: 'Suite dashboard', subtitle: 'Cross-product overview' }
  }

  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (!match) {
    return { title: 'Authenticated workspace', subtitle: 'STL Compliance Suite' }
  }

  const productKey = normalizeProductKey(match[1])
  return {
    title: productKey.charAt(0).toUpperCase() + productKey.slice(1),
    subtitle: 'Product workspace',
  }
}

export function AppTopBar() {
  const { me } = useAuth()
  const location = useLocation()
  const { title, subtitle } = resolveTitle(location.pathname)
  const productMatch = /^\/app\/([^/]+)/.exec(location.pathname)
  const ProductIcon = productMatch ? getProductNavIcon(productMatch[1]) : LayoutDashboard

  return (
    <header className="flex shrink-0 items-center justify-between border-b border-slate-700/40 bg-stl-navy px-6 py-4 text-white">
      <div className="flex min-w-0 items-center gap-3">
        <ProductIcon className="h-5 w-5 shrink-0 text-stl-teal" aria-hidden />
        <div className="min-w-0">
          <h2 className="truncate text-base font-semibold">{title}</h2>
          <p className="truncate text-xs text-slate-300">{subtitle}</p>
        </div>
      </div>

      <div className="flex items-center gap-4 text-sm">
        <ProductSwitcher />
        {me && (
          <div className="hidden text-right sm:block">
            <p className="font-medium">{me.displayName}</p>
            <p className="text-xs text-slate-300">{me.tenantDisplayName}</p>
          </div>
        )}
      </div>
    </header>
  )
}
