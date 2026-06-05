import { LayoutDashboard, LockKeyhole, LogOut, Shield } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { AppTopBar } from '../components/AppTopBar'
import { PermissionGate } from '../components/PermissionGate'
import { hasProductEntitlement, isPlatformAdmin } from '../lib/permissions'

export function AppShellLayout() {
  const { me, logout } = useAuth()
  const desktopNavLinkClassName = ({ isActive }: { isActive: boolean }) =>
    [
      'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      isActive
        ? 'border-l-2 border-stl-teal bg-slate-800/80 pl-[10px] text-white'
        : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
    ].join(' ')
  const mobileNavLinkClassName = ({ isActive }: { isActive: boolean }) =>
    [
      'flex shrink-0 items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      isActive
        ? 'bg-slate-800 text-white ring-1 ring-teal-500/50'
        : 'text-slate-300 hover:bg-slate-800/60 hover:text-white',
    ].join(' ')

  return (
    <div className="flex min-h-screen bg-[#0f172a] text-slate-100">
      <aside className="hidden min-h-0 w-64 shrink-0 flex-col overflow-y-auto border-r border-slate-700/70 bg-[#0a101c] p-4 lg:flex">
        <div className="mb-6 shrink-0">
          <p className="text-xs font-semibold uppercase tracking-wide text-stl-teal">
            STL Compliance
          </p>
          <h1 className="text-lg font-semibold text-white">Suite</h1>
          {me && (
            <div className="mt-1 space-y-0.5 text-xs text-slate-400">
              <p>{me.displayName}</p>
              <p>{me.tenantDisplayName}</p>
              <p className="font-mono text-slate-500">{me.tenantSlug}</p>
            </div>
          )}
        </div>

        <nav aria-label="Suite navigation" className="flex flex-col gap-1">
          <NavLink
            to="/app"
            end
            className={desktopNavLinkClassName}
          >
            <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
            Suite dashboard
          </NavLink>

          <PermissionGate allowed={hasProductEntitlement(me?.entitlements ?? [], 'nexarr')}>
            <NavLink
              to="/app/nexarr/identity"
              className={desktopNavLinkClassName}
            >
              <LockKeyhole className="h-4 w-4 shrink-0" aria-hidden />
              Identity & access
            </NavLink>
          </PermissionGate>
        </nav>

        <PermissionGate allowed={isPlatformAdmin(me)}>
          <NavLink
            to="/app/platform-admin"
            className={({ isActive }) =>
              [desktopNavLinkClassName({ isActive }), 'mt-6'].join(' ')
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
        <nav aria-label="Suite mobile navigation" className="border-b border-slate-700/70 bg-[#0a101c] px-4 py-2 lg:hidden">
          <div className="flex items-center gap-2 overflow-x-auto pb-1 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
            <NavLink to="/app" end className={mobileNavLinkClassName}>
              <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
              <span>Suite</span>
            </NavLink>
            <PermissionGate allowed={hasProductEntitlement(me?.entitlements ?? [], 'nexarr')}>
              <NavLink to="/app/nexarr/identity" className={mobileNavLinkClassName}>
                <LockKeyhole className="h-4 w-4 shrink-0" aria-hidden />
                <span>Identity</span>
              </NavLink>
            </PermissionGate>
            <PermissionGate allowed={isPlatformAdmin(me)}>
              <NavLink to="/app/platform-admin" className={mobileNavLinkClassName}>
                <Shield className="h-4 w-4 shrink-0" aria-hidden />
                <span>Admin</span>
              </NavLink>
            </PermissionGate>
            <button
              type="button"
              onClick={() => void logout()}
              className="ml-auto inline-flex shrink-0 items-center gap-2 rounded-md border border-slate-600 bg-slate-900/60 px-3 py-2 text-sm text-slate-100 hover:border-teal-500/50 hover:bg-slate-800/80"
            >
              <LogOut className="h-4 w-4 shrink-0 text-slate-300" aria-hidden />
              <span>Sign out</span>
            </button>
          </div>
        </nav>
        <main className="min-h-0 flex-1 overflow-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
