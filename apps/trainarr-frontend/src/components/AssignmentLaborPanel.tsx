import type {
  TrainingAssignmentLaborEntryResponse,
} from '../api/types'

interface AssignmentLaborPanelProps {
  laborEntries: TrainingAssignmentLaborEntryResponse[]
  canManage: boolean
  laborTypeKey: string
  hoursWorked: string
  costPerHour: string
  notes: string
  onLaborTypeKeyChange: (value: string) => void
  onHoursWorkedChange: (value: string) => void
  onCostPerHourChange: (value: string) => void
  onNotesChange: (value: string) => void
  onAddLaborEntry: () => void
  onRemoveLaborEntry: (laborEntryId: string) => Promise<void>
  isAdding: boolean
  removingId: string | null
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value)
}

function laborTypeLabel(value: string): string {
  switch (value) {
    case 'delivery':
      return 'Delivery'
    case 'preparation':
      return 'Preparation'
    case 'review':
      return 'Review'
    case 'administration':
      return 'Administration'
    case 'travel':
      return 'Travel'
    default:
      return value.replace(/[_-]+/g, ' ')
  }
}

export function AssignmentLaborPanel({
  laborEntries,
  canManage,
  laborTypeKey,
  hoursWorked,
  costPerHour,
  notes,
  onLaborTypeKeyChange,
  onHoursWorkedChange,
  onCostPerHourChange,
  onNotesChange,
  onAddLaborEntry,
  onRemoveLaborEntry,
  isAdding,
  removingId,
}: AssignmentLaborPanelProps) {
  const totalHours = laborEntries.reduce((sum, entry) => sum + entry.hoursWorked, 0)
  const totalCost = laborEntries.reduce((sum, entry) => sum + entry.totalCost, 0)

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4" data-testid="assignment-labor-panel">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Training labor</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Track labor spent on this assignment so reports can surface training cost and labor metrics.
      </p>

      <div className="mt-3 grid gap-2 text-sm text-slate-300 sm:grid-cols-2">
        <div className="rounded border border-slate-800 bg-slate-950/40 p-3">
          <p className="text-xs text-[var(--color-text-muted)]">Total hours</p>
          <p className="text-lg font-semibold text-white">{totalHours.toFixed(2)}</p>
        </div>
        <div className="rounded border border-slate-800 bg-slate-950/40 p-3">
          <p className="text-xs text-[var(--color-text-muted)]">Total cost</p>
          <p className="text-lg font-semibold text-white">{formatCurrency(totalCost)}</p>
        </div>
      </div>

      {laborEntries.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No labor entries recorded yet.</p>
      ) : (
        <ul className="mt-3 space-y-2 text-sm text-slate-300">
          {laborEntries.map((entry) => (
            <li key={entry.laborEntryId} className="rounded border border-slate-800 bg-slate-950/40 px-3 py-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="font-medium text-white">
                  {laborTypeLabel(entry.laborTypeKey)} · {entry.hoursWorked.toFixed(2)}h · {formatCurrency(entry.totalCost)}
                </span>
                {canManage ? (
                  <button
                    type="button"
                    className="rounded border border-slate-700 px-2 py-1 text-xs text-slate-300 hover:bg-slate-800 disabled:opacity-50"
                    disabled={removingId === entry.laborEntryId}
                    onClick={() => void onRemoveLaborEntry(entry.laborEntryId)}
                  >
                    {removingId === entry.laborEntryId ? 'Removing…' : 'Remove'}
                  </button>
                ) : null}
              </div>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Rate {formatCurrency(entry.costPerHour)}{entry.notes ? ` · ${entry.notes}` : ''}
              </p>
            </li>
          ))}
        </ul>
      )}

      {canManage ? (
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <label htmlFor="assignment-labor-type" className="block text-sm text-slate-300">
            Labor type
            <select
              id="assignment-labor-type"
              value={laborTypeKey}
              onChange={(event) => onLaborTypeKeyChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            >
              <option value="delivery">Delivery</option>
              <option value="preparation">Preparation</option>
              <option value="review">Review</option>
              <option value="administration">Administration</option>
              <option value="travel">Travel</option>
            </select>
          </label>
          <label htmlFor="assignment-labor-hours" className="block text-sm text-slate-300">
            Hours worked
            <input
              id="assignment-labor-hours"
              value={hoursWorked}
              onChange={(event) => onHoursWorkedChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              inputMode="decimal"
            />
          </label>
          <label htmlFor="assignment-labor-rate" className="block text-sm text-slate-300">
            Cost per hour
            <input
              id="assignment-labor-rate"
              value={costPerHour}
              onChange={(event) => onCostPerHourChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              inputMode="decimal"
            />
          </label>
          <label htmlFor="assignment-labor-notes" className="block text-sm text-slate-300">
            Notes
            <input
              id="assignment-labor-notes"
              value={notes}
              onChange={(event) => onNotesChange(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            />
          </label>
          <div className="md:col-span-2">
            <button
              type="button"
              className="rounded-md bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={isAdding}
              onClick={onAddLaborEntry}
            >
              {isAdding ? 'Adding…' : 'Add labor entry'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
