import {
  MILESTONE_POSTURE_LABELS,
  PROGRAM_MILESTONES,
  milestonePostureBadgeClass,
} from '../content/implementationMaturity'

export function ProgramMilestoneTable() {
  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
      <table className="min-w-full text-left text-sm" data-testid="program-milestone-table">
        <caption className="sr-only">Product rollout status</caption>
        <thead className="border-b border-slate-700 text-slate-300">
          <tr>
            <th scope="col" className="px-4 py-3 font-semibold">
              Area
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Status
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Summary
            </th>
            <th scope="col" className="hidden px-4 py-3 font-semibold lg:table-cell">
              Products
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800">
          {PROGRAM_MILESTONES.map((row) => (
            <tr key={row.id} className="text-slate-200" data-testid={`milestone-row-${row.id}`}>
              <th scope="row" className="px-4 py-3 font-medium text-white">
                <span className="text-teal-300">{row.id}</span>
                <span className="mt-0.5 block text-sm font-normal text-slate-300">{row.title}</span>
              </th>
              <td className="px-4 py-3">
                <span className={milestonePostureBadgeClass(row.posture)}>
                  {MILESTONE_POSTURE_LABELS[row.posture]}
                </span>
              </td>
              <td className="px-4 py-3 text-slate-300">{row.summary}</td>
              <td className="hidden px-4 py-3 text-slate-400 lg:table-cell">{row.primaryProducts}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
