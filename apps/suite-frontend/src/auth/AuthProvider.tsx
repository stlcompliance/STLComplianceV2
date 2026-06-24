import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { MeResponse } from '../api/types'
import * as nexarr from '../api/nexarrClient'
import { configureNexarrClient } from '../api/nexarrClient'
import {
  clearAuthSession,
  loadAuthSession,
  saveAuthSession,
  type StoredAuthSession,
} from './authStorage'

interface AuthContextValue {
  session: StoredAuthSession | null
  me: MeResponse | undefined
  isAuthenticated: boolean
  isBootstrapping: boolean
  login: (
    email: string,
    password: string,
    tenantId: string | null,
    rememberDevice?: boolean,
    mfaCode?: string | null,
    recoveryCode?: string | null,
  ) => Promise<void>
  logout: () => Promise<void>
  refreshMe: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [session, setSession] = useState<StoredAuthSession | null>(() => loadAuthSession())
  const [isBootstrapping, setIsBootstrapping] = useState(() => session !== null)

  useEffect(() => {
    configureNexarrClient({
      onSessionUpdated: (updated) => {
        setSession(updated)
        saveAuthSession(updated)
      },
    })
  }, [])

  const meQuery = useQuery({
    queryKey: ['me', session?.sessionId],
    queryFn: () => nexarr.getMe(),
    enabled: session !== null,
    retry: false,
  })

  useEffect(() => {
    if (!session) {
      setIsBootstrapping(false)
      return
    }
    if (!meQuery.isLoading) {
      setIsBootstrapping(false)
    }
    if (meQuery.isError) {
      clearAuthSession()
      setSession(null)
      setIsBootstrapping(false)
    }
  }, [session, meQuery.isLoading, meQuery.isError])

  const login = useCallback(
    async (
      email: string,
      password: string,
      tenantId: string | null,
      rememberDevice = false,
      mfaCode?: string | null,
      recoveryCode?: string | null,
    ) => {
      const next = await nexarr.login({
        email,
        password,
        tenantId,
        rememberDevice,
        mfaCode,
        recoveryCode,
      })
      setSession(next)
      await queryClient.invalidateQueries({ queryKey: ['me'] })
      await queryClient.invalidateQueries({ queryKey: ['navigation'] })
    },
    [queryClient],
  )

  const logout = useCallback(async () => {
    await nexarr.logout()
    setSession(null)
    queryClient.clear()
  }, [queryClient])

  const refreshMe = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: ['me'] })
  }, [queryClient])

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      me: meQuery.data,
      isAuthenticated: session !== null && meQuery.isSuccess,
      isBootstrapping,
      login,
      logout,
      refreshMe,
    }),
    [session, meQuery.data, meQuery.isSuccess, isBootstrapping, login, logout, refreshMe],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
