import { useState, type FormEvent } from 'react'
import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { contactMailto, siteConfig, suiteLoginUrl } from '../lib/siteConfig'

type FormState = {
  name: string
  email: string
  organization: string
  message: string
}

const initialForm: FormState = {
  name: '',
  email: '',
  organization: '',
  message: '',
}

export function DemoContactPage() {
  const [form, setForm] = useState<FormState>(initialForm)
  const [submitted, setSubmitted] = useState(false)

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSubmitted(true)
  }

  return (
    <>
      <SiteSeo
        title={`Contact — ${siteConfig.siteName}`}
        description="Request a walkthrough, ask a question, or contact the STL Compliance team. Existing customers use secure client sign-in."
        path="/demo"
      />
      <PageHero
        eyebrow="Get started"
        title="Contact STL Compliance"
        subtitle="Share what you want to see first. You can also use secure sign-in if you are already a customer."
      >
        <a
          href={suiteLoginUrl()}
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Client sign in
        </a>
      </PageHero>

      <section className="mx-auto max-w-xl px-4 pb-16 sm:px-6">
        {submitted ? (
          <div
            className="rounded-2xl border border-teal-500/40 bg-teal-950/30 px-6 py-8"
            data-testid="demo-thank-you"
          >
            <h2 className="text-lg font-semibold text-white">Thank you</h2>
            <p className="mt-3 text-slate-200">
              We received your request on this device. Email us at{' '}
              <a href={contactMailto('STL Compliance contact request')} className="text-teal-300 underline">
                {siteConfig.contactEmail}
              </a>{' '}
              or open a draft from this page.
            </p>
            <a
              href={contactMailto(`Contact request from ${form.name || 'visitor'}`)}
              className="mt-6 inline-flex rounded-lg border border-slate-500 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-400"
            >
              Open email draft
            </a>
          </div>
        ) : (
          <form
            onSubmit={handleSubmit}
            className="space-y-4 rounded-2xl border border-slate-700 bg-slate-900/70 p-6"
            data-testid="demo-contact-form"
          >
            <label className="block text-sm font-medium text-slate-200">
              Name
              <input
                required
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              />
            </label>
            <label className="block text-sm font-medium text-slate-200">
              Work email
              <input
                required
                type="email"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              />
            </label>
            <label className="block text-sm font-medium text-slate-200">
              Organization
              <input
                value={form.organization}
                onChange={(e) => setForm((f) => ({ ...f, organization: e.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              />
            </label>
            <label className="block text-sm font-medium text-slate-200">
              What would you like to see or ask?
              <textarea
                required
                rows={4}
                value={form.message}
                onChange={(e) => setForm((f) => ({ ...f, message: e.target.value }))}
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
              />
            </label>
            <button
              type="submit"
              className="w-full rounded-lg bg-teal-600 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
            >
              Submit request
            </button>
            <p className="text-xs text-slate-500">
              This public form prepares your request on this device and then opens an email draft.
            </p>
          </form>
        )}
      </section>
    </>
  )
}
