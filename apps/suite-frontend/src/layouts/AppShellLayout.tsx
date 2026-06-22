import { useEffect } from 'react'
import { LayoutDashboard, LockKeyhole, LogOut, Shield, Upload } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'
import { AppTopBar } from '../components/AppTopBar'
import { PermissionGate } from '../components/PermissionGate'
import { hasProductEntitlement, isPlatformAdmin } from '../lib/permissions'
import { HintsPreferenceProvider, StlComplianceLogo } from '@stl/shared-ui'
import { useSuitePreferences } from '../preferences/preferences'

export function AppShellLayout() {
  const { me, logout } = useAuth()
  const suitePreferences = useSuitePreferences({
    tenantId: me?.tenantId,
    personId: me?.userId,
    initialTheme: me?.themePreference,
  })
  const theme = suitePreferences.preferences.theme
  const setTheme = (nextTheme: typeof theme) => {
    suitePreferences.setPreference('theme', nextTheme)
  }
  const toggleTheme = () => {
    const nextTheme = theme === 'dark' ? 'light' : 'dark'
    setTheme(nextTheme)
  }
  const setShowHints = (next: boolean) => {
    suitePreferences.setPreference('assistantShowAssumptions', next)
  }
  useEffect(() => {
    if (!suitePreferences.isDirty || suitePreferences.isLoading) {
      return
    }

    const timer = window.setTimeout(() => {
      void suitePreferences.save().catch((error) => {
        console.error('Failed to persist suite preferences', error)
      })
    }, 0)

    return () => window.clearTimeout(timer)
  }, [
    suitePreferences.isDirty,
    suitePreferences.isLoading,
    suitePreferences.preferences.assistantShowAssumptions,
    suitePreferences.preferences.theme,
    suitePreferences.save,
  ])
  const homeLabel = me?.isPlatformAdmin ? 'Suite dashboard' : 'Launchpad'
  const desktopNavLinkClassName = ({ isActive }: { isActive: boolean }) =>
    [
      'flex min-h-10 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
      isActive
        ? 'border-l-2 border-[var(--color-accent)] bg-[var(--color-accent-soft)] pl-[10px] text-[var(--color-text-primary)]'
        : 'border-l-2 border-transparent text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)]',
    ].join(' ')
  const mobileNavLinkClassName = ({ isActive }: { isActive: boolean }) =>
    [
      'flex min-h-10 shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
      isActive
        ? 'bg-[var(--color-accent-soft)] text-[var(--color-text-primary)] ring-1 ring-[var(--color-accent-border)]'
        : 'text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)]',
    ].join(' ')

  return (
    <HintsPreferenceProvider showHints={suitePreferences.preferences.assistantShowAssumptions} setShowHints={setShowHints}>
      <div className="flex min-h-screen bg-[var(--color-bg-app)] text-[var(--color-text-primary)]">
        <aside className="hidden min-h-0 w-64 shrink-0 flex-col overflow-y-auto border-r border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] p-4 lg:flex">
          <div className="mb-6 shrink-0">
            <StlComplianceLogo theme={theme} className="h-12 w-[13rem] object-contain object-left" />
          </div>

        <nav aria-label="Suite navigation" className="flex flex-col gap-1">
          <NavLink
            to="/app"
            end
            className={desktopNavLinkClassName}
          >
            <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
            {homeLabel}
          </NavLink>

          <PermissionGate allowed={hasProductEntitlement(me?.entitlements ?? [], 'nexarr')}>
            <NavLink
              to="/app/nexarr/identity"
              className={desktopNavLinkClassName}
            >
              <LockKeyhole className="h-4 w-4 shrink-0" aria-hidden />
              Identity & access
            </NavLink>
          </PermissionGate>

          <NavLink
            to="/app/imports"
            className={desktopNavLinkClassName}
          >
            <Upload className="h-4 w-4 shrink-0" aria-hidden />
            Smart Import
          </NavLink>
        </nav>

        <PermissionGate allowed={isPlatformAdmin(me)}>
          <NavLink
            to="/app/platform-admin"
            className={({ isActive }) =>
              [desktopNavLinkClassName({ isActive }), 'mt-6'].join(' ')
            }
          >
            <Shield className="h-4 w-4 shrink-0" aria-hidden />
            Platform admin
          </NavLink>
        </PermissionGate>

        <button
          type="button"
          onClick={() => void logout()}
          className="mt-auto flex min-h-10 items-center gap-2 rounded-lg px-3 py-2 text-sm text-[var(--color-text-secondary)] transition hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
        >
          <LogOut className="h-4 w-4 shrink-0" aria-hidden />
          Sign out
        </button>
      </aside>

        <div className="flex min-h-0 min-w-0 flex-1 flex-col">
          <AppTopBar theme={theme} onToggleTheme={toggleTheme} />
          <nav aria-label="Suite mobile navigation" className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] px-3 py-2 lg:hidden">
            <div className="flex items-center gap-2 overflow-x-auto pb-1 [&::-webkit-scrollbar]:hidden">
              <NavLink to="/app" end className={mobileNavLinkClassName}>
                <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
                <span>{homeLabel}</span>
              </NavLink>
              <PermissionGate allowed={hasProductEntitlement(me?.entitlements ?? [], 'nexarr')}>
                <NavLink to="/app/nexarr/identity" className={mobileNavLinkClassName}>
                  <LockKeyhole className="h-4 w-4 shrink-0" aria-hidden />
                  <span>Identity</span>
                </NavLink>
              </PermissionGate>
              <NavLink to="/app/imports" className={mobileNavLinkClassName}>
                <Upload className="h-4 w-4 shrink-0" aria-hidden />
                <span>Import</span>
              </NavLink>
              <PermissionGate allowed={isPlatformAdmin(me)}>
                <NavLink to="/app/platform-admin" className={mobileNavLinkClassName}>
                  <Shield className="h-4 w-4 shrink-0" aria-hidden />
                  <span>Admin</span>
                </NavLink>
              </PermissionGate>
              <button
                type="button"
                onClick={() => void logout()}
                className="ml-auto inline-flex min-h-10 shrink-0 items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] transition hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
              >
                <LogOut className="h-4 w-4 shrink-0 text-[var(--color-text-secondary)]" aria-hidden />
                <span>Sign out</span>
              </button>
            </div>
          </nav>
          <main className="min-h-0 flex-1 overflow-auto p-3 sm:p-4 lg:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </HintsPreferenceProvider>
  )
}
