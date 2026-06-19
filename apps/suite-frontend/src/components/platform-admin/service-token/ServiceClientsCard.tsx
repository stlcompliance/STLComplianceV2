import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PagedResult, ServiceClientSummary } from '../../../api/types'

type Props = {
  clientsQuery: {
    isLoading: boolean
    isError: boolean
    error: unknown
    data: PagedResult<ServiceClientSummary> | undefined
    refetch: () => Promise<unknown>
  }
  page: number
  onPreviousPage: () => void
  onNextPage: () => void
  onRotate: (serviceClientId: string) => void
  onRevoke: (serviceClientId: string) => void
  onSelectClient: (serviceClientId: string) => void
  selectedServiceClientId: string
  actionPending: boolean
}

export function ServiceClientsCard({
  clientsQuery,
  page,
  onPreviousPage,
  onNextPage,
  onRotate,
  onRevoke,
  onSelectClient,
  selectedServiceClientId,
  actionPending,
}: Props) {
  const clients = clientsQuery.data?.items ?? []

  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-sm font-medium text-slate-200">Registered clients</h3>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onPreviousPage}
            disabled={page <= 1 || clientsQuery.isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Previous
          </button>
          <span className="text-xs text-[var(--color-text-muted)]">Page {page}</span>
          <button
            type="button"
            onClick={onNextPage}
            disabled={!clientsQuery.data?.hasNextPage || clientsQuery.isLoading}
            className="rounded-md border border-slate-700 px-2 py-1 text-xs text-slate-300 disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>
      {clientsQuery.isLoading ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading clients…</p>
      ) : clientsQuery.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(clientsQuery.error, 'Failed to load service clients.')}
          onRetry={() => void clientsQuery.refetch()}
          retryLabel="Retry clients"
        />
      ) : clients.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="service-token-clients-empty">
          No service clients registered.
        </p>
      ) : (
        <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="service-token-clients-list">
          {clients.map((client) => {
            const allowedProductKeys = client.allowedProductKeys ?? []
            const allowedTenantIds = client.allowedTenantIds ?? []
            return (
              <li
                key={client.serviceClientId}
                className={`py-2 ${selectedServiceClientId === client.serviceClientId ? 'rounded-md bg-slate-900/70 px-2' : ''}`}
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <span className="font-medium text-slate-100">{client.displayName}</span>
                    <span className="ml-2 font-mono text-xs text-[var(--color-text-muted)]">{client.clientKey}</span>
                    <p className="text-xs text-slate-400">
                      source {client.sourceProductKey} · allows {allowedProductKeys.join(', ')}
                    </p>
                    <p className="text-xs text-[var(--color-text-muted)]">
                      {client.isActive ? 'Active' : 'Inactive'}
                      {allowedTenantIds.length ? ` · tenants ${allowedTenantIds.length}` : ' · all tenants'}
                      {client.lastUsedAt ? ` · last used ${new Date(client.lastUsedAt).toLocaleString()}` : ' · never used'}
                      {typeof client.failedAuthenticationAttempts === 'number'
                        ? ` · failed auth ${client.failedAuthenticationAttempts}`
                        : ''}
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => onSelectClient(client.serviceClientId)}
                      className="rounded-md border border-slate-700 px-3 py-1 text-xs font-medium text-slate-200 hover:bg-slate-800/80"
                    >
                      Manage
                    </button>
                    <button
                      type="button"
                      onClick={() => onRotate(client.serviceClientId)}
                      disabled={actionPending || !client.isActive}
                      className="rounded-md border border-amber-700 px-3 py-1 text-xs font-medium text-amber-200 hover:bg-amber-950/40 disabled:opacity-50"
                    >
                      Rotate
                    </button>
                    <button
                      type="button"
                      onClick={() => onRevoke(client.serviceClientId)}
                      disabled={actionPending || !client.isActive}
                      className="rounded-md border border-red-700 px-3 py-1 text-xs font-medium text-red-200 hover:bg-red-950/40 disabled:opacity-50"
                    >
                      Revoke
                    </button>
                  </div>
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </div>
  )
}
