import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import {
  approveProcurementExceptionWaive,
  cancelProcurementException,
  closeProcurementException,
  createSubjectProcurementException,
  getRfqs,
  listProcurementExceptions,
  listSubjectProcurementExceptions,
  rejectProcurementExceptionWaive,
  requestProcurementExceptionWaive,
  resolveProcurementException,
  startProcurementExceptionInvestigation,
} from '../api/client'
import type { PurchaseOrderResponse, PurchaseRequestResponse } from '../api/types'

const CATEGORIES = [
  'approval_delay',
  'vendor_issue',
  'budget_override',
  'policy_violation',
  'pricing_variance',
  'other',
] as const

const SUBJECT_TYPES = [
  { value: 'purchase_request', label: 'Purchase request' },
  { value: 'purchase_order', label: 'Purchase order' },
  { value: 'rfq', label: 'RFQ' },
] as const

type SubjectType = (typeof SUBJECT_TYPES)[number]['value']

function statusClass(status: string): string {
  switch (status) {
    case 'open':
      return 'bg-amber-500/20 text-amber-200'
    case 'investigating':
      return 'bg-sky-500/20 text-sky-200'
    case 'waive_pending':
      return 'bg-violet-500/20 text-violet-200'
    case 'waived':
      return 'bg-indigo-500/20 text-indigo-200'
    case 'resolved':
      return 'bg-emerald-500/20 text-emerald-200'
    case 'closed':
      return 'bg-slate-500/20 text-slate-300'
    default:
      return 'bg-rose-500/20 text-rose-200'
  }
}

interface ProcurementExceptionsPanelProps {
  accessToken: string
  canManage: boolean
  canApprove: boolean
  purchaseRequests: PurchaseRequestResponse[]
  purchaseOrders: PurchaseOrderResponse[]
}

