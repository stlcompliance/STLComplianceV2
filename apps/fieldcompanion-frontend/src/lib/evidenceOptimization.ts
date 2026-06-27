import type { EvidenceCaptureKind } from './evidenceCapture'
import { defaultFileName, fileToBase64 } from './evidenceCapture'
import {
  isFieldCompanionLowDataConnection,
  readCurrentFieldCompanionNetworkProfile,
  type FieldCompanionNetworkProfile,
} from './deviceCapabilities'

export interface FieldCompanionEvidenceAttachmentSnapshot {
  originalFile: File
  uploadFile: File
  previewDataUrl: string | null
  originalSizeBytes: number
  uploadSizeBytes: number
  wasOptimized: boolean
  preservesOriginal: boolean
  summary: string
  storageSummary: string
  networkSummary: string
}

const PHOTO_MAX_EDGE = 1600
const PHOTO_LOW_DATA_MAX_EDGE = 1120
const THUMBNAIL_MAX_EDGE = 320
const JPEG_QUALITY = 0.82
const JPEG_LOW_DATA_QUALITY = 0.68
const THUMBNAIL_QUALITY = 0.72
const THUMBNAIL_LOW_DATA_QUALITY = 0.66

export async function prepareFieldCompanionEvidenceAttachment(
  file: File,
  captureKind: EvidenceCaptureKind,
  networkProfile: FieldCompanionNetworkProfile = readCurrentFieldCompanionNetworkProfile(),
): Promise<FieldCompanionEvidenceAttachmentSnapshot> {
  const lowDataMode = captureKind === 'photo' && isFieldCompanionLowDataConnection(networkProfile)

  if (captureKind !== 'photo' || !isOptimizablePhoto(file)) {
    return {
      originalFile: file,
      uploadFile: file,
      previewDataUrl: null,
      originalSizeBytes: file.size,
      uploadSizeBytes: file.size,
      wasOptimized: false,
      preservesOriginal: true,
      summary: `${describeCaptureKind(captureKind)} will upload as the original file.`,
      storageSummary: `Original file retained at ${formatFieldCompanionEvidenceBytes(file.size)}.`,
      networkSummary: 'No optimization needed for this attachment.',
    }
  }

  const bitmap = await loadAttachmentImageSource(file)
  try {
    const mainCanvas = createCanvasElement()
    const mainDimensions = scaleToFit(bitmap.width, bitmap.height, lowDataMode ? PHOTO_LOW_DATA_MAX_EDGE : PHOTO_MAX_EDGE)
    mainCanvas.width = mainDimensions.width
    mainCanvas.height = mainDimensions.height
    drawBitmap(mainCanvas, bitmap.source, mainDimensions.width, mainDimensions.height)
    const compressedBlob = await canvasToBlob(
      mainCanvas,
      'image/jpeg',
      lowDataMode ? JPEG_LOW_DATA_QUALITY : JPEG_QUALITY,
    )
    const compressedFile = new File(
      [compressedBlob],
      replaceExtension(file.name || defaultFileName('photo'), 'jpg'),
      { type: 'image/jpeg' },
    )

    const thumbnailCanvas = createCanvasElement()
    const thumbnailDimensions = scaleToFit(bitmap.width, bitmap.height, THUMBNAIL_MAX_EDGE)
    thumbnailCanvas.width = thumbnailDimensions.width
    thumbnailCanvas.height = thumbnailDimensions.height
    drawBitmap(thumbnailCanvas, bitmap.source, thumbnailDimensions.width, thumbnailDimensions.height)
    const thumbnailDataUrl = thumbnailCanvas.toDataURL(
      'image/jpeg',
      lowDataMode ? THUMBNAIL_LOW_DATA_QUALITY : THUMBNAIL_QUALITY,
    )

    const uploadFile = compressedFile.size < file.size ? compressedFile : file
    const wasOptimized = uploadFile.size < file.size
    const originalSizeBytes = file.size
    const uploadSizeBytes = uploadFile.size
    const savings = originalSizeBytes - uploadSizeBytes

    return {
      originalFile: file,
      uploadFile,
      previewDataUrl: thumbnailDataUrl,
      originalSizeBytes,
      uploadSizeBytes,
      wasOptimized,
      preservesOriginal: uploadFile === file,
      summary: wasOptimized
        ? `${lowDataMode ? 'Low-data mode: ' : ''}Photo optimized from ${formatFieldCompanionEvidenceBytes(originalSizeBytes)} to ${formatFieldCompanionEvidenceBytes(uploadSizeBytes)}.`
        : `Photo kept as original because compression would not reduce ${formatFieldCompanionEvidenceBytes(originalSizeBytes)}.`,
      storageSummary: wasOptimized
        ? `${lowDataMode ? 'Low-data mode active. ' : ''}Thumbnail generated for review. Original file remains available in the browser until submission.`
        : 'Original photo preserved because the optimized version was not smaller.',
      networkSummary: wasOptimized
        ? `${lowDataMode ? 'Low-data mode active. ' : ''}Saved ${formatFieldCompanionEvidenceBytes(savings)} on upload.`
        : 'No network savings were possible for this file.',
    }
  } finally {
    bitmap.dispose?.()
  }
}

