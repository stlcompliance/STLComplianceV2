import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ReferencePicker, ReferenceProviderClient, type CrossProductReference } from '@stl/shared-ui'

import {
  getMaintenanceVendorWork,
  issueMaintenanceVendorWorkPortalAccess,
  revokeMaintenanceVendorWorkPortalAccess,
  upsertMaintenanceVendorWork,
} from '../api/client'
import type { MaintenanceVendorWorkResponse } from '../api/types'

interface WorkOrderVendorWorkPanelProps {
  workOrder: { workOrderId: string } | null
  accessToken: string
  canPerform: boolean
}

const STATUS_OPTIONS = ['requested', 'quoted', 'approved', 'scheduled', 'in_progress', 'completed', 'rejected', 'canceled'] as const

type VendorWorkStatus = (typeof STATUS_OPTIONS)[number]

function normalizeStatus(status: string): VendorWorkStatus | 'other' {
  return (STATUS_OPTIONS as readonly string[]).includes(status.toLowerCase())
    ? (status.toLowerCase() as VendorWorkStatus)
    : 'other'
}

function chipClass(): string {
  return 'rounded-full border border-slate-700 bg-slate-950/80 px-2 py-0.5 text-[11px] text-slate-300'
}

function portalStatusLabel(status: string): string {
  switch (status) {
    case 'draft':
      return 'Draft'
    case 'sent':
      return 'Sent'
    case 'opened':
      return 'Opened'
    case 'used':
      return 'Used'
    case 'expired':
      return 'Expired'
    case 'revoked':
      return 'Revoked'
    default:
      return status.replaceAll('_', ' ')
  }
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) {
    return 'Not set'
  }

  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? 'Not set' : date.toLocaleString()
}

function toDateTimeLocalValue(value: string | null | undefined): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const offsetMinutes = date.getTimezoneOffset()
  return new Date(date.getTime() - offsetMinutes * 60_000).toISOString().slice(0, 16)
}

function fromDateTimeLocalValue(value: string): string | null {
  if (!value.trim()) {
    return null
  }

  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? null : date.toISOString()
}

function humanizeStatus(status: string): string {
  return status.replaceAll('_', ' ')
}

function parseReferenceSnapshot(value: string | null | undefined): CrossProductReference | null {
  if (!value) {
    return null
  }

  try {
    const parsed = JSON.parse(value) as Partial<CrossProductReference>
    if (
      typeof parsed.ownerProductKey === 'string' &&
      typeof parsed.referenceType === 'string' &&
      typeof parsed.referenceId === 'string' &&
      typeof parsed.displayLabelSnapshot === 'string'
    ) {
      return {
        ownerProductKey: parsed.ownerProductKey,
        referenceType: parsed.referenceType,
        referenceId: parsed.referenceId,
        displayLabelSnapshot: parsed.displayLabelSnapshot,
        secondaryLabelSnapshot: parsed.secondaryLabelSnapshot,
        statusSnapshot: parsed.statusSnapshot,
        ownerVersion: parsed.ownerVersion,
        createdVia: parsed.createdVia ?? 'selected',
      }
    }
  } catch {
    return null
  }

  return null
}

function serializeReferenceSnapshot(value: CrossProductReference | null): string {
  return value ? JSON.stringify(value) : ''
}

function formatReferenceSnapshot(value: string): string {
  const parsed = parseReferenceSnapshot(value)
  if (!parsed) {
    return value
  }

  return [
    parsed.displayLabelSnapshot,
    parsed.secondaryLabelSnapshot,
    parsed.statusSnapshot,
  ]
    .filter(Boolean)
    .join(' / ')
}

function statusLabel(status: VendorWorkStatus | 'other'): string {
  switch (status) {
    case 'requested':
      return 'Requested'
    case 'quoted':
      return 'Quoted'
    case 'approved':
      return 'Approved'
    case 'scheduled':
      return 'Scheduled'
    case 'in_progress':
      return 'In progress'
    case 'completed':
      return 'Completed'
    case 'rejected':
      return 'Rejected'
    case 'canceled':
      return 'Canceled'
    default:
      return 'Other'
  }
}

