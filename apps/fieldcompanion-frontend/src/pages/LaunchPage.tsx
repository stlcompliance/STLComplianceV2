import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
  ApiErrorCallout,
  resolveProductLaunchCallbackPath,
  resolveSuiteHomeUrl,
  saveThemePreferenceFromSession,
} from '@stl/shared-ui'
import { redeemHandoff } from '../api/client'
import type { HandoffSessionResponse } from '../api/types'
import { FieldCompanionPlainReason } from '../lib/FieldCompanionPlainReason'
import { saveSession, toStoredSession } from '../auth/sessionStorage'
import { DegradedOperationPanel } from '../components/DegradedOperationPanel'
import { buildDeviceCapabilitySnapshot } from '../lib/deviceCapabilities'
import { buildFieldCompanionOperationalFallbackSnapshot } from '../lib/degradedOperation'
import { readCurrentFieldCompanionReleaseSafetySnapshot } from '../lib/releaseSafety'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)

export function LaunchPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [errorTitle, setErrorTitle] = useState('Launch failed')
  const redeemRequestRef = useRef<{ handoff: string; promise: Promise<HandoffSessionResponse> } | null>(null)
  const releaseSafety = readCurrentFieldCompanionReleaseSafetySnapshot()
  const deviceSnapshot = useMemo(() => buildDeviceCapabilitySnapshot(), [])
  const degradedOperation = buildFieldCompanionOperationalFallbackSnapshot({
    deviceSnapshot,
    isOnline: typeof navigator === 'undefined' ? true : navigator.onLine,
    releaseSafety,
    launchError: error,
  })

  useEffect(() => {
    if (releaseSafety.isActionBlocked) {
      setErrorTitle('Update required')
      setError(releaseSafety.message)
      return
    }

    const handoff = searchParams.get('handoff')
    if (!handoff) {
      setErrorTitle('Launch failed')
      setError('Missing handoff code. Launch the Field Companion app from the suite.')
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
        saveThemePreferenceFromSession(session, { appKey: 'fieldcompanion' })
        saveSession(toStoredSession(session))
        navigate(resolveProductLaunchCallbackPath(session.callbackUrl), { replace: true })
      } catch (err) {
        if (!cancelled) {
          setErrorTitle('Launch failed')
          setError(FieldCompanionPlainReason(err, 'Handoff failed.'))
        }
      }
    })()

    return () => {
      cancelled = true
    }
  }, [navigate, releaseSafety.isActionBlocked, releaseSafety.message, searchParams])

  return (
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="max-w-md space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-8 shadow-lg">
        {degradedOperation.isVisible ? (
          <DegradedOperationPanel
            snapshot={degradedOperation}
            actions={[
              {
                label: 'Return to suite home',
                href: suiteHomeUrl,
                testId: 'fieldcompanion-launch-return-suite-home',
              },
              {
                label: 'Retry launch',
                onClick: () => {
                  window.location.reload()
                },
                testId: 'fieldcompanion-launch-retry',
              },
            ]}
          />
        ) : null}
        {error ? (
          <ApiErrorCallout title={errorTitle} message={error} />
        ) : (
          <p className="text-slate-300">Redeeming Field Companion handoff…</p>
        )}
      </div>
    </main>
  )
}
