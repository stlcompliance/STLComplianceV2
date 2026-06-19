import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { normalizeProductKey } from '@stl/shared-ui/productCatalog'
import * as nexarr from '../api/nexarrClient'
import { formatLaunchFailureError } from '../lib/launchFailure'
import { buildProductCallbackUrl, isInSuiteProduct } from '../lib/permissions'
import { NexarrApiError } from '../api/types'
import { redirectToSuiteLoginIfSessionExpired } from '../lib/sessionRedirect'

export function useProductLaunch() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (productKey: string) => {
      const normalized = normalizeProductKey(productKey)
      if (isInSuiteProduct(normalized)) {
        navigate(`/app/${normalized}`)
        return { mode: 'in-suite' as const, productKey: normalized }
      }

      try {
        const context = await nexarr.getLaunchContext(normalized)
        if (!context.canLaunch) {
          throw new Error(formatLaunchFailureError(context.denialReasonCode ?? 'launch.denied'))
        }

        const callbackUrl = buildProductCallbackUrl(normalized)
        const handoff = await nexarr.createHandoff(normalized, callbackUrl)
        window.location.assign(handoff.launchUrl)
        return { mode: 'handoff' as const, productKey: normalized, launchUrl: handoff.launchUrl }
      } catch (error) {
        if (redirectToSuiteLoginIfSessionExpired(error, normalized)) {
          return { mode: 'redirect' as const, productKey: normalized }
        }
        if (error instanceof NexarrApiError) {
          throw new Error(formatLaunchFailureError(error.code ?? error.message))
        }
        throw error
      }
    },
  })
}
