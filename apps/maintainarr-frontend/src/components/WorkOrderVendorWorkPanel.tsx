import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ReferencePicker, ReferenceProviderClient, type CrossProductReference } from '@stl/shared-ui'

import { getMaintenanceVendorWork, upsertMaintenanceVendorWork } from '../api/client'

interface WorkOrderVendorWorkPanelProps {
  workOrder: { workOrderId: string } | null
  accessToken: string
  canPerform: boolean
}

const STATUS_OPTIONS = ['requested', 'quoted', 'approved', 'scheduled', 'in_progress', 'completed', 'rejected', 'canceled'] as const

function chipClass(): string {
  return 'rounded-full border border-slate-700 bg-slate-950/80 px-2 py-0.5 text-[11px] text-slate-300'
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

  if (!workOrder) {
    return null
  }

  const items = vendorWorkQuery.data?.items ?? []

  return (
    <section className="mt-6 border-t border-slate-800 pt-4" data-testid="work-order-vendor-work-panel">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h4 className="text-sm font-semibold text-white">Vendor coordination</h4>
          <p className="mt-1 text-xs text-slate-500">
            Track external vendor work coordinated by MaintainArr for this work order.
          </p>
        </div>
        <span className="text-xs text-slate-500">{items.length} vendor work item(s)</span>
      </div>

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
                </div>
              </button>
            </li>
          ))}
        </ul>
      )}

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
