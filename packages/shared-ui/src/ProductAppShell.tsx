import { LayoutGrid, type LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { NavLink } from 'react-router-dom'

export type ProductNavItem = {
  label: string
  to: string
  icon?: LucideIcon
}

export type ProductAppShellProps = {
  productName: string
  workspaceSubtitle?: string
  tenantDisplayName?: string
  userDisplayName?: string
  navItems?: ProductNavItem[]
  children: ReactNode
}

export function ProductAppShell({
  productName,
  workspaceSubtitle = 'Operational workspace',
  tenantDisplayName,
  userDisplayName,
  navItems = [{ label: 'Workspace', to: '/' }],
  children,
}: ProductAppShellProps) {
  return (
    <div className="flex min-h-screen flex-col bg-[#0f172a] text-slate-100">
      <header className="flex h-14 shrink-0 items-center justify-between border-b border-slate-700/70 bg-[#0a101c] px-6">
        <div className="flex min-w-0 items-center gap-3">
          <LayoutGrid className="h-5 w-5 shrink-0 text-sky-400" aria-hidden />
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-white">{productName}</p>
            <p className="truncate text-xs text-slate-400">{workspaceSubtitle}</p>
          </div>
        </div>
        {(userDisplayName || tenantDisplayName) && (
          <div className="hidden text-right text-sm sm:block">
            {userDisplayName && <p className="font-medium text-slate-100">{userDisplayName}</p>}
            {tenantDisplayName && <p className="text-xs text-slate-400">{tenantDisplayName}</p>}
          </div>
        )}
      </header>

      <div className="flex min-h-0 flex-1">
        <aside className="flex w-56 shrink-0 flex-col border-r border-slate-700/70 bg-[#0a101c] p-4">
          <p className="px-3 text-xs font-semibold uppercase tracking-wide text-slate-500">
            Navigation
          </p>
          <nav aria-label={`${productName} navigation`} className="mt-3 flex flex-col gap-1">
            {navItems.map((item) => {
              const Icon = item.icon ?? LayoutGrid
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === '/'}
                  className={({ isActive }) =>
                    [
                      'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                      isActive
                        ? 'border-l-2 border-sky-400 bg-slate-800/80 pl-[10px] text-white'
                        : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
                    ].join(' ')
                  }
                >
                  <Icon className="h-4 w-4 shrink-0" aria-hidden />
                  <span>{item.label}</span>
                </NavLink>
              )
            })}
          </nav>
        </aside>

        <main className="min-w-0 flex-1 overflow-auto p-6">{children}</main>
      </div>
    </div>
  )
}
