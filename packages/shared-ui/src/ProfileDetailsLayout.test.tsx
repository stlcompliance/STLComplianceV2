import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { ProfileDetailsLayout } from './ProfileDetailsLayout'

describe('ProfileDetailsLayout', () => {
  it('switches tabs in uncontrolled mode', () => {
    const onTabChange = vi.fn()

    render(
      <MemoryRouter>
        <ProfileDetailsLayout
          backLabel="Back"
          backTo="/"
          breadcrumbs={['Bread', 'Crumb']}
          icon={<span>icon</span>}
          title="Detail page"
          subtitle="Subtitle"
          badges={[]}
          actions={null}
          metrics={[]}
          tabs={['Overview', 'Loads', 'History']}
          onTabChange={onTabChange}
          snapshotTitle="Snapshot"
          snapshotSubtitle="Snapshot subtitle"
          snapshotFields={[]}
          decisionTitle="Decision"
          decisionBadge={{ label: 'Approved' }}
          decisionSummary="Summary"
          decisionDetail="Detail"
          allowedChecks={1}
          blockedChecks={0}
          railSections={[]}
          mainContent={<div>content</div>}
        />
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('tab', { name: 'Overview' }).some((tab) => tab.getAttribute('aria-selected') === 'true')).toBe(true)

    fireEvent.click(screen.getAllByRole('tab', { name: 'History' })[0]!)

    expect(onTabChange).toHaveBeenCalledWith('History')
    expect(screen.getAllByRole('tab', { name: 'History' }).some((tab) => tab.getAttribute('aria-selected') === 'true')).toBe(true)
  })

  it('honors controlled tab state', () => {
    render(
      <MemoryRouter>
        <ProfileDetailsLayout
          backLabel="Back"
          backTo="/"
          breadcrumbs={['Bread', 'Crumb']}
          icon={<span>icon</span>}
          title="Detail page"
          subtitle="Subtitle"
          badges={[]}
          actions={null}
          metrics={[]}
          tabs={['Overview', 'Loads', 'History']}
          activeTab="Loads"
          snapshotTitle="Snapshot"
          snapshotSubtitle="Snapshot subtitle"
          snapshotFields={[]}
          decisionTitle="Decision"
          decisionBadge={{ label: 'Approved' }}
          decisionSummary="Summary"
          decisionDetail="Detail"
          allowedChecks={1}
          blockedChecks={0}
          railSections={[]}
          mainContent={<div>content</div>}
        />
      </MemoryRouter>,
    )

    expect(screen.getAllByRole('tab', { name: 'Loads' }).some((tab) => tab.getAttribute('aria-selected') === 'true')).toBe(true)
  })
})
