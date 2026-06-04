import { normalizeProductKey, resolveProductLaunchUrl, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { companionPlainReason } from './companionPlainReason'

export function buildCompanionProductCallbackUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string>,
): string {
  const normalized = normalizeProductKey(productKey)
  if (normalized === 'fieldcompanion') {
    const companionLaunch = productLaunchUrls.fieldcompanion
    if (companionLaunch) {
      return companionLaunch
    }
  }

  return resolveProductLaunchUrl(normalized, suiteHomeUrl, productLaunchUrls)
}

export function formatProductLaunchError(error: unknown): string {
  return companionPlainReason(error, 'Product launch failed.')
}

export function resolveCompanionSuiteHomeUrl(env: Record<string, string | undefined>): string {
  return resolveSuiteHomeUrl(env.VITE_SUITE_URL)
}
