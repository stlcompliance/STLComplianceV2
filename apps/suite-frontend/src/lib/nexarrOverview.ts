import type { NavigationSurfaceItem, UserSessionSummary } from '../api/types'

export function countActiveSessions(sessions: readonly UserSessionSummary[]): number {
  return sessions.filter((session) => session.isActive).length
}

export function listEnabledSurfaces(
  surfaces: readonly NavigationSurfaceItem[],
): NavigationSurfaceItem[] {
  return [...surfaces]
    .filter((surface) => surface.isEnabled)
    .sort((a, b) => a.sortOrder - b.sortOrder)
}
