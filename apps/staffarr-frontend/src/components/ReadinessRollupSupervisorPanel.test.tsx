import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const mod = await importOriginal<typeof import('@stl/shared-ui')>()
  return {
    ...mod,
    StaticSearchPicker: ({
      value,
      onChange,
      options,
      testId,
      placeholder,
    }: {
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
      testId?: string
      placeholder?: string
    }) => (
      <label>
        {placeholder}
        <input
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        />
        <ul>
          {options.map((option) => (
            <li key={option.value}>{option.label}</li>
          ))}
        </ul>
      </label>
    ),
  }
})

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
    confidenceLevel: 'medium',
    confidenceScore: 65,
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

describe('ReadinessRollupSupervisorPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders team and site rollup tables', () => {
    const onSiteFilterChange = vi.fn()
    render(
      <ReadinessRollupSupervisorPanel
        teamRollups={sampleRollups}
        siteRollups={[]}
        siteFilterOrgUnitId={null}
        onSiteFilterChange={onSiteFilterChange}
        memberReadinessFilter="all"
        onMemberReadinessFilterChange={vi.fn()}
        selectedRollup={null}
        onSelectRollup={vi.fn()}
        rollupMembers={null}
        rollupMembersLoading={false}
        rollupMembersReadErrorMessage={null}
        isLoading={false}
        readErrorMessage={null}
      />,
    )

    expect(screen.getByText('Team and site readiness rollups')).toBeTruthy()
    expect(screen.getByText('Field Team')).toBeTruthy()
    expect(screen.getByText('75.0%')).toBeTruthy()
    expect(screen.getAllByText('Medium confidence').length).toBeGreaterThan(0)
    fireEvent.change(screen.getByTestId('readiness-rollup-site-filter'), {
      target: { value: '11111111-1111-1111-1111-111111111111' },
    })
    expect(onSiteFilterChange).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111')
  })

  it('opens member drill-down when a rollup row is selected', () => {
    const onSelectRollup = vi.fn()
    const onSelectPerson = vi.fn()
    const onMemberReadinessFilterChange = vi.fn()

    render(
      <ReadinessRollupSupervisorPanel
        teamRollups={sampleRollups}
        siteRollups={[]}
        siteFilterOrgUnitId={null}
        onSiteFilterChange={vi.fn()}
        memberReadinessFilter="all"
        onMemberReadinessFilterChange={onMemberReadinessFilterChange}
        selectedRollup={{
          scopeType: 'team',
          orgUnitId: sampleRollups[0].orgUnitId,
          orgUnitName: sampleRollups[0].orgUnitName,
        }}
        rollupMembers={sampleMembers}
        onSelectRollup={onSelectRollup}
        onSelectPerson={onSelectPerson}
        rollupMembersLoading={false}
        rollupMembersReadErrorMessage={null}
        isLoading={false}
        readErrorMessage={null}
      />,
    )

    expect(screen.getByTestId('readiness-rollup-drilldown')).toBeTruthy()
    expect(screen.getByText('Alex Notready')).toBeTruthy()
    expect(screen.getByText('Training acknowledgement pending')).toBeTruthy()
    expect(screen.getByRole('option', { name: 'Missing certifications only' })).toBeTruthy()
    expect(screen.getAllByText('Medium confidence').length).toBeGreaterThan(0)
    expect(screen.getByText('Score 65')).toBeTruthy()

    fireEvent.change(screen.getByTestId('readiness-rollup-member-filter'), {
      target: { value: 'missing_certifications' },
    })

    expect(onMemberReadinessFilterChange).toHaveBeenCalledWith('missing_certifications')

    fireEvent.click(screen.getByTestId(`readiness-rollup-member-select-${sampleMembers.members[0].personId}`))
    expect(onSelectPerson).toHaveBeenCalledWith(sampleMembers.members[0].personId)
  })

  it('renders top-level fetch errors in shared callout', () => {
    const onMemberReadinessFilterChange = vi.fn()
    const onRetryRead = vi.fn()

    render(
      <ReadinessRollupSupervisorPanel
        teamRollups={sampleRollups}
        siteRollups={[]}
        siteFilterOrgUnitId={null}
        onSiteFilterChange={vi.fn()}
        memberReadinessFilter="all"
        onMemberReadinessFilterChange={onMemberReadinessFilterChange}
        selectedRollup={null}
        onSelectRollup={vi.fn()}
        rollupMembers={null}
        rollupMembersLoading={false}
        rollupMembersReadErrorMessage={null}
        isLoading={false}
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
    const onMemberReadinessFilterChange = vi.fn()
    const onRetryRollupMembers = vi.fn()

    render(
      <ReadinessRollupSupervisorPanel
        teamRollups={sampleRollups}
        siteRollups={[]}
        siteFilterOrgUnitId={null}
        onSiteFilterChange={vi.fn()}
        memberReadinessFilter="all"
        onMemberReadinessFilterChange={onMemberReadinessFilterChange}
        selectedRollup={{
          scopeType: 'team',
          orgUnitId: sampleRollups[0].orgUnitId,
          orgUnitName: sampleRollups[0].orgUnitName,
        }}
        onSelectRollup={vi.fn()}
        rollupMembers={null}
        rollupMembersLoading={false}
        rollupMembersReadErrorMessage="Could not load members"
        onRetryRollupMembersRead={onRetryRollupMembers}
        isLoading={false}
        readErrorMessage={null}
      />,
    )

    expect(screen.getByRole('alert')).toBeTruthy()
    expect(screen.getByText('Member drill-down failed')).toBeTruthy()
    expect(screen.getByText('Could not load members')).toBeTruthy()
    fireEvent.click(screen.getByRole('button', { name: 'Retry members' }))
    expect(onRetryRollupMembers).toHaveBeenCalledTimes(1)
  })
})
