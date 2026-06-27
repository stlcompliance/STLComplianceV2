import type { DeviceCapabilitySnapshot } from './deviceCapabilities'
import { formatDeviceCapabilityDiagnosticSummary } from './deviceCapabilities'
import type { FieldCompanionReleaseSafetySnapshot } from './releaseSafety'
import type { FieldCompanionSessionHealthSnapshot } from './sessionSafety'

export interface FieldCompanionOperationalFallbackInput {
  deviceSnapshot: DeviceCapabilitySnapshot
  isOnline: boolean
  releaseSafety?: FieldCompanionReleaseSafetySnapshot | null
  sessionHealth?: FieldCompanionSessionHealthSnapshot | null
  pendingOfflineActions?: number
  lastSyncError?: string | null
  launchError?: string | null
}

export interface FieldCompanionOperationalFallbackSnapshot {
  isVisible: boolean
  tone: 'info' | 'warning' | 'error'
  title: string
  summary: string
  issueLabels: string[]
  recommendedSteps: string[]
  diagnosticSummary: string
}

export function buildFieldCompanionOperationalFallbackSnapshot(
  input: FieldCompanionOperationalFallbackInput,
): FieldCompanionOperationalFallbackSnapshot {
  const issueLabels: string[] = []
  const recommendedSteps: string[] = []
  const summaryParts: string[] = []
  let tone: FieldCompanionOperationalFallbackSnapshot['tone'] = 'info'
  let title = 'Operational fallback'

  const pushIssue = (label: string) => {
    if (!issueLabels.includes(label)) {
      issueLabels.push(label)
    }
  }

  const pushStep = (step: string) => {
    if (!recommendedSteps.includes(step)) {
      recommendedSteps.push(step)
    }
  }

  const releaseSafety = input.releaseSafety ?? null
  const sessionHealth = input.sessionHealth ?? null
  const deviceWarnings = input.deviceSnapshot.capabilities.filter((item) => item.status !== 'ready')

  if (input.launchError) {
    tone = 'error'
    title = 'Launch needs attention'
    pushIssue('Launch failed')
    summaryParts.push(input.launchError)
    pushStep('Return to suite home and retry launch from a fresh session.')
  }

  if (releaseSafety?.isActionBlocked) {
    tone = 'error'
    title = releaseSafety.title
    pushIssue(releaseSafety.title)
    summaryParts.push(releaseSafety.message)
    pushStep('Update the app to the minimum supported version before retrying.')
  } else if (releaseSafety?.isPaused) {
    if (tone !== 'error') {
      tone = 'warning'
    }
    if (title === 'Operational fallback') {
      title = 'Release paused'
    }
    pushIssue('Release paused')
    summaryParts.push(releaseSafety.message)
    pushStep('Keep the shell open and retry after the release resumes.')
  }

  if (!input.isOnline) {
    if (tone === 'info') {
      tone = 'warning'
    }
    if (title === 'Operational fallback') {
      title = 'Offline fallback'
    }
    pushIssue('Offline')
    summaryParts.push('You are offline. Saved work stays local until the connection returns.')
    pushStep('Review the offline queue and keep queued work intact until sync completes.')
    pushStep('Reconnect before retrying any action that needs the server.')
  }

  if (sessionHealth?.isAccessExpired) {
    tone = 'error'
    if (title === 'Operational fallback' || title === 'Offline fallback') {
      title = 'Session refresh needed'
    }
    pushIssue('Session expired')
    summaryParts.push('Your session needs a refresh before privileged actions can continue.')
    pushStep('Refresh the session from Profile before continuing.')
  } else if (sessionHealth?.isAccessExpiringSoon) {
    if (tone === 'info') {
      tone = 'warning'
    }
    if (title === 'Operational fallback') {
      title = 'Session refresh recommended'
    }
    pushIssue('Session expiring soon')
    summaryParts.push(
      `Your session is nearing expiry. Refresh it within the ${sessionHealth.warningWindowLabel} window to avoid interruption.`,
    )
    pushStep('Refresh the session soon to avoid a forced sign-in.')
  }

  if (deviceWarnings.length > 0) {
    if (tone === 'info') {
      tone = 'warning'
    }
    if (title === 'Operational fallback') {
      title = 'Device fallback needed'
    }
    const firstWarning = deviceWarnings[0]
    pushIssue(firstWarning?.label ?? 'Device capability warning')
    summaryParts.push(
      `This device needs fallback paths for ${deviceWarnings
        .slice(0, 3)
        .map((warning) => warning.label.toLowerCase())
        .join(', ')}.`,
    )
    pushStep('Open Profile to review device readiness and fallback guidance.')
  }

  if ((input.pendingOfflineActions ?? 0) > 0 && !input.isOnline) {
    pushIssue(`${input.pendingOfflineActions} pending action${input.pendingOfflineActions === 1 ? '' : 's'}`)
    pushStep('Leave the queue intact until sync finishes or the work is intentionally discarded.')
  }

  if (input.lastSyncError) {
    if (tone === 'info') {
      tone = 'warning'
    }
    if (title === 'Operational fallback') {
      title = 'Sync issue'
    }
    pushIssue('Last sync issue')
    summaryParts.push(`Last sync issue: ${input.lastSyncError}`)
    pushStep('Review the last sync issue in the offline queue before retrying.')
  }

  if (summaryParts.length === 0) {
    summaryParts.push('No degraded state is currently active.')
  }

  const diagnosticSummary = [
    'Field Companion degraded operation summary',
    `Online: ${input.isOnline ? 'yes' : 'no'}`,
    `Release mode: ${releaseSafety?.releaseMode ?? 'not evaluated'}`,
    `Release status: ${releaseSafety?.title ?? 'not available'}`,
    `Session status: ${sessionHealth?.statusLabel ?? 'not available'}`,
    `Pending offline actions: ${input.pendingOfflineActions ?? 0}`,
    input.lastSyncError ? `Last sync issue: ${input.lastSyncError}` : null,
    issueLabels.length > 0 ? `Active issues: ${issueLabels.join(', ')}` : null,
    formatDeviceCapabilityDiagnosticSummary(input.deviceSnapshot),
  ]
    .filter((value): value is string => value != null && value.trim().length > 0)
    .join('\n')

  return {
    isVisible:
      tone !== 'info' ||
      issueLabels.length > 0 ||
      Boolean(input.launchError) ||
      Boolean(input.lastSyncError),
    tone,
    title,
    summary: summaryParts.join(' '),
    issueLabels,
    recommendedSteps,
    diagnosticSummary,
  }
}
