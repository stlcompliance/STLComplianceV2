import { LayoutDashboard } from 'lucide-react'
import { Link, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
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
    <header className="flex items-center justify-between border-b border-slate-700/40 bg-stl-navy px-6 py-4 text-white">
      <div className="flex items-center gap-3">
        <ProductIcon className="h-5 w-5 shrink-0 text-stl-teal" aria-hidden />
        <div>
          <h2 className="text-base font-semibold">{title}</h2>
          <p className="text-xs text-slate-300">{subtitle}</p>
        </div>
      </div>

      <div className="flex items-center gap-4 text-right text-sm">
        <Link
          to="/app"
          className="hidden rounded-md border border-white/20 px-3 py-1 text-xs text-slate-200 hover:bg-white/10 sm:inline-block"
        >
          Suite home
        </Link>
        {me && (
          <div>
            <p className="font-medium">{me.displayName}</p>
            <p className="text-xs text-slate-300">{me.tenantDisplayName}</p>
          </div>
        )}
      </div>
    </header>
  )
}
