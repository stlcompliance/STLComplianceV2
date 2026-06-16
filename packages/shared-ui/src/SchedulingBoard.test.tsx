import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SchedulingBoard, type SchedulingDisplayItem } from './SchedulingBoard'

const unscheduledWork: SchedulingDisplayItem = {
  itemId: 'wo-1',
  productKey: 'maintainarr',
  itemType: 'work_order',
  title: 'Replace trailer door sensor',
  status: 'planned',
  requestedWindow: {
    startAt: '2026-06-18T14:00:00Z',
    endAt: '2026-06-18T16:00:00Z',
  },
  promisedWindow: {
    startAt: '2026-06-20T14:00:00Z',
    endAt: '2026-06-20T16:00:00Z',
  },
  sourceReferences: [
    {
      productKey: 'maintainarr',
      resourceType: 'work_order',
      resourceId: 'wo-1',
      label: 'WO-1001',
    },
  ],
  allowedActions: ['schedule'],
}

const scheduledWork: SchedulingDisplayItem = {
  ...unscheduledWork,
  itemId: 'wo-2',
  title: 'Inspect reefer unit',
  status: 'scheduled',
  scheduledWindow: {
    startAt: '2026-06-19T14:00:00Z',
    endAt: '2026-06-19T16:00:00Z',
  },
  resourceAssignments: [
    {
      resourceId: 'person-1',
      label: 'Avery Tech',
      productKey: 'staffarr',
      status: 'active',
    },
  ],
  allowedActions: ['reschedule', 'unschedule', 'complete', 'cancel'],
}

describe('SchedulingBoard', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders unscheduled work with product-owned source context', () => {
    render(
      <SchedulingBoard
        unscheduledItems={[unscheduledWork]}
        scheduledItems={[]}
        resources={[]}
      />,
    )

    const backlog = screen.getByRole('heading', { name: 'Unscheduled work' }).closest('aside')!
    expect(within(backlog).getByText('Replace trailer door sensor')).toBeInTheDocument()
    expect(within(backlog).getByText(/MaintainArr \/ Work order/i)).toBeInTheDocument()
    expect(screen.getByText('WO-1001')).toBeInTheDocument()
  })

  it('delegates schedule actions to the owning product adapter', () => {
    const onSchedule = vi.fn()
    render(
      <SchedulingBoard
        unscheduledItems={[unscheduledWork]}
        scheduledItems={[]}
        resources={[]}
        onSchedule={onSchedule}
      />,
    )

    fireEvent.click(screen.getByRole('button', { name: 'Schedule' }))

    expect(onSchedule).toHaveBeenCalledWith(expect.objectContaining({ itemId: 'wo-1', productKey: 'maintainarr' }))
  })

  it('keeps requested, promised, and scheduled windows distinct', () => {
    render(
      <SchedulingBoard
        unscheduledItems={[]}
        scheduledItems={[scheduledWork]}
        resources={[{ resourceId: 'person-1', label: 'Avery Tech', productKey: 'staffarr', status: 'active' }]}
      />,
    )

    expect(screen.getByText('Requested window')).toBeInTheDocument()
    expect(screen.getByText('Promised window')).toBeInTheDocument()
    expect(screen.getByText('Scheduled window')).toBeInTheDocument()
    expect(screen.getAllByText(/Jun 18, 2026/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Jun 20, 2026/i).length).toBeGreaterThan(0)
    expect(screen.getAllByText(/Jun 19, 2026/i).length).toBeGreaterThan(0)
  })

  it('shows blocking validation conflicts in the details drawer', () => {
    render(
      <SchedulingBoard
        unscheduledItems={[
          {
            ...unscheduledWork,
            blockers: [
              {
                conflictType: 'inactive_resource',
                severity: 'blocking',
                message: 'Assigned technician is inactive in StaffArr.',
              },
            ],
          },
        ]}
        scheduledItems={[]}
        resources={[]}
      />,
    )

    expect(screen.getByText('Conflict review')).toBeInTheDocument()
    expect(screen.getByText(/Assigned technician is inactive in StaffArr/i)).toBeInTheDocument()
  })
})
