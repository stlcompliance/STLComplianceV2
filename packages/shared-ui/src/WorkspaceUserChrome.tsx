export function WorkspaceUserChrome({
  userDisplayName,
  tenantDisplayName,
  tenantSlug,
  className = 'hidden text-right text-sm sm:block',
}: {
  userDisplayName?: string
  tenantDisplayName?: string
  tenantSlug?: string
  className?: string
}) {
  if (!userDisplayName && !tenantDisplayName && !tenantSlug) {
    return null
  }

  return (
    <div data-testid="workspace-user-chrome" className={className}>
      {userDisplayName ? (
        <p data-testid="workspace-user-display-name" className="font-medium text-slate-100">
          {userDisplayName}
        </p>
      ) : null}
      {tenantDisplayName ? (
        <p data-testid="workspace-tenant-display-name" className="text-xs text-slate-400">
          {tenantDisplayName}
        </p>
      ) : null}
      {tenantSlug ? (
        <p data-testid="workspace-tenant-slug" className="font-mono text-xs text-slate-500">
          {tenantSlug}
        </p>
      ) : null}
    </div>
  )
}
