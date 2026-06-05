import { describe, expect, it } from 'vitest'

import { FieldCompanionPlainReason, parseApiErrorBody } from './FieldCompanionPlainReason'
import { FieldCompanionFieldValidationReasonCodes } from './FieldCompanionValidationReasonCodes'

describe('FieldCompanionPlainReason', () => {
  it('parses API error JSON from fieldcompanion client failures', () => {
    const body = JSON.stringify({
      code: 'fieldcompanion.field_task.not_in_inbox',
      message: 'This task is not in your field inbox.',
    })
    const parsed = parseApiErrorBody(body)
    expect(parsed?.message).toContain('field inbox')

    const message = FieldCompanionPlainReason({ body }, 'Fallback')
    expect(message).toContain('field inbox')
  })

  it('maps reason code from JSON when message is missing', () => {
    const body = JSON.stringify({
      code: FieldCompanionFieldValidationReasonCodes.NotEntitled,
    })

    expect(FieldCompanionPlainReason({ body }, 'Fallback.')).toContain('entitled')
  })

  it('maps bare reason codes on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('not_entitled'), 'Fallback.')).toContain('entitled')
  })

  it('returns fallback when error has no parseable body', () => {
    expect(FieldCompanionPlainReason(null, 'Fallback.')).toBe('Fallback.')
  })
})
