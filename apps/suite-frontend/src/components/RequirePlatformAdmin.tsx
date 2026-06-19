import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { isPlatformAdmin } from '../lib/permissions'

/** Blocks platform-admin routes for non-admins. */
export function RequirePlatformAdmin() {
  const { me, isBootstrapping } = useAuth()

  if (isBootstrapping) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading…</p>
  }

  if (!isPlatformAdmin(me)) {
    return <Navigate to="/app" replace />
  }

  return <Outlet />
}
