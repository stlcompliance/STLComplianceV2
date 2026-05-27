import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { ReadinessRollupSupervisorPanel } from './ReadinessRollupSupervisorPanel'
import type { ReadinessRollupSummaryResponse } from '../api/types'

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

describe('ReadinessRollupSupervisorPanel', () => {
  it('renders team and site rollup tables', () => {
    render(
      <ReadinessRollupSupervisorPanel
        teamRollups={sampleRollups}
        siteRollups={[]}
        isLoading={false}
        errorMessage={null}
      />,
    )

    expect(screen.getByText('Team and site readiness rollups')).toBeTruthy()
    expect(screen.getByText('Field Team')).toBeTruthy()
    expect(screen.getByText('75.0%')).toBeTruthy()
    expect(screen.getByText(/No rollups computed yet/i)).toBeTruthy()
  })
})
