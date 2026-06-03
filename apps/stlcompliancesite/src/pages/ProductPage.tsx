import { Link, useParams } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { COMPLIANCE_CORE_EDUCATION } from '../content/ownershipBoundaries'
import { getMarketingProduct } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

type DetailListProps = {
  title: string
  items: string[]
}

function DetailList({ title, items }: DetailListProps) {
  return (
    <article className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
      <h2 className="text-lg font-semibold text-white">{title}</h2>
      <ul className="mt-4 space-y-3 text-sm text-slate-300">
        {items.map((item) => (
          <li key={item} className="flex gap-3">
            <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-teal-300" />
            <span>{item}</span>
          </li>
        ))}
      </ul>
    </article>
  )
}

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
  const isComplianceCore = product.productKey === 'compliancecore'

  return (
    <>
      <SiteSeo
        title={`${product.displayName} — ${siteConfig.siteName}`}
        description={`${product.displayName}: ${product.tagline}. ${product.owns}`}
        path={`/products/${product.productKey}`}
      />
      <PageHero
        eyebrow="Product"
        title={product.displayName}
        subtitle={product.tagline}
        brandImageSrc={product.brandImageSrc}
        brandImageAlt={product.displayName}
      >
        <a
          href={suiteLoginUrl()}
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Client sign in
        </a>
        <Link
          to="/demo"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Request a walkthrough
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <div className="flex flex-wrap items-center gap-3 text-teal-300">
          <Icon className="h-8 w-8" aria-hidden />
          <p className="text-sm font-semibold uppercase tracking-wide">What it helps with</p>
        </div>
        <p className="mt-3 text-sm text-slate-400" data-testid="product-capability-detail">
          {product.overview}
        </p>
        <div className="mt-8 grid gap-6 lg:grid-cols-2">
          <article className="rounded-2xl border border-teal-500/30 bg-teal-950/20 p-6">
            <h2 className="text-lg font-semibold text-teal-200">Best for</h2>
            <p className="mt-3 text-slate-200">{product.owns}</p>
          </article>
          <article className="rounded-2xl border border-slate-600 bg-slate-900/60 p-6">
            <h2 className="text-lg font-semibold text-slate-200">Usually handled elsewhere</h2>
            <p className="mt-3 text-slate-300">{product.doesNotOwn}</p>
          </article>
        </div>
        <p className="mt-8 text-sm text-slate-400" data-testid="ownership-source-doc">
          Customers use the secure suite login for real records and daily workflows.
        </p>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">What {product.displayName} actually does</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          The checklist below is the practical operating surface: what teams do in the product,
          which records it owns, which checks it supports, and what proof it produces.
        </p>
        <div className="mt-6 grid gap-4 lg:grid-cols-2">
          <DetailList title="Primary workflows" items={product.primaryWorkflows} />
          <DetailList title="Records it manages" items={product.recordsManaged} />
          <DetailList title="Checks and gates" items={product.readinessChecks} />
          <DetailList title="Evidence and outputs" items={product.evidenceOutputs} />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <article className="rounded-2xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-lg font-semibold text-white">How it connects to the suite</h2>
          <ul className="mt-4 space-y-3 text-sm text-slate-300">
            {product.handoffs.map((handoff) => (
              <li key={handoff} className="flex gap-3">
                <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-sky-300" />
                <span>{handoff}</span>
              </li>
            ))}
          </ul>
        </article>
      </section>

      {isComplianceCore ? (
        <section
          className="mx-auto max-w-6xl px-4 pb-12 sm:px-6"
          data-testid="compliance-core-education"
        >
          <article className="rounded-2xl border border-purple-500/30 bg-purple-950/20 p-6">
            <h2 className="text-lg font-semibold text-purple-200">
              {COMPLIANCE_CORE_EDUCATION.headline}
            </h2>
            <p className="mt-3 text-slate-200">{COMPLIANCE_CORE_EDUCATION.lead}</p>
            <ul className="mt-4 list-disc space-y-2 pl-5 text-sm text-slate-300">
              {COMPLIANCE_CORE_EDUCATION.bullets.map((bullet) => (
                <li key={bullet}>{bullet}</li>
              ))}
            </ul>
          </article>
        </section>
      ) : null}
    </>
  )
}
