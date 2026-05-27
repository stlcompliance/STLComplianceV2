import type {
  EffectivePermissionProjectionResponse,
  OrgUnitResponse,
  PermissionHistoryTimelineEntryResponse,
} from '../api/types'

interface PermissionProjectionTimelinePanelProps {
  personDisplayName: string
  orgUnits: OrgUnitResponse[]
  projection: EffectivePermissionProjectionResponse | null
  timeline: PermissionHistoryTimelineEntryResponse[]
}

function scopeLabel(scopeType: string, scopeValue: string | null, orgUnits: OrgUnitResponse[]): string {
  if (scopeType === 'tenant') {
    return 'Tenant-wide'
  }

  return orgUnits.find((unit) => unit.orgUnitId === scopeValue)?.name ?? scopeValue ?? 'Unknown scope'
}

function eventTypeLabel(eventType: string): string {
  switch (eventType) {
    case 'assignment_created':
      return 'Assignment created'
    case 'assignment_status_updated':
      return 'Assignment status updated'
    case 'role_template_permissions_updated':
      return 'Role template updated'
    default:
      return eventType
  }
}

export function PermissionProjectionTimelinePanel({
  personDisplayName,
  orgUnits,
  projection,
  timeline,
}: PermissionProjectionTimelinePanelProps) {
  return (
    <section className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-sm font-medium text-slate-300">Scoped effective permissions and history</h2>
      <p className="mt-2 text-xs text-slate-500">Computed permissions and timeline for {personDisplayName}.</p>

      <div className="mt-5 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Effective permission projection</h3>
          {!projection || projection.permissions.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No active effective permissions were computed.</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-700 text-sm">
              {projection.permissions.map((permission) => (
                <li key={`${permission.permissionKey}-${permission.scopeType}-${permission.scopeValue ?? ''}`} className="py-2">
                  <p className="text-white">{permission.permissionName}</p>
                  <p className="text-xs text-slate-400">
                    {permission.permissionKey} · {scopeLabel(permission.scopeType, permission.scopeValue, orgUnits)}
                  </p>
                  <p className="text-xs text-slate-500">Sources: {permission.sources.length}</p>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-300">Permission history timeline</h3>
          {timeline.length === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No permission history events recorded yet.</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-700 text-sm">
              {timeline.map((entry) => (
                <li key={entry.eventId} className="py-2">
                  <p className="text-white">{eventTypeLabel(entry.eventType)}</p>
                  <p className="text-xs text-slate-400">
                    {entry.permissionKey} via {entry.roleKey} · {scopeLabel(entry.scopeType, entry.scopeValue, orgUnits)}
                  </p>
                  <p className="text-xs text-slate-500">{new Date(entry.occurredAt).toLocaleString()}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}
