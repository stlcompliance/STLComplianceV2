/** Public marketing URLs only. */
export const siteConfig = {
  siteName: 'STL Compliance',
  arrTagline: 'Adaptive Risk Reduction',
  defaultDescription:
    'STL Compliance helps operations teams connect people, training, assets, dispatch, inventory, vendors, and compliance proof in one Adaptive Risk Reduction platform.',
  suiteLoginUrl: import.meta.env.VITE_SUITE_LOGIN_URL ?? 'http://localhost:5174/login',
  knowledgeBaseUrl: import.meta.env.VITE_KB_URL ?? 'https://kb.stlcompliance.com',
  contactEmail: import.meta.env.VITE_CONTACT_EMAIL ?? 'hello@stlcompliance.com',
  privacyEmail: import.meta.env.VITE_PRIVACY_EMAIL ?? 'privacy@stlcompliance.com',
  companyLegalName: 'STL Compliance LLC',
  mailingAddress: '303 N Sparta St, Steeleville, IL 62288',
} as const

export function suiteLoginUrl(): string {
  return siteConfig.suiteLoginUrl
}

export function knowledgeBaseUrl(): string {
  return siteConfig.knowledgeBaseUrl
}

export function contactMailto(subject?: string): string {
  const params = new URLSearchParams()
  if (subject) {
    params.set('subject', subject)
  }
  const query = params.toString()
  return `mailto:${siteConfig.contactEmail}${query ? `?${query}` : ''}`
}
