import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { PersonTimelinePanel } from './PersonTimelinePanel'

const sampleEntry = {
  entryId: 'incident:1:reported',
  personId: 'person-1',
  category: 'incident' as const,
  eventType: 'incident_reported',
  title: 'Incident reported: Forklift near-miss',
  detail: 'safety · high · open',
  occurredAt: new Date().toISOString(),
  actorUserId: 'actor-1',
  sourceEntityType: 'personnel_incident',
  sourceEntityId: 'incident-1',
  externalReferenceId: null,
}

describe('PersonTimelinePanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders timeline entries with category and event labels', () => {
    render(
      <PersonTimelinePanel
        personDisplayName="Alex Rivera"
        totalCount={2}
        page={1}
        pageSize={25}
        hasNextPage={false}
        categoryFilter=""
        isLoading={false}
        onCategoryFilterChange={vi.fn()}
        onPageChange={vi.fn()}
        onPageSizeChange={vi.fn()}
        entries={[
          sampleEntry,
          {
            ...sampleEntry,
            entryId: 'readiness_override:2:granted',
            category: 'readiness',
            eventType: 'readiness_override_granted',
            title: 'Readiness override granted',
            detail: 'Temporary site access for audit support',
          },
        ]}
      />,
    )

    expect(screen.getByText('Person history timeline')).toBeTruthy()
    expect(screen.getByText(/Forklift near-miss/)).toBeTruthy()
    expect(screen.getByText('Temporary site access for audit support')).toBeTruthy()
    expect(screen.getByTestId('person-timeline-range').textContent).toContain('2 events')
  })

  it('shows empty state when no entries', () => {
    render(
      <PersonTimelinePanel
        personDisplayName="Alex Rivera"
        totalCount={0}
        page={1}
        pageSize={25}
        hasNextPage={false}
        categoryFilter=""
        isLoading={false}
        onCategoryFilterChange={vi.fn()}
        onPageChange={vi.fn()}
        onPageSizeChange={vi.fn()}
        entries={[]}
      />,
    )

    expect(screen.getByText('No timeline events recorded yet.')).toBeTruthy()
  })

  it('calls browse handlers for category, page size, and pagination', () => {
    const onCategoryFilterChange = vi.fn()
    const onPageChange = vi.fn()
    const onPageSizeChange = vi.fn()

    render(
      <PersonTimelinePanel
        personDisplayName="Alex Rivera"
        totalCount={30}
        page={2}
        pageSize={10}
        hasNextPage={true}
        categoryFilter=""
        isLoading={false}
        onCategoryFilterChange={onCategoryFilterChange}
        onPageChange={onPageChange}
        onPageSizeChange={onPageSizeChange}
        entries={[sampleEntry]}
      />,
    )

    fireEvent.change(screen.getByTestId('person-timeline-category-filter'), {
      target: { value: 'incident' },
    })
    expect(onCategoryFilterChange).toHaveBeenCalledWith('incident')

    fireEvent.change(screen.getByTestId('person-timeline-page-size'), {
      target: { value: '25' },
    })
    expect(onPageSizeChange).toHaveBeenCalledWith(25)

    fireEvent.click(screen.getByTestId('person-timeline-prev-page'))
    expect(onPageChange).toHaveBeenCalledWith(1)

    fireEvent.click(screen.getByTestId('person-timeline-next-page'))
    expect(onPageChange).toHaveBeenCalledWith(3)
  })
})
