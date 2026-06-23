import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'

const QUESTIONS = [
  {
    question: 'Does STL Compliance replace all our operations software?',
    answer:
      'No. STL Compliance connects your operation into one accountable stack and helps teams work across tools. We can start with the products you use now and expand by need.',
  },
  {
    question: 'Will this help with audit readiness?',
    answer:
      'STL Compliance helps organize and produce proof from day-to-day work. It helps prepare and supports your compliance workflow so audits are easier to support.',
  },
  {
    question: 'How long does implementation take?',
    answer:
      'Most teams start with identity, people, one or two operations products, then add products as workflow depth increases.',
  },
  {
    question: 'Is there one product for everything?',
    answer:
      'Each product focuses on a specific area: people, training, maintenance, dispatch, inventory, rules, records, quality, and reporting.',
  },
  {
    question: 'Can we keep our current tools while evaluating STL?',
    answer:
      'Yes. STL is designed for a practical rollout path and works as a suite foundation where meaningful handoffs are prioritized first.',
  },
]

export function FaqPage() {
  return (
    <>
      <SiteSeo
        title={`FAQ — STL Compliance`}
        description="Frequently asked questions about STL Compliance implementation, fit, and practical outcomes."
        path="/faq"
      />
      <PageHero
        eyebrow="FAQ"
        title="Answers for real teams, not sales copy"
        subtitle="Short, practical responses to common planning and rollout questions."
      />
      <section className="mx-auto max-w-3xl px-4 pb-16 sm:px-6">
        <div className="space-y-4">
          {QUESTIONS.map((item) => (
            <article
              key={item.question}
              className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5"
            >
              <h2 className="font-semibold text-white">{item.question}</h2>
              <p className="mt-2 text-sm text-slate-300">{item.answer}</p>
            </article>
          ))}
        </div>
      </section>
    </>
  )
}
