import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

type TermsSection = {
  title: string
  body?: string[]
  items?: string[]
  orderedItems?: string[]
}

const termsSections: TermsSection[] = [
  {
    title: '1. Who We Are',
    body: [
      'STL Compliance provides software intended to help organizations manage operational compliance, personnel readiness, training, maintenance, dispatch, inventory, documentation, and related business workflows.',
      `STL Compliance is operated by ${siteConfig.companyLegalName}, referred to in these Terms as STL Compliance, we, us, or our.`,
    ],
  },
  {
    title: '2. Important Compliance Disclaimer',
    body: [
      'The Services are designed to assist with compliance-related workflows, documentation, reminders, recordkeeping, operational visibility, and internal decision support.',
      'The Services do not provide legal advice, regulatory advice, professional safety advice, engineering advice, tax advice, medical advice, or any other licensed professional service.',
      'You remain solely responsible for determining whether your organization complies with applicable laws, regulations, contracts, permits, policies, standards, and industry requirements. Use of the Services does not guarantee compliance, prevent violations, eliminate risk, or ensure favorable audit, inspection, legal, regulatory, insurance, employment, or business outcomes.',
      'You should consult qualified legal, regulatory, safety, compliance, tax, engineering, or other professional advisors as appropriate for your organization.',
    ],
  },
  {
    title: '3. Eligibility and Authority',
    body: [
      'You may use the Services only if you are legally able to enter into a binding agreement.',
      'If you use the Services on behalf of a company, organization, agency, employer, or other entity, you represent that you have authority to bind that entity to these Terms. In that case, you and your refer to both you personally and the entity you represent.',
    ],
  },
  {
    title: '4. Accounts and Access',
    body: [
      'You may need an account to access certain Services. You agree to provide accurate, current, and complete information and to keep your account information up to date.',
      'You are responsible for maintaining the confidentiality of your login credentials and for all activity occurring under your account.',
      'You agree to notify us promptly if you believe your account has been compromised or used without authorization.',
      'We may suspend or restrict access if we reasonably believe your account or use of the Services creates security risk, violates these Terms, violates law, or may harm STL Compliance, another customer, or the Services.',
    ],
  },
  {
    title: '5. Platform Access and Product Availability',
    body: [
      'STL Compliance may provide multiple products under one platform ecosystem. Access to ordinary products follows active tenant membership, while product-local permissions, service configuration, and administrator controls govern what a user can do inside each product.',
      'Product launch availability follows active tenant membership and product operational state. Access to specific functions, datasets, integrations, administrative tools, or compliance modules remains governed by the relevant permissions, role, service configuration, and administrator controls.',
      'We may add, remove, rename, modify, bundle, split, or retire products or features as the Services evolve.',
    ],
  },
  {
    title: '6. Customer Users and Administrators',
    body: [
      'Your organization may designate administrators or authorized users. Administrators may control access, assign permissions, manage users, configure product settings, invite users, remove users, and access Customer Data within the tenant.',
      'You are responsible for actions taken by your administrators and users.',
      'You are responsible for ensuring that user roles, permissions, access levels, approval workflows, and internal controls are appropriate for your organization.',
    ],
  },
  {
    title: '7. Customer Data',
    body: [
      'Customer Data means data, records, documents, files, images, logs, workflows, configurations, personnel records, maintenance records, training records, inspection records, routing records, inventory records, compliance records, and other information submitted to or generated through the Services by or for you.',
      'You retain ownership of your Customer Data.',
      'You grant STL Compliance a limited license to host, process, transmit, store, copy, display, and use Customer Data as necessary to provide, maintain, secure, support, improve, and operate the Services.',
      'You are responsible for the accuracy, completeness, legality, reliability, and appropriateness of Customer Data.',
      'You are responsible for ensuring that you have the necessary rights, consents, notices, and legal basis to submit, store, process, or share Customer Data through the Services.',
    ],
  },
  {
    title: '8. Sensitive and Regulated Data',
    body: [
      'The Services may involve operational, employment, safety, compliance, vehicle, training, maintenance, inventory, or business records.',
      'Unless we expressly agree in writing, you must not use the Services to store or process:',
    ],
    items: [
      'protected health information subject to HIPAA',
      'payment card data subject to PCI-DSS',
      'classified government information',
      'criminal justice information subject to CJIS',
      'export-controlled technical data requiring special handling',
      'biometric identifiers requiring special consent',
      'highly sensitive personal information not necessary for the intended use of the Services',
    ],
  },
  {
    title: '9. Privacy',
    body: [
      'Our collection, use, and handling of personal information is described in our Privacy Policy.',
      'By using the Services, you acknowledge that STL Compliance may process personal information as described in the Privacy Policy.',
      'If your organization uses the Services to manage employee, contractor, applicant, trainee, driver, technician, customer, vendor, or other person records, you are responsible for providing legally required notices and obtaining legally required consents.',
    ],
  },
  {
    title: '10. Acceptable Use',
    body: ['You agree not to misuse the Services. You may not:'],
    items: [
      'use the Services for unlawful, fraudulent, deceptive, harmful, or abusive purposes',
      'attempt to gain unauthorized access to any account, system, tenant, database, API, or network',
      'interfere with or disrupt the Services',
      'bypass authentication, authorization, access, licensing, billing, or security controls',
      'upload malware, ransomware, spyware, viruses, or malicious code',
      'probe, scan, or test vulnerabilities without written permission',
      'reverse engineer, decompile, or attempt to extract source code except where legally permitted',
      'scrape, harvest, or bulk extract data except through authorized export features',
      'use the Services to violate employment, privacy, safety, transportation, environmental, labor, or other laws',
      'submit false, misleading, fabricated, or deceptive records',
      'use the Services to harass, intimidate, discriminate, retaliate, or unlawfully monitor individuals',
      'resell, sublicense, or commercially exploit the Services without written permission',
    ],
  },
  {
    title: '11. Customer Responsibilities',
    body: ['You are responsible for:'],
    items: [
      'configuring the Services for your organization',
      'validating workflows before relying on them operationally',
      'reviewing alerts, recommendations, reports, and generated outputs',
      'maintaining accurate records',
      'training your users',
      'assigning appropriate permissions',
      'complying with applicable laws and regulations',
      'preserving records as required by your organization or applicable law',
      'maintaining backups or exports where appropriate',
      'independently verifying compliance conclusions before acting on them',
    ],
  },
  {
    title: '12. Compliance Core, Rulepacks, and Regulatory Content',
    body: [
      'Compliance Core, rulepacks, regulatory mappings, citations, requirement libraries, evidence mapping tools, theoretical situation evaluations, and related features are intended to support research, organization, documentation, and operational decision-making.',
      'Regulatory content may be incomplete, outdated, misinterpreted, incorrectly mapped, or not applicable to your specific facts.',
      'You must independently verify any legal, regulatory, safety, or compliance requirement before relying on it.',
      'STL Compliance does not guarantee that rulepacks, citations, evidence mappings, recommendations, alerts, or compliance evaluations are complete, current, legally sufficient, or applicable to your organization.',
    ],
  },
  {
    title: '13. AI, Automation, Recommendations, and Generated Output',
    body: [
      'The Services may include automation, artificial intelligence, rules engines, suggestions, generated text, summaries, classifications, mappings, alerts, risk scoring, or recommendations.',
      'Generated output may be inaccurate, incomplete, outdated, or inappropriate for your situation.',
      'You are responsible for reviewing and approving generated output before relying on it, submitting it, sending it, publishing it, assigning it, or using it in business, employment, compliance, safety, legal, audit, regulatory, or operational decisions.',
      'The Services should not be used as the sole basis for decisions that materially affect a person\'s employment, compensation, safety status, certification status, disciplinary status, legal rights, or access to work unless reviewed by qualified human decision-makers.',
    ],
  },
  {
    title: '14. Third-Party Services and Integrations',
    body: [
      'The Services may integrate with third-party platforms, APIs, identity providers, storage providers, email systems, calendar systems, mapping tools, telematics systems, ELD systems, payment processors, cloud hosting providers, or other external services.',
      'Your use of third-party services may be subject to separate terms and privacy policies.',
      'STL Compliance is not responsible for third-party services, outages, data loss, security incidents, pricing changes, service changes, or discontinued integrations.',
      'You authorize STL Compliance to access, exchange, process, or transmit data with third-party services as necessary to provide configured integrations.',
    ],
  },
  {
    title: '15. Self-Hosted, Hybrid, or Customer-Managed Deployments',
    body: [
      'Some Services may support hosted, self-hosted, customer-managed, hybrid, or connected deployment models.',
      'For customer-managed environments, you are responsible for infrastructure, hosting, security, backups, network access, patching, system availability, data storage, database administration, and environment configuration unless otherwise agreed in writing.',
      'STL Compliance may treat data or requests from customer-managed systems as untrusted input and may validate, reject, limit, or sanitize such data to protect platform integrity.',
    ],
  },
  {
    title: '16. Fees, Billing, and Payment',
    body: [
      'Paid Services require payment of applicable fees.',
      'You agree to pay all fees, taxes, and charges associated with your subscription, order, plan, usage, add-ons, implementation, support, or other paid Services.',
      'Unless otherwise stated in writing, fees are due in advance and are non-refundable except where required by law or expressly stated in an agreement.',
      'We may use third-party payment processors. We do not control payment processor policies or systems.',
      'Failure to pay may result in suspension, downgrade, restriction, or termination of access.',
    ],
  },
  {
    title: '17. Subscriptions, Renewals, and Cancellation',
    body: [
      'Some Services may be offered on a subscription basis.',
      'Subscription terms, renewal periods, pricing, cancellation rights, and billing frequency will be presented during checkout, in an order form, invoice, customer agreement, or account settings.',
      'Subscriptions may renew automatically if disclosed at purchase or agreed in writing.',
      'You are responsible for cancelling before renewal if you do not want the subscription to continue.',
      'Cancellation may stop future billing but does not necessarily entitle you to a refund for amounts already paid, unless required by law or expressly stated in writing.',
    ],
  },
  {
    title: '18. Free Trials and Promotional Access',
    body: [
      'We may offer free trials, promotional access, pilot programs, or limited evaluation access.',
      'Trial, promotional, pilot, or evaluation access may be subject to additional limits, eligibility requirements, usage restrictions, support limits, or written terms.',
      'We may change, suspend, or discontinue trial, promotional, pilot, or evaluation access at any time unless a separate written agreement says otherwise.',
    ],
  },
  {
    title: '19. Service Availability and Support',
    body: [
      'We aim to provide reliable Services, but we do not guarantee uninterrupted availability, uptime, error-free operation, or permanent access.',
      'The Services may be unavailable due to maintenance, updates, outages, security events, third-party failures, hosting provider issues, internet disruptions, or other causes.',
      'Support availability, response times, service levels, and implementation assistance may depend on your plan or separate written agreement.',
    ],
  },
  {
    title: '20. Changes to the Services',
    body: [
      'We may modify, improve, update, rename, replace, suspend, discontinue, or limit any part of the Services.',
      'We may release new products, combine products, split products, retire features, change user interfaces, change APIs, or adjust functionality.',
      'Where practical, we may provide notice of material changes, but we are not required to maintain any feature indefinitely unless a separate written agreement says otherwise.',
    ],
  },
  {
    title: '21. Intellectual Property',
    body: [
      'STL Compliance and its licensors own all rights, title, and interest in the Services, including software, source code, interfaces, designs, workflows, templates, rule structures, documentation, trademarks, logos, product names, content, and related intellectual property.',
      'These Terms do not transfer ownership of the Services to you.',
      'You may use the Services only as permitted by these Terms and your applicable subscription or agreement.',
      'You may not copy, modify, distribute, sell, lease, sublicense, or create derivative works from the Services unless expressly permitted in writing.',
    ],
  },
  {
    title: '22. Feedback',
    body: [
      'If you provide ideas, suggestions, bug reports, feature requests, comments, or other feedback, you grant STL Compliance the right to use that feedback without restriction or compensation.',
      'We may incorporate feedback into the Services without owing you attribution, royalties, ownership rights, or approval rights.',
    ],
  },
  {
    title: '23. Templates, Forms, Documents, and Exports',
    body: [
      'The Services may provide templates, forms, checklists, reports, letters, logs, certificates, inspection records, maintenance records, compliance records, exports, and other documents.',
      'These materials are provided for convenience and workflow support only.',
      'You are responsible for reviewing, editing, validating, approving, retaining, submitting, and using documents appropriately.',
      'STL Compliance does not guarantee that any template, form, report, export, or document satisfies any legal, regulatory, audit, contractual, insurance, employment, or operational requirement.',
    ],
  },
  {
    title: '24. Marketing, Testimonials, and Publicity',
    body: [
      'You may not falsely imply endorsement, partnership, sponsorship, certification, approval, or affiliation with STL Compliance without written permission.',
      'We may use your name, logo, or general description as a customer only if permitted by your order form, agreement, or written approval.',
      'Any testimonial, review, case study, or endorsement must be truthful, accurate, and not misleading.',
    ],
  },
  {
    title: '25. Confidentiality',
    body: [
      'If either party receives non-public business, technical, financial, operational, security, product, customer, or strategic information from the other party, the receiving party must use reasonable care to protect it and may use it only for purposes related to the Services.',
      'Confidentiality obligations do not apply to information that is publicly available, already known without restriction, independently developed, or lawfully received from another source.',
    ],
  },
  {
    title: '26. Security',
    body: [
      'We use reasonable administrative, technical, and organizational measures designed to protect the Services.',
      'No system is completely secure. We do not guarantee that unauthorized access, data loss, cyberattack, malware, ransomware, misconfiguration, or security incident will never occur.',
      'You are responsible for using strong passwords, managing access, reviewing permissions, protecting credentials, securing your devices, and promptly reporting suspected security issues.',
    ],
  },
  {
    title: '27. Data Export and Deletion',
    body: [
      'Depending on your plan and product configuration, you may be able to export certain Customer Data.',
      'After termination or cancellation, access to Customer Data may be limited or unavailable.',
      'We may retain or delete Customer Data according to our policies, legal obligations, backups, security requirements, and applicable agreements.',
      'You are responsible for exporting records you need before termination where export functionality is available.',
    ],
  },
  {
    title: '28. Suspension and Termination',
    body: ['We may suspend or terminate access if:'],
    items: [
      'you violate these Terms',
      'you fail to pay amounts owed',
      'your use creates legal, security, operational, or reputational risk',
      'your use may harm the Services or other customers',
      'required by law, court order, regulator, or government authority',
      'your account appears compromised',
      'you misuse integrations, APIs, or platform access',
    ],
  },
  {
    title: '29. Disclaimers',
    body: [
      'To the fullest extent permitted by law, the Services are provided as is and as available.',
      'STL Compliance disclaims all warranties, whether express, implied, statutory, or otherwise, including warranties of merchantability, fitness for a particular purpose, title, non-infringement, accuracy, availability, and reliability.',
      'We do not warrant that the Services will be uninterrupted, secure, error-free, current, complete, legally sufficient, compliant with every regulation, or suitable for your specific organization.',
      'We do not warrant that the Services will detect every issue, prevent every violation, prevent every incident, identify every risk, maintain every record, or satisfy every audit.',
    ],
  },
  {
    title: '30. Limitation of Liability',
    body: [
      'To the fullest extent permitted by law, STL Compliance will not be liable for indirect, incidental, special, consequential, exemplary, punitive, or enhanced damages, including lost profits, lost revenue, lost data, business interruption, loss of goodwill, regulatory penalties, failed audits, employment claims, safety incidents, operational downtime, or replacement costs.',
      'To the fullest extent permitted by law, STL Compliance\'s total liability for all claims arising out of or related to the Services or these Terms will not exceed the greater of:',
    ],
    orderedItems: [
      'the amount you paid to STL Compliance for the Services giving rise to the claim during the twelve months before the event giving rise to liability',
      '$100',
    ],
  },
  {
    title: '31. Indemnification',
    body: [
      'You agree to defend, indemnify, and hold harmless STL Compliance, its owners, officers, employees, contractors, agents, affiliates, licensors, and service providers from and against claims, damages, liabilities, losses, costs, and expenses, including reasonable attorneys\' fees, arising from or related to:',
    ],
    items: [
      'your use of the Services',
      'your Customer Data',
      'your violation of these Terms',
      'your violation of law',
      'your users\' actions',
      'your business operations',
      'your employment, safety, compliance, maintenance, routing, training, inventory, or regulatory decisions',
      'your third-party integrations',
      'your infringement or misuse of third-party rights',
    ],
  },
  {
    title: '32. Governing Law',
    body: [
      'These Terms are governed by the laws of the State of Missouri, without regard to conflict-of-law rules, unless applicable law requires otherwise.',
      'Any dispute arising from or related to these Terms or the Services will be brought in the state or federal courts located in Missouri, unless applicable law requires another venue.',
    ],
  },
  {
    title: '33. Dispute Resolution',
    body: [
      'Before filing a lawsuit, either party must first attempt to resolve the dispute informally by providing written notice describing the issue and allowing at least 30 days for good-faith resolution.',
      'This section does not prevent either party from seeking emergency injunctive relief, enforcing intellectual property rights, addressing unauthorized access, or complying with legal obligations.',
    ],
  },
  {
    title: '34. Changes to These Terms',
    body: [
      'We may update these Terms from time to time.',
      'When we make material changes, we may provide notice by posting the revised Terms, updating the effective date, sending an email, displaying an in-product notice, or using another reasonable method.',
      'Your continued use of the Services after updated Terms become effective means you accept the updated Terms.',
    ],
  },
  {
    title: '35. Additional Agreements',
    body: [
      'Some Services may be subject to additional terms, order forms, statements of work, data processing agreements, service-level agreements, business associate agreements, implementation agreements, support agreements, or product-specific terms.',
      'If there is a conflict between these Terms and a signed written agreement, the signed written agreement controls for that conflict.',
    ],
  },
  {
    title: '36. Assignment',
    body: [
      'You may not assign or transfer these Terms without our written consent.',
      'We may assign these Terms as part of a merger, acquisition, financing, corporate reorganization, sale of assets, change of control, or transfer of the Services.',
    ],
  },
  {
    title: '37. Severability',
    body: [
      'If any part of these Terms is found unenforceable, the remaining sections will remain in effect.',
      'The unenforceable provision will be modified to the minimum extent necessary to make it enforceable, or removed if modification is not permitted.',
    ],
  },
  {
    title: '38. No Waiver',
    body: ['Failure to enforce any provision of these Terms does not waive our right to enforce that provision later.'],
  },
  {
    title: '39. Entire Agreement',
    body: [
      'These Terms, together with any applicable order form, product-specific terms, privacy policy, or written agreement, form the entire agreement between you and STL Compliance regarding the Services.',
    ],
  },
]

