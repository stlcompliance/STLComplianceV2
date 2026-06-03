import { ControlledSelect } from '@stl/shared-ui'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'

import {
  createOutboundShipment,
  getOutboundShipments,
} from '../api/client'
import type {
  CreateOutboundShipmentLineRequest,
  InventoryBinResponse,
  OutboundShipmentResponse,
  PartResponse,
} from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import { toBinPickerOptions, toPartPickerOptions } from '../forms/controlledFormHelpers'

interface OutboundShipmentsPanelProps {
  accessToken: string
  parts: PartResponse[]
  bins: InventoryBinResponse[]
  canManage: boolean
}

type ShipmentLineDraft = {
  id: string
  partId: string
  fromBinId: string
  quantity: string
}

function formatTimestamp(value: string | null | undefined): string | null {
  if (!value) {
    return null
  }

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return null
  }

  return date.toLocaleString()
}

function buildLineSummary(parts: PartResponse[], bins: InventoryBinResponse[], line: ShipmentLineDraft): string {
  const part = parts.find((item) => item.partId === line.partId)
  const bin = bins.find((item) => item.binId === line.fromBinId)
  if (!part && !bin) {
    return ''
  }

  return [part?.displayName, bin ? `${bin.locationKey}/${bin.binKey}` : '']
    .filter(Boolean)
    .join(' from ')
}

function defaultLine(): ShipmentLineDraft {
  return {
    id: crypto.randomUUID(),
    partId: '',
    fromBinId: '',
    quantity: '',
  }
}

