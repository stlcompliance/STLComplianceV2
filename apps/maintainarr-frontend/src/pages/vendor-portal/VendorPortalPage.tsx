import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams, useSearchParams } from 'react-router-dom'

import {
  getMaintenanceVendorWorkPortal,
  updateMaintenanceVendorWorkPortal,
} from '../../api/client'

type PortalFormState = {
  status: string
  scheduledAt: string
  completedAt: string
  notes: string
}

const PORTAL_STATUS_OPTIONS = ['scheduled', 'in_progress', 'completed', 'rejected', 'canceled'] as const

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

function humanizeStatus(value: string): string {
  return value.replaceAll('_', ' ')
}

export function VendorPortalPage() {
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const { workOrderId } = useParams<{ workOrderId: string }>()
  const accessCode = searchParams.get('accessCode') ?? ''
  const [formState, setFormState] = useState<PortalFormState>({
    status: 'scheduled',
    scheduledAt: '',
    completedAt: '',
    notes: '',
  })
  const [lastSavedAt, setLastSavedAt] = useState<string | null>(null)

  const portalQuery = useQuery({
    queryKey: ['maintainarr-vendor-portal', workOrderId, accessCode],
    queryFn: () => getMaintenanceVendorWorkPortal(workOrderId!, accessCode),
    enabled: Boolean(workOrderId && accessCode),
  })

  const portal = portalQuery.data

  useEffect(() => {
    if (!portal) {
      return
    }

    setFormState({
      status: portal.status,
      scheduledAt: toDateTimeLocalValue(portal.scheduledAt),
      completedAt: toDateTimeLocalValue(portal.completedAt),
      notes: portal.notes ?? '',
    })
  }, [portal])

  const allowedStatusOptions = useMemo(
    () =>
      PORTAL_STATUS_OPTIONS.map((status) => ({
        value: status,
        label: humanizeStatus(status),
      })),
    [],
  )

  const submitMutation = useMutation({
    mutationFn: () => {
      if (!workOrderId || !accessCode) {
        throw new Error('Work order id and access code are required')
      }

      return updateMaintenanceVendorWorkPortal(workOrderId, accessCode, {
        status: formState.status,
        scheduledAt: fromDateTimeLocalValue(formState.scheduledAt),
        completedAt: fromDateTimeLocalValue(formState.completedAt),
        notes: formState.notes,
      })
    },
    onSuccess: async (updated) => {
      setLastSavedAt(updated.updatedAt)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-vendor-portal', workOrderId, accessCode] })
    },
  })

  if (!workOrderId || !accessCode) {
    return (
      <main className="min-h-screen bg-[var(--color-bg-page)] px-4 py-8 text-[var(--color-text-primary)] sm:px-6 sm:py-10">
        <div className="mx-auto max-w-4xl rounded-[2rem] border border-slate-700 bg-[var(--color-bg-surface)] p-6 shadow-sm">
          <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Vendor portal</h1>
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">
            A work order id and access code are required to open this portal.
          </p>
        </div>
      </main>
    )
  }

  if (portalQuery.isLoading) {
    return (
      <main className="min-h-screen bg-[var(--color-bg-page)] px-4 py-8 text-[var(--color-text-primary)] sm:px-6 sm:py-10">
        <div className="mx-auto max-w-4xl rounded-[2rem] border border-slate-700 bg-[var(--color-bg-surface)] p-6 shadow-sm">
          Loading vendor portal...
        </div>
      </main>
    )
  }

  if (portalQuery.isError || !portal) {
    return (
      <main className="min-h-screen bg-[var(--color-bg-page)] px-4 py-8 text-[var(--color-text-primary)] sm:px-6 sm:py-10">
        <div className="mx-auto max-w-4xl rounded-[2rem] border border-rose-200 bg-[var(--color-bg-surface)] p-6 shadow-sm">
          <h1 className="text-2xl font-bold text-[var(--color-text-primary)]">Vendor portal link unavailable</h1>
          <p className="mt-3 text-sm text-[var(--color-text-muted)]">
            This link may be invalid, revoked, or expired.
          </p>
        </div>
      </main>
    )
  }

  const isTerminal = portal.status === 'completed' || portal.status === 'rejected' || portal.status === 'canceled'

  return (
    <main className="min-h-screen bg-[var(--color-bg-page)] px-4 py-6 text-[var(--color-text-primary)] sm:px-6 sm:py-10">
      <div className="mx-auto max-w-6xl space-y-6">
        <header className="rounded-[2rem] border border-slate-700 bg-[var(--color-bg-surface)] p-6 shadow-sm">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <p className="text-xs uppercase tracking-[0.25em] text-[var(--color-text-muted)]">MaintainArr vendor portal</p>
              <h1 className="mt-2 text-3xl font-bold text-[var(--color-text-primary)]">{portal.workOrderNumber}</h1>
              <p className="mt-3 max-w-3xl text-sm text-[var(--color-text-muted)]">
                Review the assigned maintenance work, update your execution status, and return completion details without needing an internal login.
              </p>
            </div>
            <div className="space-y-2 rounded-2xl border border-slate-700 bg-slate-950/60 px-4 py-3 text-sm">
              <p className="font-semibold text-[var(--color-text-primary)]">{humanizeStatus(portal.portalAccessStatus)}</p>
              <p className="text-[var(--color-text-muted)]">Portal expires {formatDateTime(portal.portalAccessExpiresAt)}</p>
              {lastSavedAt ? <p className="text-[var(--color-text-muted)]">Last saved {formatDateTime(lastSavedAt)}</p> : null}
            </div>
          </div>
        </header>

        <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <PortalCard label="Work order" value={portal.workOrderTitle} detail={portal.workOrderPriority} />
          <PortalCard label="Asset" value={portal.assetTag} detail={portal.assetName} />
          <PortalCard label="Current status" value={humanizeStatus(portal.status)} detail={portal.workOrderStatus} />
          <PortalCard label="Updated" value={formatDateTime(portal.updatedAt)} detail={portal.warrantyFlag ? 'Warranty work' : 'Non-warranty work'} />
        </section>

        <section className="rounded-[2rem] border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-sm">
          <h2 className="text-xl font-bold text-[var(--color-text-primary)]">Work summary</h2>
          <div className="mt-4 grid gap-4 md:grid-cols-2">
            <SummaryField label="Supplier ref" value={portal.supplierRef} />
            <SummaryField label="Vendor contact" value={portal.vendorContactSnapshot || 'No contact snapshot recorded'} />
            <SummaryField label="Quote ref" value={portal.quoteRecordRef || 'Not captured'} />
            <SummaryField label="Approval ref" value={portal.approvalRef || 'Not captured'} />
            <SummaryField label="Scheduled at" value={formatDateTime(portal.scheduledAt)} />
            <SummaryField label="Completed at" value={formatDateTime(portal.completedAt)} />
            <SummaryField label="Cost estimate" value={portal.costEstimateSnapshot || 'Not captured'} />
            <SummaryField label="Invoice ref" value={portal.invoiceRecordRef || 'Not captured'} />
            <SummaryField label="Portal access" value={portal.portalAccessStatus} />
            <SummaryField label="Allowed actions" value={portal.allowedActions.join(', ')} />
          </div>
          <div className="mt-4 rounded-2xl border border-slate-700 bg-slate-950/40 p-4">
            <p className="text-sm text-[var(--color-text-muted)]">{portal.workDescription || 'No work description was recorded yet.'}</p>
            {portal.notes ? <p className="mt-2 text-sm text-slate-300">{portal.notes}</p> : null}
          </div>
        </section>

        <section className="rounded-[2rem] border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-6 shadow-sm">
          <h2 className="text-xl font-bold text-[var(--color-text-primary)]">Update work status</h2>
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">
            Keep the status current and share any scheduling or completion notes here. Internal staff will review the update in MaintainArr.
          </p>

          <div className="mt-5 grid gap-4 md:grid-cols-2">
            <label className="text-sm text-[var(--color-text-secondary)]">
              Status
              <select
                className="mt-1 block w-full rounded-2xl border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-3 text-[var(--color-text-primary)]"
                value={formState.status}
                onChange={(event) => setFormState({ ...formState, status: event.target.value })}
                disabled={isTerminal}
              >
                {allowedStatusOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </label>

            <label className="text-sm text-[var(--color-text-secondary)]">
              Scheduled at
              <input
                className="mt-1 block w-full rounded-2xl border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-3 text-[var(--color-text-primary)]"
                type="datetime-local"
                value={formState.scheduledAt}
                onChange={(event) => setFormState({ ...formState, scheduledAt: event.target.value })}
                disabled={isTerminal}
              />
            </label>

            <label className="text-sm text-[var(--color-text-secondary)]">
              Completed at
              <input
                className="mt-1 block w-full rounded-2xl border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-3 text-[var(--color-text-primary)]"
                type="datetime-local"
                value={formState.completedAt}
                onChange={(event) => setFormState({ ...formState, completedAt: event.target.value })}
                disabled={isTerminal}
              />
            </label>

            <label className="text-sm text-[var(--color-text-secondary)] md:col-span-2">
              Notes
              <textarea
                className="mt-1 block min-h-32 w-full rounded-2xl border border-[var(--color-border-default)] bg-[var(--color-bg-surface)] px-3 py-3 text-[var(--color-text-primary)]"
                value={formState.notes}
                onChange={(event) => setFormState({ ...formState, notes: event.target.value })}
                disabled={isTerminal}
              />
            </label>
          </div>

          <div className="mt-5 flex flex-wrap items-center gap-3">
            <button
              type="button"
              className="rounded-2xl bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
              disabled={submitMutation.isPending || isTerminal}
              onClick={() => submitMutation.mutate()}
            >
              {submitMutation.isPending ? 'Saving...' : isTerminal ? 'Status locked' : 'Save update'}
            </button>
            <p className="text-sm text-[var(--color-text-muted)]">
              This portal is scoped to the current work order and expires automatically.
            </p>
          </div>
        </section>
      </div>
    </main>
  )
}

function PortalCard({
  label,
  value,
  detail,
}: {
  label: string
  value: string
  detail: string
}) {
  return (
    <div className="rounded-[2rem] border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-sm">
      <p className="text-xs uppercase tracking-[0.25em] text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-lg font-semibold text-[var(--color-text-primary)]">{value}</p>
      <p className="mt-1 text-sm text-[var(--color-text-muted)]">{detail}</p>
    </div>
  )
}

function SummaryField({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-2xl border border-slate-700 bg-slate-950/40 p-4">
      <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-sm text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}
