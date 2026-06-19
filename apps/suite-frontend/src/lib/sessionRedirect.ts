import { buildNexArrLoginUrl } from '@stl/shared-ui'
import { NexarrApiError } from '../api/types'

export function isExpiredSessionError(error: unknown): boolean {
  return error instanceof NexarrApiError && error.status === 401
}

export function buildSuiteLoginRedirectUrl(productKey: string): string {
  return buildNexArrLoginUrl({
    suiteHomeUrl: '/app',
    productKey,
  })
}

export function redirectToSuiteLogin(productKey: string): void {
  window.location.assign(buildSuiteLoginRedirectUrl(productKey))
}

export function redirectToSuiteLoginIfSessionExpired(
  error: unknown,
  productKey: string,
): boolean {
  if (!isExpiredSessionError(error)) {
    return false
  }

  redirectToSuiteLogin(productKey)
  return true
}