export function OutboundShipmentsPanel({
  accessToken,
  parts,
  bins,
  canManage,
}: OutboundShipmentsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedShipmentId, setSelectedShipmentId] = useState('')
  const [shipmentKey, setShipmentKey] = useState('')
  const [idempotencyKey, setIdempotencyKey] = useState('')
  const [shipVia, setShipVia] = useState('manual')
  const [destinationName, setDestinationName] = useState('')
  const [destinationAddressSnapshot, setDestinationAddressSnapshot] = useState('')
  const [lines, setLines] = useState<ShipmentLineDraft[]>([defaultLine()])
  const [lastCreatedShipment, setLastCreatedShipment] = useState<OutboundShipmentResponse | null>(null)

  const shipmentsQuery = useQuery({
    queryKey: ['supplyarr-outbound-shipments', accessToken],
    queryFn: () => getOutboundShipments(accessToken),
  })

  const createMutation = useMutation({
    mutationFn: () =>
      createOutboundShipment(accessToken, {
        idempotencyKey,
        shipmentKey,
        shipVia,
        destinationName,
        destinationAddressSnapshot,
        lines: lines.map(
          (line): CreateOutboundShipmentLineRequest => ({
            partId: line.partId,
            fromBinId: line.fromBinId,
            quantity: Number(line.quantity),
          }),
        ),
      }),
    onSuccess: async (created) => {
      setShipmentKey('')
      setIdempotencyKey('')
      setShipVia('manual')
      setDestinationName('')
      setDestinationAddressSnapshot('')
      setLines([defaultLine()])
      setSelectedShipmentId(created.shipmentId)
      setLastCreatedShipment(created)
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-outbound-shipments', accessToken] })
    },
  })

  const shipments = shipmentsQuery.data ?? []
  const selectedShipment =
    shipments.find((shipment) => shipment.shipmentId === selectedShipmentId) ??
    lastCreatedShipment ??
    shipments[0] ??
    null

  useEffect(() => {
    if (!selectedShipmentId && shipments[0]) {
      setSelectedShipmentId(shipments[0].shipmentId)
    }
  }, [selectedShipmentId, shipments])

  const existingShipmentKeys = useMemo(() => shipments.map((shipment) => shipment.shipmentKey), [shipments])
  const existingIdempotencyKeys = useMemo(() => shipments.map((shipment) => shipment.idempotencyKey), [shipments])
  const shipmentKeySource = useMemo(
    () =>
      [destinationName, shipVia, ...lines.map((line) => buildLineSummary(parts, bins, line))]
        .filter(Boolean)
        .join(' '),
    [bins, destinationName, lines, parts, shipVia],
  )

  const shipmentIdempotencySource = useMemo(
    () => `${shipmentKeySource || 'outbound shipment'} ${destinationAddressSnapshot}`.trim(),
    [destinationAddressSnapshot, shipmentKeySource],
  )

  const lineOptions = toBinPickerOptions(bins)
  const linePartOptions = toPartPickerOptions(parts)

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
      <h2 className="text-lg font-medium text-white">Outbound shipments</h2>
      <p className="mt-1 text-sm text-slate-400">
        Create outbound shipment records and review the current routing/fulfillment state.
      </p>

      {shipmentsQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-500">Loading outbound shipments…</p>
      ) : null}

      <div className="mt-4 space-y-2" data-testid="outbound-shipment-list">
        {shipments.length === 0 && !shipmentsQuery.isLoading ? (
          <p className="text-sm text-slate-500">No outbound shipments yet.</p>
        ) : null}
        {shipments.map((shipment) => (
          <button
            key={shipment.shipmentId}
            type="button"
            className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
              selectedShipmentId === shipment.shipmentId
                ? 'border-sky-500/60 bg-sky-500/10'
                : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
            }`}
            onClick={() => setSelectedShipmentId(shipment.shipmentId)}
            data-testid={`outbound-shipment-row-${shipment.shipmentId}`}
          >
            <div className="flex flex-wrap items-center justify-between gap-2">
              <span className="font-medium text-slate-200">{shipment.shipmentKey}</span>
              <span className="rounded-full bg-slate-800 px-2 py-0.5 text-xs text-slate-300 ring-1 ring-slate-700">
                {shipment.status}
              </span>
            </div>
            <p className="mt-1 text-xs text-slate-500">
              {shipment.shipVia} · {shipment.destinationName} · {shipment.lines.length} line
              {shipment.lines.length === 1 ? '' : 's'}
            </p>
          </button>
        ))}
      </div>

      {selectedShipment ? (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4" data-testid="outbound-shipment-detail">
          <h3 className="text-sm font-medium text-slate-200">Shipment detail</h3>
          <p className="mt-1 text-sm text-slate-300">{selectedShipment.destinationName}</p>
          <p className="mt-1 text-xs text-slate-500">
            {selectedShipment.shipVia} · {selectedShipment.status}
            {formatTimestamp(selectedShipment.createdAt) ? ` · created ${formatTimestamp(selectedShipment.createdAt)}` : ''}
          </p>
          <ul className="mt-3 space-y-1 text-sm text-slate-400" data-testid="outbound-shipment-line-list">
            {selectedShipment.lines.map((line) => (
              <li key={line.shipmentLineId} data-testid={`outbound-shipment-line-${line.shipmentLineId}`}>
                {line.partDisplayName} ({line.partKey}) from {line.fromBinKey} · requested {line.quantityRequested}{' '}
                · reserved {line.quantityReserved} · picked {line.quantityPicked} · shipped {line.quantityShipped}
              </li>
            ))}
          </ul>
        </div>
      ) : null}

      {canManage ? (
        <div className="mt-6 border-t border-slate-800 pt-6">
          <h3 className="text-sm font-medium text-slate-300">Create outbound shipment</h3>
          <div className="mt-3 grid gap-2 sm:grid-cols-2">
            <label htmlFor="outbound-shipment-destination" className="block text-sm text-slate-400 sm:col-span-2">
              Destination name
              <input
                id="outbound-shipment-destination"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={destinationName}
                onChange={(e) => setDestinationName(e.target.value)}
              />
            </label>
            <label
              htmlFor="outbound-shipment-destination-address"
              className="block text-sm text-slate-400 sm:col-span-2"
            >
              Destination address snapshot
              <textarea
                id="outbound-shipment-destination-address"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                rows={2}
                value={destinationAddressSnapshot}
                onChange={(e) => setDestinationAddressSnapshot(e.target.value)}
              />
            </label>
            <label htmlFor="outbound-shipment-via" className="block text-sm text-slate-400">
              Ship via
              <select
                id="outbound-shipment-via"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={shipVia}
                onChange={(e) => setShipVia(e.target.value)}
              >
                <option value="manual">Manual</option>
                <option value="routarr">RoutArr</option>
              </select>
            </label>
            <div className="sm:col-span-2">
              <GeneratedKeyFieldGroup
                sourceLabel={destinationName}
                existingKeys={existingShipmentKeys}
                onKeyChange={setShipmentKey}
                domain="wms"
                kind="shipment"
                label="Shipment key"
              />
            </div>
            <div className="sm:col-span-2">
              <GeneratedKeyFieldGroup
                sourceLabel={shipmentIdempotencySource}
                existingKeys={existingIdempotencyKeys}
                onKeyChange={setIdempotencyKey}
                domain="wms"
                kind="idempotency"
                label="Idempotency key"
              />
            </div>
          </div>

          <div className="mt-4 space-y-3">
            {lines.map((line, index) => (
              <div key={line.id} className="rounded-lg border border-slate-800 bg-slate-950/40 p-3">
                <div className="flex items-center justify-between gap-2">
                  <h4 className="text-xs font-medium uppercase tracking-wide text-slate-500">
                    Line {index + 1}
                  </h4>
                  {lines.length > 1 ? (
                    <button
                      type="button"
                      className="text-xs text-rose-300 hover:text-rose-200"
                      onClick={() => setLines((current) => current.filter((item) => item.id !== line.id))}
                    >
                      Remove
                    </button>
                  ) : null}
                </div>
                <div className="mt-2 grid gap-2 sm:grid-cols-3">
                  <ControlledSelect
                    label="Part"
                    value={line.partId}
                    onChange={(value) =>
                      setLines((current) =>
                        current.map((item) => (item.id === line.id ? { ...item, partId: value } : item)),
                      )
                    }
                    options={linePartOptions}
                    emptyLabel="Select part"
                  />
                  <ControlledSelect
                    label="From bin"
                    value={line.fromBinId}
                    onChange={(value) =>
                      setLines((current) =>
                        current.map((item) => (item.id === line.id ? { ...item, fromBinId: value } : item)),
                      )
                    }
                    options={lineOptions}
                    emptyLabel="Select bin"
                  />
                  <label htmlFor={`outbound-shipment-qty-${line.id}`} className="block text-sm text-slate-400">
                    Quantity
                    <input
                      id={`outbound-shipment-qty-${line.id}`}
                      className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                      type="number"
                      min="0"
                      step="any"
                      value={line.quantity}
                      onChange={(e) =>
                        setLines((current) =>
                          current.map((item) =>
                            item.id === line.id ? { ...item, quantity: e.target.value } : item,
                          ),
                        )
                      }
                    />
                  </label>
                </div>
              </div>
            ))}
          </div>

          <div className="mt-3 flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded bg-slate-700 px-3 py-1.5 text-sm text-white hover:bg-slate-600"
              onClick={() => setLines((current) => [...current, defaultLine()])}
            >
              Add line
            </button>
            <button
              type="button"
              className="rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={
                !shipmentKey ||
                !idempotencyKey ||
                !destinationName ||
                lines.some((line) => !line.partId || !line.fromBinId || !line.quantity || Number(line.quantity) <= 0) ||
                createMutation.isPending
              }
              onClick={() => createMutation.mutate()}
            >
              {createMutation.isPending ? 'Creating…' : 'Create shipment'}
            </button>
          </div>

          {lastCreatedShipment ? (
            <p className="mt-3 text-sm text-emerald-300" data-testid="outbound-shipment-created">
              Created shipment {lastCreatedShipment.shipmentKey} with {lastCreatedShipment.lines.length}{' '}
              line{lastCreatedShipment.lines.length === 1 ? '' : 's'}.
            </p>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
