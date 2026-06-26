import { AlertTriangle, ShieldAlert } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { LaunchContextResponse } from '../../api/types'
import {
  buildLaunchFailureFromContext,
  describeLaunchFailure,
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
    ? 'border-[var(--color-danger-border)] bg-[var(--color-danger-bg)]'
    : 'border-[var(--color-warning-border)] bg-[var(--color-warning-bg)]'
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
  const failureDetails = describeLaunchFailure(reasonCode ?? context?.denialReasonCode)

  return (
    <div
      role="alert"
      data-testid="launch-failure-panel"
      className={`rounded-lg border p-4 ${panelStyles(copy.severity)}`}
    >
      <div className="flex gap-3">
        <Icon
          className={`mt-0.5 h-5 w-5 shrink-0 ${copy.severity === 'error' ? 'text-[var(--color-danger-text)]' : 'text-[var(--color-warning-text)]'}`}
          aria-hidden
        />
        <div className="min-w-0 space-y-2 text-sm">
          <div>
            <p className="font-semibold text-[var(--color-text-primary)]">{copy.title}</p>
            <p className="mt-1 text-[var(--color-text-secondary)]">
              Cannot launch <span className="font-medium">{productDisplayName}</span>. {copy.message}
            </p>
          </div>
          <p className="text-xs text-[var(--color-text-muted)]">{copy.guidance}</p>
          {failureDetails && isPlatformAdmin(me) && (
            <p className="font-mono text-xs text-[var(--color-text-muted)]">
              Code: {failureDetails.normalizedCode}
              {failureDetails.rawCode ? ` · raw ${failureDetails.rawCode}` : ''}
            </p>
          )}
          {showAdminLink && isPlatformAdmin(me) && (
            <div className="flex flex-wrap gap-3 pt-1">
              <Link
                to="/app/platform-admin/launch"
                className="text-xs font-medium text-[var(--color-accent)] hover:text-[var(--color-accent-strong)]"
              >
                Open launch diagnostics
              </Link>
              <Link
                to={`/app/${normalizedKey}/launch`}
                className="text-xs font-medium text-[var(--color-text-secondary)] hover:text-[var(--color-text-primary)]"
              >
                Review product launch page
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
