import { LogOut, Shield } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { ProductSwitcher } from '../components/ProductSwitcher'
import { PermissionGate } from '../components/PermissionGate'
import { isPlatformAdmin } from '../lib/permissions'

export function AppShellLayout() {
  const { me, logout } = useAuth()

  return (
    <div className="flex min-h-screen">
      <aside className="flex w-64 shrink-0 flex-col border-r border-slate-200 bg-white/80 p-4">
        <div className="mb-6">
          <p className="text-xs font-semibold uppercase tracking-wide text-stl-teal">
            STL Compliance
          </p>
          <h1 className="text-lg font-semibold text-stl-navy">Suite</h1>
          {me && (
            <p className="mt-1 text-xs text-slate-600">
              {me.displayName} · {me.tenantDisplayName}
            </p>
          )}
        </div>

        <ProductSwitcher />

        <PermissionGate allowed={isPlatformAdmin(me)}>
          <NavLink
            to="/app/platform-admin"
            className={({ isActive }) =>
              [
                'mt-6 flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-stl-teal/15 text-stl-navy'
                  : 'text-slate-700 hover:bg-white/70',
              ].join(' ')
            }
          >
            <Shield className="h-4 w-4 shrink-0" aria-hidden />
            Platform admin
          </NavLink>
        </PermissionGate>

        <button
          type="button"
          onClick={() => void logout()}
          className="mt-auto flex items-center gap-2 rounded-md px-3 py-2 text-sm text-slate-700 hover:bg-slate-100"
        >
          <LogOut className="h-4 w-4" aria-hidden />
          Sign out
        </button>
      </aside>

      <main className="flex flex-1 flex-col">
        <header className="border-b border-slate-200 bg-white/60 px-6 py-4">
          <h2 className="text-base font-semibold text-stl-navy">Authenticated workspace</h2>
        </header>
        <div className="flex-1 p-6">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
