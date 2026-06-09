import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, Navigate, useParams } from 'react-router-dom'
import {
  archiveMaintenancePart,
  getMaintenancePart,
  getSessionBootstrap,
  updateMaintenancePart,
} from '../../api/client'
import {
  canArchiveParts,
  canUpdateParts,
  loadSession,
} from '../../auth/sessionStorage'
import type { MaintenancePartResponse } from '../../api/types'

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

function toFormState(part: MaintenancePartResponse): PartFormState {
  return {
    partNumber: part.partNumber,
    displayName: part.displayName,
    description: part.description ?? '',
    categoryKey: part.categoryKey,
    unitOfMeasure: part.unitOfMeasure,
    status: part.status,
    sourceType: part.sourceType,
    supplyArrPartId: part.supplyArrPartId ?? '',
    manufacturerName: part.manufacturerName ?? '',
    manufacturerPartNumber: part.manufacturerPartNumber ?? '',
    sdsDocumentId: part.sdsDocumentId ?? '',
    complianceCoreMaterialKey: part.complianceCoreMaterialKey ?? '',
    complianceCoreHazardKeys: part.complianceCoreHazardKeys.join(', '),
    notes: part.notes ?? '',
  }
}

function splitHazardKeys(value: string): string[] {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean)
}

function statusTone(status: string): string {
  switch (status) {
    case 'active':
      return 'bg-emerald-500/15 text-emerald-300 ring-1 ring-inset ring-emerald-400/30'
    case 'draft':
      return 'bg-sky-500/15 text-sky-300 ring-1 ring-inset ring-sky-400/30'
    case 'discontinued':
      return 'bg-rose-500/15 text-rose-300 ring-1 ring-inset ring-rose-400/30'
    default:
      return 'bg-slate-700/70 text-slate-200 ring-1 ring-inset ring-slate-500/50'
  }
}

