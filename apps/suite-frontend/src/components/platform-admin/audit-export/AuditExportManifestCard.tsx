import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PlatformAuditPackageManifest } from '../../../api/types'

type Props = {
  manifest: PlatformAuditPackageManifest | undefined
  isError: boolean
  error: unknown
  onRetry: () => void
}

export function AuditExportManifestCard({ manifest, isError, error, onRetry }: Props) {
  return (
    <div
      data-testid="platform-audit-manifest-section"
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
    >
      <h3 className="text-sm font-medium text-[var(--color-text-primary)]">
        Package sections
        {manifest?.packageVersion ? (
          <span className="ml-2 font-mono text-xs text-[var(--color-text-muted)]">v{manifest.packageVersion}</span>
        ) : null}
      </h3>
      {isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(error, 'Failed to load package manifest.')}
          onRetry={onRetry}
          retryLabel="Retry manifest"
        />
      ) : null}
      <ul className="mt-2 list-inside list-disc text-sm text-[var(--color-text-muted)]">
        {(manifest?.sections ?? []).map((section) => (
          <li key={section.key}>
            <span className="font-mono text-[var(--color-text-secondary)]">{section.fileName}</span>
            <span className="text-[var(--color-text-muted)]"> — {section.label}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
