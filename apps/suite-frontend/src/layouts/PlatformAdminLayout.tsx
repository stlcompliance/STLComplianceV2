import {
  Activity,
  Archive,
  Building2,
  HeartPulse,
  KeyRound,
  LayoutDashboard,
  Package,
  RefreshCw,
  Scale,
  ServerCog,
} from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'

const navItems = [
  { to: '/app/platform-admin', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/app/platform-admin/launch', label: 'Launch diagnostics', icon: Activity, end: false },
  { to: '/app/platform-admin/tenants', label: 'Tenants', icon: Building2, end: false },
  { to: '/app/platform-admin/products', label: 'Products', icon: Package, end: false },
  { to: '/app/platform-admin/audit-export', label: 'Audit export', icon: Archive, end: false },
  { to: '/app/platform-admin/lifecycle', label: 'Lifecycle workers', icon: ServerCog, end: false },
  { to: '/app/platform-admin/orchestration', label: 'Worker health', icon: HeartPulse, end: false },
  { to: '/app/platform-admin/service-tokens', label: 'Service tokens', icon: KeyRound, end: false },
  { to: '/app/platform-admin/entitlements', label: 'Entitlements', icon: Scale, end: false },
  { to: '/app/platform-admin/tenant-lifecycle', label: 'Tenant lifecycle', icon: RefreshCw, end: false },
] as const

export function PlatformAdminLayout() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold text-white">Platform administration</h3>
        <p className="mt-1 text-sm text-slate-400">
          Cross-tenant control plane data from NexArr{' '}
          <code className="text-xs">/api/platform-admin/*</code>.
        </p>
      </div>

      <nav
        aria-label="Platform admin sections"
        className="flex flex-wrap gap-2 border-b border-slate-700 pb-3"
      >
        {navItems.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              [
                'inline-flex items-center gap-2 rounded-md px-3 py-1.5 text-sm font-medium',
                isActive
                  ? 'border-l-2 border-stl-teal bg-slate-800/80 pl-[10px] text-white'
                  : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
              ].join(' ')
            }
          >
            <Icon className="h-4 w-4 shrink-0" aria-hidden />
            {label}
          </NavLink>
        ))}
      </nav>

      <Outlet />
    </div>
  )
}
