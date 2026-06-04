import { AlertTriangle, CheckCircle2, ClipboardCheck, FileText, Gauge, History, MapPin, ShieldCheck, Truck, Wrench, XCircle } from 'lucide-react'
import { ProfileDetailsLayout, type DetailTone } from '@stl/shared-ui'

import type {
  AssetFieldContextResponse,
  AssetInstalledComponentResponse,
  AssetReadinessHistoryResponse,
  AssetReadinessResponse,
  AssetResponse,
} from '../api/types'

interface AssetDetailsPageProps {
  asset: AssetResponse
  readiness: AssetReadinessResponse | null
  isReadinessLoading: boolean
  readinessHistory: AssetReadinessHistoryResponse | null
  isReadinessHistoryLoading: boolean
  fieldContext: AssetFieldContextResponse | null
  installedComponents: AssetInstalledComponentResponse[] | null
}

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString()
}

function formatStatusFieldKey(value: string): string {
  return value
    .replace(/[_-]+/g, ' ')
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .toLowerCase()
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatCountLabel(label: string, items: readonly unknown[]): string {
  return `${label}: ${items.length}`
}

export function AssetDetailsPage({
  asset,
  readiness,
  isReadinessLoading,
  readinessHistory,
  isReadinessHistoryLoading,
  fieldContext,
  installedComponents,
}: AssetDetailsPageProps) {
  const blockers = readiness?.blockers ?? []
  const isReady = readiness?.readinessStatus === 'ready' && blockers.length === 0
  const decisionTone: DetailTone = isReadinessLoading ? 'warn' : isReady ? 'good' : 'bad'
  const decisionLabel = isReadinessLoading ? 'Checking' : isReady ? 'Ready' : 'Blocked'
  const blockedChecks = readiness
    ? [
        readiness.signals.openCriticalDefectCount > 0,
        readiness.signals.activeWorkOrderCount > 0,
        readiness.signals.pmOverdueCount > 0,
        readiness.signals.failedInspectionCount > 0,
        blockers.length > 0,
      ].filter(Boolean).length
    : 0
  const allowedChecks = readiness ? Math.max(0, 5 - blockedChecks) : 0
  const components = installedComponents ?? []
  const componentLabelsById = new Map(
    components.map((component) => [
      component.componentId,
      `${component.componentNumber} · ${component.name}`,
    ]),
  )
  const snapshotFields = [
    { label: 'Unit number', value: asset.assetTag, source: 'Asset registry' },
    { label: 'Lifecycle status', value: humanize(asset.lifecycleStatus), source: 'Asset registry' },
    { label: 'Asset class', value: asset.className, source: 'Selectable catalog' },
    { label: 'Asset type', value: asset.typeName, source: 'Selectable catalog' },
    { label: 'Description', value: asset.description || 'No description provided', source: 'Asset profile' },
    ...(fieldContext?.fields?.slice(0, 10).map((field) => ({
      label: field.key,
      value: field.displayValue || 'Not recorded',
      source: field.sourceOfTruth || humanize(field.source),
    })) ?? []),
  ]

  return (
    <ProfileDetailsLayout
      testId="asset-details-page"
      backLabel="Assets"
      backTo="/assets/drawer"
      breadcrumbs={[asset.className, asset.assetTag]}
      icon={<Truck className="h-8 w-8" />}
      title={asset.name}
      subtitle={(
        <span className="flex flex-wrap items-center gap-2">
          <MapPin className="h-4 w-4 text-slate-400" />
          <span>{asset.siteRef ?? 'Unassigned site'}</span>
          <span className="text-slate-600">-</span>
          <span>{asset.className} / {asset.typeName}</span>
        </span>
      )}
      badges={[
        { label: asset.lifecycleStatus, tone: 'neutral' },
        { label: decisionLabel, tone: decisionTone },
      ]}
      metrics={[
        {
          label: 'Open defects',
          value: readiness?.signals.openCriticalDefectCount ?? 0,
          hint: 'Critical defects',
          icon: <AlertTriangle className="h-5 w-5" />,
          tone: (readiness?.signals.openCriticalDefectCount ?? 0) > 0 ? 'warn' : 'good',
        },
        {
          label: 'Open work orders',
          value: readiness?.signals.activeWorkOrderCount ?? 0,
          hint: 'Active',
          icon: <Wrench className="h-5 w-5" />,
          tone: (readiness?.signals.activeWorkOrderCount ?? 0) > 0 ? 'info' : 'good',
        },
        {
          label: 'PM due',
          value: readiness?.signals.pmDueCount ?? 0,
          hint: `Overdue: ${readiness?.signals.pmOverdueCount ?? 0}`,
          icon: <Gauge className="h-5 w-5" />,
          tone: (readiness?.signals.pmOverdueCount ?? 0) > 0 ? 'bad' : (readiness?.signals.pmDueCount ?? 0) > 0 ? 'warn' : 'good',
        },
        {
          label: 'Inspection state',
          value: (readiness?.signals.failedInspectionCount ?? 0) > 0 ? 'Failing' : 'Pass',
          hint: `Failed: ${readiness?.signals.failedInspectionCount ?? 0}`,
          icon: <ClipboardCheck className="h-5 w-5" />,
          tone: (readiness?.signals.failedInspectionCount ?? 0) > 0 ? 'bad' : 'good',
        },
      ]}
      tabs={['Overview', 'Inspections', 'Work Orders', 'PM Plan', 'Defects', 'Documents', 'History']}
      snapshotTitle="Asset snapshot"
      snapshotSubtitle="Core identity, platform-populated fields, and live operating counters."
      snapshotFields={snapshotFields}
      decisionTitle="Readiness decision"
      decisionBadge={{ label: decisionLabel, tone: decisionTone }}
      decisionIcon={isReady ? <CheckCircle2 className="h-5 w-5 text-emerald-300" /> : <XCircle className="h-5 w-5 text-red-300" />}
      decisionSummary={isReadinessLoading ? 'Loading readiness' : isReady ? 'No maintenance blockers' : 'Hold dispatch until blockers clear'}
      decisionDetail={
        isReadinessLoading
          ? 'Readiness signals are loading for this asset.'
          : blockers[0]?.message ?? 'Open maintenance blockers should be resolved before this asset returns to service.'
      }
      allowedChecks={allowedChecks}
      blockedChecks={blockedChecks}
      railSections={[
        {
          title: 'Activity',
          icon: <History className="h-5 w-5" />,
          content: (
            <div className="space-y-2 text-sm text-slate-300">
              <p className="rounded-lg bg-slate-900/50 p-2">Last updated: {formatDateTime(asset.updatedAt)}</p>
              <p className="rounded-lg bg-slate-900/50 p-2">Created: {formatDateTime(asset.createdAt)}</p>
            </div>
          ),
        },
        {
          title: 'Compliance links',
          icon: <ShieldCheck className="h-5 w-5" />,
          content: (
            <div className="space-y-3">
              <p className="text-sm text-slate-400">Rulepack alignment and required evidence mapping are shown in Compliance Core integrations.</p>
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <FileText className="h-4 w-4" />
                Latest readiness basis: {readiness?.readinessBasis?.replace(/_/g, ' ') ?? 'Unavailable'}
              </div>
              <div className="flex items-center gap-2 text-xs text-slate-500">
                {isReady ? <CheckCircle2 className="h-4 w-4 text-emerald-300" /> : <XCircle className="h-4 w-4 text-amber-300" />}
                Calculated at: {readiness?.calculatedAt ?? 'Unavailable'}
              </div>
            </div>
          ),
        },
        {
          title: 'Components',
          icon: <Truck className="h-5 w-5" />,
          content: components.length > 0 ? (
            <div className="space-y-3">
              <p className="text-xs text-slate-500">
                Installed components recorded against this asset and any nested child components.
              </p>
              <ul className="space-y-2">
                {components.map((component) => (
                  <li key={component.componentId} className="rounded-lg border border-slate-800 bg-slate-900/40 p-3">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <div className="text-sm font-medium text-white">
                          {component.componentNumber} · {component.name}
                        </div>
                        <div className="mt-1 text-xs text-slate-400">
                          {humanize(component.componentType)} · {humanize(component.status)} · {humanize(component.condition)}
                        </div>
                      </div>
                      <div className="text-[11px] uppercase tracking-wide text-slate-500">
                        {component.parentComponentId
                          ? `Parent: ${componentLabelsById.get(component.parentComponentId) ?? component.parentComponentId}`
                          : 'Root component'}
                      </div>
                    </div>
                    <dl className="mt-3 grid gap-2 text-xs text-slate-300 md:grid-cols-2">
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">Make / model</dt>
                        <dd className="mt-1 text-slate-100">{[component.make, component.model].filter(Boolean).join(' · ') || 'Not recorded'}</dd>
                      </div>
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">Serial / part</dt>
                        <dd className="mt-1 text-slate-100">{[component.serialNumber, component.partNumberSnapshot].filter(Boolean).join(' · ') || 'Not recorded'}</dd>
                      </div>
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">Installed</dt>
                        <dd className="mt-1 text-slate-100">{formatDateTime(component.installDate)}</dd>
                      </div>
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">Removed</dt>
                        <dd className="mt-1 text-slate-100">{formatDateTime(component.removedDate)}</dd>
                      </div>
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">Installed by</dt>
                        <dd className="mt-1 text-slate-100">{component.installedByPersonId ?? 'Not recorded'}</dd>
                      </div>
                      <div className="rounded-md bg-slate-950/60 p-2">
                        <dt className="text-[10px] uppercase tracking-wide text-slate-500">References</dt>
                        <dd className="mt-1 text-slate-100">
                          {[
                            formatCountLabel('Replacement parts', component.replacementPartRefs),
                            formatCountLabel('Documents', component.documentRefs),
                            formatCountLabel('Defects', component.defectRefs),
                            formatCountLabel('Work orders', component.workOrderRefs),
                          ].join(' · ')}
                        </dd>
                      </div>
                    </dl>
                  </li>
                ))}
              </ul>
            </div>
          ) : (
            <p className="text-sm text-slate-400">No installed components recorded yet.</p>
          ),
        },
        {
          title: 'Readiness history',
          icon: <History className="h-5 w-5" />,
          content: isReadinessHistoryLoading ? (
            <p className="text-sm text-slate-400">Loading readiness history…</p>
          ) : readinessHistory ? (
            <div className="space-y-3">
              <p className="text-xs text-slate-500">
                {readinessHistory.totalCount} status change{readinessHistory.totalCount === 1 ? '' : 's'} captured
                for {readinessHistory.assetTag}.
              </p>
              {readinessHistory.items.length === 0 ? (
                <p className="text-sm text-slate-400">No readiness history captured yet.</p>
              ) : (
                <ul className="space-y-2">
                  {readinessHistory.items.map((item) => (
                    <li key={item.entryId} className="rounded-lg border border-slate-800 bg-slate-900/40 p-3">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className="rounded bg-slate-800 px-2 py-0.5 text-[11px] font-semibold uppercase tracking-wide text-slate-300">
                          {formatStatusFieldKey(item.statusFieldKey)}
                        </span>
                        <span className="text-sm font-medium text-white">{item.statusValueKey}</span>
                      </div>
                      <div className="mt-2 text-xs text-slate-500">
                        Changed {new Date(item.changedAt).toLocaleString()}
                        {item.changedByPersonId ? ` · by ${item.changedByPersonId}` : ''}
                      </div>
                      {item.notes ? <p className="mt-2 text-xs text-slate-400">{item.notes}</p> : null}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          ) : (
            <p className="text-sm text-slate-400">Readiness history unavailable.</p>
          ),
        },
      ]}
    />
  )
}
