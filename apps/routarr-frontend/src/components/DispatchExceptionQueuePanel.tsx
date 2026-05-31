import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { AdvancedReferenceField, ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import {
  assignDispatchException,
  bulkAssignDispatchExceptions,
  bulkResolveDispatchExceptions,
  createDispatchException,
  getTrips,
  linkDispatchExceptionTrip,
  listDispatchExceptionResolutionTemplates,
  listDispatchExceptions,
  resolveDispatchException,
} from '../api/client'
import type { DispatchExceptionSummaryResponse } from '../api/types'
import { tripToPickerOption } from '../lib/referencePickers'

type Props = {
  accessToken: string
  userId: string
  canTriage: boolean
}

function formatSlaDue(slaDueAt: string | null): string {
  if (!slaDueAt) {
    return 'No SLA'
  }
  return new Date(slaDueAt).toLocaleString()
}

function ExceptionRow({
  item,
  canTriage,
  userId,
  selected,
  onToggleSelect,
  onAssignSelf,
  onResolve,
  onLinkTrip,
  isPending,
  resolutionTemplateKey,
  tripOptions,
  initialTripId,
}: {
  item: DispatchExceptionSummaryResponse
  canTriage: boolean
  userId: string
  selected: boolean
  onToggleSelect: (id: string) => void
  onAssignSelf: (id: string) => void
  onResolve: (id: string, notes: string, templateKey?: string) => void
  onLinkTrip: (id: string, tripId: string) => void
  isPending: boolean
  resolutionTemplateKey: string
  tripOptions: PickerOption[]
  initialTripId: string
}) {
  const [tripIdInput, setTripIdInput] = useState(initialTripId)
  const [resolveNotes, setResolveNotes] = useState('')

  const selectedTripOption = useMemo((): PickerOption | undefined => {
    return tripOptions.find((option) => option.value === tripIdInput)
  }, [tripIdInput, tripOptions])

  return (
    <li
      className="rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
      data-testid={`exception-row-${item.exceptionId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div className="flex items-start gap-2">
          {canTriage ? (
            <input id="dispatchexceptionqueue-input-field-5"
              type="checkbox"
              checked={selected}
              aria-label={`Select ${item.exceptionKey}`}
              data-testid={`exception-select-${item.exceptionId}`}
              onChange={() => onToggleSelect(item.exceptionId)}
            />
          ) : null}
          <div>
            <p className="font-medium text-slate-100">{item.title}</p>
            <p className="text-xs text-slate-500">
              {item.exceptionKey} · {item.category} · {item.status}
            </p>
            <p className="mt-1 text-xs text-slate-400">
              SLA due: {formatSlaDue(item.slaDueAt)}
              {item.isSlaBreached ? (
                <span
                  className="ml-2 rounded bg-rose-900/60 px-1.5 py-0.5 text-rose-200"
                  data-testid={`exception-sla-breached-${item.exceptionId}`}
                >
                  SLA breached
                </span>
              ) : null}
            </p>
            {item.description ? (
              <p className="mt-1 text-slate-400">{item.description}</p>
            ) : null}
            {item.tripNumber ? (
              <p className="mt-1 text-sky-300">
                Trip: {item.tripNumber} — {item.tripTitle}
              </p>
            ) : null}
          </div>
        </div>
        {canTriage ? (
          <div className="flex flex-col gap-1">
            <button
              type="button"
              className="rounded bg-slate-700 px-2 py-1 text-xs text-white disabled:opacity-50"
              disabled={isPending}
              onClick={() => onAssignSelf(item.exceptionId)}
            >
              Assign to me
            </button>
          </div>
        ) : null}
      </div>
      {canTriage ? (
        <div className="mt-3 flex flex-wrap gap-2 border-t border-slate-800 pt-2">
          <div className="min-w-[12rem] flex-1">
            <StaticSearchPicker
              value={tripIdInput}
              onChange={setTripIdInput}
              options={tripOptions}
              selectedOption={selectedTripOption}
              placeholder="Link trip…"
              testId={`exception-link-trip-picker-${item.exceptionId}`}
            />
            <AdvancedReferenceField
              value={tripIdInput}
              onChange={setTripIdInput}
              label="Trip id"
              testId={`exception-link-trip-advanced-${item.exceptionId}`}
            />
          </div>
          <button
            type="button"
            className="rounded bg-sky-800 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={!tripIdInput.trim() || isPending}
            onClick={() => onLinkTrip(item.exceptionId, tripIdInput.trim())}
          >
            Link trip
          </button>
          <input id="dispatchexceptionqueue-input-field-4"
            className="min-w-[10rem] flex-1 rounded border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
            placeholder="Resolution notes…"
            value={resolveNotes}
            onChange={(e) => setResolveNotes(e.target.value)}
          />
          <button
            type="button"
            className="rounded bg-emerald-800 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={isPending}
            onClick={() =>
              onResolve(
                item.exceptionId,
                resolveNotes,
                resolutionTemplateKey || undefined,
              )
            }
          >
            Resolve
          </button>
        </div>
      ) : null}
      {item.assignedToUserId ? (
        <p className="mt-2 text-xs text-slate-500">
          Assigned: {item.assignedToUserId === userId ? 'you' : item.assignedToUserId.slice(0, 8)}
        </p>
      ) : null}
    </li>
  )
}

export function DispatchExceptionQueuePanel({ accessToken, userId, canTriage }: Props) {
  const queryClient = useQueryClient()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState('delay')
  const [createTripId, setCreateTripId] = useState('')
  const [assignOnCreate, setAssignOnCreate] = useState(true)
  const [overdueOnly, setOverdueOnly] = useState(false)
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [bulkTemplateKey, setBulkTemplateKey] = useState('')
  const [bulkResolveNotes, setBulkResolveNotes] = useState('')

  const listQuery = useQuery({
    queryKey: ['routarr-dispatch-exceptions', accessToken, overdueOnly],
    queryFn: () => listDispatchExceptions(accessToken, 'open', overdueOnly),
  })

  const templatesQuery = useQuery({
    queryKey: ['routarr-dispatch-exception-templates', accessToken],
    queryFn: () => listDispatchExceptionResolutionTemplates(accessToken),
    enabled: canTriage,
  })

  const tripsQuery = useQuery({
    queryKey: ['routarr-trips-exceptions', accessToken],
    queryFn: () => getTrips(accessToken),
    enabled: canTriage,
  })

  const tripOptions = useMemo(
    () => (tripsQuery.data ?? []).map(tripToPickerOption),
    [tripsQuery.data],
  )

  const createTripSelectedOption = useMemo((): PickerOption | undefined => {
    return tripOptions.find((option) => option.value === createTripId)
  }, [createTripId, tripOptions])

  const templates = templatesQuery.data ?? []

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['routarr-dispatch-exceptions'] })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createDispatchException(accessToken, {
        title,
        description,
        category,
        tripId: createTripId.trim() || undefined,
        assignedToUserId: assignOnCreate ? userId : undefined,
      }),
    onSuccess: () => {
      setTitle('')
      setDescription('')
      setCreateTripId('')
      invalidate()
    },
  })

  const triageMutation = useMutation({
    mutationFn: async (action: {
      type: 'assign' | 'resolve' | 'link'
      exceptionId: string
      tripId?: string
      notes?: string
      templateKey?: string
    }) => {
      if (action.type === 'assign') {
        await assignDispatchException(accessToken, action.exceptionId, {
          assignedToUserId: userId,
        })
      }
      if (action.type === 'resolve') {
        await resolveDispatchException(accessToken, action.exceptionId, {
          resolutionNotes: action.notes,
          resolutionTemplateKey: action.templateKey,
        })
      }
      if (action.type === 'link' && action.tripId) {
        await linkDispatchExceptionTrip(accessToken, action.exceptionId, {
          tripId: action.tripId,
        })
      }
    },
    onSuccess: () => {
      setSelectedIds([])
      invalidate()
    },
  })

  const bulkAssignMutation = useMutation({
    mutationFn: () =>
      bulkAssignDispatchExceptions(accessToken, {
        exceptionIds: selectedIds,
        assignedToUserId: userId,
      }),
    onSuccess: () => {
      setSelectedIds([])
      invalidate()
    },
  })

  const bulkResolveMutation = useMutation({
    mutationFn: () =>
      bulkResolveDispatchExceptions(accessToken, {
        exceptionIds: selectedIds,
        resolutionNotes: bulkResolveNotes || undefined,
        resolutionTemplateKey: bulkTemplateKey || undefined,
      }),
    onSuccess: () => {
      setSelectedIds([])
      setBulkResolveNotes('')
      invalidate()
    },
  })

  const toggleSelect = (exceptionId: string) => {
    setSelectedIds((current) =>
      current.includes(exceptionId)
        ? current.filter((id) => id !== exceptionId)
        : [...current, exceptionId],
    )
  }

  const selectAllVisible = () => {
    if (!listQuery.data) {
      return
    }
    setSelectedIds(listQuery.data.items.map((item) => item.exceptionId))
  }

  const isPending =
    triageMutation.isPending ||
    bulkAssignMutation.isPending ||
    bulkResolveMutation.isPending

  const defaultTemplateKey = useMemo(
    () => templates.find((t) => t.templateKey === 'reassign_driver')?.templateKey ?? '',
    [templates],
  )

  if (listQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading exception queue…</p>
  }

  if (listQuery.isError) {
    return (
      <ApiErrorCallout
        title="Exception queue unavailable"
        message={getErrorMessage(listQuery.error, 'Failed to load exception queue.')}
        retryLabel="Retry queue"
        onRetry={() => {
          void listQuery.refetch()
        }}
      />
    )
  }

  const list = listQuery.data!

  return (
    <section
      className="rounded-xl border border-amber-800/40 bg-amber-950/15 p-5"
      data-testid="dispatch-exception-queue-panel"
    >
      <header className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Exception queue</h2>
          <p className="mt-1 text-sm text-slate-400">
            {list.openCount} open · {list.overdueCount} overdue SLA · showing {list.totalCount}{' '}
            in queue
          </p>
        </div>
        <label className="flex items-center gap-2 text-sm text-slate-300" htmlFor="exception-overdue-filter">
          <input id="exception-overdue-filter"
            type="checkbox"
            checked={overdueOnly}
            data-testid="exception-overdue-filter"
            onChange={(e) => {
              setOverdueOnly(e.target.checked)
              setSelectedIds([])
            }}
          />
          Overdue only
        </label>
      </header>

      {canTriage ? (
        <form
          className="mt-4 grid gap-2 rounded-lg border border-slate-700 bg-slate-900/50 p-3 md:grid-cols-2"
          onSubmit={(e) => {
            e.preventDefault()
            createMutation.mutate()
          }}
        >
          <input id="dispatchexceptionqueue-input-field-3"
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200 md:col-span-2"
            placeholder="Exception title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />
          <input id="dispatchexceptionqueue-input-field-2"
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200 md:col-span-2"
            placeholder="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <select id="dispatchexceptionqueue-select-field"
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
            value={category}
            onChange={(e) => setCategory(e.target.value)}
          >
            {['delay', 'driver', 'vehicle', 'route', 'stop', 'compliance', 'other'].map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
          <div>
            <StaticSearchPicker
              label="Optional trip"
              value={createTripId}
              onChange={setCreateTripId}
              options={tripOptions}
              selectedOption={createTripSelectedOption}
              placeholder="Search trips…"
              disabled={tripsQuery.isLoading}
              testId="exception-create-trip-picker"
            />
            <AdvancedReferenceField
              value={createTripId}
              onChange={setCreateTripId}
              label="Trip id"
              testId="exception-create-trip-advanced"
            />
          </div>
          <label className="flex items-center gap-2 text-sm text-slate-300 md:col-span-2" htmlFor="exception-assign-on-create">
            <input id="exception-assign-on-create"
              type="checkbox"
              checked={assignOnCreate}
              data-testid="exception-assign-on-create"
              onChange={(e) => setAssignOnCreate(e.target.checked)}
            />
            Assign to me on create (category SLA applies)
          </label>
          <button
            type="submit"
            className="rounded bg-amber-700 px-3 py-1 text-sm text-white disabled:opacity-50 md:col-span-2"
            disabled={!title.trim() || createMutation.isPending}
          >
            Log exception
          </button>
        </form>
      ) : null}

      {canTriage && templates.length > 0 ? (
        <div
          className="mt-4 flex flex-wrap items-end gap-2 rounded-lg border border-slate-700 bg-slate-900/40 p-3"
          data-testid="exception-bulk-actions"
        >
          <p className="w-full text-xs text-slate-400">
            {selectedIds.length} selected · resolution template for row/bulk resolve
          </p>
          <select id="exception-resolution-template"
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
            value={bulkTemplateKey || defaultTemplateKey}
            data-testid="exception-resolution-template"
            onChange={(e) => setBulkTemplateKey(e.target.value)}
          >
            <option value="">No template</option>
            {templates.map((template) => (
              <option key={template.templateKey} value={template.templateKey}>
                {template.label}
              </option>
            ))}
          </select>
          <input id="dispatchexceptionqueue-input-field"
            className="min-w-[12rem] flex-1 rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
            placeholder="Bulk resolve notes…"
            value={bulkResolveNotes}
            onChange={(e) => setBulkResolveNotes(e.target.value)}
          />
          <button
            type="button"
            className="rounded bg-slate-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={list.items.length === 0 || isPending}
            onClick={selectAllVisible}
          >
            Select all
          </button>
          <button
            type="button"
            className="rounded bg-slate-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={selectedIds.length === 0 || isPending}
            data-testid="exception-bulk-assign"
            onClick={() => bulkAssignMutation.mutate()}
          >
            Bulk assign to me
          </button>
          <button
            type="button"
            className="rounded bg-emerald-800 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={selectedIds.length === 0 || isPending}
            data-testid="exception-bulk-resolve"
            onClick={() => bulkResolveMutation.mutate()}
          >
            Bulk resolve
          </button>
        </div>
      ) : null}

      <ul className="mt-4 max-h-80 space-y-2 overflow-y-auto">
        {list.items.length === 0 ? (
          <li className="text-sm text-slate-500">
            {overdueOnly ? 'No overdue exceptions' : 'No open exceptions'}
          </li>
        ) : (
          list.items.map((item) => (
            <ExceptionRow
              key={item.exceptionId}
              item={item}
              canTriage={canTriage}
              userId={userId}
              selected={selectedIds.includes(item.exceptionId)}
              onToggleSelect={toggleSelect}
              isPending={isPending}
              resolutionTemplateKey={bulkTemplateKey || defaultTemplateKey}
              tripOptions={tripOptions}
              initialTripId={item.tripId ?? ''}
              onAssignSelf={(exceptionId) =>
                triageMutation.mutate({ type: 'assign', exceptionId })
              }
              onResolve={(exceptionId, notes, templateKey) =>
                triageMutation.mutate({ type: 'resolve', exceptionId, notes, templateKey })
              }
              onLinkTrip={(exceptionId, tripId) =>
                triageMutation.mutate({ type: 'link', exceptionId, tripId })
              }
            />
          ))
        )}
      </ul>
    </section>
  )
}
