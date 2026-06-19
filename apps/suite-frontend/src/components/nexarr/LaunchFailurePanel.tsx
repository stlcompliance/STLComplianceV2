import { AlertTriangle, ShieldAlert } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { LaunchContextResponse } from '../../api/types'
import {
  buildLaunchFailureFromContext,
  resolveLaunchFailureCopy,
  type LaunchFailureCopy,
} from '../../lib/launchFailure'
import { isPlatformAdmin } from '../../lib/permissions'
import { useAuth } from '../../auth/AuthProvider'

type LaunchFailurePanelProps = {
  productDisplayName: string
  productKey: string
  context?: LaunchContextResponse | null
  reasonCode?: string | null
  showAdminLink?: boolean
}

function panelStyles(severity: LaunchFailureCopy['severity']): string {
  return severity === 'error'
    ? 'border-red-800/60 bg-red-950/30'
    : 'border-amber-800/60 bg-amber-950/30'
}

export function LaunchFailurePanel({
  productDisplayName,
  productKey,
  context,
  reasonCode,
  showAdminLink = true,
}: LaunchFailurePanelProps) {
  const { me } = useAuth()
  const copy =
    context !== undefined && context !== null
      ? buildLaunchFailureFromContext(context)
      : resolveLaunchFailureCopy(reasonCode)

  if (!copy) {
    return null
  }

  const Icon = copy.severity === 'error' ? ShieldAlert : AlertTriangle
  const normalizedKey = productKey.trim().toLowerCase()

  return (
    <div
      role="alert"
      data-testid="launch-failure-panel"
      className={`rounded-lg border p-4 ${panelStyles(copy.severity)}`}
    >
      <div className="flex gap-3">
        <Icon
          className={`mt-0.5 h-5 w-5 shrink-0 ${copy.severity === 'error' ? 'text-red-300' : 'text-amber-300'}`}
          aria-hidden
        />
        <div className="min-w-0 space-y-2 text-sm">
          <div>
            <p className="font-semibold text-white">{copy.title}</p>
            <p className="mt-1 text-slate-300">
              Cannot launch <span className="font-medium">{productDisplayName}</span>. {copy.message}
            </p>
          </div>
          <p className="text-xs text-slate-400">{copy.guidance}</p>
          {(reasonCode ?? context?.denialReasonCode) && (
            <p className="font-mono text-xs text-[var(--color-text-muted)]">
              Reason code: {reasonCode ?? context?.denialReasonCode}
            </p>
          )}
          {showAdminLink && isPlatformAdmin(me) && (
            <div className="flex flex-wrap gap-3 pt-1">
              <Link
                to="/app/platform-admin/launch"
                className="text-xs font-medium text-teal-400 hover:text-teal-300"
              >
                Open launch diagnostics
              </Link>
              <Link
                to={`/app/${normalizedKey}/launch`}
                className="text-xs font-medium text-slate-300 hover:text-white"
              >
                Review launch surface
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
