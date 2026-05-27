import type { ReactNode } from 'react'

interface PermissionGateProps {
  allowed: boolean
  children: ReactNode
  fallback?: ReactNode
}

/** Hides UI the user cannot use; APIs remain authoritative. */
export function PermissionGate({ allowed, children, fallback = null }: PermissionGateProps) {
  if (!allowed) {
    return <>{fallback}</>
  }
  return <>{children}</>
}