function summarizeVendorWork(items: MaintenanceVendorWorkResponse[]) {
  const counts = items.reduce(
    (acc, item) => {
      const normalized = normalizeStatus(item.status)
      acc[normalized] = (acc[normalized] ?? 0) + 1
      return acc
    },
    {
      requested: 0,
      quoted: 0,
      approved: 0,
      scheduled: 0,
      in_progress: 0,
      completed: 0,
      rejected: 0,
      canceled: 0,
      other: 0,
    } as Record<VendorWorkStatus | 'other', number>,
  )

  const activeCount = counts.requested + counts.quoted + counts.approved + counts.scheduled + counts.in_progress
  const terminalCount = counts.completed + counts.rejected + counts.canceled
  const summaryLabel =
    counts.in_progress > 0
      ? 'Vendor work in progress'
      : counts.scheduled > 0
        ? 'Vendor work scheduled'
        : counts.approved > 0
          ? 'Vendor work approved'
          : counts.quoted > 0
            ? 'Vendor quote review'
            : counts.requested > 0
              ? 'Vendor work awaiting quote'
              : counts.completed > 0 && activeCount === 0
                ? 'Vendor work complete'
                : counts.rejected > 0 && activeCount === 0
                  ? 'Vendor work rejected'
                  : counts.canceled > 0 && activeCount === 0
                    ? 'Vendor work canceled'
                    : 'Vendor coordination'

  const summaryDetail =
    counts.requested > 0
      ? `${counts.requested} work item${counts.requested === 1 ? '' : 's'} still need a vendor quote or scoped proposal.`
      : counts.quoted > 0
        ? `${counts.quoted} quoted item${counts.quoted === 1 ? '' : 's'} are waiting on approval before scheduling.`
        : counts.approved > 0
          ? `${counts.approved} approved item${counts.approved === 1 ? '' : 's'} can now be scheduled or dispatched.`
          : counts.scheduled > 0
            ? `${counts.scheduled} scheduled item${counts.scheduled === 1 ? '' : 's'} are waiting for vendor execution.`
            : counts.in_progress > 0
              ? `${counts.in_progress} item${counts.in_progress === 1 ? '' : 's'} are actively being worked by the vendor.`
              : counts.completed > 0
                ? `${counts.completed} completed item${counts.completed === 1 ? '' : 's'} are waiting on closeout refs or downstream handoff.`
                : terminalCount > 0
                  ? `${terminalCount} item${terminalCount === 1 ? '' : 's'} are in a terminal state.`
                  : 'No vendor work items have been coordinated yet.'

  const nextStep =
    counts.requested > 0
      ? 'Capture the vendor quote, then move the work to approved before scheduling.'
      : counts.quoted > 0
        ? 'Review the quote and approval refs, then schedule the external work.'
        : counts.approved > 0
          ? 'Share the access window and schedule the visit or portal dispatch.'
          : counts.scheduled > 0
            ? 'Track vendor updates and evidence until the work is complete.'
            : counts.in_progress > 0
              ? 'Collect completion evidence and invoice refs before closeout.'
              : counts.completed > 0
                ? 'Resolve invoice and warranty refs, then close the work order handoff.'
                : 'Create a vendor work item when external execution is needed.'

  return {
    counts,
    summaryLabel,
    summaryDetail,
    nextStep,
  }
}

