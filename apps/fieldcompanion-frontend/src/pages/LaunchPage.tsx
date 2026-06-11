import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ApiErrorCallout, resolveProductLaunchCallbackPath } from '@stl/shared-ui'
import { FieldCompanionApiError, redeemHandoff } from '../api/client'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { saveSession, toStoredSession } from '../auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch the Field Companion app from the suite.')
      return
    }

    let cancelled = false

    ;(async () => {
      try {
        const session = await redeemHandoff(handoff)
        if (cancelled) {
          return
        }
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          if (err instanceof FieldCompanionApiError && err.status === 403) {
            setError('Your account is not entitled to the Field Companion app for this tenant.')
            return
          }
          if (err instanceof FieldCompanionApiError && err.status === 401) {
            setError('The handoff code is invalid or expired. Relaunch from the suite.')
            return
          }
          setError(FieldCompanionPlainReason(err, 'Handoff failed.'))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, searchParams])

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center shadow-lg">
        {error ? (
          <>
            <ApiErrorCallout title="Launch failed" message={error} />
          </>
        ) : (
          <p className="text-slate-300">Redeeming Field Companion handoff…</p>
        )}
      </div>
    </main>
  )
}
