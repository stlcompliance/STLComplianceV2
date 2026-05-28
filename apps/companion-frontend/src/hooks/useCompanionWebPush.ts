import { useEffect, useRef } from 'react'

import { getPushPermissionState, isWebPushSupported, syncCompanionPushSubscription } from '../lib/pushNotifications'

export function useCompanionWebPush(accessToken: string | undefined) {
  const syncedRef = useRef(false)

  useEffect(() => {
    if (!accessToken || !isWebPushSupported() || syncedRef.current) {
      return
    }

    if (getPushPermissionState() !== 'granted') {
      return
    }

    syncedRef.current = true
    void syncCompanionPushSubscription(accessToken)
  }, [accessToken])
}
