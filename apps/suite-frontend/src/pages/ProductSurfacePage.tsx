import { useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { PermissionGate } from '../components/PermissionGate'
import { IdentityAccessPanel } from '../components/nexarr/IdentityAccessPanel'
import { LaunchFailurePanel } from '../components/nexarr/LaunchFailurePanel'
import { NexArrOverviewPanel } from '../components/nexarr/NexArrOverviewPanel'
import { NexArrTenantsPanel } from '../components/nexarr/NexArrTenantsPanel'
import { TenantIntegrationsPanel } from '../components/nexarr/TenantIntegrationsPanel'
import { useLaunchContextGate } from '../hooks/useLaunchContextGate'
import { useProductLaunch } from '../hooks/useProductLaunch'
import {
  findNavigationProduct,
  getProductDisplayName,
  isLaunchSurface,
  normalizeProductKey,
  resolveActiveSurface,
} from '../navigation/suiteNavigation'
import { isInSuiteProduct } from '../lib/permissions'
import { redirectToSuiteLoginIfSessionExpired } from '../lib/sessionRedirect'

export function ProductSurfacePage() {
  const {
    productKey = '',
    surfaceKey = '',
    '*': nestedSurfacePath,
  } = useParams<{ productKey: string; surfaceKey?: string; '*': string }>()
  const { me } = useAuth()
  const normalized = normalizeProductKey(productKey)
  const launch = useProductLaunch()
  const launchAllowedQuery = useLaunchContextGate(normalized)

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: Boolean(me),
  })

  const product = findNavigationProduct(navigationQuery.data?.products ?? [], normalized)
  const surface = product ? resolveActiveSurface(product.surfaces, surfaceKey) : undefined

  const contextQuery = useQuery({
    queryKey: ['launch-context-detail', normalized, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(normalized),
    enabled: Boolean(me) && Boolean(surface && isLaunchSurface(surface)) && !isInSuiteProduct(normalized),
  })

  useEffect(() => {
    if (contextQuery.isError) {
      redirectToSuiteLoginIfSessionExpired(contextQuery.error, normalized)
    }
  }, [contextQuery.error, contextQuery.isError, normalized])

  if (!surface) {
    return (
      <p className="text-sm text-[var(--color-text-muted)]">
        Select a surface from the product navigation.
      </p>
    )
  }

  if (!surface.isEnabled) {
    return (
      <ApiErrorCallout
        message={surface.permissionHint ?? 'You do not have access to this surface.'}
      />
    )
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'overview') {
    return <NexArrOverviewPanel />
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'identity') {
    return <IdentityAccessPanel />
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'tenants') {
    return <NexArrTenantsPanel />
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'integrations') {
    const [providerKey, childRoute] = (nestedSurfacePath ?? '').split('/').filter(Boolean)
    return (
      <TenantIntegrationsPanel
        providerKey={providerKey}
        mode={childRoute === 'mappings' ? 'mappings' : providerKey ? 'detail' : 'list'}
      />
    )
  }

  if (isLaunchSurface(surface)) {
    const launchContext = contextQuery.data
    const launchDenied = launchContext?.canLaunch === false

    return (
      <div className="max-w-2xl space-y-4">
        <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">{surface.label}</h3>
        {surface.permissionHint && (
          <p className="text-sm text-[var(--color-text-muted)]">{surface.permissionHint}</p>
        )}

        {contextQuery.isLoading && !launchContext && (
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm text-[var(--color-text-muted)]">
            Checking launch details…
          </div>
        )}

        {launchDenied && launchContext && (
          <LaunchFailurePanel
            productDisplayName={launchContext.productDisplayName}
            productKey={normalized}
            context={launchContext}
          />
        )}

        {launchContext?.canLaunch && (
          <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 text-sm">
            <p className="text-[var(--color-text-secondary)]">
              <span className="font-medium text-[var(--color-text-primary)]">Launch URL:</span>{' '}
              {launchContext.launchUrl || '—'}
            </p>
            <p className="mt-2 text-[var(--color-text-secondary)]">
              <span className="font-medium text-[var(--color-text-primary)]">Status:</span> Ready to
              launch
            </p>
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              NexArr validated tenant context and destination readiness. Product-local permissions
              still apply after launch.
            </p>
          </div>
        )}

        {launch.isError && (
          <ApiErrorCallout message={getErrorMessage(launch.error, 'Failed to launch product.')} />
        )}

        <PermissionGate allowed={launchAllowedQuery.data === true && !launchDenied}>
          <button
            type="button"
            disabled={launch.isPending || launchDenied}
            onClick={() => launch.mutate(normalized)}
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-60"
          >
            {launch.isPending ? 'Launching…' : 'Launch product'}
          </button>
        </PermissionGate>
      </div>
    )
  }

  return (
    <div className="max-w-3xl space-y-3">
      <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">{surface.label}</h3>
      <p className="text-sm text-[var(--color-text-secondary)]">
        This in-suite surface is wired to NexArr navigation and launch routing. Product-specific
        workflows launch in the dedicated {getProductDisplayName(normalized, product?.displayName)} application when
        shipped.
      </p>
      {surface.permissionHint && (
        <p className="text-xs text-[var(--color-text-muted)]">{surface.permissionHint}</p>
      )}
    </div>
  )
}
