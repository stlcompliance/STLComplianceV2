import { describe, expect, it } from 'vitest'
import { normalizeScanPayload } from './scanPayload'

const assignmentId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'

describe('normalizeScanPayload', () => {
  it('passes through task keys', () => {
    const raw = `trainarr:assignment:${assignmentId}`
    expect(normalizeScanPayload(raw)).toBe(raw)
  })

  it('maps assignment deep link paths', () => {
    expect(normalizeScanPayload(`/assignments/${assignmentId}`)).toBe(
      `trainarr:assignment:${assignmentId}`,
    )
  })

  it('unwraps stl-field-task prefix', () => {
    expect(normalizeScanPayload(`stl-field-task:trainarr:assignment:${assignmentId}`)).toBe(
      `trainarr:assignment:${assignmentId}`,
    )
  })
})
