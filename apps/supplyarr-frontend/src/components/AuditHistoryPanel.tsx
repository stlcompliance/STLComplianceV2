import { useQuery } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { ScrollText } from 'lucide-react'
import { ApiErrorCallout, ControlledSelect, getErrorMessage } from '@stl/shared-ui'

import { listAuditHistory } from '../api/client'

interface AuditHistoryPanelProps {
  accessToken: string
  canRead: boolean
}

export function AuditHistoryPanel({ accessToken, canRead }: AuditHistoryPanelProps) {
  const [actionFilter, setActionFilter] = useState('')
  const [targetTypeFilter, setTargetTypeFilter] = useState('')
  const [targetIdFilter, setTargetIdFilter] = useState('')
  const [resultFilter, setResultFilter] = useState('')
  const [cursor, setCursor] = useState<string | undefined>(undefined)
  const [accumulatedItems, setAccumulatedItems] = useState<
    Awaited<ReturnType<typeof listAuditHistory>>['items']
  >([])

  const query = useQuery({
    queryKey: [
      'supplyarr-audit-history',
      accessToken,
      actionFilter,
      targetTypeFilter,
      targetIdFilter,
      resultFilter,
      cursor,
    ],
    queryFn: () =>
      listAuditHistory(accessToken, {
        cursor,
        action: actionFilter || undefined,
        targetType: targetTypeFilter || undefined,
        targetId: targetIdFilter || undefined,
        result: resultFilter || undefined,
        limit: 25,
      }),
    enabled: canRead,
  })

  useEffect(() => {
    if (!query.data) {
      return
    }

    if (!cursor) {
      setAccumulatedItems(query.data.items)
      return
    }

    setAccumulatedItems((current) => [...current, ...query.data.items])
  }, [query.data, cursor])

  if (!canRead) {
    return null
  }

  const resetFilters = () => {
    setCursor(undefined)
    setAccumulatedItems([])
    setActionFilter('')
    setTargetTypeFilter('')
    setTargetIdFilter('')
    setResultFilter('')
  }

  const auditItems = useMemo(() => accumulatedItems, [accumulatedItems])

  const targetTypeOptions = useMemo(() => {
    const values = new Set<string>()
    for (const item of auditItems) {
      if (item.targetType.trim().length > 0) {
        values.add(item.targetType)
      }
    }
    if (targetTypeFilter.trim().length > 0) {
      values.add(targetTypeFilter.trim())
    }

    return [...values]
      .sort((left, right) => left.localeCompare(right))
      .map((value) => ({ value, label: value }))
  }, [auditItems, targetTypeFilter])

  const targetIdOptions = useMemo(() => {
    const values = new Set<string>()
    for (const item of auditItems) {
      if (targetTypeFilter && item.targetType !== targetTypeFilter) {
        continue
      }

      if (item.targetId?.trim()) {
        values.add(item.targetId)
      }
    }

    if (targetIdFilter.trim().length > 0) {
      values.add(targetIdFilter.trim())
    }

    return [...values]
      .sort((left, right) => left.localeCompare(right))
      .map((value) => ({ value, label: value }))
  }, [auditItems, targetIdFilter, targetTypeFilter])

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="audit-history-panel"
    >
      <div className="flex gap-3">
        <ScrollText className="mt-0.5 h-5 w-5 text-violet-400" aria-hidden />
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Audit history</h2>
          <p className="mt-1 text-sm text-slate-400">
            Tenant-scoped SupplyArr actions — who, what, when, and entity references.
          </p>
        </div>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <label htmlFor="audit-history-action-filter" className="text-xs text-slate-400">
          Action contains
          <input
            id="audit-history-action-filter"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
            value={actionFilter}
            onChange={(event) => {
              setCursor(undefined)
              setAccumulatedItems([])
              setActionFilter(event.target.value)
            }}
          />
        </label>
        <ControlledSelect
          id="audit-history-target-type-filter"
          label="Target type"
          value={targetTypeFilter}
          onChange={(value) => {
            setCursor(undefined)
            setAccumulatedItems([])
            setTargetTypeFilter(value)
            setTargetIdFilter('')
          }}
          options={targetTypeOptions}
          emptyLabel="Any"
          testId="audit-history-target-type-filter"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
        />
        <ControlledSelect
          id="audit-history-target-id-filter"
          label="Target id"
          value={targetIdFilter}
          onChange={(value) => {
            setCursor(undefined)
            setAccumulatedItems([])
            setTargetIdFilter(value)
          }}
          options={targetIdOptions}
          emptyLabel="Any"
          testId="audit-history-target-id-filter"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
        />
        <label htmlFor="audit-history-result-filter" className="text-xs text-slate-400">
          Audit result filter
          <select
            id="audit-history-result-filter"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
            value={resultFilter}
            onChange={(event) => {
              setCursor(undefined)
              setAccumulatedItems([])
              setResultFilter(event.target.value)
            }}
          >
            <option value="">Any</option>
            <option value="success">success</option>
            <option value="failure">failure</option>
          </select>
        </label>
      </div>

      <div className="mt-3">
        <button
          type="button"
          className="text-xs text-slate-400 underline hover:text-slate-200"
          onClick={resetFilters}
        >
          Clear filters
        </button>
      </div>

      {query.isLoading && !accumulatedItems.length && (
        <p className="mt-3 text-sm text-slate-500">Loading audit history…</p>
      )}

      {query.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Audit history unavailable"
            message={getErrorMessage(query.error, 'Failed to load audit history.')}
            retryLabel="Retry history"
            onRetry={() => {
              void query.refetch()
            }}
          />
        </div>
      )}

      {accumulatedItems.length > 0 && (
        <div className="mt-4 overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="text-xs uppercase text-slate-500">
              <tr>
                <th className="px-2 py-2">When</th>
                <th className="px-2 py-2">Action</th>
                <th className="px-2 py-2">Target</th>
                <th className="px-2 py-2">Actor</th>
                <th className="px-2 py-2">Result</th>
              </tr>
            </thead>
            <tbody>
              {accumulatedItems.map((item) => (
                <tr key={item.id} className="border-t border-slate-800">
                  <td className="px-2 py-2 text-slate-400">
                    {new Date(item.occurredAt).toLocaleString()}
                  </td>
                  <td className="px-2 py-2 text-slate-200">{item.action}</td>
                  <td className="px-2 py-2 text-slate-300">
                    {item.targetType}
                    {item.targetId ? ` · ${item.targetId}` : ''}
                  </td>
                  <td className="px-2 py-2 text-slate-400">
                    {item.actorUserId ?? 'system'}
                  </td>
                  <td className="px-2 py-2 capitalize text-slate-300">{item.result}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {query.data?.hasMore ? (
        <button
          type="button"
          className="mt-4 rounded-md border border-slate-600 px-3 py-1.5 text-sm text-slate-200 hover:bg-slate-800"
          onClick={() => setCursor(query.data.nextCursor ?? undefined)}
        >
          Load more
        </button>
      ) : null}

      {query.isSuccess && accumulatedItems.length === 0 && (
        <p className="mt-3 text-sm text-slate-500">No audit events match the current filters.</p>
      )}
    </section>
  )
}
