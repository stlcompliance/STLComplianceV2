import { ControlledSelect, GeneratedKeyField, slugifyKey } from '@stl/shared-ui'

import type {
  AssetClassResponse,
  AssetReadinessSummaryResponse,
  AssetResponse,
  AssetTypeResponse,
} from '../api/types'

interface AssetRegistryPanelProps {
  canManage: boolean
  classes: AssetClassResponse[]
  types: AssetTypeResponse[]
  assets: AssetResponse[]
  readinessByAssetId: Record<string, AssetReadinessSummaryResponse>
  isLoading: boolean
  isReadinessLoading: boolean
  className: string
  classDescription: string
  classKeyManualOverride: string
  confirmedClassKey: string | null
  selectedClassId: string
  typeName: string
  typeDescription: string
  typeKeyManualOverride: string
  confirmedTypeKey: string | null
  selectedTypeId: string
  assetTag: string
  assetName: string
  assetDescription: string
  siteRef: string
  onClassNameChange: (value: string) => void
  onClassDescriptionChange: (value: string) => void
  onClassKeyManualOverrideChange: (value: string) => void
  onSelectedClassIdChange: (value: string) => void
  onTypeNameChange: (value: string) => void
  onTypeDescriptionChange: (value: string) => void
  onTypeKeyManualOverrideChange: (value: string) => void
  onSelectedTypeIdChange: (value: string) => void
  onAssetTagChange: (value: string) => void
  onAssetNameChange: (value: string) => void
  onAssetDescriptionChange: (value: string) => void
  onSiteRefChange: (value: string) => void
  onCreateClass: () => void
  onCreateType: () => void
  onCreateAsset: () => void
  isCreatingClass: boolean
  isCreatingType: boolean
  isCreatingAsset: boolean
}

function readinessBadgeClass(status: AssetReadinessSummaryResponse['readinessStatus']): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
}

