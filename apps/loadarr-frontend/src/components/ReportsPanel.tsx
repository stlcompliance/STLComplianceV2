import { useMemo, useState, type ReactNode } from 'react'
import { Activity, AlertTriangle, BarChart3, CalendarRange, DatabaseZap, MapPin, Package, ShieldCheck, Search } from 'lucide-react'
import { ControlledSelect, FormField } from '@stl/shared-ui'
import {
  buildCountVarianceRows,
  buildInventoryByItemRows,
  buildInventoryByLocationRows,
  buildInventoryByStatusRows,
  buildMovementHistoryRows,
  buildOriginHistoryRows,
  type LoadArrReportAdjustment,
  type LoadArrReportCount,
  type LoadArrReportHold,
  type LoadArrReportInventoryBalance,
  type LoadArrReportUnexplainedInventoryRecord,
} from '../reports'

type LoadArrReportsSummary = {
  metrics: {
    activeLocations: number
    quantityOnHand: number
    quantityCommitted: number
    quantityBlocked: number
    openTasks: number
    openHolds: number
    unexplainedInventory: number
  }
  locations: Array<{
    id: string
    name: string
    staffarrSiteNameSnapshot: string
  }>
  inventory: LoadArrReportInventoryBalance[]
  holds: LoadArrReportHold[]
  unexplainedInventory: LoadArrReportUnexplainedInventoryRecord[]
}

type ReportsPanelProps = {
  summary: LoadArrReportsSummary
  counts: LoadArrReportCount[]
  adjustments: LoadArrReportAdjustment[]
}

type ReportFilterState = {
  locationId: string
  itemId: string
  state: string
  query: string
  sinceUtc: string
  untilUtc: string
}

const allValue = 'all'

