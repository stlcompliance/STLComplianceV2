import { describe, expect, it } from 'vitest'
import {
  nextWorkOrderStatusAction,
  parseLaborHoursInput,
  workOrderEditable,
  workOrderStatusActionLabel,
} from './fieldWorkOrder'

describe('fieldWorkOrder', () => {
  it('identifies editable work order statuses', () => {
    expect(workOrderEditable('open')).toBe(true)
    expect(workOrderEditable('in_progress')).toBe(true)
    expect(workOrderEditable('completed')).toBe(false)
  })

  it('derives next status action labels', () => {
    expect(nextWorkOrderStatusAction('open')).toBe('in_progress')
    expect(workOrderStatusActionLabel('open')).toBe('Start work')
    expect(nextWorkOrderStatusAction('in_progress')).toBe('completed')
    expect(workOrderStatusActionLabel('in_progress')).toBe('Complete work order')
  })

  it('parses labor hour inputs', () => {
    expect(parseLaborHoursInput('1.5')).toBe(1.5)
    expect(parseLaborHoursInput('')).toBeNull()
    expect(parseLaborHoursInput('0')).toBeNull()
  })
})
