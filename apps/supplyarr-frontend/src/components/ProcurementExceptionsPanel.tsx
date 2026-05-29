import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { ControlledSelect } from '@stl/shared-ui'

import {
  approveProcurementExceptionWaive,
  assignProcurementException,
  cancelProcurementException,
  reopenProcurementException,
  closeProcurementException,
  createSubjectProcurementException,
  getRfqs,
  linkProcurementExceptionActions,
  listProcurementExceptionResolutionTemplates,
  listProcurementExceptions,
  listSubjectProcurementExceptions,
  rejectProcurementExceptionWaive,
  requestProcurementExceptionWaive,
  resolveProcurementException,
  startProcurementExceptionInvestigation,
} from '../api/client'
import type { PurchaseOrderResponse, PurchaseRequestResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

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

const CATEGORY_OPTIONS = CATEGORIES.map((value) => ({
  value,
  label: value.replace(/_/g, ' '),
}))

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
    case 'cancelled':
      return 'bg-rose-500/20 text-rose-200'
    default:
      return 'bg-rose-500/20 text-rose-200'
  }
}

function formatSlaDue(slaDueAt: string | null): string {
  if (!slaDueAt) {
    return 'No SLA'
  }
  return new Date(slaDueAt).toLocaleString()
}

interface ProcurementExceptionsPanelProps {
  accessToken: string
  currentUserId: string
  canManage: boolean
  canApprove: boolean
  purchaseRequests: PurchaseRequestResponse[]
  purchaseOrders: PurchaseOrderResponse[]
}

