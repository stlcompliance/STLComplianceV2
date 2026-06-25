import { Link } from 'react-router-dom'
import type { MeResponse, NavigationItem, TenantSummary } from '../../api/types'
import { buildWhatINeedActions, type DashboardActionKind } from '../../lib/dashboard'
import { isInSuiteProduct } from '../../lib/permissions'
import { useProductLaunch } from '../../hooks/useProductLaunch'
import { DashboardCard } from './DashboardCard'

const kindStyles: Record<DashboardActionKind, string> = {
  warning: 'border-amber-800/60 bg-amber-950/30',
  action: 'border-slate-700 bg-slate-950/40',
  info: 'border-slate-700 bg-slate-900/60',
}

export function WhatINeedWidget({
  me,
  tenants,
  navigationProducts,
}: {
  me: MeResponse
  tenants: readonly TenantSummary[]
  navigationProducts: readonly NavigationItem[]
}) {
  const launch = useProductLaunch()
  const actions = buildWhatINeedActions({
    me,
    tenants,
    navigationProducts,
  })

  if (actions.length === 0) {
    return (
      <DashboardCard title="What you need">
        <p className="text-sm text-slate-400">You are set — no outstanding actions.</p>
      </DashboardCard>
    )
  }

  return (
    <DashboardCard title="What you need" className="sm:col-span-2">
      <ul className="space-y-2">
        {actions.map((action) => (
          <li
            key={action.id}
            className={`rounded-md border px-3 py-2 ${kindStyles[action.kind]}`}
          >
            <div className="flex flex-wrap items-start justify-between gap-2">
              <div className="min-w-0 flex-1">
                <p className="text-sm font-medium text-white">{action.title}</p>
                {action.description && (
                  <p className="mt-0.5 text-xs text-slate-400">{action.description}</p>
                )}
              </div>
              {action.href && action.productKey && !isInSuiteProduct(action.productKey) ? (
                <div className="flex shrink-0 gap-2">
                  <Link
                    to={action.href}
                    className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800/50"
                  >
                    Details
                  </Link>
                  <button
                    type="button"
                    disabled={launch.isPending}
                    onClick={() => launch.mutate(action.productKey!)}
                    className="rounded bg-teal-600 px-2 py-1 text-xs font-medium text-white hover:bg-teal-500 disabled:opacity-50"
                  >
                    Launch
                  </button>
                </div>
              ) : action.href ? (
                <Link
                  to={action.href}
                  className="shrink-0 rounded bg-teal-600 px-2 py-1 text-xs font-medium text-white hover:bg-teal-500"
                >
                  Open
                </Link>
              ) : null}
            </div>
          </li>
        ))}
      </ul>
    </DashboardCard>
  )
}
