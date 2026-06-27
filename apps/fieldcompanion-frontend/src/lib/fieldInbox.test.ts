import { describe, expect, it } from 'vitest'
import {
  filterTasks,
  formatFieldInboxRelativeTime,
  formatWhen,
  groupFieldInboxTasks,
  inboxSourceLoadFailures,
  productLabel,
  summarizeFieldInboxUrgency,
  taskTypeLabel,
} from './fieldInbox'
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

  it('groups tasks by urgency and freshness', () => {
    const now = new Date('2026-05-27T10:00:00.000Z')
    const items: FieldInboxTaskItem[] = [
      {
        ...sampleTask,
        taskKey: 'blocked-task',
        title: 'Blocked work',
        blockedReason: 'Waiting on compliance review',
        dueAt: '2026-05-27T18:00:00.000Z',
        sortAt: '2026-05-27T08:00:00.000Z',
      },
      {
        ...sampleTask,
        taskKey: 'due-soon-task',
        productKey: 'routarr',
        taskType: 'trip',
        title: 'Due soon work',
        dueAt: '2026-05-27T13:30:00.000Z',
        sortAt: '2026-05-27T09:15:00.000Z',
      },
      {
        ...sampleTask,
        taskKey: 'stale-task',
        productKey: 'trainarr',
        taskType: 'training_assignment',
        title: 'Stale work',
        dueAt: '2026-05-31T10:00:00.000Z',
        sortAt: '2026-05-25T10:00:00.000Z',
      },
    ]

    const summary = summarizeFieldInboxUrgency(items, now)
    expect(summary).toEqual({
      dueSoonCount: 1,
      overdueCount: 0,
      staleCount: 1,
      urgentCount: 2,
    })

    const groups = groupFieldInboxTasks(items, now)
    expect(groups.map((group) => group.bucket)).toEqual(['blocked', 'due_soon', 'stale'])
    expect(groups[0].items[0].insight.dueLabel).toContain('Due in')
    expect(groups[1].items[0].insight.freshnessLabel).toContain('Updated')
    expect(groups[2].items[0].insight.freshnessLabel).toContain('ago')
    expect(formatFieldInboxRelativeTime('2026-05-27T09:58:00.000Z', now)).toBe('2m ago')
    expect(formatFieldInboxRelativeTime('2026-05-27T10:30:00.000Z', now)).toBe('in 30m')
  })
})
