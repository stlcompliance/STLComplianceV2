import { useEffect, useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { FileSpreadsheet, Landmark, Scale, Users } from 'lucide-react'
import { useLocation } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  listPayrollBatchLines,
  listPayrollBatches,
  listPayrollCalendars,
  listPayrollCodeMappings,
  listPayrollExportPackets,
  listPayrollJournalSnapshots,
  type PayrollBatch,
  type PayrollBatchLine,
  type PayrollCalendar,
  type PayrollCodeMapping,
  type PayrollExportPacket,
  type PayrollJournalSnapshot,
} from '../api/client'
import { loadSession } from '../auth/sessionStorage'

function formatMoney(value: number | null | undefined): string {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value ?? 0)
}

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return 'n/a'
  }

  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      })
}

function titleize(value: string): string {
  return value
    .split(/[_-]/g)
    .filter(Boolean)
    .map((part) => part.slice(0, 1).toUpperCase() + part.slice(1))
    .join(' ')
}

function PageHeader({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col gap-3 border-b border-slate-700/70 pb-4 lg:flex-row lg:items-end lg:justify-between">
      <div className="space-y-2">
        <p className="ledgarr-label">{eyebrow}</p>
        <h1 className="text-2xl font-semibold text-slate-50">{title}</h1>
        <p className="max-w-4xl text-sm text-slate-300">{description}</p>
      </div>
      {action}
    </div>
  )
}

function Panel({
  title,
  icon,
  children,
}: {
  title: string
  icon: ReactNode
  children: ReactNode
}) {
  return (
    <section className="ledgarr-panel">
      <div className="ledgarr-panel-inner space-y-3">
        <div className="flex items-center gap-2">
          {icon}
          <h2 className="text-base font-semibold text-slate-50">{title}</h2>
        </div>
        {children}
      </div>
    </section>
  )
}

function Metric({
  label,
  value,
  hint,
}: {
  label: string
  value: string | number
  hint: string
}) {
  return (
    <div className="ledgarr-panel">
      <div className="ledgarr-panel-inner">
        <p className="ledgarr-label">{label}</p>
        <p className="mt-2 text-3xl font-semibold text-slate-50">{value}</p>
        <p className="mt-2 text-sm text-slate-300">{hint}</p>
      </div>
    </div>
  )
}

function EmptyState({ title, detail }: { title: string; detail?: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-700/80 p-4 text-sm text-slate-400">
      <p>{title}</p>
      {detail ? <p className="mt-2 text-[var(--color-text-muted)]">{detail}</p> : null}
    </div>
  )
}

function StatusPill({ status }: { status: string }) {
  const lowered = status.toLowerCase()
  const className =
    lowered.includes('failed') ||
    lowered.includes('rejected') ||
    lowered.includes('blocked') ||
    lowered.includes('missing')
      ? 'ledgarr-pill warning'
      : 'ledgarr-pill'
  return <span className={className}>{titleize(status)}</span>
}

function ScopeNote({ children }: { children: ReactNode }) {
  return (
    <div className="rounded-xl border border-slate-700/80 bg-slate-950/70 px-4 py-3 text-sm text-slate-300">
      <span className="font-medium text-slate-100">Page scope:</span> {children}
    </div>
  )
}

function TabStrip({
  tabs,
  activeTab,
  onChange,
}: {
  tabs: readonly string[]
  activeTab: string
  onChange: (tab: string) => void
}) {
  return (
    <div className="flex flex-wrap gap-2">
      {tabs.map((tab) => {
        const active = tab === activeTab
        return (
          <button
            key={tab}
            type="button"
            onClick={() => onChange(tab)}
            className={`rounded-full border px-3 py-1.5 text-sm transition ${
              active
                ? 'border-teal-400/50 bg-teal-400/12 text-teal-100'
                : 'border-slate-700 bg-slate-900/70 text-slate-300 hover:border-slate-600 hover:text-slate-100'
            }`}
          >
            {tab}
          </button>
        )
      })}
    </div>
  )
}

