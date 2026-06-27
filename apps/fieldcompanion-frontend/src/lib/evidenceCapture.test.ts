import { describe, expect, it } from 'vitest'

import { canvasToFile, defaultContentType, defaultFileName, isRoutarrTripTask, isTrainarrFieldTask } from './evidenceCapture'

describe('evidenceCapture', () => {
  it('detects TrainArr assignment task keys', () => {
    expect(isTrainarrFieldTask('trainarr:assignment:11111111-1111-1111-1111-111111111111')).toBe(true)
    expect(isTrainarrFieldTask('maintainarr:work_order:abc')).toBe(false)
  })

  it('detects RoutArr trip task keys', () => {
    expect(isRoutarrTripTask('routarr:trip:11111111-1111-1111-1111-111111111111')).toBe(true)
    expect(isRoutarrTripTask('trainarr:assignment:abc')).toBe(false)
  })

  it('provides defaults per capture kind', () => {
    expect(defaultFileName('photo')).toContain('jpg')
    expect(defaultContentType('signature')).toBe('image/png')
  })

  it('converts a canvas to a signature file', async () => {
    const canvas = document.createElement('canvas')
    canvas.width = 24
    canvas.height = 24

    Object.defineProperty(canvas, 'toBlob', {
      configurable: true,
      value: (callback: BlobCallback) => {
        callback(new Blob(['signature-bytes'], { type: 'image/png' }))
      },
    })

    const file = await canvasToFile(canvas, 'field-signature.png')

    expect(file.name).toBe('field-signature.png')
    expect(file.type).toBe('image/png')
    expect(file.size).toBeGreaterThan(0)
  })
})
