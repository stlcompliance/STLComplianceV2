import { Navigate, useParams, useSearchParams } from 'react-router-dom'

export function WorkOrderWorkspacePage() {
  const { workOrderId } = useParams<{ workOrderId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  if (!workOrderId) {
    return <Navigate to="/" replace />
  }

  return <Navigate to={`/work-orders/details?workOrderId=${encodeURIComponent(workOrderId)}`} replace />
}
