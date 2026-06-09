import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'
import type { ReferenceDatasetResponse, ReferenceImportResponse } from '../../api/types'
import { useToast } from '../../feedback'

function formatDatasetLabel(dataset: ReferenceDatasetResponse): string {
  return `${dataset.ownerService} - ${dataset.name}`
}

function formatProductLabel(ownerService: string): string {
  return ownerService
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

  const datasetsByProduct = useMemo(() => {
    const grouped = new Map<string, ReferenceDatasetResponse[]>()
    for (const dataset of datasetsQuery.data ?? []) {
      const bucket = grouped.get(dataset.ownerService) ?? []
      bucket.push(dataset)
      grouped.set(dataset.ownerService, bucket)
    }

    return Array.from(grouped.entries())
      .map(([ownerService, datasets]) => ({
        ownerService,
        datasets: datasets.slice().sort((left, right) => left.name.localeCompare(right.name)),
      }))
      .sort((left, right) => left.ownerService.localeCompare(right.ownerService))
  }, [datasetsQuery.data])

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
      pushToast({ message: 'Dataset values imported.', variant: 'success' })
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
          Pick a product-owned dataset, then add one value or import several values directly.
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

      <section className="rounded-xl border border-slate-200 bg-white p-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h5 className="font-semibold text-stl-navy">Browse by product</h5>
            <p className="mt-1 text-sm text-slate-600">
              The seeded datasets are grouped by owning product so you can jump directly to the right list.
            </p>
          </div>
          <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-700">
            {datasetsQuery.data?.length ?? 0} datasets
          </span>
        </div>

        <div className="mt-4 grid gap-4 xl:grid-cols-2">
          {datasetsByProduct.map((group) => (
            <div key={group.ownerService} className="rounded-lg border border-slate-200 bg-slate-50 p-4">
              <div className="flex items-center justify-between gap-3">
                <h6 className="font-medium text-stl-navy">{formatProductLabel(group.ownerService)}</h6>
                <span className="rounded-full bg-white px-2 py-0.5 text-xs font-medium text-slate-600">
                  {group.datasets.length}
                </span>
              </div>

              <div className="mt-3 flex flex-wrap gap-2">
                {group.datasets.map((dataset) => {
                  const isSelected = dataset.id === selectedDatasetId
                  return (
                    <button
                      key={dataset.id}
                      type="button"
                      onClick={() => setSelectedDatasetId(dataset.id)}
                      className={[
                        'rounded-full border px-3 py-1.5 text-left text-sm transition',
                        isSelected
                          ? 'border-stl-teal bg-white text-stl-navy shadow-sm'
                          : 'border-slate-200 bg-white text-slate-700 hover:border-slate-300 hover:bg-slate-50',
                      ].join(' ')}
                    >
                      <span className="block font-medium">{dataset.name}</span>
                      <span className="block font-mono text-[11px] text-slate-500">{dataset.key}</span>
                    </button>
                  )
                })}
              </div>
            </div>
          ))}
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
            Paste one value per line to import them immediately.
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
