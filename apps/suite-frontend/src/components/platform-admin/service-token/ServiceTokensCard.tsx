import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PagedResult, ServiceTokenSummary } from '../../../api/types'

type Props = {
  tokensQuery: {
    isLoading: boolean
    isError: boolean
    error: unknown
    data: PagedResult<ServiceTokenSummary> | undefined
    refetch: () => Promise<unknown>
  }
  revokePending: boolean
  onRevoke: (tokenId: string) => void
  page: number
  onPreviousPage: () => void
  onNextPage: () => void
}

export function ServiceTokensCard({
  tokensQuery,
  revokePending,
  onRevoke,
  page,
  onPreviousPage,
  onNextPage,
}: Props) {
  const tokens = tokensQuery.data?.items ?? []

  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Issued tokens</h3>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onPreviousPage}
            disabled={page <= 1 || tokensQuery.isLoading}
            className="rounded-md border border-[var(--color-border-subtle)] px-2 py-1 text-xs text-[var(--color-text-secondary)] disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-xs text-[var(--color-text-muted)]">Page {page}</span>
          <button
            type="button"
            onClick={onNextPage}
            disabled={!tokensQuery.data?.hasNextPage || tokensQuery.isLoading}
            className="rounded-md border border-[var(--color-border-subtle)] px-2 py-1 text-xs text-[var(--color-text-secondary)] disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {tokensQuery.isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading tokens…</p>
      ) : tokensQuery.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(tokensQuery.error, 'Failed to load service tokens.')}
          onRetry={() => void tokensQuery.refetch()}
          retryLabel="Retry tokens"
        />
      ) : tokens.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="service-token-list-empty">
          No service tokens issued yet.
        </p>
      ) : (
        <ul className="mt-3 divide-y divide-[var(--color-border-subtle)] text-sm" data-testid="service-token-list">
          {tokens.map((token) => (
            <li key={token.tokenId} className="flex flex-wrap items-center justify-between gap-2 py-2">
              <div>
                <span className="font-mono text-xs text-[var(--color-accent)]">{token.clientKey}</span>
                <p className="text-xs text-[var(--color-text-secondary)]">
                  {token.revokedAt ? 'Revoked' : 'Active'} · expires {new Date(token.expiresAt).toLocaleString()}
                  {token.tenantId ? ` · tenant ${token.tenantId}` : ''}
                </p>
              </div>
              {!token.revokedAt ? (
                <button
                  type="button"
                  onClick={() => onRevoke(token.tokenId)}
                  disabled={revokePending}
                  data-testid={`service-token-revoke-${token.tokenId}`}
                  className="rounded-md border border-[var(--color-destructive-border)] bg-[var(--color-destructive-bg)] px-3 py-1 text-xs font-medium text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                >
                  Revoke
                </button>
              ) : null}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
