import { describe, expect, it } from 'vitest'
import { filterTasks, formatWhen, inboxSourceLoadFailures, productLabel, taskTypeLabel } from './fieldInbox'
import type { FieldInboxTaskItem } from '../api/types'

const sampleTask: FieldInboxTaskItem = {
  taskKey: 'maintainarr:work-order:1',
  productKey: 'maintainarr',
  taskType: 'work_order',
  title: 'Replace filter',
  subtitle: 'PMP-100',
  status: 'open',
  priority: 'high',
  dueAt: '2026-05-27T12:00:00.000Z',
  sortAt: '2026-05-27T12:00:00.000Z',
  deepLinkPath: '/work-orders/1',
}

describe('fieldInbox helpers', () => {
  it('maps product and task labels', () => {
    expect(productLabel('maintainarr')).toBe('MaintainArr')
    expect(productLabel('loadarr')).toBe('LoadArr')
    expect(taskTypeLabel('training_assignment')).toBe('Training')
  })

  it('filters tasks by product key', () => {
    const items = [
      sampleTask,
      { ...sampleTask, taskKey: 'routarr:trip:1', productKey: 'routarr', taskType: 'trip' },
    ]
    expect(filterTasks(items, 'routarr')).toHaveLength(1)
    expect(filterTasks(items, '')).toHaveLength(2)
  })

  it('formats due timestamps for display', () => {
    expect(formatWhen(null)).toBe('No due date')
    expect(formatWhen('2026-05-27T12:00:00.000Z')).toContain('May')
  })

  it('collects plain inbox source load failures', () => {
    expect(
      inboxSourceLoadFailures([
        {
          productKey: 'routarr',
          available: true,
          fetched: false,
          errorCode: 'upstream_unreachable',
          errorMessage: null,
          items: [],
        },
      ]),
    ).toEqual([
      {
        productKey: 'routarr',
        message: expect.stringContaining('connectivity'),
      },
    ])
  })
})
