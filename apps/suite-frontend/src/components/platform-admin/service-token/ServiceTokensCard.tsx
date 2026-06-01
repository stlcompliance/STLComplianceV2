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
    <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-sm font-medium text-slate-200">Issued tokens</h3>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onPreviousPage}
            disabled={page <= 1 || tokensQuery.isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-xs text-slate-500">Page {page}</span>
          <button
            type="button"
            onClick={onNextPage}
            disabled={!tokensQuery.data?.hasNextPage || tokensQuery.isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {tokensQuery.isLoading ? (
        <p className="mt-2 text-sm text-slate-500">Loading tokens…</p>
      ) : tokensQuery.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(tokensQuery.error, 'Failed to load service tokens.')}
          onRetry={() => void tokensQuery.refetch()}
          retryLabel="Retry tokens"
        />
      ) : tokens.length === 0 ? (
        <p className="mt-2 text-sm text-slate-500" data-testid="service-token-list-empty">
          No service tokens issued yet.
        </p>
      ) : (
        <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="service-token-list">
          {tokens.map((token) => (
            <li key={token.tokenId} className="flex flex-wrap items-center justify-between gap-2 py-2">
              <div>
                <span className="font-mono text-xs text-teal-300">{token.clientKey}</span>
                <p className="text-xs text-slate-400">
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
                  className="rounded-md bg-rose-700 px-3 py-1 text-xs font-medium text-white hover:bg-rose-600 disabled:opacity-50"
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
