import { normalizeProductKey } from './productCatalog'

export function isProductWorkspaceAuthError(error: unknown): boolean {
  if (typeof error !== 'object' || error === null || !('status' in error)) {
    return false
  }

  const status = (error as { status: unknown }).status
  return status === 401 || status === 403
}

export function resolveProductWorkspaceBootstrapError(
  error: unknown,
): 'forbidden' | 'expired' | null {
  if (!isProductWorkspaceAuthError(error)) {
    return null
  }

  const status = (error as { status: number }).status
  return status === 403 ? 'forbidden' : 'expired'
}

const DEFAULT_SUITE_HOME_URL = 'http://localhost:5174/app'
const DEFAULT_SUITE_LOGIN_URL = 'http://localhost:5174/login'

function getCurrentHref(): string {
  const href = globalThis.location?.href
  return typeof href === 'string' && href.trim() ? href : DEFAULT_SUITE_HOME_URL
}

function resolveSuiteLoginUrl(suiteHomeUrl: string | undefined): URL {
  const rawSuiteUrl = suiteHomeUrl?.trim() || DEFAULT_SUITE_HOME_URL
  const baseHref = getCurrentHref()

  try {
    const suiteUrl = new URL(rawSuiteUrl, baseHref)
    const path = suiteUrl.pathname.replace(/\/+$/, '')
    if (path.endsWith('/login')) {
      suiteUrl.pathname = path
    } else if (path.endsWith('/app')) {
      suiteUrl.pathname = `${path.slice(0, -4)}/login`
    } else {
      suiteUrl.pathname = `${path}/login`
    }
    suiteUrl.search = ''
    suiteUrl.hash = ''
    return suiteUrl
  } catch {
    return new URL(DEFAULT_SUITE_LOGIN_URL)
  }
}

export function buildNexArrLoginUrl(input: {
  suiteHomeUrl?: string
  productKey: string
  callbackUrl?: string
}): string {
  const loginUrl = resolveSuiteLoginUrl(input.suiteHomeUrl)
  const callbackUrl = input.callbackUrl?.trim() || getCurrentHref()

  loginUrl.searchParams.set('productKey', normalizeProductKey(input.productKey))
  loginUrl.searchParams.set('callbackUrl', callbackUrl)

  return loginUrl.toString()
}

export function resolveProductLaunchCallbackPath(callbackUrl: string | null | undefined): string {
  const trimmed = callbackUrl?.trim()
  if (!trimmed) {
    return '/'
  }

  try {
    const currentUrl = new URL(getCurrentHref())
    const targetUrl = new URL(trimmed, currentUrl)
    if (targetUrl.origin !== currentUrl.origin) {
      return '/'
    }

    const path = `${targetUrl.pathname}${targetUrl.search}${targetUrl.hash}`
    if (!path || targetUrl.pathname.replace(/\/+$/, '') === '/launch') {
      return '/'
    }

    return path
  } catch {
    return '/'
  }
}
