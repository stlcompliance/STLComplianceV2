import {
  getProductRouteSlug,
  normalizeProductKey,
  SUITE_PRODUCT_CATALOG,
} from './productCatalog'
import { normalizeBrowserLaunchUrl } from './browserLaunchUrl'

/** Local Vite preview bases aligned with StlE2eFrontendCatalog (+ Field Companion). */
const LOCAL_FRONTEND_BASES: Record<string, string> = {
  staffarr: 'http://localhost:5175',
  trainarr: 'http://localhost:5176',
  compliancecore: 'http://localhost:5177',
  maintainarr: 'http://localhost:5178',
  supplyarr: 'http://localhost:5179',
  customarr: 'http://localhost:5186',
  routarr: 'http://localhost:5180',
  fieldcompanion: 'http://localhost:5181',
  loadarr: 'http://localhost:5182',
  assurarr: 'http://localhost:5183',
  recordarr: 'http://localhost:5184',
  reportarr: 'http://localhost:5185',
}

function resolveSuiteHomeUrl(suiteHomeUrl: string): string {
  const trimmed = suiteHomeUrl.trim()
  if (!trimmed) {
    return 'http://localhost:5174/app'
  }
  const resolved = trimmed.endsWith('/app') ? trimmed : `${trimmed.replace(/\/$/, '')}/app`
  return normalizeBrowserLaunchUrl(resolved)
}

function readFrontendBase(
  env: Record<string, string | undefined>,
  productKey: string,
): string | undefined {
  const normalized = normalizeProductKey(productKey)
  const upper = normalized.toUpperCase()
  const candidates = [env[`VITE_${upper}_FRONTEND_BASE`], env[`VITE_${upper}_FRONTEND_URL`]]
  for (const value of candidates) {
    const trimmed = value?.trim()
    if (trimmed) {
      return normalizeBrowserLaunchUrl(trimmed.replace(/\/$/, ''))
    }
  }
  return LOCAL_FRONTEND_BASES[normalized]
}

function readAppPublicBaseUrl(env: Record<string, string | undefined>): string | undefined {
  const trimmed = env.VITE_APP_PUBLIC_BASE_URL?.trim()
  return trimmed ? trimmed.replace(/\/$/, '') : undefined
}

function getProductLaunchPath(_productKey: string): string {
  return '/launch'
}

/** Build direct `/launch` URLs for entitled product frontends from Vite env (with local defaults). */
export function buildProductLaunchUrlMap(
  env: Record<string, string | undefined>,
): Record<string, string> {
  const map: Record<string, string> = {}
  const appBase = readAppPublicBaseUrl(env)
  for (const entry of SUITE_PRODUCT_CATALOG) {
    if (entry.productKey === 'nexarr') {
      continue
    }
    const launchPath = getProductLaunchPath(entry.productKey)
    if (appBase) {
      map[entry.productKey] = `${appBase}/${getProductRouteSlug(entry.productKey)}${launchPath}`
      continue
    }
    const base = readFrontendBase(env, entry.productKey)
    if (base) {
      map[entry.productKey] = `${base}${launchPath}`
    }
  }
  return map
}

/** Resolve launch target for a product from the topbar switcher. */
export function resolveProductLaunchUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string> = {},
): string {
  const normalized = normalizeProductKey(productKey)
  if (normalized === 'nexarr') {
    return resolveSuiteHomeUrl(suiteHomeUrl)
  }

  const direct = productLaunchUrls[normalized]
  if (direct) {
    return normalizeBrowserLaunchUrl(direct)
  }

  const suiteUrl = resolveSuiteHomeUrl(suiteHomeUrl)
  return `${suiteUrl}/${getProductRouteSlug(normalized)}${getProductLaunchPath(normalized)}`
}
