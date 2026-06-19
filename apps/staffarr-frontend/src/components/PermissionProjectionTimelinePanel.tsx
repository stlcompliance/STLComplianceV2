import { ApiErrorCallout } from '@stl/shared-ui'
import type {
  EffectivePermissionProjectionResponse,
  OrgUnitResponse,
} from '../api/types'

interface PermissionProjectionTimelinePanelProps {
  personDisplayName: string
  orgUnits: OrgUnitResponse[]
  projection: EffectivePermissionProjectionResponse | null
  isLoading?: boolean
  isError?: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
}

function scopeLabel(scopeType: string, scopeValue: string | null, orgUnits: OrgUnitResponse[]): string {
  if (scopeType === 'tenant') {
    return 'Tenant-wide'
  }

  return orgUnits.find((unit) => unit.orgUnitId === scopeValue)?.name ?? scopeValue ?? 'Unknown scope'
}

export function PermissionProjectionTimelinePanel({
  personDisplayName,
  orgUnits,
  projection,
  isLoading = false,
  isError = false,
  readErrorMessage = null,
  onRetryRead,
}: PermissionProjectionTimelinePanelProps) {
  const projectedPermissions = projection?.permissions ?? []

  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-sm font-medium text-slate-300">Scoped effective permissions</h2>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">Computed permissions for {personDisplayName}.</p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading permission projection and history…</p>
      ) : isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Permission projection unavailable"
            message={readErrorMessage ?? 'Failed to load permission projection and history.'}
            onRetry={onRetryRead}
            retryLabel="Retry permissions"
          />
        </div>
      ) : null}

      <div className="mt-5">
        <h3 className="text-sm font-medium text-slate-300">Effective permission projection</h3>
        {!isLoading && !isError && projectedPermissions.length === 0 ? (
          <p className="mt-3 text-sm text-slate-400">No active effective permissions were computed.</p>
        ) : !isLoading && !isError ? (
          <ul className="mt-3 divide-y divide-slate-700 text-sm">
            {projectedPermissions.map((permission) => (
              <li key={`${permission.permissionKey}-${permission.scopeType}-${permission.scopeValue ?? ''}`} className="py-2">
                <p className="text-white">{permission.permissionName}</p>
                <p className="text-xs text-slate-400">
                  {permission.permissionKey} · {scopeLabel(permission.scopeType, permission.scopeValue, orgUnits)}
                </p>
                <p className="text-xs text-[var(--color-text-muted)]">Sources: {permission.sources.length}</p>
              </li>
            ))}
          </ul>
        ) : null}
      </div>
    </section>
  )
}