export function WorkOrderVendorWorkPanel({ workOrder, accessToken, canPerform }: WorkOrderVendorWorkPanelProps) {
  const queryClient = useQueryClient()
  const [selectedVendorWorkId, setSelectedVendorWorkId] = useState('')
  const [supplierReference, setSupplierReference] = useState<CrossProductReference | null>(null)
  const [legacySupplierRef, setLegacySupplierRef] = useState('')
  const [vendorContactSnapshot, setVendorContactSnapshot] = useState('')
  const [status, setStatus] = useState<(typeof STATUS_OPTIONS)[number]>('requested')
  const [workDescription, setWorkDescription] = useState('')
  const [quoteRecordRef, setQuoteRecordRef] = useState('')
  const [approvalRef, setApprovalRef] = useState('')
  const [scheduledAt, setScheduledAt] = useState('')
  const [completedAt, setCompletedAt] = useState('')
  const [costEstimateSnapshot, setCostEstimateSnapshot] = useState('')
  const [invoiceRecordRef, setInvoiceRecordRef] = useState('')
  const [warrantyFlag, setWarrantyFlag] = useState(false)
  const [notes, setNotes] = useState('')

  const vendorWorkQuery = useQuery({
    queryKey: ['maintainarr-vendor-work', accessToken, workOrder?.workOrderId],
    queryFn: () => getMaintenanceVendorWork(accessToken, workOrder!.workOrderId),
    enabled: Boolean(accessToken && workOrder?.workOrderId),
  })

  const supplyReferenceClient = useMemo(
    () =>
      new ReferenceProviderClient({
        baseUrl: import.meta.env.VITE_SUPPLYARR_API_BASE ?? import.meta.env.VITE_MAINTAINARR_API_BASE ?? '',
        getHeaders: () => ({ Authorization: `Bearer ${accessToken}` }),
      }),
    [accessToken],
  )

  const selectedVendorWork = useMemo(
    () => vendorWorkQuery.data?.items.find((item) => item.vendorWorkId === selectedVendorWorkId) ?? null,
    [selectedVendorWorkId, vendorWorkQuery.data?.items],
  )

  const selectedVendorWorkPortalHref = typeof window !== 'undefined' && selectedVendorWork?.portalAccessUrl
    ? `${window.location.origin}${selectedVendorWork.portalAccessUrl}`
    : ''

  useEffect(() => {
    const initialVendorWork = vendorWorkQuery.data?.items[0]
    if (!selectedVendorWorkId && initialVendorWork) {
      setSelectedVendorWorkId(initialVendorWork.vendorWorkId)
    }
  }, [selectedVendorWorkId, vendorWorkQuery.data?.items])

  useEffect(() => {
    if (!selectedVendorWork) {
      setSupplierReference(null)
      setLegacySupplierRef('')
      setVendorContactSnapshot('')
      setStatus('requested')
      setWorkDescription('')
      setQuoteRecordRef('')
      setApprovalRef('')
      setScheduledAt('')
      setCompletedAt('')
      setCostEstimateSnapshot('')
      setInvoiceRecordRef('')
      setWarrantyFlag(false)
      setNotes('')
      return
    }

    const parsedSupplier = parseReferenceSnapshot(selectedVendorWork.supplierRef)
    setSupplierReference(parsedSupplier)
    setLegacySupplierRef(parsedSupplier ? '' : selectedVendorWork.supplierRef)
    setVendorContactSnapshot(selectedVendorWork.vendorContactSnapshot ?? '')
    setStatus(selectedVendorWork.status as (typeof STATUS_OPTIONS)[number])
    setWorkDescription(selectedVendorWork.workDescription ?? '')
    setQuoteRecordRef(selectedVendorWork.quoteRecordRef ?? '')
    setApprovalRef(selectedVendorWork.approvalRef ?? '')
    setScheduledAt(toDateTimeLocalValue(selectedVendorWork.scheduledAt))
    setCompletedAt(toDateTimeLocalValue(selectedVendorWork.completedAt))
    setCostEstimateSnapshot(selectedVendorWork.costEstimateSnapshot ?? '')
    setInvoiceRecordRef(selectedVendorWork.invoiceRecordRef ?? '')
    setWarrantyFlag(selectedVendorWork.warrantyFlag)
    setNotes(selectedVendorWork.notes ?? '')
  }, [selectedVendorWork])

  useEffect(() => {
    if (!workOrder?.workOrderId) {
      setSelectedVendorWorkId('')
    }
  }, [workOrder?.workOrderId])

  const upsertMutation = useMutation({
    mutationFn: () =>
      upsertMaintenanceVendorWork(accessToken, workOrder!.workOrderId, {
        supplierRef: serializeReferenceSnapshot(supplierReference) || legacySupplierRef,
        vendorContactSnapshot: vendorContactSnapshot.trim() || null,
        status,
        workDescription: workDescription.trim() || null,
        quoteRecordRef: quoteRecordRef.trim() || null,
        approvalRef: approvalRef.trim() || null,
        scheduledAt: fromDateTimeLocalValue(scheduledAt),
        completedAt: fromDateTimeLocalValue(completedAt),
        costEstimateSnapshot: costEstimateSnapshot.trim() || null,
        invoiceRecordRef: invoiceRecordRef.trim() || null,
        warrantyFlag,
        notes: notes.trim() || null,
      }),
    onSuccess: async (saved) => {
      setSelectedVendorWorkId(saved.vendorWorkId)
      await queryClient.invalidateQueries({
        queryKey: ['maintainarr-vendor-work', accessToken, workOrder?.workOrderId],
      })
    },
  })

  const issuePortalAccessMutation = useMutation({
    mutationFn: () =>
      issueMaintenanceVendorWorkPortalAccess(accessToken, workOrder!.workOrderId, selectedVendorWork!.vendorWorkId),
    onSuccess: async (saved) => {
      setSelectedVendorWorkId(saved.vendorWorkId)
      await queryClient.invalidateQueries({
        queryKey: ['maintainarr-vendor-work', accessToken, workOrder?.workOrderId],
      })
    },
  })

  const revokePortalAccessMutation = useMutation({
    mutationFn: () =>
      revokeMaintenanceVendorWorkPortalAccess(accessToken, workOrder!.workOrderId, selectedVendorWork!.vendorWorkId),
    onSuccess: async (saved) => {
      setSelectedVendorWorkId(saved.vendorWorkId)
      await queryClient.invalidateQueries({
        queryKey: ['maintainarr-vendor-work', accessToken, workOrder?.workOrderId],
      })
    },
  })

  if (!workOrder) {
    return null
  }

  const items = vendorWorkQuery.data?.items ?? []
  const summary = useMemo(() => summarizeVendorWork(items), [items])

  return (
    <section className="mt-6 border-t border-slate-800 pt-4" data-testid="work-order-vendor-work-panel">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h4 className="text-sm font-semibold text-white">Vendor coordination</h4>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Track external vendor work coordinated by MaintainArr for this work order.
          </p>
        </div>
        <span className="text-xs text-[var(--color-text-muted)]">{items.length} vendor work item(s)</span>
      </div>

      {items.length > 0 ? (
        <div
          className="mt-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4"
          data-testid="vendor-work-summary"
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                {summary.summaryLabel}
              </h5>
              <p className="mt-1 text-sm text-slate-300">{summary.summaryDetail}</p>
            </div>
            <span className="rounded-full border border-slate-700 bg-slate-900 px-3 py-1 text-xs text-slate-200">
              {summary.counts.completed > 0 && summary.counts.requested === 0 && summary.counts.quoted === 0 && summary.counts.approved === 0 && summary.counts.scheduled === 0 && summary.counts.in_progress === 0
                ? 'Ready for closeout'
                : summary.counts.in_progress > 0
                  ? 'Active'
                  : summary.counts.scheduled > 0 || summary.counts.approved > 0
                    ? 'Awaiting execution'
                    : summary.counts.requested > 0 || summary.counts.quoted > 0
                      ? 'Awaiting approval'
                      : 'Tracked'}
            </span>
          </div>
          <dl className="mt-3 grid gap-2 text-sm sm:grid-cols-4 lg:grid-cols-8">
            {STATUS_OPTIONS.map((option) => (
              <div key={option}>
                <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{statusLabel(option)}</dt>
                <dd className="mt-1 font-semibold text-white">{summary.counts[option]}</dd>
              </div>
            ))}
          </dl>
          <p className="mt-3 text-sm text-slate-300">{summary.nextStep}</p>
        </div>
      ) : (
        <div className="mt-4 rounded-xl border border-dashed border-slate-800 bg-slate-950/30 p-4">
          <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
            Vendor work summary
          </h5>
          <p className="mt-1 text-sm text-slate-400">
            Create a vendor work item to track quote, approval, scheduling, completion, and invoice handoff.
          </p>
        </div>
      )}

      {vendorWorkQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-400">Loading vendor work…</p>
      ) : items.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No vendor work has been coordinated for this work order yet.</p>
      ) : (
        <ul className="mt-3 space-y-2">
          {items.map((item) => (
            <li key={item.vendorWorkId}>
              <button
                type="button"
                className={`w-full rounded-xl border px-3 py-2 text-left transition ${
                  selectedVendorWorkId === item.vendorWorkId
                    ? 'border-amber-500/60 bg-amber-500/10'
                    : 'border-slate-800 bg-slate-900/40 hover:border-slate-700 hover:bg-slate-900/70'
                }`}
                onClick={() => setSelectedVendorWorkId(item.vendorWorkId)}
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <div className="font-medium text-white">{formatReferenceSnapshot(item.supplierRef)}</div>
                    <div className="text-xs text-slate-400">
                      Updated {new Date(item.updatedAt).toLocaleString()}
                      {item.completedAt ? ` · Completed ${new Date(item.completedAt).toLocaleString()}` : ''}
                    </div>
                  </div>
                  <span className={chipClass()}>{humanizeStatus(item.status)}</span>
                </div>
                <div className="mt-2 text-sm text-slate-300">
                  {item.workDescription || 'No work description set.'}
                </div>
                <div className="mt-2 flex flex-wrap gap-1">
                  {item.warrantyFlag ? <span className={chipClass()}>Warranty</span> : null}
                  {item.quoteRecordRef ? <span className={chipClass()}>Quote ref</span> : null}
                  {item.invoiceRecordRef ? <span className={chipClass()}>Invoice ref</span> : null}
                  {item.portalAccessCode ? <span className={chipClass()}>Portal {portalStatusLabel(item.portalAccessStatus)}</span> : null}
                </div>
                <div className="mt-3 grid gap-2 text-xs text-[var(--color-text-muted)] sm:grid-cols-2 xl:grid-cols-4">
                  <div>
                    <div className="uppercase tracking-wide">Quote</div>
                    <div className="mt-1 text-slate-200">{item.quoteRecordRef ?? 'Not captured'}</div>
                  </div>
                  <div>
                    <div className="uppercase tracking-wide">Approval</div>
                    <div className="mt-1 text-slate-200">{item.approvalRef ?? 'Not captured'}</div>
                  </div>
                  <div>
                    <div className="uppercase tracking-wide">Scheduled</div>
                    <div className="mt-1 text-slate-200">
                      {item.scheduledAt ? new Date(item.scheduledAt).toLocaleString() : 'Not scheduled'}
                    </div>
                  </div>
                  <div>
                    <div className="uppercase tracking-wide">Completed</div>
                    <div className="mt-1 text-slate-200">
                      {formatDateTime(item.completedAt)}
                    </div>
                  </div>
                </div>
              </button>
            </li>
          ))}
        </ul>
      )}

      {selectedVendorWork ? (
        <div className="mt-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h5 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
                Portal access
              </h5>
              <p className="mt-1 text-sm text-slate-300">
                Issue a scoped, expiring vendor portal link for this work item.
              </p>
            </div>
            <span className={chipClass()}>{portalStatusLabel(selectedVendorWork.portalAccessStatus)}</span>
          </div>

          <div className="mt-3 grid gap-3 md:grid-cols-2">
            <div>
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Issued</div>
              <div className="mt-1 text-sm text-slate-200">{formatDateTime(selectedVendorWork.portalAccessCodeIssuedAt)}</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Expires</div>
              <div className="mt-1 text-sm text-slate-200">{formatDateTime(selectedVendorWork.portalAccessExpiresAt)}</div>
            </div>
            <label className="block text-xs text-slate-400 md:col-span-2">
              Portal code
              <input
                readOnly
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={selectedVendorWork.portalAccessCode ?? 'Not issued yet'}
              />
            </label>
            <label className="block text-xs text-slate-400 md:col-span-2">
              Portal link
              <input
                readOnly
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={selectedVendorWorkPortalHref || 'Not issued yet'}
              />
            </label>
            <div>
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Opened</div>
              <div className="mt-1 text-sm text-slate-200">{formatDateTime(selectedVendorWork.portalAccessOpenedAt)}</div>
            </div>
            <div>
              <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Revoked</div>
              <div className="mt-1 text-sm text-slate-200">{formatDateTime(selectedVendorWork.portalAccessRevokedAt)}</div>
            </div>
          </div>

          <div className="mt-4 flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded bg-emerald-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={issuePortalAccessMutation.isPending || revokePortalAccessMutation.isPending}
              onClick={() => issuePortalAccessMutation.mutate()}
            >
              {selectedVendorWork.portalAccessCode && selectedVendorWork.portalAccessStatus !== 'revoked' && selectedVendorWork.portalAccessStatus !== 'expired'
                ? 'Refresh portal access'
                : 'Issue portal access'}
            </button>
            {selectedVendorWork.portalAccessCode && selectedVendorWork.portalAccessStatus !== 'revoked' && selectedVendorWork.portalAccessStatus !== 'expired' ? (
              <button
                type="button"
                className="rounded border border-rose-500/60 px-3 py-2 text-sm text-rose-200 disabled:opacity-50"
                disabled={revokePortalAccessMutation.isPending || issuePortalAccessMutation.isPending}
                onClick={() => revokePortalAccessMutation.mutate()}
              >
                {revokePortalAccessMutation.isPending ? 'Revoking…' : 'Revoke portal access'}
              </button>
            ) : null}
          </div>
        </div>
      ) : null}

      {canPerform ? (
        <form
          className="mt-4 grid gap-4 rounded-xl border border-slate-800 bg-slate-950/60 p-4"
          onSubmit={(event) => {
            event.preventDefault()
            upsertMutation.mutate()
          }}
        >
          <div className="grid gap-4 md:grid-cols-2">
            <label className="block text-xs text-slate-400">
              Supplier ref
              <ReferencePicker
                client={supplyReferenceClient}
                ownerProductKey="supplyarr"
                referenceType="supplier"
                value={supplierReference}
                onChange={(value) => {
                  setSupplierReference(value)
                  if (value) {
                    setLegacySupplierRef('')
                  }
                }}
                placeholder="Search SupplyArr suppliers"
                disabled={upsertMutation.isPending}
              />
              {legacySupplierRef ? (
                <span className="mt-1 block text-xs text-amber-200">
                  Legacy supplier ref: {legacySupplierRef}. Select a SupplyArr supplier to replace it with an owner snapshot.
                </span>
              ) : null}
            </label>
            <label className="block text-xs text-slate-400">
              Status
              <select
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={status}
                onChange={(event) => setStatus(event.target.value as (typeof STATUS_OPTIONS)[number])}
                disabled={upsertMutation.isPending}
              >
                {STATUS_OPTIONS.map((option) => (
                  <option key={option} value={option}>
                    {humanizeStatus(option)}
                  </option>
                ))}
              </select>
            </label>
            <label className="block text-xs text-slate-400 md:col-span-2">
              Vendor contact snapshot
              <textarea
                className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={vendorContactSnapshot}
                onChange={(event) => setVendorContactSnapshot(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400 md:col-span-2">
              Work description
              <textarea
                className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={workDescription}
                onChange={(event) => setWorkDescription(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Quote record ref
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={quoteRecordRef}
                onChange={(event) => setQuoteRecordRef(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Approval ref
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={approvalRef}
                onChange={(event) => setApprovalRef(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Scheduled at
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                type="datetime-local"
                value={scheduledAt}
                onChange={(event) => setScheduledAt(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Completed at
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                type="datetime-local"
                value={completedAt}
                onChange={(event) => setCompletedAt(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Cost estimate snapshot
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={costEstimateSnapshot}
                onChange={(event) => setCostEstimateSnapshot(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Invoice record ref
              <input
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={invoiceRecordRef}
                onChange={(event) => setInvoiceRecordRef(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
            <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2">
              <input
                type="checkbox"
                checked={warrantyFlag}
                onChange={(event) => setWarrantyFlag(event.target.checked)}
                disabled={upsertMutation.isPending}
              />
              Warranty work
            </label>
            <label className="block text-xs text-slate-400 md:col-span-2">
              Notes
              <textarea
                className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                disabled={upsertMutation.isPending}
              />
            </label>
          </div>
          <div className="flex flex-wrap gap-2">
            <button
              type="submit"
              className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
              disabled={upsertMutation.isPending || (!supplierReference && legacySupplierRef.trim().length === 0)}
            >
              {upsertMutation.isPending ? 'Saving…' : selectedVendorWork ? 'Save vendor work' : 'Create vendor work'}
            </button>
            {selectedVendorWork ? (
              <button
                type="button"
                className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-600"
                onClick={() => setSelectedVendorWorkId('')}
              >
                Clear selection
              </button>
            ) : null}
          </div>
        </form>
      ) : null}
    </section>
  )
}
