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
      <p className="px-3 text-xs text-[var(--color-text-muted)]" role="status">
        No enabled surfaces for this product.
      </p>
    )
  }

  if (variant === 'mobile') {
    return (
      <nav aria-label="Product surfaces" className="mt-3">
        <div className="flex gap-2 overflow-x-auto pb-1 [&::-webkit-scrollbar]:hidden">
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
                    'flex min-h-10 shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]',
                    isActive
                      ? 'bg-[var(--color-accent-soft)] text-[var(--color-text-primary)] ring-1 ring-[var(--color-accent-border)]'
                      : 'text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-surface-elevated)] hover:text-[var(--color-text-primary)]',
                    launch ? 'border border-dashed border-[var(--color-accent-border)]' : '',
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
    <nav
      aria-label="Product surfaces"
      className="mt-4 flex flex-col gap-0.5 border-t border-[var(--color-border-subtle)] pt-4"
    >
      <p className="px-3 text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">In this product</p>
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
                'flex min-h-10 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]',
                isActive
                  ? 'border-l-2 border-[var(--color-accent)] bg-[var(--color-accent-soft)] pl-[10px] text-[var(--color-text-primary)]'
                  : 'border-l-2 border-transparent text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-surface-elevated)] hover:text-[var(--color-text-primary)]',
                launch ? 'border border-dashed border-[var(--color-accent-border)]' : '',
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
