import { createContext, useContext, type ReactNode } from 'react'

export type HintsPreferenceContextValue = {
  showHints: boolean
  setShowHints: (nextValue: boolean) => void
}

const defaultValue: HintsPreferenceContextValue = {
  showHints: true,
  setShowHints: () => undefined,
}

const HintsPreferenceContext = createContext<HintsPreferenceContextValue>(defaultValue)

export function HintsPreferenceProvider({
  showHints,
  setShowHints,
  children,
}: HintsPreferenceContextValue & { children: ReactNode }) {
  return (
    <HintsPreferenceContext.Provider value={{ showHints, setShowHints }}>
      {children}
    </HintsPreferenceContext.Provider>
  )
}

export function useHintsPreference() {
  return useContext(HintsPreferenceContext)
}
