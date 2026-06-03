import { Link } from 'react-router-dom'

import {

  CAPABILITY_LABELS,

  CAPABILITY_ORDER,

  MARKETING_PRODUCTS,

  PRODUCT_CATEGORY_LABELS,

  type CapabilityLevel,

  productPagePath,

} from '../content/products'

const LEVEL_STYLES: Record<CapabilityLevel, string> = {
  primary: 'border-teal-500/40 bg-teal-950/70 text-teal-100',
  connected: 'border-sky-500/30 bg-sky-950/50 text-sky-100',
  none: 'border-slate-700 bg-slate-950/70 text-slate-500',
}

const LEVEL_LABELS: Record<CapabilityLevel, string> = {
  primary: 'Primary',
  connected: 'Connected',
  none: 'No',
}



export function ProductsComparisonTable() {

  return (

    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">

      <table className="min-w-[1120px] text-left text-sm">

        <caption className="sr-only">Suite product comparison</caption>

        <thead className="border-b border-slate-700 text-slate-300">

          <tr>

            <th scope="col" className="px-4 py-3 font-semibold">

              Product

            </th>

            <th scope="col" className="px-4 py-3 font-semibold">

              Category

            </th>

            {CAPABILITY_ORDER.map((capability) => (
              <th key={capability} scope="col" className="px-3 py-3 text-xs font-semibold">
                {CAPABILITY_LABELS[capability]}
              </th>
            ))}

            <th scope="col" className="px-4 py-3 font-semibold">

              Actual focus

            </th>

          </tr>

        </thead>

        <tbody className="divide-y divide-slate-800">

          {MARKETING_PRODUCTS.map((product) => (

            <tr key={product.productKey} className="text-slate-200">

              <td className="px-4 py-3 font-medium">

                <Link
                  to={productPagePath(product.productKey)}
                  className="inline-flex items-center gap-2 text-teal-400 hover:text-teal-300"
                >

                  <img
                    src={product.brandImageSrc}
                    alt=""
                    className="h-7 w-7 rounded-md bg-white object-contain p-0.5"
                    aria-hidden
                  />

                  {product.displayName}

                </Link>

              </td>

              <td className="px-4 py-3 text-slate-300">
                {PRODUCT_CATEGORY_LABELS[product.category]}
              </td>

              {CAPABILITY_ORDER.map((capability) => {
                const level = product.checklist[capability]
                return (
                  <td key={capability} className="px-3 py-3">
                    <span
                      className={`inline-flex min-w-20 justify-center rounded-full border px-2 py-1 text-[11px] font-semibold ${LEVEL_STYLES[level]}`}
                    >
                      {LEVEL_LABELS[level]}
                    </span>
                  </td>
                )
              })}

              <td className="px-4 py-3 text-slate-400">
                <p className="max-w-xs text-xs">{product.owns}</p>

              </td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>

  )

}


