import { NavLink } from 'react-router-dom'
import type { NavigationSurfaceItem } from '../api/types'
import { getNavIcon } from '../navigation/navIcons'
import { buildProductSurfacePath, isLaunchSurface } from '../navigation/suiteNavigation'

type ProductSurfaceNavProps = {
  productKey: string
  surfaces: readonly NavigationSurfaceItem[]
  variant?: 'sidebar' | 'mobile'
}

export function ProductSurfaceNav({
  productKey,
  surfaces,
  variant = 'sidebar',
}: ProductSurfaceNavProps) {
  const enabledSurfaces = surfaces.filter((surface) => surface.isEnabled)

  if (enabledSurfaces.length === 0) {
    return (
      <p className="px-3 text-xs text-slate-500" role="status">
        No enabled surfaces for this product.
      </p>
    )
  }

  if (variant === 'mobile') {
    return (
      <nav aria-label="Product surfaces" className="mt-3">
        <div className="flex gap-2 overflow-x-auto pb-1 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
          {enabledSurfaces.map((surface) => {
            const Icon = getNavIcon(surface.iconKey)
            const to = buildProductSurfacePath(productKey, surface)
            const launch = isLaunchSurface(surface)

            return (
              <NavLink
                key={surface.surfaceKey}
                to={to}
                end={!surface.relativePath}
                title={surface.permissionHint ?? undefined}
                className={({ isActive }) =>
                  [
                    'flex min-h-10 shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
                    isActive
                      ? 'bg-slate-800 text-white ring-1 ring-teal-500/50'
                      : 'text-slate-300 hover:bg-slate-800/60 hover:text-white',
                    launch ? 'border border-dashed border-teal-500/40' : '',
                  ].join(' ')
                }
              >
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span>{surface.label}</span>
              </NavLink>
            )
          })}
        </div>
      </nav>
    )
  }

  return (
    <nav aria-label="Product surfaces" className="mt-4 flex flex-col gap-0.5 border-t border-slate-700 pt-4">
      <p className="px-3 text-xs font-semibold uppercase tracking-wide text-slate-500">In this product</p>
      {enabledSurfaces.map((surface) => {
        const Icon = getNavIcon(surface.iconKey)
        const to = buildProductSurfacePath(productKey, surface)
        const launch = isLaunchSurface(surface)

        return (
          <NavLink
            key={surface.surfaceKey}
            to={to}
            end={!surface.relativePath}
            title={surface.permissionHint ?? undefined}
            className={({ isActive }) =>
              [
                'flex min-h-10 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
                isActive
                  ? 'border-l-2 border-teal-400 bg-slate-800/80 pl-[10px] text-white'
                  : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
                launch ? 'border border-dashed border-teal-500/40' : '',
              ].join(' ')
            }
          >
            <Icon className="h-4 w-4 shrink-0" aria-hidden />
            <span>{surface.label}</span>
          </NavLink>
        )
      })}
    </nav>
  )
}
