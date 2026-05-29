import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { formatLaunchFailureError } from '../lib/launchFailure'
import { buildProductCallbackUrl, isInSuiteProduct } from '../lib/permissions'
import { NexarrApiError } from '../api/types'

export function useProductLaunch() {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (productKey: string) => {
      const normalized = productKey.trim().toLowerCase()
      if (isInSuiteProduct(normalized)) {
        navigate(`/app/${normalized}`)
        return { mode: 'in-suite' as const, productKey: normalized }
      }

      const context = await nexarr.getLaunchContext(normalized)
      if (!context.canLaunch) {
        throw new Error(formatLaunchFailureError(context.denialReasonCode ?? 'launch.denied'))
      }

      const callbackUrl = buildProductCallbackUrl(normalized)
      let handoff
      try {
        handoff = await nexarr.createHandoff(normalized, callbackUrl)
      } catch (error) {
        if (error instanceof NexarrApiError) {
          throw new Error(formatLaunchFailureError(error.code ?? error.message))
        }
        throw error
      }
      window.location.assign(handoff.launchUrl)
      return { mode: 'handoff' as const, productKey: normalized, launchUrl: handoff.launchUrl }
    },
  })
}
