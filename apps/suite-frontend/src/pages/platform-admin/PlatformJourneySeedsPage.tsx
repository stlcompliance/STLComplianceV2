import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import type { ReferenceDatasetResponse, ReferenceImportResponse } from '../../api/types'
import { useToast } from '../../feedback'

function formatDatasetLabel(dataset: ReferenceDatasetResponse): string {
  return `${dataset.ownerService} - ${dataset.name}`
}

export function PlatformJourneySeedsPage() {
  const { pushToast } = useToast()
  const [selectedDatasetId, setSelectedDatasetId] = useState('')
  const [singleValue, setSingleValue] = useState('')
  const [valuesText, setValuesText] = useState('')
  const [resultsByDataset, setResultsByDataset] = useState<Record<string, ReferenceImportResponse>>({})

  const datasetsQuery = useQuery({
    queryKey: ['platform-admin-reference-data-datasets'],
    queryFn: () => nexarr.listReferenceDatasets(),
  })

  const datasetOptions = useMemo(
    () =>
      (datasetsQuery.data ?? []).map((dataset) => ({
        value: dataset.id,
        label: formatDatasetLabel(dataset),
        helper: dataset.key,
      })),
    [datasetsQuery.data],
  )

  useEffect(() => {
    if (!selectedDatasetId && datasetOptions.length > 0) {
      setSelectedDatasetId(datasetOptions[0].value)
    }
  }, [datasetOptions, selectedDatasetId])

  const selectedDataset = (datasetsQuery.data ?? []).find((dataset) => dataset.id === selectedDatasetId) ?? null
  const selectedResult = selectedDatasetId ? resultsByDataset[selectedDatasetId] ?? null : null

  const addValueMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceDatasetInput(selectedDatasetId, {
        value: singleValue.trim(),
      }),
    onSuccess: (result) => {
      setSingleValue('')
      setResultsByDataset((current) => ({
        ...current,
        [result.datasetId]: result,
      }))
      pushToast({ message: 'Dataset value added.', variant: 'success' })
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const importValuesMutation = useMutation({
    mutationFn: () =>
      nexarr.createReferenceDatasetInput(selectedDatasetId, {
        valuesText: valuesText.trim(),
      }),
    onSuccess: (result) => {
      setValuesText('')
      setResultsByDataset((current) => ({
        ...current,
        [result.datasetId]: result,
      }))
      pushToast({ message: 'Dataset import queued.', variant: 'success' })
    },
    onError: (error: Error) => pushToast({ message: error.message, variant: 'error' }),
  })

  const isLoading = datasetsQuery.isLoading

  if (isLoading) {
    return <p className="text-sm text-slate-500">Loading datasets…</p>
  }

  if (datasetsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(datasetsQuery.error, 'Failed to load datasets.')}
        onRetry={() => void datasetsQuery.refetch()}
        retryLabel="Retry datasets"
      />
    )
  }

  return (
    <div className="space-y-6">
      <header>
        <h4 className="text-lg font-semibold text-stl-navy">Dataset inputs</h4>
        <p className="mt-1 text-sm text-slate-600">
          Pick a product-owned dataset, then add one value or import several values in a single pass.
        </p>
      </header>

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        <div className="grid gap-4 lg:grid-cols-[2fr_1fr]">
          <label className="block text-sm text-slate-700">
            Dataset
            <select
              value={selectedDatasetId}
              onChange={(event) => setSelectedDatasetId(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
            >
              {datasetOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>
          <div className="rounded-md border border-slate-200 bg-slate-50 p-3 text-sm text-slate-600">
            <p className="font-medium text-stl-navy">Selected dataset</p>
            <p className="mt-1">{selectedDataset ? formatDatasetLabel(selectedDataset) : '—'}</p>
            <p className="mt-1 font-mono text-xs text-slate-500">{selectedDataset?.key ?? '—'}</p>
          </div>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-2">
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h5 className="font-semibold text-stl-navy">Add value</h5>
          <p className="mt-1 text-sm text-slate-600">
            Add a single value to the selected dataset.
          </p>

          <div className="mt-4 space-y-3">
            <label className="block text-sm text-slate-700">
              Value
              <input
                value={singleValue}
                onChange={(event) => setSingleValue(event.target.value)}
                placeholder="Asset Class A"
                className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </label>

            <button
              type="button"
              disabled={!selectedDatasetId || !singleValue.trim() || addValueMutation.isPending}
              onClick={() => addValueMutation.mutate()}
              className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
            >
              {addValueMutation.isPending ? 'Saving…' : 'Add value'}
            </button>
          </div>
        </section>

        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <h5 className="font-semibold text-stl-navy">Import values</h5>
          <p className="mt-1 text-sm text-slate-600">
            Paste one value per line to queue a bulk import.
          </p>

          <div className="mt-4 space-y-3">
            <label className="block text-sm text-slate-700">
              Values
              <textarea
                value={valuesText}
                onChange={(event) => setValuesText(event.target.value)}
                rows={8}
                placeholder={`Asset Class A\nAsset Class B\nAsset Class C`}
                className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
              />
            </label>

            <button
              type="button"
              disabled={!selectedDatasetId || !valuesText.trim() || importValuesMutation.isPending}
              onClick={() => importValuesMutation.mutate()}
              className="rounded-md bg-stl-navy px-4 py-2 text-sm font-medium text-white hover:bg-stl-navy/90 disabled:opacity-50"
            >
              {importValuesMutation.isPending ? 'Importing…' : 'Import values'}
            </button>
          </div>
        </section>
      </div>

      {selectedResult ? (
        <section className="rounded-xl border border-slate-200 bg-white p-5">
          <div className="flex flex-wrap items-center gap-3">
            <div>
              <h5 className="font-semibold text-stl-navy">Latest dataset input</h5>
              <p className="text-sm text-slate-600">
                {selectedResult.datasetName} · {selectedResult.status}
              </p>
            </div>
            <span className="ml-auto rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700">
              {selectedResult.stagingRecordCount} records
            </span>
          </div>

          <dl className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <Detail label="Dataset" value={selectedResult.datasetName} />
            <Detail label="Source" value={selectedResult.sourceName} />
            <Detail label="Pending review" value={String(selectedResult.pendingReviewCount)} />
            <Detail label="Approved" value={String(selectedResult.approvedCount)} />
          </dl>
        </section>
      ) : null}
    </div>
  )
}

function Detail({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-slate-50 p-3">
      <p className="text-[11px] font-semibold uppercase tracking-[0.18em] text-slate-500">{label}</p>
      <p className="mt-2 text-sm font-medium text-stl-navy">{value}</p>
    </div>
  )
}
