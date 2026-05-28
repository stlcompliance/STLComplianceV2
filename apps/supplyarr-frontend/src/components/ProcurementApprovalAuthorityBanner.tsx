import { useQuery } from '@tanstack/react-query'

import { getProcurementApprovalAuthority } from '../api/client'
import type { ProcurementApprovalAuthorityMirrorResponse } from '../api/types'

interface Props {
  accessToken: string
  canRead: boolean
}

function formatLimit(value: number | null | undefined): string {
  if (value == null) {
    return 'no limit'
  }

  return `$${value.toLocaleString(undefined, { maximumFractionDigits: 2 })}`
}

export function ProcurementApprovalAuthorityBanner({ accessToken, canRead }: Props) {
  const authorityQuery = useQuery({
    queryKey: ['supplyarr-procurement-approval-authority', accessToken],
    queryFn: () => getProcurementApprovalAuthority(accessToken),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  const authority: ProcurementApprovalAuthorityMirrorResponse | undefined = authorityQuery.data

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-4 lg:col-span-2"
      data-testid="procurement-approval-authority-banner"
    >
      <h2 className="text-sm font-semibold text-slate-50">StaffArr approval authority</h2>
      <p className="mt-1 text-xs text-slate-400">
        Purchase request submit/approve and purchase order issue are limited by effective StaffArr
        permissions (source: {authority?.authoritySource ?? 'loading…'}).
      </p>
      {authority && (
        <ul className="mt-3 grid gap-1 text-sm text-slate-300 md:grid-cols-3">
          <li>
            Submit PR:{' '}
            <span className={authority.canSubmitPurchaseRequests ? 'text-emerald-300' : 'text-rose-300'}>
              {authority.canSubmitPurchaseRequests ? 'allowed' : 'denied'}
            </span>
            {authority.canSubmitPurchaseRequests && (
              <span className="text-slate-500"> ({formatLimit(authority.maxSubmitAmount)})</span>
            )}
          </li>
          <li>
            Approve PR:{' '}
            <span className={authority.canApprovePurchaseRequests ? 'text-emerald-300' : 'text-rose-300'}>
              {authority.canApprovePurchaseRequests ? 'allowed' : 'denied'}
            </span>
            {authority.canApprovePurchaseRequests && (
              <span className="text-slate-500"> ({formatLimit(authority.maxApproveAmount)})</span>
            )}
          </li>
          <li>
            Issue PO:{' '}
            <span className={authority.canIssuePurchaseOrders ? 'text-emerald-300' : 'text-rose-300'}>
              {authority.canIssuePurchaseOrders ? 'allowed' : 'denied'}
            </span>
            {authority.canIssuePurchaseOrders && (
              <span className="text-slate-500"> ({formatLimit(authority.maxIssueAmount)})</span>
            )}
          </li>
        </ul>
      )}
      {authorityQuery.isError && (
        <p className="mt-2 text-sm text-amber-300">
          Could not load StaffArr authority mirror. Denied actions will return a procurement approval
          authority error from the API.
        </p>
      )}
    </section>
  )
}
