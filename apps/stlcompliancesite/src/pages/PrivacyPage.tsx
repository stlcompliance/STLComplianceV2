import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

type PolicySection = {
  title: string
  body?: string[]
  groups?: {
    title: string
    body?: string[]
    items?: string[]
  }[]
  items?: string[]
}

const policySections: PolicySection[] = [
  {
    title: '1. Information We Collect',
    body: ['We collect information needed to provide, secure, maintain, improve, and support the Services.'],
    groups: [
      {
        title: '1.1 Account Information',
        body: [
          'We may collect account information to create accounts, authenticate users, manage access, support tenant administration, and provide the Services.',
        ],
        items: [
          'name',
          'email address',
          'phone number',
          'username',
          'organization name',
          'job title or role',
          'tenant, company, or workspace association',
          'login credentials or authentication-related information',
          'product launch, role, permission, and launch-session information',
        ],
      },
      {
        title: '1.2 Customer Data',
        body: [
          'Customers and authorized users may submit, upload, create, or generate data through the Services. Customer Data belongs to the customer or organization that submitted it. STL Compliance processes Customer Data to provide the Services.',
          'Customer Data may include, depending on the products used:',
        ],
        items: [
          'personnel records',
          'training records',
          'qualification records',
          'certification records',
          'maintenance records',
          'inspection records',
          'asset records',
          'dispatch and routing records',
          'inventory records',
          'vendor and customer records',
          'compliance records',
          'documents, files, photos, attachments, notes, comments, forms, approvals, and audit history',
        ],
      },
      {
        title: '1.3 Usage and Diagnostic Information',
        body: [
          'We may collect usage, diagnostic, technical, and operational information for bug fixes, troubleshooting, support, feature improvements, product planning, service performance, security monitoring, abuse prevention, internal audits, and compliance-related operational review.',
        ],
        items: [
          'pages, screens, modules, and features used',
          'buttons, workflows, forms, and actions used',
          'timestamps and session activity',
          'device type, browser type, and operating system',
          'approximate location based on IP address',
          'IP address',
          'referring URLs',
          'log files',
          'error reports, crash reports, and performance data',
          'API requests and system events',
          'failed login attempts, security events, and audit logs',
        ],
      },
      {
        title: '1.4 Cookies and Similar Technologies',
        body: [
          'We use cookies, local storage, session storage, and similar technologies. Cookies are small files or data stored on your device or browser.',
          'We may use cookies and similar technologies for:',
        ],
        items: [
          'login and authentication',
          'session management and keeping users signed in',
          'remembering tenant or workspace selection',
          'maintaining security protections and preventing unauthorized access',
          'detecting suspicious activity',
          'remembering preferences',
          'measuring product usage',
          'diagnosing errors',
          'improving features',
          'internal analytics',
          'internal audit and security logging',
        ],
      },
    ],
  },
  {
    title: '2. Types of Cookies We Use',
    groups: [
      {
        title: '2.1 Strictly Necessary Cookies',
        body: [
          'These cookies are required for the Services to function. You cannot disable strictly necessary cookies through our cookie tools because the Services may not work without them.',
        ],
        items: [
          'logging in',
          'maintaining secure sessions',
          'routing users to the correct tenant or product',
          'protecting against cross-site request forgery',
          'preventing unauthorized access',
          'load balancing',
          'security checks',
          'storing essential account or session state',
        ],
      },
      {
        title: '2.2 Preference Cookies',
        body: ['These cookies remember choices and help improve usability.'],
        items: [
          'selected tenant',
          'product selection',
          'interface preferences',
          'saved settings',
          'display preferences',
        ],
      },
      {
        title: '2.3 Usage and Analytics Cookies',
        body: [
          'We may use usage or analytics cookies to understand how the Services are used and how they can be improved. We use this information for internal product improvement, bug fixes, service reliability, internal audits, and feature planning.',
        ],
        items: [
          'which features are used most often',
          'where users encounter errors',
          'how workflows perform',
          'which pages load slowly',
          'which product areas need improvement',
          'whether recent updates caused bugs or usability issues',
        ],
      },
      {
        title: '2.4 Security Cookies',
        body: ['We may use security cookies to:'],
        items: [
          'detect suspicious login activity',
          'prevent account abuse',
          'identify session hijacking risks',
          'protect against fraud',
          'enforce authentication',
          'protect tenant boundaries',
          'support audit trails',
        ],
      },
      {
        title: '2.5 Advertising Cookies',
        body: [
          'STL Compliance does not currently use advertising cookies for cross-site behavioral advertising unless specifically disclosed.',
          'If we later use advertising or remarketing cookies, we will update this Privacy Policy or provide additional notice as required.',
        ],
      },
    ],
  },
  {
    title: '3. How We Use Information',
    body: ['We use information for the following purposes:'],
    items: [
      'to provide the Services',
      'to authenticate users',
      'to manage accounts',
      'to manage tenants, roles, permissions, and product launch context',
      'to provide customer support',
      'to troubleshoot bugs and errors',
      'to improve features and workflows',
      'to monitor service performance',
      'to conduct internal audits',
      'to maintain security',
      'to detect abuse, fraud, or unauthorized access',
      'to comply with legal obligations',
      'to enforce our Terms and Conditions',
      'to communicate with users and customers',
      'to send service notices',
      'to evaluate product usage and reliability',
      'to develop new products, features, and integrations',
    ],
    groups: [
      {
        title: 'Customer decisions',
        body: [
          'We do not use Customer Data to make legal, regulatory, employment, safety, or compliance decisions on behalf of customers. Customers remain responsible for reviewing and approving their own records, workflows, and decisions.',
        ],
      },
    ],
  },
  {
    title: '4. Internal Audits and Operational Review',
    body: [
      'We may review account activity, usage logs, audit logs, security events, workflow history, and system activity for internal purposes, including:',
    ],
    items: [
      'security review',
      'bug investigation',
      'support investigation',
      'abuse prevention',
      'compliance with internal policies',
      'service reliability',
      'product improvement',
      'billing and access verification',
      'investigation of suspected misuse',
      'review of administrative actions',
      'review of system performance',
    ],
    groups: [
      {
        title: 'Purpose',
        body: ['Internal audits are intended to protect users, customers, STL Compliance, and the Services.'],
      },
    ],
  },
  {
    title: '5. How We Share Information',
    body: ['We do not sell personal information. We may share information in limited circumstances, including:'],
    groups: [
      {
        title: '5.1 Service Providers',
        body: [
          'We may share information with vendors and service providers that help us operate the Services. These providers may process information only as needed to provide services to us.',
        ],
        items: [
          'hosting providers',
          'database providers',
          'authentication providers',
          'email providers',
          'payment processors',
          'analytics providers',
          'error monitoring providers',
          'customer support tools',
          'security tools',
          'infrastructure providers',
        ],
      },
      {
        title: '5.2 Customer Administrators',
        body: [
          'If your account is part of an organization, tenant, workspace, employer, or customer account, administrators may be able to access information associated with your use of the Services, including account information, role information, activity logs, records, assignments, approvals, and Customer Data.',
        ],
      },
      {
        title: '5.3 Legal and Safety Reasons',
        body: ['We may disclose information if we believe disclosure is necessary to:'],
        items: [
          'comply with law',
          'respond to subpoenas, court orders, or legal process',
          'protect the rights, safety, or property of STL Compliance, customers, users, or others',
          'investigate fraud, abuse, or security incidents',
          'enforce our Terms and Conditions',
          'prevent harm or unauthorized access',
        ],
      },
      {
        title: '5.4 Business Transfers',
        body: [
          'If STL Compliance is involved in a merger, acquisition, financing, reorganization, sale of assets, or similar transaction, information may be transferred as part of that transaction.',
        ],
      },
    ],
  },
  {
    title: '6. Customer Responsibility for Personal Information',
    body: [
      'Customers are responsible for the personal information they submit to the Services.',
      'If a customer uses the Services to manage employees, contractors, trainees, drivers, technicians, vendors, customers, or other individuals, the customer is responsible for:',
    ],
    items: [
      'providing required privacy notices',
      'obtaining required consents',
      'ensuring the data is lawful to collect and use',
      'assigning appropriate permissions',
      'limiting access to authorized users',
      'maintaining accurate records',
      'complying with employment, privacy, safety, transportation, and compliance laws',
    ],
  },
  {
    title: '7. Data Retention',
    body: [
      'We retain information for as long as reasonably necessary to provide the Services, comply with legal obligations, resolve disputes, maintain security, conduct audits, enforce agreements, and support legitimate business purposes.',
      'Retention periods may vary based on:',
    ],
    items: [
      'account status',
      'customer configuration',
      'product used',
      'legal requirements',
      'audit requirements',
      'backup systems',
      'security needs',
      'contractual obligations',
    ],
    groups: [
      {
        title: 'Exports',
        body: [
          'Customers are responsible for exporting records they need before cancelling or terminating the Services where export functionality is available.',
        ],
      },
    ],
  },
  {
    title: '8. Data Security',
    body: [
      'We use reasonable administrative, technical, and organizational safeguards designed to protect information. These safeguards may include:',
    ],
    items: [
      'authentication controls',
      'authorization controls',
      'tenant separation',
      'encryption where appropriate',
      'access logging',
      'audit logging',
      'monitoring',
      'least-privilege access',
      'secure infrastructure practices',
      'security review of suspicious activity',
    ],
    groups: [
      {
        title: 'Security limits',
        body: [
          'No system is perfectly secure. We cannot guarantee that unauthorized access, cyberattack, data loss, outage, or security incident will never occur.',
        ],
      },
    ],
  },
  {
    title: '9. Your Choices',
    body: ['Depending on your location, account type, and applicable law, you may have rights to:'],
    items: [
      'access personal information',
      'correct personal information',
      'delete personal information',
      'export personal information',
      'object to certain processing',
      'restrict certain processing',
      'opt out of certain uses',
      'withdraw consent where processing is based on consent',
    ],
    groups: [
      {
        title: 'Requests',
        body: [
          `Some requests may need to be handled through your organization's administrator if your account is provided by an employer or customer.`,
          `To make a privacy request, contact us at ${siteConfig.privacyEmail}.`,
        ],
      },
    ],
  },
  {
    title: '10. Cookie Choices',
    body: [
      'You may be able to control cookies through your browser settings.',
      'Blocking strictly necessary cookies may prevent the Services from working properly.',
      'Where required, we may provide cookie notices or consent options for non-essential cookies, such as analytics or advertising cookies.',
    ],
  },
  {
    title: "11. Children's Privacy",
    body: [
      'The Services are not intended for children under 13.',
      'We do not knowingly collect personal information from children under 13.',
      `If you believe a child has provided personal information to us, contact us at ${siteConfig.privacyEmail}.`,
    ],
  },
  {
    title: '12. International Users',
    body: [
      'STL Compliance is based in the United States.',
      'If you access the Services from outside the United States, your information may be processed in the United States or other countries where our service providers operate.',
      'Privacy laws may differ from those in your location.',
    ],
  },
  {
    title: '13. Third-Party Links and Integrations',
    body: [
      'The Services may link to or integrate with third-party services.',
      'Third-party services have their own privacy practices. We are not responsible for the privacy practices of third parties.',
      'Your use of third-party services may be governed by separate terms and privacy policies.',
    ],
  },
  {
    title: '14. Changes to This Privacy Policy',
    body: [
      'We may update this Privacy Policy from time to time.',
      'When we make material changes, we may notify users by updating the effective date, posting the revised policy, sending an email, displaying an in-product notice, or using another reasonable method.',
      'Your continued use of the Services after changes become effective means you acknowledge the revised Privacy Policy.',
    ],
  },
]

