import { describe, expect, it } from 'vitest'

import { companionPlainReason, parseApiErrorBody } from './companionPlainReason'
import { CompanionFieldValidationReasonCodes } from './companionValidationReasonCodes'

describe('companionPlainReason', () => {
  it('parses API error JSON from companion client failures', () => {
    const body = JSON.stringify({
      code: 'companion.field_task.not_in_inbox',
      message: 'This task is not in your field inbox.',
    })
    const parsed = parseApiErrorBody(body)
    expect(parsed?.message).toContain('field inbox')

    const message = companionPlainReason({ body }, 'Fallback')
    expect(message).toContain('field inbox')
  })

  it('maps reason code from JSON when message is missing', () => {
    const body = JSON.stringify({
      code: CompanionFieldValidationReasonCodes.NotEntitled,
    })

    expect(companionPlainReason({ body }, 'Fallback.')).toContain('entitled')
  })

  it('maps bare reason codes on Error messages', () => {
    expect(companionPlainReason(new Error('not_entitled'), 'Fallback.')).toContain('entitled')
  })

  it('returns fallback when error has no parseable body', () => {
    expect(companionPlainReason(null, 'Fallback.')).toBe('Fallback.')
  })
})
