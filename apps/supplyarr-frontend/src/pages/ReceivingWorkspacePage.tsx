import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import { getReceivingReceipt } from '../api/client'
import { loadSession } from '../auth/sessionStorage'

export function ReceivingWorkspacePage() {
  const { receivingReceiptId } = useParams<{ receivingReceiptId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const receiptQuery = useQuery({
    queryKey: ['supplyarr-receiving', session?.accessToken, receivingReceiptId],
    queryFn: () => getReceivingReceipt(session!.accessToken, receivingReceiptId!),
    enabled: Boolean(session?.accessToken && receivingReceiptId),
  })

  if (!session) {
    return <p className="text-sm text-slate-400">Loading receiving receipt…</p>
  }

  if (!receivingReceiptId) {
    return <Navigate to="/" replace />
  }

  const receipt = receiptQuery.data

  return (
    <div className="mx-auto max-w-3xl space-y-6" data-testid="receiving-workspace">
      <PageHeader
        title="Receiving receipt"
        subtitle={receipt?.receiptKey ?? 'Loading receipt…'}
      />
      <p className="text-sm">
        <Link to="/" className="text-teal-300 hover:text-teal-200">
          ← Back to supply workspace
        </Link>
      </p>
      {receipt ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4 text-sm text-slate-200">
          <p>
            <span className="text-slate-400">Purchase order:</span> {receipt.purchaseOrderKey}
          </p>
          <p className="mt-2">
            <span className="text-slate-400">Status:</span> {receipt.status}
          </p>
        </section>
      ) : null}
    </div>
  )
}
