import { describe, expect, it } from 'vitest'
import { parsePassFailTranscript } from './voiceGuidance'

describe('voiceGuidance', () => {
  it('maps spoken pass/fail answers', () => {
    expect(parsePassFailTranscript('pass')).toBe('pass')
    expect(parsePassFailTranscript('failed')).toBe('fail')
    expect(parsePassFailTranscript('not applicable')).toBe('na')
  })
})
