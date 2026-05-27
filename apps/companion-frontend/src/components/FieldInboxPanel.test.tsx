import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { FieldInboxPanel } from './FieldInboxPanel'
import type { AggregatedFieldInboxResponse, FieldInboxTaskItem } from '../api/types'

const workOrderTask: FieldInboxTaskItem = {
  taskKey: 'maintainarr:work-order:1',
  productKey: 'maintainarr',
  taskType: 'work_order',
  title: 'Replace belt',
  subtitle: 'PMP-100',
  status: 'open',
  priority: 'high',
  dueAt: '2026-05-27T12:00:00.000Z',
  sortAt: '2026-05-27T12:00:00.000Z',
  deepLinkPath: '/work-orders/1',
}

const tripTask: FieldInboxTaskItem = {
  taskKey: 'routarr:trip:1',
  productKey: 'routarr',
  taskType: 'trip',
  title: 'North route',
  subtitle: 'TR-100',
  status: 'assigned',
  priority: null,
  dueAt: null,
  sortAt: '2026-05-27T08:00:00.000Z',
  deepLinkPath: '/trips/1',
}

const inbox: AggregatedFieldInboxResponse = {
  summary: {
    totalCount: 2,
    blockedCount: 0,
    countByProduct: { maintainarr: 1, routarr: 1 },
  },
  items: [workOrderTask, tripTask],
  sources: [
    {
      productKey: 'maintainarr',
      entitled: true,
      fetched: true,
      errorCode: null,
      errorMessage: null,
      items: [workOrderTask],
    },
    {
      productKey: 'routarr',
      entitled: true,
      fetched: true,
      errorCode: null,
      errorMessage: null,
      items: [tripTask],
    },
  ],
}

describe('FieldInboxPanel', () => {
  it('renders tasks and filters by product', () => {
    const onFilter = vi.fn()

    render(
      <FieldInboxPanel inbox={inbox} productFilter="" onProductFilterChange={onFilter} />,
    )

    expect(screen.getByText('Replace belt')).toBeInTheDocument()
    expect(screen.getByText('North route')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /MaintainArr \(1\)/ }))
    expect(onFilter).toHaveBeenCalledWith('maintainarr')
  })
})
