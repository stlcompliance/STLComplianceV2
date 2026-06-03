import { COMPARISON_DIMENSIONS } from '../content/compare'

const ANSWER_STYLES: Record<string, string> = {
  Native: 'border-teal-500/40 bg-teal-950/70 text-teal-100',
  Connected: 'border-sky-500/30 bg-sky-950/60 text-sky-100',
  'Manual outside suite': 'border-slate-700 bg-slate-950/70 text-slate-400',
}

export function AlternativeComparisonTable() {
  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
      <table className="min-w-full text-left text-sm" data-testid="compare-dimensions-table">
        <caption className="sr-only">
          Honest comparison of spreadsheets, point tools, and the STL Compliance suite
        </caption>
        <thead className="border-b border-slate-700 text-slate-300">
          <tr>
            <th scope="col" className="px-4 py-3 font-semibold">
              Checklist item
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Spreadsheets
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Point tools
            </th>
            <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
              STL Compliance suite
            </th>
            <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
              STL answer
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800">
          {COMPARISON_DIMENSIONS.map((row) => (
            <tr key={row.id} className="text-slate-200" data-testid={`compare-row-${row.id}`}>
              <th scope="row" className="max-w-xs px-4 py-3 font-medium text-white">
                {row.checklistItem}
              </th>
              <td className="px-4 py-3 text-slate-300">{row.spreadsheets}</td>
              <td className="px-4 py-3 text-slate-300">{row.pointTools}</td>
              <td className="px-4 py-3 text-slate-200">{row.stlSuite}</td>
              <td className="px-4 py-3">
                <span
                  className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold ${ANSWER_STYLES[row.stlAnswer]}`}
                >
                  {row.stlAnswer}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
