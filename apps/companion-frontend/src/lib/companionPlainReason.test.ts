import { describe, expect, it } from 'vitest'

import { companionPlainReason, parseApiErrorBody } from './companionPlainReason'

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

  it('returns fallback when error has no parseable body', () => {
    expect(companionPlainReason(null, 'Fallback.')).toBe('Fallback.')
  })
})
