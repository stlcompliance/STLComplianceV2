import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import { getWorkOrder } from '../api/client'
import { loadSession } from '../auth/sessionStorage'

export function WorkOrderWorkspacePage() {
  const { workOrderId } = useParams<{ workOrderId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const workOrderQuery = useQuery({
    queryKey: ['maintainarr-work-order', session?.accessToken, workOrderId],
    queryFn: () => getWorkOrder(session!.accessToken, workOrderId!),
    enabled: Boolean(session?.accessToken && workOrderId),
  })

  if (!session) {
    return <p className="text-sm text-slate-400">Loading work order…</p>
  }

  if (!workOrderId) {
    return <Navigate to="/" replace />
  }

  const workOrder = workOrderQuery.data

  return (
    <div className="mx-auto max-w-3xl space-y-6" data-testid="work-order-workspace">
      <PageHeader
        title="Work order"
        subtitle={workOrder?.title ?? 'Loading work order…'}
      />
      <p className="text-sm">
        <Link to="/" className="text-teal-300 hover:text-teal-200">
          ← Back to maintenance workspace
        </Link>
      </p>
      {workOrder ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4 text-sm text-slate-200">
          <p>
            <span className="text-slate-400">Number:</span> {workOrder.workOrderNumber}
          </p>
          <p className="mt-2">
            <span className="text-slate-400">Asset:</span> {workOrder.assetTag} · {workOrder.assetName}
          </p>
          <p className="mt-2">
            <span className="text-slate-400">Status:</span> {workOrder.status}
          </p>
        </section>
      ) : null}
    </div>
  )
}
