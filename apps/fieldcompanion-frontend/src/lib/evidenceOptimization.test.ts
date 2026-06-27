import { afterEach, describe, expect, it, vi } from 'vitest'

import { prepareFieldCompanionEvidenceAttachment } from './evidenceOptimization'

function mockPhotoOptimizationEnvironment() {
  const drawImage = vi.fn()
  const clearRect = vi.fn()
  const close = vi.fn()

  vi.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue({
    drawImage,
    clearRect,
  } as never)
  vi.spyOn(HTMLCanvasElement.prototype, 'toBlob').mockImplementation(function (
    this: HTMLCanvasElement,
    callback: BlobCallback,
    type?: string,
    quality?: number,
  ) {
    const compressionQuality = typeof quality === 'number' ? quality : 1
    const pixelCount = Math.max(1, this.width * this.height)
    const size = Math.max(1, Math.round((pixelCount * compressionQuality) / 500))

    callback(new Blob([new Uint8Array(size)], { type: type ?? 'image/jpeg' }))
  })
  vi.spyOn(HTMLCanvasElement.prototype, 'toDataURL').mockImplementation(function (this: HTMLCanvasElement) {
    return `data:image/jpeg;base64,thumb-${this.width}`
  })
  vi.stubGlobal(
    'createImageBitmap',
    vi.fn(async () => ({
      width: 4000,
      height: 3000,
      close,
    })),
  )

  return { close, drawImage }
}

describe('evidenceOptimization', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('compresses photo attachments and produces a thumbnail preview', async () => {
    const { close, drawImage } = mockPhotoOptimizationEnvironment()

    const file = new File([new Uint8Array(8000)], 'field-photo.png', { type: 'image/png' })
    const snapshot = await prepareFieldCompanionEvidenceAttachment(file, 'photo')

    expect(snapshot.wasOptimized).toBe(true)
    expect(snapshot.uploadFile.name).toBe('field-photo.jpg')
    expect(snapshot.uploadFile.type).toBe('image/jpeg')
    expect(snapshot.uploadSizeBytes).toBeLessThan(snapshot.originalSizeBytes)
    expect(snapshot.previewDataUrl).toContain('thumb-320')
    expect(snapshot.summary).toContain('Photo optimized from')
    expect(snapshot.storageSummary).toContain('Thumbnail generated')
    expect(drawImage).toHaveBeenCalled()
    expect(close).toHaveBeenCalled()
  })

  it('tightens photo payloads in low-data mode', async () => {
    mockPhotoOptimizationEnvironment()

    const file = new File([new Uint8Array(8000)], 'field-photo.png', { type: 'image/png' })
    const standardSnapshot = await prepareFieldCompanionEvidenceAttachment(file, 'photo', {
      supported: true,
      saveData: false,
      effectiveType: '4g',
      downlinkMbps: 12,
    })
    const lowDataSnapshot = await prepareFieldCompanionEvidenceAttachment(file, 'photo', {
      supported: true,
      saveData: true,
      effectiveType: '2g',
      downlinkMbps: 0.4,
    })

    expect(lowDataSnapshot.wasOptimized).toBe(true)
    expect(lowDataSnapshot.summary).toContain('Low-data mode')
    expect(lowDataSnapshot.storageSummary).toContain('Low-data mode active.')
    expect(lowDataSnapshot.networkSummary).toContain('Low-data mode active.')
    expect(lowDataSnapshot.uploadSizeBytes).toBeLessThan(standardSnapshot.uploadSizeBytes)
  })

  it('preserves non-photo attachments without generating a preview', async () => {
    const file = new File(['pdf-bytes'], 'guide.pdf', { type: 'application/pdf' })

    const snapshot = await prepareFieldCompanionEvidenceAttachment(file, 'document')

    expect(snapshot.wasOptimized).toBe(false)
    expect(snapshot.preservesOriginal).toBe(true)
    expect(snapshot.uploadFile).toBe(file)
    expect(snapshot.previewDataUrl).toBeNull()
    expect(snapshot.summary).toContain('Document evidence will upload as the original file.')
  })
})
