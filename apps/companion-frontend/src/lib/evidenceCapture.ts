export const EVIDENCE_CAPTURE_KINDS = ['photo', 'document', 'signature'] as const
export type EvidenceCaptureKind = (typeof EVIDENCE_CAPTURE_KINDS)[number]

export function isTrainarrFieldTask(taskKey: string): boolean {
  return taskKey.startsWith('trainarr:assignment:')
}

export function defaultFileName(kind: EvidenceCaptureKind): string {
  switch (kind) {
    case 'photo':
      return 'field-photo.jpg'
    case 'document':
      return 'field-document.pdf'
    case 'signature':
      return 'field-signature.png'
    default:
      return 'field-evidence.bin'
  }
}

export function defaultContentType(kind: EvidenceCaptureKind): string {
  switch (kind) {
    case 'photo':
      return 'image/jpeg'
    case 'document':
      return 'application/pdf'
    case 'signature':
      return 'image/png'
    default:
      return 'application/octet-stream'
  }
}

export async function fileToBase64(file: File): Promise<string> {
  const buffer = await file.arrayBuffer()
  const bytes = new Uint8Array(buffer)
  let binary = ''
  for (let index = 0; index < bytes.length; index += 1) {
    binary += String.fromCharCode(bytes[index]!)
  }
  return btoa(binary)
}
