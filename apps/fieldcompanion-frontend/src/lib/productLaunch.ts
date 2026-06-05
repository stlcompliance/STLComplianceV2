import { normalizeProductKey, resolveProductLaunchUrl, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { FieldCompanionPlainReason } from './FieldCompanionPlainReason'

export function buildFieldCompanionProductCallbackUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string>,
): string {
  const normalized = normalizeProductKey(productKey)
  if (normalized === 'fieldcompanion') {
    const FieldCompanionLaunch = productLaunchUrls.fieldcompanion
    if (FieldCompanionLaunch) {
      return FieldCompanionLaunch
    }
  }

  return resolveProductLaunchUrl(normalized, suiteHomeUrl, productLaunchUrls)
}

export function formatProductLaunchError(error: unknown): string {
  return FieldCompanionPlainReason(error, 'Product launch failed.')
}

export function resolveFieldCompanionSuiteHomeUrl(env: Record<string, string | undefined>): string {
  return resolveSuiteHomeUrl(env.VITE_SUITE_URL)
}
