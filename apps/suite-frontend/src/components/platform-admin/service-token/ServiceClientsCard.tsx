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
}

export function ServiceClientsCard({ clientsQuery, page, onPreviousPage, onNextPage }: Props) {
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
          <span className="text-xs text-slate-500">Page {page}</span>
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
        <p className="mt-2 text-sm text-slate-500">Loading clients…</p>
      ) : clientsQuery.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(clientsQuery.error, 'Failed to load service clients.')}
          onRetry={() => void clientsQuery.refetch()}
          retryLabel="Retry clients"
        />
      ) : clients.length === 0 ? (
        <p className="mt-2 text-sm text-slate-500" data-testid="service-token-clients-empty">
          No service clients registered.
        </p>
      ) : (
        <ul className="mt-3 divide-y divide-slate-800 text-sm" data-testid="service-token-clients-list">
          {clients.map((client) => (
            <li key={client.serviceClientId} className="py-2">
              <span className="font-medium text-slate-100">{client.displayName}</span>
              <span className="ml-2 font-mono text-xs text-slate-500">{client.clientKey}</span>
              <p className="text-xs text-slate-400">
                source {client.sourceProductKey} · allows {client.allowedProductKeys.join(', ')}
              </p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
