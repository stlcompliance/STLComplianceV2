import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { resolveNexArrLaunchFailureMessage, resolveProductLaunchCallbackPath, saveThemePreferenceFromSession } from '@stl/shared-ui'
import { redeemHandoff } from '../api/client'
import { saveSession, toStoredSession } from '../auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch Compliance Core from the suite.')
      return
    }

    let cancelled = false

    ;(async () => {
      try {
        const session = await redeemHandoff(handoff)
        if (cancelled) {
          return
        }
        saveThemePreferenceFromSession(session, { appKey: 'compliancecore' })
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          setError(resolveNexArrLaunchFailureMessage('Compliance Core', err))
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
        {error ? (
          <>
            <h1 className="text-lg font-semibold text-[var(--tone-danger-text)]">Launch failed</h1>
            <p className="mt-3 text-sm text-[var(--color-text-secondary)]">{error}</p>
          </>
        ) : (
          <p className="text-[var(--color-text-secondary)]">Redeeming Compliance Core handoff…</p>
        )}
      </div>
    </main>
  )
}
