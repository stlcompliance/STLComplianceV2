import {
  MARKET_CHECKLIST_ROWS,
  MARKET_PRODUCT_COMPARISONS,
} from '../content/marketComparison'

export function MarketComparisonTable() {
  return (
    <div className="space-y-8">
      <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
        <table className="min-w-[1180px] text-left text-sm" data-testid="market-checklist-table">
          <caption className="sr-only">
            STL-biased feature checklist compared with market specialists
          </caption>
          <thead className="border-b border-slate-700 text-slate-300">
            <tr>
              <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
                STL feature
              </th>
              <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
                STL coverage
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Typical market coverage
              </th>
              <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
                STL advantage
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Competitor examples
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {MARKET_CHECKLIST_ROWS.map((row) => (
              <tr key={row.id} className="align-top text-slate-200">
                <th scope="row" className="max-w-xs px-4 py-4 font-semibold text-white">
                  {row.feature}
                </th>
                <td className="max-w-xs px-4 py-4 text-slate-200">{row.stlCoverage}</td>
                <td className="max-w-xs px-4 py-4 text-slate-300">{row.typicalMarketCoverage}</td>
                <td className="max-w-xs px-4 py-4 text-slate-100">{row.stlAdvantage}</td>
                <td className="max-w-xs px-4 py-4 text-slate-400">{row.competitorExamples}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">
        <table className="min-w-[1180px] text-left text-sm" data-testid="market-products-table">
          <caption className="sr-only">
            Comparison of STL Compliance with named market products
          </caption>
          <thead className="border-b border-slate-700 text-slate-300">
            <tr>
              <th scope="col" className="px-4 py-3 font-semibold">
                Market product
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Category
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Strongest at
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Best buyer fit
              </th>
              <th scope="col" className="px-4 py-3 font-semibold text-teal-200">
                STL difference
              </th>
              <th scope="col" className="px-4 py-3 font-semibold">
                Source
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {MARKET_PRODUCT_COMPARISONS.map((row) => (
              <tr key={row.id} className="align-top text-slate-200">
                <th scope="row" className="px-4 py-4 font-semibold text-white">
                  {row.product}
                </th>
                <td className="px-4 py-4 text-slate-300">{row.category}</td>
                <td className="max-w-xs px-4 py-4 text-slate-300">{row.bestAt}</td>
                <td className="max-w-xs px-4 py-4 text-slate-300">{row.buyerFit}</td>
                <td className="max-w-xs px-4 py-4 text-slate-200">{row.stlDifference}</td>
                <td className="px-4 py-4">
                  <a
                    href={row.sourceHref}
                    className="text-teal-400 hover:text-teal-300"
                    rel="noopener noreferrer"
                    target="_blank"
                  >
                    {row.sourceLabel}
                  </a>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
