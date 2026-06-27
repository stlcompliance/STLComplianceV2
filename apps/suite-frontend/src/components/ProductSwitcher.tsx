import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { useProductLaunch } from '../hooks/useProductLaunch'
import { isInSuiteProduct } from '../lib/permissions'
import { getErrorMessage } from '@stl/shared-ui/ApiErrorCallout'
import {
  ProductSwitcher as SharedProductSwitcher,
} from '@stl/shared-ui/ProductSwitcher'
import { normalizeProductKey } from '@stl/shared-ui/productCatalog'

function resolveCurrentProductKey(pathname: string): string {
  if (pathname.startsWith('/app/platform-admin')) {
    return 'nexarr'
  }
  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (match) {
    return normalizeProductKey(match[1])
  }
  return 'nexarr'
}

export function ProductSwitcher() {
  const { me } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const launch = useProductLaunch()
  const currentProductKey = resolveCurrentProductKey(location.pathname)

  function handleSelect(productKey: string) {
    const normalized = normalizeProductKey(productKey)

    if (normalized === currentProductKey) {
      return
    }

    if (isInSuiteProduct(normalized)) {
      navigate('/app')
      return
    }

    void launch.mutate(normalized)
  }

  return (
    <SharedProductSwitcher
      currentProductKey={currentProductKey}
      suiteHomeUrl="/app"
      showComplianceCore={me?.isPlatformAdmin === true}
      onSelectProduct={handleSelect}
      isPending={launch.isPending}
      errorMessage={launch.isError ? getErrorMessage(launch.error, 'Failed to launch product.') : null}
    />
  )
}
