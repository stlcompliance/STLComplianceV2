import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import type { PrintableSurfaceRegistration } from './types'

type RegisteredPrintableSurface = {
  registrationId: number
  surface: PrintableSurfaceRegistration
}

type PrintRuntimeContextValue = {
  surface: PrintableSurfaceRegistration | null
  registerSurface: (surface: PrintableSurfaceRegistration) => number
  updateSurface: (registrationId: number, surface: PrintableSurfaceRegistration) => void
  clearSurface: (registrationId: number) => void
}

const PrintRuntimeContext = createContext<PrintRuntimeContextValue | null>(null)

function sameSurface(
  left: PrintableSurfaceRegistration | null | undefined,
  right: PrintableSurfaceRegistration | null | undefined,
) {
  if (left === right) {
    return true
  }

  if (!left || !right) {
    return false
  }

  const leftKeys = Object.keys(left) as Array<keyof PrintableSurfaceRegistration>
  const rightKeys = Object.keys(right) as Array<keyof PrintableSurfaceRegistration>
  if (leftKeys.length !== rightKeys.length) {
    return false
  }

  return leftKeys.every((key) => Object.is(left[key], right[key]))
}

function sameRegisteredSurface(
  left: RegisteredPrintableSurface | null | undefined,
  right: RegisteredPrintableSurface | null | undefined,
) {
  if (left === right) {
    return true
  }

  if (!left || !right) {
    return false
  }

  return left.registrationId === right.registrationId && sameSurface(left.surface, right.surface)
}

function getLatestRegisteredSurface(
  registrations: Map<number, PrintableSurfaceRegistration>,
): RegisteredPrintableSurface | null {
  let latest: RegisteredPrintableSurface | null = null

  for (const [registrationId, surface] of registrations.entries()) {
    if (!latest || registrationId > latest.registrationId) {
      latest = { registrationId, surface }
    }
  }

  return latest
}

export function PrintRuntimeProvider({ children }: { children: ReactNode }) {
  const nextRegistrationId = useRef(1)
  const registrationsRef = useRef(new Map<number, PrintableSurfaceRegistration>())
  const currentSurfaceRef = useRef<RegisteredPrintableSurface | null>(null)
  const [currentSurface, setCurrentSurface] = useState<RegisteredPrintableSurface | null>(null)

  const commitCurrentSurface = useCallback((nextSurface: RegisteredPrintableSurface | null) => {
    if (sameRegisteredSurface(currentSurfaceRef.current, nextSurface)) {
      return
    }

    currentSurfaceRef.current = nextSurface
    setCurrentSurface(nextSurface)
  }, [])

  const recomputeCurrentSurface = useCallback(() => {
    commitCurrentSurface(getLatestRegisteredSurface(registrationsRef.current))
  }, [commitCurrentSurface])

  const registerSurface = useCallback((surface: PrintableSurfaceRegistration) => {
    const registrationId = nextRegistrationId.current++
    registrationsRef.current.set(registrationId, surface)
    recomputeCurrentSurface()
    return registrationId
  }, [recomputeCurrentSurface])

  const updateSurface = useCallback((registrationId: number, surface: PrintableSurfaceRegistration) => {
    if (!registrationsRef.current.has(registrationId)) {
      return
    }

    registrationsRef.current.set(registrationId, surface)
    recomputeCurrentSurface()
  }, [recomputeCurrentSurface])

  const clearSurface = useCallback((registrationId: number) => {
    if (!registrationsRef.current.delete(registrationId)) {
      return
    }

    recomputeCurrentSurface()
  }, [recomputeCurrentSurface])

  const value = useMemo<PrintRuntimeContextValue>(
    () => ({
      surface: currentSurface?.surface ?? null,
      registerSurface,
      updateSurface,
      clearSurface,
    }),
    [currentSurface, registerSurface, updateSurface, clearSurface],
  )

  return <PrintRuntimeContext.Provider value={value}>{children}</PrintRuntimeContext.Provider>
}

export function usePrintRuntime() {
  const context = useContext(PrintRuntimeContext)
  if (!context) {
    throw new Error('usePrintRuntime must be used within a PrintRuntimeProvider.')
  }

  return context
}

export function useRegisterPrintableSurface(
  surface: PrintableSurfaceRegistration | null | false | undefined,
) {
  const context = useContext(PrintRuntimeContext)
  const registerSurface = context?.registerSurface
  const updateSurface = context?.updateSurface
  const clearSurface = context?.clearSurface
  const registrationIdRef = useRef<number | null>(null)

  useEffect(() => {
    if (!registerSurface || !updateSurface || !clearSurface) {
      return
    }

    if (!surface) {
      if (registrationIdRef.current !== null) {
        clearSurface(registrationIdRef.current)
        registrationIdRef.current = null
      }
      return
    }

    if (registrationIdRef.current === null) {
      registrationIdRef.current = registerSurface(surface)
      return
    }

    updateSurface(registrationIdRef.current, surface)
  }, [clearSurface, registerSurface, surface, updateSurface])

  useEffect(
    () => () => {
      if (!clearSurface || registrationIdRef.current === null) {
        return
      }

      clearSurface(registrationIdRef.current)
      registrationIdRef.current = null
    },
    [clearSurface],
  )
}
