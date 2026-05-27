import type { ReactNode } from 'react'

/**
 * Display-only permission gate. Server APIs remain authoritative.
 */
export function PermissionHint({
  allowed,
  children,
}: {
  allowed: boolean
  children: ReactNode
}) {
  if (!allowed) {
    return null
  }

  return <>{children}</>
}
