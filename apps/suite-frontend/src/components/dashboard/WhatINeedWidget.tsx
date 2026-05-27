import { Link } from 'react-router-dom'
import type { EntitlementSummary, MeResponse, NavigationItem, TenantSummary } from '../../api/types'
import { buildWhatINeedActions, type DashboardActionKind } from '../../lib/dashboard'
import { isInSuiteProduct } from '../../lib/permissions'
import { useProductLaunch } from '../../hooks/useProductLaunch'
import { DashboardCard } from './DashboardCard'

const kindStyles: Record<DashboardActionKind, string> = {
  warning: 'border-amber-200 bg-amber-50/80',
  action: 'border-slate-100 bg-slate-50/80',
  info: 'border-slate-100 bg-white',
}

export function WhatINeedWidget({
  me,
  tenants,
  entitlements,
  navigationProducts,
}: {
  me: MeResponse
  tenants: readonly TenantSummary[]
  entitlements: readonly EntitlementSummary[]
  navigationProducts: readonly NavigationItem[]
}) {
  const launch = useProductLaunch()
  const actions = buildWhatINeedActions({
    me,
    tenants,
    entitlements,
    navigationProducts,
  })

  if (actions.length === 0) {
    return (
      <DashboardCard title="What you need">
        <p className="text-sm text-slate-600">You are set — no outstanding actions.</p>
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
                <p className="text-sm font-medium text-stl-navy">{action.title}</p>
                {action.description && (
                  <p className="mt-0.5 text-xs text-slate-600">{action.description}</p>
                )}
              </div>
              {action.href && action.productKey && !isInSuiteProduct(action.productKey) ? (
                <div className="flex shrink-0 gap-2">
                  <Link
                    to={action.href}
                    className="rounded border border-slate-200 px-2 py-1 text-xs text-slate-700 hover:bg-white"
                  >
                    Details
                  </Link>
                  <button
                    type="button"
                    disabled={launch.isPending}
                    onClick={() => launch.mutate(action.productKey!)}
                    className="rounded bg-stl-teal px-2 py-1 text-xs font-medium text-white hover:bg-stl-teal/90 disabled:opacity-50"
                  >
                    Launch
                  </button>
                </div>
              ) : action.href ? (
                <Link
                  to={action.href}
                  className="shrink-0 rounded bg-stl-teal px-2 py-1 text-xs font-medium text-white hover:bg-stl-teal/90"
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
