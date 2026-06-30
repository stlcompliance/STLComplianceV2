import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import {
  createEmergencyPurchase,
  expeditedSubmitEmergencyPurchase,
  getEmergencyPurchases,
  issueEmergencyPurchaseOrder,
  listPendingEmergencyPurchases,
  managerOverrideApproveEmergencyPurchase,
} from '../api/client'
import type { EmergencyPurchaseResponse, PartResponse } from '../api/types'
import {
  toPartPickerOptions,
  toSupplierUnitPickerOptions,
  type SupplierUnitPickerSource,
} from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import {
  formatSupplierIdentitySummary,
  formatSupplierServiceTypes,
  humanizeSupplierUnitKind,
  resolveSupplierId,
} from '../utils/supplierPresentation'

interface EmergencyPurchasePanelProps {
  accessToken: string
  canCreate: boolean
  canOverrideApprove: boolean
  parts: PartResponse[]
  suppliers: SupplierUnitPickerSource[]
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'submitted':
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
    case 'rejected':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-sky-500/20 text-sky-200 ring-sky-500/40'
  }
}

export function EmergencyPurchasePanel({
  accessToken,
  canCreate,
  canOverrideApprove,
  parts,
  suppliers,
}: EmergencyPurchasePanelProps) {
  if (!canCreate && !canOverrideApprove) {
    return null
  }

  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState('')
  const [requestKey, setRequestKey] = useState('')
  const [title, setTitle] = useState('')
  const [emergencyReason, setEmergencyReason] = useState('')
  const [notes] = useState('')
  const [supplierUnitId, setSupplierUnitId] = useState('')
  const [partId, setPartId] = useState('')
  const [lineQty] = useState('1')
  const [justification, setJustification] = useState('')
  const [orderKey, setOrderKey] = useState('')

  const listQuery = useQuery({
    queryKey: ['supplyarr-emergency-purchases', accessToken],
    queryFn: () => getEmergencyPurchases(accessToken),
    enabled: canCreate || canOverrideApprove,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-emergency-purchases-pending', accessToken],
    queryFn: () => listPendingEmergencyPurchases(accessToken),
    enabled: canOverrideApprove,
  })

  const selected: EmergencyPurchaseResponse | undefined = useMemo(
    () => listQuery.data?.find((x) => x.purchaseRequestId === selectedId),
    [listQuery.data, selectedId],
  )
  const existingRequestKeys = useMemo(
    () => (listQuery.data ?? []).map((item) => item.requestKey),
    [listQuery.data],
  )
  const supplierUnitOptions = useMemo<PickerOption[]>(
    () => toSupplierUnitPickerOptions(suppliers),
    [suppliers],
  )
  const selectedSupplierUnitOption = useMemo<PickerOption | undefined>(
    () => supplierUnitOptions.find((option) => option.value === supplierUnitId),
    [supplierUnitId, supplierUnitOptions],
  )
  const selectedSupplierUnit = useMemo<SupplierUnitPickerSource | undefined>(
    () => suppliers.find((supplier) => resolveSupplierId(supplier) === supplierUnitId),
    [supplierUnitId, suppliers],
  )
  const partOptions = useMemo<PickerOption[]>(
    () => toPartPickerOptions(parts),
    [parts],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === partId),
    [partId, partOptions],
  )
  const orderKeySource = selected ? `${selected.requestKey}-po` : ''

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-emergency-purchases', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-emergency-purchases-pending', accessToken],
    })
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-requests'] })
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-purchase-orders'] })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createEmergencyPurchase(accessToken, {
        requestKey,
        title,
        emergencyReason,
        supplierId: supplierUnitId,
        notes,
        lines: partId
          ? [{ partId, quantityRequested: Number(lineQty) || 1, notes: '' }]
          : [],
      }),
    onSuccess: (created) => {
      setSelectedId(created.purchaseRequestId)
      invalidate()
    },
  })

  const submitMutation = useMutation({
    mutationFn: () => expeditedSubmitEmergencyPurchase(accessToken, selectedId, notes),
    onSuccess: invalidate,
  })

  const approveMutation = useMutation({
    mutationFn: () =>
      managerOverrideApproveEmergencyPurchase(accessToken, selectedId, justification),
    onSuccess: invalidate,
  })

  const issueMutation = useMutation({
    mutationFn: () => issueEmergencyPurchaseOrder(accessToken, selectedId, orderKey),
    onSuccess: invalidate,
  })

  const canExpediteSubmit = selected?.status === 'draft'
  const canOverride = selected?.status === 'submitted' && canOverrideApprove
  const canIssue =
    selected?.status === 'approved' &&
    selected.managerOverrideApproved &&
    !selected.linkedPurchaseOrderId &&
    canCreate

  return (
    <section
      data-testid="emergency-purchase-panel"
      className="rounded-xl border border-rose-900/50 bg-rose-950/20 p-5 lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Emergency purchase</h2>
      <p className="mt-1 text-sm text-slate-400">
        Urgent procurement from a supplier identity or sub-unit with expedited submit, manager override, and linked PO issue.
      </p>

      {canOverrideApprove && (pendingQuery.data?.length ?? 0) > 0 ? (
        <div className="mt-4 rounded-lg border border-amber-800/50 bg-amber-950/30 p-3">
          <p className="text-sm font-medium text-amber-200">
            {pendingQuery.data!.length} awaiting manager override
          </p>
          <ul className="mt-2 space-y-1 text-sm">
            {pendingQuery.data!.map((item) => (
              <li key={item.purchaseRequestId}>
                <button
                  type="button"
                  className="text-left underline decoration-dotted hover:text-white"
                  onClick={() => setSelectedId(item.purchaseRequestId)}
                >
                  {item.title} ({item.requestKey})
                </button>
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-4 grid gap-2 sm:grid-cols-2">
          <label htmlFor="emergency-purchase-title" className="block text-sm text-slate-400 sm:col-span-2">
            Emergency purchase title
            <input
              id="emergency-purchase-title"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
            />
          </label>
          <div className="sm:col-span-2">
            <GeneratedKeyFieldGroup
              sourceLabel={title}
              existingKeys={existingRequestKeys}
              onKeyChange={setRequestKey}
              domain="purchase"
              kind="request"
              label="Request key"
            />
          </div>
          <label htmlFor="emergency-purchase-reason" className="block text-sm text-slate-400 sm:col-span-2">
            Emergency reason
            <input
              id="emergency-purchase-reason"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              value={emergencyReason}
              onChange={(e) => setEmergencyReason(e.target.value)}
            />
          </label>
          <StaticSearchPicker
            label="Supplier identity or sub-unit"
            id="emergency-purchase-supplier-unit"
            value={supplierUnitId}
            options={supplierUnitOptions}
            selectedOption={selectedSupplierUnitOption}
            onChange={setSupplierUnitId}
            placeholder="Search supplier identities or sub-units…"
            testId="emergency-purchase-supplier-unit-picker"
          />
          {selectedSupplierUnit ? (
            <p className="text-xs text-[var(--color-text-muted)] sm:col-span-2">
              {formatSupplierIdentitySummary({
                supplierDisplayName: selectedSupplierUnit.displayName,
                supplierKey: selectedSupplierUnit.supplierKey,
                parentSupplierDisplayName: selectedSupplierUnit.parentSupplierDisplayName,
                supplierUnitKind: selectedSupplierUnit.unitKind,
              })}{' '}
              · {humanizeSupplierUnitKind(selectedSupplierUnit.unitKind)}
            </p>
          ) : null}
          <StaticSearchPicker
            label="Part"
            id="emergency-purchase-part"
            value={partId}
            options={partOptions}
            selectedOption={selectedPartOption}
            onChange={setPartId}
            placeholder="Search parts…"
            testId="emergency-purchase-part-picker"
          />
          <button
            type="button"
            className="rounded-lg bg-rose-700 px-3 py-2 text-sm text-white disabled:opacity-50"
            disabled={
              createMutation.isPending ||
              !requestKey ||
              !title ||
              !emergencyReason ||
              !supplierUnitId ||
              !partId
            }
            onClick={() => createMutation.mutate()}
          >
            Create emergency PR
          </button>
        </div>
      ) : null}

      <div className="mt-4">
        <label htmlFor="emergency-purchase-select" className="block text-sm text-slate-400">
          Active emergency purchases
          <select
            id="emergency-purchase-select"
            className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            value={selectedId}
            onChange={(e) => setSelectedId(e.target.value)}
          >
            <option value="">Select…</option>
            {(listQuery.data ?? []).map((item) => (
              <option key={item.purchaseRequestId} value={item.purchaseRequestId}>
                {item.requestKey} — {item.status}
              </option>
            ))}
          </select>
        </label>
      </div>

      {selected ? (
        <div className="mt-4 rounded-lg border border-slate-800 p-3">
          <div className="flex justify-between gap-2">
            <div>
              <div className="font-medium">{selected.title}</div>
              <div className="text-sm text-slate-400">{selected.emergencyReason}</div>
              {selected.supplierDisplayName ? (
                <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                  {formatSupplierIdentitySummary(selected)} · {humanizeSupplierUnitKind(selected.supplierUnitKind)} ·{' '}
                  {formatSupplierServiceTypes(selected.supplierServiceTypes)}
                </div>
              ) : null}
            </div>
            <span
              className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(selected.status)}`}
            >
              {selected.status}
            </span>
          </div>
          {selected.managerOverrideApproved ? (
            <p className="mt-2 text-sm text-emerald-300">Manager override approved</p>
          ) : null}
          {selected.linkedPurchaseOrderKey ? (
            <p className="mt-1 text-sm text-slate-400">PO: {selected.linkedPurchaseOrderKey}</p>
          ) : null}
        </div>
      ) : null}

      {canCreate && selected && canExpediteSubmit ? (
        <button
          type="button"
          className="mt-3 rounded-lg bg-amber-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={submitMutation.isPending}
          onClick={() => submitMutation.mutate()}
        >
          Expedited submit
        </button>
      ) : null}

      {canOverride && selected ? (
        <div className="mt-3 flex flex-wrap gap-2">
          <label htmlFor="emergency-purchase-override-justification" className="min-w-[12rem] flex-1 text-sm text-slate-400">
            Override justification
            <input
              id="emergency-purchase-override-justification"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              value={justification}
              onChange={(e) => setJustification(e.target.value)}
            />
          </label>
          <button
            type="button"
            className="rounded-lg bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={approveMutation.isPending || !justification.trim()}
            onClick={() => approveMutation.mutate()}
          >
            Manager override approve
          </button>
        </div>
      ) : null}

      {canIssue && selected ? (
        <div className="mt-3 flex flex-wrap gap-2">
          <div className="min-w-[12rem] flex-1">
            <GeneratedKeyFieldGroup
              sourceLabel={orderKeySource}
              existingKeys={[]}
              onKeyChange={setOrderKey}
              domain="purchase"
              kind="order"
              label="PO order key"
            />
          </div>
          <button
            type="button"
            className="rounded-lg bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={issueMutation.isPending || !orderKey.trim()}
            onClick={() => issueMutation.mutate()}
          >
            Issue purchase order
          </button>
        </div>
      ) : null}
    </section>
  )
}
