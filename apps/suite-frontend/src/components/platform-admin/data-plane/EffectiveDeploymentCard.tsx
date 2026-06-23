import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { EffectiveDataPlaneProfile } from '../../../api/types'

type Props = {
  isLoading: boolean
  isError: boolean
  error: unknown
  profiles: EffectiveDataPlaneProfile[]
  onRetry: () => void
}

export function EffectiveDeploymentCard({ isLoading, isError, error, profiles, onRetry }: Props) {
  return (
    <div
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
      data-testid="data-plane-effective-section"
    >
      <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Effective deployment map</h3>
      {isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading effective profiles…</p>
      ) : isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(error, 'Failed to load effective data-plane map.')}
          onRetry={onRetry}
          retryLabel="Retry effective map"
        />
      ) : (
        <ul className="mt-3 divide-y divide-[var(--color-border-subtle)] text-sm">
          {profiles.map((profile) => (
            <li key={profile.productKey} className="py-2">
              <span className="font-medium text-[var(--color-text-primary)]">{profile.productDisplayName}</span>
              <span className="ml-2 font-mono text-xs text-[var(--color-accent)]">{profile.deploymentMode}</span>
              <span className="ml-2 text-xs text-[var(--color-text-muted)]">{profile.trustStatus}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
