import { Link } from 'react-router-dom'

import {

  MARKETING_PRODUCTS,

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

              Focus

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

              <td className="px-4 py-3 text-slate-300">{PRODUCT_CATEGORY_LABELS[product.category]}</td>

              <td className="px-4 py-3">

                <p className="text-xs text-slate-400">{product.tagline}</p>

              </td>

              <td className="hidden px-4 py-3 text-slate-400 md:table-cell">{product.owns}</td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>

  )

}


