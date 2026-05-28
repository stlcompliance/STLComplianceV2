/** Default local suite Vite port per StlE2eFrontendCatalog. */
const DEFAULT_SUITE_HOME_URL = 'http://localhost:5174/app'

export function resolveSuiteHomeUrl(envValue: string | undefined): string {
  const trimmed = envValue?.trim()
  if (!trimmed) {
    return DEFAULT_SUITE_HOME_URL
  }
  return trimmed.endsWith('/app') ? trimmed : `${trimmed.replace(/\/$/, '')}/app`
}