function PayrollCalendarTable({ calendars }: { calendars: PayrollCalendar[] }) {
  if (calendars.length === 0) {
    return <EmptyState title="No payroll calendars are configured yet." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Calendar</th>
            <th>Frequency</th>
            <th>Period</th>
            <th>Pay Date</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {calendars.map((calendar) => (
            <tr key={calendar.id}>
              <td className="font-semibold text-slate-50">{calendar.name}</td>
              <td>{titleize(calendar.frequency)}</td>
              <td>
                {formatDate(calendar.periodStartDate)} to {formatDate(calendar.periodEndDate)}
              </td>
              <td>{formatDate(calendar.payDate)}</td>
              <td>
                <StatusPill status={calendar.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PayrollCodeMappingTable({ mappings }: { mappings: PayrollCodeMapping[] }) {
  if (mappings.length === 0) {
    return <EmptyState title="No payroll code mappings have been created." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>StaffArr Pay Code</th>
            <th>Provider Earning Code</th>
            <th>GL Account</th>
            <th>Cost Center</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {mappings.map((mapping) => (
            <tr key={mapping.id}>
              <td className="font-semibold text-slate-50">{mapping.staffArrPayCodeRef}</td>
              <td>{mapping.providerEarningCode}</td>
              <td>{mapping.glAccountRef}</td>
              <td>{mapping.costCenterRef ?? 'n/a'}</td>
              <td>
                <StatusPill status={mapping.active ? 'active' : 'inactive'} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PayrollBatchTable({
  batches,
  selectedBatchId,
  onSelectBatch,
}: {
  batches: PayrollBatch[]
  selectedBatchId: string | null
  onSelectBatch: (batchId: string) => void
}) {
  if (batches.length === 0) {
    return <EmptyState title="No payroll batches are available." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Batch</th>
            <th>Period</th>
            <th>Workers</th>
            <th>Gross Estimate</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {batches.map((batch) => (
            <tr
              key={batch.id}
              className={selectedBatchId === batch.id ? 'bg-slate-900/80' : undefined}
              onClick={() => onSelectBatch(batch.id)}
            >
              <td className="font-semibold text-slate-50">{formatDate(batch.payDate)}</td>
              <td>
                {formatDate(batch.periodStartDate)} to {formatDate(batch.periodEndDate)}
              </td>
              <td>{batch.totalWorkers}</td>
              <td>{formatMoney(batch.totalGrossEstimate)}</td>
              <td>
                <StatusPill status={batch.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PayrollExportPacketTable({ packets }: { packets: PayrollExportPacket[] }) {
  if (packets.length === 0) {
    return <EmptyState title="No payroll export packets have been generated for the selected batch." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Provider</th>
            <th>Format</th>
            <th>Exported</th>
            <th>Response</th>
          </tr>
        </thead>
        <tbody>
          {packets.map((packet) => (
            <tr key={packet.id}>
              <td className="font-semibold text-slate-50">{titleize(packet.providerKey)}</td>
              <td>{packet.exportFormat.toUpperCase()}</td>
              <td>{formatDate(packet.exportedAt)}</td>
              <td>
                <StatusPill status={packet.providerResponseStatus} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PayrollBatchLineTable({ lines }: { lines: PayrollBatchLine[] }) {
  if (lines.length === 0) {
    return <EmptyState title="No payroll lines have been collected for the selected batch." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>Worker</th>
            <th>Pay Code</th>
            <th>Minutes</th>
            <th>Gross Estimate</th>
            <th>Validation</th>
          </tr>
        </thead>
        <tbody>
          {lines.map((line) => (
            <tr key={line.id}>
              <td className="font-semibold text-slate-50">{line.workerNumber}</td>
              <td>{line.payCodeRef}</td>
              <td>{line.durationMinutes}</td>
              <td>{formatMoney(line.grossEstimate)}</td>
              <td>
                <StatusPill status={line.validationStatus} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function PayrollJournalSnapshotTable({ snapshots }: { snapshots: PayrollJournalSnapshot[] }) {
  if (snapshots.length === 0) {
    return <EmptyState title="No payroll journal snapshots have been posted for the selected batch." />
  }

  return (
    <div className="ledgarr-panel overflow-hidden">
      <table className="ledgarr-table">
        <thead>
          <tr>
            <th>GL Account</th>
            <th>Cost Object</th>
            <th>Product</th>
            <th>Debit</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {snapshots.map((snapshot) => (
            <tr key={snapshot.id}>
              <td className="font-semibold text-slate-50">{snapshot.glAccountRef}</td>
              <td>
                {snapshot.costObjectType} · {snapshot.costObjectRef}
              </td>
              <td>{titleize(snapshot.productKey)}</td>
              <td>{formatMoney(snapshot.debitAmount)}</td>
              <td>
                <StatusPill status={snapshot.status} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function tabFromPath(pathname: string): string {
  if (pathname.includes('/code-mappings')) {
    return 'Code Mappings'
  }
  if (pathname.includes('/batches')) {
    return 'Batches'
  }
  if (pathname.includes('/calendars')) {
    return 'Calendars'
  }
  return 'Batches'
}

export function PayrollPage() {
  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
  const location = useLocation()
  const [activeTab, setActiveTab] = useState(tabFromPath(location.pathname))
  const [selectedBatchId, setSelectedBatchId] = useState<string | null>(null)

  const calendarsQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'calendars', accessToken],
    queryFn: () => listPayrollCalendars(accessToken),
    enabled: Boolean(accessToken),
  })
  const mappingsQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'code-mappings', accessToken],
    queryFn: () => listPayrollCodeMappings(accessToken),
    enabled: Boolean(accessToken),
  })
  const batchesQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'batches', accessToken],
    queryFn: () => listPayrollBatches(accessToken),
    enabled: Boolean(accessToken),
  })

  useEffect(() => {
    setActiveTab(tabFromPath(location.pathname))
  }, [location.pathname])

  useEffect(() => {
    if (!selectedBatchId && batchesQuery.data?.[0]?.id) {
      setSelectedBatchId(batchesQuery.data[0].id)
    }
  }, [batchesQuery.data, selectedBatchId])

  const batchLinesQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'batch-lines', accessToken, selectedBatchId],
    queryFn: () => listPayrollBatchLines(accessToken, selectedBatchId!),
    enabled: Boolean(accessToken && selectedBatchId),
  })
  const exportPacketsQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'export-packets', accessToken, selectedBatchId],
    queryFn: () => listPayrollExportPackets(accessToken, selectedBatchId!),
    enabled: Boolean(accessToken && selectedBatchId),
  })
  const journalSnapshotsQuery = useQuery({
    queryKey: ['ledgarr', 'payroll', 'journal-snapshots', accessToken, selectedBatchId],
    queryFn: () => listPayrollJournalSnapshots(accessToken, selectedBatchId!),
    enabled: Boolean(accessToken && selectedBatchId),
  })

  const batches = batchesQuery.data ?? []
  const selectedBatch = batches.find((batch) => batch.id === selectedBatchId) ?? null
  const activeMappingCount = (mappingsQuery.data ?? []).filter((mapping) => mapping.active).length
  const openBatchCount = batches.filter((batch) => !['closed', 'provider_accepted'].includes(batch.status)).length
  const totalGrossEstimate = batches.reduce((sum, batch) => sum + (batch.totalGrossEstimate ?? 0), 0)
  const tabs = ['Calendars', 'Code Mappings', 'Batches', 'Journal Snapshots'] as const

  return (
    <div className="ledgarr-page">
      <PageHeader
        eyebrow="Payroll financials"
        title="Payroll preparation, export, and ledger snapshots"
        description="Manage payroll calendars, exports, journal snapshots, and labor-cost settlement."
        action={<span className="ledgarr-pill">Finance controls</span>}
      />
      <div className="ledgarr-grid cols-4">
        <Metric label="Payroll calendars" value={calendarsQuery.data?.length ?? 0} hint="Active pay-cycle definitions" />
        <Metric label="Active mappings" value={activeMappingCount} hint="Pay codes mapped to provider and GL outputs" />
        <Metric label="Open batches" value={openBatchCount} hint="Payroll batches still in review, export, or correction flow" />
        <Metric label="Gross estimate" value={formatMoney(totalGrossEstimate)} hint="Current batch estimate across visible payroll batches" />
      </div>
      <TabStrip tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />
      {calendarsQuery.isError ||
      mappingsQuery.isError ||
      batchesQuery.isError ||
      batchLinesQuery.isError ||
      exportPacketsQuery.isError ||
      journalSnapshotsQuery.isError ? (
        <ApiErrorCallout
          title="Unable to load payroll financials"
          message={
            getErrorMessage(calendarsQuery.error, '') ||
            getErrorMessage(mappingsQuery.error, '') ||
            getErrorMessage(batchesQuery.error, '') ||
            getErrorMessage(batchLinesQuery.error, '') ||
            getErrorMessage(exportPacketsQuery.error, '') ||
            getErrorMessage(journalSnapshotsQuery.error, 'Failed to load payroll financials.')
          }
        />
      ) : null}
      {activeTab === 'Calendars' ? (
        <Panel title="Payroll calendars" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
          <PayrollCalendarTable calendars={calendarsQuery.data ?? []} />
        </Panel>
      ) : null}
      {activeTab === 'Code Mappings' ? (
        <Panel title="Payroll code mappings" icon={<Scale className="h-4 w-4 text-teal-300" />}>
          <PayrollCodeMappingTable mappings={mappingsQuery.data ?? []} />
        </Panel>
      ) : null}
      {activeTab === 'Batches' ? (
        <div className="space-y-6">
          <Panel title="Payroll batches" icon={<Users className="h-4 w-4 text-teal-300" />}>
            <PayrollBatchTable
              batches={batches}
              selectedBatchId={selectedBatchId}
              onSelectBatch={setSelectedBatchId}
            />
          </Panel>
          <div className="ledgarr-grid cols-2">
            <Panel title="Selected batch summary" icon={<Users className="h-4 w-4 text-teal-300" />}>
              {selectedBatch ? (
                <div className="space-y-3 text-sm text-slate-300">
                  <p>
                    <strong className="text-slate-100">Period:</strong>{' '}
                    {formatDate(selectedBatch.periodStartDate)} to {formatDate(selectedBatch.periodEndDate)}
                  </p>
                  <p>
                    <strong className="text-slate-100">Pay date:</strong> {formatDate(selectedBatch.payDate)}
                  </p>
                  <p>
                    <strong className="text-slate-100">Status:</strong> <StatusPill status={selectedBatch.status} />
                  </p>
                  <p>
                    <strong className="text-slate-100">Gross estimate:</strong> {formatMoney(selectedBatch.totalGrossEstimate)}
                  </p>
                </div>
              ) : (
                <EmptyState title="Select a payroll batch to inspect its finance outputs." />
              )}
            </Panel>
            <Panel title="Export packets" icon={<FileSpreadsheet className="h-4 w-4 text-teal-300" />}>
              <PayrollExportPacketTable packets={exportPacketsQuery.data ?? []} />
            </Panel>
          </div>
        </div>
      ) : null}
      {activeTab === 'Journal Snapshots' ? (
        <div className="space-y-6">
          <Panel title="Payroll journal snapshots" icon={<Landmark className="h-4 w-4 text-teal-300" />}>
            <PayrollJournalSnapshotTable snapshots={journalSnapshotsQuery.data ?? []} />
          </Panel>
          <Panel title="Underlying payroll lines" icon={<Users className="h-4 w-4 text-teal-300" />}>
            <PayrollBatchLineTable lines={batchLinesQuery.data ?? []} />
          </Panel>
        </div>
      ) : null}
      <ScopeNote>
        Payroll export packets, payroll journals, and labor-cost allocations are handled here. Worker identity, approved time, and labor context come from the timekeeping workflow.
      </ScopeNote>
    </div>
  )
}
