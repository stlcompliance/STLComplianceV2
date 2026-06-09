import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import {
  createMaintenancePartsKit,
  createMaintenancePartsKitLine,
  deleteMaintenancePartsKitLine,
  getMaintenancePartsKit,
  getMaintenancePartsKits,
  updateMaintenancePartsKit,
  updateMaintenancePartsKitLine,
  updateMaintenancePartsKitStatus,
} from '../api/client'

interface MaintenancePartsKitsPanelProps {
  accessToken: string
  canManage: boolean
}

const STATUS_OPTIONS = ['draft', 'active', 'retired'] as const

function splitLines(value: string): string[] {
  return value
    .split(/\r?\n|,/)
    .map((item) => item.trim())
    .filter(Boolean)
}

function joinLines(values: string[]): string {
  return values.join('\n')
}

function statusClass(status: string): string {
  switch (status) {
    case 'active':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'retired':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    default:
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
  }
}

function chipClass(): string {
  return 'rounded-full border border-slate-700 bg-slate-950/80 px-2 py-0.5 text-[11px] text-slate-300'
}

export function MaintenancePartsKitsPanel({ accessToken, canManage }: MaintenancePartsKitsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedPartsKitId, setSelectedPartsKitId] = useState('')
  const [kitNumber, setKitNumber] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [assetApplicability, setAssetApplicability] = useState('')
  const [workOrderApplicability, setWorkOrderApplicability] = useState('')
  const [pmPlanRef, setPmPlanRef] = useState('')
  const [status, setStatus] = useState<'draft' | 'active' | 'retired'>('draft')
  const [selectedLineId, setSelectedLineId] = useState('')
  const [lineItemRef, setLineItemRef] = useState('')
  const [lineDescription, setLineDescription] = useState('')
  const [lineQuantity, setLineQuantity] = useState('1')
  const [lineUnitOfMeasure, setLineUnitOfMeasure] = useState('each')
  const [lineRequired, setLineRequired] = useState(true)
  const [lineSubstituteAllowed, setLineSubstituteAllowed] = useState(false)

  const kitsQuery = useQuery({
    queryKey: ['maintainarr-parts-kits', accessToken],
    queryFn: () => getMaintenancePartsKits(accessToken),
    enabled: Boolean(accessToken),
  })

  const selectedKitQuery = useQuery({
    queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId],
    queryFn: () => getMaintenancePartsKit(accessToken, selectedPartsKitId),
    enabled: Boolean(accessToken && selectedPartsKitId),
  })

  useEffect(() => {
    const initialKit = kitsQuery.data?.items[0]
    if (!selectedPartsKitId && initialKit) {
      setSelectedPartsKitId(initialKit.partsKitId)
    }
  }, [kitsQuery.data?.items, selectedPartsKitId])

  useEffect(() => {
    const kit = selectedKitQuery.data
    if (!kit) {
      return
    }
    setKitNumber(kit.kitNumber)
    setTitle(kit.title)
    setDescription(kit.description)
    setAssetApplicability(joinLines(kit.assetTypeApplicability))
    setWorkOrderApplicability(joinLines(kit.workOrderTypeApplicability))
    setPmPlanRef(kit.pmPlanRef ?? '')
    setStatus(kit.status as 'draft' | 'active' | 'retired')
    setSelectedLineId('')
    setLineItemRef('')
    setLineDescription('')
    setLineQuantity('1')
    setLineUnitOfMeasure('each')
    setLineRequired(true)
    setLineSubstituteAllowed(false)
  }, [selectedKitQuery.data])

  useEffect(() => {
    if (selectedPartsKitId) {
      return
    }
    setKitNumber('')
    setTitle('')
    setDescription('')
    setAssetApplicability('')
    setWorkOrderApplicability('')
    setPmPlanRef('')
    setStatus('draft')
    setSelectedLineId('')
    setLineItemRef('')
    setLineDescription('')
    setLineQuantity('1')
    setLineUnitOfMeasure('each')
    setLineRequired(true)
    setLineSubstituteAllowed(false)
  }, [selectedPartsKitId])

  const selectedKit = selectedKitQuery.data ?? null
  const lines = selectedKit?.lines ?? []
  const selectedLine = useMemo(
    () => lines.find((line) => line.partsKitLineId === selectedLineId) ?? null,
    [lines, selectedLineId],
  )

  useEffect(() => {
    if (!selectedLine) {
      return
    }
    setLineItemRef(selectedLine.itemRef)
    setLineDescription(selectedLine.itemDescriptionSnapshot)
    setLineQuantity(String(selectedLine.quantity))
    setLineUnitOfMeasure(selectedLine.unitOfMeasure)
    setLineRequired(selectedLine.required)
    setLineSubstituteAllowed(selectedLine.substituteAllowed)
  }, [selectedLine])

  useEffect(() => {
    if (selectedLineId) {
      return
    }
    setLineItemRef('')
    setLineDescription('')
    setLineQuantity('1')
    setLineUnitOfMeasure('each')
    setLineRequired(true)
    setLineSubstituteAllowed(false)
  }, [selectedLineId])

  const createKitMutation = useMutation({
    mutationFn: () =>
      createMaintenancePartsKit(accessToken, {
        kitNumber,
        title,
        description,
        assetTypeApplicability: splitLines(assetApplicability),
        workOrderTypeApplicability: splitLines(workOrderApplicability),
        pmPlanRef: pmPlanRef.trim() || null,
      }),
    onSuccess: async (created) => {
      setSelectedPartsKitId(created.partsKitId)
      setKitNumber('')
      setTitle('')
      setDescription('')
      setAssetApplicability('')
      setWorkOrderApplicability('')
      setPmPlanRef('')
      await queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] })
    },
  })

  const updateKitMutation = useMutation({
    mutationFn: () =>
      updateMaintenancePartsKit(accessToken, selectedPartsKitId, {
        title,
        description,
        assetTypeApplicability: splitLines(assetApplicability),
        workOrderTypeApplicability: splitLines(workOrderApplicability),
        pmPlanRef: pmPlanRef.trim() || null,
      }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId] }),
      ])
    },
  })

  const statusMutation = useMutation({
    mutationFn: () => updateMaintenancePartsKitStatus(accessToken, selectedPartsKitId, { status }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId] }),
      ])
    },
  })

  const createLineMutation = useMutation({
    mutationFn: () =>
      createMaintenancePartsKitLine(accessToken, selectedPartsKitId, {
        itemRef: lineItemRef,
        itemDescriptionSnapshot: lineDescription,
        quantity: Number(lineQuantity),
        unitOfMeasure: lineUnitOfMeasure,
        required: lineRequired,
        substituteAllowed: lineSubstituteAllowed,
      }),
    onSuccess: async () => {
      setSelectedLineId('')
      setLineItemRef('')
      setLineDescription('')
      setLineQuantity('1')
      setLineUnitOfMeasure('each')
      setLineRequired(true)
      setLineSubstituteAllowed(false)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId] }),
      ])
    },
  })

  const updateLineMutation = useMutation({
    mutationFn: () =>
      updateMaintenancePartsKitLine(accessToken, selectedPartsKitId, selectedLineId, {
        itemDescriptionSnapshot: lineDescription,
        quantity: Number(lineQuantity),
        unitOfMeasure: lineUnitOfMeasure,
        required: lineRequired,
        substituteAllowed: lineSubstituteAllowed,
      }),
    onSuccess: async () => {
      setSelectedLineId('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId] }),
      ])
    },
  })

  const deleteLineMutation = useMutation({
    mutationFn: (lineId: string) => deleteMaintenancePartsKitLine(accessToken, selectedPartsKitId, lineId),
    onSuccess: async () => {
      setSelectedLineId('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kits', accessToken] }),
        queryClient.invalidateQueries({ queryKey: ['maintainarr-parts-kit', accessToken, selectedPartsKitId] }),
      ])
    },
  })

  const isEditingLine = Boolean(selectedLineId)

  return (
    <section className="space-y-6" data-testid="maintenance-parts-kits-panel">
      <header className="rounded-2xl border border-slate-800 bg-slate-950/70 p-5 shadow-lg shadow-black/20">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h2 className="text-xl font-semibold text-white">Maintenance parts kits</h2>
            <p className="mt-1 text-sm text-slate-400">
              Curated parts kits stay in MaintainArr as reusable maintenance execution guidance.
            </p>
          </div>
          <div className="text-xs text-slate-500">
            {kitsQuery.data?.items.length ?? 0} kit(s) · {selectedKit?.lines.length ?? 0} line(s)
          </div>
          <Link
            to="/parts-kits/create"
            className="rounded-full border border-sky-500/40 bg-sky-500/10 px-3 py-1.5 text-xs font-medium text-sky-100 hover:bg-sky-500/20"
          >
            Guided create
          </Link>
        </div>
      </header>

      <div className="grid gap-6 lg:grid-cols-[320px_minmax(0,1fr)]">
        <aside className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
          <h3 className="text-sm font-semibold text-slate-100">Kit registry</h3>
          {kitsQuery.isLoading ? (
            <p className="mt-3 text-sm text-slate-400">Loading parts kits…</p>
          ) : (kitsQuery.data?.items.length ?? 0) === 0 ? (
            <p className="mt-3 text-sm text-slate-400">No parts kits yet.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {kitsQuery.data?.items.map((kit) => (
                <li key={kit.partsKitId}>
                  <button
                    type="button"
                    className={`w-full rounded-xl border px-3 py-2 text-left transition ${
                      selectedPartsKitId === kit.partsKitId
                        ? 'border-amber-500/60 bg-amber-500/10'
                        : 'border-slate-800 bg-slate-900/40 hover:border-slate-700 hover:bg-slate-900/70'
                    }`}
                    onClick={() => setSelectedPartsKitId(kit.partsKitId)}
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <div className="font-mono text-sm text-amber-200">{kit.kitNumber}</div>
                        <div className="text-sm text-white">{kit.title}</div>
                      </div>
                      <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusClass(kit.status)}`}>
                        {kit.status}
                      </span>
                    </div>
                    <div className="mt-2 flex flex-wrap gap-1">
                      {kit.assetTypeApplicability.slice(0, 3).map((item) => (
                        <span key={item} className={chipClass()}>
                          {item}
                        </span>
                      ))}
                    </div>
                  </button>
                </li>
              ))}
            </ul>
          )}
        </aside>

        <div className="space-y-6">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
            <h3 className="text-sm font-semibold text-slate-100">
              {selectedPartsKitId ? 'Edit kit' : 'Create kit'}
            </h3>
            <div className="mt-4 grid gap-4 md:grid-cols-2">
              <label className="block text-xs text-slate-400">
                Kit number
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={kitNumber}
                  onChange={(event) => setKitNumber(event.target.value)}
                  disabled={!canManage || Boolean(selectedPartsKitId)}
                />
              </label>
              <label className="block text-xs text-slate-400">
                Title
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={title}
                  onChange={(event) => setTitle(event.target.value)}
                  disabled={!canManage}
                />
              </label>
              <label className="block text-xs text-slate-400 md:col-span-2">
                Description
                <textarea
                  className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={description}
                  onChange={(event) => setDescription(event.target.value)}
                  disabled={!canManage}
                />
              </label>
              <label className="block text-xs text-slate-400">
                Asset type applicability
                <textarea
                  className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={assetApplicability}
                  onChange={(event) => setAssetApplicability(event.target.value)}
                  placeholder="One per line or comma-separated"
                  disabled={!canManage}
                />
              </label>
              <label className="block text-xs text-slate-400">
                Work order type applicability
                <textarea
                  className="mt-1 min-h-20 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={workOrderApplicability}
                  onChange={(event) => setWorkOrderApplicability(event.target.value)}
                  placeholder="One per line or comma-separated"
                  disabled={!canManage}
                />
              </label>
              <label className="block text-xs text-slate-400">
                PM plan ref
                <input
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={pmPlanRef}
                  onChange={(event) => setPmPlanRef(event.target.value)}
                  disabled={!canManage}
                />
              </label>
              <label className="block text-xs text-slate-400">
                Status
                <select
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={status}
                  onChange={(event) => setStatus(event.target.value as typeof status)}
                  disabled={!canManage}
                >
                  {STATUS_OPTIONS.map((option) => (
                    <option key={option} value={option}>
                      {option}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            {canManage ? (
              <div className="mt-4 flex flex-wrap gap-2">
                {!selectedPartsKitId ? (
                  <button
                    type="button"
                    className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                    disabled={createKitMutation.isPending}
                    onClick={() => createKitMutation.mutate()}
                  >
                    Create kit
                  </button>
                ) : (
                  <>
                    <button
                      type="button"
                      className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                      disabled={updateKitMutation.isPending}
                      onClick={() => updateKitMutation.mutate()}
                    >
                      Save kit
                    </button>
                    <button
                      type="button"
                      className="rounded bg-amber-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                      disabled={statusMutation.isPending}
                      onClick={() => statusMutation.mutate()}
                    >
                      Update status
                    </button>
                  </>
                )}
              </div>
            ) : null}
          </section>

          {selectedKit ? (
            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold text-slate-100">Kit detail</h3>
                  <p className="mt-1 text-sm text-slate-400">
                    {selectedKit.kitNumber} · {selectedKit.lines.length} line(s)
                  </p>
                </div>
                <button
                  type="button"
                  className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-600 hover:bg-slate-900/70"
                  onClick={() => setSelectedPartsKitId('')}
                >
                  Clear selection
                </button>
              </div>

              <div className="mt-4 grid gap-4 lg:grid-cols-2">
                <div>
                  <h4 className="text-xs uppercase tracking-wide text-slate-500">Applicability</h4>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {selectedKit.assetTypeApplicability.length === 0 ? (
                      <span className="text-sm text-slate-400">No asset applicability set.</span>
                    ) : (
                      selectedKit.assetTypeApplicability.map((item) => (
                        <span key={item} className={chipClass()}>
                          Asset: {item}
                        </span>
                      ))
                    )}
                  </div>
                </div>
                <div>
                  <h4 className="text-xs uppercase tracking-wide text-slate-500">Work order types</h4>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {selectedKit.workOrderTypeApplicability.length === 0 ? (
                      <span className="text-sm text-slate-400">No work-order applicability set.</span>
                    ) : (
                      selectedKit.workOrderTypeApplicability.map((item) => (
                        <span key={item} className={chipClass()}>
                          WO: {item}
                        </span>
                      ))
                    )}
                  </div>
                </div>
              </div>

              <div className="mt-6 border-t border-slate-800 pt-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <h4 className="text-sm font-semibold text-slate-100">Kit lines</h4>
                  <div className="text-xs text-slate-500">Line refs are maintained by MaintainArr</div>
                </div>
                {lines.length === 0 ? (
                  <p className="mt-3 text-sm text-slate-400">No lines defined yet.</p>
                ) : (
                  <ul className="mt-3 space-y-2">
                    {lines.map((line) => (
                      <li
                        key={line.partsKitLineId}
                        className={`rounded-xl border px-3 py-2 ${
                          selectedLineId === line.partsKitLineId
                            ? 'border-amber-500/60 bg-amber-500/10'
                            : 'border-slate-800 bg-slate-900/40'
                        }`}
                      >
                        <div className="flex flex-wrap items-start justify-between gap-3">
                          <button
                            type="button"
                            className="text-left"
                            onClick={() => setSelectedLineId(line.partsKitLineId)}
                          >
                            <div className="font-medium text-white">{line.itemRef}</div>
                            <div className="text-sm text-slate-300">{line.itemDescriptionSnapshot}</div>
                          </button>
                          <div className="flex flex-wrap gap-2">
                            <span className={chipClass()}>
                              {line.quantity} {line.unitOfMeasure}
                            </span>
                            <span className={chipClass()}>{line.required ? 'required' : 'optional'}</span>
                            <span className={chipClass()}>
                              {line.substituteAllowed ? 'substitute ok' : 'no substitute'}
                            </span>
                            {canManage ? (
                              <button
                                type="button"
                                className="rounded border border-slate-700 px-2 py-1 text-xs text-slate-200 hover:border-slate-600"
                                onClick={() => deleteLineMutation.mutate(line.partsKitLineId)}
                              >
                                Delete
                              </button>
                            ) : null}
                          </div>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              {canManage ? (
                <div className="mt-6 border-t border-slate-800 pt-4">
                  <h4 className="text-sm font-semibold text-slate-100">
                    {isEditingLine ? 'Edit line' : 'Add line'}
                  </h4>
                  <div className="mt-4 grid gap-4 md:grid-cols-2">
                    <label className="block text-xs text-slate-400">
                      Item ref
                      <input
                        className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                        value={lineItemRef}
                        onChange={(event) => setLineItemRef(event.target.value)}
                        disabled={isEditingLine}
                      />
                    </label>
                    <label className="block text-xs text-slate-400">
                      Description snapshot
                      <input
                        className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                        value={lineDescription}
                        onChange={(event) => setLineDescription(event.target.value)}
                      />
                    </label>
                    <label className="block text-xs text-slate-400">
                      Quantity
                      <input
                        className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                        value={lineQuantity}
                        onChange={(event) => setLineQuantity(event.target.value)}
                      />
                    </label>
                    <label className="block text-xs text-slate-400">
                      Unit of measure
                      <input
                        className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                        value={lineUnitOfMeasure}
                        onChange={(event) => setLineUnitOfMeasure(event.target.value)}
                      />
                    </label>
                  </div>
                  <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-300">
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={lineRequired}
                        onChange={(event) => setLineRequired(event.target.checked)}
                      />
                      Required
                    </label>
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={lineSubstituteAllowed}
                        onChange={(event) => setLineSubstituteAllowed(event.target.checked)}
                      />
                      Substitute allowed
                    </label>
                  </div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <button
                      type="button"
                      className="rounded bg-sky-600 px-3 py-2 text-sm text-white disabled:opacity-50"
                      disabled={createLineMutation.isPending || updateLineMutation.isPending}
                      onClick={() => {
                        if (isEditingLine) {
                          updateLineMutation.mutate()
                          return
                        }
                        createLineMutation.mutate()
                      }}
                    >
                      {isEditingLine ? 'Save line' : 'Add line'}
                    </button>
                    {isEditingLine ? (
                      <button
                        type="button"
                        className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:border-slate-600"
                        onClick={() => setSelectedLineId('')}
                      >
                        Cancel edit
                      </button>
                    ) : null}
                  </div>
                </div>
              ) : null}
            </section>
          ) : (
            <section className="rounded-2xl border border-slate-800 bg-slate-950/70 p-4">
              <p className="text-sm text-slate-400">
                Select a kit to inspect its lines, or create a new one if you have Manage access.
              </p>
            </section>
          )}
        </div>
      </div>
    </section>
  )
}
