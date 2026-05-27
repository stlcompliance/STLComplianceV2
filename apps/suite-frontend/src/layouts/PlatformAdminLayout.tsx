import { Activity, Building2, LayoutDashboard, Package } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'

const navItems = [
  { to: '/app/platform-admin', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/app/platform-admin/launch', label: 'Launch diagnostics', icon: Activity, end: false },
  { to: '/app/platform-admin/tenants', label: 'Tenants', icon: Building2, end: false },
  { to: '/app/platform-admin/products', label: 'Products', icon: Package, end: false },
] as const

export function PlatformAdminLayout() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold text-stl-navy">Platform administration</h3>
        <p className="mt-1 text-sm text-slate-600">
          Cross-tenant control plane data from NexArr{' '}
          <code className="text-xs">/api/platform-admin/*</code>.
        </p>
      </div>

      <nav
        aria-label="Platform admin sections"
        className="flex flex-wrap gap-2 border-b border-slate-200 pb-3"
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
                  ? 'bg-stl-teal/15 text-stl-navy'
                  : 'text-slate-600 hover:bg-slate-100',
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
