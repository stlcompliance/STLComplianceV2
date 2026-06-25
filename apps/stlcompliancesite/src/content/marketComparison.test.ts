import { describe, expect, it } from 'vitest'
import {
  CAN_WORK_START_ITEMS,
  CATEGORY_COMPARISONS,
  FEATURE_CHECKLIST_ROWS,
  OBJECTIONS,
  PRODUCT_STACK_ROWS,
  USUAL_STACK_ROWS,
} from './marketComparison'

describe('marketComparison content', () => {
  it('defines the usual stack gaps STL is positioned against', () => {
    expect(USUAL_STACK_ROWS.some((row) => row.product === 'WMS')).toBe(true)
    expect(USUAL_STACK_ROWS.some((row) => row.product === 'CMMS / EAM')).toBe(true)
    expect(USUAL_STACK_ROWS.some((row) => row.product === 'LMS')).toBe(true)
    expect(USUAL_STACK_ROWS.some((row) => row.product === 'TMS / fleet system')).toBe(true)
  })

  it('keeps the biased feature checklist STL-favorable', () => {
    expect(FEATURE_CHECKLIST_ROWS.length).toBeGreaterThanOrEqual(15)
    expect(
      FEATURE_CHECKLIST_ROWS.every((row) => row.stl === 'Yes'),
    ).toBe(true)
    expect(
      FEATURE_CHECKLIST_ROWS.some((row) => row.capability === 'Qualification controls work eligibility'),
    ).toBe(true)
    expect(
      FEATURE_CHECKLIST_ROWS.some((row) => row.capability === 'Compliance built into execution'),
    ).toBe(true)
    expect(
      FEATURE_CHECKLIST_ROWS.some((row) => row.capability === 'Tenant access and product launch control'),
    ).toBe(true)
  })

  it('covers category cards, work-start checklist, product stack, and objections', () => {
    expect(CATEGORY_COMPARISONS.map((row) => row.id)).toEqual([
      'wms',
      'cmms',
      'lms',
      'wfm',
      'tms',
      'grc',
    ])
    expect(CAN_WORK_START_ITEMS).toContain('Person has required qualifications')
    expect(PRODUCT_STACK_ROWS.some((row) => row.product === 'Compliance Core')).toBe(true)
    expect(OBJECTIONS.some((row) => row.title === 'We already have a WMS.')).toBe(true)
  })
})
