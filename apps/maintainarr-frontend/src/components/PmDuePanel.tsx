import type { PmScheduleResponse } from '../api/types'

interface PmDuePanelProps {
  dueSchedules: PmScheduleResponse[]
  isLoading: boolean
}

function formatDueStatus(status: string): string {
  if (status === 'overdue') {
    return 'Overdue'
  }
  if (status === 'due') {
    return 'Due'
  }
  return status
}

export function PmDuePanel({ dueSchedules, isLoading }: PmDuePanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading due preventive maintenance…</p>
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="mb-4">
        <h2 className="text-lg font-semibold text-white">Due preventive maintenance</h2>
        <p className="mt-1 text-sm text-slate-400">
          Schedules marked due or overdue by the MaintainArr PM scan worker.
        </p>
      </header>

      {dueSchedules.length === 0 ? (
        <p className="text-sm text-slate-400">No PM schedules are currently due.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-700 text-slate-400">
              <tr>
                <th className="px-3 py-2 font-medium">Asset</th>
                <th className="px-3 py-2 font-medium">Schedule</th>
                <th className="px-3 py-2 font-medium">Due status</th>
                <th className="px-3 py-2 font-medium">Next due</th>
                <th className="px-3 py-2 font-medium">Work order</th>
              </tr>
            </thead>
            <tbody>
              {dueSchedules.map((schedule) => (
                <tr key={schedule.pmScheduleId} className="border-b border-slate-800 text-slate-200">
                  <td className="px-3 py-2">
                    <div className="font-medium">{schedule.assetTag}</div>
                    <div className="text-xs text-slate-400">{schedule.assetName}</div>
                  </td>
                  <td className="px-3 py-2">
                    <div className="font-medium">{schedule.name}</div>
                    <div className="text-xs text-slate-400">{schedule.scheduleKey}</div>
                  </td>
                  <td className="px-3 py-2">
                    <span
                      className={
                        schedule.dueStatus === 'overdue'
                          ? 'rounded-full bg-red-950 px-2 py-0.5 text-xs text-red-200'
                          : 'rounded-full bg-amber-950 px-2 py-0.5 text-xs text-amber-200'
                      }
                    >
                      {formatDueStatus(schedule.dueStatus)}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-slate-300">
                    {new Date(schedule.nextDueAt).toLocaleDateString()}
                  </td>
                  <td className="px-3 py-2 text-slate-300">
                    {schedule.linkedWorkOrderNumber ? (
                      <div>
                        <div className="font-medium">{schedule.linkedWorkOrderNumber}</div>
                        <div className="text-xs capitalize text-slate-400">
                          {schedule.linkedWorkOrderStatus?.replace('_', ' ') ?? 'open'}
                        </div>
                      </div>
                    ) : (
                      <span className="text-xs text-slate-500">Pending generation</span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
