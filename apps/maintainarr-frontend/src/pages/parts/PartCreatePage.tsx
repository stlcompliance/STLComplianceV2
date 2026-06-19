import { useMemo, useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { createMaintenancePart, getSessionBootstrap } from '../../api/client'
import { canCreateParts, loadSession } from '../../auth/sessionStorage'

type PartFormState = {
  partNumber: string
  displayName: string
  description: string
  categoryKey: string
  unitOfMeasure: string
  status: string
  sourceType: string
  supplyArrPartId: string
  manufacturerName: string
  manufacturerPartNumber: string
  sdsDocumentId: string
  complianceCoreMaterialKey: string
  complianceCoreHazardKeys: string
  notes: string
}

const initialFormState: PartFormState = {
  partNumber: '',
  displayName: '',
  description: '',
  categoryKey: 'maintenance',
  unitOfMeasure: 'each',
  status: 'active',
  sourceType: 'manual',
  supplyArrPartId: '',
  manufacturerName: '',
  manufacturerPartNumber: '',
  sdsDocumentId: '',
  complianceCoreMaterialKey: '',
  complianceCoreHazardKeys: '',
  notes: '',
}

function splitHazardKeys(value: string): string[] {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

export function PartCreatePage() {
  const navigate = useNavigate()
  const session = loadSession()
  const [form, setForm] = useState<PartFormState>(initialFormState)

  const bootstrapQuery = useQuery({
    queryKey: ['maintainarr-session-bootstrap', session?.accessToken],
    enabled: !!session,
    queryFn: () => getSessionBootstrap(session!.accessToken),
  })

  const createMutation = useMutation({
    mutationFn: async () =>
      createMaintenancePart(session!.accessToken, {
        partNumber: form.partNumber,
        displayName: form.displayName,
        description: form.description || null,
        categoryKey: form.categoryKey || null,
        unitOfMeasure: form.unitOfMeasure || null,
        status: form.status || null,
        sourceType: form.sourceType || null,
        supplyArrPartId: form.supplyArrPartId || null,
        manufacturerName: form.manufacturerName || null,
        manufacturerPartNumber: form.manufacturerPartNumber || null,
        sdsDocumentId: form.sdsDocumentId || null,
        complianceCoreMaterialKey: form.complianceCoreMaterialKey || null,
        complianceCoreHazardKeys: splitHazardKeys(form.complianceCoreHazardKeys),
        notes: form.notes || null,
      }),
    onSuccess: (created) => {
      navigate(`/parts/${created.partId}`)
    },
  })

  const canCreate = bootstrapQuery.data
    ? canCreateParts(bootstrapQuery.data.tenantRoleKey, bootstrapQuery.data.isPlatformAdmin)
    : false

  const identityComplete = useMemo(
    () => form.partNumber.trim().length > 0 && form.displayName.trim().length > 0,
    [form.displayName, form.partNumber],
  )

  const detailsComplete = useMemo(
    () => identityComplete && form.categoryKey.trim().length > 0 && form.unitOfMeasure.trim().length > 0,
    [form.categoryKey, form.unitOfMeasure, identityComplete],
  )

  const reviewItems = useMemo(
    () => [
      `Part number: ${form.partNumber || 'Required'}`,
      `Display name: ${form.displayName || 'Required'}`,
      `Source: ${form.sourceType === 'supplyarr_snapshot' ? 'SupplyArr snapshot reference' : 'MaintainArr maintenance profile'}`,
      `SupplyArr part ID: ${form.supplyArrPartId || 'Not linked'}`,
      `Compliance keys: ${splitHazardKeys(form.complianceCoreHazardKeys).length || 0} hazard key(s)`,
    ],
    [form],
  )

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
      <div className="flex flex-col gap-3 rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-lg shadow-slate-950/30">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="text-xs uppercase tracking-[0.24em] text-sky-300">MaintainArr • Create</p>
            <h1 className="mt-2 text-3xl font-semibold text-white">Maintenance part profile</h1>
          </div>
          <Link to="/parts" className="text-sm text-slate-300 hover:text-white">
            Back to parts
          </Link>
        </div>
        <p className="max-w-4xl text-sm text-slate-300">
          This flow creates a MaintainArr-owned maintenance part profile. SupplyArr remains the source of truth
          for canonical supplier part master, pricing, vendor links, and procurement.
        </p>
      </div>

      {!bootstrapQuery.isLoading && !canCreate ? (
        <div className="rounded-2xl border border-amber-500/30 bg-amber-500/10 p-6 text-sm text-amber-100">
          You do not have permission to create maintenance part profiles in MaintainArr.
        </div>
      ) : null}

      {bootstrapQuery.isError ? (
        <div className="rounded-2xl border border-rose-500/30 bg-rose-500/10 p-6 text-sm text-rose-100">
          {bootstrapQuery.error instanceof Error
            ? bootstrapQuery.error.message
            : 'Failed to verify Create Parts permissions.'}
        </div>
      ) : null}

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]">
        <div className="space-y-6">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
            <div className="mb-4 flex items-center justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">Step 1</p>
                <h2 className="text-xl font-semibold text-white">Identity and source</h2>
              </div>
              <span className="rounded-full bg-slate-800 px-2.5 py-1 text-xs text-slate-200">
                {identityComplete ? 'Complete' : 'Needs required fields'}
              </span>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <label className="space-y-2 text-sm text-slate-200">
                <span>Part number</span>
                <input
                  value={form.partNumber}
                  onChange={(event) => setForm((current) => ({ ...current, partNumber: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Display name</span>
                <input
                  value={form.displayName}
                  onChange={(event) => setForm((current) => ({ ...current, displayName: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Source type</span>
                <select
                  value={form.sourceType}
                  onChange={(event) => setForm((current) => ({ ...current, sourceType: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                >
                  <option value="manual">MaintainArr maintenance profile</option>
                  <option value="supplyarr_snapshot">SupplyArr snapshot reference</option>
                </select>
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>SupplyArr part ID (optional)</span>
                <input
                  value={form.supplyArrPartId}
                  onChange={(event) => setForm((current) => ({ ...current, supplyArrPartId: event.target.value }))}
                  placeholder="Stable external part ID"
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-white focus:border-sky-500"
                />
              </label>
            </div>
            <p className="mt-4 text-sm text-slate-400">
              Cross-product references are stored as stable IDs and labeled snapshots. MaintainArr does not become
              the canonical supplier part master.
            </p>
          </section>

          <section className={`rounded-2xl border p-6 ${identityComplete ? 'border-slate-800 bg-slate-950/70' : 'border-slate-900 bg-slate-950/40 opacity-70'}`}>
            <div className="mb-4 flex items-center justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">Step 2</p>
                <h2 className="text-xl font-semibold text-white">Classification and snapshot</h2>
              </div>
              <span className="rounded-full bg-slate-800 px-2.5 py-1 text-xs text-slate-200">
                {!identityComplete ? 'Locked' : detailsComplete ? 'Complete' : 'In progress'}
              </span>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <label className="space-y-2 text-sm text-slate-200">
                <span>Category key</span>
                <input
                  disabled={!identityComplete}
                  value={form.categoryKey}
                  onChange={(event) => setForm((current) => ({ ...current, categoryKey: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Unit of measure</span>
                <input
                  disabled={!identityComplete}
                  value={form.unitOfMeasure}
                  onChange={(event) => setForm((current) => ({ ...current, unitOfMeasure: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Manufacturer name</span>
                <input
                  disabled={!identityComplete}
                  value={form.manufacturerName}
                  onChange={(event) => setForm((current) => ({ ...current, manufacturerName: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Manufacturer part number</span>
                <input
                  disabled={!identityComplete}
                  value={form.manufacturerPartNumber}
                  onChange={(event) => setForm((current) => ({ ...current, manufacturerPartNumber: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
            </div>
            <label className="mt-4 block space-y-2 text-sm text-slate-200">
              <span>Description</span>
              <textarea
                disabled={!identityComplete}
                value={form.description}
                onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
                rows={4}
                className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
              />
            </label>
            <label className="mt-4 block space-y-2 text-sm text-slate-200">
              <span>Notes</span>
              <textarea
                disabled={!identityComplete}
                value={form.notes}
                onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))}
                rows={3}
                className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
              />
            </label>
          </section>

          <section className={`rounded-2xl border p-6 ${detailsComplete ? 'border-slate-800 bg-slate-950/70' : 'border-slate-900 bg-slate-950/40 opacity-70'}`}>
            <div className="mb-4 flex items-center justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.2em] text-[var(--color-text-muted)]">Step 3</p>
                <h2 className="text-xl font-semibold text-white">Compliance and review</h2>
              </div>
              <span className="rounded-full bg-slate-800 px-2.5 py-1 text-xs text-slate-200">
                {!detailsComplete ? 'Locked' : 'Ready for review'}
              </span>
            </div>
            <div className="grid gap-4 md:grid-cols-2">
              <label className="space-y-2 text-sm text-slate-200">
                <span>SDS document ID</span>
                <input
                  disabled={!detailsComplete}
                  value={form.sdsDocumentId}
                  onChange={(event) => setForm((current) => ({ ...current, sdsDocumentId: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
              <label className="space-y-2 text-sm text-slate-200">
                <span>Compliance Core material key</span>
                <input
                  disabled={!detailsComplete}
                  value={form.complianceCoreMaterialKey}
                  onChange={(event) => setForm((current) => ({ ...current, complianceCoreMaterialKey: event.target.value }))}
                  className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
                />
              </label>
            </div>
            <label className="mt-4 block space-y-2 text-sm text-slate-200">
              <span>Compliance Core hazard keys</span>
              <input
                disabled={!detailsComplete}
                value={form.complianceCoreHazardKeys}
                onChange={(event) => setForm((current) => ({ ...current, complianceCoreHazardKeys: event.target.value }))}
                placeholder="flammable, corrosive, respiratory"
                className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white disabled:cursor-not-allowed disabled:opacity-60 focus:border-sky-500"
              />
            </label>
          </section>
        </div>

        <aside className="space-y-6">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
            <p className="text-xs uppercase tracking-[0.22em] text-[var(--color-text-muted)]">Review & submit</p>
            <h2 className="mt-2 text-xl font-semibold text-white">What will be created</h2>
            <ul className="mt-4 space-y-3 text-sm text-slate-300">
              {reviewItems.map((item) => (
                <li key={item} className="rounded-xl border border-slate-800 bg-slate-900/60 px-3 py-2">
                  {item}
                </li>
              ))}
            </ul>
            <div className="mt-6 rounded-xl border border-sky-500/20 bg-sky-500/10 p-4 text-sm text-sky-100">
              Saving this record creates a MaintainArr maintenance part profile only. It does not create a SupplyArr
              canonical part, vendor link, quote, or order.
            </div>
            {createMutation.isError ? (
              <p className="mt-4 text-sm text-rose-200">
                {createMutation.error instanceof Error
                  ? createMutation.error.message
                  : 'Failed to create maintenance part profile.'}
              </p>
            ) : null}
            <div className="mt-6 flex flex-col gap-3">
              <button
                type="button"
                disabled={!detailsComplete || createMutation.isPending || !canCreate}
                onClick={() => createMutation.mutate()}
                className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-60 hover:bg-sky-500"
              >
                {createMutation.isPending ? 'Creating part profile…' : 'Create maintenance part profile'}
              </button>
              <Link to="/parts" className="text-sm text-slate-300 hover:text-white">
                Cancel
              </Link>
            </div>
          </section>
        </aside>
      </div>
    </div>
  )
}