export function formatFieldCompanionEvidenceBytes(bytes: number): string {
  if (!Number.isFinite(bytes) || bytes < 0) {
    return '0 B'
  }

  if (bytes < 1024) {
    return `${bytes} B`
  }

  const units = ['KB', 'MB', 'GB'] as const
  let value = bytes / 1024
  let unitIndex = 0

  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }

  const formatted = value >= 100 ? value.toFixed(0) : value >= 10 ? value.toFixed(1) : value.toFixed(2)
  return `${formatted} ${units[unitIndex]}`
}

function describeCaptureKind(captureKind: EvidenceCaptureKind): string {
  switch (captureKind) {
    case 'photo':
      return 'Photo evidence'
    case 'document':
      return 'Document evidence'
    case 'signature':
      return 'Signature evidence'
    default:
      return 'Evidence'
  }
}

function isOptimizablePhoto(file: File): boolean {
  return file.type.startsWith('image/') && file.type !== 'image/gif'
}

function replaceExtension(fileName: string, extension: string): string {
  const dotIndex = fileName.lastIndexOf('.')
  const baseName = dotIndex > 0 ? fileName.slice(0, dotIndex) : fileName
  return `${baseName || 'field-photo'}.${extension.replace(/^\./, '')}`
}

function createCanvasElement(): HTMLCanvasElement {
  const canvas = document.createElement('canvas')
  return canvas
}

function drawBitmap(
  canvas: HTMLCanvasElement,
  bitmap: CanvasImageSource,
  width: number,
  height: number,
): void {
  const context = canvas.getContext('2d')
  if (!context) {
    throw new Error('Unable to create a canvas context for attachment optimization.')
  }

  context.clearRect(0, 0, width, height)
  context.drawImage(bitmap, 0, 0, width, height)
}

async function canvasToBlob(
  canvas: HTMLCanvasElement,
  type: string,
  quality: number,
): Promise<Blob> {
  const blob = await new Promise<Blob | null>((resolve) => {
    canvas.toBlob((capturedBlob) => resolve(capturedBlob), type, quality)
  })

  if (!blob) {
    throw new Error('Unable to optimize the selected attachment.')
  }

  return blob
}

function scaleToFit(width: number, height: number, maxEdge: number): { width: number; height: number } {
  if (width <= maxEdge && height <= maxEdge) {
    return { width, height }
  }

  const scale = Math.min(maxEdge / width, maxEdge / height)
  return {
    width: Math.max(1, Math.round(width * scale)),
    height: Math.max(1, Math.round(height * scale)),
  }
}

interface LoadedAttachmentImage {
  source: CanvasImageSource
  width: number
  height: number
  dispose?: () => void
}

async function loadAttachmentImageSource(file: File): Promise<LoadedAttachmentImage> {
  if (typeof createImageBitmap === 'function') {
    const bitmap = await createImageBitmap(file)
    return {
      source: bitmap,
      width: bitmap.width,
      height: bitmap.height,
      dispose: () => bitmap.close?.(),
    }
  }

  const dataUrl = `data:${file.type || 'application/octet-stream'};base64,${await fileToBase64(file)}`
  const image = await loadHtmlImage(dataUrl)
  return {
    source: image,
    width: image.naturalWidth || image.width,
    height: image.naturalHeight || image.height,
  }
}

async function loadHtmlImage(src: string): Promise<HTMLImageElement> {
  return new Promise<HTMLImageElement>((resolve, reject) => {
    const image = new Image()
    image.onload = () => resolve(image)
    image.onerror = () => reject(new Error('Unable to load the attachment for optimization.'))
    image.src = src
  })
}
