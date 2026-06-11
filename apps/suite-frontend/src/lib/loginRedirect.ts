import { normalizeProductKey } from '@stl/shared-ui'

export type LoginRedirectTarget =
  | {
      kind: 'product'
      productKey: string
      callbackUrl: string
    }
  | {
      kind: 'internal'
      to: string
    }

function getCurrentHref(currentHref?: string): string {
  return currentHref?.trim() || globalThis.location?.href || 'http://localhost:5174/login'
}

function parseUrl(value: string, currentHref?: string): URL | null {
  try {
    return new URL(value, getCurrentHref(currentHref))
  } catch {
    return null
  }
}

function isHttpUrl(url: URL): boolean {
  return url.protocol === 'http:' || url.protocol === 'https:'
}

function resolveProductCallbackTarget(
  productKey: string,
  callbackUrl: URL,
  productLaunchUrls: Record<string, string>,
  currentHref?: string,
): LoginRedirectTarget | null {
  const launchUrl = productLaunchUrls[productKey]
  if (!launchUrl) {
    return null
  }

  const allowedLaunchUrl = parseUrl(launchUrl, currentHref)
  if (!allowedLaunchUrl || !isHttpUrl(allowedLaunchUrl)) {
    return null
  }

  if (callbackUrl.origin !== allowedLaunchUrl.origin) {
    return null
  }

  return {
    kind: 'product',
    productKey,
    callbackUrl: callbackUrl.toString(),
  }
}

export function resolveLoginRedirectTarget(
  search: string,
  productLaunchUrls: Record<string, string>,
  currentHref?: string,
): LoginRedirectTarget | null {
  const params = new URLSearchParams(search)
  const rawCallbackUrl = params.get('callbackUrl') ?? params.get('callbackurl')
  if (!rawCallbackUrl?.trim()) {
    return null
  }

  const callbackUrl = parseUrl(rawCallbackUrl, currentHref)
  if (!callbackUrl || !isHttpUrl(callbackUrl)) {
    return null
  }

  const rawProductKey = params.get('productKey') ?? params.get('productkey')
  if (rawProductKey?.trim()) {
    return resolveProductCallbackTarget(
      normalizeProductKey(rawProductKey),
      callbackUrl,
      productLaunchUrls,
      currentHref,
    )
  }

  const currentUrl = parseUrl(getCurrentHref(currentHref), currentHref)
  if (currentUrl && callbackUrl.origin === currentUrl.origin) {
    return {
      kind: 'internal',
      to: `${callbackUrl.pathname}${callbackUrl.search}${callbackUrl.hash}`,
    }
  }

  return null
}
