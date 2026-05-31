import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ReadinessRollupSupervisorPanel } from './ReadinessRollupSupervisorPanel'
import type { ReadinessRollupMembersResponse, ReadinessRollupSummaryResponse } from '../api/types'

const sampleRollups: ReadinessRollupSummaryResponse[] = [
  {
    orgUnitId: '11111111-1111-1111-1111-111111111111',
    scopeType: 'team',
    orgUnitName: 'Field Team',
    totalMembers: 4,
    readyCount: 3,
    notReadyCount: 1,
    overrideCount: 0,
    readyPercent: 75,
    computedAt: '2026-05-27T12:00:00.000Z',
  },
]

const sampleMembers: ReadinessRollupMembersResponse = {
  rollup: sampleRollups[0],
  members: [
    {
      personId: '22222222-2222-2222-2222-222222222222',
      displayName: 'Alex Notready',
      readinessStatus: 'not_ready',
      readinessBasis: 'training_blockers',
      hasActiveOverride: false,
      blockerCount: 1,
      primaryBlockerMessage: 'Training acknowledgement pending',
    },
  ],
}

const baseProps = {
  teamRollups: sampleRollups,
  siteRollups: [] as ReadinessRollupSummaryResponse[],
  siteFilterOrgUnitId: null,
  onSiteFilterChange: vi.fn(),
  memberReadinessFilter: 'all' as const,
  onMemberReadinessFilterChange: vi.fn(),
  selectedRollup: null,
  onSelectRollup: vi.fn(),
  rollupMembers: null,
  rollupMembersLoading: false,
  rollupMembersReadErrorMessage: null,
  isLoading: false,
  readErrorMessage: null,
}

describe('ReadinessRollupSupervisorPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders team and site rollup tables', () => {
    render(<ReadinessRollupSupervisorPanel {...baseProps} />)

    expect(screen.getByText('Team and site readiness rollups')).toBeTruthy()
    expect(screen.getByText('Field Team')).toBeTruthy()
    expect(screen.getByText('75.0%')).toBeTruthy()
    expect(screen.getByText(/No rollups computed yet/i)).toBeTruthy()
  })

  it('opens member drill-down when a rollup row is selected', () => {
    const onSelectRollup = vi.fn()
    const onSelectPerson = vi.fn()

    render(
      <ReadinessRollupSupervisorPanel
        {...baseProps}
        selectedRollup={{
          scopeType: 'team',
          orgUnitId: sampleRollups[0].orgUnitId,
          orgUnitName: sampleRollups[0].orgUnitName,
        }}
        rollupMembers={sampleMembers}
        onSelectRollup={onSelectRollup}
        onSelectPerson={onSelectPerson}
      />,
    )

    expect(screen.getByTestId('readiness-rollup-drilldown')).toBeTruthy()
    expect(screen.getByText('Alex Notready')).toBeTruthy()
    expect(screen.getByText('Training acknowledgement pending')).toBeTruthy()

    fireEvent.click(screen.getByTestId(`readiness-rollup-member-select-${sampleMembers.members[0].personId}`))
    expect(onSelectPerson).toHaveBeenCalledWith(sampleMembers.members[0].personId)
  })

  it('renders top-level fetch errors in shared callout', () => {
    const onRetryRead = vi.fn()
    render(
      <ReadinessRollupSupervisorPanel
        {...baseProps}
        readErrorMessage="Could not load rollups"
        onRetryRead={onRetryRead}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Readiness rollup load failed')).toBeTruthy()
    expect(screen.getByText('Could not load rollups')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry rollups' }))
    expect(onRetryRead).toHaveBeenCalledTimes(1)
  })

  it('renders drill-down errors in shared callout', () => {
    const onRetryRollupMembers = vi.fn()
    render(
      <ReadinessRollupSupervisorPanel
        {...baseProps}
        selectedRollup={{
          scopeType: 'team',
          orgUnitId: sampleRollups[0].orgUnitId,
          orgUnitName: sampleRollups[0].orgUnitName,
        }}
        rollupMembersReadErrorMessage="Could not load members"
        onRetryRollupMembersRead={onRetryRollupMembers}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Member drill-down failed')).toBeTruthy()
    expect(screen.getByText('Could not load members')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry members' }))
    expect(onRetryRollupMembers).toHaveBeenCalledTimes(1)
  })
})