function readinessLabel(status: AssetReadinessSummaryResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

export function AssetRegistryPanel({
  canManage,
  classes,
  types,
  assets,
  readinessByAssetId,
  isLoading,
  isReadinessLoading,
  className,
  classDescription,
  classKeyManualOverride,
  confirmedClassKey,
  selectedClassId,
  typeName,
  typeDescription,
  typeKeyManualOverride,
  confirmedTypeKey,
  selectedTypeId,
  assetTag,
  assetName,
  assetDescription,
  siteRef,
  onClassNameChange,
  onClassDescriptionChange,
  onClassKeyManualOverrideChange,
  onSelectedClassIdChange,
  onTypeNameChange,
  onTypeDescriptionChange,
  onTypeKeyManualOverrideChange,
  onSelectedTypeIdChange,
  onAssetTagChange,
  onAssetNameChange,
  onAssetDescriptionChange,
  onSiteRefChange,
  onCreateClass,
  onCreateType,
  onCreateAsset,
  isCreatingClass,
  isCreatingType,
  isCreatingAsset,
}: AssetRegistryPanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading asset registry…</p>
  }

  const classOptions = classes.map((item) => ({
    value: item.assetClassId,
    label: `${item.name} (${item.classKey})`,
  }))
  const typeOptions = types.map((item) => ({
    value: item.assetTypeId,
    label: `${item.className} / ${item.name}`,
  }))

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Asset classes</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {classes.length === 0 ? (
            <li className="text-slate-400">No asset classes yet.</li>
          ) : (
            classes.map((item) => (
              <li key={item.assetClassId} className="rounded-lg border border-slate-800 p-3">
                <div className="font-medium">{item.name}</div>
                <div className="text-slate-400">{item.classKey}</div>
              </li>
            ))
          )}
        </ul>
        {canManage ? (
          <div className="mt-4 space-y-2">
            <GeneratedKeyField
              sourceLabel={className}
              generatedKey={slugifyKey(className)}
              confirmedKey={confirmedClassKey}
              manualOverride={classKeyManualOverride}
              onManualOverrideChange={onClassKeyManualOverrideChange}
              showAdvancedKey
              label="Class key"
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Class name"
              value={className}
              onChange={(event) => onClassNameChange(event.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Description"
              value={classDescription}
              onChange={(event) => onClassDescriptionChange(event.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              disabled={isCreatingClass}
              onClick={onCreateClass}
            >
              {isCreatingClass ? 'Creating…' : 'Create class'}
            </button>
          </div>
        ) : null}
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <h2 className="text-lg font-medium text-white">Asset types</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {types.length === 0 ? (
            <li className="text-slate-400">No asset types yet.</li>
          ) : (
            types.map((item) => (
              <li key={item.assetTypeId} className="rounded-lg border border-slate-800 p-3">
                <div className="font-medium">{item.name}</div>
                <div className="text-slate-400">
                  {item.className} · {item.typeKey}
                </div>
              </li>
            ))
          )}
        </ul>
        {canManage ? (
          <div className="mt-4 space-y-2">
            <ControlledSelect
              label="Asset class"
              value={selectedClassId}
              onChange={onSelectedClassIdChange}
              options={classOptions}
              emptyLabel="Select asset class"
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            />
            <GeneratedKeyField
              sourceLabel={typeName}
              generatedKey={slugifyKey(typeName)}
              confirmedKey={confirmedTypeKey}
              manualOverride={typeKeyManualOverride}
              onManualOverrideChange={onTypeKeyManualOverrideChange}
              showAdvancedKey
              label="Type key"
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Type name"
              value={typeName}
              onChange={(event) => onTypeNameChange(event.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Description"
              value={typeDescription}
              onChange={(event) => onTypeDescriptionChange(event.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
              disabled={isCreatingType || !selectedClassId}
              onClick={onCreateType}
            >
              {isCreatingType ? 'Creating…' : 'Create type'}
            </button>
          </div>
        ) : null}
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2">
        <h2 className="text-lg font-medium text-white">Assets</h2>
        <ul className="mt-4 space-y-2 text-sm">
          {assets.length === 0 ? (
            <li className="text-slate-400">No assets registered yet.</li>
          ) : (
            assets.map((item) => {
              const readiness = readinessByAssetId[item.assetId]
              return (
                <li key={item.assetId} className="rounded-lg border border-slate-800 p-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className="font-medium">{item.assetTag}</span>
                    <span className="text-slate-300">{item.name}</span>
                    <span className="rounded-full bg-slate-800 px-2 py-0.5 text-xs text-slate-300">
                      {item.lifecycleStatus}
                    </span>
                    {isReadinessLoading ? (
                      <span className="rounded-full bg-slate-800 px-2 py-0.5 text-xs text-slate-400">
                        Readiness…
                      </span>
                    ) : readiness ? (
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${readinessBadgeClass(readiness.readinessStatus)}`}
                        title={readiness.primaryBlockerMessage ?? undefined}
                      >
                        {readinessLabel(readiness.readinessStatus)}
                        {readiness.blockerCount > 0 ? ` (${readiness.blockerCount})` : ''}
                      </span>
                    ) : null}
                  </div>
                  <div className="text-slate-400">
                    {item.className} / {item.typeName}
                    {item.siteRef ? ` · ${item.siteRef}` : ''}
                  </div>
                  {readiness?.primaryBlockerMessage ? (
                    <p className="mt-2 text-xs text-amber-200/90">{readiness.primaryBlockerMessage}</p>
                  ) : null}
                </li>
              )
            })
          )}
        </ul>
        {canManage ? (
          <div className="mt-4 grid gap-2 md:grid-cols-2">
            <ControlledSelect
              label="Asset type"
              value={selectedTypeId}
              onChange={onSelectedTypeIdChange}
              options={typeOptions}
              emptyLabel="Select asset type"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
            />
            <input
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Asset tag"
              value={assetTag}
              onChange={(event) => onAssetTagChange(event.target.value)}
            />
            <input
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Asset name"
              value={assetName}
              onChange={(event) => onAssetNameChange(event.target.value)}
            />
            <input
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
              placeholder="Description"
              value={assetDescription}
              onChange={(event) => onAssetDescriptionChange(event.target.value)}
            />
            <input
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
              placeholder="Site reference (optional)"
              value={siteRef}
              onChange={(event) => onSiteRefChange(event.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50 md:col-span-2"
              disabled={isCreatingAsset || !selectedTypeId}
              onClick={onCreateAsset}
            >
              {isCreatingAsset ? 'Creating…' : 'Create asset'}
            </button>
          </div>
        ) : null}
      </section>
    </div>
  )
}
