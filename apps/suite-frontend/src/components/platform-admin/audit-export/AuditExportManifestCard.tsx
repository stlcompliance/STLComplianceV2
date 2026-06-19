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
      className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
    >
      <h3 className="text-sm font-medium text-slate-200">
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
      <ul className="mt-2 list-inside list-disc text-sm text-slate-400">
        {(manifest?.sections ?? []).map((section) => (
          <li key={section.key}>
            <span className="font-mono text-slate-300">{section.fileName}</span>
            <span className="text-[var(--color-text-muted)]"> — {section.label}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}
