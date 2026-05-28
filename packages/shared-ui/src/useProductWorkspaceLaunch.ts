import { useMutation } from '@tanstack/react-query'
import { normalizeProductKey } from './productCatalog'
import {
  buildProductWorkspaceCallbackUrl,
  createProductHandoff,
  getLaunchContext,
  isSameProductKey,
} from './productLaunchHandoff'

export function useProductWorkspaceLaunch(input: {
  apiBase: string
  accessToken: string
  currentProductKey: string
  suiteHomeUrl: string
  productLaunchUrls: Record<string, string>
}) {
  return useMutation({
    mutationFn: async (productKey: string) => {
      const normalized = normalizeProductKey(productKey)
      if (isSameProductKey(normalized, input.currentProductKey)) {
        return { mode: 'current' as const, productKey: normalized }
      }

      const context = await getLaunchContext(input.apiBase, input.accessToken, normalized)
      if (!context.canLaunch) {
        throw new Error(context.denialReasonCode ?? 'launch.denied')
      }

      const callbackUrl = buildProductWorkspaceCallbackUrl(
        normalized,
        input.suiteHomeUrl,
        input.productLaunchUrls,
      )
      const handoff = await createProductHandoff(
        input.apiBase,
        input.accessToken,
        normalized,
        callbackUrl,
      )
      window.location.assign(handoff.launchUrl)
      return { mode: 'handoff' as const, productKey: normalized, launchUrl: handoff.launchUrl }
    },
  })
}
