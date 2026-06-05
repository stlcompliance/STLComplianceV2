import { useEffect, useRef } from 'react'

import { getPushPermissionState, isWebPushSupported, syncFieldCompanionPushSubscription } from '../lib/pushNotifications'

export function useFieldCompanionWebPush(accessToken: string | undefined) {
  const syncedRef = useRef(false)

  useEffect(() => {
    if (!accessToken || !isWebPushSupported() || syncedRef.current) {
      return
    }

    if (getPushPermissionState() !== 'granted') {
      return
    }

    syncedRef.current = true
    void syncFieldCompanionPushSubscription(accessToken)
  }, [accessToken])
}
