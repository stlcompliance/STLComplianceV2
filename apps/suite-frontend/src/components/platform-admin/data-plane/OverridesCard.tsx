import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { DataPlaneProfile, PagedResult } from '../../../api/types'

type Props = {
  isLoading: boolean
  isError: boolean
  error: unknown
  pagedProfiles: PagedResult<DataPlaneProfile> | undefined
  deletePending: boolean
  onDelete: (productKey: string) => void
  page: number
  onPreviousPage: () => void
  onNextPage: () => void
  onRetry: () => void
}

export function OverridesCard({
  isLoading,
  isError,
  error,
  pagedProfiles,
  deletePending,
  onDelete,
  page,
  onPreviousPage,
  onNextPage,
  onRetry,
}: Props) {
  const profiles = pagedProfiles?.items ?? []

  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-sm font-medium text-slate-200">Stored overrides</h3>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onPreviousPage}
            disabled={page <= 1 || isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-xs text-[var(--color-text-muted)]">Page {page}</span>
          <button
            type="button"
            onClick={onNextPage}
            disabled={!pagedProfiles?.hasNextPage || isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading overrides…</p>
      ) : isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(error, 'Failed to load data-plane overrides.')}
          onRetry={onRetry}
          retryLabel="Retry overrides"
        />
      ) : profiles.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="data-plane-overrides-empty">
          No overrides — all products default to hosted/trusted.
        </p>
      ) : (
        <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="data-plane-overrides-list">
          {profiles.map((profile) => (
            <li key={profile.profileId} className="flex flex-wrap items-center justify-between gap-2 py-2">
              <div>
                <span className="font-medium text-slate-100">{profile.productDisplayName}</span>
                <p className="text-xs text-slate-400">
                  {profile.deploymentMode} · {profile.trustStatus}
                  {profile.dataEndpointUrl ? ` · ${profile.dataEndpointUrl}` : ''}
                </p>
              </div>
              <button
                type="button"
                onClick={() => onDelete(profile.productKey)}
                disabled={deletePending}
                data-testid={`data-plane-reset-${profile.productKey}`}
                className="rounded-md bg-slate-700 px-3 py-1 text-xs font-medium text-white hover:bg-slate-600 disabled:opacity-50"
              >
                Reset to hosted default
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
