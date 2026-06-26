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
      code: FieldCompanionFieldValidationReasonCodes.AccessUnavailable,
    })

    expect(FieldCompanionPlainReason({ body }, 'Fallback.')).toContain('permission')
  })

  it('maps bare legacy reason codes on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('not_available'), 'Fallback.')).toContain('unavailable')
  })

  it('maps current availability reason codes on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('not_available'), 'Fallback.')).toContain('unavailable')
  })

  it('maps canonical product-unavailable reason codes on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('product_unavailable'), 'Fallback.')).toContain('unavailable')
  })

  it('maps revoked availability aliases on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('availability_revoked'), 'Fallback.')).toContain('unavailable')
  })

  it('maps bare legacy availability codes on Error messages', () => {
    expect(FieldCompanionPlainReason(new Error('availability_inactive'), 'Fallback.')).toContain('unavailable')
  })

  it('returns fallback when error has no parseable body', () => {
    expect(FieldCompanionPlainReason(null, 'Fallback.')).toBe('Fallback.')
  })
})
