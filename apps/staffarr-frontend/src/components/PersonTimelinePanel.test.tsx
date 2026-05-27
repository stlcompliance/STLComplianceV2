import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'
import { PersonTimelinePanel } from './PersonTimelinePanel'

describe('PersonTimelinePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders timeline entries with category and event labels', () => {
    render(
      <PersonTimelinePanel
        personDisplayName="Alex Rivera"
        totalCount={2}
        isLoading={false}
        entries={[
          {
            entryId: 'incident:1:reported',
            personId: 'person-1',
            category: 'incident',
            eventType: 'incident_reported',
            title: 'Incident reported: Forklift near-miss',
            detail: 'safety · high · open',
            occurredAt: new Date().toISOString(),
            actorUserId: 'actor-1',
            sourceEntityType: 'personnel_incident',
            sourceEntityId: 'incident-1',
            externalReferenceId: null,
          },
          {
            entryId: 'readiness_override:2:granted',
            personId: 'person-1',
            category: 'readiness',
            eventType: 'readiness_override_granted',
            title: 'Readiness override granted',
            detail: 'Temporary site access for audit support',
            occurredAt: new Date().toISOString(),
            actorUserId: 'actor-2',
            sourceEntityType: 'person_readiness_override',
            sourceEntityId: 'override-1',
            externalReferenceId: null,
          },
        ]}
      />,
    )

    expect(screen.getByText('Person history timeline')).toBeTruthy()
    expect(screen.getByText(/Forklift near-miss/)).toBeTruthy()
    expect(screen.getByText('Incident')).toBeTruthy()
    expect(screen.getByText('Readiness')).toBeTruthy()
    expect(screen.getByText('Temporary site access for audit support')).toBeTruthy()
    expect(screen.getByText('2 events total')).toBeTruthy()
  })

  it('shows empty state when no entries', () => {
    render(
      <PersonTimelinePanel
        personDisplayName="Alex Rivera"
        totalCount={0}
        isLoading={false}
        entries={[]}
      />,
    )

    expect(screen.getByText('No timeline events recorded yet.')).toBeTruthy()
  })
})
