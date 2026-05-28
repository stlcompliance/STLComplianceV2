import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  assignDispatchException,
  createDispatchException,
  linkDispatchExceptionTrip,
  listDispatchExceptions,
  resolveDispatchException,
} from '../api/client'
import type { DispatchExceptionSummaryResponse } from '../api/types'

type Props = {
  accessToken: string
  userId: string
  canTriage: boolean
}

function ExceptionRow({
  item,
  canTriage,
  userId,
  onAssignSelf,
  onResolve,
  onLinkTrip,
  isPending,
}: {
  item: DispatchExceptionSummaryResponse
  canTriage: boolean
  userId: string
  onAssignSelf: (id: string) => void
  onResolve: (id: string, notes: string) => void
  onLinkTrip: (id: string, tripId: string) => void
  isPending: boolean
}) {
  const [tripIdInput, setTripIdInput] = useState(item.tripId ?? '')
  const [resolveNotes, setResolveNotes] = useState('')

  return (
    <li
      className="rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
      data-testid={`exception-row-${item.exceptionId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="font-medium text-slate-100">{item.title}</p>
          <p className="text-xs text-slate-500">
            {item.exceptionKey} · {item.category} · {item.status}
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
          <input
            className="min-w-[12rem] flex-1 rounded border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
            placeholder="Trip id to link…"
            value={tripIdInput}
            onChange={(e) => setTripIdInput(e.target.value)}
          />
          <button
            type="button"
            className="rounded bg-sky-800 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={!tripIdInput.trim() || isPending}
            onClick={() => onLinkTrip(item.exceptionId, tripIdInput.trim())}
          >
            Link trip
          </button>
          <input
            className="min-w-[10rem] flex-1 rounded border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
            placeholder="Resolution notes…"
            value={resolveNotes}
            onChange={(e) => setResolveNotes(e.target.value)}
          />
          <button
            type="button"
            className="rounded bg-emerald-800 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={isPending}
            onClick={() => onResolve(item.exceptionId, resolveNotes)}
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

  const listQuery = useQuery({
    queryKey: ['routarr-dispatch-exceptions', accessToken],
    queryFn: () => listDispatchExceptions(accessToken, 'open'),
  })

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
    }) => {
      if (action.type === 'assign') {
        await assignDispatchException(accessToken, action.exceptionId, {
          assignedToUserId: userId,
        })
      }
      if (action.type === 'resolve') {
        await resolveDispatchException(accessToken, action.exceptionId, {
          resolutionNotes: action.notes,
        })
      }
      if (action.type === 'link' && action.tripId) {
        await linkDispatchExceptionTrip(accessToken, action.exceptionId, {
          tripId: action.tripId,
        })
      }
    },
    onSuccess: invalidate,
  })

  if (listQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading exception queue…</p>
  }

  if (listQuery.isError) {
    return <p className="text-sm text-red-300">{(listQuery.error as Error).message}</p>
  }

  const list = listQuery.data!

  return (
    <section
      className="rounded-xl border border-amber-800/40 bg-amber-950/15 p-5"
      data-testid="dispatch-exception-queue-panel"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Exception queue</h2>
        <p className="mt-1 text-sm text-slate-400">
          {list.openCount} open · showing {list.totalCount} in queue
        </p>
      </header>

      {canTriage ? (
        <form
          className="mt-4 grid gap-2 rounded-lg border border-slate-700 bg-slate-900/50 p-3 md:grid-cols-2"
          onSubmit={(e) => {
            e.preventDefault()
            createMutation.mutate()
          }}
        >
          <input
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200 md:col-span-2"
            placeholder="Exception title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
          />
          <input
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200 md:col-span-2"
            placeholder="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <select
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
          <input
            className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
            placeholder="Optional trip id"
            value={createTripId}
            onChange={(e) => setCreateTripId(e.target.value)}
          />
          <button
            type="submit"
            className="rounded bg-amber-700 px-3 py-1 text-sm text-white disabled:opacity-50 md:col-span-2"
            disabled={!title.trim() || createMutation.isPending}
          >
            Log exception
          </button>
        </form>
      ) : null}

      <ul className="mt-4 max-h-80 space-y-2 overflow-y-auto">
        {list.items.length === 0 ? (
          <li className="text-sm text-slate-500">No open exceptions</li>
        ) : (
          list.items.map((item) => (
            <ExceptionRow
              key={item.exceptionId}
              item={item}
              canTriage={canTriage}
              userId={userId}
              isPending={triageMutation.isPending}
              onAssignSelf={(exceptionId) =>
                triageMutation.mutate({ type: 'assign', exceptionId })
              }
              onResolve={(exceptionId, notes) =>
                triageMutation.mutate({ type: 'resolve', exceptionId, notes })
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
