import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage, normalizeProductKey } from '@stl/shared-ui'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { PermissionGate } from '../components/PermissionGate'
import { useLaunchContextGate } from '../hooks/useLaunchContextGate'
import { useProductLaunch } from '../hooks/useProductLaunch'
import {
  canAccessProductRoute,
  isInSuiteProduct,
} from '../lib/permissions'
import { redirectToSuiteLoginIfSessionExpired } from '../lib/sessionRedirect'
import { getProductDisplayName } from '../navigation/suiteNavigation'

export function ProductHubPage() {
  const { productKey = '' } = useParams<{ productKey: string }>()
  const { me } = useAuth()
  const launch = useProductLaunch()
  const normalized = normalizeProductKey(productKey)
  const productDisplayName = getProductDisplayName(
    normalized,
    normalized.charAt(0).toUpperCase() + normalized.slice(1),
  )
  const canAccess = canAccessProductRoute(me?.entitlements ?? [], normalized)
  const launchAllowedQuery = useLaunchContextGate(normalized)

  const contextQuery = useQuery({
    queryKey: ['launch-context-detail', normalized, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(normalized),
    enabled: Boolean(me) && canAccess && !isInSuiteProduct(normalized),
  })

  useEffect(() => {
    if (contextQuery.isError) {
      redirectToSuiteLoginIfSessionExpired(contextQuery.error, normalized)
    }
  }, [contextQuery.error, contextQuery.isError, normalized])

  return (
    <div className="max-w-2xl space-y-4">
      <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">{productDisplayName}</h3>

      <PermissionGate
        allowed={canAccess}
        fallback={
          <ApiErrorCallout message="You are not entitled to this product." />
        }
      >
        {isInSuiteProduct(normalized) ? (
          <p className="text-sm text-[var(--color-text-secondary)]">
            {productDisplayName} platform surfaces will live here. Identity and launch APIs are wired.
          </p>
        ) : (
          <div className="space-y-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm">
            {contextQuery.data && (
              <>
                <p className="text-[var(--color-text-secondary)]">
                  <span className="font-medium text-[var(--color-text-primary)]">Launch URL:</span>{' '}
                  {contextQuery.data.launchUrl || '—'}
                </p>
                <p className="text-[var(--color-text-secondary)]">
                  <span className="font-medium text-[var(--color-text-primary)]">Can launch:</span>{' '}
                  {contextQuery.data.canLaunch ? 'Yes' : 'No'}
                  {contextQuery.data.denialReasonCode
                    ? ` (${contextQuery.data.denialReasonCode})`
                    : ''}
                </p>
              </>
            )}
            {contextQuery.isError && (
              <ApiErrorCallout
                message={getErrorMessage(
                  contextQuery.error,
                  'Failed to load product launch context.',
                )}
                onRetry={() => void contextQuery.refetch()}
                retryLabel="Retry launch context"
              />
            )}

            <PermissionGate allowed={launchAllowedQuery.data === true}>
              <button
                type="button"
                disabled={launch.isPending}
                onClick={() => launch.mutate(normalized)}
                className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-60"
              >
                {launch.isPending ? 'Launching…' : 'Launch product (handoff)'}
              </button>
            </PermissionGate>
          </div>
        )}
      </PermissionGate>
    </div>
  )
}
