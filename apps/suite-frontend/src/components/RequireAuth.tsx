import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'

export function RequireAuth() {
  const { isAuthenticated, isBootstrapping, me } = useAuth()
  const location = useLocation()

  if (isBootstrapping) {
    return (
      <div className="flex min-h-screen items-center justify-center text-sm text-[var(--color-text-muted)]">
        Loading session…
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (
    me?.requiresPasswordChange &&
    !/^\/app\/nexarr\/preferences\/?$/.test(location.pathname)
    && location.pathname !== '/app/nexarr/preferences'
  ) {
    return <Navigate to="/app/nexarr/preferences" replace />
  }

  return <Outlet />
}