export function PartDetailPage() {
  const { partId = '' } = useParams()
  const session = loadSession()
  const queryClient = useQueryClient()
  const [isEditing, setIsEditing] = useState(false)
  const [form, setForm] = useState<PartFormState | null>(null)

  const bootstrapQuery = useQuery({
    queryKey: ['maintainarr-session-bootstrap', session?.accessToken],
    enabled: !!session,
    queryFn: () => getSessionBootstrap(session!.accessToken),
  })

  const partQuery = useQuery({
    queryKey: ['maintainarr-part', session?.accessToken, partId],
    enabled: !!session && partId.length > 0,
    queryFn: () => getMaintenancePart(session!.accessToken, partId),
  })

  useEffect(() => {
    if (partQuery.data) {
      setForm(toFormState(partQuery.data))
    }
  }, [partQuery.data])

  const canEdit = bootstrapQuery.data
    ? canUpdateParts(bootstrapQuery.data.tenantRoleKey, bootstrapQuery.data.isPlatformAdmin)
    : false

  const canArchive = bootstrapQuery.data
    ? canArchiveParts(bootstrapQuery.data.tenantRoleKey, bootstrapQuery.data.isPlatformAdmin)
    : false

  const updateMutation = useMutation({
    mutationFn: async () =>
      updateMaintenancePart(session!.accessToken, partId, {
        partNumber: form!.partNumber,
        displayName: form!.displayName,
        description: form!.description || null,
        categoryKey: form!.categoryKey || null,
        unitOfMeasure: form!.unitOfMeasure || null,
        status: form!.status || null,
        sourceType: form!.sourceType || null,
        supplyArrPartId: form!.supplyArrPartId || null,
        manufacturerName: form!.manufacturerName || null,
        manufacturerPartNumber: form!.manufacturerPartNumber || null,
        sdsDocumentId: form!.sdsDocumentId || null,
        complianceCoreMaterialKey: form!.complianceCoreMaterialKey || null,
        complianceCoreHazardKeys: splitHazardKeys(form!.complianceCoreHazardKeys),
        notes: form!.notes || null,
      }),
    onSuccess: async () => {
      setIsEditing(false)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-parts'] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-part', session?.accessToken, partId] })
    },
  })

  const archiveMutation = useMutation({
    mutationFn: async () => archiveMaintenancePart(session!.accessToken, partId),
    onSuccess: async () => {
      setIsEditing(false)
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-parts'] })
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-part', session?.accessToken, partId] })
    },
  })

  const detail = partQuery.data
  const reviewItems = useMemo(() => {
    if (!detail) {
      return []
    }

    return [
      `Source label: ${detail.sourceLabel}`,
      `SupplyArr reference: ${detail.supplyArrPartId ?? 'Not linked'}`,
      `Compliance hazards: ${detail.complianceCoreHazardKeys.length}`,
      `Created: ${new Date(detail.createdAt).toLocaleString()}`,
      `Updated: ${new Date(detail.updatedAt).toLocaleString()}`,
    ]
  }, [detail])

  if (!session) {
    return <Navigate to="/launch" replace />
  }

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6 lg:px-8">
      {partQuery.isLoading ? (
        <div className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6 text-sm text-slate-300">
          Loading maintenance part profile…
        </div>
      ) : null}

      {partQuery.isError ? (
        <div className="rounded-2xl border border-rose-500/30 bg-rose-500/10 p-6 text-sm text-rose-100">
          {partQuery.error instanceof Error
            ? partQuery.error.message
            : 'Failed to load maintenance part profile.'}
        </div>
      ) : null}

      {detail ? (
        <>
          <div className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-lg shadow-slate-950/30">
            <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
              <div className="space-y-3">
                <div className="flex flex-wrap items-center gap-3">
                  <p className="text-xs uppercase tracking-[0.24em] text-sky-300">MaintainArr • Detail</p>
                  <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${statusTone(detail.status)}`}>
                    {detail.status}
                  </span>
                </div>
                <div>
                  <h1 className="text-3xl font-semibold text-white">{detail.displayName}</h1>
                  <p className="mt-2 font-mono text-sm text-sky-300">{detail.partNumber}</p>
                </div>
                <p className="max-w-4xl text-sm text-slate-300">
                  {detail.description || 'No description captured for this maintenance part profile yet.'}
                </p>
                <div className="flex flex-wrap gap-3 text-sm text-slate-300">
                  <span>{detail.categoryKey}</span>
                  <span>{detail.unitOfMeasure}</span>
                  <span>{detail.sourceType === 'supplyarr_snapshot' ? 'SupplyArr snapshot reference' : 'MaintainArr maintenance profile'}</span>
                </div>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <Link to="/parts" className="text-sm text-slate-300 hover:text-white">
                  Back to parts
                </Link>
                {canEdit ? (
                  <button
                    type="button"
                    onClick={() => {
                      setForm(toFormState(detail))
                      setIsEditing((current) => !current)
                    }}
                    className="rounded-md border border-slate-700 px-4 py-2 text-sm text-white hover:border-sky-500"
                  >
                    {isEditing ? 'Cancel edit' : 'Edit profile'}
                  </button>
                ) : null}
                {canArchive && detail.status !== 'inactive' ? (
                  <button
                    type="button"
                    onClick={() => archiveMutation.mutate()}
                    disabled={archiveMutation.isPending}
                    className="rounded-md border border-rose-500/40 px-4 py-2 text-sm text-rose-200 hover:bg-rose-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {archiveMutation.isPending ? 'Archiving…' : 'Archive'}
                  </button>
                ) : null}
              </div>
            </div>
          </div>

          <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_320px]">
            <div className="space-y-6">
              {isEditing && form ? (
                <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
                  <div className="mb-4">
                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Edit mode</p>
                    <h2 className="text-xl font-semibold text-white">Update maintenance part profile</h2>
                  </div>
                  <div className="grid gap-4 md:grid-cols-2">
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Part number</span>
                      <input
                        value={form.partNumber}
                        onChange={(event) => setForm((current) => current ? { ...current, partNumber: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Display name</span>
                      <input
                        value={form.displayName}
                        onChange={(event) => setForm((current) => current ? { ...current, displayName: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Category key</span>
                      <input
                        value={form.categoryKey}
                        onChange={(event) => setForm((current) => current ? { ...current, categoryKey: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Unit of measure</span>
                      <input
                        value={form.unitOfMeasure}
                        onChange={(event) => setForm((current) => current ? { ...current, unitOfMeasure: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Status</span>
                      <select
                        value={form.status}
                        onChange={(event) => setForm((current) => current ? { ...current, status: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      >
                        <option value="draft">Draft</option>
                        <option value="active">Active</option>
                        <option value="inactive">Inactive</option>
                        <option value="discontinued">Discontinued</option>
                      </select>
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Source type</span>
                      <select
                        value={form.sourceType}
                        onChange={(event) => setForm((current) => current ? { ...current, sourceType: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      >
                        <option value="manual">MaintainArr maintenance profile</option>
                        <option value="supplyarr_snapshot">SupplyArr snapshot reference</option>
                      </select>
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>SupplyArr part ID</span>
                      <input
                        value={form.supplyArrPartId}
                        onChange={(event) => setForm((current) => current ? { ...current, supplyArrPartId: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 font-mono text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Manufacturer name</span>
                      <input
                        value={form.manufacturerName}
                        onChange={(event) => setForm((current) => current ? { ...current, manufacturerName: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Manufacturer part number</span>
                      <input
                        value={form.manufacturerPartNumber}
                        onChange={(event) => setForm((current) => current ? { ...current, manufacturerPartNumber: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>SDS document ID</span>
                      <input
                        value={form.sdsDocumentId}
                        onChange={(event) => setForm((current) => current ? { ...current, sdsDocumentId: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                    <label className="space-y-2 text-sm text-slate-200">
                      <span>Compliance Core material key</span>
                      <input
                        value={form.complianceCoreMaterialKey}
                        onChange={(event) => setForm((current) => current ? { ...current, complianceCoreMaterialKey: event.target.value } : current)}
                        className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                      />
                    </label>
                  </div>
                  <label className="mt-4 block space-y-2 text-sm text-slate-200">
                    <span>Hazard keys</span>
                    <input
                      value={form.complianceCoreHazardKeys}
                      onChange={(event) => setForm((current) => current ? { ...current, complianceCoreHazardKeys: event.target.value } : current)}
                      className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                    />
                  </label>
                  <label className="mt-4 block space-y-2 text-sm text-slate-200">
                    <span>Description</span>
                    <textarea
                      value={form.description}
                      onChange={(event) => setForm((current) => current ? { ...current, description: event.target.value } : current)}
                      rows={4}
                      className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                    />
                  </label>
                  <label className="mt-4 block space-y-2 text-sm text-slate-200">
                    <span>Notes</span>
                    <textarea
                      value={form.notes}
                      onChange={(event) => setForm((current) => current ? { ...current, notes: event.target.value } : current)}
                      rows={3}
                      className="w-full rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-white focus:border-sky-500"
                    />
                  </label>
                  {updateMutation.isError ? (
                    <p className="mt-4 text-sm text-rose-200">
                      {updateMutation.error instanceof Error
                        ? updateMutation.error.message
                        : 'Failed to update maintenance part profile.'}
                    </p>
                  ) : null}
                  <div className="mt-6 flex flex-wrap gap-3">
                    <button
                      type="button"
                      onClick={() => updateMutation.mutate()}
                      disabled={updateMutation.isPending}
                      className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      {updateMutation.isPending ? 'Saving…' : 'Save changes'}
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setForm(toFormState(detail))
                        setIsEditing(false)
                      }}
                      className="rounded-md border border-slate-700 px-4 py-2 text-sm text-white hover:border-slate-500"
                    >
                      Cancel
                    </button>
                  </div>
                </section>
              ) : (
                <>
                  <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Overview</p>
                    <dl className="mt-4 grid gap-4 md:grid-cols-2">
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Category</dt>
                        <dd className="mt-1 text-sm text-white">{detail.categoryKey}</dd>
                      </div>
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Unit of measure</dt>
                        <dd className="mt-1 text-sm text-white">{detail.unitOfMeasure}</dd>
                      </div>
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Manufacturer</dt>
                        <dd className="mt-1 text-sm text-white">{detail.manufacturerName ?? 'Not captured'}</dd>
                      </div>
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Manufacturer part number</dt>
                        <dd className="mt-1 text-sm text-white">{detail.manufacturerPartNumber ?? 'Not captured'}</dd>
                      </div>
                    </dl>
                  </section>

                  <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Source of truth</p>
                    <div className="mt-4 grid gap-4 md:grid-cols-2">
                      <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                        <p className="text-xs uppercase tracking-[0.18em] text-slate-500">Owned here</p>
                        <p className="mt-2 text-sm text-slate-200">
                          Maintenance applicability, maintenance-facing description, and compliance snapshot references.
                        </p>
                      </div>
                      <div className="rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                        <p className="text-xs uppercase tracking-[0.18em] text-slate-500">Referenced externally</p>
                        <p className="mt-2 text-sm text-slate-200">
                          SupplyArr part master, supplier links, pricing snapshots, availability, quotes, and orders.
                        </p>
                      </div>
                    </div>
                    <dl className="mt-4 grid gap-4 md:grid-cols-2">
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Source label</dt>
                        <dd className="mt-1 text-sm text-white">{detail.sourceLabel}</dd>
                      </div>
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">SupplyArr part ID</dt>
                        <dd className="mt-1 font-mono text-sm text-white">{detail.supplyArrPartId ?? 'Not linked'}</dd>
                      </div>
                    </dl>
                  </section>

                  <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
                    <p className="text-xs uppercase tracking-[0.2em] text-slate-500">Compliance snapshot</p>
                    <dl className="mt-4 grid gap-4 md:grid-cols-2">
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">SDS document</dt>
                        <dd className="mt-1 text-sm text-white">{detail.sdsDocumentId ?? 'Not linked'}</dd>
                      </div>
                      <div>
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Material key</dt>
                        <dd className="mt-1 text-sm text-white">{detail.complianceCoreMaterialKey ?? 'Not linked'}</dd>
                      </div>
                      <div className="md:col-span-2">
                        <dt className="text-xs uppercase tracking-[0.18em] text-slate-500">Hazard keys</dt>
                        <dd className="mt-1 text-sm text-white">
                          {detail.complianceCoreHazardKeys.length > 0
                            ? detail.complianceCoreHazardKeys.join(', ')
                            : 'No hazard keys captured'}
                        </dd>
                      </div>
                    </dl>
                    {detail.notes ? (
                      <div className="mt-4 rounded-xl border border-slate-800 bg-slate-900/60 p-4">
                        <p className="text-xs uppercase tracking-[0.18em] text-slate-500">Notes</p>
                        <p className="mt-2 text-sm text-slate-200">{detail.notes}</p>
                      </div>
                    ) : null}
                  </section>
                </>
              )}
            </div>

            <aside className="space-y-6">
              <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-6">
                <p className="text-xs uppercase tracking-[0.22em] text-slate-500">Decision panel</p>
                <h2 className="mt-2 text-xl font-semibold text-white">Current guidance</h2>
                <ul className="mt-4 space-y-3 text-sm text-slate-300">
                  {reviewItems.map((item) => (
                    <li key={item} className="rounded-xl border border-slate-800 bg-slate-900/60 px-3 py-2">
                      {item}
                    </li>
                  ))}
                </ul>
                <div className="mt-4 rounded-xl border border-sky-500/20 bg-sky-500/10 p-4 text-sm text-sky-100">
                  Use this profile for maintenance approvals, kits, and work-order demand references. Supplier and
                  procurement actions still belong in SupplyArr.
                </div>
              </section>
            </aside>
          </div>
        </>
      ) : null}
    </div>
  )
}
