import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { NexArrOverviewPanel } from '../components/nexarr/NexArrOverviewPanel'
import { useDashboardData } from '../hooks/useDashboardData'
import { isPlatformAdmin } from '../lib/permissions'
import { LaunchPadPage } from './LaunchPadPage'

export function HomePage() {
  const { me, navigationProducts, isLoading, error } = useDashboardData()

  if (isLoading || !me) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading your workspace…</p>
  }

  if (error) {
    return (
      <ApiErrorCallout message={getErrorMessage(error, 'Failed to load dashboard.')} />
    )
  }

  if (isPlatformAdmin(me)) {
    return <NexArrOverviewPanel />
  }

  return (
    <LaunchPadPage me={me} navigationProducts={navigationProducts} />
  )
}
