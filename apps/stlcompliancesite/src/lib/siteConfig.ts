/** Public marketing URLs only. */
export const siteConfig = {
  siteName: 'STL Compliance',
  arrTagline: 'Adaptive Risk Reduction',
  defaultDescription:
    'STL Compliance helps operations teams connect people, training, assets, dispatch, inventory, vendors, and compliance proof in one Adaptive Risk Reduction platform.',
  suiteLoginUrl: import.meta.env.VITE_SUITE_LOGIN_URL ?? 'http://localhost:5174/login',
  contactEmail: import.meta.env.VITE_CONTACT_EMAIL ?? 'hello@stlcompliance.com',
  companyLegalName: 'STL Compliance',
} as const

export function suiteLoginUrl(): string {
  return siteConfig.suiteLoginUrl
}

export function contactMailto(subject?: string): string {
  const params = new URLSearchParams()
  if (subject) {
    params.set('subject', subject)
  }
  const query = params.toString()
  return `mailto:${siteConfig.contactEmail}${query ? `?${query}` : ''}`
}
