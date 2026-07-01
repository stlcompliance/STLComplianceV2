import { useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { resolveNexArrLaunchFailureMessage, resolveProductLaunchCallbackPath, saveThemePreferenceFromSession } from '@stl/shared-ui'
import { redeemHandoff } from '../api/client'
import type { HandoffSessionResponse } from '../api/types'
import { saveSession, toStoredSession } from '../auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const redeemRequestRef = useRef<{ handoff: string; promise: Promise<HandoffSessionResponse> } | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch StaffArr from the suite.')
      return
    }
    const redeemPromise =
      redeemRequestRef.current?.handoff === handoff
        ? redeemRequestRef.current.promise
        : redeemHandoff(handoff)
    if (redeemRequestRef.current?.handoff !== handoff) {
      redeemRequestRef.current = { handoff, promise: redeemPromise }
    }

    let cancelled = false

    ;(async () => {
      try {
        const session = await redeemPromise
        if (cancelled) {
          return
        }
        saveThemePreferenceFromSession(session, { appKey: 'staffarr' })
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          setError(resolveNexArrLaunchFailureMessage('StaffArr', err))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, searchParams])

  return (
    <main className="flex min-h-screen items-center justify-center bg-[var(--color-bg-app)] p-6">
      <div className="max-w-md rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8 text-center shadow-[var(--shadow-surface)]">
        <h1 className="text-xl font-semibold text-[var(--color-text-primary)]">StaffArr</h1>
        {error ? (
          <p className="mt-4 text-sm text-[var(--tone-danger-text)]">{error}</p>
        ) : (
          <p className="mt-4 text-sm text-[var(--color-text-secondary)]">Completing secure launch…</p>
        )}
      </div>
    </main>
  )
}