export function ProcurementExceptionsPanel({
  accessToken,
  canManage,
  canApprove,
  purchaseRequests,
  purchaseOrders,
}: ProcurementExceptionsPanelProps) {
  const queryClient = useQueryClient()
  const [subjectType, setSubjectType] = useState<SubjectType>('purchase_request')
  const [subjectId, setSubjectId] = useState('')
  const [exceptionKey, setExceptionKey] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState<(typeof CATEGORIES)[number]>('policy_violation')
  const [waiveJustification, setWaiveJustification] = useState('')
  const [resolutionNotes, setResolutionNotes] = useState('')

  const activeQuery = useQuery({
    queryKey: ['supplyarr-procurement-exceptions-active', accessToken],
    queryFn: () =>
      listProcurementExceptions(accessToken, { status: 'open' }).then(async (open) => {
        const investigating = await listProcurementExceptions(accessToken, {
          status: 'investigating',
        })
        const waivePending = await listProcurementExceptions(accessToken, {
          status: 'waive_pending',
        })
        return [...open, ...investigating, ...waivePending]
      }),
    enabled: canManage,
  })

  const rfqsQuery = useQuery({
    queryKey: ['supplyarr-rfqs-for-exceptions', accessToken],
    queryFn: () => getRfqs(accessToken),
    enabled: canManage && subjectType === 'rfq',
  })

  const subjectOptions = useMemo(() => {
    if (subjectType === 'purchase_request') {
      return purchaseRequests.map((pr) => ({
        id: pr.purchaseRequestId,
        label: `${pr.requestKey} — ${pr.title}`,
      }))
    }
    if (subjectType === 'purchase_order') {
      return purchaseOrders.map((po) => ({
        id: po.purchaseOrderId,
        label: po.orderKey,
      }))
    }
    return (rfqsQuery.data ?? []).map((rfq) => ({
      id: rfq.rfqId,
      label: rfq.rfqKey,
    }))
  }, [subjectType, purchaseRequests, purchaseOrders, rfqsQuery.data])

  const subjectExceptionsQuery = useQuery({
    queryKey: ['supplyarr-subject-procurement-exceptions', accessToken, subjectType, subjectId],
    queryFn: () => listSubjectProcurementExceptions(accessToken, subjectType, subjectId),
    enabled: Boolean(subjectId),
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-procurement-exceptions-active', accessToken],
    })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-subject-procurement-exceptions', accessToken, subjectType, subjectId],
    })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createSubjectProcurementException(accessToken, subjectType, subjectId, {
        exceptionKey,
        exceptionCategory: category,
        title,
        description,
      }),
    onSuccess: () => {
      setExceptionKey('')
      setTitle('')
      setDescription('')
      invalidate()
    },
  })

  const workflowMutation = useMutation({
    mutationFn: async (action: {
      type:
        | 'investigate'
        | 'resolve'
        | 'request_waive'
        | 'approve_waive'
        | 'reject_waive'
        | 'close'
        | 'cancel'
      exceptionId: string
    }) => {
      const { type, exceptionId } = action
      if (type === 'investigate') {
        return startProcurementExceptionInvestigation(accessToken, exceptionId)
      }
      if (type === 'resolve') {
        return resolveProcurementException(accessToken, exceptionId, {
          resolutionNotes: resolutionNotes || 'Resolved from purchasing workspace',
        })
      }
      if (type === 'request_waive') {
        return requestProcurementExceptionWaive(accessToken, exceptionId, {
          waiveJustification:
            waiveJustification || 'Policy exception approved by operations leadership.',
        })
      }
      if (type === 'approve_waive') {
        return approveProcurementExceptionWaive(accessToken, exceptionId)
      }
      if (type === 'reject_waive') {
        return rejectProcurementExceptionWaive(accessToken, exceptionId, {
          reason: 'Waive not justified for this procurement record.',
        })
      }
      if (type === 'close') {
        return closeProcurementException(accessToken, exceptionId)
      }
      return cancelProcurementException(accessToken, exceptionId, {
        reason: 'Cancelled from purchasing workspace',
      })
    },
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="procurement-exceptions-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Procurement exceptions</h2>
      <p className="mt-1 text-sm text-slate-400">
        Structured exceptions on purchase requests, orders, and RFQs — distinct from receiving
        exceptions and supplier incidents. Waive requires approver sign-off.
      </p>

      {activeQuery.data && (
        <p className="mt-3 text-sm text-slate-500">
          {activeQuery.data.length} active exception{activeQuery.data.length === 1 ? '' : 's'}{' '}
          tenant-wide
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <label className="block text-sm text-slate-400">
          Subject type
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={subjectType}
            onChange={(event) => {
              setSubjectType(event.target.value as SubjectType)
              setSubjectId('')
            }}
          >
            {SUBJECT_TYPES.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>

        <label className="block text-sm text-slate-400">
          Subject record
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={subjectId}
            onChange={(event) => setSubjectId(event.target.value)}
          >
            <option value="">Select record…</option>
            {subjectOptions.map((option) => (
              <option key={option.id} value={option.id}>
                {option.label}
              </option>
            ))}
          </select>
        </label>

        <label className="block text-sm text-slate-400">
          Exception key
          <input
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={exceptionKey}
            onChange={(event) => setExceptionKey(event.target.value)}
            placeholder="PEX-001"
          />
        </label>

        <label className="block text-sm text-slate-400">
          Category
          <select
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={category}
            onChange={(event) => setCategory(event.target.value as (typeof CATEGORIES)[number])}
          >
            {CATEGORIES.map((value) => (
              <option key={value} value={value}>
                {value.replace('_', ' ')}
              </option>
            ))}
          </select>
        </label>

        <label className="block text-sm text-slate-400 md:col-span-2">
          Title
          <input
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
          />
        </label>

        <label className="block text-sm text-slate-400 md:col-span-2">
          Description
          <textarea
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            value={description}
            onChange={(event) => setDescription(event.target.value)}
          />
        </label>

        <label className="block text-sm text-slate-400 md:col-span-2">
          Waive justification (for waive request)
          <textarea
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            value={waiveJustification}
            onChange={(event) => setWaiveJustification(event.target.value)}
          />
        </label>

        <label className="block text-sm text-slate-400 md:col-span-2">
          Resolution notes
          <input
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={resolutionNotes}
            onChange={(event) => setResolutionNotes(event.target.value)}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded bg-sky-600 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
        disabled={
          !subjectId ||
          !exceptionKey ||
          title.length < 3 ||
          createMutation.isPending
        }
        onClick={() => createMutation.mutate()}
      >
        Open exception
      </button>

      <div className="mt-6 space-y-3">
        <h3 className="text-sm font-medium text-slate-300">Exceptions on selected subject</h3>
        {(subjectExceptionsQuery.data ?? []).length === 0 && (
          <p className="text-sm text-slate-500">No exceptions for this record.</p>
        )}
        {(subjectExceptionsQuery.data ?? []).map((exception) => (
          <div
            key={exception.exceptionId}
            className="rounded-lg border border-slate-800 bg-slate-950/60 p-3"
          >
            <div className="flex flex-wrap items-center gap-2">
              <span className="font-mono text-sm text-slate-200">{exception.exceptionKey}</span>
              <span className={`rounded px-2 py-0.5 text-xs ${statusClass(exception.status)}`}>
                {exception.status}
              </span>
              <span className="text-xs text-slate-500">{exception.exceptionCategory}</span>
            </div>
            <p className="mt-1 text-sm text-slate-300">{exception.title}</p>
            <div className="mt-2 flex flex-wrap gap-2">
              {exception.status === 'open' && (
                <button
                  type="button"
                  className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                  onClick={() =>
                    workflowMutation.mutate({
                      type: 'investigate',
                      exceptionId: exception.exceptionId,
                    })
                  }
                >
                  Investigate
                </button>
              )}
              {exception.status === 'investigating' && (
                <>
                  <button
                    type="button"
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                    onClick={() =>
                      workflowMutation.mutate({
                        type: 'resolve',
                        exceptionId: exception.exceptionId,
                      })
                    }
                  >
                    Resolve
                  </button>
                  <button
                    type="button"
                    className="rounded border border-violet-600 px-2 py-0.5 text-xs text-violet-200"
                    onClick={() =>
                      workflowMutation.mutate({
                        type: 'request_waive',
                        exceptionId: exception.exceptionId,
                      })
                    }
                  >
                    Request waive
                  </button>
                  <button
                    type="button"
                    className="rounded border border-rose-700 px-2 py-0.5 text-xs text-rose-200"
                    onClick={() =>
                      workflowMutation.mutate({
                        type: 'cancel',
                        exceptionId: exception.exceptionId,
                      })
                    }
                  >
                    Cancel
                  </button>
                </>
              )}
              {exception.status === 'waive_pending' && canApprove && (
                <>
                  <button
                    type="button"
                    className="rounded border border-emerald-600 px-2 py-0.5 text-xs text-emerald-200"
                    onClick={() =>
                      workflowMutation.mutate({
                        type: 'approve_waive',
                        exceptionId: exception.exceptionId,
                      })
                    }
                  >
                    Approve waive
                  </button>
                  <button
                    type="button"
                    className="rounded border border-amber-600 px-2 py-0.5 text-xs text-amber-200"
                    onClick={() =>
                      workflowMutation.mutate({
                        type: 'reject_waive',
                        exceptionId: exception.exceptionId,
                      })
                    }
                  >
                    Reject waive
                  </button>
                </>
              )}
              {(exception.status === 'resolved' || exception.status === 'waived') && (
                <button
                  type="button"
                  className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                  onClick={() =>
                    workflowMutation.mutate({
                      type: 'close',
                      exceptionId: exception.exceptionId,
                    })
                  }
                >
                  Close
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
