import { RefreshCcw, ShieldCheck } from 'lucide-react'
import { ApiErrorCallout } from '@stl/shared-ui'

import type { FieldCompanionReleaseSafetySnapshot } from '../lib/releaseSafety'

interface FieldCompanionReleaseSafetyBannerProps {
  snapshot: FieldCompanionReleaseSafetySnapshot
  suiteHomeUrl: string
  onRefresh: () => void
}

export function FieldCompanionReleaseSafetyBanner({
  snapshot,
  suiteHomeUrl,
  onRefresh,
}: FieldCompanionReleaseSafetyBannerProps) {
  if (!snapshot.isActionBlocked && snapshot.releaseMode === 'ready' && snapshot.stagedFlags.length === 0 && snapshot.killSwitches.length === 0) {
    return null
  }

  const footer = (
    <div className="space-y-3">
      <div className="grid gap-2 text-xs text-current/80 sm:grid-cols-2">
        <p>App version: {snapshot.appVersion}</p>
        <p>Minimum supported version: {snapshot.minimumSupportedVersion ?? 'not set'}</p>
        <p>Release mode: {snapshot.releaseMode}</p>
        <p>Staged flags: {snapshot.stagedFlags.length > 0 ? snapshot.stagedFlags.join(', ') : 'none'}</p>
        <p className="sm:col-span-2">
          Kill switches: {snapshot.killSwitches.length > 0 ? snapshot.killSwitches.join(', ') : 'none'}
        </p>
      </div>
      <div className="flex flex-wrap gap-2">
        <button
          type="button"
          className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-current/40 px-4 py-2 text-sm font-medium hover:bg-black/5"
          onClick={onRefresh}
        >
          <RefreshCcw className="h-4 w-4" aria-hidden />
          Refresh after update
        </button>
        <a
          href={suiteHomeUrl}
          className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-current/40 px-4 py-2 text-sm font-medium hover:bg-black/5"
        >
          <ShieldCheck className="h-4 w-4" aria-hidden />
          Return to suite home
        </a>
      </div>
    </div>
  )

  return (
    <ApiErrorCallout
      className="mb-5"
      message={snapshot.message}
      title={snapshot.title}
      tone={snapshot.tone}
      retryLabel="Refresh app"
      onRetry={onRefresh}
      footer={footer}
      testId="fieldcompanion-release-safety-banner"
    />
  )
}
