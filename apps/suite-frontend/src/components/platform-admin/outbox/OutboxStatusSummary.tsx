import type { PlatformOutboxPublisherStatusResponse } from '../../../api/types'

type Props = {
  status: PlatformOutboxPublisherStatusResponse | undefined
}

export function OutboxStatusSummary({ status }: Props) {
  if (!status) {
    return null
  }

  return (
    <dl className="mt-4 grid gap-2 text-sm text-slate-300 sm:grid-cols-3">
      <div>
        <dt className="text-slate-500">Pending</dt>
        <dd className="font-medium tabular-nums text-white">{status.pendingCount}</dd>
      </div>
      <div>
        <dt className="text-slate-500">Dead letter</dt>
        <dd className="font-medium tabular-nums text-white">{status.deadLetterCount}</dd>
      </div>
      <div>
        <dt className="text-slate-500">Publisher</dt>
        <dd className="font-medium text-white">{status.isEnabled ? 'Enabled' : 'Disabled'}</dd>
      </div>
    </dl>
  )
}