export function ReportsPanel({ summary, counts, adjustments }: ReportsPanelProps) {
  const locationById = useMemo(
    () => new Map(summary.locations.map((location) => [location.id, location] as const)),
    [summary.locations],
  )

  const reportInventory = useMemo(
    () =>
      summary.inventory.map((item) => ({
        ...item,
        staffarrSiteNameSnapshot: locationById.get(item.locationId)?.staffarrSiteNameSnapshot ?? 'Unknown site',
      })),
    [locationById, summary.inventory],
  )

  const [filters, setFilters] = useState<ReportFilterState>({
    locationId: allValue,
    itemId: allValue,
    state: allValue,
    query: '',
    sinceUtc: '',
    untilUtc: '',
  })

  const locationOptions = useMemo(
    () => [
      { value: allValue, label: 'All locations' },
      ...summary.locations.map((location) => ({
        value: location.id,
        label: `${location.staffarrSiteNameSnapshot} · ${location.name}`,
      })),
    ],
    [summary.locations],
  )

  const itemOptions = useMemo(
    () => [
      { value: allValue, label: 'All items' },
      ...Array.from(new Map(reportInventory.map((item) => [item.supplyarrItemId, item])).values()).map((item) => ({
        value: item.supplyarrItemId,
        label: `${item.supplyarrItemId} · ${item.itemNameSnapshot}`,
      })),
    ],
    [reportInventory],
  )

  const stateOptions = useMemo(
    () => [
      { value: allValue, label: 'All statuses' },
      ...Array.from(new Set(reportInventory.map((item) => item.state)))
        .sort((left, right) => left.localeCompare(right))
        .map((state) => ({ value: state, label: state.replaceAll('_', ' ') })),
    ],
    [reportInventory],
  )

  const inventoryFilter = {
    locationId: filters.locationId === allValue ? undefined : filters.locationId,
    itemId: filters.itemId === allValue ? undefined : filters.itemId,
    state: filters.state === allValue ? undefined : filters.state,
    query: filters.query || undefined,
  }

  const inventoryByLocation = useMemo(
    () => buildInventoryByLocationRows(reportInventory, inventoryFilter),
    [inventoryFilter, reportInventory],
  )
  const inventoryByItem = useMemo(
    () => buildInventoryByItemRows(reportInventory, inventoryFilter),
    [inventoryFilter, reportInventory],
  )
  const inventoryByStatus = useMemo(
    () => buildInventoryByStatusRows(reportInventory, inventoryFilter),
    [inventoryFilter, reportInventory],
  )
  const originHistory = useMemo(
    () => buildOriginHistoryRows(reportInventory, inventoryFilter),
    [inventoryFilter, reportInventory],
  )
  const movementHistory = useMemo(
    () =>
      buildMovementHistoryRows(counts, adjustments, summary.holds, summary.unexplainedInventory, {
        locationId: filters.locationId === allValue ? undefined : filters.locationId,
        locationNameSnapshot:
          filters.locationId === allValue
            ? undefined
            : summary.locations.find((location) => location.id === filters.locationId)?.name,
        itemId: filters.itemId === allValue ? undefined : filters.itemId,
        state: filters.state === allValue ? undefined : filters.state,
        sinceUtc: filters.sinceUtc || undefined,
        untilUtc: filters.untilUtc || undefined,
        query: filters.query || undefined,
      }),
    [adjustments, counts, filters.itemId, filters.locationId, filters.query, filters.sinceUtc, filters.state, filters.untilUtc, summary.holds, summary.locations, summary.unexplainedInventory],
  )
  const countVariance = useMemo(
    () =>
      buildCountVarianceRows(counts, adjustments, {
        locationId: filters.locationId === allValue ? undefined : filters.locationId,
        itemId: filters.itemId === allValue ? undefined : filters.itemId,
        state: filters.state === allValue ? undefined : filters.state,
        sinceUtc: filters.sinceUtc || undefined,
        untilUtc: filters.untilUtc || undefined,
        query: filters.query || undefined,
      }),
    [adjustments, counts, filters.itemId, filters.locationId, filters.query, filters.sinceUtc, filters.state, filters.untilUtc],
  )

  const filteredHolds = useMemo(() => {
    return summary.holds.filter((hold) => {
      if (filters.locationId !== allValue && hold.locationNameSnapshot !== summary.locations.find((location) => location.id === filters.locationId)?.name) {
        return false
      }

      if (filters.itemId !== allValue && hold.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters.state !== allValue && hold.status !== filters.state) {
        return false
      }

      if (filters.sinceUtc && Date.parse(hold.openedAtUtc) < Date.parse(filters.sinceUtc)) {
        return false
      }

      if (filters.untilUtc && Date.parse(hold.openedAtUtc) > Date.parse(filters.untilUtc)) {
        return false
      }

      if (filters.query) {
        const needle = filters.query.trim().toLowerCase()
        return [hold.holdType, hold.locationNameSnapshot, hold.supplyarrItemId, hold.reason, hold.sourceReference]
          .join(' ')
          .toLowerCase()
          .includes(needle)
      }

      return true
    })
  }, [filters.itemId, filters.locationId, filters.query, filters.sinceUtc, filters.state, filters.untilUtc, summary.holds, summary.locations])

  const filteredUnexplained = useMemo(() => {
    return summary.unexplainedInventory.filter((record) => {
      if (filters.locationId !== allValue && record.warehouseLocationId !== filters.locationId) {
        return false
      }

      if (filters.itemId !== allValue && record.supplyarrItemId !== filters.itemId) {
        return false
      }

      if (filters.state !== allValue && record.status !== filters.state) {
        return false
      }

      if (filters.sinceUtc && Date.parse(record.discoveredAtUtc) < Date.parse(filters.sinceUtc)) {
        return false
      }

      if (filters.untilUtc && Date.parse(record.discoveredAtUtc) > Date.parse(filters.untilUtc)) {
        return false
      }

      if (filters.query) {
        const needle = filters.query.trim().toLowerCase()
        return [
          record.recordNumber,
          record.discoverySource,
          record.staffarrSiteNameSnapshot,
          record.locationNameSnapshot,
          record.itemNameSnapshot,
          record.reasonCode,
          record.resolutionState,
          record.evidenceSummary,
        ]
          .join(' ')
          .toLowerCase()
          .includes(needle)
      }

      return true
    })
  }, [filters.itemId, filters.locationId, filters.query, filters.sinceUtc, filters.state, filters.untilUtc, summary.unexplainedInventory])

  return (
    <section className="reports-layout" aria-label="Records and operations">
      <article className="workflow-panel">
        <div className="section-heading">
          <BarChart3 aria-hidden="true" />
          <h2>Records and operations</h2>
        </div>

        <div className="form-grid">
          <FormField label="Location" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <ControlledSelect
              value={filters.locationId}
              onChange={(value) => setFilters((current) => ({ ...current, locationId: value }))}
              options={locationOptions}
              className={fieldControlClassName}
            />
          </FormField>

          <FormField label="Item" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <ControlledSelect
              value={filters.itemId}
              onChange={(value) => setFilters((current) => ({ ...current, itemId: value }))}
              options={itemOptions}
              className={fieldControlClassName}
            />
          </FormField>

          <FormField label="Status" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <ControlledSelect
              value={filters.state}
              onChange={(value) => setFilters((current) => ({ ...current, state: value }))}
              options={stateOptions}
              className={fieldControlClassName}
            />
          </FormField>

          <FormField label="Search" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <label className="search-field compact">
              <Search aria-hidden="true" />
              <span className="sr-only">Search records and operations</span>
              <input
                className={fieldControlClassName}
                value={filters.query}
                onChange={(event) => setFilters((current) => ({ ...current, query: event.target.value }))}
                placeholder="Search item, location, reason, or reference"
              />
            </label>
          </FormField>

          <FormField label="Since" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <input
              className={fieldControlClassName}
              type="date"
              value={filters.sinceUtc}
              onChange={(event) => setFilters((current) => ({ ...current, sinceUtc: event.target.value }))}
            />
          </FormField>

          <FormField label="Until" className={fieldClassName} labelClassName={fieldLabelClassName}>
            <input
              className={fieldControlClassName}
              type="date"
              value={filters.untilUtc}
              onChange={(event) => setFilters((current) => ({ ...current, untilUtc: event.target.value }))}
            />
          </FormField>
        </div>

        <section className="metrics" aria-label="Operations summary">
          <Metric icon={Package} label="Inventory rows" value={summary.inventory.length} />
          <Metric icon={MapPin} label="Locations" value={inventoryByLocation.length} />
          <Metric icon={DatabaseZap} label="Items" value={inventoryByItem.length} />
          <Metric icon={ShieldCheck} label="Holds" value={filteredHolds.length} tone="warning" />
          <Metric icon={AlertTriangle} label="Unexplained" value={filteredUnexplained.length} tone="warning" />
          <Metric icon={Activity} label="Variance rows" value={countVariance.length} tone="warning" />
        </section>

        <div className="report-stack">
          <ReportSection
            icon={MapPin}
            title="Inventory by location"
            subtitle="On-hand, reserved, allocated, and blocked totals by StaffArr location."
            rows={inventoryByLocation.map((row) => (
              <article className="panel" key={row.locationId}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.staffarrSiteNameSnapshot ?? 'Unknown site'}</span>
                    <h3>{row.locationNameSnapshot}</h3>
                  </div>
                  <span className="chip neutral">{row.activeStates.join(' · ')}</span>
                </div>
                <dl className="quantity-grid compact">
                  <Quantity label="Item count" value={row.itemCount} />
                  <Quantity label="On hand" value={row.quantityOnHand} />
                  <Quantity label="Reserved" value={row.quantityReserved} />
                  <Quantity label="Allocated" value={row.quantityAllocated} />
                  <Quantity label="Blocked" value={row.quantityBlocked} />
                </dl>
              </article>
            ))}
            emptyMessage="No inventory rows matched the current filters."
          />

          <ReportSection
            icon={DatabaseZap}
            title="Inventory by item"
            subtitle="Totals grouped by SupplyArr item reference."
            rows={inventoryByItem.map((row) => (
              <article className="panel" key={row.supplyarrItemId}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.supplyarrItemId}</span>
                    <h3>{row.itemNameSnapshot}</h3>
                  </div>
                  <span className="chip neutral">{row.unitOfMeasureSnapshot}</span>
                </div>
                <dl className="quantity-grid compact">
                  <Quantity label="Locations" value={row.locationCount} />
                  <Quantity label="On hand" value={row.quantityOnHand} />
                  <Quantity label="Reserved" value={row.quantityReserved} />
                  <Quantity label="Allocated" value={row.quantityAllocated} />
                  <Quantity label="Blocked" value={row.quantityBlocked} />
                </dl>
              </article>
            ))}
            emptyMessage="No item rows matched the current filters."
          />

          <ReportSection
            icon={ShieldCheck}
            title="Inventory by status"
            subtitle="Snapshot of stock health across states."
            rows={inventoryByStatus.map((row) => (
              <article className="panel" key={row.state}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.state}</span>
                    <h3>{row.state.replaceAll('_', ' ')}</h3>
                  </div>
                  <span className="chip neutral">{row.itemCount} items</span>
                </div>
                <dl className="quantity-grid compact">
                  <Quantity label="Locations" value={row.locationCount} />
                  <Quantity label="On hand" value={row.quantityOnHand} />
                  <Quantity label="Blocked" value={row.quantityBlocked} />
                </dl>
              </article>
            ))}
            emptyMessage="No status rows matched the current filters."
          />

          <ReportSection
            icon={Activity}
            title="Movement history"
            subtitle="Counts, adjustments, holds, and unexplained events in reverse chronological order."
            rows={movementHistory.map((row) => (
              <article className="panel" key={`${row.category}-${row.id}`}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.category}</span>
                    <h3>{row.title}</h3>
                  </div>
                  <StatusChip value={row.status} />
                </div>
                <div className="detail-line">
                  <MapPin aria-hidden="true" />
                  <span>{row.locationNameSnapshot}</span>
                </div>
                <div className="detail-line">
                  <Package aria-hidden="true" />
                  <span>{row.itemNameSnapshot}</span>
                </div>
                <p className="notes">
                  {row.subtitle}
                  {row.reason ? ` · ${row.reason}` : ''}
                </p>
              </article>
            ))}
            emptyMessage="No movement history rows matched the current filters."
          />

          <ReportSection
            icon={DatabaseZap}
            title="Origin history"
            subtitle="Current origin snapshot for on-hand inventory rows."
            rows={originHistory.map((row) => (
              <article className="panel" key={row.id}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.subtitle}</span>
                    <h3>{row.title}</h3>
                  </div>
                  <StatusChip value={row.status} />
                </div>
                <div className="detail-line">
                  <MapPin aria-hidden="true" />
                  <span>{row.locationNameSnapshot}</span>
                </div>
                <div className="detail-line">
                  <Package aria-hidden="true" />
                  <span>{row.itemNameSnapshot}</span>
                </div>
                <p className="notes">{row.reason}</p>
              </article>
            ))}
            emptyMessage="No origin rows matched the current filters."
          />

          <ReportSection
            icon={ShieldCheck}
            title="Holds and release activity"
            subtitle="Open and review holds with the selected filters."
            rows={filteredHolds.map((hold) => (
              <article className="panel" key={hold.id}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{hold.holdType}</span>
                    <h3>{hold.supplyarrItemId}</h3>
                  </div>
                  <StatusChip value={hold.status} />
                </div>
                <div className="detail-line">
                  <MapPin aria-hidden="true" />
                  <span>{hold.locationNameSnapshot}</span>
                </div>
                <p className="notes">{hold.reason}</p>
                <p className="notes">{hold.sourceReference}</p>
              </article>
            ))}
            emptyMessage="No holds matched the current filters."
          />

          <ReportSection
            icon={AlertTriangle}
            title="Unexplained inventory"
            subtitle="Found stock records awaiting resolution, quarantine, or scrap."
            rows={filteredUnexplained.map((record) => (
              <article className="panel" key={record.id}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{record.recordNumber}</span>
                    <h3>{record.itemNameSnapshot}</h3>
                  </div>
                  <StatusChip value={record.status} />
                </div>
                <dl className="quantity-grid compact">
                  <Quantity label="Observed" value={record.quantity} />
                  <Quantity label="Variance" value={record.varianceQuantity} />
                </dl>
                <div className="detail-line">
                  <MapPin aria-hidden="true" />
                  <span>{record.locationNameSnapshot}</span>
                </div>
                <p className="notes">{record.reasonCode} · {record.evidenceSummary}</p>
              </article>
            ))}
            emptyMessage="No unexplained inventory matched the current filters."
          />

          <ReportSection
            icon={CalendarRange}
            title="Count variance history"
            subtitle="Counts with non-zero variance and their downstream adjustments."
            rows={countVariance.map((row) => (
              <article className="panel" key={row.id}>
                <div className="panel-title-row">
                  <div>
                    <span className="kicker">{row.countNumber}</span>
                    <h3>{row.itemNameSnapshot}</h3>
                  </div>
                  <StatusChip value={row.status} />
                </div>
                <dl className="quantity-grid compact">
                  <Quantity label="Expected" value={row.expectedQuantity} />
                  <Quantity label="Counted" value={row.countedQuantity} />
                  <Quantity label="Variance" value={row.varianceQuantity} />
                </dl>
                <div className="detail-line">
                  <MapPin aria-hidden="true" />
                  <span>{row.locationNameSnapshot}</span>
                </div>
                <p className="notes">
                  {row.reasonCode}
                  {row.adjustmentReasonCode ? ` · ${row.adjustmentReasonCode}` : ''}
                </p>
              </article>
            ))}
            emptyMessage="No count variances matched the current filters."
          />
        </div>
      </article>
    </section>
  )
}

