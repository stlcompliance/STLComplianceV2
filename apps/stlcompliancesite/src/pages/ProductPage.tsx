import { Link, useParams } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { getMarketingProduct } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function ProductPage() {
  const { productKey = '' } = useParams()
  const product = getMarketingProduct(productKey)

  if (!product) {
    return (
      <section className="mx-auto max-w-3xl px-4 py-16 text-center sm:px-6">
        <h1 className="text-2xl font-bold text-white">Product not found</h1>
        <Link to="/products" className="mt-4 inline-block text-teal-400 hover:text-teal-300">
          Back to products
        </Link>
      </section>
    )
  }

  const Icon = product.icon

  return (
    <>
      <SiteSeo
        title={`${product.displayName} — ${siteConfig.siteName}`}
        description={`${product.displayName}: ${product.tagline}. ${product.owns}`}
      />
      <PageHero
        eyebrow="Product"
        title={product.displayName}
        subtitle={product.tagline}
      >
        <a
          href={suiteLoginUrl()}
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Sign in through NexArr
        </a>
        <Link
          to="/demo"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Request a walkthrough
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <div className="flex items-center gap-3 text-teal-300">
          <Icon className="h-8 w-8" aria-hidden />
          <p className="text-sm font-semibold uppercase tracking-wide">Ownership boundary</p>
        </div>
        <div className="mt-8 grid gap-6 lg:grid-cols-2">
          <article className="rounded-2xl border border-teal-500/30 bg-teal-950/20 p-6">
            <h2 className="text-lg font-semibold text-teal-200">Owns</h2>
            <p className="mt-3 text-slate-200">{product.owns}</p>
          </article>
          <article className="rounded-2xl border border-slate-600 bg-slate-900/60 p-6">
            <h2 className="text-lg font-semibold text-slate-200">Does not own</h2>
            <p className="mt-3 text-slate-300">{product.doesNotOwn}</p>
          </article>
        </div>
        <p className="mt-8 text-sm text-slate-400">
          Product launch and entitlements are enforced by NexArr and each product API — not by this
          marketing site.
        </p>
      </section>
    </>
  )
}
