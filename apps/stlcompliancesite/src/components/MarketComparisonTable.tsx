import {
  CAN_WORK_START_ITEMS,
  CATEGORY_COMPARISONS,
  FEATURE_CHECKLIST_ROWS,
  OBJECTIONS,
  PRODUCT_STACK_ROWS,
  USUAL_STACK_ROWS,
} from '../content/marketComparison'

const VALUE_STYLES: Record<string, string> = {
  Yes: 'border-teal-500/40 bg-teal-950/70 text-teal-100',
  Partial: 'border-sky-500/30 bg-sky-950/60 text-sky-100',
  Rare: 'border-amber-500/30 bg-amber-950/50 text-amber-100',
  No: 'border-slate-700 bg-slate-950/70 text-slate-500',
}

function ValuePill({ value }: { value: string }) {
  return (
    <span
      className={`inline-flex min-w-16 justify-center rounded-full border px-2 py-1 text-xs font-semibold ${VALUE_STYLES[value] ?? VALUE_STYLES.No}`}
    >
      {value}
    </span>
  )
}

export function UsualStackTable() {
  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
      <table className="min-w-[960px] text-left text-sm" data-testid="usual-stack-table">
        <caption className="sr-only">The usual software stack and gaps between systems</caption>
        <thead className="border-b border-slate-700 text-slate-300">
          <tr>
            <th scope="col" className="px-4 py-3 font-semibold">
              Business need
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Typical product
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              What it usually solves
            </th>
            <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
              What still falls between systems
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800">
          {USUAL_STACK_ROWS.map((row) => (
            <tr key={row.need} className="align-top text-slate-200">
              <th scope="row" className="px-4 py-4 font-semibold text-white">
                {row.need}
              </th>
              <td className="px-4 py-4 text-slate-300">{row.product}</td>
              <td className="max-w-sm px-4 py-4 text-slate-300">{row.solves}</td>
              <td className="max-w-sm px-4 py-4 text-slate-100">{row.gap}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function FeatureChecklistTable() {
  const columns = ['wms', 'cmms', 'lms', 'wfm', 'tms', 'grc', 'stl'] as const
  const labels: Record<(typeof columns)[number], string> = {
    wms: 'WMS',
    cmms: 'CMMS',
    lms: 'LMS',
    wfm: 'WFM',
    tms: 'TMS',
    grc: 'GRC',
    stl: 'STL',
  }

  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
      <table className="min-w-[1040px] text-left text-sm" data-testid="feature-checklist-table">
        <caption className="sr-only">Biased feature checklist comparison</caption>
        <thead className="border-b border-slate-700 text-slate-300">
          <tr>
            <th scope="col" className="px-4 py-3 font-semibold">
              Capability
            </th>
            {columns.map((column) => (
              <th
                key={column}
                scope="col"
                className={`px-3 py-3 text-center font-semibold ${column === 'stl' ? 'text-teal-200' : ''}`}
              >
                {labels[column]}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800">
          {FEATURE_CHECKLIST_ROWS.map((row) => (
            <tr key={row.capability} className="text-slate-200">
              <th scope="row" className="max-w-xs px-4 py-4 font-semibold text-white">
                {row.capability}
              </th>
              {columns.map((column) => (
                <td key={column} className="px-3 py-4 text-center">
                  <ValuePill value={row[column]} />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function CategoryComparisonCards() {
  return (
    <div className="grid gap-4 lg:grid-cols-2" data-testid="category-comparison-cards">
      {CATEGORY_COMPARISONS.map((item) => (
        <article key={item.id} className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
          <h3 className="text-lg font-semibold text-white">{item.title}</h3>
          <p className="mt-4 text-sm font-semibold text-slate-200">Category tools are great at</p>
          <p className="mt-1 text-sm text-slate-300">{item.pointToolsGreatAt}</p>
          <p className="mt-4 text-sm font-semibold text-slate-200">STL adds</p>
          <ul className="mt-2 space-y-2 text-sm text-slate-300">
            {item.stlAdds.map((addition) => (
              <li key={addition} className="flex gap-2">
                <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-teal-300" />
                <span>{addition}</span>
              </li>
            ))}
          </ul>
          <p className="mt-4 rounded-xl border border-teal-500/30 bg-teal-950/20 p-3 text-sm font-semibold text-teal-100">
            {item.takeaway}
          </p>
        </article>
      ))}
    </div>
  )
}

export function CanWorkStartChecklist() {
  return (
    <div className="rounded-2xl border border-teal-500/30 bg-teal-950/15 p-6">
      <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3" data-testid="can-work-start-list">
        {CAN_WORK_START_ITEMS.map((item) => (
          <li key={item} className="flex gap-2 text-sm text-slate-200">
            <span className="font-bold text-teal-300">✓</span>
            <span>{item}</span>
          </li>
        ))}
      </ul>
      <p className="mt-6 text-lg font-semibold text-white">
        Result: Allowed. Blocked. Needs override. Needs evidence. Needs retraining. Needs repair.
        Needs approval.
      </p>
    </div>
  )
}

export function ProductStackTable() {
  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
      <table className="min-w-[920px] text-left text-sm" data-testid="product-stack-table">
        <caption className="sr-only">STL product stack comparison</caption>
        <thead className="border-b border-slate-700 text-slate-300">
          <tr>
            <th scope="col" className="px-4 py-3 font-semibold">
              STL product
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Replaces / complements
            </th>
            <th scope="col" className="px-4 py-3 font-semibold">
              Primary job
            </th>
            <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
              Biased positioning
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800">
          {PRODUCT_STACK_ROWS.map((row) => (
            <tr key={row.product} className="text-slate-200">
              <th scope="row" className="px-4 py-4 font-semibold text-white">
                {row.product}
              </th>
              <td className="px-4 py-4 text-slate-300">{row.complements}</td>
              <td className="px-4 py-4 text-slate-300">{row.primaryJob}</td>
              <td className="px-4 py-4 text-slate-100">{row.positioning}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export function ObjectionCards() {
  return (
    <div className="grid gap-4 lg:grid-cols-2" data-testid="objection-cards">
      {OBJECTIONS.map((item) => (
        <article key={item.title} className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
          <h3 className="text-lg font-semibold text-white">{item.title}</h3>
          <p className="mt-3 text-sm text-slate-300">{item.body}</p>
          <p className="mt-4 rounded-xl border border-teal-500/30 bg-teal-950/20 p-3 text-sm font-semibold text-teal-100">
            {item.answer}
          </p>
        </article>
      ))}
    </div>
  )
}
