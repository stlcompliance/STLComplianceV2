import { normalizeProductKey, resolveProductLaunchUrl, resolveSuiteHomeUrl } from '@stl/shared-ui'

export function buildCompanionProductCallbackUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string>,
): string {
  const normalized = normalizeProductKey(productKey)
  if (normalized === 'companion') {
    const companionLaunch = productLaunchUrls.companion
    if (companionLaunch) {
      return companionLaunch
    }
  }

  return resolveProductLaunchUrl(normalized, suiteHomeUrl, productLaunchUrls)
}

export function formatProductLaunchError(error: unknown): string {
  if (error instanceof Error) {
    return error.message
  }
  return 'Product launch failed.'
}

export function resolveCompanionSuiteHomeUrl(env: Record<string, string | undefined>): string {
  return resolveSuiteHomeUrl(env.VITE_SUITE_URL)
}
