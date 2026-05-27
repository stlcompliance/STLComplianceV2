import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { buildProductCallbackUrl, isInSuiteProduct } from '../lib/permissions'

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
        throw new Error(context.denialReasonCode ?? 'launch.denied')
      }

      const callbackUrl = buildProductCallbackUrl(normalized)
      const handoff = await nexarr.createHandoff(normalized, callbackUrl)
      window.location.assign(handoff.launchUrl)
      return { mode: 'handoff' as const, productKey: normalized, launchUrl: handoff.launchUrl }
    },
  })
}