export function PrivacyPage() {
  return (
    <>
      <SiteSeo
        title={`Privacy Policy - ${siteConfig.siteName}`}
        description={`Privacy Policy for ${siteConfig.companyLegalName} websites, applications, products, software, APIs, and related services.`}
        path="/privacy"
      />
      <PageHero
        eyebrow="Legal"
        title="Privacy Policy"
        subtitle="How STL Compliance collects, uses, stores, shares, and protects information across its websites, applications, products, software, APIs, and related services."
      />
      <article className="mx-auto max-w-4xl px-4 pb-16 text-slate-300 sm:px-6">
        <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-6 shadow-2xl shadow-slate-950/20">
          <p className="text-sm font-semibold uppercase tracking-[0.16em] text-teal-300">
            Effective Date: June 3, 2026
          </p>
          <div className="mt-6 space-y-4 text-base leading-7">
            <p>
              This Privacy Policy explains how STL Compliance collects, uses, stores, shares, and
              protects information when you use our websites, applications, products, software, APIs,
              and related services, including StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr,
              LoadArr, Compliance Core, Field Companion, and related STL Compliance services collectively
              referred to as the Services.
            </p>
            <p>
              STL Compliance is operated by {siteConfig.companyLegalName}, referred to in this
              Privacy Policy as STL Compliance, we, us, or our.
            </p>
            <p>By using the Services, you acknowledge this Privacy Policy.</p>
          </div>
        </div>

        <div className="mt-10 space-y-10">
          {policySections.map((section) => (
            <section key={section.title} className="space-y-5">
              <h2 className="text-2xl font-semibold text-white">{section.title}</h2>
              {section.body?.map((paragraph) => (
                <p key={paragraph} className="leading-7">
                  {paragraph}
                </p>
              ))}
              {section.items ? <PolicyList items={section.items} /> : null}
              {section.groups?.map((group) => (
                <div key={group.title} className="space-y-3">
                  <h3 className="text-lg font-semibold text-slate-100">{group.title}</h3>
                  {group.body?.map((paragraph) => (
                    <p key={paragraph} className="leading-7">
                      {paragraph}
                    </p>
                  ))}
                  {group.items ? <PolicyList items={group.items} /> : null}
                </div>
              ))}
            </section>
          ))}
        </div>

        <section className="mt-10 rounded-lg border border-slate-800 bg-slate-950/60 p-6">
          <h2 className="text-2xl font-semibold text-white">15. Contact Us</h2>
          <p className="mt-4 leading-7">
            Questions or requests about this Privacy Policy may be sent to:
          </p>
          <address className="mt-4 not-italic leading-7 text-slate-200">
            STL Compliance
            <br />
            {siteConfig.companyLegalName}
            <br />
            {siteConfig.mailingAddress}
            <br />
            Email:{' '}
            <a className="text-teal-300 hover:text-teal-200" href={`mailto:${siteConfig.privacyEmail}`}>
              {siteConfig.privacyEmail}
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

function PolicyList({ items }: { items: string[] }) {
  return (
    <ul className="ml-5 list-disc space-y-2 leading-7 text-slate-300">
      {items.map((item) => (
        <li key={item}>{item}</li>
      ))}
    </ul>
  )
}
