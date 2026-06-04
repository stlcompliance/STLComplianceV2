import { useMutation } from '@tanstack/react-query'
import { normalizeProductKey } from '@stl/shared-ui'

import { createHandoff, getLaunchContext } from '../api/client'
import { resolveDeniedReason } from '../lib/companionDeniedReasonCatalog'
import { buildCompanionProductCallbackUrl } from '../lib/productLaunch'

export function useCompanionProductLaunch(input: {
  accessToken: string
  suiteHomeUrl: string
  productLaunchUrls: Record<string, string>
}) {
  return useMutation({
    mutationFn: async (productKey: string) => {
      const normalized = normalizeProductKey(productKey)
      if (normalized === 'fieldcompanion') {
        return { mode: 'current' as const, productKey: normalized }
      }

      const context = await getLaunchContext(input.accessToken, normalized)
      if (!context.canLaunch) {
        throw new Error(
          resolveDeniedReason(
            { reasonCode: context.denialReasonCode },
            'Product launch is not permitted.',
          ),
        )
      }

      const callbackUrl = buildCompanionProductCallbackUrl(
        normalized,
        input.suiteHomeUrl,
        input.productLaunchUrls,
      )
      const handoff = await createHandoff(input.accessToken, normalized, callbackUrl)
      window.location.assign(handoff.launchUrl)
      return { mode: 'handoff' as const, productKey: normalized, launchUrl: handoff.launchUrl }
    },
  })
}
