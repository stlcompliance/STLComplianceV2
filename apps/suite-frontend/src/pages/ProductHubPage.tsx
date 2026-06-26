import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage, getSuiteProductCatalogEntry, normalizeProductKey } from '@stl/shared-ui'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { LaunchFailurePanel } from '../components/nexarr/LaunchFailurePanel'
import { useLaunchContextGate } from '../hooks/useLaunchContextGate'
import { useProductLaunch } from '../hooks/useProductLaunch'
import { isInSuiteProduct } from '../lib/permissions'
import { redirectToSuiteLoginIfSessionExpired } from '../lib/sessionRedirect'
import { getProductDisplayName } from '../navigation/suiteNavigation'

export function ProductHubPage() {
  const { productKey = '' } = useParams<{ productKey: string }>()
  const { me } = useAuth()
  const launch = useProductLaunch()
  const normalized = normalizeProductKey(productKey)
  const catalogEntry = getSuiteProductCatalogEntry(normalized)
  const productDisplayName = getProductDisplayName(
    normalized,
    catalogEntry?.displayName ?? normalized.charAt(0).toUpperCase() + normalized.slice(1),
  )
  const launchAllowedQuery = useLaunchContextGate(normalized)

  const contextQuery = useQuery({
    queryKey: ['launch-context-detail', normalized, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(normalized),
    enabled: Boolean(me) && Boolean(catalogEntry) && !isInSuiteProduct(normalized),
  })

  useEffect(() => {
    if (contextQuery.isError) {
      redirectToSuiteLoginIfSessionExpired(contextQuery.error, normalized)
    }
  }, [contextQuery.error, contextQuery.isError, normalized])

  return (
    <div className="max-w-2xl space-y-4">
      <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">{productDisplayName}</h3>

      {!catalogEntry ? (
        <ApiErrorCallout message="This product is unavailable in the current workspace." />
      ) : isInSuiteProduct(normalized) ? (
        <p className="text-sm text-[var(--color-text-secondary)]">
          {productDisplayName} platform surfaces live here. Identity and launch APIs are wired.
        </p>
      ) : (
        <div className="space-y-3 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm">
          {contextQuery.data && (
            <>
              {contextQuery.data.canLaunch ? (
                <>
                  <p className="text-[var(--color-text-secondary)]">
                    <span className="font-medium text-[var(--color-text-primary)]">Launch URL:</span>{' '}
                    {contextQuery.data.launchUrl || '—'}
                  </p>
                  <p className="text-[var(--color-text-secondary)]">
                    <span className="font-medium text-[var(--color-text-primary)]">Status:</span>{' '}
                    Ready to launch
                  </p>
                </>
              ) : (
                <LaunchFailurePanel
                  productDisplayName={contextQuery.data.productDisplayName}
                  productKey={normalized}
                  context={contextQuery.data}
                  showAdminLink={false}
                />
              )}
            </>
          )}
          {contextQuery.isError && (
            <ApiErrorCallout
              message={getErrorMessage(
                contextQuery.error,
                'Failed to load product launch details.',
              )}
              onRetry={() => void contextQuery.refetch()}
              retryLabel="Retry launch details"
            />
          )}

          {launchAllowedQuery.data === true ? (
            <button
              type="button"
              disabled={launch.isPending}
              onClick={() => launch.mutate(normalized)}
              className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-60"
            >
              {launch.isPending ? 'Launching…' : 'Launch product'}
            </button>
          ) : null}
        </div>
      )}
    </div>
  )
}
