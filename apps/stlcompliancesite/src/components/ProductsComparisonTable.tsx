import { Link } from 'react-router-dom'

import {

  MARKETING_PRODUCTS,

  MATURITY_LABELS,

  PRODUCT_CATEGORY_LABELS,

  productPagePath,

} from '../content/products'



export function ProductsComparisonTable() {

  return (

    <div className="overflow-x-auto rounded-2xl border border-slate-700 bg-slate-900/50">

      <table className="min-w-full text-left text-sm">

        <caption className="sr-only">Suite product comparison</caption>

        <thead className="border-b border-slate-700 text-slate-300">

          <tr>

            <th scope="col" className="px-4 py-3 font-semibold">

              Product

            </th>

            <th scope="col" className="px-4 py-3 font-semibold">

              Category

            </th>

            <th scope="col" className="px-4 py-3 font-semibold">

              Status

            </th>

            <th scope="col" className="hidden px-4 py-3 font-semibold md:table-cell">

              Helps manage

            </th>

          </tr>

        </thead>

        <tbody className="divide-y divide-slate-800">

          {MARKETING_PRODUCTS.map((product) => (

            <tr key={product.productKey} className="text-slate-200">

              <td className="px-4 py-3 font-medium">

                <Link to={productPagePath(product.productKey)} className="text-teal-400 hover:text-teal-300">

                  {product.displayName}

                </Link>

              </td>

              <td className="px-4 py-3 text-slate-300">{PRODUCT_CATEGORY_LABELS[product.category]}</td>

              <td className="px-4 py-3">

                <span

                  className={

                    product.maturity === 'v1-operational'

                      ? 'rounded-full bg-teal-950/80 px-2 py-0.5 text-xs font-semibold text-teal-200'

                      : 'rounded-full bg-amber-950/60 px-2 py-0.5 text-xs font-semibold text-amber-200'

                  }

                >

                  {MATURITY_LABELS[product.maturity]}

                </span>

                <p className="mt-1 text-xs text-slate-400">{product.maturityLabel}</p>

              </td>

              <td className="hidden px-4 py-3 text-slate-400 md:table-cell">{product.owns}</td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>

  )

}


