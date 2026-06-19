import { describe, expect, it } from 'vitest'

import { shouldAutoAdvanceInspectionTemplateBasics } from './InspectionTemplateCreatePage'

describe('shouldAutoAdvanceInspectionTemplateBasics', () => {
  it('keeps the basics section in place for a 2-character title', () => {
    expect(
      shouldAutoAdvanceInspectionTemplateBasics({
        name: 'AB',
        inspectionType: 'annual_dot',
        effectiveTemplateKey: 'ab',
        fieldErrors: {},
      }),
    ).toBe(false)
  })

  it('allows auto-advance once the title is a bit more substantial', () => {
    expect(
      shouldAutoAdvanceInspectionTemplateBasics({
        name: 'ABC',
        inspectionType: 'annual_dot',
        effectiveTemplateKey: 'abc',
        fieldErrors: {},
      }),
    ).toBe(true)
  })

  it('never auto-advances when required basics still have errors', () => {
    expect(
      shouldAutoAdvanceInspectionTemplateBasics({
        name: 'ABC',
        inspectionType: 'annual_dot',
        effectiveTemplateKey: 'abc',
        fieldErrors: {
          name: 'Template name must be between 2 and 128 characters.',
        },
      }),
    ).toBe(false)
  })
})
