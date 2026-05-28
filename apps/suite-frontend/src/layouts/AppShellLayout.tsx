import { LayoutDashboard, LogOut, Shield } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { AppTopBar } from '../components/AppTopBar'
import { PermissionGate } from '../components/PermissionGate'
import { isPlatformAdmin } from '../lib/permissions'

export function AppShellLayout() {
  const { me, logout } = useAuth()

  return (
    <div className="flex min-h-screen bg-[#0f172a] text-slate-100">
      <aside className="flex w-64 shrink-0 flex-col min-h-0 overflow-y-auto border-r border-slate-700/70 bg-[#0a101c] p-4">
        <div className="mb-6 shrink-0">
          <p className="text-xs font-semibold uppercase tracking-wide text-stl-teal">
            STL Compliance
          </p>
          <h1 className="text-lg font-semibold text-white">Suite</h1>
          {me && (
            <p className="mt-1 text-xs text-slate-400">
              {me.displayName} · {me.tenantDisplayName}
            </p>
          )}
        </div>

        <nav aria-label="Suite navigation" className="flex flex-col gap-1">
          <NavLink
            to="/app"
            end
            className={({ isActive }) =>
              [
                'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'border-l-2 border-stl-teal bg-slate-800/80 pl-[10px] text-white'
                  : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
              ].join(' ')
            }
          >
            <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
            Suite dashboard
          </NavLink>
        </nav>

        <PermissionGate allowed={isPlatformAdmin(me)}>
          <NavLink
            to="/app/platform-admin"
            className={({ isActive }) =>
              [
                'mt-6 flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'border-l-2 border-stl-teal bg-slate-800/80 pl-[10px] text-white'
                  : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
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
          className="mt-auto flex items-center gap-2 rounded-md px-3 py-2 text-sm text-slate-300 hover:bg-slate-800/50 hover:text-white"
        >
          <LogOut className="h-4 w-4 shrink-0" aria-hidden />
          Sign out
        </button>
      </aside>

      <div className="flex min-h-0 min-w-0 flex-1 flex-col">
        <AppTopBar />
        <main className="min-h-0 flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
