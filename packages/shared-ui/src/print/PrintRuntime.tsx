import { createContext, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
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

export function PrintRuntimeProvider({ children }: { children: ReactNode }) {
  const nextRegistrationId = useRef(1)
  const [currentSurface, setCurrentSurface] = useState<RegisteredPrintableSurface | null>(null)

  const value = useMemo<PrintRuntimeContextValue>(
    () => ({
      surface: currentSurface?.surface ?? null,
      registerSurface: (surface) => {
        const registrationId = nextRegistrationId.current++
        setCurrentSurface({ registrationId, surface })
        return registrationId
      },
      updateSurface: (registrationId, surface) => {
        setCurrentSurface((existing) => {
          if (!existing || existing.registrationId !== registrationId) {
            return existing
          }

          if (sameSurface(existing.surface, surface)) {
            return existing
          }

          return { registrationId, surface }
        })
      },
      clearSurface: (registrationId) => {
        setCurrentSurface((existing) =>
          existing?.registrationId === registrationId ? null : existing,
        )
      },
    }),
    [currentSurface],
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
  const registrationIdRef = useRef<number | null>(null)

  useEffect(() => {
    if (!context) {
      return
    }

    if (!surface) {
      if (registrationIdRef.current !== null) {
        context.clearSurface(registrationIdRef.current)
        registrationIdRef.current = null
      }
      return
    }

    if (registrationIdRef.current === null) {
      registrationIdRef.current = context.registerSurface(surface)
      return
    }

    context.updateSurface(registrationIdRef.current, surface)
  }, [context, surface])

  useEffect(
    () => () => {
      if (!context || registrationIdRef.current === null) {
        return
      }

      context.clearSurface(registrationIdRef.current)
      registrationIdRef.current = null
    },
    [context],
  )
}
