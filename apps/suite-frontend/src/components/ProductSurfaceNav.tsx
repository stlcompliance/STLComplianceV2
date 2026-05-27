import { NavLink } from 'react-router-dom'
import type { NavigationSurfaceItem } from '../api/types'
import { getNavIcon } from '../navigation/navIcons'
import { buildProductSurfacePath, isLaunchSurface } from '../navigation/suiteNavigation'

type ProductSurfaceNavProps = {
  productKey: string
  surfaces: readonly NavigationSurfaceItem[]
}

export function ProductSurfaceNav({ productKey, surfaces }: ProductSurfaceNavProps) {
  const enabledSurfaces = surfaces.filter((surface) => surface.isEnabled)

  if (enabledSurfaces.length === 0) {
    return (
      <p className="px-3 text-xs text-slate-500" role="status">
        No enabled surfaces for this product.
      </p>
    )
  }

  return (
    <nav aria-label="Product surfaces" className="mt-4 flex flex-col gap-0.5 border-t border-slate-200 pt-4">
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
                'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-stl-teal/15 text-stl-navy'
                  : 'text-slate-700 hover:bg-white/70',
                launch ? 'border border-dashed border-stl-teal/40' : '',
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
