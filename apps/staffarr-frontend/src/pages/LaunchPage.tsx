import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { resolveNexArrLaunchFailureMessage } from '@stl/shared-ui'
import { redeemHandoff } from '../api/client'
import { saveSession, toStoredSession } from '../auth/sessionStorage'

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setError('Missing handoff code. Launch StaffArr from the suite.')
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
        navigate('/', { replace: true })
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
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center shadow-lg">
        <h1 className="text-xl font-semibold text-white">StaffArr</h1>
        {error ? (
          <p className="mt-4 text-sm text-red-300">{error}</p>
        ) : (
          <p className="mt-4 text-sm text-slate-400">Completing secure launch…</p>
        )}
      </div>
    </main>
  )
}
