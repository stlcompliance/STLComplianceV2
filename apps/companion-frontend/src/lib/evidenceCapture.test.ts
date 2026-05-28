import { describe, expect, it } from 'vitest'

import { defaultContentType, defaultFileName, isTrainarrFieldTask } from './evidenceCapture'

describe('evidenceCapture', () => {
  it('detects TrainArr assignment task keys', () => {
    expect(isTrainarrFieldTask('trainarr:assignment:11111111-1111-1111-1111-111111111111')).toBe(true)
    expect(isTrainarrFieldTask('maintainarr:work_order:abc')).toBe(false)
  })

  it('provides defaults per capture kind', () => {
    expect(defaultFileName('photo')).toContain('jpg')
    expect(defaultContentType('signature')).toBe('image/png')
  })
})
