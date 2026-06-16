export const kbConfig = {
  siteName: 'STL Compliance Knowledge Base',
  shortName: 'STL KB',
  description:
    'Plain-language STL Compliance help for tenant users, product admins, managers, compliance users, and frontline teams.',
  kbBaseUrl: import.meta.env.VITE_KB_BASE_URL ?? 'https://kb.stlcompliance.com',
  marketingSiteUrl: import.meta.env.VITE_MARKETING_SITE_URL ?? 'https://stlcompliance.com',
  suiteLoginUrl: import.meta.env.VITE_SUITE_LOGIN_URL ?? 'https://app.stlcompliance.com/login',
} as const