export function TermsPage() {
  return (
    <>
      <SiteSeo
        title={`Terms and Conditions - ${siteConfig.siteName}`}
        description={`Terms and Conditions for ${siteConfig.companyLegalName} websites, applications, products, software, content, and related services.`}
        path="/terms"
      />
      <PageHero
        eyebrow="Legal"
        title="Terms and Conditions"
        subtitle="Terms governing access to and use of STL Compliance websites, applications, products, services, software, content, and related offerings."
      />
      <article className="mx-auto max-w-4xl px-4 pb-16 text-slate-300 sm:px-6">
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-6 shadow-2xl shadow-slate-950/20">
          <p className="text-sm font-semibold uppercase tracking-[0.16em] text-teal-300">
            Effective Date: June 3, 2026
          </p>
          <div className="mt-6 space-y-4 text-base leading-7">
            <p>
              These Terms and Conditions govern your access to and use of STL Compliance websites,
              applications, products, services, software, content, and related offerings, including
              but not limited to StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, LoadArr,
              Compliance Core, Field Companion, and any related STL Compliance services collectively
              referred to as the Services.
            </p>
            <p>
              By accessing or using the Services, creating an account, subscribing to a product, or
              clicking to accept these Terms, you agree to be bound by these Terms. If you do not
              agree, do not use the Services.
            </p>
          </div>
        </div>

        <div className="mt-10 space-y-10">
          {termsSections.map((section) => (
            <section key={section.title} className="space-y-5">
              <h2 className="text-2xl font-semibold text-white">{section.title}</h2>
              {section.body?.map((paragraph) => (
                <p key={paragraph} className="leading-7">
                  {paragraph}
                </p>
              ))}
              {section.items ? <TermsList items={section.items} /> : null}
              {section.orderedItems ? <TermsOrderedList items={section.orderedItems} /> : null}
            </section>
          ))}
        </div>

        <section className="mt-10 rounded-lg border border-slate-800 bg-slate-950/60 p-6">
          <h2 className="text-2xl font-semibold text-white">40. Contact</h2>
          <p className="mt-4 leading-7">Questions about these Terms may be sent to:</p>
          <address className="mt-4 not-italic leading-7 text-slate-200">
            STL Compliance
            <br />
            {siteConfig.companyLegalName}
            <br />
            {siteConfig.mailingAddress}
            <br />
            Email:{' '}
            <a className="text-teal-300 hover:text-teal-200" href={`mailto:${siteConfig.contactEmail}`}>
              {siteConfig.contactEmail}
            </a>
            <br />
            Website:{' '}
            <a className="text-teal-300 hover:text-teal-200" href="https://stlcompliance.com">
              https://stlcompliance.com
            </a>
          </address>
        </section>
      </article>
    </>
  )
}

function TermsList({ items }: { items: string[] }) {
  return (
    <ul className="ml-5 list-disc space-y-2 leading-7 text-slate-300">
      {items.map((item) => (
        <li key={item}>{item}</li>
      ))}
    </ul>
  )
}

function TermsOrderedList({ items }: { items: string[] }) {
  return (
    <ol className="ml-5 list-decimal space-y-2 leading-7 text-slate-300">
      {items.map((item) => (
        <li key={item}>{item}</li>
      ))}
    </ol>
  )
}