function ReportSection({
  icon: Icon,
  title,
  subtitle,
  rows,
  emptyMessage,
}: {
  icon: typeof BarChart3
  title: string
  subtitle: string
  rows: ReactNode[]
  emptyMessage: string
}) {
  return (
    <section className="report-section" aria-label={title}>
      <div className="section-heading">
        <Icon aria-hidden="true" />
        <div>
          <h3>{title}</h3>
          <p className="notes">{subtitle}</p>
        </div>
      </div>
      {rows.length > 0 ? <div className="report-grid">{rows}</div> : <p className="empty-state">{emptyMessage}</p>}
    </section>
  )
}

function Quantity({ label, value }: { label: string; value: number }) {
  return (
    <div>
      <dt>{label}</dt>
      <dd>{value.toLocaleString()}</dd>
    </div>
  )
}

function Metric({
  icon: Icon,
  label,
  value,
  tone = 'neutral',
}: {
  icon: typeof Package
  label: string
  value: number
  tone?: 'neutral' | 'warning'
}) {
  return (
    <article className={`metric ${tone}`}>
      <Icon aria-hidden="true" />
      <div>
        <span>{label}</span>
        <strong>{value.toLocaleString()}</strong>
      </div>
    </article>
  )
}

function StatusChip({ value }: { value: string }) {
  return <span className={`chip ${value.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`}>{value.replaceAll('_', ' ')}</span>
}

const fieldClassName = 'field'
const fieldLabelClassName = 'field-label'
const fieldControlClassName = 'field-control'
