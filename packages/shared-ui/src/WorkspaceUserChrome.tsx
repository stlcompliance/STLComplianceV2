export function WorkspaceUserChrome({
  userDisplayName,
  tenantDisplayName,
  className = 'hidden text-right text-sm sm:block',
}: {
  userDisplayName?: string
  tenantDisplayName?: string
  tenantSlug?: string
  className?: string
}) {
  if (!userDisplayName && !tenantDisplayName) {
    return null
  }

  return (
    <div data-testid="workspace-user-chrome" className={className}>
      {userDisplayName ? (
        <p data-testid="workspace-user-display-name" className="font-medium text-[var(--color-text-primary)]">
          {userDisplayName}
        </p>
      ) : null}
      {tenantDisplayName ? (
        <p data-testid="workspace-tenant-display-name" className="text-xs text-[var(--color-text-muted)]">
          {tenantDisplayName}
        </p>
      ) : null}
    </div>
  )
}
