import { useCallback, useEffect, useRef, useState } from 'react'

export type SharedDeviceProtectionPhase = 'inactive' | 'active' | 'warning' | 'locked'

const isTestMode = import.meta.env.MODE === 'test'

export const SHARED_DEVICE_WARNING_DELAY_MS = isTestMode ? 1000 : 14 * 60_000
export const SHARED_DEVICE_LOCK_DELAY_MS = isTestMode ? 2000 : 15 * 60_000

function isStandaloneDisplayMode(): boolean {
  if (typeof window === 'undefined') {
    return false
  }

  const navigatorWithStandalone = navigator as Navigator & { standalone?: boolean }
  if (navigatorWithStandalone.standalone === true) {
    return true
  }

  if (typeof window.matchMedia === 'function') {
    try {
      return window.matchMedia('(display-mode: standalone)').matches
    } catch {
      return false
    }
  }

  return false
}

export function isFieldCompanionSharedDeviceModeEnabled(): boolean {
  return import.meta.env.VITE_FIELD_COMPANION_SHARED_DEVICE_MODE === '1' || isStandaloneDisplayMode()
}

export function useSharedDeviceProtection(enabled: boolean) {
  const [phase, setPhase] = useState<SharedDeviceProtectionPhase>(enabled ? 'active' : 'inactive')
  const phaseRef = useRef<SharedDeviceProtectionPhase>(phase)
  const warningTimerRef = useRef<number | null>(null)
  const lockTimerRef = useRef<number | null>(null)

  useEffect(() => {
    phaseRef.current = phase
  }, [phase])

  const clearTimers = useCallback(() => {
    if (warningTimerRef.current != null) {
      window.clearTimeout(warningTimerRef.current)
      warningTimerRef.current = null
    }

    if (lockTimerRef.current != null) {
      window.clearTimeout(lockTimerRef.current)
      lockTimerRef.current = null
    }
  }, [])

  const armTimers = useCallback(() => {
    if (!enabled || typeof window === 'undefined') {
      return
    }

    clearTimers()
    setPhase('active')

    warningTimerRef.current = window.setTimeout(() => {
      setPhase('warning')
    }, SHARED_DEVICE_WARNING_DELAY_MS)

    lockTimerRef.current = window.setTimeout(() => {
      setPhase('locked')
    }, SHARED_DEVICE_LOCK_DELAY_MS)
  }, [clearTimers, enabled])

  const recordActivity = useCallback(() => {
    if (!enabled || phaseRef.current === 'locked' || typeof window === 'undefined') {
      return
    }

    armTimers()
  }, [armTimers, enabled])

  const lockNow = useCallback(() => {
    if (!enabled) {
      return
    }

    clearTimers()
    setPhase('locked')
  }, [clearTimers, enabled])

  useEffect(() => {
    if (!enabled || typeof window === 'undefined') {
      clearTimers()
      setPhase(enabled ? 'active' : 'inactive')
      return
    }

    armTimers()

    const handleActivity = () => {
      recordActivity()
    }

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        recordActivity()
      }
    }

    window.addEventListener('pointerdown', handleActivity, { passive: true })
    window.addEventListener('keydown', handleActivity)
    window.addEventListener('touchstart', handleActivity, { passive: true })
    window.addEventListener('mousemove', handleActivity, { passive: true })
    window.addEventListener('scroll', handleActivity, { passive: true })
    document.addEventListener('visibilitychange', handleVisibilityChange)

    return () => {
      clearTimers()
      window.removeEventListener('pointerdown', handleActivity)
      window.removeEventListener('keydown', handleActivity)
      window.removeEventListener('touchstart', handleActivity)
      window.removeEventListener('mousemove', handleActivity)
      window.removeEventListener('scroll', handleActivity)
      document.removeEventListener('visibilitychange', handleVisibilityChange)
    }
  }, [armTimers, clearTimers, enabled, recordActivity])

  return {
    phase,
    isEnabled: enabled,
    recordActivity,
    lockNow,
  }
}