export function ProcurementExceptionsPanel({
  accessToken,
  currentUserId,
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
  const [assignOnCreate, setAssignOnCreate] = useState(true)
  const [waiveJustification, setWaiveJustification] = useState('')
  const [cancelReason, setCancelReason] = useState('')
  const [reopenReason, setReopenReason] = useState('')
  const [resolutionNotes, setResolutionNotes] = useState('')
  const [resolutionTemplateKey, setResolutionTemplateKey] = useState('')
  const [selectedExceptionId, setSelectedExceptionId] = useState('')
  const [linkedPrId, setLinkedPrId] = useState('')
  const [linkedPoId, setLinkedPoId] = useState('')

  const templatesQuery = useQuery({
    queryKey: ['supplyarr-procurement-exception-templates', accessToken],
    queryFn: () => listProcurementExceptionResolutionTemplates(accessToken),
    enabled: canManage,
  })

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

  const overdueQuery = useQuery({
    queryKey: ['supplyarr-procurement-exceptions-overdue', accessToken],
    queryFn: () => listProcurementExceptions(accessToken, { overdueOnly: true }),
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

  const selectedException = useMemo(() => {
    const pool = [
      ...(subjectExceptionsQuery.data ?? []),
      ...(activeQuery.data ?? []),
    ]
    return pool.find((x) => x.exceptionId === selectedExceptionId) ?? null
  }, [selectedExceptionId, subjectExceptionsQuery.data, activeQuery.data])

  const existingExceptionKeys = useMemo(() => {
    const pool = [
      ...(subjectExceptionsQuery.data ?? []),
      ...(activeQuery.data ?? []),
    ]
    return pool.map((exception) => exception.exceptionKey)
  }, [subjectExceptionsQuery.data, activeQuery.data])

  const templateOptions = useMemo(
    () =>
      (templatesQuery.data ?? []).map((template) => ({
        value: template.templateKey,
        label: template.label,
      })),
    [templatesQuery.data],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-procurement-exceptions-active', accessToken],
    })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-procurement-exceptions-overdue', accessToken],
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
        assignedToUserId: assignOnCreate ? currentUserId : null,
      }),
    onSuccess: (created) => {
      setExceptionKey('')
      setTitle('')
      setDescription('')
      setSelectedExceptionId(created.exceptionId)
      invalidate()
    },
  })

  const assignMutation = useMutation({
    mutationFn: (exceptionId: string) =>
      assignProcurementException(accessToken, exceptionId, {
        assignedToUserId: currentUserId,
      }),
    onSuccess: invalidate,
  })

  const linkMutation = useMutation({
    mutationFn: (exceptionId: string) =>
      linkProcurementExceptionActions(accessToken, exceptionId, {
        linkedPurchaseRequestId: linkedPrId || null,
        linkedPurchaseOrderId: linkedPoId || null,
      }),
    onSuccess: invalidate,
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
        | 'reopen'
      exceptionId: string
    }) => {
      const { type, exceptionId } = action
      if (type === 'investigate') {
        return startProcurementExceptionInvestigation(accessToken, exceptionId)
      }
      if (type === 'resolve') {
        return resolveProcurementException(accessToken, exceptionId, {
          resolutionNotes: resolutionNotes || 'Resolved from purchasing workspace',
          resolutionTemplateKey: resolutionTemplateKey || null,
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
      if (type === 'reopen') {
        return reopenProcurementException(accessToken, exceptionId, {
          reason:
            reopenReason ||
            'Reopened from purchasing workspace after mistaken cancellation.',
        })
      }
      return cancelProcurementException(accessToken, exceptionId, {
        reason: cancelReason || 'Cancelled from purchasing workspace',
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
        Resolver assignment, SLA due dates, resolution templates, and linked PR/PO follow-up
        actions for purchase requests, orders, and RFQs.
      </p>

      {activeQuery.data && (
        <p className="mt-3 text-sm text-slate-500" data-testid="procurement-exceptions-active-count">
          {activeQuery.data.length} active exception{activeQuery.data.length === 1 ? '' : 's'}{' '}
          tenant-wide
          {(overdueQuery.data?.length ?? 0) > 0 ? (
            <span className="ml-2 text-rose-300">
              · {overdueQuery.data!.length} past SLA
            </span>
          ) : null}
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <ControlledSelect
          id="procurement-exception-subject-type"
          label="Subject type"
          value={subjectType}
          onChange={(value) => {
            setSubjectType(value as SubjectType)
            setSubjectId('')
          }}
          options={SUBJECT_TYPES.map((option) => ({ value: option.value, label: option.label }))}
          emptyLabel="Select type…"
        />

        <ControlledSelect
          id="procurement-exception-subject-record"
          label="Subject record"
          value={subjectId}
          onChange={setSubjectId}
          options={subjectOptions.map((option) => ({ value: option.id, label: option.label }))}
          emptyLabel="Select record…"
          testId="procurement-exception-subject-record"
        />

        <label htmlFor="procurement-exception-title" className="block text-sm text-slate-400 md:col-span-2">
          Exception title
          <input
            id="procurement-exception-title"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
          />
        </label>

        <div className="md:col-span-2">
          <GeneratedKeyFieldGroup
            sourceLabel={title}
            existingKeys={existingExceptionKeys}
            onKeyChange={setExceptionKey}
            label="Exception key"
          />
        </div>

        <ControlledSelect
          id="procurement-exception-category"
          label="Exception category"
          value={category}
          onChange={(value) => setCategory(value as (typeof CATEGORIES)[number])}
          options={CATEGORY_OPTIONS}
          testId="procurement-exception-category"
        />

        <label htmlFor="procurement-exception-assign-on-create" className="flex items-center gap-2 text-sm text-slate-400 md:col-span-2">
          <input
            id="procurement-exception-assign-on-create"
            type="checkbox"
            checked={assignOnCreate}
            onChange={(event) => setAssignOnCreate(event.target.checked)}
          />
          Assign to me on create (category-based SLA applied automatically)
        </label>

        <label htmlFor="procurement-exception-description" className="block text-sm text-slate-400 md:col-span-2">
          Exception description
          <textarea
            id="procurement-exception-description"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            value={description}
            onChange={(event) => setDescription(event.target.value)}
          />
        </label>

        <ControlledSelect
          id="procurement-exception-resolution-template"
          label="Resolution template"
          value={resolutionTemplateKey}
          onChange={setResolutionTemplateKey}
          options={templateOptions}
          emptyLabel="Custom resolution notes"
          testId="procurement-exception-resolution-template"
        />

        <label htmlFor="procurement-exception-resolution-notes" className="block text-sm text-slate-400 md:col-span-2">
          Resolution notes
          <textarea
            id="procurement-exception-resolution-notes"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            value={resolutionNotes}
            onChange={(event) => setResolutionNotes(event.target.value)}
          />
        </label>

        <label htmlFor="procurement-exception-waive-justification" className="block text-sm text-slate-400 md:col-span-2">
          Waive justification (for waive request)
          <textarea
            id="procurement-exception-waive-justification"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            data-testid="procurement-exception-waive-justification"
            value={waiveJustification}
            onChange={(event) => setWaiveJustification(event.target.value)}
          />
        </label>

        <label htmlFor="procurement-exception-cancel-reason" className="block text-sm text-slate-400 md:col-span-2">
          Cancel reason (for cancel action)
          <textarea
            id="procurement-exception-cancel-reason"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            data-testid="procurement-exception-cancel-reason"
            value={cancelReason}
            onChange={(event) => setCancelReason(event.target.value)}
          />
        </label>

        <label htmlFor="procurement-exception-reopen-reason" className="block text-sm text-slate-400 md:col-span-2">
          Reopen reason (for reopen after cancel)
          <textarea
            id="procurement-exception-reopen-reason"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
            rows={2}
            data-testid="procurement-exception-reopen-reason"
            value={reopenReason}
            onChange={(event) => setReopenReason(event.target.value)}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded bg-sky-600 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
        disabled={!subjectId || !exceptionKey || title.length < 3 || createMutation.isPending}
        onClick={() => createMutation.mutate()}
      >
        Open exception
      </button>

      {selectedException && (
        <div
          className="mt-6 rounded-lg border border-slate-600 bg-slate-950/80 p-4"
          data-testid="procurement-exception-detail"
        >
          <h3 className="text-sm font-medium text-slate-200">
            Selected: {selectedException.exceptionKey}
          </h3>
          <p className="mt-1 text-xs text-slate-500">
            SLA due {formatSlaDue(selectedException.slaDueAt)}
            {selectedException.isSlaBreached ? (
              <span className="ml-2 text-rose-300">· past due</span>
            ) : null}
            {selectedException.assignedToUserId ? (
              <span className="ml-2">· resolver {selectedException.assignedToUserId}</span>
            ) : (
              <span className="ml-2">· unassigned</span>
            )}
          </p>

          <div className="mt-3 grid gap-3 md:grid-cols-2">
            <label htmlFor="procurement-exception-link-pr" className="block text-xs text-slate-400">
              Link follow-up PR
              <select
                id="procurement-exception-link-pr"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                data-testid="procurement-exception-link-pr"
                value={linkedPrId}
                onChange={(event) => setLinkedPrId(event.target.value)}
              >
                <option value="">None</option>
                {purchaseRequests.map((pr) => (
                  <option key={pr.purchaseRequestId} value={pr.purchaseRequestId}>
                    {pr.requestKey}
                  </option>
                ))}
              </select>
            </label>
            <label htmlFor="procurement-exception-link-po" className="block text-xs text-slate-400">
              Link follow-up PO
              <select
                id="procurement-exception-link-po"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                data-testid="procurement-exception-link-po"
                value={linkedPoId}
                onChange={(event) => setLinkedPoId(event.target.value)}
              >
                <option value="">None</option>
                {purchaseOrders.map((po) => (
                  <option key={po.purchaseOrderId} value={po.purchaseOrderId}>
                    {po.orderKey}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <div className="mt-3 flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
              data-testid={`procurement-exception-assign-${selectedException.exceptionId}`}
              disabled={assignMutation.isPending}
              onClick={() => assignMutation.mutate(selectedException.exceptionId)}
            >
              Assign to me
            </button>
            <button
              type="button"
              className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
              data-testid={`procurement-exception-save-links-${selectedException.exceptionId}`}
              disabled={linkMutation.isPending}
              onClick={() => linkMutation.mutate(selectedException.exceptionId)}
            >
              Save PR/PO links
            </button>
          </div>

          {(selectedException.linkedPurchaseRequestKey ||
            selectedException.linkedPurchaseOrderKey) && (
            <p
              className="mt-2 text-xs text-emerald-300"
              data-testid="procurement-exception-linked-actions"
            >
              Linked actions:{' '}
              {selectedException.linkedPurchaseRequestKey
                ? `PR ${selectedException.linkedPurchaseRequestKey}`
                : ''}
              {selectedException.linkedPurchaseRequestKey &&
              selectedException.linkedPurchaseOrderKey
                ? ' · '
                : ''}
              {selectedException.linkedPurchaseOrderKey
                ? `PO ${selectedException.linkedPurchaseOrderKey}`
                : ''}
            </p>
          )}
        </div>
      )}

      <div className="mt-6 space-y-3">
        <h3 className="text-sm font-medium text-slate-300">Exceptions on selected subject</h3>
        {(subjectExceptionsQuery.data ?? []).length === 0 && (
          <p className="text-sm text-slate-500">No exceptions for this record.</p>
        )}
        {(subjectExceptionsQuery.data ?? []).map((exception) => (
          <div
            key={exception.exceptionId}
            className={`rounded-lg border p-3 ${
              selectedExceptionId === exception.exceptionId
                ? 'border-sky-600 bg-slate-950'
                : 'border-slate-800 bg-slate-950/60'
            }`}
            data-testid={`procurement-exception-row-${exception.exceptionId}`}
          >
            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                className="font-mono text-sm text-slate-200 underline-offset-2 hover:underline"
                data-testid={`procurement-exception-key-${exception.exceptionId}`}
                onClick={() => {
                  setSelectedExceptionId(exception.exceptionId)
                  setLinkedPrId(exception.linkedPurchaseRequestId ?? '')
                  setLinkedPoId(exception.linkedPurchaseOrderId ?? '')
                }}
              >
                {exception.exceptionKey}
              </button>
              <span
                className={`rounded px-2 py-0.5 text-xs ${statusClass(exception.status)}`}
                data-testid={`procurement-exception-status-${exception.exceptionId}`}
              >
                {exception.status}
              </span>
              <span className="text-xs text-slate-500">{exception.exceptionCategory}</span>
              {exception.isSlaBreached ? (
                <span
                  className="rounded bg-rose-500/20 px-2 py-0.5 text-xs text-rose-200"
                  data-testid={`procurement-exception-sla-breached-${exception.exceptionId}`}
                >
                  SLA breached
                </span>
              ) : null}
            </div>
            <p className="mt-1 text-sm text-slate-300">{exception.title}</p>
            <p className="mt-1 text-xs text-slate-500">
              Due {formatSlaDue(exception.slaDueAt)}
              {exception.assignedToUserId ? ` · assigned` : ' · unassigned'}
            </p>
            <div className="mt-2 flex flex-wrap gap-2">
              {exception.status === 'open' && (
                <button
                  type="button"
                  className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-200"
                  data-testid={`procurement-exception-investigate-${exception.exceptionId}`}
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
                    data-testid={`procurement-exception-resolve-${exception.exceptionId}`}
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
                    data-testid={`procurement-exception-request-waive-${exception.exceptionId}`}
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
                    data-testid={`procurement-exception-cancel-${exception.exceptionId}`}
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
                    data-testid={`procurement-exception-approve-waive-${exception.exceptionId}`}
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
                    data-testid={`procurement-exception-reject-waive-${exception.exceptionId}`}
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
                  data-testid={`procurement-exception-close-${exception.exceptionId}`}
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
              {exception.status === 'cancelled' && (
                <button
                  type="button"
                  className="rounded border border-sky-600 px-2 py-0.5 text-xs text-sky-200"
                  data-testid={`procurement-exception-reopen-${exception.exceptionId}`}
                  onClick={() =>
                    workflowMutation.mutate({
                      type: 'reopen',
                      exceptionId: exception.exceptionId,
                    })
                  }
                >
                  Reopen
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
